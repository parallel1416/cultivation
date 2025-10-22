using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the event detail panel that slides out when an event button is clicked.
/// Handles panel positioning (left/right based on button position), animation, and content display.
/// </summary>
public class EventPanelManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private Button closeButton;
    
    [Header("Content References")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject teamMemberArea;
    [SerializeField] private Transform teamMemberContainer;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject teamMemberPrefab;
    
    [Header("Animation Settings")]
    [SerializeField] private float slideAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve slideEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Panel Settings")]
    [SerializeField] private float panelWidthRatio = 0.4f; // 40% of canvas width
    [SerializeField] private float panelMargin = 20f; // Space between button and panel
    
    private Canvas canvas;
    private EventButton currentActiveButton;
    private bool isOpen;
    private bool isAnimating;
    private Coroutine animationCoroutine;
    private List<GameObject> spawnedTeamMembers = new List<GameObject>();
    private bool isConfirmed;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }
    }

    /// <summary>
    /// Opens the event panel for the specified event button.
    /// </summary>
    public void OpenPanel(EventButton eventButton, string eventId)
    {
        if (isAnimating) return;
        
        // Close current panel if different button clicked
        if (isOpen && currentActiveButton != eventButton)
        {
            ClosePanel();
        }
        
        currentActiveButton = eventButton;
        currentActiveButton.SetActive(true);
        isConfirmed = false;
        
        // Load event data (placeholder - your teammate will implement backend)
        LoadEventData(eventId);
        
        // Position panel based on button location
        PositionPanel(eventButton.GetRectTransform());
        
        // Animate panel in
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimatePanel(true));
        
        isOpen = true;
    }

    /// <summary>
    /// Closes the event panel and resets the button state.
    /// </summary>
    public void ClosePanel()
    {
        if (!isOpen || isAnimating) return;
        
        if (currentActiveButton != null)
        {
            currentActiveButton.SetActive(false);
        }
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimatePanel(false));
        
        isOpen = false;
        currentActiveButton = null;
    }

    /// <summary>
    /// Load event data from backend (placeholder implementation).
    /// </summary>
    private void LoadEventData(string eventId)
    {
        // TODO: Replace with actual backend calls from your teammate
        // string description = EventBackend.GetDescription(eventId);
        // EventType eventType = EventBackend.GetEventType(eventId);
        // List<TeamMember> availableMembers = EventBackend.GetPeopleList();
        
        // Placeholder data
        string description = $"This is the description for event {eventId}.\n\nThis event requires you to allocate team members to complete it successfully.";
        bool isTeamEvent = true; // EventBackend.GetEventType(eventId) == EventType.Team;
        
        // Set description
        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
        
        // Show/hide team member area based on event type
        if (teamMemberArea != null)
        {
            teamMemberArea.SetActive(isTeamEvent);
        }
        
        // Populate team members if it's a team event
        if (isTeamEvent)
        {
            PopulateTeamMembers(eventId);
        }
        
        // Reset confirm button
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
    }

    /// <summary>
    /// Populate the team member selection area (placeholder).
    /// </summary>
    private void PopulateTeamMembers(string eventId)
    {
        // Clear existing team member UI
        ClearTeamMembers();
        
        // TODO: Get actual team member list from backend
        // List<TeamMember> members = EventBackend.GetPeopleList();
        
        // Placeholder: Create some example team member slots
        if (teamMemberPrefab != null && teamMemberContainer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject memberObj = Instantiate(teamMemberPrefab, teamMemberContainer);
                spawnedTeamMembers.Add(memberObj);
                
                // Set up the team member UI (name, stats, toggle, etc.)
                // This would connect to your teammate's backend
                TextMeshProUGUI nameText = memberObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = $"Team Member {i + 1}";
                }
            }
        }
    }

    /// <summary>
    /// Clear spawned team member UI elements.
    /// </summary>
    private void ClearTeamMembers()
    {
        foreach (GameObject member in spawnedTeamMembers)
        {
            if (member != null)
            {
                Destroy(member);
            }
        }
        spawnedTeamMembers.Clear();
    }

    /// <summary>
    /// Position the panel to the left or right of the button based on screen position.
    /// </summary>
    private void PositionPanel(RectTransform buttonRect)
    {
        if (panelRectTransform == null || canvas == null) return;
        
        // Calculate panel width
        float canvasWidth = (canvas.transform as RectTransform).rect.width;
        float panelWidth = canvasWidth * panelWidthRatio;
        
        // Set panel width
        panelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
        
        // Get button position in canvas space
        Vector3[] buttonCorners = new Vector3[4];
        buttonRect.GetWorldCorners(buttonCorners);
        Vector2 buttonCenter = RectTransformUtility.WorldToScreenPoint(null, buttonCorners[0]);
        
        // Determine if button is on left or right side of screen
        bool isButtonOnLeft = buttonCenter.x < Screen.width * 0.5f;
        
        // Position panel on opposite side of button
        panelRectTransform.pivot = new Vector2(isButtonOnLeft ? 0f : 1f, 0.5f);
        panelRectTransform.anchorMin = new Vector2(isButtonOnLeft ? 0f : 1f, 0.5f);
        panelRectTransform.anchorMax = new Vector2(isButtonOnLeft ? 0f : 1f, 0.5f);
        
        // Position next to button
        Vector2 buttonLocalPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            buttonCenter,
            canvas.worldCamera,
            out buttonLocalPos
        );
        
        float xOffset = isButtonOnLeft ? (buttonRect.rect.width / 2f + panelMargin) : -(buttonRect.rect.width / 2f + panelMargin);
        panelRectTransform.anchoredPosition = new Vector2(buttonLocalPos.x + xOffset, buttonLocalPos.y);
    }

    /// <summary>
    /// Animate panel sliding in or out.
    /// </summary>
    private IEnumerator AnimatePanel(bool slideIn)
    {
        isAnimating = true;
        
        if (slideIn)
        {
            panelRoot.SetActive(true);
        }
        
        float elapsed = 0f;
        Vector3 startScale = slideIn ? new Vector3(0f, 1f, 1f) : Vector3.one;
        Vector3 endScale = slideIn ? Vector3.one : new Vector3(0f, 1f, 1f);
        
        CanvasGroup canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panelRoot.AddComponent<CanvasGroup>();
        }
        
        while (elapsed < slideAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideAnimationDuration);
            float curveT = slideEaseCurve.Evaluate(t);
            
            panelRectTransform.localScale = Vector3.Lerp(startScale, endScale, curveT);
            canvasGroup.alpha = slideIn ? curveT : (1f - curveT);
            
            yield return null;
        }
        
        panelRectTransform.localScale = endScale;
        canvasGroup.alpha = slideIn ? 1f : 0f;
        
        if (!slideIn)
        {
            panelRoot.SetActive(false);
            ClearTeamMembers();
        }
        
        isAnimating = false;
    }

    /// <summary>
    /// Handle confirm button click.
    /// </summary>
    private void OnConfirmClicked()
    {
        if (isConfirmed) return;
        
        isConfirmed = true;
        
        // TODO: Send confirmation to backend with selected team members
        // List<string> selectedMemberIds = GetSelectedTeamMembers();
        // EventBackend.ConfirmEvent(currentActiveButton.GetEventId(), selectedMemberIds);
        
        Debug.Log($"Event confirmed: {currentActiveButton?.GetEventId()}");
        
        // Disable confirm button and team area
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
        
        if (teamMemberArea != null)
        {
            // Disable all interactive elements in team member area
            Button[] teamButtons = teamMemberArea.GetComponentsInChildren<Button>();
            foreach (Button btn in teamButtons)
            {
                btn.interactable = false;
            }
            
            Toggle[] teamToggles = teamMemberArea.GetComponentsInChildren<Toggle>();
            foreach (Toggle toggle in teamToggles)
            {
                toggle.interactable = false;
            }
        }
    }

    /// <summary>
    /// Get list of selected team member IDs (placeholder).
    /// </summary>
    private List<string> GetSelectedTeamMembers()
    {
        List<string> selectedIds = new List<string>();
        
        // TODO: Implement actual selection logic based on your teammate's backend
        // For now, just return empty list
        
        return selectedIds;
    }
}
