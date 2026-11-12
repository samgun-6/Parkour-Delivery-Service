# Parkour Delivery Service

A 2D parkour platformer made in Unity, originally a course project for
1DV437 Introduction to Game Programming. You play a courier on the
fastest delivery service in town: run, jump, slide and wall-jump across
rooftops, pick up packages and deliver them at the end of each run.

The full design is in [docs/Game Description.pdf](docs/Game%20Description.pdf).

## Requirements

- Unity **2019.4.3f1** (see `ProjectSettings/ProjectVersion.txt`)
- [Git LFS](https://git-lfs.com/) — run `git lfs install` once before cloning

## Setup

The game uses free packs from the Unity Asset Store. Their licenses do
not allow redistributing them in this repository, so `Assets/ThirdParty/`
is git-ignored and every developer imports the packs themselves:

1. Clone the repo and open it in Unity Hub with the editor version above.
2. Sign in to Unity and add the packs below to *My Assets* on the Asset Store.
3. In the editor, open **Window → Package Manager → My Assets**, download and
   import each pack.
4. In the Project window, move each imported pack folder into
   `Assets/ThirdParty/`. Move them inside the editor (not in Explorer) so the
   `.meta` files follow. References keep working because they use the pack's
   asset GUIDs, not paths.

| Pack | Used for |
|------|----------|
| [2D Demo Character Sprite Sheet](https://assetstore.unity.com/packages/2d/characters/2d-demo-character-sprite-sheet-137085) | Player sprites |
| [White City](https://assetstore.unity.com/packages/3d/environments/urban/white-city-76766) | Level buildings |
| [Simple City Pack Plain](https://assetstore.unity.com/packages/3d/environments/urban/simple-city-pack-plain-100348) | Level buildings |
| [Fantasy Skybox FREE](https://assetstore.unity.com/packages/2d/textures-materials/sky/fantasy-skybox-free-18353) | Background |
| [100 Alchemy Item Icons Free](https://assetstore.unity.com/packages/2d/gui/icons/100-alchemy-item-icons-free-72605) | Collectable packages |
| [2D Death Traps Free](https://assetstore.unity.com/packages/2d/environments/2d-death-traps-free-20706) | Traps |
| [2D Death Traps Ice Free](https://assetstore.unity.com/packages/2d/environments/2d-death-traps-ice-free-24714) | Traps |
| [2D Sprites Pack](https://assetstore.unity.com/packages/essentials/asset-packs/2d-sprites-pack-73728) | Enemy birds |
| [Epic Rock Track](https://assetstore.unity.com/packages/audio/music/rock/epic-rock-track-60043) | Music |
| [Attack Jump Hit Damage Human Sounds](https://assetstore.unity.com/packages/audio/sound-fx/voices/attack-jump-hit-damage-human-sounds-32785) | Sound effects |

## Playing

Open `Assets/_Project/Scenes/Level1.unity` and press Play.

| Action | Keys |
|--------|------|
| Move | A / D or ← / → |
| Jump | Space, W or ↑ |
| Crouch / slide | S or ↓ |
| Sprint | Hold J |

## Project structure

```
Assets/
  _Project/      Everything made for this game (scripts, scenes, animations, art)
  ThirdParty/    Imported Asset Store packs (git-ignored)
docs/            Game design document
```

## Merging Unity assets

Scenes and prefabs are text (YAML). To let git use Unity's Smart Merge,
add this to your git config, pointing at your editor install:

```
[merge "unityyamlmerge"]
    name = Unity SmartMerge
    driver = '<Unity install>/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A
    recursive = binary
```

## License

Code is MIT licensed, see [LICENSE](LICENSE). Asset Store content is
covered by the Unity Asset Store EULA and is not part of this repository.
