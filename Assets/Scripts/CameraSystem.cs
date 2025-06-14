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
    
    [Header("Shot Selection & Preview")]
    public int selectedShotIndex = -1;
    public bool highlightSelectedShot = true;
    public bool showOnlySelectedShot = false;
    
    [Header("Debug")]
    public bool showGizmos = true;
    public bool showShotPaths = true;
    public bool showCameraFrustum = true;
    public bool showLookDirections = true;
    public bool showTargetConnections = true;
    public float frustumSize = 2f;
    public float lookDirectionLength = 3f;
    
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
    
    [ContextMenu("Select Next Shot")]
    public void SelectNextShot()
    {
        if (shots.Count > 0)
        {
            selectedShotIndex = (selectedShotIndex + 1) % shots.Count;
            if (showShotProgressInConsole)
                Debug.Log($"[CameraSystem] Selected Shot {selectedShotIndex + 1}: '{shots[selectedShotIndex].shotName}'");
        }
    }
    
    [ContextMenu("Select Previous Shot")]
    public void SelectPreviousShot()
    {
        if (shots.Count > 0)
        {
            selectedShotIndex = selectedShotIndex <= 0 ? shots.Count - 1 : selectedShotIndex - 1;
            if (showShotProgressInConsole)
                Debug.Log($"[CameraSystem] Selected Shot {selectedShotIndex + 1}: '{shots[selectedShotIndex].shotName}'");
        }
    }
    
    public void SelectShot(int shotIndex)
    {
        if (shotIndex >= 0 && shotIndex < shots.Count)
        {
            selectedShotIndex = shotIndex;
            if (showShotProgressInConsole)
                Debug.Log($"[CameraSystem] Selected Shot {shotIndex + 1}: '{shots[shotIndex].shotName}'");
        }
    }
    
    [ContextMenu("Preview Selected Shot")]
    public void PreviewSelectedShot()
    {
        if (selectedShotIndex >= 0 && selectedShotIndex < shots.Count && !isPlayingSequence)
        {
            PlaySpecificShot(selectedShotIndex);
        }
    }

    [ContextMenu("Jump to Selected Shot")]
    public void JumpToSelectedShot()
    {
        if (selectedShotIndex >= 0 && selectedShotIndex < shots.Count)
        {
            CameraShot shot = shots[selectedShotIndex];
            if (shot.startPosition != null)
            {
                virtualCamera.transform.position = shot.startPosition.position;
                virtualCamera.transform.rotation = shot.startPosition.rotation;
                
                if (showShotProgressInConsole)
                    Debug.Log($"[CameraSystem] Jumped to Shot {selectedShotIndex + 1}: '{shot.shotName}'");
            }
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
                shots[currentShotIndex].StartWalk?.Invoke();
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
        
        // Handle rotation based on type or override
        if (shot.useStaticRotationOverride)
        {
            virtualCamera.transform.rotation = Quaternion.Euler(shot.staticRotationOverride);
        }
        else
        {
            SetCameraRotation(shot, 0f);
        }
        
        // Handle look-at if enabled
        if (shot.useLookAt && shot.lookAtTarget != null)
        {
            float elapsed = 0f;
            while (elapsed < shot.duration)
            {
                if (shot.continuousLookAt)
                {
                    Vector3 direction = (shot.lookAtTarget.position - virtualCamera.transform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(direction);
                        if (shot.smoothLookAt)
                        {
                            virtualCamera.transform.rotation = Quaternion.Slerp(
                                virtualCamera.transform.rotation, 
                                targetRot, 
                                Time.deltaTime * shot.lookAtSpeed
                            );
                        }
                        else
                        {
                            virtualCamera.transform.rotation = targetRot;
                        }
                    }
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // Hold for duration
            yield return new WaitForSeconds(shot.duration);
        }
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
            Vector3 direction = (targetPos - virtualCamera.transform.position).normalized;
            
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                
                // Apply rotation offset if specified
                if (shot.panRotationOffset != Vector3.zero)
                {
                    lookRotation *= Quaternion.Euler(shot.panRotationOffset);
                }
                
                virtualCamera.transform.rotation = lookRotation;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Final look with offset
        Vector3 finalDirection = (shot.panEndTarget.position - virtualCamera.transform.position).normalized;
        if (finalDirection != Vector3.zero)
        {
            Quaternion finalRotation = Quaternion.LookRotation(finalDirection);
            if (shot.panRotationOffset != Vector3.zero)
            {
                finalRotation *= Quaternion.Euler(shot.panRotationOffset);
            }
            virtualCamera.transform.rotation = finalRotation;
        }
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
            
            // Maintain height or use start position height
            if (shot.maintainOrbitHeight)
            {
                orbitPos.y = shot.startPosition.position.y;
            }
            
            virtualCamera.transform.position = orbitPos;
            
            // Look at center or specified target
            Transform lookTarget = shot.useLookAt && shot.lookAtTarget != null ? shot.lookAtTarget : shot.orbitCenter;
            Vector3 lookDirection = (lookTarget.position - virtualCamera.transform.position).normalized;
            
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                
                // Apply look offset if specified
                if (shot.orbitLookOffset != Vector3.zero)
                {
                    lookRotation *= Quaternion.Euler(shot.orbitLookOffset);
                }
                
                virtualCamera.transform.rotation = lookRotation;
            }
            
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
        
        // Set position
        virtualCamera.transform.position = shot.startPosition.position;
        
        // Initial look at target with offset
        Vector3 direction = (shot.lookAtTarget.position - virtualCamera.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            if (shot.lookAtRotationOffset != Vector3.zero)
            {
                lookRotation *= Quaternion.Euler(shot.lookAtRotationOffset);
            }
            virtualCamera.transform.rotation = lookRotation;
        }
        
        // Hold and continuously look if needed
        float elapsed = 0f;
        while (elapsed < shot.duration)
        {
            if (shot.continuousLookAt)
            {
                Vector3 currentDirection = (shot.lookAtTarget.position - virtualCamera.transform.position).normalized;
                if (currentDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
                    if (shot.lookAtRotationOffset != Vector3.zero)
                    {
                        targetRotation *= Quaternion.Euler(shot.lookAtRotationOffset);
                    }
                    
                    if (shot.smoothLookAt)
                    {
                        virtualCamera.transform.rotation = Quaternion.Slerp(
                            virtualCamera.transform.rotation, 
                            targetRotation, 
                            Time.deltaTime * shot.lookAtSpeed
                        );
                    }
                    else
                    {
                        virtualCamera.transform.rotation = targetRotation;
                    }
                }
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
                if (shot.endPosition != null)
                {
                    float rotT = shot.rotationCurve.Evaluate(t);
                    virtualCamera.transform.rotation = Quaternion.Lerp(
                        shot.startPosition.rotation, 
                        shot.endPosition.rotation, 
                        rotT
                    );
                }
                else
                {
                    virtualCamera.transform.rotation = shot.startPosition.rotation;
                }
                break;
                
            case CameraShot.RotationType.LookAtTarget:
                if (shot.lookAtTarget != null)
                {
                    Vector3 direction = (shot.lookAtTarget.position - virtualCamera.transform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(direction);
                        if (shot.smoothLookAt)
                        {
                            virtualCamera.transform.rotation = Quaternion.Slerp(
                                virtualCamera.transform.rotation, 
                                lookRotation, 
                                Time.deltaTime * shot.lookAtSpeed
                            );
                        }
                        else
                        {
                            virtualCamera.transform.rotation = lookRotation;
                        }
                    }
                }
                break;
                
            case CameraShot.RotationType.KeepCurrentRotation:
                // Do nothing - keep current rotation
                break;
                
            case CameraShot.RotationType.FreeRotation:
            case CameraShot.RotationType.CustomEulerAngles:
                if (shot.endPosition != null)
                {
                    float rotT = shot.rotationCurve.Evaluate(t);
                    
                    if (shot.rotationType == CameraShot.RotationType.CustomEulerAngles)
                    {
                        // Use custom Euler angles
                        Vector3 startEuler = shot.customRotationStart;
                        Vector3 endEuler = shot.customRotationEnd;
                        Vector3 currentEuler = Vector3.Lerp(startEuler, endEuler, rotT);
                        
                        if (shot.useLocalRotation)
                        {
                            virtualCamera.transform.localRotation = Quaternion.Euler(currentEuler);
                        }
                        else
                        {
                            virtualCamera.transform.rotation = Quaternion.Euler(currentEuler);
                        }
                    }
                    else
                    {
                        // Use transform rotations
                        virtualCamera.transform.rotation = Quaternion.Lerp(
                            shot.startPosition.rotation, 
                            shot.endPosition.rotation, 
                            rotT
                        );
                    }
                }
                break;
                
            case CameraShot.RotationType.FollowMovementDirection:
                if (shot.endPosition != null)
                {
                    Vector3 moveDirection = (shot.endPosition.position - shot.startPosition.position).normalized;
                    if (moveDirection != Vector3.zero)
                    {
                        virtualCamera.transform.rotation = Quaternion.LookRotation(moveDirection);
                    }
                }
                break;
        }
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmos()
    {
        if (!showGizmos || shots == null) return;
        
        // Validate selected shot index
        if (selectedShotIndex >= shots.Count) selectedShotIndex = -1;
        
        for (int i = 0; i < shots.Count; i++)
        {
            CameraShot shot = shots[i];
            if (shot.startPosition == null) continue;
            
            // Skip non-selected shots if showing only selected
            if (showOnlySelectedShot && selectedShotIndex != -1 && i != selectedShotIndex)
                continue;
            
            // Determine if this shot is selected
            bool isSelected = (i == selectedShotIndex);
            bool isCurrentlyPlaying = (i == currentShotIndex && isPlayingSequence);
            
            // Color code by shot type with selection highlighting
            Color shotColor = GetShotTypeColor(shot.shotType);
            if (isSelected && highlightSelectedShot)
            {
                shotColor = Color.Lerp(shotColor, Color.white, 0.5f); // Brighten selected shot
            }
            if (isCurrentlyPlaying)
            {
                shotColor = Color.Lerp(shotColor, Color.yellow, 0.3f); // Yellow tint for playing shot
            }
            
            Gizmos.color = shotColor;
            
            // Draw start position marker (larger if selected)
            float markerSize = isSelected ? 0.5f : 0.3f;
            Gizmos.DrawWireSphere(shot.startPosition.position, markerSize);
            
            // Draw selection indicator
            if (isSelected && highlightSelectedShot)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(shot.startPosition.position, markerSize + 0.1f);
                Gizmos.color = shotColor;
            }
            
            // Draw camera frustum and look direction at start position
            if (showCameraFrustum || showLookDirections)
            {
                float sizeMultiplier = isSelected ? 1.3f : 1.0f;
                DrawCameraVisualization(shot, shot.startPosition, shotColor, $"Shot {i + 1} Start", sizeMultiplier, isSelected);
            }
            
            // Draw end position if applicable
            if (shot.endPosition != null && (shot.shotType == CameraShot.ShotType.Movement || 
                shot.shotType == CameraShot.ShotType.MovementWithPan ||
                shot.shotType == CameraShot.ShotType.FreeRotation ||
                shot.shotType == CameraShot.ShotType.CustomSequence))
            {
                Gizmos.color = shotColor;
                Vector3 cubeSize = Vector3.one * (isSelected ? 0.4f : 0.3f);
                Gizmos.DrawWireCube(shot.endPosition.position, cubeSize);
                Gizmos.DrawLine(shot.startPosition.position, shot.endPosition.position);
                
                // Draw camera visualization at end position
                if (showCameraFrustum || showLookDirections)
                {
                    float sizeMultiplier = isSelected ? 1.3f : 1.0f;
                    DrawCameraVisualization(shot, shot.endPosition, shotColor * 0.7f, $"Shot {i + 1} End", sizeMultiplier, isSelected);
                }
            }
            
            // Draw shot-specific gizmos
            DrawShotSpecificGizmos(shot, i, isSelected);
        }
        
        // Draw sequence path (unless showing only selected shot)
        if (showShotPaths && shots.Count > 1 && !showOnlySelectedShot)
        {
            Gizmos.color = Color.white * 0.5f;
            for (int i = 0; i < shots.Count - 1; i++)
            {
                if (shots[i].startPosition != null && shots[i + 1].startPosition != null)
                {
                    Gizmos.DrawLine(shots[i].startPosition.position, shots[i + 1].startPosition.position);
                }
            }
        }
        
        // Draw selection info
        if (selectedShotIndex >= 0 && selectedShotIndex < shots.Count)
        {
            #if UNITY_EDITOR
            CameraShot selectedShot = shots[selectedShotIndex];
            if (selectedShot.startPosition != null)
            {
                UnityEditor.Handles.color = Color.cyan;
                Vector3 labelPos = selectedShot.startPosition.position + Vector3.up * 1.5f;
                UnityEditor.Handles.Label(labelPos, $"SELECTED: {selectedShot.shotName}\nType: {selectedShot.shotType}\nDuration: {selectedShot.duration}s");
            }
            #endif
        }
    }
    
    private void DrawCameraVisualization(CameraShot shot, Transform cameraTransform, Color color, string label, float sizeMultiplier = 1.0f, bool isSelected = false)
    {
        Vector3 cameraPos = cameraTransform.position;
        Quaternion cameraRot = GetCameraRotationForVisualization(shot, cameraTransform);
        
        Gizmos.color = color;
        
        // Draw camera frustum if enabled
        if (showCameraFrustum)
        {
            DrawCameraFrustum(cameraPos, cameraRot, color, sizeMultiplier);
        }
        
        // Draw look direction if enabled
        if (showLookDirections)
        {
            Vector3 lookDirection = cameraRot * Vector3.forward;
            Gizmos.color = color;
            float arrowLength = lookDirectionLength * sizeMultiplier;
            Gizmos.DrawRay(cameraPos, lookDirection * arrowLength);
            
            // Draw arrowhead (larger if selected)
            Vector3 arrowHead = cameraPos + lookDirection * arrowLength;
            float arrowSize = 0.2f * sizeMultiplier;
            Vector3 right = cameraRot * Vector3.right * arrowSize;
            Vector3 up = cameraRot * Vector3.up * arrowSize;
            
            Gizmos.DrawLine(arrowHead, arrowHead - lookDirection * (0.3f * sizeMultiplier) + right);
            Gizmos.DrawLine(arrowHead, arrowHead - lookDirection * (0.3f * sizeMultiplier) - right);
            Gizmos.DrawLine(arrowHead, arrowHead - lookDirection * (0.3f * sizeMultiplier) + up);
            Gizmos.DrawLine(arrowHead, arrowHead - lookDirection * (0.3f * sizeMultiplier) - up);
        }
        
        // Draw target connections if enabled
        if (showTargetConnections)
        {
            DrawTargetConnections(shot, cameraPos, color, sizeMultiplier);
        }
        
        // Label (highlighted if selected)
        #if UNITY_EDITOR
        UnityEditor.Handles.color = isSelected ? Color.white : color;
        float labelOffset = 0.5f * sizeMultiplier;
        UnityEditor.Handles.Label(cameraPos + Vector3.up * labelOffset, label);
        #endif
    }
    
    private void DrawCameraFrustum(Vector3 position, Quaternion rotation, Color color, float sizeMultiplier = 1.0f)
    {
        // Simple frustum representation
        float size = frustumSize * sizeMultiplier;
        float distance = lookDirectionLength * sizeMultiplier;
        
        Vector3 forward = rotation * Vector3.forward;
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;
        
        // Near plane corners
        Vector3 nearTL = position + forward * 0.5f + up * (size * 0.3f) - right * (size * 0.4f);
        Vector3 nearTR = position + forward * 0.5f + up * (size * 0.3f) + right * (size * 0.4f);
        Vector3 nearBL = position + forward * 0.5f - up * (size * 0.3f) - right * (size * 0.4f);
        Vector3 nearBR = position + forward * 0.5f - up * (size * 0.3f) + right * (size * 0.4f);
        
        // Far plane corners
        Vector3 farTL = position + forward * distance + up * size - right * (size * 1.33f);
        Vector3 farTR = position + forward * distance + up * size + right * (size * 1.33f);
        Vector3 farBL = position + forward * distance - up * size - right * (size * 1.33f);
        Vector3 farBR = position + forward * distance - up * size + right * (size * 1.33f);
        
        Gizmos.color = color * 0.6f;
        
        // Near plane
        Gizmos.DrawLine(nearTL, nearTR);
        Gizmos.DrawLine(nearTR, nearBR);
        Gizmos.DrawLine(nearBR, nearBL);
        Gizmos.DrawLine(nearBL, nearTL);
        
        // Far plane
        Gizmos.DrawLine(farTL, farTR);
        Gizmos.DrawLine(farTR, farBR);
        Gizmos.DrawLine(farBR, farBL);
        Gizmos.DrawLine(farBL, farTL);
        
        // Connecting lines
        Gizmos.DrawLine(nearTL, farTL);
        Gizmos.DrawLine(nearTR, farTR);
        Gizmos.DrawLine(nearBL, farBL);
        Gizmos.DrawLine(nearBR, farBR);
    }
    
    private void DrawTargetConnections(CameraShot shot, Vector3 cameraPos, Color color, float sizeMultiplier = 1.0f)
    {
        Gizmos.color = color * 0.8f;
        
        // Look at target connection
        if (shot.useLookAt && shot.lookAtTarget != null)
        {
            Gizmos.color = Color.green * 0.8f;
            Gizmos.DrawLine(cameraPos, shot.lookAtTarget.position);
            Gizmos.DrawWireSphere(shot.lookAtTarget.position, 0.2f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.Label(shot.lookAtTarget.position + Vector3.up * 0.3f, "Look Target");
            #endif
        }
        
        // Pan targets
        if (shot.panStartTarget != null && shot.panEndTarget != null)
        {
            Gizmos.color = Color.red * 0.8f;
            Gizmos.DrawLine(cameraPos, shot.panStartTarget.position);
            Gizmos.DrawLine(cameraPos, shot.panEndTarget.position);
            Gizmos.DrawWireSphere(shot.panStartTarget.position, 0.15f);
            Gizmos.DrawWireSphere(shot.panEndTarget.position, 0.15f);
            Gizmos.DrawLine(shot.panStartTarget.position, shot.panEndTarget.position);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.Label(shot.panStartTarget.position + Vector3.up * 0.2f, "Pan Start");
            UnityEditor.Handles.Label(shot.panEndTarget.position + Vector3.up * 0.2f, "Pan End");
            #endif
        }
        
        // Orbit center
        if (shot.shotType == CameraShot.ShotType.OrbitAround && shot.orbitCenter != null)
        {
            Gizmos.color = Color.magenta * 0.8f;
            Gizmos.DrawLine(cameraPos, shot.orbitCenter.position);
            Gizmos.DrawWireSphere(shot.orbitCenter.position, shot.orbitRadius);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.magenta;
            UnityEditor.Handles.Label(shot.orbitCenter.position + Vector3.up * 0.3f, "Orbit Center");
            #endif
        }
    }
    
    private Quaternion GetCameraRotationForVisualization(CameraShot shot, Transform cameraTransform)
    {
        // Return the appropriate rotation based on shot settings
        switch (shot.rotationType)
        {
            case CameraShot.RotationType.UseTransformRotation:
                return cameraTransform.rotation;
                
            case CameraShot.RotationType.LookAtTarget:
                if (shot.lookAtTarget != null)
                {
                    Vector3 direction = (shot.lookAtTarget.position - cameraTransform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(direction);
                        if (shot.lookAtRotationOffset != Vector3.zero)
                        {
                            lookRot *= Quaternion.Euler(shot.lookAtRotationOffset);
                        }
                        return lookRot;
                    }
                }
                return cameraTransform.rotation;
                
            case CameraShot.RotationType.CustomEulerAngles:
                if (shot.useLocalRotation)
                {
                    return cameraTransform.rotation * Quaternion.Euler(shot.customRotationStart);
                }
                else
                {
                    return Quaternion.Euler(shot.customRotationStart);
                }
                
            case CameraShot.RotationType.FollowMovementDirection:
                if (shot.endPosition != null)
                {
                    Vector3 moveDirection = (shot.endPosition.position - shot.startPosition.position).normalized;
                    if (moveDirection != Vector3.zero)
                    {
                        return Quaternion.LookRotation(moveDirection);
                    }
                }
                return cameraTransform.rotation;
                
            case CameraShot.RotationType.KeepCurrentRotation:
            default:
                return cameraTransform.rotation;
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
            case CameraShot.ShotType.FreeRotation: return new Color(1f, 0.5f, 0f); // Orange
            case CameraShot.ShotType.LookAtOnly: return Color.white;
            case CameraShot.ShotType.CustomSequence: return Color.gray;
            default: return Color.white;
        }
    }
    
    private void DrawShotSpecificGizmos(CameraShot shot, int index, bool isSelected = false)
    {
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
                    
                    // Draw camera visualization at each dolly point
                    if (showCameraFrustum || showLookDirections)
                    {
                        Color dollyColor = Color.cyan * 0.5f;
                        Quaternion dollyRot = GetCameraRotationForVisualization(shot, shot.dollyPath[i]);
                        if (showLookDirections)
                        {
                            Vector3 lookDir = dollyRot * Vector3.forward;
                            Gizmos.color = dollyColor;
                            Gizmos.DrawRay(shot.dollyPath[i].position, lookDir * (lookDirectionLength * 0.5f));
                        }
                    }
                }
            }
            if (shot.dollyPath[shot.dollyPath.Count - 1] != null)
            {
                Gizmos.DrawWireSphere(shot.dollyPath[shot.dollyPath.Count - 1].position, 0.1f);
            }
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.Label(shot.dollyPath[0].position + Vector3.up * 0.3f, $"Dolly Path {shot.dollyPath.Count} points");
            #endif
        }
    }
    
    #endregion
}