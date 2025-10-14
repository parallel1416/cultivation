using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the seamless transition between Map and Tower views
/// Uses camera zoom and sprite crossfade for smooth transition
/// Recommended: Single scene approach with both views in same scene
/// </summary>
public class MapTowerTransition : TransitionAnimator
{
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float mapOrthographicSize = 5f;
    [SerializeField] private float towerOrthographicSize = 15f;

    [Header("View References")]
    [SerializeField] private GameObject mapView;
    [SerializeField] private GameObject towerView;
    [SerializeField] private SpriteRenderer towerPreviewSprite; // Small tower in map
    [SerializeField] private SpriteRenderer towerFullSprite;    // Large tower in tower view

    [Header("Tower Settings")]
    [SerializeField] private Vector3 towerViewPosition = new Vector3(0, 0, -10);
    [SerializeField] private Vector3 mapViewPosition = new Vector3(0, 0, -10);

    [Header("Crossfade Settings")]
    [SerializeField] private float crossfadeDuration = 0.4f;
    [SerializeField] private float crossfadeStartDelay = 0.4f; // Start crossfade partway through zoom

    private bool isInTowerView = false;

    protected override void Awake()
    {
        base.Awake();
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Initialize view states
        InitializeViews();
    }

    private void InitializeViews()
    {
        // Start in map view
        if (mapView != null) mapView.SetActive(true);
        if (towerView != null) towerView.SetActive(false);
        
        // Set initial sprite visibility
        if (towerPreviewSprite != null)
        {
            Color color = towerPreviewSprite.color;
            color.a = 1f;
            towerPreviewSprite.color = color;
        }
        
        if (towerFullSprite != null)
        {
            Color color = towerFullSprite.color;
            color.a = 0f;
            towerFullSprite.color = color;
        }

        // Set initial camera
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = mapOrthographicSize;
            mainCamera.transform.position = mapViewPosition;
        }

        isInTowerView = false;
    }

    /// <summary>
    /// Transition from Map to Tower view
    /// </summary>
    public IEnumerator TransitionToTower(Vector3 towerWorldPosition, float duration = 1.0f)
    {
        if (isInTowerView)
        {
            Debug.LogWarning("Already in Tower view!");
            yield break;
        }

        // Activate tower view (keep map active for crossfade)
        if (towerView != null)
        {
            towerView.SetActive(true);
        }

        // Calculate target camera position (keep tower centered)
        Vector3 targetCameraPos = towerViewPosition;

        // Start camera zoom and movement
        Coroutine cameraAnimation = StartCoroutine(AnimateCameraToTower(targetCameraPos, duration));
        
        // Start crossfade after delay
        yield return new WaitForSeconds(crossfadeStartDelay);
        Coroutine crossfade = StartCoroutine(CrossfadeToTower(crossfadeDuration));

        // Wait for camera animation to complete
        yield return cameraAnimation;
        yield return crossfade;

        // Deactivate map view to save performance
        if (mapView != null)
        {
            mapView.SetActive(false);
        }

        // Enable tower interactions
        TowerDragController dragController = FindObjectOfType<TowerDragController>();
        if (dragController != null)
        {
            dragController.enabled = true;
        }

        isInTowerView = true;
    }

    /// <summary>
    /// Transition from Tower back to Map view
    /// </summary>
    public IEnumerator TransitionToMap(float duration = 0.8f)
    {
        if (!isInTowerView)
        {
            Debug.LogWarning("Already in Map view!");
            yield break;
        }

        // Disable tower interactions
        TowerDragController dragController = FindObjectOfType<TowerDragController>();
        if (dragController != null)
        {
            dragController.enabled = false;
        }

        // Activate map view for crossfade
        if (mapView != null)
        {
            mapView.SetActive(true);
        }

        // Start reverse crossfade
        Coroutine crossfade = StartCoroutine(CrossfadeToMap(crossfadeDuration));
        
        // Start camera zoom out and movement
        yield return new WaitForSeconds(0.2f);
        Coroutine cameraAnimation = StartCoroutine(AnimateCameraToMap(duration));

        // Wait for animations to complete
        yield return cameraAnimation;
        yield return crossfade;

        // Deactivate tower view
        if (towerView != null)
        {
            towerView.SetActive(false);
        }

        isInTowerView = false;
    }

    private IEnumerator AnimateCameraToTower(Vector3 targetPosition, float duration)
    {
        if (mainCamera == null) yield break;

        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Animate camera position
            mainCamera.transform.position = CurveLerp(startPos, targetPosition, t, zoomCurve);
            
            // Animate orthographic size (zoom)
            mainCamera.orthographicSize = CurveLerp(startSize, towerOrthographicSize, t, zoomCurve);
            
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.orthographicSize = towerOrthographicSize;
    }

    private IEnumerator AnimateCameraToMap(float duration)
    {
        if (mainCamera == null) yield break;

        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Animate camera position
            mainCamera.transform.position = CurveLerp(startPos, mapViewPosition, t, zoomCurve);
            
            // Animate orthographic size (zoom out)
            mainCamera.orthographicSize = CurveLerp(startSize, mapOrthographicSize, t, zoomCurve);
            
            yield return null;
        }

        mainCamera.transform.position = mapViewPosition;
        mainCamera.orthographicSize = mapOrthographicSize;
    }

    private IEnumerator CrossfadeToTower(float duration)
    {
        // Fade out preview, fade in full tower
        Coroutine fadeOut = null;
        Coroutine fadeIn = null;

        if (towerPreviewSprite != null)
        {
            fadeOut = StartCoroutine(FadeSpriteRenderer(towerPreviewSprite, 1f, 0f, duration));
        }

        if (towerFullSprite != null)
        {
            fadeIn = StartCoroutine(FadeSpriteRenderer(towerFullSprite, 0f, 1f, duration));
        }

        if (fadeOut != null) yield return fadeOut;
        if (fadeIn != null) yield return fadeIn;
    }

    private IEnumerator CrossfadeToMap(float duration)
    {
        // Fade in preview, fade out full tower
        Coroutine fadeOut = null;
        Coroutine fadeIn = null;

        if (towerFullSprite != null)
        {
            fadeOut = StartCoroutine(FadeSpriteRenderer(towerFullSprite, 1f, 0f, duration));
        }

        if (towerPreviewSprite != null)
        {
            fadeIn = StartCoroutine(FadeSpriteRenderer(towerPreviewSprite, 0f, 1f, duration));
        }

        if (fadeOut != null) yield return fadeOut;
        if (fadeIn != null) yield return fadeIn;
    }

    public bool IsInTowerView => isInTowerView;
}
