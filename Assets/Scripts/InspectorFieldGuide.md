# Unity Camera System - Inspector Field Guide

## Dynamic Inspector Fields

The camera system uses conditional attributes to show only relevant fields based on the selected shot type. Here's what fields appear for each shot type:

## Shot Type Field Visibility

### Static (0)
- **Always Visible:** Shot Identity, Shot Type, Start Position, Wait settings
- **Conditionally Visible:** Look At System (useLookAt checkbox available)

### Movement (1) 
- **Always Visible:** Shot Identity, Shot Type, Start Position, End Position
- **Conditionally Visible:** 
  - Camera Rotation (rotationType)
  - Look At System (useLookAt checkbox)
  - Movement Curve
  - Rotation Curve

### Pan (2)
- **Always Visible:** Shot Identity, Shot Type, Start Position
- **Conditionally Visible:**
  - Pan System (panStartTarget, panEndTarget)
  - Pan Curve

### Movement With Pan (3)
- **Always Visible:** Shot Identity, Shot Type, Start Position, End Position
- **Conditionally Visible:**
  - Camera Rotation (rotationType)
  - Pan System (panStartTarget, panEndTarget)
  - Movement Curve
  - Pan Curve

### Orbit Around (4)
- **Always Visible:** Shot Identity, Shot Type, Start Position
- **Conditionally Visible:**
  - Look At System (useLookAt checkbox)
  - Orbit/Rotate Around Point (orbitCenter, orbitAxis, orbitAngle, orbitRadius)
  - Movement Curve

### Dolly Path (5)
- **Always Visible:** Shot Identity, Shot Type, Start Position
- **Conditionally Visible:**
  - Camera Rotation (rotationType)
  - Look At System (useLookAt checkbox)
  - Dolly Track (dollyPath, useDollyPath)
  - Movement Curve
  - Rotation Curve

### Free Rotation (6)
- **Always Visible:** Shot Identity, Shot Type, Start Position, End Position
- **Conditionally Visible:**
  - Camera Rotation (rotationType)
  - Rotation Curve

### Look At Only (7)
- **Always Visible:** Shot Identity, Shot Type, Start Position
- **Conditionally Visible:**
  - Look At System (useLookAt checkbox available)

### Custom Sequence (8)
- **Always Visible:** Shot Identity, Shot Type, Start Position, End Position
- **Conditionally Visible:**
  - Camera Rotation (rotationType)
  - Look At System (useLookAt checkbox)
  - Movement Curve

## Additional Conditional Fields

### When useLookAt = true
- lookAtTarget
- continuousLookAt
- smoothLookAt
- lookAtSpeed (when smoothLookAt = true)

### When waitAtEnd = true
- waitDuration

## Technical Implementation

The system uses:
1. `ConditionalHideAttribute.cs` - Custom PropertyAttribute
2. `ConditionalHidePropertyDrawer.cs` - Unity Editor PropertyDrawer
3. Enum index-based conditional logic

Each shot type is assigned an index (0-8) which is used in the conditional attributes to determine field visibility.
