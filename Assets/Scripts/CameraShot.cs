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

    [Header("Events")]
    public UnityEngine.Events.UnityEvent StartWalk;

    [Header("Camera Position")]
    public Transform startPosition;
    
    [ConditionalHide("shotType", 1, 3, 6, 8)] // Movement, MovementWithPan, FreeRotation, CustomSequence
    public Transform endPosition;
    
    [Header("Camera Rotation")]
    [ConditionalHide("shotType", 1, 3, 5, 6, 8)] // Movement, MovementWithPan, DollyPath, FreeRotation, CustomSequence
    public RotationType rotationType = RotationType.UseTransformRotation;
    
    [ConditionalHide("rotationType", 2)] // FreeRotation
    public Vector3 customRotationStart = Vector3.zero;
    
    [ConditionalHide("rotationType", 2)] // FreeRotation
    public Vector3 customRotationEnd = Vector3.zero;
    
    [ConditionalHide("rotationType", 2)] // FreeRotation
    public bool useLocalRotation = false;
    
    [Header("Look At System")]
    [ConditionalHide("shotType", 0, 1, 4, 5, 7, 8)] // Static, Movement, OrbitAround, DollyPath, LookAtOnly, CustomSequence
    public bool useLookAt = false;
    
    [ConditionalHide("useLookAt", true)]
    public Transform lookAtTarget;
    
    [ConditionalHide("useLookAt", true)]
    public bool continuousLookAt = true;
    
    [Header("Pan System")]
    [ConditionalHide("shotType", 2, 3)] // Pan, MovementWithPan
    public Transform panStartTarget;
    
    [ConditionalHide("shotType", 2, 3)] // Pan, MovementWithPan
    public Transform panEndTarget;
    
    [Header("Orbit/Rotate Around Point")]
    [ConditionalHide("shotType", 4)] // OrbitAround
    public Transform orbitCenter;
    
    [ConditionalHide("shotType", 4)] // OrbitAround
    public Vector3 orbitAxis = Vector3.up;
    
    [ConditionalHide("shotType", 4)] // OrbitAround
    public float orbitAngle = 360f;
    
    [ConditionalHide("shotType", 4)] // OrbitAround
    public float orbitRadius = 5f;
    
    [Header("Dolly Track")]
    [ConditionalHide("shotType", 5)] // DollyPath
    public List<Transform> dollyPath = new List<Transform>();
    
    [ConditionalHide("shotType", 5)] // DollyPath
    public bool useDollyPath = false;
    
    [Header("Animation Curves")]
    [ConditionalHide("shotType", 1, 3, 4, 5, 8)] // Movement, MovementWithPan, OrbitAround, DollyPath, CustomSequence
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [ConditionalHide("shotType", 1, 5, 6)] // Movement, DollyPath, FreeRotation
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [ConditionalHide("shotType", 2, 3)] // Pan, MovementWithPan
    public AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Shot-Specific Rotation")]
    [ConditionalHide("shotType", 0)] // Static
    public Vector3 staticRotationOverride = Vector3.zero;
    
    [ConditionalHide("shotType", 0)] // Static
    public bool useStaticRotationOverride = false;
    
    [ConditionalHide("shotType", 2)] // Pan
    public Vector3 panRotationOffset = Vector3.zero;
    
    [ConditionalHide("shotType", 4)] // OrbitAround
    public Vector3 orbitLookOffset = Vector3.zero;
    
    [ConditionalHide("shotType", 4)] // OrbitAround
    public bool maintainOrbitHeight = true;
    
    [ConditionalHide("shotType", 7)] // LookAtOnly
    public Vector3 lookAtRotationOffset = Vector3.zero;
    [ConditionalHide("useLookAt", true)]
    public bool smoothLookAt = true;
    
    [ConditionalHide("smoothLookAt", true)]
    public float lookAtSpeed = 2f;
    
    public bool waitAtEnd = false;
    
    [ConditionalHide("waitAtEnd", true)]
    public float waitDuration = 1f;
    
    public enum ShotType
    {
        Static,              // 0 - Camera stays in one position
        Movement,            // 1 - Camera moves from point A to B
        Pan,                 // 2 - Camera rotates to look between targets
        MovementWithPan,     // 3 - Camera moves while panning between targets
        OrbitAround,         // 4 - Camera orbits around a point
        DollyPath,           // 5 - Camera follows a path of points
        FreeRotation,        // 6 - Camera rotates freely (no movement)
        LookAtOnly,          // 7 - Camera only looks at target (no movement)
        CustomSequence       // 8 - Combine multiple behaviors
    }
    
    public enum RotationType
    {
        UseTransformRotation,    // Use the rotation of start/end transforms
        LookAtTarget,           // Always look at the lookAtTarget
        FreeRotation,           // Rotate by specified angles (Euler)
        FollowMovementDirection, // Look in the direction of movement
        KeepCurrentRotation,     // Don't change rotation
        CustomEulerAngles       // Use custom Euler angles for precise control
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
        
        // Rotation overrides
        customRotationStart = Vector3.zero;
        customRotationEnd = Vector3.zero;
        useLocalRotation = false;
        staticRotationOverride = Vector3.zero;
        useStaticRotationOverride = false;
        panRotationOffset = Vector3.zero;
        orbitLookOffset = Vector3.zero;
        maintainOrbitHeight = true;
        lookAtRotationOffset = Vector3.zero;
    }
}
