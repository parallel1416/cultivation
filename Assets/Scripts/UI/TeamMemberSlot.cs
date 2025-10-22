using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single team member slot in the event panel.
/// Handles selection toggle and displays member information.
/// </summary>
public class TeamMemberSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI memberNameText;
    [SerializeField] private TextMeshProUGUI memberStatsText;
    [SerializeField] private Toggle selectionToggle;
    [SerializeField] private Image portraitImage;
    
    private string memberId;
    private bool isSelected;

    private void Awake()
    {
        if (selectionToggle != null)
        {
            selectionToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnDestroy()
    {
        if (selectionToggle != null)
        {
            selectionToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    /// <summary>
    /// Initialize the team member slot with data.
    /// </summary>
    public void Initialize(string id, string name, string stats, Sprite portrait = null)
    {
        memberId = id;
        
        if (memberNameText != null)
        {
            memberNameText.text = name;
        }
        
        if (memberStatsText != null)
        {
            memberStatsText.text = stats;
        }
        
        if (portraitImage != null && portrait != null)
        {
            portraitImage.sprite = portrait;
        }
        
        if (selectionToggle != null)
        {
            selectionToggle.isOn = false;
        }
        
        isSelected = false;
    }

    /// <summary>
    /// Handle toggle value change.
    /// </summary>
    private void OnToggleChanged(bool value)
    {
        isSelected = value;
        
        // TODO: Notify parent or backend about selection change
        Debug.Log($"Team member {memberId} selection: {isSelected}");
    }

    /// <summary>
    /// Get whether this member is selected.
    /// </summary>
    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// Get the member ID.
    /// </summary>
    public string GetMemberId()
    {
        return memberId;
    }

    /// <summary>
    /// Set interactability of the slot.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (selectionToggle != null)
        {
            selectionToggle.interactable = interactable;
        }
    }
}
