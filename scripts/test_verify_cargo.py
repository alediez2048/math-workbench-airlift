import tempfile
import unittest
from pathlib import Path
import zipfile
import struct
from verify_cargo import validate_tests, scan_apk, metadata_with_string_boundaries

class VerificationTests(unittest.TestCase):
    def test_metadata_string_boundaries(self):
        literals = [b'sk-SK', b'X'*30, b'sk-'+b'Y'*30]
        offsets = [0,5,35,68]
        header = struct.pack('<8I', 0xFAB11BAF,108,32,16,4,48,68,68)
        data = header + struct.pack('<4I', *offsets) + b''.join(literals)
        normalized = metadata_with_string_boundaries(data)
        self.assertIn(b'sk-SK\0'+b'X'*30, normalized)
        self.assertIn(b'sk-'+b'Y'*30, normalized) # Real token remains detectable.
        with self.assertRaises(RuntimeError): metadata_with_string_boundaries(b'bad')

    def test_missing_tests_fail(self):
        for summary in ({}, {'total':0}, {'total':2,'passed':1,'failed':1},
                        {'total':2,'passed':1,'skipped':1}):
            with self.assertRaises(RuntimeError): validate_tests({'summary':summary})

    def test_all_pass(self):
        self.assertEqual(validate_tests({'summary':{'total':2,'passed':2}}), 2)

    def test_unsafe_apk_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory)/'test.apk'
            with zipfile.ZipFile(path,'w') as archive:
                archive.writestr('AndroidManifest.xml', b'test')
                archive.writestr('assets/test', b'accessToken: '+b'X'*32)
            with self.assertRaises(RuntimeError): scan_apk(path)

    def test_non_apk_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory)/'test.apk'
            with zipfile.ZipFile(path,'w') as archive: archive.writestr('test', 'test')
            with self.assertRaises(RuntimeError): scan_apk(path)

if __name__ == '__main__': unittest.main()
