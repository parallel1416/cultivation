using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles dragging the tower vertically in the Tower view
/// Supports both mouse and touch input with smooth drag physics
/// </summary>
public class TowerDragController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform tower; // Assign the TowerFull transform

    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 1.0f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private bool invertDrag = false;

    [Header("Bounds")]
    [SerializeField] private float minY = -50f; // Bottom of tower
    [SerializeField] private float maxY = 50f;  // Top of tower
    [SerializeField] private bool autoCalculateBounds = true;

    [Header("Inertia")]
    [SerializeField] private bool enableInertia = true;
    [SerializeField] private float inertiaDamping = 0.95f;
    [SerializeField] private float inertiaThreshold = 0.01f;

    private Camera mainCamera;
    private Vector3 dragStartPosition;
    private Vector3 towerStartPosition;
    private bool isDragging = false;
    private Vector3 velocity = Vector3.zero;
    private Vector3 lastPosition;
    private float lastDragTime;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (tower == null)
        {
            Debug.LogError("TowerDragController requires a reference to the Tower transform.", this);
            enabled = false;
            return;
        }

        spriteRenderer = tower.GetComponent<SpriteRenderer>();

        if (autoCalculateBounds && spriteRenderer != null)
        {
            CalculateBoundsFromSprite();
        }
    }

    private void OnEnable()
    {
        // Reset velocity when enabled
        velocity = Vector3.zero;
        isDragging = false;
    }

    private void Update()
    {
        if (!enabled || tower == null) return;

        // Check if transition is in progress
        if (SceneTransitionManager.Instance.IsTransitioning)
        {
            return;
        }

        HandleInput();
        ApplyInertia();
    }

    private void HandleInput()
    {
        // Mouse/Touch down
        if (Input.GetMouseButtonDown(0))
        {
            // Check if we're clicking on UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            StartDrag();
        }
        // Mouse/Touch drag
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag();
        }
        // Mouse/Touch up
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        isDragging = true;
        dragStartPosition = GetWorldPosition(Input.mousePosition);
        towerStartPosition = tower.position;
        lastPosition = tower.position;
        lastDragTime = Time.time;
        velocity = Vector3.zero;
    }

    private void UpdateDrag()
    {
        Vector3 currentWorldPosition = GetWorldPosition(Input.mousePosition);
        Vector3 dragDelta = currentWorldPosition - dragStartPosition;

        if (invertDrag)
        {
            dragDelta = -dragDelta;
        }

        // Only allow vertical movement
        Vector3 targetPosition = towerStartPosition + new Vector3(0, dragDelta.y * dragSpeed, 0);
        
        // Clamp to bounds
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Calculate velocity for inertia
        float deltaTime = Time.time - lastDragTime;
        if (deltaTime > 0)
        {
            velocity = (targetPosition - lastPosition) / deltaTime;
        }

        tower.position = targetPosition;
        lastPosition = targetPosition;
        lastDragTime = Time.time;
    }

    private void EndDrag()
    {
        isDragging = false;
        
        if (!enableInertia)
        {
            velocity = Vector3.zero;
        }
    }

    private void ApplyInertia()
    {
        if (!enableInertia || isDragging) return;

        if (velocity.magnitude > inertiaThreshold)
        {
            // Apply velocity
            Vector3 newPosition = tower.position + velocity * Time.deltaTime;
            
            // Only vertical movement
            newPosition.x = tower.position.x;
            newPosition.z = tower.position.z;
            
            // Clamp to bounds
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            
            // Check if we hit bounds - stop inertia
            if (newPosition.y <= minY || newPosition.y >= maxY)
            {
                velocity = Vector3.zero;
            }
            else
            {
                tower.position = newPosition;
                velocity *= inertiaDamping;
            }
        }
        else
        {
            velocity = Vector3.zero;
        }
    }

    private Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (tower == null) return Vector3.zero;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = tower.position.z; // Maintain z position
        return worldPos;
    }

    private void CalculateBoundsFromSprite()
    {
        if (spriteRenderer == null) return;

        // Get sprite bounds in world space
        Bounds bounds = spriteRenderer.bounds;
        float spriteHeight = bounds.size.y;
        
        // Calculate camera visible height
        float cameraHeight = mainCamera.orthographicSize * 2f;
        
        // Set bounds so we can scroll through entire tower
        // But keep some of it always on screen
        minY = -(spriteHeight - cameraHeight) / 2f;
        maxY = (spriteHeight - cameraHeight) / 2f;

        Debug.Log($"Auto-calculated bounds: minY={minY}, maxY={maxY}, spriteHeight={spriteHeight}");
    }

    /// <summary>
    /// Manually set the drag bounds
    /// </summary>
    public void SetBounds(float min, float max)
    {
        minY = min;
        maxY = max;
    }

    /// <summary>
    /// Smoothly scroll to a specific Y position
    /// </summary>
    public void ScrollToPosition(float targetY, float duration = 0.5f)
    {
        StartCoroutine(ScrollToPositionCoroutine(targetY, duration));
    }

    private System.Collections.IEnumerator ScrollToPositionCoroutine(float targetY, float duration)
    {
    Vector3 startPos = tower.position;
        Vector3 targetPos = new Vector3(startPos.x, Mathf.Clamp(targetY, minY, maxY), startPos.z);
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Smooth step
            t = t * t * (3f - 2f * t);
            
            tower.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        tower.position = targetPos;
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        
        // Draw bounds
        if (tower == null) return;

        float xPos = tower.position.x;
        float zPos = tower.position.z;
        
        // Min bound
        Gizmos.DrawLine(new Vector3(xPos - 5f, minY, zPos), new Vector3(xPos + 5f, minY, zPos));
        
        // Max bound
        Gizmos.DrawLine(new Vector3(xPos - 5f, maxY, zPos), new Vector3(xPos + 5f, maxY, zPos));
        
        // Current position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(tower.position, 0.5f);
    }
}
