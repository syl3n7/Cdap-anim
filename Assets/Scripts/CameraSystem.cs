using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CinemachinePanner : MonoBehaviour
{
    [Header("Camera Setup")]
    public CinemachineVirtualCamera virtualCamera;
    
    [Header("Pan Points")]
    public List<Transform> panPoints = new List<Transform>();
    
    [Header("Settings")]
    public float panDuration = 2f;
    public AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool autoStart = false;
    public bool loopPanning = false;
    
    private int currentPointIndex = 0;
    private bool isPanning = false;
    
    void Start()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            
        if (autoStart && panPoints.Count > 0)
        {
            StartPanning();
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
}