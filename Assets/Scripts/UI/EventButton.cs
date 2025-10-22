using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles individual event button behavior in the EventContainer.
/// Changes sprite when clicked and triggers the event panel to open.
/// </summary>
[RequireComponent(typeof(Button))]
public class EventButton : MonoBehaviour
{
    [Header("Button States")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite activeSprite;
    
    [Header("Event Data")]
    [SerializeField] private string eventId;
    
    private Button button;
    private Image buttonImage;
    private EventPanelManager panelManager;
    private bool isActive;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        if (buttonImage != null && normalSprite == null)
        {
            normalSprite = buttonImage.sprite;
        }
    }

    private void Start()
    {
        panelManager = FindObjectOfType<EventPanelManager>();
        
        if (panelManager == null)
        {
            Debug.LogError("EventPanelManager not found in scene!", this);
        }
        
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (panelManager == null) return;
        
        // If this button is already active, close the panel
        if (isActive)
        {
            panelManager.ClosePanel();
        }
        else
        {
            // Open panel for this event
            panelManager.OpenPanel(this, eventId);
        }
    }

    /// <summary>
    /// Set the button to active state (changes sprite).
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (buttonImage != null)
        {
            buttonImage.sprite = active ? activeSprite : normalSprite;
        }
    }

    /// <summary>
    /// Get the button's RectTransform for panel positioning.
    /// </summary>
    public RectTransform GetRectTransform()
    {
        return transform as RectTransform;
    }

    /// <summary>
    /// Set the event ID for this button.
    /// </summary>
    public void SetEventId(string id)
    {
        eventId = id;
    }

    /// <summary>
    /// Get the event ID.
    /// </summary>
    public string GetEventId()
    {
        return eventId;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        // Auto-grab the button's image sprite
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            normalSprite = buttonImage.sprite;
        }
    }
#endif
}
