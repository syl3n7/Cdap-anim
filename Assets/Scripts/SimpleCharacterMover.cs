using UnityEngine;
using System.Collections;

public class SimpleCharacterMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 5f;
    public bool autoStartOnPlay = true;
    public bool loopMovement = false;
    
    [Header("Animation Curve")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Rotation Settings")]
    public bool rotateTowardsTarget = true;
    public float rotationSpeed = 10f;
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    // Runtime variables
    private bool isMoving = false;
    private Coroutine movementCoroutine;
    private Vector3 currentTarget;
    private bool movingToB = true; // True when moving A->B, False when moving B->A
    
    void Start()
    {
        if (autoStartOnPlay && pointA != null && pointB != null)
        {
            // Start at point A
            transform.position = pointA.position;
            MoveToPointB();
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
    
    [ContextMenu("Stop Movement")]
    public void StopMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
        isMoving = false;
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
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, target);
        float duration = distance / speed;
        
        Debug.Log($"[CharacterMover] Moving to {(movingToB ? "Point B" : "Point A")} - Distance: {distance:F2}m, Duration: {duration:F2}s");
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = movementCurve.Evaluate(elapsed / duration);
            transform.position = Vector3.Lerp(startPosition, target, t);
            
            // Rotate towards movement direction
            if (rotateTowardsTarget)
            {
                Vector3 direction = (target - startPosition).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position
        transform.position = target;
        isMoving = false;
        
        Debug.Log($"[CharacterMover] Reached {(movingToB ? "Point B" : "Point A")}");
        
        // Handle looping
        if (loopMovement)
        {
            yield return new WaitForSeconds(0.5f); // Small delay before loop
            
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
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw points
        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointA.position, 0.5f);
            Gizmos.DrawCube(pointA.position + Vector3.up * 1f, Vector3.one * 0.3f);
            
            // Label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(pointA.position + Vector3.up * 1.5f, "Point A");
            #endif
        }
        
        if (pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointB.position, 0.5f);
            Gizmos.DrawCube(pointB.position + Vector3.up * 1f, Vector3.one * 0.3f);
            
            // Label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(pointB.position + Vector3.up * 1.5f, "Point B");
            #endif
        }
        
        // Draw path
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            
            // Draw distance
            Vector3 midPoint = (pointA.position + pointB.position) / 2f;
            float distance = Vector3.Distance(pointA.position, pointB.position);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(midPoint + Vector3.up * 0.5f, $"{distance:F1}m");
            #endif
        }
        
        // Draw current target if moving
        if (isMoving)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentTarget, 0.3f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
        
        // Draw character direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 1f);
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
    
    #endregion
}
