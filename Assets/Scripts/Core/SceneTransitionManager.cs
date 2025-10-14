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

        // Find the menu transition controller in the scene
        MenuToMapTransition menuTransition = FindObjectOfType<MenuToMapTransition>();
        
        if (menuTransition != null)
        {
            yield return menuTransition.PlayTransition(duration);
        }
        else
        {
            Debug.LogError("MenuToMapTransition component not found in scene!");
            yield return new WaitForSeconds(duration);
        }

        // Load the Map scene (or activate it if using single scene approach)
        // For now, we'll use scene loading
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
        MapTowerTransition mapTransition = FindObjectOfType<MapTowerTransition>();
        
        if (mapTransition != null)
        {
            yield return mapTransition.TransitionToTower(towerPosition, duration);
        }
        else
        {
            Debug.LogError("MapTowerTransition component not found in scene!");
            yield return new WaitForSeconds(duration);
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

        // Find the map to tower transition controller
        MapTowerTransition mapTransition = FindObjectOfType<MapTowerTransition>();
        
        if (mapTransition != null)
        {
            yield return mapTransition.TransitionToMap(duration);
        }
        else
        {
            Debug.LogError("MapTowerTransition component not found in scene!");
            yield return new WaitForSeconds(duration);
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
