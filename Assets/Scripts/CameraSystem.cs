using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CinemachinePanner : MonoBehaviour
{
    [Header("Camera Setup")]
    public CinemachineCamera virtualCamera;
    
    [Header("Dolly Track (Camera Movement)")]
    public List<Transform> dollyPoints = new List<Transform>();
    
    [Header("Pan Targets (Camera Look At)")]
    public List<Transform> panTargets = new List<Transform>();
    
    [Header("Legacy Pan Points (for backward compatibility)")]
    public List<Transform> panPoints = new List<Transform>();
    
    [Header("Movement Settings")]
    public float movementDuration = 3f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Pan Settings")]
    public float panDuration = 2f;
    public AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Sequence Settings")]
    public bool autoStart = false;
    public bool loopPanning = false;
    public SequenceMode sequenceMode = SequenceMode.LegacyPanPoints;
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    public enum SequenceMode
    {
        LegacyPanPoints,    // Use original pan points system
        DollyOnly,          // Just move camera along dolly track
        PanOnly,            // Just pan between targets
        DollyWithPan,       // Move along dolly while panning through targets
        SyncedDollyPan      // Synchronized - each dolly point corresponds to a pan target
    }
    
    private int currentPointIndex = 0;
    private int currentDollyIndex = 0;
    private int currentPanIndex = 0;
    private bool isPanning = false;
    
    void Start()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineCamera>();
            
        if (autoStart)
        {
            StartSequence();
        }
    }
    
    public void StartSequence()
    {
        if (isPanning) return;
        
        switch (sequenceMode)
        {
            case SequenceMode.LegacyPanPoints:
                if (panPoints.Count > 1)
                    StartCoroutine(PanThroughPoints());
                break;
            case SequenceMode.DollyOnly:
                if (dollyPoints.Count > 1)
                    StartCoroutine(DollySequence());
                break;
            case SequenceMode.PanOnly:
                if (panTargets.Count > 1)
                    StartCoroutine(PanSequence());
                break;
            case SequenceMode.DollyWithPan:
                if (dollyPoints.Count > 1 && panTargets.Count > 1)
                    StartCoroutine(DollyWithPanSequence());
                break;
            case SequenceMode.SyncedDollyPan:
                if (dollyPoints.Count > 1 && panTargets.Count > 1)
                    StartCoroutine(SyncedDollyPanSequence());
                break;
        }
    }
    
    public void StartPanning()
    {
        if (panPoints.Count > 1 && !isPanning)
        {
            StartCoroutine(PanThroughPoints());
        }
    }
    
    public void PanToPoint(int index)
    {
        if (index >= 0 && index < panPoints.Count && !isPanning)
        {
            currentPointIndex = index;
            StartCoroutine(PanToSinglePoint(panPoints[index]));
        }
    }
    
    private IEnumerator PanThroughPoints()
    {
        isPanning = true;
        
        do
        {
            for (int i = 0; i < panPoints.Count - 1; i++)
            {
                yield return StartCoroutine(PanBetweenPoints(panPoints[i], panPoints[i + 1]));
                currentPointIndex = i + 1;
            }
            
            if (loopPanning && panPoints.Count > 0)
            {
                yield return StartCoroutine(PanBetweenPoints(panPoints[panPoints.Count - 1], panPoints[0]));
                currentPointIndex = 0;
            }
            
        } while (loopPanning);
        
        isPanning = false;
    }
    
    private IEnumerator PanToSinglePoint(Transform target)
    {
        isPanning = true;
        
        Transform cameraTransform = virtualCamera.transform;
        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;
        
        float elapsed = 0f;
        
        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = panCurve.Evaluate(elapsed / panDuration);
            
            cameraTransform.position = Vector3.Lerp(startPos, target.position, t);
            cameraTransform.rotation = Quaternion.Lerp(startRot, target.rotation, t);
            
            yield return null;
        }
        
        cameraTransform.position = target.position;
        cameraTransform.rotation = target.rotation;
        
        isPanning = false;
    }
    
    private IEnumerator PanBetweenPoints(Transform from, Transform to)
    {
        Transform cameraTransform = virtualCamera.transform;
        
        float elapsed = 0f;
        
        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = panCurve.Evaluate(elapsed / panDuration);
            
            cameraTransform.position = Vector3.Lerp(from.position, to.position, t);
            cameraTransform.rotation = Quaternion.Lerp(from.rotation, to.rotation, t);
            
            yield return null;
        }
        
        cameraTransform.position = to.position;
        cameraTransform.rotation = to.rotation;
    }
    
    [ContextMenu("Stop Panning")]
    public void StopPanning()
    {
        StopAllCoroutines();
        isPanning = false;
    }
    
    // Dolly System Methods
    public void MoveToDollyPoint(int index)
    {
        if (index >= 0 && index < dollyPoints.Count && !isPanning)
        {
            StartCoroutine(MoveCameraTo(dollyPoints[index].position, dollyPoints[index].rotation, movementDuration));
        }
    }
    
    public void PanToTarget(int index)
    {
        if (index >= 0 && index < panTargets.Count && !isPanning)
        {
            StartCoroutine(PanCameraTo(panTargets[index], panDuration));
        }
    }
    
    private IEnumerator DollySequence()
    {
        isPanning = true;
        
        do
        {
            for (int i = 0; i < dollyPoints.Count - 1; i++)
            {
                yield return StartCoroutine(MoveBetweenDollyPoints(i, i + 1));
                currentDollyIndex = i + 1;
            }
            
            if (loopPanning)
            {
                yield return StartCoroutine(MoveBetweenDollyPoints(dollyPoints.Count - 1, 0));
                currentDollyIndex = 0;
            }
            
        } while (loopPanning);
        
        isPanning = false;
    }
    
    private IEnumerator PanSequence()
    {
        isPanning = true;
        
        do
        {
            for (int i = 0; i < panTargets.Count - 1; i++)
            {
                yield return StartCoroutine(PanBetweenTargets(i, i + 1));
                currentPanIndex = i + 1;
            }
            
            if (loopPanning)
            {
                yield return StartCoroutine(PanBetweenTargets(panTargets.Count - 1, 0));
                currentPanIndex = 0;
            }
            
        } while (loopPanning);
        
        isPanning = false;
    }
    
    private IEnumerator DollyWithPanSequence()
    {
        isPanning = true;
        
        do
        {
            // Start both dolly and pan movements simultaneously
            Coroutine dollyCoroutine = StartCoroutine(DollyMovementLoop());
            Coroutine panCoroutine = StartCoroutine(PanMovementLoop());
            
            yield return dollyCoroutine;
            yield return panCoroutine;
            
        } while (loopPanning);
        
        isPanning = false;
    }
    
    private IEnumerator SyncedDollyPanSequence()
    {
        isPanning = true;
        
        int maxPoints = Mathf.Min(dollyPoints.Count, panTargets.Count);
        
        do
        {
            for (int i = 0; i < maxPoints - 1; i++)
            {
                // Move dolly and pan simultaneously to synchronized points
                Coroutine dollyMove = StartCoroutine(MoveBetweenDollyPoints(i, i + 1));
                Coroutine panMove = StartCoroutine(PanBetweenTargets(i, i + 1));
                
                yield return dollyMove;
                yield return panMove;
                
                currentDollyIndex = i + 1;
                currentPanIndex = i + 1;
            }
            
            if (loopPanning)
            {
                Coroutine dollyMove = StartCoroutine(MoveBetweenDollyPoints(maxPoints - 1, 0));
                Coroutine panMove = StartCoroutine(PanBetweenTargets(maxPoints - 1, 0));
                
                yield return dollyMove;
                yield return panMove;
                
                currentDollyIndex = 0;
                currentPanIndex = 0;
            }
            
        } while (loopPanning);
        
        isPanning = false;
    }
    
    private IEnumerator DollyMovementLoop()
    {
        for (int i = 0; i < dollyPoints.Count - 1; i++)
        {
            yield return StartCoroutine(MoveBetweenDollyPoints(i, i + 1));
            currentDollyIndex = i + 1;
        }
    }
    
    private IEnumerator PanMovementLoop()
    {
        for (int i = 0; i < panTargets.Count - 1; i++)
        {
            yield return StartCoroutine(PanBetweenTargets(i, i + 1));
            currentPanIndex = i + 1;
        }
    }
    
    private IEnumerator MoveBetweenDollyPoints(int fromIndex, int toIndex)
    {
        Transform from = dollyPoints[fromIndex];
        Transform to = dollyPoints[toIndex];
        
        float elapsed = 0f;
        
        while (elapsed < movementDuration)
        {
            elapsed += Time.deltaTime;
            float t = movementCurve.Evaluate(elapsed / movementDuration);
            
            virtualCamera.transform.position = Vector3.Lerp(from.position, to.position, t);
            
            // Only use dolly rotation if not panning to targets
            if (sequenceMode == SequenceMode.DollyOnly)
            {
                virtualCamera.transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, t);
            }
            
            yield return null;
        }
        
        virtualCamera.transform.position = to.position;
        if (sequenceMode == SequenceMode.DollyOnly)
        {
            virtualCamera.transform.rotation = to.rotation;
        }
    }
    
    private IEnumerator PanBetweenTargets(int fromIndex, int toIndex)
    {
        Transform from = panTargets[fromIndex];
        Transform to = panTargets[toIndex];
        
        float elapsed = 0f;
        
        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = panCurve.Evaluate(elapsed / panDuration);
            
            Vector3 targetPos = Vector3.Lerp(from.position, to.position, t);
            virtualCamera.transform.LookAt(targetPos);
            
            yield return null;
        }
        
        virtualCamera.transform.LookAt(to.position);
    }
    
    private IEnumerator MoveCameraTo(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isPanning = true;
        
        Vector3 startPos = virtualCamera.transform.position;
        Quaternion startRot = virtualCamera.transform.rotation;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = movementCurve.Evaluate(elapsed / duration);
            
            virtualCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            virtualCamera.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            
            yield return null;
        }
        
        virtualCamera.transform.position = targetPos;
        virtualCamera.transform.rotation = targetRot;
        
        isPanning = false;
    }
    
    private IEnumerator PanCameraTo(Transform target, float duration)
    {
        isPanning = true;
        
        Quaternion startRot = virtualCamera.transform.rotation;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = panCurve.Evaluate(elapsed / duration);
            
            Vector3 directionToTarget = (target.position - virtualCamera.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            virtualCamera.transform.rotation = Quaternion.Lerp(startRot, targetRotation, t);
            
            yield return null;
        }
        
        virtualCamera.transform.LookAt(target.position);
        
        isPanning = false;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw dolly track
        Gizmos.color = Color.blue;
        for (int i = 0; i < dollyPoints.Count; i++)
        {
            if (dollyPoints[i] != null)
            {
                Gizmos.DrawWireSphere(dollyPoints[i].position, 0.3f);
                
                if (i < dollyPoints.Count - 1 && dollyPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(dollyPoints[i].position, dollyPoints[i + 1].position);
                }
            }
        }
        
        // Draw pan targets
        Gizmos.color = Color.red;
        for (int i = 0; i < panTargets.Count; i++)
        {
            if (panTargets[i] != null)
            {
                Gizmos.DrawWireCube(panTargets[i].position, Vector3.one * 0.5f);
            }
        }
        
        // Draw legacy pan points
        Gizmos.color = Color.yellow;
        for (int i = 0; i < panPoints.Count; i++)
        {
            if (panPoints[i] != null)
            {
                Gizmos.DrawWireSphere(panPoints[i].position, 0.2f);
                
                if (i < panPoints.Count - 1 && panPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(panPoints[i].position, panPoints[i + 1].position);
                }
            }
        }
        
        // Draw look direction if camera exists
        if (virtualCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(virtualCamera.transform.position, virtualCamera.transform.forward * 3f);
        }
    }
}