# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

2D parkour platformer built in **Unity 2019.4.3f1** (see `ProjectSettings/ProjectVersion.txt`) as a course project (1DV437 Introduction to Game Programming). The full intended feature set — wall jump, slide, dash, roll on landing, lives/score/timer HUD, packages to collect, traps, birds, menus — is described in `Assets/sg223bv-Game Description.pdf`. Only player movement and animation are implemented so far; treat the PDF as the spec for new features.

## Building and running

There is no CLI build, lint or test setup. Everything goes through the Unity Editor:

- Open the repo root as a project in Unity Hub with editor version 2019.4.3f1 (other versions will trigger an asset upgrade — avoid committing that).
- The playable scene is `Assets/Scenes/Level1.unity`. `EditorBuildSettings` has no scenes registered, so builds need scenes added first.
- `com.unity.test-framework` is installed but there are no tests or test assemblies.
- `Library/`, `Temp/`, `*.csproj`, `*.sln` are generated and gitignored.

## Code architecture

Game code lives in `Assets/Scripts/` (everything under `Assets/Asset Store/`, `Assets/GE Common Assets/` and `Assets/Free/` is third-party):

- `PlayerController.cs` — physics-driven movement on a `Rigidbody2D` + `CapsuleCollider2D`. Sets velocity directly each `Update` (no `FixedUpdate`). Ground/wall/ceiling detection uses `Physics2D.OverlapCircle` at child `Transform`s (`isGroundedCheck`, `leftWallCheck`, `rightWallCheck`, `topObstacleCheck`) against the `Ground` layer mask. Crouching resizes the capsule collider using hardcoded height/offset constants that match the character sprite. Facing is flipped by negating `transform.localScale.x`.
- `PlayerAnim.cs` — sits on the same GameObject and drives the `Animator`. It reads raw input itself (duplicating the key checks in `PlayerController`) and calls `PlayerController`'s public `CheckIfGrounded()` / `CheckIfHasWall()` and `rigBody`.
- Animator: `Assets/Asset Store/Free 2D Demo Character sprite sheet/Animation/PlayerAnim.controller` (the one wired up in `Level1`; `Player.controller` next to it is unused). Parameters set from code: `isRunning`, `isSprinting`, `isMoving`, `isGrounded`, `hasWall`, `crouch` (bools) and `jump` (trigger). Renaming a parameter requires changing both the controller and `PlayerAnim.cs`.

Not yet wired up: `wallSliding`/`wallSlidingSpeed` and `CheckIfGroundAbove()` exist in `PlayerController` but are unused.

### Controls

Legacy Input Manager (`ProjectSettings/InputManager.asset`): `Horizontal` axis (A/D, arrows), `Jump` button (W, up arrow, space), crouch on `S`/down arrow via `KeyCode`, sprint while holding `J`.

### Layers and sorting

- Physics layers (`TagManager.asset`): `Player` (8), `Ground` (9). Anything the player should stand on or wall-slide against must be on `Ground`.
- Sorting layers in order: `Background`, `Foreground`, `Player`, `Default`.

## Conventions

- Unity assets are YAML text; every asset has a `.meta` file with a GUID that scenes/prefabs reference. Always move/rename assets together with their `.meta` (preferably in the Editor), never edit GUIDs by hand.
- C# files are UTF-8 with BOM and CRLF line endings; keep that when editing.
- Commits reference GitHub issues where relevant (`closes #6`, `fixes #11`).
