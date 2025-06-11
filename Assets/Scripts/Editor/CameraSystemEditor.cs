using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(CameraSystem))]
public class CameraSystemEditor : Editor
{
    private CameraSystem cameraSystem;
    
    void OnEnable()
    {
        cameraSystem = (CameraSystem)target;
    }
    
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Add shot selection controls
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shot Selection Controls", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Previous shot button
        if (GUILayout.Button("◀ Previous", GUILayout.Width(80)))
        {
            cameraSystem.SelectPreviousShot();
        }
        
        // Shot info display
        string shotInfo = "No Shot Selected";
        if (cameraSystem.selectedShotIndex >= 0 && cameraSystem.selectedShotIndex < cameraSystem.shots.Count)
        {
            var shot = cameraSystem.shots[cameraSystem.selectedShotIndex];
            shotInfo = $"Shot {cameraSystem.selectedShotIndex + 1}: {shot.shotName}";
        }
        
        GUILayout.Label(shotInfo, EditorStyles.centeredGreyMiniLabel);
        
        // Next shot button
        if (GUILayout.Button("Next ▶", GUILayout.Width(80)))
        {
            cameraSystem.SelectNextShot();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Action buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Jump to Shot"))
        {
            cameraSystem.JumpToSelectedShot();
        }
        
        if (GUILayout.Button("Preview Shot"))
        {
            cameraSystem.PreviewSelectedShot();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Quick shot selection dropdown
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Selection", EditorStyles.boldLabel);
        
        if (cameraSystem.shots.Count > 0)
        {
            string[] shotNames = new string[cameraSystem.shots.Count + 1];
            shotNames[0] = "None Selected";
            
            for (int i = 0; i < cameraSystem.shots.Count; i++)
            {
                shotNames[i + 1] = $"Shot {i + 1}: {cameraSystem.shots[i].shotName}";
            }
            
            int currentIndex = cameraSystem.selectedShotIndex + 1;
            int newIndex = EditorGUILayout.Popup("Select Shot", currentIndex, shotNames);
            
            if (newIndex != currentIndex)
            {
                cameraSystem.selectedShotIndex = newIndex - 1;
                if (cameraSystem.selectedShotIndex >= 0)
                {
                    cameraSystem.SelectShot(cameraSystem.selectedShotIndex);
                }
            }
        }
        
        // Keyboard shortcuts info
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Keyboard Shortcuts:\n" +
            "• Page Up/Down - Navigate shots\n" +
            "• Space - Preview selected shot\n" +
            "• Enter - Jump to selected shot", 
            MessageType.Info);
    }
    
    void OnSceneGUI()
    {
        // Handle keyboard shortcuts in Scene view
        Event e = Event.current;
        
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.PageUp:
                    cameraSystem.SelectPreviousShot();
                    e.Use();
                    break;
                    
                case KeyCode.PageDown:
                    cameraSystem.SelectNextShot();
                    e.Use();
                    break;
                    
                case KeyCode.Space:
                    if (!Application.isPlaying)
                    {
                        cameraSystem.PreviewSelectedShot();
                        e.Use();
                    }
                    break;
                    
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    cameraSystem.JumpToSelectedShot();
                    e.Use();
                    break;
            }
        }
        
        // Draw shot selection handles in Scene view
        if (cameraSystem.selectedShotIndex >= 0 && cameraSystem.selectedShotIndex < cameraSystem.shots.Count)
        {
            var selectedShot = cameraSystem.shots[cameraSystem.selectedShotIndex];
            if (selectedShot.startPosition != null)
            {
                // Draw a special handle for the selected shot
                Handles.color = Color.cyan;
                Vector3 position = selectedShot.startPosition.position;
                
                // Draw selection ring
                Handles.DrawWireArc(position, Vector3.up, Vector3.forward, 360f, 1f);
                
                // Draw shot index label
                Handles.Label(position + Vector3.up * 2f, 
                    $"SELECTED SHOT {cameraSystem.selectedShotIndex + 1}\n{selectedShot.shotName}",
                    EditorStyles.whiteLabel);
            }
        }
    }
}
#endif
