# Unity Camera System - Shot Selection Guide

## Overview

The Camera System now includes comprehensive shot selection functionality that allows you to easily navigate, preview, and test individual camera shots within your sequence. This guide covers all the selection features and how to use them effectively.

## Shot Selection Features

### Core Selection Properties
- **`selectedShotIndex`**: Currently selected shot index (-1 for none selected)
- **`highlightSelectedShot`**: Enable visual highlighting of the selected shot
- **`showOnlySelectedShot`**: Hide all other shots except the selected one

### Navigation Methods

#### Context Menu Commands
All shot selection methods are available in the Unity Inspector context menu:

1. **Select Next Shot** - Navigate to the next shot in sequence
2. **Select Previous Shot** - Navigate to the previous shot  
3. **Preview Selected Shot** - Play the currently selected shot
4. **Jump to Selected Shot** - Move camera to selected shot's start position

#### Programmatic Selection
```csharp
// Select a specific shot by index
cameraSystem.SelectShot(2); // Select shot at index 2

// Navigate through shots
cameraSystem.SelectNextShot();
cameraSystem.SelectPreviousShot();

// Preview or jump to selected shot
cameraSystem.PreviewSelectedShot();
cameraSystem.JumpToSelectedShot();
```

## Visual Feedback System

### Shot Highlighting
When `highlightSelectedShot` is enabled:
- **Selected shots** appear with brighter colors (lerped 50% toward white)
- **Selection indicator** - White wireframe outline around selected shot markers
- **Enlarged markers** - Selected shots use 0.5f radius vs 0.3f for normal shots
- **Size multipliers** - All gizmo elements are 1.3x larger for selected shots

### Playing Shot Indication
- **Currently playing shots** receive a yellow tint overlay
- Helps distinguish between selected vs currently executing shots

### Isolation Mode
With `showOnlySelectedShot` enabled:
- Only the selected shot is visible in Scene view
- Sequence path lines are hidden
- Perfect for focusing on individual shot setup

## Scene View Gizmos

### Enhanced Shot Markers
```
Normal Shot:     Blue wireframe sphere (0.3f radius)
Selected Shot:   Bright blue sphere (0.5f) + white outline (0.6f)
Playing Shot:    Yellow-tinted sphere
```

### Camera Frustum Visualization
- **Camera frustum wireframes** show field of view at shot positions
- **Look direction arrows** with arrowheads indicate camera orientation
- **Target connection lines** link cameras to their look-at targets
- All elements scale up 1.3x for selected shots

### Information Labels
Selected shots display detailed info labels:
```
SELECTED: [Shot Name]
Type: [Shot Type]
Duration: [Duration]s
```

## Workflow Examples

### Basic Shot Selection
1. Set `selectedShotIndex = 0` in inspector
2. Enable `highlightSelectedShot = true`
3. Use "Select Next Shot" context menu to navigate
4. Use "Preview Selected Shot" to test individual shots

### Focused Shot Setup  
1. Select the shot you want to configure: `SelectShot(3)`
2. Enable `showOnlySelectedShot = true`
3. Position camera transforms in Scene view
4. Use "Jump to Selected Shot" to test positioning
5. Disable `showOnlySelectedShot` when done

### Shot Comparison
1. Select first shot: `selectedShotIndex = 0`
2. Note the camera position and targets
3. Use "Select Next Shot" to compare with adjacent shots
4. Use "Jump to Selected Shot" to quickly test different positions

## Integration with Shot System

### Automatic Selection Updates
- Selection index is validated against shot count
- Invalid indices are automatically reset to -1
- Console logging shows selection changes (when `showShotProgressInConsole = true`)

### Runtime Compatibility
- Shot selection works alongside sequence playback
- Selection highlighting updates during runtime
- Preview commands respect `isPlayingSequence` state

## Inspector Layout

```
[Header("Shot Selection & Preview")]
public int selectedShotIndex = -1;           // Manual selection
public bool highlightSelectedShot = true;    // Visual highlighting
public bool showOnlySelectedShot = false;    // Isolation mode
```

## Advanced Usage

### Custom Selection Logic
```csharp
// Select shots by criteria
for (int i = 0; i < cameraSystem.shots.Count; i++)
{
    if (cameraSystem.shots[i].shotType == CameraShot.ShotType.Movement)
    {
        cameraSystem.SelectShot(i);
        break; // Select first movement shot
    }
}
```

### Selection Events
The system logs selection changes to console when enabled:
```
[CameraSystem] Selected Shot 3: 'Character Close-up'
[CameraSystem] Jumped to Shot 3: 'Character Close-up'
```

## Best Practices

### Shot Setup Workflow
1. **Plan** - Use selection to navigate and plan your shot sequence
2. **Configure** - Use isolation mode to focus on individual shot setup
3. **Test** - Preview individual shots before running full sequence
4. **Refine** - Jump between shots to compare and adjust

### Performance Considerations
- `showOnlySelectedShot` reduces gizmo overhead for complex sequences
- Visual highlighting adds minimal performance cost
- Consider disabling `highlightSelectedShot` for very large shot lists

## Troubleshooting

### Selection Not Working
- Ensure `selectedShotIndex` is within valid range (0 to shots.Count-1)
- Check that shots have valid `startPosition` transforms
- Verify `showGizmos = true` for visual feedback

### Gizmos Not Appearing
- Enable Scene view gizmo toggle
- Check individual gizmo flags: `showCameraFrustum`, `showLookDirections`
- Ensure shot transforms are not null

### Preview Not Playing
- Verify shot has valid start position
- Check that `isPlayingSequence = false` 
- Ensure Cinemachine camera is assigned

The shot selection system provides intuitive tools for managing complex camera sequences, making it easy to test, refine, and perfect individual shots within your cinematic sequences.
