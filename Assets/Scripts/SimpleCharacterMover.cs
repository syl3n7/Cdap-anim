using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleCharacterMover : MonoBehaviour
{
    public enum GizmoStyle
    {
        Simple,     // Basic wireframe
        Modern,     // Enhanced with gradients and effects
        Minimal,    // Clean, minimal style
        Debug       // Information-heavy for debugging
    }
    
    public enum MovementMode
    {
        ConstantSpeed,      // Uniform speed throughout
        EaseInOut,          // Smooth acceleration/deceleration
        Accelerate,         // Continuous acceleration
        Decelerate,         // Continuous deceleration
        CustomCurve        // Use animation curve
    }
    
    [Header("Movement Settings")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 5f;
    public bool autoStartOnPlay = true;
    public bool loopMovement = false;
    public float loopDelay = 0.5f;
    
    [Header("Multi-Point Path")]
    public bool useMultiPointPath = false;
    public List<Transform> pathPoints = new List<Transform>();
    public bool loopPath = true;
    
    [Header("Animation Curve")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Rotation Settings")]
    public bool rotateTowardsTarget = true;
    public float rotationSpeed = 10f;
    public bool useCustomRotation = false;
    public AnimationCurve rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("Animation Events")]
    public bool enableMovementEvents = false;
    public UnityEngine.Events.UnityEvent OnMovementStart;
    public UnityEngine.Events.UnityEvent OnMovementComplete;
    public UnityEngine.Events.UnityEvent OnReachPointA;
    public UnityEngine.Events.UnityEvent OnReachPointB;
    
    [Header("Camera Integration")]
    public bool notifyCameraSystem = false;
    public CameraSystem cameraSystem;
    public int triggerShotOnReachA = -1;
    public int triggerShotOnReachB = -1;
    
    [Header("Advanced Movement Options")]
    public bool pauseOnReachPoint = false;
    public float pauseDuration = 1f;
    public bool smoothSpeedTransitions = true;
    public float accelerationTime = 0.5f;
    public float decelerationTime = 0.5f;
    public bool predictiveRotation = false;
    public float rotationPredictionDistance = 2f;
    
    [Header("Movement Constraints")]
    public bool constrainToGround = false;
    public LayerMask groundLayer = 1;
    public float groundOffset = 0.1f;
    public bool avoidObstacles = false;
    public LayerMask obstacleLayer = 1;
    public float obstacleDetectionDistance = 1f;
    
    [Header("Gizmo Visualization")]
    public bool showGizmos = true;
    public bool showPath = true;
    public bool showMovementProgress = true;
    public bool showRotationIndicator = true;
    public bool showSpeedVisualization = true;
    public bool showDistanceLabels = true;
    public bool showTimingInfo = true;
    public bool showVelocityVector = true;
    public bool showAccelerationZones = true;
    public bool showGroundProjection = false;
    public bool showObstacleDetection = false;
    public bool showWaypoints = true;
    public bool showPathNumbers = true;
    public bool showCollisionBounds = false;
    public bool showInfluenceZones = false;
    public bool showPathPrediction = false;
    
    [Header("Gizmo Colors & Style")]
    public Color pathColor = Color.yellow;
    public Color pointAColor = Color.green;
    public Color pointBColor = Color.red;
    public Color progressColor = Color.cyan;
    public Color velocityColor = Color.magenta;
    public Color accelerationColor = Color.orange;
    public Color decelerationColor = Color.blue;
    public Color groundProjectionColor = Color.gray;
    public Color obstacleDetectionColor = Color.red;
    public Color waypointColor = Color.white;
    public Color influenceZoneColor = Color.yellow;
    public Color collisionBoundsColor = Color.red;
    public Color predictionColor = Color.green;
    public float gizmoSize = 0.5f;
    public float pathLineWidth = 3f;
    public float gizmoAlpha = 0.8f;
    public GizmoStyle gizmoStyle = GizmoStyle.Modern;
    
    [Header("Movement Behavior")]
    public MovementMode movementMode = MovementMode.EaseInOut;
    
    // Runtime variables
    private bool isMoving = false;
    private Coroutine movementCoroutine;
    private Vector3 currentTarget;
    private bool movingToB = true; // True when moving A->B, False when moving B->A
    private int currentPathIndex = 0;
    private float currentMovementProgress = 0f;
    private Vector3 movementStartPosition;
    private float totalMovementDistance = 0f;
    
    // Runtime variables for enhanced visualization
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;
    private float currentSpeed = 0f;
    private float maxSpeed = 0f;
    private List<Vector3> recentPositions = new List<Vector3>();
    private int maxTrailLength = 20;
    private float lastGizmoUpdateTime = 0f;
    private Vector3 predictedPosition = Vector3.zero;
    private float pathCompletionPercentage = 0f;
    private Dictionary<int, float> waypointTimings = new Dictionary<int, float>();

    void Start()
    {
        // Initialize path points if using multi-point path
        if (useMultiPointPath && pathPoints.Count == 0 && pointA != null && pointB != null)
        {
            pathPoints.Add(pointA);
            pathPoints.Add(pointB);
        }
        
        if (autoStartOnPlay)
        {
            if (useMultiPointPath && pathPoints.Count > 1)
            {
                StartPathMovement();
            }
            else if (pointA != null && pointB != null)
            {
                // Start at point A
                transform.position = pointA.position;
                MoveToPointB();
            }
        }
    }
    
    #region Public Controls
    
    [ContextMenu("Move to Point A")]
    public void MoveToPointA()
    {
        if (pointA == null)
        {
            Debug.LogWarning("[CharacterMover] Point A is not assigned!");
            return;
        }
        
        StopMovement();
        currentTarget = pointA.position;
        movingToB = false;
        movementCoroutine = StartCoroutine(MoveToTarget(currentTarget));
    }
    
    [ContextMenu("Move to Point B")]
    public void MoveToPointB()
    {
        if (pointB == null)
        {
            Debug.LogWarning("[CharacterMover] Point B is not assigned!");
            return;
        }
        
        StopMovement();
        currentTarget = pointB.position;
        movingToB = true;
        movementCoroutine = StartCoroutine(MoveToTarget(currentTarget));
    }
    
    [ContextMenu("Start Path Movement")]
    public void StartPathMovement()
    {
        if (!useMultiPointPath || pathPoints.Count < 2)
        {
            Debug.LogWarning("[CharacterMover] Need at least 2 path points for path movement!");
            return;
        }
        
        StopMovement();
        currentPathIndex = 0;
        transform.position = pathPoints[0].position;
        movementCoroutine = StartCoroutine(MoveAlongPath());
    }
    
    [ContextMenu("Stop Movement")]
    public void StopMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
        isMoving = false;
        currentMovementProgress = 0f;
    }
    
    [ContextMenu("Toggle Movement")]
    public void ToggleMovement()
    {
        if (isMoving)
        {
            StopMovement();
        }
        else
        {
            // Move to the opposite point
            if (movingToB || Vector3.Distance(transform.position, pointA.position) < Vector3.Distance(transform.position, pointB.position))
            {
                MoveToPointB();
            }
            else
            {
                MoveToPointA();
            }
        }
    }
    
    public void MoveToCustomPoint(Vector3 targetPosition, float customSpeed = -1f)
    {
        StopMovement();
        currentTarget = targetPosition;
        float speed = customSpeed > 0 ? customSpeed : moveSpeed;
        movementCoroutine = StartCoroutine(MoveToTarget(targetPosition, speed));
    }
    
    #endregion
    
    #region Movement Implementation
    
    private IEnumerator MoveToTarget(Vector3 target, float speed = -1f)
    {
        if (speed < 0) speed = moveSpeed;
        
        isMoving = true;
        movementStartPosition = transform.position;
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, target);
        totalMovementDistance = distance;
        float duration = distance / speed;
        
        Debug.Log($"[CharacterMover] Moving to {(movingToB ? "Point B" : "Point A")} - Distance: {distance:F2}m, Duration: {duration:F2}s");
        
        // Trigger movement start event
        if (enableMovementEvents)
        {
            OnMovementStart?.Invoke();
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = movementCurve.Evaluate(elapsed / duration);
            currentMovementProgress = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, target, t);
            
            // Handle rotation
            HandleRotation(startPosition, target, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position
        transform.position = target;
        currentMovementProgress = 1f;
        isMoving = false;
        
        Debug.Log($"[CharacterMover] Reached {(movingToB ? "Point B" : "Point A")}");
        
        // Trigger completion events
        if (enableMovementEvents)
        {
            OnMovementComplete?.Invoke();
            
            if (movingToB)
            {
                OnReachPointB?.Invoke();
                TriggerCameraShot(triggerShotOnReachB);
            }
            else
            {
                OnReachPointA?.Invoke();
                TriggerCameraShot(triggerShotOnReachA);
            }
        }
        
        // Handle looping
        if (loopMovement)
        {
            yield return new WaitForSeconds(loopDelay);
            
            if (movingToB)
            {
                MoveToPointA();
            }
            else
            {
                MoveToPointB();
            }
        }
    }
    
    private IEnumerator MoveAlongPath()
    {
        isMoving = true;
        
        if (enableMovementEvents)
        {
            OnMovementStart?.Invoke();
        }
        
        do
        {
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                currentPathIndex = i;
                Vector3 startPos = pathPoints[i].position;
                Vector3 endPos = pathPoints[i + 1].position;
                
                float distance = Vector3.Distance(startPos, endPos);
                float duration = distance / moveSpeed;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    float t = movementCurve.Evaluate(elapsed / duration);
                    currentMovementProgress = (float)(i + t) / (pathPoints.Count - 1);
                    
                    transform.position = Vector3.Lerp(startPos, endPos, t);
                    HandleRotation(startPos, endPos, t);
                    
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                transform.position = endPos;
                yield return new WaitForSeconds(loopDelay * 0.5f); // Small pause at each point
            }
            
            if (loopPath)
            {
                yield return new WaitForSeconds(loopDelay);
            }
            
        } while (loopPath && isMoving);
        
        isMoving = false;
        currentMovementProgress = 1f;
        
        if (enableMovementEvents)
        {
            OnMovementComplete?.Invoke();
        }
    }
    
    private void HandleRotation(Vector3 startPos, Vector3 endPos, float t)
    {
        if (rotateTowardsTarget)
        {
            Vector3 direction = (endPos - startPos).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                if (useCustomRotation)
                {
                    float rotT = rotationCurve.Evaluate(t);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotT * Time.deltaTime * rotationSpeed);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }
        }
    }
    
    private void TriggerCameraShot(int shotIndex)
    {
        if (notifyCameraSystem && cameraSystem != null && shotIndex >= 0)
        {
            cameraSystem.PlaySpecificShot(shotIndex);
            Debug.Log($"[CharacterMover] Triggered camera shot {shotIndex + 1}");
        }
    }
    
    #endregion
    
    #region Enhanced Gizmos
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        UpdateRuntimeVisualizationData();
        
        if (useMultiPointPath)
        {
            DrawEnhancedMultiPointPath();
        }
        else
        {
            DrawEnhancedTwoPointPath();
        }
        
        // Draw character state and movement info
        DrawCharacterVisualization();
        
        // Draw movement progress and timing
        if (showMovementProgress && isMoving)
        {
            DrawAdvancedMovementProgress();
        }
        
        // Draw velocity and speed information
        if (showVelocityVector || showSpeedVisualization)
        {
            DrawVelocityVisualization();
        }
        
        // Draw acceleration/deceleration zones
        if (showAccelerationZones)
        {
            DrawAccelerationZones();
        }
        
        // Draw ground projection
        if (showGroundProjection && constrainToGround)
        {
            DrawGroundProjection();
        }
        
        // Draw obstacle detection
        if (showObstacleDetection && avoidObstacles)
        {
            DrawObstacleDetection();
        }
        
        // Draw waypoint information
        if (showWaypoints)
        {
            DrawWaypointInformation();
        }
        
        // Draw collision bounds
        if (showCollisionBounds)
        {
            DrawCollisionBounds();
        }
        
        // Draw influence zones
        if (showInfluenceZones)
        {
            DrawInfluenceZones();
        }
        
        // Draw path prediction
        if (showPathPrediction && isMoving)
        {
            DrawPathPrediction();
        }
        
        // Draw distance and timing labels
        if (showDistanceLabels || showTimingInfo)
        {
            DrawInformationLabels();
        }
        
        // Draw interactive gizmo elements
        if (Application.isPlaying && gizmoStyle != GizmoStyle.Minimal)
        {
            DrawInteractiveGizmoElements();
        }
    }
    
    private void UpdateRuntimeVisualizationData()
    {
        if (!Application.isPlaying) return;
        
        float currentTime = Time.time;
        
        // Update velocity calculation
        Vector3 currentPos = transform.position;
        if (lastPosition != Vector3.zero && Time.deltaTime > 0)
        {
            currentVelocity = (currentPos - lastPosition) / Time.deltaTime;
            currentSpeed = currentVelocity.magnitude;
            maxSpeed = Mathf.Max(maxSpeed, currentSpeed);
        }
        lastPosition = currentPos;
        
        // Update position trail
        if (recentPositions.Count == 0 || Vector3.Distance(currentPos, recentPositions[recentPositions.Count - 1]) > 0.1f)
        {
            recentPositions.Add(currentPos);
            if (recentPositions.Count > maxTrailLength)
            {
                recentPositions.RemoveAt(0);
            }
        }
        
        // Update timing data for waypoints
        if (useMultiPointPath && pathPoints != null && isMoving)
        {
            for (int i = 0; i < pathPoints.Count; i++)
            {
                if (pathPoints[i] != null)
                {
                    float distanceToWaypoint = Vector3.Distance(currentPos, pathPoints[i].position);
                    if (distanceToWaypoint < gizmoSize && !waypointTimings.ContainsKey(i))
                    {
                        waypointTimings[i] = currentTime;
                    }
                }
            }
        }
        
        // Update path completion percentage
        if (isMoving)
        {
            if (useMultiPointPath && pathPoints != null && pathPoints.Count > 1)
            {
                float totalDistance = GetTotalPathDistance();
                float completedDistance = 0f;
                
                // Calculate completed segments
                for (int i = 0; i < currentPathIndex && i < pathPoints.Count - 1; i++)
                {
                    if (pathPoints[i] != null && pathPoints[i + 1] != null)
                    {
                        completedDistance += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
                    }
                }
                
                // Add progress on current segment
                if (currentPathIndex < pathPoints.Count - 1 && pathPoints[currentPathIndex] != null && pathPoints[currentPathIndex + 1] != null)
                {
                    float segmentLength = Vector3.Distance(pathPoints[currentPathIndex].position, pathPoints[currentPathIndex + 1].position);
                    float segmentProgress = Vector3.Distance(pathPoints[currentPathIndex].position, currentPos);
                    completedDistance += Mathf.Min(segmentProgress, segmentLength);
                }
                
                pathCompletionPercentage = totalDistance > 0 ? (completedDistance / totalDistance) * 100f : 0f;
            }
            else if (pointA != null && pointB != null)
            {
                Vector3 start = movingToB ? pointA.position : pointB.position;
                Vector3 end = movingToB ? pointB.position : pointA.position;
                float totalDistance = Vector3.Distance(start, end);
                float completedDistance = Vector3.Distance(start, currentPos);
                pathCompletionPercentage = totalDistance > 0 ? (completedDistance / totalDistance) * 100f : 0f;
            }
        }
        
        lastGizmoUpdateTime = currentTime;
    }
    
    private void DrawEnhancedTwoPointPath()
    {
        if (pointA == null || pointB == null) return;
        
        Vector3 posA = pointA.position;
        Vector3 posB = pointB.position;
        Vector3 direction = (posB - posA).normalized;
        float distance = Vector3.Distance(posA, posB);
        
        // Draw enhanced point markers
        DrawEnhancedPointMarker(posA, pointAColor, "Point A (Start)", true);
        DrawEnhancedPointMarker(posB, pointBColor, "Point B (End)", false);
        
        // Draw main path with style variations
        if (showPath)
        {
            DrawStyledPath(posA, posB, pathColor, distance);
        }
        
        // Draw direction indicators
        DrawDirectionIndicators(posA, posB, direction, distance);
        
        // Draw movement zones (acceleration/constant/deceleration)
        if (showAccelerationZones)
        {
            DrawMovementZones(posA, posB, distance);
        }
    }
    
    private void DrawEnhancedMultiPointPath()
    {
        if (pathPoints.Count < 2) return;
        
        float totalDistance = 0f;
        
        // Calculate total path distance
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (pathPoints[i] != null && pathPoints[i + 1] != null)
            {
                totalDistance += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }
        
        // Draw path segments with progress indication
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (pathPoints[i] == null || pathPoints[i + 1] == null) continue;
            
            Vector3 start = pathPoints[i].position;
            Vector3 end = pathPoints[i + 1].position;
            float segmentDistance = Vector3.Distance(start, end);
            
            // Color based on segment progress
            Color segmentColor = pathColor;
            if (isMoving && i == currentPathIndex)
            {
                segmentColor = Color.Lerp(pathColor, progressColor, 0.7f);
            }
            else if (isMoving && i < currentPathIndex)
            {
                segmentColor = progressColor;
            }
            
            DrawStyledPath(start, end, segmentColor, segmentDistance);
            DrawEnhancedPointMarker(start, Color.Lerp(pointAColor, pointBColor, (float)i / (pathPoints.Count - 1)), 
                                  $"Point {i + 1}", i == 0);
        }
        
        // Draw final point
        if (pathPoints[pathPoints.Count - 1] != null)
        {
            DrawEnhancedPointMarker(pathPoints[pathPoints.Count - 1].position, pointBColor, 
                                  $"Point {pathPoints.Count}", false);
        }
        
        // Draw loop connection if enabled
        if (loopPath && pathPoints.Count > 2 && pathPoints[0] != null && pathPoints[pathPoints.Count - 1] != null)
        {
            Vector3 start = pathPoints[pathPoints.Count - 1].position;
            Vector3 end = pathPoints[0].position;
            DrawStyledPath(start, end, pathColor * 0.7f, Vector3.Distance(start, end), true);
        }
    }
    
    private void DrawEnhancedPointMarker(Vector3 position, Color color, string label, bool isStart)
    {
        Gizmos.color = color;
        
        switch (gizmoStyle)
        {
            case GizmoStyle.Simple:
                Gizmos.DrawWireSphere(position, gizmoSize);
                break;
                
            case GizmoStyle.Modern:
                // Outer glow effect
                Gizmos.color = color * 0.3f;
                Gizmos.DrawSphere(position, gizmoSize * 1.5f);
                
                // Main marker
                Gizmos.color = color;
                Gizmos.DrawWireSphere(position, gizmoSize);
                Gizmos.DrawSphere(position, gizmoSize * 0.7f);
                
                // Direction indicator for start point
                if (isStart && pointB != null)
                {
                    Vector3 direction = (pointB.position - position).normalized;
                    DrawArrowHead(position + direction * gizmoSize * 2f, direction, gizmoSize * 0.5f, color);
                }
                break;
                
            case GizmoStyle.Minimal:
                Gizmos.DrawWireCube(position, Vector3.one * gizmoSize);
                break;
                
            case GizmoStyle.Debug:
                Gizmos.DrawWireSphere(position, gizmoSize);
                Gizmos.DrawWireCube(position + Vector3.up * gizmoSize * 2f, Vector3.one * gizmoSize * 0.5f);
                break;
        }
        
        // Labels
        #if UNITY_EDITOR
        if (gizmoStyle == GizmoStyle.Debug || showDistanceLabels)
        {
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(position + Vector3.up * (gizmoSize + 0.5f), label);
        }
        #endif
    }
    
    private void DrawStyledPath(Vector3 start, Vector3 end, Color color, float distance, bool isDashed = false)
    {
        switch (gizmoStyle)
        {
            case GizmoStyle.Simple:
                Gizmos.color = color;
                Gizmos.DrawLine(start, end);
                break;
                
            case GizmoStyle.Modern:
                #if UNITY_EDITOR
                UnityEditor.Handles.color = color;
                if (isDashed)
                {
                    UnityEditor.Handles.DrawDottedLine(start, end, 5f);
                }
                else
                {
                    UnityEditor.Handles.DrawBezier(start, end, start, end, color, null, pathLineWidth);
                }
                #endif
                break;
                
            case GizmoStyle.Minimal:
                Gizmos.color = color * 0.8f;
                Gizmos.DrawLine(start, end);
                break;
                
            case GizmoStyle.Debug:
                Gizmos.color = color;
                Gizmos.DrawLine(start, end);
                
                // Draw distance markers along the path
                Vector3 direction = (end - start).normalized;
                int markers = Mathf.Max(1, Mathf.FloorToInt(distance / 2f));
                for (int i = 1; i < markers; i++)
                {
                    float t = (float)i / markers;
                    Vector3 markerPos = Vector3.Lerp(start, end, t);
                    Gizmos.color = color * 0.5f;
                    Gizmos.DrawWireCube(markerPos, Vector3.one * 0.1f);
                }
                break;
        }
    }
    
    private void DrawDirectionIndicators(Vector3 start, Vector3 end, Vector3 direction, float distance)
    {
        if (gizmoStyle == GizmoStyle.Minimal) return;
        
        Gizmos.color = pathColor * 0.8f;
        
        // Draw arrows along the path
        int arrowCount = Mathf.Max(1, Mathf.FloorToInt(distance / 3f));
        for (int i = 0; i < arrowCount; i++)
        {
            float t = (float)(i + 0.5f) / arrowCount;
            Vector3 arrowPos = Vector3.Lerp(start, end, t);
            DrawArrowHead(arrowPos, direction, gizmoSize * 0.3f, pathColor * 0.6f);
        }
    }
    
    private void DrawMovementZones(Vector3 start, Vector3 end, float distance)
    {
        if (movementMode == MovementMode.ConstantSpeed) return;
        
        Vector3 direction = (end - start).normalized;
        float accelDist = accelerationTime * moveSpeed * 0.5f; // Approximate
        float decelDist = decelerationTime * moveSpeed * 0.5f;
        
        // Acceleration zone
        if (accelDist > 0 && accelDist < distance * 0.4f)
        {
            Vector3 accelEnd = start + direction * accelDist;
            Gizmos.color = accelerationColor * 0.3f;
            #if UNITY_EDITOR
            UnityEditor.Handles.color = accelerationColor * 0.2f;
            UnityEditor.Handles.DrawSolidArc(start, Vector3.up, direction, 360f, accelDist);
            #endif
        }
        
        // Deceleration zone
        if (decelDist > 0 && decelDist < distance * 0.4f)
        {
            Vector3 decelStart = end - direction * decelDist;
            Gizmos.color = decelerationColor * 0.3f;
            #if UNITY_EDITOR
            UnityEditor.Handles.color = decelerationColor * 0.2f;
            UnityEditor.Handles.DrawSolidArc(end, Vector3.up, -direction, 360f, decelDist);
            #endif
        }
    }
    
    private void DrawCharacterVisualization()
    {
        Vector3 charPos = transform.position;
        
        // Character position indicator
        Gizmos.color = Color.white;
        switch (gizmoStyle)
        {
            case GizmoStyle.Modern:
                Gizmos.color = isMoving ? progressColor : Color.white;
                Gizmos.DrawWireSphere(charPos, gizmoSize * 0.8f);
                if (isMoving)
                {
                    Gizmos.color = progressColor * 0.3f;
                    Gizmos.DrawSphere(charPos, gizmoSize * 0.6f);
                }
                break;
            default:
                Gizmos.DrawWireCube(charPos, Vector3.one * gizmoSize * 0.6f);
                break;
        }
        
        // Rotation indicator
        if (showRotationIndicator && rotateTowardsTarget)
        {
            Vector3 forward = transform.forward;
            Gizmos.color = velocityColor;
            Gizmos.DrawRay(charPos, forward * gizmoSize * 2f);
            DrawArrowHead(charPos + forward * gizmoSize * 2f, forward, gizmoSize * 0.2f, velocityColor);
        }
        
        // Movement trail
        if (Application.isPlaying && recentPositions.Count > 1)
        {
            for (int i = 1; i < recentPositions.Count; i++)
            {
                float alpha = (float)i / recentPositions.Count;
                Gizmos.color = Color.Lerp(Color.clear, progressColor, alpha);
                Gizmos.DrawLine(recentPositions[i - 1], recentPositions[i]);
            }
        }
    }
    
    private void DrawAdvancedMovementProgress()
    {
        if (!isMoving) return;
        
        Vector3 start, end;
        
        if (useMultiPointPath && currentPathIndex < pathPoints.Count - 1)
        {
            start = pathPoints[currentPathIndex].position;
            end = pathPoints[currentPathIndex + 1].position;
        }
        else if (pointA != null && pointB != null)
        {
            start = movingToB ? pointA.position : pointB.position;
            end = movingToB ? pointB.position : pointA.position;
        }
        else return;
        
        Vector3 currentPos = Vector3.Lerp(start, end, currentMovementProgress);
        Vector3 direction = (end - start).normalized;
        
        // Progress indicator
        Gizmos.color = progressColor;
        Gizmos.DrawSphere(currentPos, gizmoSize * 0.4f);
        
        // Progress line
        Gizmos.color = progressColor * 0.6f;
        Gizmos.DrawLine(start, currentPos);
        
        // Remaining path
        Gizmos.color = pathColor * 0.3f;
        Gizmos.DrawLine(currentPos, end);
        
        // Progress percentage
        #if UNITY_EDITOR
        if (showTimingInfo)
        {
            UnityEditor.Handles.color = progressColor;
            string progressText = $"{(currentMovementProgress * 100f):F1}%";
            if (gizmoStyle == GizmoStyle.Debug)
            {
                progressText += $"\nSpeed: {currentSpeed:F1}m/s";
                progressText += $"\nDistance: {Vector3.Distance(currentPos, end):F1}m";
            }
            UnityEditor.Handles.Label(currentPos + Vector3.up * gizmoSize, progressText);
        }
        #endif
    }
    
    private void DrawVelocityVisualization()
    {
        if (!Application.isPlaying || !isMoving) return;
        
        Vector3 charPos = transform.position;
        
        // Current velocity vector
        if (showVelocityVector && currentVelocity.magnitude > 0.1f)
        {
            Gizmos.color = velocityColor;
            Vector3 velEnd = charPos + currentVelocity;
            Gizmos.DrawLine(charPos, velEnd);
            DrawArrowHead(velEnd, currentVelocity.normalized, gizmoSize * 0.3f, velocityColor);
            
            #if UNITY_EDITOR
            if (gizmoStyle == GizmoStyle.Debug)
            {
                UnityEditor.Handles.color = velocityColor;
                UnityEditor.Handles.Label(velEnd, $"Vel: {currentVelocity.magnitude:F1}m/s");
            }
            #endif
        }
        
        // Speed visualization
        if (showSpeedVisualization && maxSpeed > 0)
        {
            float speedRatio = currentSpeed / maxSpeed;
            Color speedColor = Color.Lerp(Color.green, Color.red, speedRatio);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = speedColor;
            UnityEditor.Handles.DrawSolidArc(charPos, Vector3.up, Vector3.forward, 360f * speedRatio, gizmoSize * 0.3f);
            #endif
        }
    }
    
    private void DrawAccelerationZones()
    {
        // Implementation for showing acceleration/deceleration zones
        // Already implemented in DrawMovementZones
    }
    
    private void DrawGroundProjection()
    {
        Vector3 charPos = transform.position;
        
        // Cast ray to ground
        if (Physics.Raycast(charPos, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            Gizmos.color = groundProjectionColor;
            Gizmos.DrawLine(charPos, hit.point);
            Gizmos.DrawWireCube(hit.point, Vector3.one * gizmoSize * 0.3f);
            
            #if UNITY_EDITOR
            if (gizmoStyle == GizmoStyle.Debug)
            {
                UnityEditor.Handles.color = groundProjectionColor;
                UnityEditor.Handles.Label(hit.point, $"Ground: {hit.distance:F1}m");
            }
            #endif
        }
    }
    
    private void DrawObstacleDetection()
    {
        Vector3 charPos = transform.position;
        Vector3 forward = transform.forward;
        
        // Forward obstacle detection
        bool hasObstacle = Physics.Raycast(charPos, forward, obstacleDetectionDistance, obstacleLayer);
        
        Gizmos.color = hasObstacle ? Color.red : obstacleDetectionColor;
        Gizmos.DrawRay(charPos, forward * obstacleDetectionDistance);
        
        if (hasObstacle)
        {
            Gizmos.color = Color.red * 0.5f;
            Gizmos.DrawSphere(charPos + forward * obstacleDetectionDistance, gizmoSize * 0.2f);
        }
    }
    
    private void DrawInformationLabels()
    {
        #if UNITY_EDITOR
        Vector3 charPos = transform.position;
        
        if (useMultiPointPath && pathPoints != null && pathPoints.Count > 1)
        {
            // Multi-point path information
            float totalDistance = GetTotalPathDistance();
            float estimatedTime = GetEstimatedTravelTime();
            
            // Find the center point for main label
            Vector3 center = Vector3.zero;
            int validPoints = 0;
            foreach (var point in pathPoints)
            {
                if (point != null)
                {
                    center += point.position;
                    validPoints++;
                }
            }
            if (validPoints > 0) center /= validPoints;
            
            UnityEditor.Handles.color = Color.white;
            string pathInfo = "";
            
            if (showDistanceLabels)
            {
                pathInfo += $"Path Distance: {totalDistance:F1}m\n";
                pathInfo += $"Waypoints: {pathPoints.Count}\n";
            }
            
            if (showTimingInfo)
            {
                pathInfo += $"Total Time: {estimatedTime:F1}s\n";
                pathInfo += $"Avg Speed: {moveSpeed:F1}m/s\n";
            }
            
            if (Application.isPlaying && isMoving)
            {
                pathInfo += $"Progress: {pathCompletionPercentage:F1}%\n";
                pathInfo += $"Current Speed: {currentSpeed:F1}m/s\n";
                pathInfo += $"Max Speed: {maxSpeed:F1}m/s\n";
                pathInfo += $"Current Segment: {currentPathIndex + 1}/{pathPoints.Count - 1}";
            }
            
            if (!string.IsNullOrEmpty(pathInfo))
            {
                UnityEditor.Handles.Label(center + Vector3.up * gizmoSize * 3f, pathInfo);
            }
            
            // Individual segment information (Debug style only)
            if (gizmoStyle == GizmoStyle.Debug && (showDistanceLabels || showTimingInfo))
            {
                for (int i = 0; i < pathPoints.Count - 1; i++)
                {
                    if (pathPoints[i] != null && pathPoints[i + 1] != null)
                    {
                        Vector3 segmentMid = (pathPoints[i].position + pathPoints[i + 1].position) * 0.5f;
                        float segmentDistance = Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
                        float segmentTime = segmentDistance / moveSpeed;
                        
                        UnityEditor.Handles.color = i == currentPathIndex ? progressColor : Color.gray;
                        string segmentInfo = $"Seg {i + 1}: {segmentDistance:F1}m, {segmentTime:F1}s";
                        UnityEditor.Handles.Label(segmentMid + Vector3.up * gizmoSize, segmentInfo);
                    }
                }
            }
        }
        else if (pointA != null && pointB != null)
        {
            // Two-point path information
            Vector3 midPoint = (pointA.position + pointB.position) * 0.5f;
            float distance = Vector3.Distance(pointA.position, pointB.position);
            float estimatedTime = distance / moveSpeed;
            
            UnityEditor.Handles.color = Color.white;
            string info = "";
            
            if (showDistanceLabels)
            {
                info += $"Distance: {distance:F1}m\n";
            }
            
            if (showTimingInfo)
            {
                info += $"Time: {estimatedTime:F1}s\n";
                info += $"Speed: {moveSpeed:F1}m/s\n";
            }
            
            if (Application.isPlaying && isMoving)
            {
                Vector3 start = movingToB ? pointA.position : pointB.position;
                Vector3 end = movingToB ? pointB.position : pointA.position;
                float remainingDistance = Vector3.Distance(charPos, end);
                float remainingTime = remainingDistance / Mathf.Max(currentSpeed, 0.1f);
                
                info += $"Progress: {pathCompletionPercentage:F1}%\n";
                info += $"Remaining: {remainingDistance:F1}m, {remainingTime:F1}s\n";
                info += $"Current Speed: {currentSpeed:F1}m/s";
            }
            
            if (!string.IsNullOrEmpty(info))
            {
                UnityEditor.Handles.Label(midPoint + Vector3.up * gizmoSize * 2f, info);
            }
        }
        
        // Character state information (always shown if moving and Debug style)
        if (Application.isPlaying && isMoving && gizmoStyle == GizmoStyle.Debug)
        {
            UnityEditor.Handles.color = velocityColor;
            string characterInfo = "";
            characterInfo += $"Position: ({charPos.x:F1}, {charPos.y:F1}, {charPos.z:F1})\n";
            characterInfo += $"Velocity: {currentVelocity.magnitude:F2}m/s\n";
            characterInfo += $"Direction: {currentVelocity.normalized}\n";
            
            if (useMultiPointPath)
            {
                characterInfo += $"Target Waypoint: {currentPathIndex + 1}\n";
            }
            else
            {
                characterInfo += $"Moving to: {(movingToB ? "Point B" : "Point A")}\n";
            }
            
            characterInfo += $"Movement Mode: {movementMode}";
            
            UnityEditor.Handles.Label(charPos + Vector3.up * gizmoSize * 2f + Vector3.right * gizmoSize * 2f, characterInfo);
        }
        
        // Performance information (Debug style only)
        if (gizmoStyle == GizmoStyle.Debug && Application.isPlaying)
        {
            UnityEditor.Handles.color = Color.cyan;
            string perfInfo = "";
            perfInfo += $"Trail Points: {recentPositions.Count}/{maxTrailLength}\n";
            perfInfo += $"Update Time: {lastGizmoUpdateTime:F3}s\n";
            perfInfo += $"Gizmo Alpha: {gizmoAlpha:F2}\n";
            perfInfo += $"Frame: {Time.frameCount}";
            
            Vector3 perfLabelPos = charPos + Vector3.up * gizmoSize * 4f;
            UnityEditor.Handles.Label(perfLabelPos, perfInfo);
        }
        #endif
    }
    
    private void DrawInteractiveGizmoElements()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying) return;
        
        Vector3 charPos = transform.position;
        
        // Interactive speed control rings (Debug style only)
        if (gizmoStyle == GizmoStyle.Debug && isMoving)
        {
            // Speed adjustment rings
            float baseRadius = gizmoSize * 2f;
            for (int i = 1; i <= 3; i++)
            {
                float ringRadius = baseRadius * i;
                Color ringColor = Color.Lerp(Color.green, Color.red, (float)(i - 1) / 2f);
                ringColor.a = 0.3f * gizmoAlpha;
                
                UnityEditor.Handles.color = ringColor;
                UnityEditor.Handles.DrawWireArc(charPos, Vector3.up, Vector3.forward, 360f, ringRadius);
                
                // Speed labels
                float speedMultiplier = i * 0.5f;
                UnityEditor.Handles.color = ringColor;
                UnityEditor.Handles.Label(charPos + Vector3.forward * ringRadius, $"{moveSpeed * speedMultiplier:F1}m/s");
            }
        }
        
        // Movement constraints visualization
        if (constrainToGround || avoidObstacles)
        {
            // Constraint zone
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.2f * gizmoAlpha);
            float constraintRadius = gizmoSize * 3f;
            UnityEditor.Handles.DrawSolidArc(charPos, Vector3.up, Vector3.forward, 360f, constraintRadius);
            
            // Constraint boundary
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.8f * gizmoAlpha);
            UnityEditor.Handles.DrawWireArc(charPos, Vector3.up, Vector3.forward, 360f, constraintRadius);
        }
        
        // Movement history heatmap (Modern style only)
        if (gizmoStyle == GizmoStyle.Modern && recentPositions.Count > 5)
        {
            for (int i = 1; i < recentPositions.Count; i++)
            {
                float intensity = (float)i / recentPositions.Count;
                Color heatColor = Color.Lerp(Color.blue, Color.red, intensity);
                heatColor.a = intensity * 0.5f * gizmoAlpha;
                
                UnityEditor.Handles.color = heatColor;
                float heatSize = gizmoSize * 0.2f * intensity;
                UnityEditor.Handles.DrawSolidArc(recentPositions[i], Vector3.up, Vector3.forward, 360f, heatSize);
            }
        }
        
        // Gizmo style indicator (always shown in upper right)
        if (gizmoStyle == GizmoStyle.Debug)
        {
            UnityEditor.Handles.color = Color.white;
            Vector3 styleIndicatorPos = charPos + Vector3.up * gizmoSize * 5f + Vector3.right * gizmoSize * 3f;
            string styleInfo = $"Gizmo Style: {gizmoStyle}\nAlpha: {gizmoAlpha:F1}\nSize: {gizmoSize:F1}";
            UnityEditor.Handles.Label(styleIndicatorPos, styleInfo);
        }
        #endif
    }
    
    private void DrawArrowHead(Vector3 position, Vector3 direction, float size, Color color)
    {
        if (direction == Vector3.zero) return;
        
        Gizmos.color = color;
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, direction).normalized;
        
        if (right == Vector3.zero) // Handle case where direction is up/down
        {
            right = Vector3.right;
            up = Vector3.Cross(right, direction).normalized;
        }
        
        Vector3 arrowTip = position;
        Vector3 arrowBase = position - direction * size;
        
        // Draw arrow head
        Gizmos.DrawLine(arrowTip, arrowBase + right * size * 0.5f);
        Gizmos.DrawLine(arrowTip, arrowBase - right * size * 0.5f);
        Gizmos.DrawLine(arrowTip, arrowBase + up * size * 0.5f);
        Gizmos.DrawLine(arrowTip, arrowBase - up * size * 0.5f);
    }
    
    private void DrawWaypointInformation()
    {
        if (!useMultiPointPath || pathPoints == null) return;
        
        for (int i = 0; i < pathPoints.Count; i++)
        {
            if (pathPoints[i] == null) continue;
            
            Vector3 pos = pathPoints[i].position;
            Color waypointCol = Color.Lerp(waypointColor, progressColor, 
                isMoving && i <= currentPathIndex ? 1f : 0f);
            
            // Enhanced waypoint marker
            Gizmos.color = waypointCol * gizmoAlpha;
            switch (gizmoStyle)
            {
                case GizmoStyle.Modern:
                    Gizmos.DrawWireSphere(pos, gizmoSize * 0.8f);
                    Gizmos.color = waypointCol * 0.3f;
                    Gizmos.DrawSphere(pos, gizmoSize * 0.6f);
                    break;
                default:
                    Gizmos.DrawWireCube(pos, Vector3.one * gizmoSize * 0.8f);
                    break;
            }
            
            // Waypoint numbers
            if (showPathNumbers)
            {
                #if UNITY_EDITOR
                UnityEditor.Handles.color = waypointCol;
                string label = $"{i + 1}";
                if (gizmoStyle == GizmoStyle.Debug && waypointTimings.ContainsKey(i))
                {
                    label += $"\n{waypointTimings[i]:F1}s";
                }
                UnityEditor.Handles.Label(pos + Vector3.up * gizmoSize * 1.5f, label);
                #endif
            }
        }
    }
    
    private void DrawCollisionBounds()
    {
        Vector3 charPos = transform.position;
        Collider col = GetComponent<Collider>();
        
        if (col != null)
        {
            Gizmos.color = collisionBoundsColor * gizmoAlpha;
            
            if (col is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(charPos, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else if (col is SphereCollider sphere)
            {
                float radius = sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                Gizmos.DrawWireSphere(charPos + sphere.center, radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                // Simplified capsule visualization
                float radius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                float height = capsule.height * transform.lossyScale.y;
                Gizmos.DrawWireSphere(charPos + Vector3.up * (height * 0.5f - radius), radius);
                Gizmos.DrawWireSphere(charPos + Vector3.up * (-height * 0.5f + radius), radius);
            }
        }
        else
        {
            // Default collision bounds
            Gizmos.DrawWireCube(charPos, Vector3.one * gizmoSize);
        }
    }
    
    private void DrawInfluenceZones()
    {
        if (!useMultiPointPath || pathPoints == null) return;
        
        for (int i = 0; i < pathPoints.Count; i++)
        {
            if (pathPoints[i] == null) continue;
            
            Vector3 pos = pathPoints[i].position;
            float influenceRadius = gizmoSize * 3f;
            
            // Different influence for different waypoint types
            Color zoneColor = influenceZoneColor * 0.2f;
            if (i == 0) zoneColor = pointAColor * 0.2f; // Start zone
            else if (i == pathPoints.Count - 1) zoneColor = pointBColor * 0.2f; // End zone
            
            Gizmos.color = zoneColor;
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = zoneColor;
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, influenceRadius);
            
            // Influence zone rings
            UnityEditor.Handles.color = zoneColor * 2f;
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, influenceRadius);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, influenceRadius * 0.7f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, influenceRadius * 0.4f);
            #endif
        }
    }
    
    private void DrawPathPrediction()
    {
        if (!isMoving || currentVelocity.magnitude < 0.1f) return;
        
        Vector3 currentPos = transform.position;
        Vector3 predictedPos = currentPos + currentVelocity * 2f; // Predict 2 seconds ahead
        
        // Update predicted position
        predictedPosition = predictedPos;
        
        Gizmos.color = predictionColor * gizmoAlpha;
        
        switch (gizmoStyle)
        {
            case GizmoStyle.Modern:
                // Dashed prediction line
                #if UNITY_EDITOR
                UnityEditor.Handles.color = predictionColor * 0.7f;
                UnityEditor.Handles.DrawDottedLine(currentPos, predictedPos, 3f);
                #endif
                
                // Prediction marker
                Gizmos.color = predictionColor * 0.5f;
                Gizmos.DrawWireSphere(predictedPos, gizmoSize * 0.6f);
                break;
                
            default:
                Gizmos.DrawLine(currentPos, predictedPos);
                Gizmos.DrawWireCube(predictedPos, Vector3.one * gizmoSize * 0.5f);
                break;
        }
        
        // Prediction cone showing movement uncertainty
        if (gizmoStyle == GizmoStyle.Debug)
        {
            Vector3 perpendicular = Vector3.Cross(currentVelocity.normalized, Vector3.up).normalized;
            float coneWidth = gizmoSize * 0.5f;
            
            Gizmos.color = predictionColor * 0.3f;
            Vector3 coneLeft = predictedPos + perpendicular * coneWidth;
            Vector3 coneRight = predictedPos - perpendicular * coneWidth;
            
            Gizmos.DrawLine(currentPos, coneLeft);
            Gizmos.DrawLine(currentPos, coneRight);
            Gizmos.DrawLine(coneLeft, coneRight);
        }
        
        #if UNITY_EDITOR
        if (gizmoStyle == GizmoStyle.Debug)
        {
            UnityEditor.Handles.color = predictionColor;
            UnityEditor.Handles.Label(predictedPos + Vector3.up * gizmoSize, 
                $"Predicted\n{Time.time + 2f:F1}s");
        }
        #endif
    }
    
    #endregion
    
    #region Utility Methods
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public Vector3 GetCurrentTarget()
    {
        return currentTarget;
    }
    
    public float GetProgressToTarget()
    {
        if (!isMoving || pointA == null || pointB == null) return 0f;
        
        Vector3 start = movingToB ? pointA.position : pointB.position;
        float totalDistance = Vector3.Distance(start, currentTarget);
        float currentDistance = Vector3.Distance(start, transform.position);
        
        return Mathf.Clamp01(currentDistance / totalDistance);
    }
    
    public void SetPoints(Transform newPointA, Transform newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
    }
    
    public void SetMovementMode(MovementMode mode)
    {
        movementMode = mode;
        switch (mode)
        {
            case MovementMode.ConstantSpeed:
                movementCurve = AnimationCurve.Linear(0, 0, 1, 1);
                break;
            case MovementMode.EaseInOut:
                movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                break;
            case MovementMode.Accelerate:
                movementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
                break;
            case MovementMode.Decelerate:
                movementCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
                break;
        }
    }
    
    public float GetTotalPathDistance()
    {
        if (useMultiPointPath)
        {
            float total = 0f;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                if (pathPoints[i] != null && pathPoints[i + 1] != null)
                {
                    total += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
                }
            }
            return total;
        }
        else if (pointA != null && pointB != null)
        {
            return Vector3.Distance(pointA.position, pointB.position);
        }
        return 0f;
    }
    
    public float GetEstimatedTravelTime()
    {
        return GetTotalPathDistance() / moveSpeed;
    }
    
    #endregion
}
