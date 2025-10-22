using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager for handling all scene transitions
/// Prevents concurrent transitions and provides a clean API
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SceneTransitionManager");
                _instance = go.AddComponent<SceneTransitionManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public enum GameScene
    {
        Menu,
        Map,
        Tower
    }

    [Header("Transition Settings")]
    [SerializeField] private bool blockInputDuringTransition = true;
    
    private GameScene currentScene = GameScene.Menu;
    private bool isTransitioning = false;
    
    // Events for other systems to hook into
    public event Action<GameScene> OnSceneTransitionStarted;
    public event Action<GameScene> OnSceneTransitionCompleted;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Returns true if a transition is currently in progress
    /// </summary>
    public bool IsTransitioning => isTransitioning;

    /// <summary>
    /// Gets the current active scene
    /// </summary>
    public GameScene CurrentScene => currentScene;

    /// <summary>
    /// Transition from Menu to Map with obstacle animation
    /// Note: This is now handled directly by MenuToMapTransition via Start button click
    /// This method is kept for compatibility but does a simple scene load
    /// </summary>
    public void TransitionMenuToMap(float duration = 2.0f, Action onComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Transition already in progress!");
            return;
        }

        StartCoroutine(MenuToMapTransitionCoroutine(duration, onComplete));
    }

    /// <summary>
    /// Transition from Map to Tower with zoom animation
    /// </summary>
    public void TransitionMapToTower(Vector3 towerPosition, float duration = 1.0f, Action onComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Transition already in progress!");
            return;
        }

        StartCoroutine(MapToTowerTransitionCoroutine(towerPosition, duration, onComplete));
    }

    /// <summary>
    /// Transition from Tower back to Map with reverse zoom
    /// </summary>
    public void TransitionTowerToMap(float duration = 0.8f, Action onComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Transition already in progress!");
            return;
        }

        StartCoroutine(TowerToMapTransitionCoroutine(duration, onComplete));
    }

    private IEnumerator MenuToMapTransitionCoroutine(float duration, Action onComplete)
    {
        isTransitioning = true;
        OnSceneTransitionStarted?.Invoke(GameScene.Map);

        // MenuToMapTransition now handles its own animation and scene loading via button click
        // This fallback just waits and loads the scene
        yield return new WaitForSeconds(duration);

        // Load the Map scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MapScene");
        yield return asyncLoad;

        currentScene = GameScene.Map;
        isTransitioning = false;
        
        OnSceneTransitionCompleted?.Invoke(GameScene.Map);
        onComplete?.Invoke();
    }

    private IEnumerator MapToTowerTransitionCoroutine(Vector3 towerPosition, float duration, Action onComplete)
    {
        isTransitioning = true;
        OnSceneTransitionStarted?.Invoke(GameScene.Tower);

        // Find the map to tower transition controller
        MapToTowerTransition mapTransition = FindObjectOfType<MapToTowerTransition>();
        
        if (mapTransition != null)
        {
            // The MapToTowerTransition will handle loading the scene
            yield return new WaitForSeconds(duration);
        }
        else
        {
            Debug.LogError("MapToTowerTransition component not found in scene!");
            yield return new WaitForSeconds(duration);
            // Fallback: load scene directly
            SceneManager.LoadScene("TowerScene");
        }

        currentScene = GameScene.Tower;
        isTransitioning = false;
        
        OnSceneTransitionCompleted?.Invoke(GameScene.Tower);
        onComplete?.Invoke();
    }

    private IEnumerator TowerToMapTransitionCoroutine(float duration, Action onComplete)
    {
        isTransitioning = true;
        OnSceneTransitionStarted?.Invoke(GameScene.Map);

        // Use TowerToMapTransition for fade effect
        TowerToMapTransition towerTransition = FindObjectOfType<TowerToMapTransition>();
        
        if (towerTransition != null)
        {
            // TowerToMapTransition will handle the scene loading
            yield return new WaitForSeconds(duration);
        }
        else
        {
            Debug.LogError("TowerToMapTransition component not found in scene!");
            yield return new WaitForSeconds(duration);
            // Fallback: load scene directly
            SceneManager.LoadScene("MapScene");
        }

        currentScene = GameScene.Map;
        isTransitioning = false;
        
        OnSceneTransitionCompleted?.Invoke(GameScene.Map);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Block input during transitions if enabled
    /// </summary>
    public bool ShouldBlockInput()
    {
        return blockInputDuringTransition && isTransitioning;
    }
}
