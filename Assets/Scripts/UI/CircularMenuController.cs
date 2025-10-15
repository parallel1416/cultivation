using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple script to rotate CircularMenu on scroll with smooth acceleration/deceleration
/// Each option is 37 degrees apart. Highlights the top icon with pink color.
/// </summary>
public class CircularMenuController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float degreesPerOption = 37f;
    [SerializeField] private float smoothTime = 0.5f; // Time to reach target rotation
    [SerializeField] private bool invertScroll = false;

    [Header("Visual Settings")]
    [SerializeField] private Color highlightColor = Color.magenta; // Pink color for top icon
    [SerializeField] private Color normalColor = Color.white; // Default color

    private RectTransform rectTransform;
    private Image[] iconImages; // All icon images under this menu
    private float targetRotation = 0f;
    private float currentRotation = 0f;
    private float rotationVelocity = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        currentRotation = rectTransform.localEulerAngles.z;
        targetRotation = currentRotation;

        // Get all icon images (children of this menu)
        iconImages = GetComponentsInChildren<Image>();
    }

    private void Start()
    {
        // Update colors on start
        UpdateIconColors();
    }

    private void Update()
    {
        // Handle mouse wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float scrollDirection = invertScroll ? -scroll : scroll;
            targetRotation += scrollDirection * degreesPerOption * 10f; // Multiply for scroll sensitivity
        }

        // Keyboard support (optional)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            targetRotation += degreesPerOption;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            targetRotation -= degreesPerOption;
        }

        // Smooth rotation with acceleration/deceleration (SmoothDampAngle)
        currentRotation = Mathf.SmoothDampAngle(
            currentRotation,
            targetRotation,
            ref rotationVelocity,
            smoothTime
        );

        // Apply rotation to Z axis
        rectTransform.localEulerAngles = new Vector3(0, 0, currentRotation);

        // Update icon colors based on position
        UpdateIconColors();
    }

    /// <summary>
    /// Updates icon colors - highlights the one on top (270 degrees in world space)
    /// </summary>
    private void UpdateIconColors()
    {
        if (iconImages == null || iconImages.Length == 0) return;

        foreach (Image icon in iconImages)
        {
            if (icon == null || icon.transform == rectTransform) continue; // Skip parent

            // Get icon's world rotation angle
            float iconWorldAngle = icon.transform.eulerAngles.z;

            // Normalize angle to 0-360 range
            iconWorldAngle = (iconWorldAngle + 360f) % 360f;

            // Check if icon is at the top (270 degrees ± threshold)
            float topAngle = 270f; // Top position in Unity's coordinate system
            float threshold = degreesPerOption / 2f; // Half the spacing between icons

            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(iconWorldAngle, topAngle));

            if (angleDiff < threshold)
            {
                // This icon is on top - highlight it
                icon.color = highlightColor;
            }
            else
            {
                // Not on top - normal color
                icon.color = normalColor;
            }
        }
    }
}
