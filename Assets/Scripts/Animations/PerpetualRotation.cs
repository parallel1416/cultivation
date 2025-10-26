using UnityEngine;

/// <summary>
/// Rotates a RectTransform continuously at a constant speed.
/// Useful for decorative elements like spinning birds, coins, etc.
/// </summary>
public class PerpetualRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.forward; // Z-axis for 2D rotation
    [SerializeField] private bool clockwise = true;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogWarning("PerpetualRotation: No RectTransform found. This script works best with UI elements.");
        }
    }

    private void Update()
    {
        if (rectTransform == null) return;

        // Calculate rotation amount for this frame
        float rotationAmount = rotationSpeed * Time.deltaTime;
        
        // Apply clockwise or counter-clockwise direction
        if (!clockwise)
        {
            rotationAmount = -rotationAmount;
        }

        // Rotate around the specified axis
        rectTransform.Rotate(rotationAxis, rotationAmount, Space.Self);
    }
}
