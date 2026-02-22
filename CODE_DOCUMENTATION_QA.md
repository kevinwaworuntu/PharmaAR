# PharmaAR Code Documentation (Q&A)

Scope: This document is based only on project code in `Assets/Scripts` (plus direct script relationships). It intentionally excludes package configuration, Unity project settings, and non-code assets.

## 1) What is the core purpose of this codebase?
The code implements an AR learning flow where users:
1. Choose a mode (`TBA` or `Kompleksometri`).
2. Select an unlocked stage (`tahap`).
3. Scan a marker and run step-by-step interactions (narration, audio, animation, contextual actions).
4. Complete the stage and persist progress.

## 2) Which scripts are the main entry points?
- `Assets/Scripts/MainMenu.cs`
  - Handles main menu button actions (start, how-to-play, team info, mode selection, reset progress, back).
- `Assets/Scripts/GameManager.cs`
  - Runtime coordinator for mode selection, stage start/completion, marker activation, and progress persistence.
- `Assets/Scripts/UIManager.cs`
  - Controls panel navigation/history, AR popup visibility, stage button lock state, and navigation buttons.

## 3) How does user flow work in code?
1. `MainMenu.PilihModeTBA()` or `MainMenu.PilihModeKompleksometri()` calls `GameManager.SetMode(...)`.
2. `GameManager.SetMode(...)` updates current mode, shows mode panel, refreshes stage button lock states.
3. Stage button (`TahapanController`) calls `GameManager.StartTahap(tahapIndex)`.
4. `GameManager.StartTahap(...)` activates mapped marker object and enables AR camera system.
5. Marker detection should call `ARContentManager.OnTargetFound()`.
6. `ARContentManager` starts `TahapanInteractionController.StartInteraction()`.
7. Interaction controller plays each mapped step via `TahapanInteractionPlayer` and optional custom `UnityEvent` logic.
8. When complete, `ARContentManager` wires completion button to `GameManager.CompleteCurrentTahap()`.

## 4) How is stage progression locked/unlocked?
- Stored with `PlayerPrefs` in `GameManager`.
- Two keys are used:
  - `LastCompletedTahapTBA`
  - `LastCompletedTahapKomp`
- Unlock rule in `UIManager.UpdateTahapButtonStates()`:
  - stage `i` is interactable if `i <= lastCompleted + 1`.

## 5) What code manages stage buttons and stage metadata?
- `Assets/Scripts/TahapanController.cs`
  - One component per stage button.
  - Owns `tahapIndex` and triggers stage start.
  - Applies visual state (alpha/interactable) via `CanvasGroup`.
- `Assets/Scripts/TahapanData.cs`
  - Holds stage metadata:
    - stage display name
    - Vuforia marker name
    - info panel reference

## 6) What is the AR interaction framework inside this code?
### Base orchestration
- `Assets/Scripts/ARContentManager.cs`
  - Base class for AR stage managers.
  - Subscribes to `TahapanInteractionController` events.
  - Handles:
    - marker found/lost callbacks
    - next/complete button visibility and actions

### Interaction timeline engine
- `Assets/Scripts/Gameplay/TahapanInteractionController.cs`
  - Executes ordered mappings of:
    - `TahapanInteractionData`
    - `IsNeedPlayerInputToContinue`
    - optional `UniqueEvent`
  - Emits events:
    - `OnStartWaitingForPlayerInputToContinue`
    - `OnFinishPlayingInteraction`
    - `OnInteractionComplete`
- `Assets/Scripts/Gameplay/TahapanInteractionPlayer.cs`
  - Plays animation/audio for a single interaction data item.
  - Updates narration panel text.
  - Completes step only when required media is finished.
- `Assets/Scripts/Gameplay/TahapanInteractionData.cs`
  - ScriptableObject fields: title, description, narration audio, animation clip.

## 7) What scripts implement custom interaction mechanics?
### Weighing interaction
- `Assets/Scripts/Gameplay/TimbanganInteractionManager.cs`
  - Creates contextual button(s) for adding sample amount.
  - Plays weighing animation.
  - Validates target weight before allowing continue.
- `Assets/Scripts/TimbanganObject.cs`
  - Numeric weight display and animated increase/decrease.
- `Assets/Scripts/Gameplay/TimbanganAnimNotify.cs`
  - Animation event helper for activating an object at runtime.

### Measuring cylinder / liquid addition
- `Assets/Scripts/Gameplay/GelasUkurFillInteractionManager.cs`
  - Handles two liquid types, target volumes, fill and meniscus visual updates.
  - Generates 1 ml / 10 ml contextual buttons.
  - Restarts interaction on overshoot.
- `Assets/Scripts/Gameplay/TBA6Manager.cs`
  - Inherits `GelasUkurFillInteractionManager` without extra behavior.

### Burette drip/titration interaction
- `Assets/Scripts/Gameplay/BuretteFillInteractionManager.cs`
  - Handles 0.1 ml / 1 ml actions.
  - Animates valve/drip sequence.
  - Updates burette fill and meniscus transforms.
  - Validates target window (floor/ceil), restarts on overshoot.
  - Changes titration material color on successful target.

## 8) How are contextual action buttons built?
- `Assets/Scripts/UI/ContextualButtonController.cs`
  - Dynamically instantiates button prefab list.
  - Registers per-button text and action delegates.
  - Destroys generated buttons after step completion.
- `Assets/Scripts/UI/ContextualButton.cs`
  - Wraps a Unity `Button` and `TextMeshProUGUI`.
  - Stores and invokes runtime action.

## 9) How is narration and info text handled?
- `Assets/Scripts/UI/NarationPanel.cs`
  - Shows title + description during interaction steps.
- `Assets/Scripts/GameManager.cs` + `Assets/Scripts/UIManager.cs`
  - Show/hide stage info popup panel and apply style configuration.
- `Assets/Scripts/InfoTextBank.cs`
  - Static fallback content arrays (`TBA`, `TK`) containing long instruction text.
- `Assets/Scripts/Data/InfoTextData.cs`
  - ScriptableObject key-value lookup store for info text.

## 10) Which scripts are configuration models?
- `Assets/Scripts/Config/AnimationConfig.cs`
  - Defines animator trigger parameter names and shared override controller entry.
- `Assets/Scripts/Config/InfoStyleConfig.cs`
  - ScriptableObject wrapper for text style.
- `Assets/Scripts/Config/InfoStyleStruct.cs`
  - Struct for font, size, alignment, spacing, margins.
- `Assets/Scripts/Enum/GameMode.cs`
  - Enum: `TBA`, `Kompleksometri`.

## 11) Which scripts look legacy/prototype/non-core?
Likely touch-manipulation prototypes and not part of main orchestrated flow:
- `Assets/Scripts/CSharpScaling.cs`
- `Assets/Scripts/OnClickForScaling.cs`
- `Assets/Scripts/DragObject.cs`
- `Assets/Scripts/Rotate.cs`
- `Assets/Scripts/RotateObject.cs`
- `Assets/Scripts/ObjectRotator.cs` (empty class)

## 12) What are key code-level risks visible from this code?
1. Event unsubscription lifecycle risk
- `ARContentManager` subscribes in `OnEnable`, unsubscribes in `OnDestroy` (not `OnDisable`), which can duplicate subscriptions if object toggles active.

2. Missing index validation
- `ContextualButtonController.RegisterAction()` and `RegisterTextToButton()` directly index list without guard.

3. Singleton assumptions
- Many call sites depend on `GameManager.Instance`/`UIManager.Instance` being present and wired.

4. Duplicate text sources
- Both `InfoTextBank` static arrays and `InfoTextData` assets exist, increasing drift risk.

5. Hardcoded interaction constants
- Fill/meniscus offsets and initial values are hardcoded in interaction managers.

## 13) What should a developer read first to understand behavior quickly?
Recommended order:
1. `Assets/Scripts/GameManager.cs`
2. `Assets/Scripts/UIManager.cs`
3. `Assets/Scripts/ARContentManager.cs`
4. `Assets/Scripts/Gameplay/TahapanInteractionController.cs`
5. `Assets/Scripts/Gameplay/TahapanInteractionPlayer.cs`
6. Stage-specific managers (`TimbanganInteractionManager`, `GelasUkurFillInteractionManager`, `BuretteFillInteractionManager`)

## 14) If I need to add a new stage, which code paths matter?
1. Add stage metadata (`TahapanData`) and button (`TahapanController` with correct `tahapIndex`).
2. Ensure `GameManager` arrays include the new stage and marker mapping.
3. Add/assign `TahapanInteractionController` mappings and interaction data assets.
4. If custom mechanics are needed, implement or reuse an `ARContentManager`-derived manager.
5. Verify `UIManager.UpdateTahapButtonStates()` reflects expected unlock behavior.

## 15) What utility script exists for non-UI tap detection?
- `Assets/Scripts/Utility/ScreenTapDetection.cs`
  - Uses Input System action (`<Pointer>/press`) to raise `OnScreenTappedDelegate`.
  - Ignores taps over UI (`EventSystem.IsPointerOverGameObject`).

## 16) Are there additional non-core scripts outside `Assets/Scripts`?
Yes, but out of this document’s scope focus:
- `Assets/Renderer/CustomRenderPassFeature.cs` (template render feature)
- `Assets/3D/Material/Mask.cs` (material vector updater)
- `Assets/Editor/Migration/AddVuforiaEnginePackage.cs` (editor migration helper)

