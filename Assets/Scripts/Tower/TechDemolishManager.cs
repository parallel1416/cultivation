using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the demolish mode for tech nodes in the tower scene.
/// When globalTag "tech_disable" is true, allows players to demolish (disable) tech nodes.
/// </summary>
public class TechDemolishManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button demolishButton;

    [Header("Visual Feedback")]
    [SerializeField] private Image screenOverlay;
    [SerializeField] private Color overlayTintColor = new Color(1f, 0.3f, 0.3f, 0.2f);
    [SerializeField] private Sprite demolishButtonActiveSprite;
    [SerializeField] private Sprite demolishButtonNormalSprite;

    [Header("Settings")]
    [SerializeField] private string techDisableTagKey = "tech_disable";

    private bool isDemolishMode = false;

    private void Awake()
    {
        // Ensure demolish mode is off at start
        SetDemolishMode(false);
    }

    private void Start()
    {
        // Set up button listener
        if (demolishButton != null)
        {
            demolishButton.onClick.AddListener(OnDemolishButtonClicked);
        }

        // Update button state based on global tag
        UpdateDemolishButtonState();
    }

    private void OnEnable()
    {
        // Update button state when this object is enabled
        UpdateDemolishButtonState();
    }

    private void Update()
    {
        // Continuously check if demolish should be available
        UpdateDemolishButtonState();
    }

    /// <summary>
    /// Updates the demolish button's interactable state based on the global tag
    /// </summary>
    private void UpdateDemolishButtonState()
    {
        if (demolishButton == null)
        {
            return;
        }

        // Check if tech_disable global tag is true
        bool canDemolish = GlobalTagManager.Instance != null && 
                          GlobalTagManager.Instance.GetTagValue(techDisableTagKey);

        demolishButton.interactable = canDemolish;

        // Optionally disable the button GameObject entirely if not available
        if (demolishButton.gameObject.activeSelf != canDemolish)
        {
            demolishButton.gameObject.SetActive(canDemolish);
        }
    }

    /// <summary>
    /// Called when the demolish button is clicked
    /// </summary>
    private void OnDemolishButtonClicked()
    {
        // Toggle demolish mode
        SetDemolishMode(!isDemolishMode);
    }

    /// <summary>
    /// Sets the demolish mode state
    /// </summary>
    private void SetDemolishMode(bool enabled)
    {
        isDemolishMode = enabled;

        // Update screen overlay
        if (screenOverlay != null)
        {
            screenOverlay.gameObject.SetActive(enabled);
            if (enabled)
            {
                screenOverlay.color = overlayTintColor;
            }
        }

        // Update button sprite
        if (demolishButton != null)
        {
            Image buttonImage = demolishButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (enabled && demolishButtonActiveSprite != null)
                {
                    buttonImage.sprite = demolishButtonActiveSprite;
                }
                else if (!enabled && demolishButtonNormalSprite != null)
                {
                    buttonImage.sprite = demolishButtonNormalSprite;
                }
            }
        }
    }

    /// <summary>
    /// Called by tech nodes when they are clicked.
    /// Should be hooked up to tech node buttons.
    /// </summary>
    public void OnTechNodeClicked(Button techNodeButton, string techId)
    {
        if (!isDemolishMode)
        {
            // Not in demolish mode, ignore
            return;
        }

        if (TechManager.Instance == null)
        {
            Debug.LogError("TechDemolishManager: TechManager instance not found!");
            return;
        }

        if (techNodeButton == null)
        {
            Debug.LogWarning("TechDemolishManager: Tech node button is null!");
            return;
        }

        // Demolish the tech node
        DemolishTechNode(techNodeButton, techId);
    }

    /// <summary>
    /// Demolishes a tech node by disabling it and calling the dismantle function
    /// </summary>
    private void DemolishTechNode(Button techNodeButton, string techId)
    {
        // Disable the button
        techNodeButton.interactable = false;

        // Optionally change visual appearance to show it's demolished
        Image buttonImage = techNodeButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color demolishedColor = buttonImage.color;
            demolishedColor.a = 0.3f; // Make it semi-transparent
            buttonImage.color = demolishedColor;
        }

        // Call dismantle function in TechManager
        if (!string.IsNullOrEmpty(techId))
        {
            TechManager.Instance.DismantleTech(techId);
        }
    }

    /// <summary>
    /// Public method to check if currently in demolish mode
    /// </summary>
    public bool IsDemolishMode()
    {
        return isDemolishMode;
    }

    /// <summary>
    /// Public method to exit demolish mode (can be called by other systems)
    /// </summary>
    public void ExitDemolishMode()
    {
        SetDemolishMode(false);
    }

    /// <summary>
    /// Public method to manually refresh button state (can be called after global tag changes)
    /// </summary>
    public void RefreshButtonState()
    {
        UpdateDemolishButtonState();
    }

    private void OnDestroy()
    {
        // Clean up button listener
        if (demolishButton != null)
        {
            demolishButton.onClick.RemoveListener(OnDemolishButtonClicked);
        }
    }
}
