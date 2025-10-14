using UnityEngine;

/// <summary>
/// Helper script with utility functions for testing and debugging transitions
/// Attach to an empty GameObject in your scene for quick testing
/// </summary>
public class TransitionTester : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode testMenuToMapKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode testMapToTowerKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode testTowerToMapKey = KeyCode.Alpha3;
    
    [Header("References")]
    [SerializeField] private MenuToMapTransition menuTransition;
    [SerializeField] private MapTowerTransition mapTowerTransition;

    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;

    private void Update()
    {
        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(testMenuToMapKey))
        {
            TestMenuToMap();
        }
        
        if (Input.GetKeyDown(testMapToTowerKey))
        {
            TestMapToTower();
        }
        
        if (Input.GetKeyDown(testTowerToMapKey))
        {
            TestTowerToMap();
        }

        // Debug display
        if (showDebugInfo)
        {
            DisplayDebugInfo();
        }
    }

    private void TestMenuToMap()
    {
        if (menuTransition == null)
        {
            Debug.LogWarning("MenuToMapTransition not assigned!");
            return;
        }

        Debug.Log("Testing Menu → Map transition");
        SceneTransitionManager.Instance.TransitionMenuToMap(2.0f, () => {
            Debug.Log("Menu → Map transition complete!");
        });
    }

    private void TestMapToTower()
    {
        if (mapTowerTransition == null)
        {
            Debug.LogWarning("MapTowerTransition not assigned!");
            return;
        }

        Debug.Log("Testing Map → Tower transition");
        SceneTransitionManager.Instance.TransitionMapToTower(Vector3.zero, 1.0f, () => {
            Debug.Log("Map → Tower transition complete!");
        });
    }

    private void TestTowerToMap()
    {
        if (mapTowerTransition == null)
        {
            Debug.LogWarning("MapTowerTransition not assigned!");
            return;
        }

        Debug.Log("Testing Tower → Map transition");
        SceneTransitionManager.Instance.TransitionTowerToMap(0.8f, () => {
            Debug.Log("Tower → Map transition complete!");
        });
    }

    private void DisplayDebugInfo()
    {
        // This will be visible in Scene view
        string info = $"Scene: {SceneTransitionManager.Instance.CurrentScene}\n" +
                      $"Transitioning: {SceneTransitionManager.Instance.IsTransitioning}\n" +
                      $"Press {testMenuToMapKey} for Menu→Map\n" +
                      $"Press {testMapToTowerKey} for Map→Tower\n" +
                      $"Press {testTowerToMapKey} for Tower→Map";
        
        Debug.DrawRay(Vector3.zero, Vector3.up * 5f, Color.green);
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== Transition Tester ===");
        GUILayout.Label($"Current Scene: {SceneTransitionManager.Instance.CurrentScene}");
        GUILayout.Label($"Is Transitioning: {SceneTransitionManager.Instance.IsTransitioning}");
        GUILayout.Space(10);
        
        GUILayout.Label("Keyboard Shortcuts:");
        GUILayout.Label($"{testMenuToMapKey} - Menu → Map");
        GUILayout.Label($"{testMapToTowerKey} - Map → Tower");
        GUILayout.Label($"{testTowerToMapKey} - Tower → Map");
        
        GUILayout.Space(10);
        
        // Buttons for testing
        GUI.enabled = !SceneTransitionManager.Instance.IsTransitioning;
        
        if (GUILayout.Button("Test Menu → Map"))
        {
            TestMenuToMap();
        }
        
        if (GUILayout.Button("Test Map → Tower"))
        {
            TestMapToTower();
        }
        
        if (GUILayout.Button("Test Tower → Map"))
        {
            TestTowerToMap();
        }
        
        GUI.enabled = true;
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    // Auto-find components
    private void Reset()
    {
        menuTransition = FindObjectOfType<MenuToMapTransition>();
        mapTowerTransition = FindObjectOfType<MapTowerTransition>();
    }
}
