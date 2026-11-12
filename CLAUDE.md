# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Workflow overrides

These override the global "never run `git add`" / "commit only when I say" rules, for this repo only:

- Claude stages and commits its own work without waiting for review, still one logical commit at a time (conventional commits, English, Co-Authored-By trailer), and continues to the next planned commit.
- `git push` (including force-push), branch deletion on the remote and GitHub settings changes always require explicit approval.

## Project

2D parkour platformer in **Unity 6.3 LTS (6000.3.28f1)**, see `ProjectSettings/ProjectVersion.txt`. Originally a course project (1DV437); the design document `docs/Game Description.pdf` is the spec for features. Level 1 is complete (packages, hazards, birds, checkpoints, delivery, HUD, menus, audio); a main menu, level select, high-score screen and sound options from the PDF are not built yet.

Asset Store content lives in `Assets/ThirdParty/`, which is **git-ignored** (licences forbid redistribution). It exists on this machine but not in a fresh clone; README lists the packs. Scenes reference those assets by GUID, so they must be imported, not recreated. Never commit anything under `Assets/ThirdParty/`.

## Building and running

Everything runs headless through the editor (no editor window needed; a Personal licence must be active in Unity Hub). From the repo root in Git Bash:

```bash
U="/c/Program Files/Unity/Hub/Editor/6000.3.28f1/Editor/Unity.exe"; P="$(pwd -W)"
"$U" -batchmode -quit -projectPath "$P" -logFile "$P/Logs/compile.log"                      # import + compile
"$U" -batchmode -projectPath "$P" -runTests -testPlatform EditMode -assemblyNames ParkourDelivery.Tests.EditMode -testResults "$P/Logs/editmode.xml" -logFile "$P/Logs/editmode.log"
"$U" -batchmode -projectPath "$P" -runTests -testPlatform PlayMode -assemblyNames ParkourDelivery.Tests.PlayMode -testResults "$P/Logs/playmode.xml" -logFile "$P/Logs/playmode.log"
#   single test/class: add  -testFilter ShaftCanBeClimbedWithWallJumps   (or a class name)
"$U" -batchmode -projectPath "$P" -executeMethod ParkourDelivery.Editor.BatchTools.ValidateScenes -logFile "$P/Logs/validate.log"
"$U" -batchmode -projectPath "$P" -executeMethod ParkourDelivery.Editor.BatchTools.CaptureShots -shots "10,6;95,26" -logFile "$P/Logs/shots.log"   # -> Logs/shots/*.png
"$U" -batchmode -projectPath "$P" -executeMethod ParkourDelivery.Editor.ArtGenerator.GenerateAll -logFile "$P/Logs/art.log"
"$U" -batchmode -projectPath "$P" -executeMethod ParkourDelivery.Editor.Level1Builder.BuildAndExit -logFile "$P/Logs/level.log"
```

- Exit code 0/1/2 tells success; details are in the log (`grep -E 'error CS|\[BatchTools\]|\[Level1Builder\]'`) and the NUnit XML.
- Only one editor may have the project open; batch runs fail while the Unity Editor has it open.
- `EPERM ... rename ... Library\PackageCache` during package resolution is a transient Windows file lock; rerun.
- Play mode tests use Input System's `InputTestFixture`; in batch mode call `InputSystem.Update()` before simulated presses (see `PlayerFixture.PressKey`).

## Code architecture

Own code is under `Assets/_Project/` in assemblies `ParkourDelivery` (runtime, `Scripts/`), `ParkourDelivery.Editor` (`Editor/`) and the two test assemblies (`Tests/EditMode`, `Tests/PlayMode`).

- **Player** (`Scripts/Player/`): `PlayerController` reads the Input System actions in `Update` (jump presses latched via the `performed` callback), and applies movement, jumps, wall slide/jump and extra fall gravity in `FixedUpdate`. Ground/wall/ceiling checks are `OverlapCircle`s at child transforms against the `Ground` layer; wall side is derived from checker positions because the checkers flip with the sprite. It exposes read-only state (`IsGrounded`, `HasWall`, `IsCrouching`, `IsWallSliding`, …) and a `Jumped` event; `PlayerAnim` drives the Animator from that state only.
- **Game loop** (`Scripts/Core/`): `GameManager` (singleton per scene) owns the state machine (Countdown → Playing ⇄ Paused → LevelComplete/GameOver), `RunStats` (lives, packages, time), respawn at the last checkpoint, and scoring via `ScoreCalculator`. It enables/disables the `PlayerController` to freeze control. `GameUI` (`Scripts/UI/`, builds its uGUI canvas at runtime) and `GameAudio` only listen to its events.
- **Level pieces** (`Scripts/Gameplay/`): `Package`, `DeliveryPoint`, `Hazard` (traps, birds, kill zone), `Checkpoint`, `Bird`, `CameraFollow`. They find the player via `GetComponentInParent<PlayerController>` and talk to `GameManager.Instance`.
- **Level content is generated, not hand-placed**: `Level1Builder` rebuilds everything under the scene's `Level` and `Game` roots from coordinate tables; edit the tables and rerun it rather than editing the scene by hand. `ArtGenerator` draws the own sprites/sounds into `Art/Generated/`. `Level1RouteTests` play the shaft, tunnel and sprint gap in the real scene, so rerun them after changing layout or movement numbers.
- Animator: `Assets/_Project/Animations/Player/PlayerAnim.controller`; parameters `isRunning`, `isSprinting`, `isMoving`, `isGrounded`, `hasWall`, `crouch` (bools) and `jump` (trigger). Renaming one means changing the controller and `PlayerAnim.cs`.

### Controls

Input System asset `Scripts/Input/PlayerControls.inputactions` with a generated C# wrapper (`PlayerControls.cs`, regenerated on import). Player map: Move (A/D, arrows, left stick), Jump (Space/W/Up, south button), Crouch (S/Down, east), Sprint (hold J, west). Game map: Pause (Esc, Start). When adding an action, import the asset first so the wrapper exists before code uses it. The legacy Input Manager is disabled (`activeInputHandler: 1`).

### Layers and sorting

- Physics layers (`TagManager.asset`): `Player` (8), `Ground` (9). Anything the player should stand on or wall-slide against must be on `Ground`; triggers (packages, hazards) stay on `Default`.
- Sorting layers in order: `Background` (facades, clouds), `Foreground` (pickups, hazards), `Player`, `Default`.
- The real player's collider is 2.56 × 2.56 with wall checkers at ±1.28; level gaps and headroom are sized for that.

## Conventions

- Unity assets are YAML text; every asset has a `.meta` file with a GUID that scenes/prefabs reference. Always move/rename assets together with their `.meta` (preferably in the Editor), never edit GUIDs by hand.
- Keep each C# file's encoding (the original player scripts have a UTF-8 BOM); line endings are normalized by `.gitattributes`.
- Binary assets go through Git LFS (`.gitattributes`).
- Commits reference GitHub issues where relevant (`closes #6`, `fixes #11`).
