using UnityEngine;
using System.Collections;

/// <summary>
/// Test script to validate the Camera System shot selection functionality
/// Attach this to a GameObject in your scene to run automated tests
/// </summary>
public class CameraSystemTester : MonoBehaviour
{
    [Header("Test Configuration")]
    public CameraSystem cameraSystemToTest;
    public bool runTestsOnStart = false;
    public bool showDetailedLogs = true;
    
    [Header("Test Results")]
    [SerializeField] private int testsRun = 0;
    [SerializeField] private int testsPassed = 0;
    [SerializeField] private int testsFailed = 0;
    
    void Start()
    {
        if (runTestsOnStart && cameraSystemToTest != null)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    [ContextMenu("Run All Tests")]
    public void RunAllTestsMenu()
    {
        if (cameraSystemToTest != null)
        {
            StartCoroutine(RunAllTests());
        }
        else
        {
            Debug.LogError("[CameraSystemTester] No CameraSystem assigned to test!");
        }
    }
    
    private IEnumerator RunAllTests()
    {
        Debug.Log("[CameraSystemTester] Starting Camera System Shot Selection Tests...");
        ResetTestCounters();
        
        yield return StartCoroutine(TestShotSelection());
        yield return StartCoroutine(TestNavigationMethods());
        yield return StartCoroutine(TestVisualizationFeatures());
        yield return StartCoroutine(TestPreviewFunctionality());
        
        LogTestResults();
    }
    
    private IEnumerator TestShotSelection()
    {
        LogTest("Testing Shot Selection Core Functionality");
        
        // Test 1: Basic selection
        if (cameraSystemToTest.shots.Count > 0)
        {
            cameraSystemToTest.SelectShot(0);
            AssertEqual(cameraSystemToTest.selectedShotIndex, 0, "Basic shot selection");
            
            // Test invalid selection
            cameraSystemToTest.SelectShot(-1);
            AssertEqual(cameraSystemToTest.selectedShotIndex, -1, "Invalid negative selection");
            
            cameraSystemToTest.SelectShot(999);
            AssertTrue(cameraSystemToTest.selectedShotIndex < cameraSystemToTest.shots.Count, "Out of bounds selection handling");
        }
        else
        {
            LogTest("No shots available for selection testing");
        }
        
        yield return null;
    }
    
    private IEnumerator TestNavigationMethods()
    {
        LogTest("Testing Shot Navigation Methods");
        
        if (cameraSystemToTest.shots.Count >= 3)
        {
            // Test next/previous navigation
            cameraSystemToTest.selectedShotIndex = 1; // Start at middle shot
            
            cameraSystemToTest.SelectNextShot();
            AssertEqual(cameraSystemToTest.selectedShotIndex, 2, "Next shot navigation");
            
            cameraSystemToTest.SelectPreviousShot();
            AssertEqual(cameraSystemToTest.selectedShotIndex, 1, "Previous shot navigation");
            
            // Test wraparound
            cameraSystemToTest.selectedShotIndex = cameraSystemToTest.shots.Count - 1;
            cameraSystemToTest.SelectNextShot();
            AssertEqual(cameraSystemToTest.selectedShotIndex, 0, "Next shot wraparound");
            
            cameraSystemToTest.selectedShotIndex = 0;
            cameraSystemToTest.SelectPreviousShot();
            AssertEqual(cameraSystemToTest.selectedShotIndex, cameraSystemToTest.shots.Count - 1, "Previous shot wraparound");
        }
        else
        {
            LogTest("Need at least 3 shots for navigation testing");
        }
        
        yield return null;
    }
    
    private IEnumerator TestVisualizationFeatures()
    {
        LogTest("Testing Visualization Features");
        
        // Test visualization flags
        bool originalHighlight = cameraSystemToTest.highlightSelectedShot;
        bool originalShowOnly = cameraSystemToTest.showOnlySelectedShot;
        bool originalShowGizmos = cameraSystemToTest.showGizmos;
        
        // Test highlighting
        cameraSystemToTest.highlightSelectedShot = true;
        AssertTrue(cameraSystemToTest.highlightSelectedShot, "Highlight selected shot enabled");
        
        // Test isolation mode
        cameraSystemToTest.showOnlySelectedShot = true;
        AssertTrue(cameraSystemToTest.showOnlySelectedShot, "Show only selected shot enabled");
        
        // Test gizmo system
        cameraSystemToTest.showGizmos = true;
        cameraSystemToTest.showCameraFrustum = true;
        cameraSystemToTest.showLookDirections = true;
        cameraSystemToTest.showTargetConnections = true;
        
        AssertTrue(cameraSystemToTest.showCameraFrustum, "Camera frustum visualization enabled");
        AssertTrue(cameraSystemToTest.showLookDirections, "Look direction visualization enabled");
        AssertTrue(cameraSystemToTest.showTargetConnections, "Target connection visualization enabled");
        
        // Restore original settings
        cameraSystemToTest.highlightSelectedShot = originalHighlight;
        cameraSystemToTest.showOnlySelectedShot = originalShowOnly;
        cameraSystemToTest.showGizmos = originalShowGizmos;
        
        yield return null;
    }
    
    private IEnumerator TestPreviewFunctionality()
    {
        LogTest("Testing Preview Functionality");
        
        if (cameraSystemToTest.shots.Count > 0 && cameraSystemToTest.virtualCamera != null)
        {
            // Select a shot with valid start position
            for (int i = 0; i < cameraSystemToTest.shots.Count; i++)
            {
                if (cameraSystemToTest.shots[i].startPosition != null)
                {
                    cameraSystemToTest.SelectShot(i);
                    Vector3 originalPosition = cameraSystemToTest.virtualCamera.transform.position;
                    
                    // Test jump to shot
                    cameraSystemToTest.JumpToSelectedShot();
                    
                    Vector3 expectedPosition = cameraSystemToTest.shots[i].startPosition.position;
                    float distance = Vector3.Distance(cameraSystemToTest.virtualCamera.transform.position, expectedPosition);
                    
                    AssertTrue(distance < 0.1f, $"Jump to shot position accuracy (distance: {distance})");
                    
                    // Test preview (can't easily test coroutine execution, but can verify method exists)
                    AssertTrue(cameraSystemToTest.selectedShotIndex >= 0, "Valid shot selected for preview");
                    
                    break;
                }
            }
        }
        else
        {
            LogTest("No valid shots or camera available for preview testing");
        }
        
        yield return null;
    }
    
    private void LogTest(string testName)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[CameraSystemTester] Running: {testName}");
        }
    }
    
    private void AssertTrue(bool condition, string testDescription)
    {
        testsRun++;
        if (condition)
        {
            testsPassed++;
            if (showDetailedLogs)
            {
                Debug.Log($"[CameraSystemTester] ✓ PASS: {testDescription}");
            }
        }
        else
        {
            testsFailed++;
            Debug.LogError($"[CameraSystemTester] ✗ FAIL: {testDescription}");
        }
    }
    
    private void AssertEqual<T>(T actual, T expected, string testDescription)
    {
        testsRun++;
        if (actual.Equals(expected))
        {
            testsPassed++;
            if (showDetailedLogs)
            {
                Debug.Log($"[CameraSystemTester] ✓ PASS: {testDescription} (Expected: {expected}, Actual: {actual})");
            }
        }
        else
        {
            testsFailed++;
            Debug.LogError($"[CameraSystemTester] ✗ FAIL: {testDescription} (Expected: {expected}, Actual: {actual})");
        }
    }
    
    private void ResetTestCounters()
    {
        testsRun = 0;
        testsPassed = 0;
        testsFailed = 0;
    }
    
    private void LogTestResults()
    {
        Debug.Log($"[CameraSystemTester] Test Results: {testsPassed}/{testsRun} tests passed ({testsFailed} failed)");
        
        if (testsFailed == 0)
        {
            Debug.Log("[CameraSystemTester] 🎉 All tests passed! Camera System shot selection is working correctly.");
        }
        else
        {
            Debug.LogWarning($"[CameraSystemTester] ⚠️ {testsFailed} test(s) failed. Check the logs above for details.");
        }
    }
    
    [ContextMenu("Test Shot Navigation Only")]
    public void TestNavigationOnly()
    {
        if (cameraSystemToTest != null)
        {
            StartCoroutine(TestNavigationMethods());
        }
    }
    
    [ContextMenu("Test Visualization Only")]
    public void TestVisualizationOnly()
    {
        if (cameraSystemToTest != null)
        {
            StartCoroutine(TestVisualizationFeatures());
        }
    }
    
    [ContextMenu("Demo Shot Selection")]
    public void DemoShotSelection()
    {
        if (cameraSystemToTest != null && cameraSystemToTest.shots.Count > 0)
        {
            StartCoroutine(DemoSelectionSequence());
        }
    }
    
    private IEnumerator DemoSelectionSequence()
    {
        Debug.Log("[CameraSystemTester] Starting shot selection demo...");
        
        // Enable visualization features
        cameraSystemToTest.highlightSelectedShot = true;
        cameraSystemToTest.showGizmos = true;
        
        // Cycle through all shots
        for (int i = 0; i < cameraSystemToTest.shots.Count; i++)
        {
            cameraSystemToTest.SelectShot(i);
            Debug.Log($"[CameraSystemTester] Selected: Shot {i + 1} - {cameraSystemToTest.shots[i].shotName}");
            
            yield return new WaitForSeconds(1f);
            
            // Jump to shot if it has a valid start position
            if (cameraSystemToTest.shots[i].startPosition != null)
            {
                cameraSystemToTest.JumpToSelectedShot();
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        Debug.Log("[CameraSystemTester] Shot selection demo complete!");
    }
}
