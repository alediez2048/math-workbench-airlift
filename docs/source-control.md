# Source control

Use a private GitHub repository for this project. Keep the baseline on `main` and perform the Unity build on `unity-airlift`.

Commit requirements, plans, mockups, scripts, Unity `Assets` with their `.meta` files, `Packages` (including the lock file), and `ProjectSettings`. Do not commit generated caches, builds, credentials, or signing keys. The Unity project is planned under `unity/`; the LFS patterns target binary assets there. Extend the patterns if that location changes or new binary formats are added.

Before each commit, inspect `git status` and `git diff`. Commit a small, named milestone and push to GitHub to back it up; a local commit alone is not an off-device backup. Never force-push shared history or discard uncommitted work without explicit approval. Keep asset redistribution rights in mind even for a private repository.

When the Editor is available, verify Visible Meta Files and Force Text serialization, then configure UnityYAMLMerge using that installed Editor. These Editor settings have not yet been verified. Git cannot meaningfully merge most binary art files; coordinate edits to those files.

## Setup checkpoint — 2026-09-14

- Git, GitHub CLI authentication, Git LFS, and Unity Hub are available.
- Node 22 is installed at `/opt/homebrew/opt/node@22/bin/node`; the existing default Node version and shell configuration were not changed. Homebrew updated shared dependencies as part of installation.
- Updated checkpoint: the user reports editor installation complete. Local inspection found `6000.6.0f1`; Android modules are absent/unselected. This differs from the planned 6.3 LTS family. See [the progress log](PROGRESS.md) for current blockers and next actions.
- Android Build Support, Android SDK & NDK Tools, and OpenJDK should be installed through Unity Hub for the selected Editor. Android Studio is not required for this workflow.
- Meta Quest Developer Hub is an optional device-management and capture companion. Its license and account onboarding remain user-owned.
- Editor automation is not configured. Review Unity's current authorized agentic-access terms and its official Unity CLI/Pipeline route before enabling any bridge; earlier third-party bridge suggestions are provisional.

References: [Unity CLI overview](https://docs.unity.com/en-us/hub/cli-overview), [Unity terms](https://unity.com/legal/terms-of-service).
