# PharmaAR Project Documentation

## 1. Project Overview
PharmaAR is a Unity-based augmented reality learning application for pharmaceutical lab workflows. The project contains two learning modes:
- `TBA` (Titrasi Bebas Air)
- `Kompleksometri`

Learners select a mode, progress through locked/unlocked `tahap` (stages), scan AR markers, and follow guided interaction sequences with animations, narration, and contextual action buttons.

## 2. Tech Stack and Environment
- Unity version: `6000.0.39f1` (`ProjectSettings/ProjectVersion.txt`)
- Render pipeline: URP (`com.unity.render-pipelines.universal`)
- AR framework: Vuforia (`com.ptc.vuforia.engine` local tgz package)
- Unity XR packages: AR Foundation, ARCore, XR Interaction Toolkit, Mock HMD, XR Simulation content
- Input: Unity Input System (`com.unity.inputsystem`)

Primary dependencies are defined in `Packages/manifest.json`.

## 3. Entry Scene and Build Configuration
`ProjectSettings/EditorBuildSettings.asset` shows:
- Enabled build scene: `Assets/Scenes/ARPharma.unity`
- Additional disabled scene: `Assets/Scenes/Tes.unity`

Other scenes in `Assets/Scenes` appear to be development, backup, or test scenes.

## 4. High-Level Runtime Architecture
Main runtime singletons and orchestration:
- `GameManager` (`Assets/Scripts/GameManager.cs`)
- `UIManager` (`Assets/Scripts/UIManager.cs`)
- `ContextualButtonController` (`Assets/Scripts/UI/ContextualButtonController.cs`)

Core flow:
1. `MainMenu` routes user to mode selection, help/info pages, or exits app.
2. User chooses mode (`TBA` or `Kompleksometri`).
3. `GameManager` updates mode, shows mode panel, and updates stage button lock state.
4. User selects stage (`TahapanController`).
5. `GameManager.StartTahap()` enables marker mapping object and opens AR scan panel.
6. Marker found -> `ARContentManager.OnTargetFound()` -> starts `TahapanInteractionController` sequence.
7. Stage-specific manager (e.g., balance/fill/titration) can inject custom interactions via UnityEvents and contextual buttons.
8. On completion, `GameManager.CompleteCurrentTahap()` persists progress in `PlayerPrefs`, disables marker, and returns to previous panel.

## 5. Script Responsibilities

### 5.1 Core Managers
- `GameManager`
  - Owns current mode and stage progression.
  - Holds stage data arrays (`tahapanTBA`, `tahapanKompleksometri`) and marker mappings.
  - Persists progress with mode-specific keys:
    - `LastCompletedTahapTBA`
    - `LastCompletedTahapKomp`
  - Controls Vuforia enable/disable via `VuforiaBehaviour.Instance.enabled`.
  - Applies info text style configuration.

- `UIManager`
  - Handles panel navigation/history.
  - Controls AR popup visibility.
  - Updates stage button interactable/alpha states according to progress.
  - Wires info panel and animation navigation buttons.

- `MainMenu`
  - Button event bridge to panel navigation and mode selection.

### 5.2 Stage and Interaction System
- `TahapanController`
  - Bound to each stage button.
  - Starts stage via `GameManager.StartTahap()`.
  - Shows info panel trigger.

- `TahapanData`
  - Per-stage metadata: stage name, Vuforia marker name, and info panel reference.

- `ARContentManager` (base for AR stage managers)
  - Connects to `TahapanInteractionController` events.
  - Handles found/lost marker callbacks.
  - Manages next/complete interaction button behavior.

- `TahapanInteractionController`
  - Executes ordered interaction mappings:
    - `TahapanInteractionData`
    - `IsNeedPlayerInputToContinue`
    - optional `UniqueEvent`
  - Uses `TahapanInteractionPlayer` for animation/audio playback.
  - Emits events for wait/finish/complete states.

- `TahapanInteractionData` (ScriptableObject)
  - Narration title/description
  - Audio clip
  - Animation clip

- `TahapanInteractionPlayer`
  - Applies override animation clip through shared `AnimationConfig`.
  - Plays audio and animation in parallel.
  - Calls completion callback when both finish.

### 5.3 Stage-Specific Gameplay Managers
- `TimbanganInteractionManager`
  - Implements weighing interaction.
  - Uses `TimbanganObject` display updates and powder fill visual scaling.

- `GelasUkurFillInteractionManager`
  - Handles 1 ml / 10 ml additions for two solution types.
  - Tracks target volumes, updates fill/meniscus visuals, and resets on overshoot.

- `BuretteFillInteractionManager`
  - Handles burette drip interactions.
  - Supports different increments (0.1 ml and 1 ml), color transition on target completion, and restart on overshoot.

- `TBA6Manager`
  - Inherits `GelasUkurFillInteractionManager` (currently no additional logic).

### 5.4 UI Utilities
- `ContextualButton` and `ContextualButtonController`
  - Runtime-generated buttons for per-step user actions.

- `NarationPanel`
  - UI wrapper for title and description display.

### 5.5 Data and Config
- `AnimationConfig` (ScriptableObject)
  - Shared animation parameter names and generic override controller.

- `InfoStyleConfig` + `InfoStyleStruct`
  - Text style data for info popups.

- `InfoTextData` (ScriptableObject)
  - Key-value info text store (linear lookup).

- `InfoTextBank`
  - Static fallback arrays for TBA and TK info text.

### 5.6 Legacy/Prototype Scripts (likely non-core)
Located in `Assets/Scripts`:
- `CSharpScaling`, `OnClickForScaling`, `Rotate`, `RotateObject`, `DragObject`, `ObjectRotator`

These implement touch drag/rotate/scale patterns and appear separate from the main stage orchestration architecture.

## 6. Data and Content Layout
Important content locations:
- `Assets/ARPharma/Config`
  - `AnimationConfig.asset`
  - `Default_InfoStyleConfig.asset`
- `Assets/ARPharma/Data`
  - `TBA_InfoTextData.asset`
  - `TK_InfoTextData.asset`
  - per-stage interaction assets under `TBA/` and `TK/`
- `Assets/Scripts`
  - all custom runtime code
- `Assets/Scenes`
  - primary and development scenes

Notes:
- TBA interaction data is organized by stage folders `1` through `10`.
- TK interaction data is split between stage folders and root-level TK assets (e.g., stage 5 and some stage 6 assets), so layout is not fully normalized.

## 7. Progression and Persistence Model
- Progress is stored per mode in `PlayerPrefs` as the latest completed stage index.
- Stage unlock rule: stage `i` is interactable when `i <= lastCompleted + 1`.
- Reset clears both progression keys and returns to non-AR state.

## 8. AR and Marker Workflow
- Stage start activates only the marker mapping object for current stage and enables Vuforia behavior.
- On marker found, interaction sequence starts if info panel is not active.
- On marker lost, AR popups are hidden.
- On stage finish, marker object is disabled and AR camera behavior is turned off.

## 9. Known Technical Risks and Improvement Opportunities
Based on current code analysis:

1. Null-safety gaps around singleton assumptions
- Some calls assume `UIManager.Instance` or `GameManager.Instance` is always available.
- Risk: runtime null exceptions when scene wiring is incomplete.

2. Event lifecycle mismatch potential
- `ARContentManager` subscribes in `OnEnable` but unsubscribes only in `OnDestroy`.
- If component toggles enabled/disabled repeatedly, duplicate subscriptions can occur.

3. `ContextualButtonController` index safety
- `RegisterAction` and `RegisterTextToButton` do not validate index bounds.

4. Data duplication and naming inconsistency in interaction assets
- Multiple assets with suffixes like `" 1"`, `_1`, `_2`, etc.
- Increases maintenance cost and risk of wrong asset linkage.

5. Unused or prototype scripts mixed with production scripts
- Makes ownership and intended architecture less clear.

6. Hardcoded visual constants in interaction managers
- Fill scale and meniscus offset constants are script-level magic numbers.
- Better moved into serialized config assets.

7. `InfoTextBank` static text duplication with `InfoTextData` ScriptableObjects
- Two content sources can drift and cause inconsistencies.

8. Minor typo/consistency issues
- Method name typo: `BackFrromCurrentTahap()`.
- Naming mixes Bahasa Indonesia and English in ways that may reduce maintainability for external contributors.

## 10. How to Extend the Project Safely

### Add a New Stage
1. Add/prepare stage interaction data assets (`TahapanInteractionData`) under `Assets/ARPharma/Data`.
2. Configure the stage mapping in scene inspector:
- `GameManager` stage arrays and marker mapping arrays.
- `TahapanController` button index.
3. Wire a `TahapanInteractionController` with mappings and optional `UniqueEvent` callbacks.
4. Ensure popup references are added to `UIManager.allARPopups` if required.
5. Validate progression lock/unlock behavior.

### Add a New Interaction Type
1. Create a manager inheriting `ARContentManager` if it participates in stage sequence lifecycle.
2. Use `ContextualButtonController` for runtime action buttons.
3. Reuse `TahapanInteractionController` for narration/audio/animation step playback.
4. Keep visual constants serialized where possible.

## 11. Suggested Refactor Roadmap
1. Normalize interaction data folder structure (`TBA/<tahap>`, `TK/<tahap>`).
2. Move all textual content to `InfoTextData` assets and retire static `InfoTextBank` fallback.
3. Improve subscription lifecycle (`OnEnable` subscribe / `OnDisable` unsubscribe) in AR interaction managers.
4. Add validation helpers for contextual button index access.
5. Introduce a lightweight editor validation script for scene wiring and data completeness.

## 12. Quick Start for Maintainers
1. Open with Unity `6000.0.39f1`.
2. Confirm scene `Assets/Scenes/ARPharma.unity` is in build and enabled.
3. Verify local package `Packages/com.ptc.vuforia.engine-11.4.4.tgz` exists.
4. Open `GameManager` in scene and check:
- mode stage arrays
- marker mappings
- animation config
5. Enter Play Mode from `ARPharma.unity` and test both modes from home UI.
