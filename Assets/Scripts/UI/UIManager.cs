using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages UI in Map and Tower scenes
/// Handles back button and other UI interactions
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Tower Scene UI")]
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject towerUI;

    [Header("Map Scene UI")]
    [SerializeField] private GameObject mapUI;

    private void Start()
    {
        // Setup button listeners
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // Subscribe to scene transition events
        SceneTransitionManager.Instance.OnSceneTransitionStarted += OnSceneTransitionStarted;
        SceneTransitionManager.Instance.OnSceneTransitionCompleted += OnSceneTransitionCompleted;

        // Initialize UI state
        UpdateUIState();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.OnSceneTransitionStarted -= OnSceneTransitionStarted;
            SceneTransitionManager.Instance.OnSceneTransitionCompleted -= OnSceneTransitionCompleted;
        }
    }

    private void OnBackButtonClicked()
    {
        if (SceneTransitionManager.Instance.IsTransitioning) return;

        // Return to map from tower
        if (SceneTransitionManager.Instance.CurrentScene == SceneTransitionManager.GameScene.Tower)
        {
            SceneTransitionManager.Instance.TransitionTowerToMap();
        }
    }

    private void OnSceneTransitionStarted(SceneTransitionManager.GameScene targetScene)
    {
        // Disable UI interactions during transition
        if (backButton != null)
        {
            backButton.interactable = false;
        }
    }

    private void OnSceneTransitionCompleted(SceneTransitionManager.GameScene currentScene)
    {
        // Re-enable UI after transition
        if (backButton != null)
        {
            backButton.interactable = true;
        }

        UpdateUIState();
    }

    private void UpdateUIState()
    {
        SceneTransitionManager.GameScene currentScene = SceneTransitionManager.Instance.CurrentScene;

        // Show/hide UI based on current scene
        if (mapUI != null)
        {
            mapUI.SetActive(currentScene == SceneTransitionManager.GameScene.Map);
        }

        if (towerUI != null)
        {
            towerUI.SetActive(currentScene == SceneTransitionManager.GameScene.Tower);
        }

        // Back button only visible in tower scene
        if (backButton != null)
        {
            backButton.gameObject.SetActive(currentScene == SceneTransitionManager.GameScene.Tower);
        }
    }
}
