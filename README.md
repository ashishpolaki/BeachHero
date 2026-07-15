# Beach Hero

Beach Hero is a Unity mobile game built around a draw-path rescue loop. The player chooses a level from a map, draws a path for the boat, saves drowning characters, collects currency, avoids obstacles, and progresses through a level-map meta loop with stars, boats, powerups, ads, purchases, and tutorials.

## Project Basics

- Unity version: `6000.4.1f1` according to `ProjectSettings/ProjectVersion.txt`.
- Main runtime scenes:
  - `Assets/Scenes/Init.unity`
  - `Assets/Scenes/Game.unity`
- Editor/tooling scenes:
  - `Assets/Scenes/GameEditorScene.unity`
  - `Assets/Scenes/MapEditorScene.unity`
  - `Assets/Scenes/Test.unity`
- Main script root: `Assets/Scripts`.
- Main authored data root: `Assets/ScriptableObjects`.
- Architecture memo: `Docs/BeachHero_Architecture_Hardening_Memo.md` and PDF beside it.

## How To Run

1. Open the project in Unity Hub using the Unity version listed in `ProjectSettings/ProjectVersion.txt`.
2. Let Unity restore packages from `Packages/manifest.json`.
3. Open `Assets/Scenes/Init.unity`.
4. Press Play.

The `Init` scene bootstraps services, loads the `Game` scene additively, spawns the current level, opens the main menu, and unloads `Init`.

If you are working in the editor, use the Unity menu:

- `Beach Hero/Scenes/Init`
- `Beach Hero/Scenes/Game`
- `Beach Hero/Scenes/Level Editor`
- `Beach Hero/Scenes/Map Editor`
- `Beach Hero/Scenes/Test`
- `Beach Hero/Level Editor Window`

## Gameplay Flow

Current high-level loop:

1. `Initializer` boots global controllers.
2. `GameController` initializes progression, powerups, store, and current level.
3. `MainMenuUIScreen` opens.
4. Player opens the map.
5. `MapController` selects a level and starts gameplay.
6. `LevelController` handles level spawning, path drawing, simulation, collectables, obstacles, and win/fail state.
7. Result UI handles win/fail actions such as next level, retry, home, skip, and rewarded ad options.

## Important Systems

- `GameController`: macro game state and level flow.
- `LevelController`: current level runtime loop.
- `MapController`: level-map visuals and map navigation.
- `UIController` and `UIScreenManager`: screen stack and transitions.
- `StoreManager`: IAP and coin purchases.
- `SkinController`: boat and color ownership/selection.
- `PowerupController`: powerup unlocks, balances, and activation.
- `AdController`: Google Mobile Ads wrapper and ad policy.
- `SaveSystem`: ES3-backed static save helpers.
- `RemoteConfig`: Firebase Remote Config values.

See the architecture memo for known coupling points and the recommended hardening roadmap.

## Level Editing

Use `Beach Hero/Level Editor Window` in Unity. The level editor expects `GameEditorScene` to be active and edits ScriptableObject level data under:

`Assets/ScriptableObjects/Levels`

The project also has scene menu shortcuts under `Beach Hero/Scenes` for fast scene switching.

## Repository Hygiene

- Do not commit Unity-generated folders such as `Library`, `Temp`, `Obj`, `Logs`, or `UserSettings`.
- Do not commit macOS `.DS_Store` files.
- Keep unrelated Unity asset, prefab, package, and project setting changes unstaged unless they are part of the task.
- Treat `.unity`, `.prefab`, `.asset`, and `.meta` files carefully. They can contain user/editor changes that are unrelated to code work.
- Commit generated PDFs as binary files; `.gitattributes` contains `*.pdf binary`.

## Current Architecture Direction

The project should be hardened incrementally:

1. Stabilize progression, stars, rewards, ads, and UI stack behavior.
2. Introduce typed progress state and ES3 migration wrappers.
3. Add a central flow service for main menu, map, gameplay, result, retry, skip, and home.
4. Move content spawning toward registries/factories for obstacles, collectables, powerups, and rewards.
5. Reduce direct singleton cross-calls over time.

The first recommended implementation batch is documented in `Docs/BeachHero_Architecture_Hardening_Memo.md`.
