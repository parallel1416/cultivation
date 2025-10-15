using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual menu icon that can be clicked or interacted with
/// Provides visual feedback and animation
/// </summary>
public class MenuIcon : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float scaleSpeed = 10f;

    [Header("Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text labelText;
    [SerializeField] private GameObject selectionIndicator; // Optional glow or border

    private Vector3 targetScale = Vector3.one;
    private Button button;
    private bool isHovered = false;
    private bool isPressed = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (iconImage == null)
            iconImage = GetComponent<Image>();
    }

    private void Update()
    {
        // Smooth scale animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
            iconImage.sprite = sprite;
    }

    public void SetLabel(string label)
    {
        if (labelText != null)
            labelText.text = label;
    }

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);
    }

    public void OnPointerEnter()
    {
        isHovered = true;
        if (!isPressed)
            targetScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit()
    {
        isHovered = false;
        if (!isPressed)
            targetScale = Vector3.one;
    }

    public void OnPointerDown()
    {
        isPressed = true;
        targetScale = Vector3.one * pressScale;
    }

    public void OnPointerUp()
    {
        isPressed = false;
        targetScale = isHovered ? Vector3.one * hoverScale : Vector3.one;
    }
}
