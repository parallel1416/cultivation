using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets a RectTransform's height proportional to the canvas width.
/// Useful for maintaining aspect ratios relative to screen width.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ProportionalHeightToCanvasWidth : MonoBehaviour
{
    [Header("Proportional Height Settings")]
    [SerializeField] private float heightToWidthRatio = 1.0f; // Height = Canvas Width * this ratio
    [SerializeField] private bool updateContinuously = true; // Update every frame (for responsive resizing)
    [SerializeField] private float minHeight = 0f; // Minimum height limit
    [SerializeField] private float maxHeight = float.MaxValue; // Maximum height limit

    private RectTransform rectTransform;
    private Canvas canvas;
    private RectTransform canvasRectTransform;
    private float lastCanvasWidth = -1f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        FindCanvas();
    }

    private void Start()
    {
        UpdateHeight();
    }

    private void Update()
    {
        if (updateContinuously)
        {
            // Only update if canvas width changed (optimization)
            if (canvasRectTransform != null)
            {
                float currentWidth = canvasRectTransform.rect.width;
                if (!Mathf.Approximately(currentWidth, lastCanvasWidth))
                {
                    UpdateHeight();
                }
            }
            else
            {
                // Try to find canvas again if it was null
                FindCanvas();
                if (canvasRectTransform != null)
                {
                    UpdateHeight();
                }
            }
        }
    }

    private void FindCanvas()
    {
        // Try to find canvas in parent hierarchy
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            // Fallback: find root canvas in scene
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas != null)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning($"ProportionalHeightToCanvasWidth: No Canvas found for {gameObject.name}");
        }
    }

    /// <summary>
    /// Update the height based on current canvas width
    /// </summary>
    public void UpdateHeight()
    {
        if (rectTransform == null || canvasRectTransform == null)
        {
            return;
        }

        float canvasWidth = canvasRectTransform.rect.width;
        lastCanvasWidth = canvasWidth;

        // Calculate proportional height
        float targetHeight = canvasWidth * heightToWidthRatio;

        // Apply min/max limits
        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);

        // Set height (preserve width)
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = targetHeight;
        rectTransform.sizeDelta = sizeDelta;
    }

    /// <summary>
    /// Set the height to width ratio and update immediately
    /// </summary>
    public void SetRatio(float ratio)
    {
        heightToWidthRatio = ratio;
        UpdateHeight();
    }

    /// <summary>
    /// Get current canvas width
    /// </summary>
    public float GetCanvasWidth()
    {
        return canvasRectTransform != null ? canvasRectTransform.rect.width : 0f;
    }

    /// <summary>
    /// Get current calculated height
    /// </summary>
    public float GetCalculatedHeight()
    {
        return rectTransform != null ? rectTransform.sizeDelta.y : 0f;
    }

    private void OnValidate()
    {
        // Update in editor when values change
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (canvasRectTransform == null)
        {
            FindCanvas();
        }

        UpdateHeight();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw canvas width reference line in scene view
        if (canvasRectTransform != null && rectTransform != null)
        {
            Vector3 position = rectTransform.position;
            float canvasWidth = canvasRectTransform.rect.width;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                position - Vector3.right * canvasWidth * 0.5f,
                position + Vector3.right * canvasWidth * 0.5f
            );
            
            Gizmos.color = Color.yellow;
            float height = rectTransform.sizeDelta.y;
            Gizmos.DrawLine(
                position - Vector3.up * height * 0.5f,
                position + Vector3.up * height * 0.5f
            );
        }
    }
#endif
}
