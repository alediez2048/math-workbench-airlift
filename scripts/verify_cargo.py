"""Fail-closed verification through the installed official Unity Pipeline CLI."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import struct
import sys
import time
import zipfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / 'unity'

def command(name, *args):
    proc = subprocess.run(['unity', 'command', name, *args, '--project-path', str(PROJECT),
                           '--format', 'json'], capture_output=True, text=True, timeout=60)
    if proc.returncode:
        raise RuntimeError(f'Unity command failed: {name}. Inspect the editor; no automatic restart.')
    payload = json.loads(proc.stdout)
    if not payload.get('success') or not payload.get('data', {}).get('success', True):
        raise RuntimeError(f'Unity rejected {name}')
    result = payload['data']['result']
    if isinstance(result, str):
        try: result = json.loads(result)
        except json.JSONDecodeError: pass
    if isinstance(result, dict) and result.get('success') is False:
        raise RuntimeError(f'Unity operation failed: {name}; inspect editor diagnostics.')
    return result

def validate_tests(result):
    summary = result.get('summary', result.get('Summary', {}))
    summary = {k.lower(): v for k, v in summary.items()}
    total = summary.get('total', 0)
    if total <= 0 or summary.get('passed') != total or any(
            summary.get(k, 0) for k in ('failed', 'skipped', 'inconclusive')):
        raise RuntimeError('Missing, zero, failed or skipped required tests.')
    return total

def order_suites(baseline, selected):
    # PlayMode suites return zero results once any EditMode suite has run in the same editor session
    # (observed 2026-09-15/16), so every PlayMode suite, baseline included, runs first.
    suites = list(baseline) + list(selected)
    return sorted(suites, key=lambda suite: 0 if suite['mode'] == 'playmode' else 1)

def should_retry_playmode(mode, result, attempts):
    summary = {k.lower(): v for k, v in result.get('summary', result.get('Summary', {})).items()}
    return mode == 'playmode' and summary.get('total', 0) <= 0 and attempts == 0

def reload_script_domain():
    # Verified recovery for a PlayMode run that reports zero tests: request a script-domain reload,
    # then wait until the editor reports ready again.
    command('run_script', '--file', str(PROJECT/'AgentScripts/ReloadAfterStalledTests.cs'),
            '--entry', 'ReloadAfterStalledTests.Run', '--timeout_ms', '60000')
    time.sleep(10)
    deadline = time.monotonic() + 180
    while time.monotonic() < deadline:
        try:
            status = command('editor_status')
            if status.get('status') == 'ready' and not status.get('compiling') and not status.get('domainReloadInProgress'):
                return
        except (RuntimeError, ValueError, subprocess.SubprocessError):
            pass
        time.sleep(3)
    raise RuntimeError('Editor did not return to ready after the script-domain reload.')

def run_tests(mode, name):
    command('run_tests', mode, name, 'testName', 'false', 'true', '300')
    deadline = time.monotonic() + 360
    while time.monotonic() < deadline:
        result = command('test_status')
        if result.get('status') == 'completed':
            return result
        if result.get('status') in ('failed', 'error', 'cancelled'):
            raise RuntimeError('Unity test run failed or was cancelled.')
        time.sleep(2)
    raise RuntimeError('Unity test deadline exceeded; inspect editor before retrying.')

def metadata_with_string_boundaries(data):
    # Unity 6000.6 IL2CPP metadata v108: adjacent string literals have no NUL.
    # Use the installed GlobalMetadataFileInternals.h / GlobalMetadata.cpp layout.
    if len(data) < 32: raise RuntimeError('Truncated IL2CPP metadata.')
    magic, version, offset, size, count, start, length, _ = struct.unpack_from('<8I', data)
    if magic != 0xFAB11BAF or version != 108:
        raise RuntimeError('Unknown IL2CPP metadata layout; review scanner before accepting APK.')
    if size % 4 or size < 8 or offset + size > start or start + length > len(data):
        raise RuntimeError('Invalid IL2CPP string table bounds.')
    indices = [x[0] for x in struct.iter_unpack('<I', data[offset:offset+size])]
    if indices[0] != 0 or indices[-1] != length or any(a > b for a,b in zip(indices,indices[1:])):
        raise RuntimeError('Invalid IL2CPP string literal boundaries.')
    return data[:start] + b'\0'.join(data[start+a:start+b] for a,b in zip(indices,indices[1:])) + data[start+length:]

def scan_apk(path):
    # Bounded credential signatures, not a claim to detect every possible secret.
    patterns = [rb'sk-[A-Za-z0-9_-]{20,}',
                rb'(?i)(?:accessToken|witClientAccessToken)\s*[\":= ]+\s*[\"\']?[A-Za-z0-9_-]{16,}']
    with zipfile.ZipFile(path) as archive:
        if 'AndroidManifest.xml' not in archive.namelist():
            raise RuntimeError('APK has no Android manifest.')
        for item in archive.infolist():
            data = archive.read(item)
            if item.filename.endswith('/Metadata/global-metadata.dat'):
                data = metadata_with_string_boundaries(data)
            if any(re.search(pattern, data) for pattern in patterns):
                raise RuntimeError('Potential embedded credential detected; APK not accepted.')
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser = argparse.ArgumentParser()
    selection = parser.add_mutually_exclusive_group(required=True)
    selection.add_argument('--suite')
    selection.add_argument('--phase', choices=['1','2','3','4'])
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--approved-dirty-build', action='store_true',
                        help='Use only with explicit owner approval for this local build; leaves Git unchanged.')
    args = parser.parse_args()
    manifest = json.loads((ROOT/'scripts/cargo-milestones.json').read_text())
    tickets = manifest['tickets']
    if args.phase:
        end = manifest['phaseEnds'][args.phase]
        if end not in tickets:
            raise RuntimeError('Phase is not implemented; future requirements cannot be reported passed.')
        selected = [suite for key, ticket in sorted(tickets.items()) if key <= end for suite in ticket['suites']]
    else:
        requested = args.suite.split(',')
        known = [suite for ticket in tickets.values() for suite in ticket['suites']]
        selected = [suite for suite in known if suite['filter'] in requested or
                    suite['filter'].removesuffix('SceneTests') in requested]
        if any(not any(s['filter'] == name for s in selected) for name in requested):
            raise RuntimeError('Unknown suite; update the milestone manifest in its owning ticket.')
    for ticket in tickets.values():
        for asset in ticket['requiredAssets']:
            if not (PROJECT/asset).is_file(): raise RuntimeError('Required milestone asset missing: '+asset)
    # Existing editor only: do not close unsaved work or spawn a second locked editor.
    status = command('editor_status')
    if status.get('playMode') not in (None, 'stopped'):
        raise RuntimeError('Editor is in play mode; exit play mode before verification.')
    # A dirty open scene makes the test framework raise a native save alert that hangs the
    # editor main thread. Preserve a copy of any unsaved state, then reload from disk.
    prepared = command('run_script', '--file', str(PROJECT/'AgentScripts/PrepareCleanScenes.cs'),
                       '--entry', 'PrepareCleanScenes.Run', '--timeout_ms', '90000')
    if isinstance(prepared, dict) and not prepared.get('success', True):
        raise RuntimeError('Scene preparation failed: ' + str(prepared.get('errorDetails', ''))[:300])
    print('Scenes: ' + str(prepared.get('result') if isinstance(prepared, dict) else prepared), flush=True)
    evidence = ROOT/'artifacts/qa'/time.strftime('cargo-%Y%m%d-%H%M%S')
    evidence.mkdir(parents=True, exist_ok=False)
    completed = []
    for suite in order_suites(manifest['baseline'], selected):
        result = run_tests(suite['mode'], suite['filter'])
        if should_retry_playmode(suite['mode'], result, 0):
            print(f"RETRY {suite['filter']}: zero PlayMode results; reloading the script domain once", flush=True)
            reload_script_domain()
            result = run_tests(suite['mode'], suite['filter'])
        (evidence/(suite['filter']+'.json')).write_text(json.dumps(result, indent=2))
        report = ET.Element('testsuite', name=suite['filter'])
        for case in result.get('results', []):
            node = ET.SubElement(report, 'testcase', name=case.get('FullName', 'unknown'))
            if case.get('Status') != 'Passed':
                ET.SubElement(node, 'failure').text = case.get('Message') or case.get('Status')
        ET.ElementTree(report).write(evidence/(suite['filter']+'.xml'), encoding='utf-8', xml_declaration=True)
        completed.append({'suite':suite['filter'], 'passed':validate_tests(result)})
        print(f"PASS {suite['filter']}: {validate_tests(result)}", flush=True)
    (evidence/'summary.json').write_text(json.dumps(completed, indent=2))
    if args.build:
        dirty = subprocess.check_output(['git','status','--porcelain'], cwd=ROOT, text=True)
        if dirty and not args.approved_dirty_build:
            raise RuntimeError('Tests passed; build blocked by uncommitted work. Obtain an approved checkpoint or explicit documented development-build exception. No commit was made.')
        paths = subprocess.check_output(['git','ls-files','-z','--cached','--others','--exclude-standard'], cwd=ROOT).decode().split('\0')
        hashes = {p:hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
                  for p in sorted(set(paths)) if p and (ROOT/p).is_file()}
        snapshot = {'head':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),
                    'approvedDirtyBuild':args.approved_dirty_build, 'files':hashes}
        snapshot['sourceHash'] = hashlib.sha256(json.dumps(hashes,sort_keys=True).encode()).hexdigest()
        (evidence/'source-state.json').write_text(json.dumps(snapshot,indent=2))
        output = evidence/'airlift-cargo.apk'
        prior = command('build_status')
        command('build', 'Android', str(output), '',
                '["DetailedBuildReport"]', json.dumps([manifest['scene']]), 'true', 'false')
        deadline = time.monotonic() + 1800
        while time.monotonic() < deadline:
            status = command('build_status')
            if status.get('buildId') != prior.get('buildId') and status.get('status') == 'completed':
                if status.get('result') != 'Succeeded' or Path(status.get('outputPath','')) != output:
                    raise RuntimeError('Build failed or output does not match this verification run.')
                digest = scan_apk(output)
                summary = {k: status.get(k) for k in ('buildId','result','platform','outputPath','totalWarnings','totalErrors')}
                summary['sha256'] = digest
                (evidence/'build.json').write_text(json.dumps(summary, indent=2))
                print('APK built and bounded credential scan passed; device acceptance pending.', flush=True)
                break
            if status.get('buildId') != prior.get('buildId') and status.get('status') in ('failed','error','cancelled'):
                raise RuntimeError('Build failed or was cancelled.')
            time.sleep(5)
        else: raise RuntimeError('Build deadline exceeded; inspect editor before retrying.')
    print('Automated verification complete. Device acceptance remains separate.')

if __name__ == '__main__':
    try: main()
    except (RuntimeError, ValueError, OSError, subprocess.SubprocessError) as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
