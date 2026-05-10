# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Fours** is a Unity 6 (6000.4.x) puzzle game where players rotate 2x2 groups of colored squares on a grid to match a target configuration. Written in C#, all game scripts live under `Assets/Scripts/`.

## Build & Run

This is a Unity project — open it in Unity Editor 6000.4.x. There is no CLI build or test pipeline; everything runs through the Unity Editor. The level editor is at **Tools > Board > Level Editor** in the menu bar. Code analysis via Qodana (`qodana.yaml`, QDNET profile).

## Key Dependencies
- **Reflex** (v13.0.3) — DI framework
- **UniTask** (Cysharp) — async/await replacing coroutines
- **PrimeTween** — animation library
- **Unity Input System** (v1.19.0) — new Input System with Main/UI action maps

## Architecture

### Core Game Loop
- **LevelManager** (singleton) loads a `LevelData` ScriptableObject into two grids: a `PlayableGrid` (player-interactive) and a `TargetGrid` (goal state). When the player's grid snapshot matches the target's, a `LevelCompletedEvent` fires.
- **PlayableGrid** extends `SpriteGrid` — handles input (tap/click to select a dot, swipe/right-click to rotate). Uses a **command pattern** (`CommandManager`, `SelectDotCommand`, `RotateGroupCommand`) for undo/redo.
- **SquareGroup** represents a 2x2 block of squares that can be rotated. Rotation animation uses PrimeTween sequences. `GridGroupFinder` discovers all valid 2x2 groups from `GridData`.
- **DotManager** places interactive dots at the center of each `SquareGroup`.

### Key Patterns
- **Dependency Injection**: Uses [Reflex](https://github.com/gustavopsantos/Reflex). Bindings in `ProjectInstaller` (singletons: logger, level manager, input, swipe detector) and `SceneInstaller`. Inject with `[Inject]` attribute.
- **Event Bus**: Generic static `EventBus<T>` for decoupled communication. Key events: `LevelLoadedEvent`, `LevelCompletedEvent`, `GroupRotatedEvent`, `UIEvent`. Register/deregister bindings manually.
- **State Machine**: Generic FSM in `StateMachine/` — used by `UIManager` to manage UI screen transitions (MainMenu, InGame, LevelSelect, LevelComplete, Paused, etc.) with PrimeTween slide animations.
- **Singletons**: `Singleton<T>`, `PersistentSingleton<T>`, `RegulatorSingleton<T>` base classes in `Singletons/`.

### Level System
- `LevelData` (ScriptableObject) stores grid dimensions, initial/target square configurations, move thresholds for star ratings, and solution steps.
- `LevelEditorWindow` is a custom Unity Editor window for creating and editing levels visually.
- `ColorPalette` ScriptableObject defines available colors for levels.

### Input
- `InputManager` (singleton) wraps Unity's new Input System. Exposes events: `Tap`, `LeftClick`, `RightClick`, `SwipeLeft`, `SwipeRight`.
- `SwipeDetector` handles touch/mouse swipe gesture detection.

### Async
- Uses **UniTask** (Cysharp) for async operations, not Unity coroutines. Grid rotation, level loading, and UI transitions are all async.

## Code Conventions
- Namespaces match folder structure (`Board`, `Levels`, `UI`, `EventBus`, `StateMachine`, etc.)
- Never under absolutely any cirumstances use a character that cannot be typed on a standard american keyboard. especially arrows and em dashes that arent a minus sign.
- `[Required]` custom attribute marks serialized fields that must be assigned in the Inspector
- `[ScriptableObjectDropdown]` custom attribute for ScriptableObject selection in Inspector
- Logger aliasing: `using ILogger = Logging.ILogger;` (to avoid conflict with `UnityEngine.ILogger`)
- All animations use **PrimeTween** (not DOTween or coroutines)
- Prefer `[Inject]` over direct `Singleton<T>.Instance` access
- `RuntimeResolver` provides fallback resolution (Scene -> Project container) when `[Inject]` isn't available

## File Encoding
- Never add a UTF-8 BOM (Byte Order Mark) to files. Save all C# and text files as UTF-8 without BOM. When editing an existing file, preserve its current encoding and never introduce a BOM.