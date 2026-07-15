# Beach Hero Agent Guide

This file is for coding agents and automation working in the Beach Hero Unity project. Follow it before editing files.

## Mission

Keep the project runnable while making scoped, reviewable changes. This repo often has unrelated Unity-generated or editor-touched files in the working tree, so never assume every modified file belongs to the current task.

## First Checks

Run these before making decisions:

```sh
git status --short --branch
sed -n '1,80p' ProjectSettings/ProjectVersion.txt
sed -n '1,160p' Packages/manifest.json
```

Prefer `rg` for search:

```sh
rg -n "GameController|LevelController|ScreenEvent|SaveSystem|GetInstance" Assets/Scripts
```

## Run The Game

Human/editor path:

1. Open the project in Unity Hub with the version in `ProjectSettings/ProjectVersion.txt`.
2. Open `Assets/Scenes/Init.unity`.
3. Press Play.

Unity menu shortcuts:

- `Beach Hero/Scenes/Init`
- `Beach Hero/Scenes/Game`
- `Beach Hero/Scenes/Level Editor`
- `Beach Hero/Scenes/Map Editor`
- `Beach Hero/Scenes/Test`
- `Beach Hero/Level Editor Window`

Do not start from `Game.unity` for normal play unless you are intentionally testing scene internals. `Init.unity` owns the normal boot path.

## Core Project Map

- Runtime scripts: `Assets/Scripts`
- Controllers: `Assets/Scripts/Controllers`
- Gameplay entities: `Assets/Scripts/Gameplay`, `Assets/Scripts/Obstacles`, `Assets/Scripts/Collectables`
- UI screens/tabs: `Assets/Scripts/UI`
- Level data types: `Assets/Scripts/Level`
- Map flow: `Assets/Scripts/Map`
- Editor tools: `Assets/Scripts/Editor`
- Level assets: `Assets/ScriptableObjects/Levels`
- Architecture docs: `Docs`

## Current Runtime Shape

Current loop:

1. `Initializer` starts global systems.
2. `UIController.LoadingUI` loads `Game` additively.
3. `GameController.SpawnLevel()` prepares the current level and opens the main menu.
4. `MainMenuUIScreen` sends the player to the map.
5. `MapController` selects/animates level entry.
6. `LevelController` spawns the level, handles path drawing, updates simulation, and resolves win/fail.
7. Result tabs handle next/retry/home/skip/ad actions.

Known architecture issue: UI, gameplay, ads, saves, and economy currently call each other directly through singletons. Prefer additive seams and compatibility wrappers over rewrites.

## Safe Change Rules

- Keep changes narrowly scoped to the requested task.
- Do not revert unrelated modified files.
- Do not stage unrelated `.asset`, `.prefab`, `.unity`, `.meta`, `Packages`, or `ProjectSettings` changes.
- If touching Unity serialized files, inspect diffs carefully before staging.
- Add new code in the existing namespace: `BeachHero`.
- Preserve current scenes, prefabs, ScriptableObjects, save keys, and public methods unless the task explicitly includes migration.
- Prefer compatibility facades and small vertical slices over broad controller rewrites.

## Architecture Direction

Use the roadmap in `Docs/BeachHero_Architecture_Hardening_Memo.md`.

Preferred long-term direction:

- `GameFlowService` owns menu/map/game/result transitions.
- `PlayerProgress` and `IProgressRepository` own typed save state.
- `EconomyService`, `ProgressionService`, `PowerupService`, and `AdService` own business rules.
- UI screens send intent, not business logic.
- Gameplay entities emit facts/events, not UI/economy/ad calls.
- Spawning uses registries/factories instead of large enum switches.

## Manual Smoke Checklist

Use this checklist after gameplay or flow changes:

- Boot from `Init`.
- Main menu opens.
- Map opens.
- Current level marker is correct.
- Start level from map.
- Draw a valid path.
- Win a level.
- Coins/stars are granted once.
- Continue to next level.
- Retry a failed level.
- Skip level through rewarded ad path.
- Return home.
- Open store.
- Open boat customization.
- Open settings.
- No-internet/ad-unavailable paths do not break flow.

## Git Workflow

Before staging:

```sh
git status --short
git diff --check
```

Stage explicitly:

```sh
git add path/to/file1 path/to/file2
```

Confirm staged files:

```sh
git diff --cached --name-status
git diff --cached --stat
```

Only commit files that belong to the task. Leave unrelated local Unity changes unstaged.
