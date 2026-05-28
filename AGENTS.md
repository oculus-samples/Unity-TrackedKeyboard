# Agent Instructions — Tracked Keyboard Sample

Unity sample for the tracked-keyboard feature, demonstrating best-practice UX for text input across MR and VR with 2D / 3D keyboard visualization, desk / passthrough toggles, and tracking-status feedback.

## Source-of-truth files (read these first, do not duplicate their contents in this file)

For setup, build steps, SDK versions, and project layout, read:

- `README.md` — official setup, the "Integrating Tracked Keyboard within Your Own Project" walkthrough, and the known-limitations list
- `ProjectSettings/ProjectVersion.txt` — Unity editor version
- `Packages/manifest.json` — Unity package versions (Interaction SDK, MR Utility Kit, TextMesh Pro)
- `Assets/TrackedKeyboard/Scenes/TrackedKeyboard.unity` — main sample scene
- `Assets/TrackedKeyboard/ScriptableObjects/` — boundary-visual configuration
- `RuntimeActionBindings.json` — input action bindings
- `LICENSE.txt` — MIT for most of the project; `Text Mesh Pro` files are under Unity's Companion License

## Quest / Horizon-specific notes

- A physical Bluetooth keyboard must be paired and tracking-enabled in headset settings (Settings → Devices → Keyboard) before the sample is meaningful — there is also an in-app `Connect` button as a fallback.
- The feature requires a sufficiently new Horizon OS build; older devices/OS versions silently lose the tracked-keyboard surface. Check OS version on the device before debugging "no keyboard outline" reports.
- Keyboard tracking degrades in bad lighting, and the estimated desk height is derived from the keyboard's tracked position — both are README-documented limitations, not bugs to chase.
- The sample includes a `Custom Pointable Canvas Module` as a temporary mouse/keyboard input shim until Interaction SDK gains first-class mouse/keyboard support; per the README it can be removed once that lands. Don't refactor it away prematurely.
- Hand proximity to the keyboard is driven by capsule colliders added to `CameraRig > TrackingSpace > LeftHandAnchor / RightHandAnchor`; preserve these when modifying the rig.
- Before first build, run `Meta > Tools > Project Setup > Fix All` for both Android and Standalone, as the README integration steps require.

# Meta Quest tooling

This is a Meta Quest / Horizon OS sample. The bespoke intro above is the source of truth for what this project is and how it's built — use it (and the files it points at) instead of restating facts from memory.

When the user asks anything about Quest device behavior, build / deploy / debug / capture flows, on-device performance, or Horizon OS APIs, reach for these tools instead of generic Unity answers:

- **`hzdb`** — Quest-aware ADB wrapper (device list, install / launch / stop, logs, screenshots, Perfetto traces, on-device docs search). Already wired up as an MCP server via `.mcp.json`, `.vscode/mcp.json`, and `.cursor/mcp.json`. Also runnable directly: `npx -y @meta-quest/hzdb <subcommand>`.
- **Meta Quest Agentic Tools** — the full skill set, including Unity-specific skills: <https://github.com/meta-quest/agentic-tools>. Install per your client (Claude Code: `/plugin install meta-vr@meta-quest`; Gemini CLI: `gemini extensions install https://github.com/meta-quest/agentic-tools`; Cursor / VS Code: install the **Meta Horizon** extension from the Marketplace).

A few behavior expectations:

- **Read this repo's files first.** Before answering anything project-specific, read `README.md` and whichever source-of-truth files the intro above points at. Don't restate their contents in chat — quote or link instead.
- **Use `hzdb` for device-side work.** Anything that touches an attached Quest (install, launch, logs, screenshot, capture, manifest inspection) goes through `hzdb`, not raw `adb`.
- **Check live Horizon OS docs before answering API questions.** `hzdb docs search "..."` queries the live docs; training data on Horizon OS APIs goes stale fast.
- **Don't fabricate SDK / engine versions.** If a version isn't visible in this repo's files, say so rather than guessing.
