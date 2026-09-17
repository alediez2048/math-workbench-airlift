# Nerdy lounge and onboarding — contracts (CC-FD-*)

Design: `docs/superpowers/specs/2026-09-17-nerdy-front-door-design.md`. Structural reference:
`docs/00-build/REFERENCE-THEATRE-ELSEWHERE.md`. Tickets: CC-FD-01…10 in TICKETS.md.
Implementation starts **after the Friday 2026-09-18 submission**. Nothing here is authorized before then.

**Shape, corrected by the owner on 2026-09-17:** one scene, one place. The app opens in the Nerdy lounge, the logo
animates, Dee appears beside the onboarding cards we already have, the learner picks AR or VR, gets a thorough
rundown of how the experience works, and lands on the discovery wall that hangs in that same lounge. The lounge is
the **home**: every session opens there and browsing happens there. It is hidden only while a lesson runs, and
lessons themselves are exactly as they are today.
**There is no second scene, no scene router, no persistent core and no loading screen between spaces** — an earlier
draft had all four and was rejected.

The onboarding and the rundown are required in full. If time contracts, the VR room goes first — AR ships and the wall
floats in passthrough as the cards do today — then dashboard filters, then the arrival animation. The room's
frame budget is a release gate, not a nicety: it is on screen every session.

## Rules for every implementer

- Edit only the files your brief owns. Read anything.
- **No `unity` CLI at all** (no recompile, run_script, run_tests, build). No hand edits to `.unity`/`.asset` YAML.
  Compile offline with Unity's bundled Roslyn against `unity/Library/ScriptAssemblies/*.dll`; run pure NUnit logic
  under Unity's bundled mono in your scratchpad.
- No git commit, push, stash, checkout or reset. Never revert other people's changes.
- C# 9. Inside `namespace Airlift.*`, `Math` is the `Airlift.Math` namespace: use `Mathf` or `System.Math`.
- Tests first: write the failing EditMode test, then the code. Say which builder must run before a scene test.
- Every new suite must be registered in `scripts/cargo-milestones.json` under its ticket or the wrapper skips it.
- Cargo Crew stays locked: `CargoLessonDirector`, `CargoLessonModel`, `CargoChapter`, `OnboardingDirector` and
  `BuildDockWorkbench` are not touched, and `CargoVoiceCharacterizationTests` stays green. The one agreed exception
  is `LessonStation.Open(int)`, which the stations implement without changing lesson maths.
- The existing onboarding cards keep their wording and their tests. They are reparented, not rewritten.
- **Dee has no body.** The guide, its HUD and the assistant bar we already ship do all of the new onboarding; the
  lounge gives them a place to speak from. No mascot, rig, lip sync or purchase — do not build one.
- Tile mockups for the six coming-soon worlds: `https://claude.ai/artifact/Sv1f3tnPP6BA84CzDmTbBA`.
- Copy: short sentences a 7–10 year old understands; no timers, stars, scores or praise words. Math is
  deterministic C#; Dee never judges correctness.
- Every colour, font, radius and sprite comes from `NerdyStyle.asset` and the Live Learning Style Guide recipes in
  the spec — no literals. `LoungeStyleTests` enforces it.

## Shared API — the director agent owns these; everyone else codes against them now

`unity/Assets/Airlift/Scripts/Welcome/WelcomeFlow.cs` gains states; it is not replaced:

```csharp
public enum WelcomePhase
{
    Arrival,     // logo animation and, only if real work is pending, a progress bar
    Scenery,     // "your room" or "Nerdy lounge", one press
    Consent,     // existing
    Welcome,     // existing chip questions
    Rundown,     // how the experience works, five stops
    Dashboard,   // the expanded board, replaces the three flat cards
    Lesson       // existing
}
```

`unity/Assets/Airlift/Scripts/Lounge/LoungeRoom.cs`, namespace `Airlift.Lounge`:

```csharp
public sealed class LoungeRoom : MonoBehaviour
{
    public enum Scenery { YourRoom, NerdyLounge }
    public Scenery Mode { get; }
    public void Apply(Scenery mode);      // room shell on/off, passthrough on/off, glows rescaled
    public void Show(bool on);            // the whole root, off only while a lesson runs
    public bool Ready { get; }            // arrival animation finished and startup work done
    public float StartupProgress { get; } // real work only: asset warm-up, guide connect, mic permission
}
```

```csharp
public sealed class SettingsStore          // one versioned JSON record, defaults survive a missing/older file
{
    public event Action<string> Changed;   // setting id
    public LoungeRoom.Scenery Scenery;
    public bool VoiceGuide, Captions, Haptics, ButtonLabels;
    public string Language;                // change reconnects the guide; refused inside a lesson
    public float VoiceVolume, MusicVolume, EffectsVolume;
    public void ResetToDefaults();
}

public sealed class LibraryStore           // local file on the headset; no identifiers; erasable from settings
{
    public void RecordOpened(string lessonId, int chapterIndex);
    public int OpenCount(string tileId);
    public IReadOnlyList<string> MostViewed { get; }     // open count, then recency — the toolbar's third filter
    public (string lessonId, int chapter)? Continue { get; }
    public void ClearAll();
}
```

## Station change (director agent writes it, lesson stations implement)

```csharp
public abstract void Open();                  // unchanged: briefing, as today
public abstract void Open(int chapterIndex);  // 0 = same as Open(); 1..4 skip the briefing
```

- Chapter 0 keeps today's behaviour: briefing → concept intro when it has not been seen this run → chapter. Cargo
  additionally runs its existing practice step.
- Chapters 1–4 start directly. No chapter is locked and nothing scores the order.
- `CargoVoiceCharacterizationTests` must still reproduce the golden script byte for byte through `Open()`.

## File ownership

| Agent | Owns |
|---|---|
| director | `Scripts/Welcome/WelcomeFlow.cs`, `NerdyDirector.cs`, the station `Open(int)` contract, `sessionConfig.js` |
| lounge-bench | `AgentScripts/BuildLounge.cs`, `Scripts/Lounge/*` (room, scenery, arrival animation), `LoungeStyleTests` |
| dashboard | `Scripts/Catalog/*` (entries, the three filters, the six coming-soon placeholders), `Scripts/Presentation/Dashboard/*` (wall grid, toolbar under it, gear), the tile-art script |
| stores | `Scripts/Settings/SettingsStore.cs`, `LibraryStore.cs`, the settings panel |
| rundown | `Scripts/Lounge/Rundown*.cs`, its narration data, the helper controller card, stop gating |

Only the main session runs Unity: compile, builders in order, tests, renders, build, install.

## Voice tools (director agent)

`dashboard_open_tile {lesson_id, chapter}`, `dashboard_filter {featured|newest|most_viewed}`, `open_settings`,
`replay_rundown`. Asking to open a coming-soon tile is refused in one honest line. Same conventions as the lesson tools: declared in
`services/guide-proxy/lib/sessionConfig.js`, carried in `tools_now`, wrong-place calls refused with a short line,
and `ToolNameSyncTests` extended so C# and the proxy cannot drift. The proxy is deployed (2026-09-17), so this is
a code change plus `vercel deploy --prod`, not a blocked item.

## Report at the end

Files changed, tests added (and which you saw fail first, offline), builders to run in order, anything you could
not do.
