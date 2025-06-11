using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CameraSystem : MonoBehaviour
{
    [Header("Camera Setup")]
    public CinemachineCamera virtualCamera;
    
    [Header("Multi-Shot System")]
    public bool autoPlayOnStart = false;
    public List<CameraShot> shots = new List<CameraShot>();
    public bool loopSequence = false;
    
    [Header("Sequence Controls")]
    public float delayBetweenShots = 0.5f;
    public bool showShotProgressInConsole = true;
    
    [Header("Debug")]
    public bool showGizmos = true;
    public bool showShotPaths = true;
    
    // Runtime variables
    private int currentShotIndex = 0;
    private bool isPlayingSequence = false;
    private Coroutine sequenceCoroutine;
    
    void Start()
    {
        // Auto-find camera if not assigned
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineCamera>();
            
        // Auto-play if enabled
        if (autoPlayOnStart && shots.Count > 0)
        {
            PlaySequence();
        }
    }
    
    #region Public Controls
    
    [ContextMenu("Play Shot Sequence")]
    public void PlaySequence()
    {
        if (shots.Count == 0 || isPlayingSequence) return;
        
        if (showShotProgressInConsole)
            Debug.Log($"[CameraSystem] Starting sequence with {shots.Count} shots");
            
        sequenceCoroutine = StartCoroutine(ExecuteSequence());
    }
    
    [ContextMenu("Stop Sequence")]
    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        isPlayingSequence = false;
        
        if (showShotProgressInConsole)
            Debug.Log("[CameraSystem] Sequence stopped");
    }
    
    public void PlaySpecificShot(int shotIndex)
    {
        if (shotIndex >= 0 && shotIndex < shots.Count && !isPlayingSequence)
        {
            StartCoroutine(ExecuteShot(shots[shotIndex], shotIndex));
        }
    }
    
    public void JumpToShot(int shotIndex)
    {
        if (shotIndex >= 0 && shotIndex < shots.Count)
        {
            StopSequence();
            currentShotIndex = shotIndex;
            PlaySpecificShot(shotIndex);
        }
    }
    
    #endregion
    
    #region Sequence Execution
    
    private IEnumerator ExecuteSequence()
    {
        isPlayingSequence = true;
        currentShotIndex = 0;
        
        do
        {
            for (int i = 0; i < shots.Count; i++)
            {
                currentShotIndex = i;
                yield return StartCoroutine(ExecuteShot(shots[i], i));
                
                // Delay between shots
                if (delayBetweenShots > 0 && i < shots.Count - 1)
                {
                    yield return new WaitForSeconds(delayBetweenShots);
                }
            }
            
        } while (loopSequence && isPlayingSequence);
        
        isPlayingSequence = false;
        
        if (showShotProgressInConsole)
            Debug.Log("[CameraSystem] Sequence completed");
    }
    
    private IEnumerator ExecuteShot(CameraShot shot, int shotIndex)
    {
        if (shot.startPosition == null)
        {
            Debug.LogWarning($"[CameraSystem] Shot {shotIndex} '{shot.shotName}' has no start position!");
            yield break;
        }
        
        if (showShotProgressInConsole)
            Debug.Log($"[CameraSystem] Executing Shot {shotIndex + 1}: '{shot.shotName}' ({shot.shotType})");
        
        // Execute based on shot type
        switch (shot.shotType)
        {
            case CameraShot.ShotType.Static:
                yield return StartCoroutine(ExecuteStaticShot(shot));
                break;
                
            case CameraShot.ShotType.Movement:
                yield return StartCoroutine(ExecuteMovementShot(shot));
                break;
                
            case CameraShot.ShotType.Pan:
                yield return StartCoroutine(ExecutePanShot(shot));
                break;
                
            case CameraShot.ShotType.MovementWithPan:
                yield return StartCoroutine(ExecuteMovementWithPanShot(shot));
                break;
                
            case CameraShot.ShotType.OrbitAround:
                yield return StartCoroutine(ExecuteOrbitShot(shot));
                break;
                
            case CameraShot.ShotType.DollyPath:
                yield return StartCoroutine(ExecuteDollyPathShot(shot));
                break;
                
            case CameraShot.ShotType.FreeRotation:
                yield return StartCoroutine(ExecuteFreeRotationShot(shot));
                break;
                
            case CameraShot.ShotType.LookAtOnly:
                yield return StartCoroutine(ExecuteLookAtOnlyShot(shot));
                break;
                
            case CameraShot.ShotType.CustomSequence:
                yield return StartCoroutine(ExecuteCustomSequenceShot(shot));
                break;
        }
        
        // Optional wait at end
        if (shot.waitAtEnd && shot.waitDuration > 0)
        {
            yield return new WaitForSeconds(shot.waitDuration);
        }
    }
    
    #endregion
    
    #region Shot Type Implementations
    
    private IEnumerator ExecuteStaticShot(CameraShot shot)
    {
        // Set position
        virtualCamera.transform.position = shot.startPosition.position;
        
        // Handle rotation based on type
        SetCameraRotation(shot, 0f);
        
        // Hold for duration
        yield return new WaitForSeconds(shot.duration);
    }
    
    private IEnumerator ExecuteMovementShot(CameraShot shot)
    {
        if (shot.endPosition == null)
        {
            Debug.LogWarning($"[CameraSystem] Movement shot '{shot.shotName}' has no end position!");
            yield break;
        }
        
        Vector3 startPos = shot.startPosition.position;
        Vector3 endPos = shot.endPosition.position;
        Quaternion startRot = shot.startPosition.rotation;
        Quaternion endRot = shot.endPosition.rotation;
        
        float elapsed = 0f;
        
        while (elapsed < shot.duration)
        {
            float t = shot.movementCurve.Evaluate(elapsed / shot.duration);
            
            // Move camera
            virtualCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            
            // Handle rotation
            if (shot.rotationType == CameraShot.RotationType.LookAtTarget && shot.lookAtTarget != null)
            {
                if (shot.smoothLookAt)
                {
                    Vector3 direction = (shot.lookAtTarget.position - virtualCamera.transform.position).normalized;
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    virtualCamera.transform.rotation = Quaternion.Slerp(virtualCamera.transform.rotation, targetRot, Time.deltaTime * shot.lookAtSpeed);
                }
                else
                {
                    virtualCamera.transform.LookAt(shot.lookAtTarget.position);
                }
            }
            else if (shot.rotationType == CameraShot.RotationType.FollowMovementDirection)
            {
                Vector3 moveDirection = (endPos - startPos).normalized;
                if (moveDirection != Vector3.zero)
                {
                    virtualCamera.transform.rotation = Quaternion.LookRotation(moveDirection);
                }
            }
            else if (shot.rotationType == CameraShot.RotationType.UseTransformRotation)
            {
                float rotT = shot.rotationCurve.Evaluate(elapsed / shot.duration);
                virtualCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, rotT);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position
        virtualCamera.transform.position = endPos;
        if (shot.rotationType == CameraShot.RotationType.UseTransformRotation)
        {
            virtualCamera.transform.rotation = endRot;
        }
    }
    
    private IEnumerator ExecutePanShot(CameraShot shot)
    {
        if (shot.panStartTarget == null || shot.panEndTarget == null)
        {
            Debug.LogWarning($"[CameraSystem] Pan shot '{shot.shotName}' missing pan targets!");
            yield break;
        }
        
        // Set to start position
        virtualCamera.transform.position = shot.startPosition.position;
        
        float elapsed = 0f;
        
        while (elapsed < shot.duration)
        {
            float t = shot.panCurve.Evaluate(elapsed / shot.duration);
            
            Vector3 targetPos = Vector3.Lerp(shot.panStartTarget.position, shot.panEndTarget.position, t);
            virtualCamera.transform.LookAt(targetPos);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Final look
        virtualCamera.transform.LookAt(shot.panEndTarget.position);
    }
    
    private IEnumerator ExecuteMovementWithPanShot(CameraShot shot)
    {
        if (shot.endPosition == null || shot.panStartTarget == null || shot.panEndTarget == null)
        {
            Debug.LogWarning($"[CameraSystem] MovementWithPan shot '{shot.shotName}' missing required transforms!");
            yield break;
        }
        
        Vector3 startPos = shot.startPosition.position;
        Vector3 endPos = shot.endPosition.position;
        
        float elapsed = 0f;
        
        while (elapsed < shot.duration)
        {
            float moveT = shot.movementCurve.Evaluate(elapsed / shot.duration);
            float panT = shot.panCurve.Evaluate(elapsed / shot.duration);
            
            // Move camera
            virtualCamera.transform.position = Vector3.Lerp(startPos, endPos, moveT);
            
            // Pan between targets
            Vector3 targetPos = Vector3.Lerp(shot.panStartTarget.position, shot.panEndTarget.position, panT);
            virtualCamera.transform.LookAt(targetPos);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        virtualCamera.transform.position = endPos;
        virtualCamera.transform.LookAt(shot.panEndTarget.position);
    }
    
    private IEnumerator ExecuteOrbitShot(CameraShot shot)
    {
        if (shot.orbitCenter == null)
        {
            Debug.LogWarning($"[CameraSystem] Orbit shot '{shot.shotName}' has no orbit center!");
            yield break;
        }
        
        Vector3 centerPos = shot.orbitCenter.position;
        Vector3 startOffset = shot.startPosition.position - centerPos;
        float startAngle = Mathf.Atan2(startOffset.x, startOffset.z) * Mathf.Rad2Deg;
        
        float elapsed = 0f;
        
        while (elapsed < shot.duration)
        {
            float t = shot.movementCurve.Evaluate(elapsed / shot.duration);
            float currentAngle = startAngle + (shot.orbitAngle * t);
            
            // Calculate orbit position
            Vector3 orbitPos = centerPos + Quaternion.AngleAxis(currentAngle, shot.orbitAxis) * 
                              (Vector3.forward * shot.orbitRadius);
            orbitPos.y = shot.startPosition.position.y; // Maintain height
            
            virtualCamera.transform.position = orbitPos;
            
            // Look at center or specified target
            Transform lookTarget = shot.useLookAt && shot.lookAtTarget != null ? shot.lookAtTarget : shot.orbitCenter;
            virtualCamera.transform.LookAt(lookTarget.position);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator ExecuteDollyPathShot(CameraShot shot)
    {
        if (shot.dollyPath.Count < 2)
        {
            Debug.LogWarning($"[CameraSystem] Dolly path shot '{shot.shotName}' needs at least 2 path points!");
            yield break;
        }
        
        float elapsed = 0f;
        int totalSegments = shot.dollyPath.Count - 1;
        
        while (elapsed < shot.duration)
        {
            float t = shot.movementCurve.Evaluate(elapsed / shot.duration);
            float pathProgress = t * totalSegments;
            int segmentIndex = Mathf.FloorToInt(pathProgress);
            float segmentT = pathProgress - segmentIndex;
            
            // Clamp to valid segment
            segmentIndex = Mathf.Clamp(segmentIndex, 0, totalSegments - 1);
            
            Vector3 startPos = shot.dollyPath[segmentIndex].position;
            Vector3 endPos = shot.dollyPath[segmentIndex + 1].position;
            
            virtualCamera.transform.position = Vector3.Lerp(startPos, endPos, segmentT);
            
            // Handle rotation
            SetCameraRotation(shot, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator ExecuteFreeRotationShot(CameraShot shot)
    {
        // Stay at start position, rotate freely
        virtualCamera.transform.position = shot.startPosition.position;
        
        Quaternion startRot = shot.startPosition.rotation;
        Quaternion endRot = shot.endPosition != null ? shot.endPosition.rotation : startRot;
        
        float elapsed = 0f;
        
        while (elapsed < shot.duration)
        {
            float t = shot.rotationCurve.Evaluate(elapsed / shot.duration);
            virtualCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        virtualCamera.transform.rotation = endRot;
    }
    
    private IEnumerator ExecuteLookAtOnlyShot(CameraShot shot)
    {
        if (shot.lookAtTarget == null)
        {
            Debug.LogWarning($"[CameraSystem] LookAtOnly shot '{shot.shotName}' has no look at target!");
            yield break;
        }
        
        // Set position and look at target
        virtualCamera.transform.position = shot.startPosition.position;
        virtualCamera.transform.LookAt(shot.lookAtTarget.position);
        
        // Hold and continuously look if needed
        float elapsed = 0f;
        while (elapsed < shot.duration)
        {
            if (shot.continuousLookAt)
            {
                virtualCamera.transform.LookAt(shot.lookAtTarget.position);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator ExecuteCustomSequenceShot(CameraShot shot)
    {
        // This can be extended for complex custom behaviors
        // For now, combine movement with continuous look-at
        
        if (shot.endPosition == null)
        {
            yield return StartCoroutine(ExecuteStaticShot(shot));
            yield break;
        }
        
        Vector3 startPos = shot.startPosition.position;
        Vector3 endPos = shot.endPosition.position;
        
        float elapsed = 0f;
        
        while (elapsed < shot.duration)
        {
            float t = shot.movementCurve.Evaluate(elapsed / shot.duration);
            
            virtualCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            
            if (shot.useLookAt && shot.lookAtTarget != null)
            {
                virtualCamera.transform.LookAt(shot.lookAtTarget.position);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        virtualCamera.transform.position = endPos;
    }
    
    #endregion
    
    #region Helper Methods
    
    private void SetCameraRotation(CameraShot shot, float t)
    {
        switch (shot.rotationType)
        {
            case CameraShot.RotationType.UseTransformRotation:
                virtualCamera.transform.rotation = shot.startPosition.rotation;
                break;
                
            case CameraShot.RotationType.LookAtTarget:
                if (shot.lookAtTarget != null)
                    virtualCamera.transform.LookAt(shot.lookAtTarget.position);
                break;
                
            case CameraShot.RotationType.KeepCurrentRotation:
                // Do nothing - keep current rotation
                break;
                
            case CameraShot.RotationType.FreeRotation:
                if (shot.endPosition != null)
                {
                    float rotT = shot.rotationCurve.Evaluate(t);
                    virtualCamera.transform.rotation = Quaternion.Lerp(
                        shot.startPosition.rotation, 
                        shot.endPosition.rotation, 
                        rotT
                    );
                }
                break;
        }
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmos()
    {
        if (!showGizmos || shots == null) return;
        
        for (int i = 0; i < shots.Count; i++)
        {
            CameraShot shot = shots[i];
            if (shot.startPosition == null) continue;
            
            // Color code by shot type
            Color shotColor = GetShotTypeColor(shot.shotType);
            Gizmos.color = shotColor;
            
            // Draw start position
            Gizmos.DrawWireSphere(shot.startPosition.position, 0.3f);
            
            // Draw shot-specific gizmos
            DrawShotSpecificGizmos(shot, i);
        }
        
        // Draw sequence path
        if (showShotPaths && shots.Count > 1)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < shots.Count - 1; i++)
            {
                if (shots[i].startPosition != null && shots[i + 1].startPosition != null)
                {
                    Gizmos.DrawLine(shots[i].startPosition.position, shots[i + 1].startPosition.position);
                }
            }
        }
    }
    
    private Color GetShotTypeColor(CameraShot.ShotType shotType)
    {
        switch (shotType)
        {
            case CameraShot.ShotType.Static: return Color.blue;
            case CameraShot.ShotType.Movement: return Color.green;
            case CameraShot.ShotType.Pan: return Color.red;
            case CameraShot.ShotType.MovementWithPan: return Color.yellow;
            case CameraShot.ShotType.OrbitAround: return Color.magenta;
            case CameraShot.ShotType.DollyPath: return Color.cyan;
            case CameraShot.ShotType.FreeRotation: return Color.orange;
            case CameraShot.ShotType.LookAtOnly: return Color.white;
            case CameraShot.ShotType.CustomSequence: return Color.gray;
            default: return Color.white;
        }
    }
    
    private void DrawShotSpecificGizmos(CameraShot shot, int index)
    {
        // Draw end position for movement shots
        if (shot.endPosition != null && 
            (shot.shotType == CameraShot.ShotType.Movement || 
             shot.shotType == CameraShot.ShotType.MovementWithPan ||
             shot.shotType == CameraShot.ShotType.FreeRotation))
        {
            Gizmos.DrawWireCube(shot.endPosition.position, Vector3.one * 0.3f);
            Gizmos.DrawLine(shot.startPosition.position, shot.endPosition.position);
        }
        
        // Draw look at target
        if (shot.useLookAt && shot.lookAtTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(shot.lookAtTarget.position, 0.2f);
            Gizmos.DrawLine(shot.startPosition.position, shot.lookAtTarget.position);
        }
        
        // Draw pan targets
        if (shot.panStartTarget != null && shot.panEndTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(shot.panStartTarget.position, 0.15f);
            Gizmos.DrawWireSphere(shot.panEndTarget.position, 0.15f);
            Gizmos.DrawLine(shot.panStartTarget.position, shot.panEndTarget.position);
        }
        
        // Draw orbit center and radius
        if (shot.shotType == CameraShot.ShotType.OrbitAround && shot.orbitCenter != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(shot.orbitCenter.position, shot.orbitRadius);
            Gizmos.DrawLine(shot.startPosition.position, shot.orbitCenter.position);
        }
        
        // Draw dolly path
        if (shot.shotType == CameraShot.ShotType.DollyPath && shot.dollyPath.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < shot.dollyPath.Count - 1; i++)
            {
                if (shot.dollyPath[i] != null && shot.dollyPath[i + 1] != null)
                {
                    Gizmos.DrawLine(shot.dollyPath[i].position, shot.dollyPath[i + 1].position);
                    Gizmos.DrawWireSphere(shot.dollyPath[i].position, 0.1f);
                }
            }
            if (shot.dollyPath[shot.dollyPath.Count - 1] != null)
            {
                Gizmos.DrawWireSphere(shot.dollyPath[shot.dollyPath.Count - 1].position, 0.1f);
            }
        }
    }
    
    #endregion
}