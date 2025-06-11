using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CameraShot
{
    [Header("Shot Identity")]
    public string shotName = "Shot 1";
    public float duration = 5f;
    [TextArea(2, 3)]
    public string shotDescription = "Describe what this shot does...";
    
    [Header("Shot Type")]
    public ShotType shotType = ShotType.Static;
    
    [Header("Camera Position")]
    public Transform startPosition;
    public Transform endPosition; // Only used for movement shots
    
    [Header("Camera Rotation")]
    public RotationType rotationType = RotationType.UseTransformRotation;
    
    [Header("Look At System")]
    public bool useLookAt = false;
    public Transform lookAtTarget;
    public bool continuousLookAt = true; // Keep looking during movement
    
    [Header("Pan System")]
    public Transform panStartTarget;
    public Transform panEndTarget;
    
    [Header("Orbit/Rotate Around Point")]
    public Transform orbitCenter;
    public Vector3 orbitAxis = Vector3.up;
    public float orbitAngle = 360f;
    public float orbitRadius = 5f;
    
    [Header("Dolly Track")]
    public List<Transform> dollyPath = new List<Transform>();
    public bool useDollyPath = false;
    
    [Header("Animation Curves")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Advanced Settings")]
    public bool smoothLookAt = true;
    public float lookAtSpeed = 2f;
    public bool waitAtEnd = false;
    public float waitDuration = 1f;
    
    public enum ShotType
    {
        Static,              // Camera stays in one position
        Movement,            // Camera moves from point A to B
        Pan,                 // Camera rotates to look between targets
        MovementWithPan,     // Camera moves while panning between targets
        OrbitAround,         // Camera orbits around a point
        DollyPath,           // Camera follows a path of points
        FreeRotation,        // Camera rotates freely (no movement)
        LookAtOnly,          // Camera only looks at target (no movement)
        CustomSequence       // Combine multiple behaviors
    }
    
    public enum RotationType
    {
        UseTransformRotation,    // Use the rotation of start/end transforms
        LookAtTarget,           // Always look at the lookAtTarget
        FreeRotation,           // Rotate by specified angles
        FollowMovementDirection, // Look in the direction of movement
        KeepCurrentRotation     // Don't change rotation
    }
    
    // Constructor
    public CameraShot()
    {
        shotName = "New Shot";
        duration = 5f;
        shotType = ShotType.Static;
        rotationType = RotationType.UseTransformRotation;
        useLookAt = false;
        continuousLookAt = true;
        movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        orbitAxis = Vector3.up;
        orbitAngle = 360f;
        orbitRadius = 5f;
        smoothLookAt = true;
        lookAtSpeed = 2f;
        waitAtEnd = false;
        waitDuration = 1f;
    }
}
