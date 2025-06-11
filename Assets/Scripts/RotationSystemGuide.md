# Enhanced Camera Rotation System

## Overview
The camera system now provides granular control over camera rotation for each shot type. You can specify custom rotations, offsets, and behaviors to achieve precise camera movements.

## Global Rotation Types

### 1. UseTransformRotation
- **Description**: Uses the rotation of start/end transform objects
- **Best for**: Pre-positioned camera setups
- **Usage**: Position your camera transforms with desired rotations

### 2. LookAtTarget
- **Description**: Camera always looks at the specified target
- **Best for**: Following subjects, character tracking
- **Options**: 
  - `smoothLookAt`: Smooth rotation transition
  - `lookAtSpeed`: Speed of smooth rotation

### 3. FreeRotation / CustomEulerAngles
- **Description**: Rotate using specified Euler angles
- **Best for**: Precise rotation control, custom camera movements
- **Fields**:
  - `customRotationStart`: Starting Euler angles (X, Y, Z)
  - `customRotationEnd`: Ending Euler angles (X, Y, Z)
  - `useLocalRotation`: Use local vs world space rotation

### 4. FollowMovementDirection
- **Description**: Camera looks in the direction of movement
- **Best for**: Travel shots, following paths
- **Usage**: Automatically calculated from start/end positions

### 5. KeepCurrentRotation
- **Description**: Maintains current camera rotation
- **Best for**: Position-only movements

## Shot-Specific Rotation Controls

### Static Shot Rotation
```csharp
useStaticRotationOverride = true;
staticRotationOverride = new Vector3(15f, 45f, 0f); // Tilt down 15°, rotate right 45°
```

### Pan Shot Rotation
```csharp
panRotationOffset = new Vector3(10f, 0f, 0f); // Slight downward tilt while panning
```

### Orbit Shot Rotation
```csharp
orbitLookOffset = new Vector3(-10f, 0f, 0f);    // Look down while orbiting
maintainOrbitHeight = false;                    // Allow height variation during orbit
```

### Look At Only Rotation
```csharp
lookAtRotationOffset = new Vector3(0f, 15f, 0f); // Offset look direction slightly right
```

## Practical Examples

### Example 1: Cinematic Character Introduction
```csharp
// Shot 1: Static shot with custom angle
shotType = ShotType.Static;
useStaticRotationOverride = true;
staticRotationOverride = new Vector3(20f, 135f, 0f); // Dramatic angle
duration = 3f;

// Shot 2: Orbit around character
shotType = ShotType.OrbitAround;
orbitAngle = 180f;
orbitLookOffset = new Vector3(-5f, 0f, 0f); // Slight downward look
maintainOrbitHeight = true;
```

### Example 3: Custom Rotation Sequence
```csharp
shotType = ShotType.FreeRotation;
rotationType = RotationType.CustomEulerAngles;
customRotationStart = new Vector3(0f, 0f, 0f);
customRotationEnd = new Vector3(0f, 360f, 0f); // Full Y rotation
useLocalRotation = false;
```

### Example 4: Follow Shot with Offset
```csharp
shotType = ShotType.LookAtOnly;
lookAtTarget = characterTransform;
lookAtRotationOffset = new Vector3(10f, -15f, 0f); // Look slightly down and left
continuousLookAt = true;
smoothLookAt = true;
lookAtSpeed = 3f;
```

## Animation Curves for Rotation

All rotation types respect the `rotationCurve` setting:
- **Linear**: Constant rotation speed
- **EaseIn**: Slow start, fast finish
- **EaseOut**: Fast start, slow finish  
- **EaseInOut**: Smooth acceleration and deceleration
- **Custom**: Define your own timing curve

## Tips for Best Results

### 1. Coordinate Systems
- **World Space**: Rotation relative to world axes (most common)
- **Local Space**: Rotation relative to camera's current orientation

### 2. Euler Angle Ranges
- **X (Pitch)**: -90° to 90° (looking up/down)
- **Y (Yaw)**: 0° to 360° (looking left/right)
- **Z (Roll)**: ±180° (camera tilt)

### 3. Smooth Transitions
- Use `smoothLookAt = true` for natural character tracking
- Adjust `lookAtSpeed` for different feels (1-5 typical range)
- Use animation curves for custom timing

### 4. Combining Rotations
- Start with basic rotation type
- Add shot-specific offsets for fine-tuning
- Use look-at targets for subject focus

### 5. Testing Workflow
1. Set up basic shot with standard rotation
2. Add offsets incrementally
3. Test with different animation curves
4. Use gizmos to visualize paths and targets

## Inspector Field Visibility

The inspector now dynamically shows rotation fields based on:
- **Shot Type**: Only relevant rotation options appear
- **Rotation Type**: Custom angle fields appear when needed
- **Boolean Toggles**: Offset fields appear when enabled

This keeps the interface clean while providing full control when needed.
