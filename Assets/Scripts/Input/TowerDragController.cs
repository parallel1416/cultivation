using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Provides drag and scroll-wheel control for the tech tree background.
/// Only the background RectTransform moves while child content (nodes) follows.
/// </summary>
[DisallowMultipleComponent]
public class TowerDragController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform treeBackground;
    [SerializeField] private RectTransform nodesContainer;

    [Header("Drag")]
    [SerializeField] private float dragMultiplier = 1f;
    [SerializeField] private bool invertDrag;

    [Header("Scroll Wheel")]
    [SerializeField] private bool enableScrollWheel = true;
    [SerializeField] private float scrollSensitivity = 250f;

    [Header("Bounds")]
    [SerializeField] private float lowerPadding = 0f;
    [SerializeField] private float overscrollPadding = 500f;

    [Header("Inertia")]
    [SerializeField] private bool enableInertia = true;
    [SerializeField] private float inertiaDamping = 0.9f;
    [SerializeField] private float velocityCutoff = 5f;

    private static readonly Vector3[] cornersBuffer = new Vector3[4];

    private Camera cachedCamera;
    private RectTransform parentRect;
    private Vector2 dragStartLocalPoint;
    private float dragStartY;
    private float lastSampleY;
    private float lastSampleTime;
    private float velocityY;
    private bool isDragging;

    private float baseY;
    private float minY;
    private float maxY;

    private void Awake()
    {
        if (treeBackground == null)
        {
            Debug.LogError("TowerDragController requires a TreeBackground reference.", this);
            enabled = false;
            return;
        }

        if (nodesContainer == null && treeBackground.childCount > 0)
        {
            nodesContainer = treeBackground.GetChild(0) as RectTransform;
        }

        Canvas canvas = treeBackground.GetComponentInParent<Canvas>();
        
        // For Screen Space - Overlay, we must use null camera
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            cachedCamera = null;
        }
        else if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cachedCamera = canvas.worldCamera;
            if (cachedCamera == null)
            {
                cachedCamera = Camera.main;
            }
        }
        else
        {
            cachedCamera = Camera.main;
        }

        parentRect = treeBackground.parent as RectTransform;
        baseY = treeBackground.anchoredPosition.y;
        RecalculateBounds();
    }

    private void OnEnable()
    {
        isDragging = false;
        velocityY = 0f;
        baseY = treeBackground != null ? treeBackground.anchoredPosition.y : 0f;
        RecalculateBounds();
    }

    private void Update()
    {
        if (!enabled || treeBackground == null)
        {
            return;
        }

        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning)
        {
            return;
        }

        HandlePointerInput();
        HandleScrollWheel();
        ApplyInertia();
    }

    private void HandlePointerInput()
    {
        bool pointerOverTree = RectTransformUtility.RectangleContainsScreenPoint(treeBackground, Input.mousePosition, cachedCamera);

        if (Input.GetMouseButtonDown(0) && pointerOverTree)
        {
            StartDrag();
            return;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            UpdateDrag();
            return;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        isDragging = true;
        dragStartY = treeBackground.anchoredPosition.y;
        dragStartLocalPoint = ScreenToParentLocal(Input.mousePosition);
        lastSampleY = dragStartY;
        lastSampleTime = Time.unscaledTime;
        velocityY = 0f;
    }

    private void UpdateDrag()
    {
        Vector2 currentLocalPoint = ScreenToParentLocal(Input.mousePosition);
        float delta = currentLocalPoint.y - dragStartLocalPoint.y;
        if (invertDrag)
        {
            delta = -delta;
        }

        float target = dragStartY + delta * dragMultiplier;
        float clamped = Mathf.Clamp(target, minY, maxY);
        SetTreeY(clamped);

        float now = Time.unscaledTime;
        float deltaTime = now - lastSampleTime;
        if (deltaTime > Mathf.Epsilon)
        {
            velocityY = (treeBackground.anchoredPosition.y - lastSampleY) / deltaTime;
        }

        lastSampleY = treeBackground.anchoredPosition.y;
        lastSampleTime = now;
    }

    private void EndDrag()
    {
        isDragging = false;
    }

    private void HandleScrollWheel()
    {
        if (!enableScrollWheel)
        {
            return;
        }

        float scrollDelta = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollDelta) < Mathf.Epsilon)
        {
            return;
        }

        bool pointerOverTree = RectTransformUtility.RectangleContainsScreenPoint(treeBackground, Input.mousePosition, cachedCamera);
        if (!pointerOverTree)
        {
            return;
        }

        float direction = invertDrag ? 1f : -1f;
        float delta = scrollDelta * scrollSensitivity * direction;
        
        // Add to velocity instead of directly setting position for smooth inertia
        velocityY += delta / Time.unscaledDeltaTime;
        
        lastSampleY = treeBackground.anchoredPosition.y;
        lastSampleTime = Time.unscaledTime;
    }

    private void ApplyInertia()
    {
        if (!enableInertia || isDragging)
        {
            return;
        }

        if (Mathf.Abs(velocityY) <= velocityCutoff)
        {
            velocityY = 0f;
            return;
        }

        float newY = Mathf.Clamp(treeBackground.anchoredPosition.y + velocityY * Time.unscaledDeltaTime, minY, maxY);
        SetTreeY(newY);
        velocityY *= inertiaDamping;

        if (Mathf.Approximately(newY, minY) || Mathf.Approximately(newY, maxY))
        {
            velocityY = 0f;
        }
    }

    private void SetTreeY(float newY)
    {
        Vector2 anchored = treeBackground.anchoredPosition;
        anchored.y = newY;
        treeBackground.anchoredPosition = anchored;
    }

    private Vector2 ScreenToParentLocal(Vector2 screenPosition)
    {
        RectTransform target = parentRect != null ? parentRect : treeBackground;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPosition, cachedCamera, out Vector2 localPoint);
        return localPoint;
    }

    /// <summary>
    /// Recompute the drag bounds from the tallest active node.
    /// </summary>
    public void RecalculateBounds()
    {
        if (treeBackground == null)
        {
            return;
        }

        baseY = treeBackground.anchoredPosition.y;
        maxY = 0f;  // Can't scroll above the starting position

        if (nodesContainer == null)
        {
            minY = -lowerPadding;
            SetTreeY(Mathf.Clamp(treeBackground.anchoredPosition.y, minY, maxY));
            return;
        }

        // Calculate canvas height for the drag limit
        float canvasHeight = 0f;
        if (parentRect != null)
        {
            canvasHeight = parentRect.rect.height;
        }
        
        // Calculate tree background height
        float treeBackgroundHeight = treeBackground.rect.height;
        
        // Calculate the limit based on tree height and canvas height
        // This prevents dragging past the bottom of the tree
        float heightBasedLimit = -(treeBackgroundHeight - canvasHeight);
        
        float highest = float.MinValue;
        for (int i = 0; i < nodesContainer.childCount; i++)
        {
            RectTransform child = nodesContainer.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeInHierarchy)
            {
                continue;
            }

            float top = CalculateTopEdge(child);
            if (top > highest)
            {
                highest = top;
            }
        }

        overscrollPadding = (LevelManager.Instance != null && LevelManager.Instance.IsBuggy) ? 0f : 2250f;

        if (highest <= float.MinValue)
        {
            minY = Mathf.Max(heightBasedLimit, -lowerPadding);
        }
        else
        {
            // minY is negative: allows scrolling down to show the highest node
            // Use the lower (more restrictive) of the two limits
            float nodeBasedLimit = -(highest - overscrollPadding);
            minY = Mathf.Max(heightBasedLimit, nodeBasedLimit);
        }
        
        SetTreeY(Mathf.Clamp(treeBackground.anchoredPosition.y, minY, maxY));
    }

    private float CalculateTopEdge(RectTransform rect)
    {
        rect.GetWorldCorners(cornersBuffer);
        Vector3 worldTop = cornersBuffer[1];
        Vector3 localTop = treeBackground.InverseTransformPoint(worldTop);
        return localTop.y;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (treeBackground == null)
        {
            return;
        }

        if (!Application.isPlaying)
        {
            cachedCamera = null;
        }

        if (nodesContainer == null && treeBackground.childCount > 0)
        {
            nodesContainer = treeBackground.GetChild(0) as RectTransform;
        }

        baseY = treeBackground.anchoredPosition.y;
        RecalculateBounds();
    }
#endif
}
