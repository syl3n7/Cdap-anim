# Camera System Gizmo Visualization Guide

## Overview
The camera system now includes comprehensive visual gizmos to help you easily set up and understand your camera shots. These visualizations show exactly where the camera will be looking and how it will move through each shot.

## Gizmo Controls

### **Main Toggle Controls**
- **Show Gizmos**: Master toggle for all visualizations
- **Show Shot Paths**: Shows connections between shot positions
- **Show Camera Frustum**: Displays camera field of view wireframes
- **Show Look Directions**: Shows direction arrows where camera points
- **Show Target Connections**: Lines connecting camera to targets

### **Size Controls**
- **Frustum Size**: Adjusts the size of camera frustum visualization (default: 2)
- **Look Direction Length**: Controls length of direction arrows (default: 3)

## Visual Elements

### **Camera Position Markers**
- **Wireframe Spheres**: Start positions (0.3 unit radius)
- **Wireframe Cubes**: End positions (0.3 unit size)
- **Color Coded**: Each shot type has a unique color

### **Camera Frustum (Field of View)**
- **Wireframe Pyramid**: Shows camera's viewing volume
- **Near Plane**: Smaller rectangle close to camera
- **Far Plane**: Larger rectangle at distance
- **Connecting Lines**: Shows frustum edges
- **Semi-transparent**: Uses 60% opacity for clarity

### **Look Direction Arrows**
- **Main Ray**: Shows primary look direction
- **Arrowhead**: Four lines forming arrow tip
- **Length Adjustable**: Configurable via `lookDirectionLength`
- **Color Matched**: Uses shot type color

### **Target Connections**
- **Look Targets**: Green lines and spheres
- **Pan Targets**: Red lines and spheres  
- **Orbit Centers**: Magenta lines and circles
- **Dolly Points**: Cyan lines and small spheres

### **Labels and Text**
- **Shot Identifiers**: "Shot 1 Start", "Shot 2 End", etc.
- **Target Names**: "Look Target", "Pan Start", "Orbit Center"
- **Path Info**: "Dolly Path 5 points"
- **Color Coordinated**: Labels match element colors

## Shot Type Color Coding

| Shot Type | Color | RGB Values |
|-----------|-------|------------|
| Static | Blue | (0, 0, 1) |
| Movement | Green | (0, 1, 0) |
| Pan | Red | (1, 0, 0) |
| MovementWithPan | Yellow | (1, 1, 0) |
| OrbitAround | Magenta | (1, 0, 1) |
| DollyPath | Cyan | (0, 1, 1) |
| FreeRotation | Orange | (1, 0.5, 0) |
| LookAtOnly | White | (1, 1, 1) |
| CustomSequence | Gray | (0.5, 0.5, 0.5) |

## Smart Rotation Visualization

The gizmos intelligently show where the camera will actually look based on your rotation settings:

### **UseTransformRotation**
- Shows the exact rotation of the start/end transform objects

### **LookAtTarget**
- Calculates direction to look-at target
- Applies any rotation offsets you've specified
- Updates in real-time as you move targets

### **CustomEulerAngles**
- Shows rotation based on your custom Euler angles
- Respects local vs world space settings

### **FollowMovementDirection**
- Calculates direction based on movement path
- Shows where camera will look during travel

### **Pan Shots**
- Shows both start and end look targets
- Visualizes the pan arc between targets

## Setup Workflow with Gizmos

### **1. Basic Shot Setup**
1. Create empty GameObjects for camera positions
2. Position them in your scene
3. Assign to shot's startPosition (and endPosition if needed)
4. Observe wireframe markers and frustums

### **2. Look Direction Setup**
1. Enable "Show Look Directions" and "Show Camera Frustum"
2. Rotate your position transforms to aim the camera
3. Watch the direction arrows and frustums update
4. Fine-tune rotation until framing looks correct

### **3. Target Assignment**
1. Create target objects (characters, props, etc.)
2. Assign to appropriate shot fields (lookAtTarget, panTargets, etc.)
3. Observe green/red connection lines
4. Move targets to see real-time updates

### **4. Path Refinement**
1. For dolly shots, create multiple waypoint objects
2. Assign to dollyPath array
3. Watch cyan path lines and mini-frustums
4. Adjust waypoints for smooth camera movement

## Pro Tips

### **Scene View Navigation**
- Use Scene view to position cameras while seeing gizmos
- Gizmos update in real-time as you move objects
- Use different Scene view angles to check shot composition

### **Gizmo Layering**
- Turn off specific gizmo types to reduce visual clutter
- Use smaller frustum/arrow sizes for complex scenes
- Color coding helps identify shot types quickly

### **Performance**
- Gizmos only render in Scene view, not Game view
- No performance impact on built game
- Can be toggled off completely if needed

### **Debugging Shots**
- If a shot looks wrong, check its gizmo visualization
- Missing connections indicate unassigned targets
- Frustum direction shows exactly where camera points
- Use labels to identify elements quickly

## Common Setup Issues

### **Camera Not Looking Right**
- Check rotation type setting
- Verify look-at targets are assigned
- Look for rotation offset values
- Observe direction arrows and frustums

### **Missing Gizmos**
- Ensure "Show Gizmos" is enabled
- Check individual toggle settings
- Verify transforms are assigned to shots
- Make sure you're in Scene view

### **Confusing Visuals**
- Reduce frustum size for cleaner look
- Turn off specific gizmo types temporarily
- Use color coding to focus on specific shots
- Adjust arrow length for better visibility

This visualization system makes camera setup much more intuitive and helps you create professional camera movements with confidence!
