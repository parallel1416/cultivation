using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Central manager for event buttons and the detail panel in the Map scene.
/// Uses TeamSelectionPanel for team assignment when diceLimit > 0.
/// </summary>
public class EventPanelManager : MonoBehaviour
{
    [System.Serializable]
    private class EventButtonMapping
    {
        public Button button;
        public string eventId;

        [HideInInspector] public Image image;
        [HideInInspector] public Sprite originalSprite;
        [HideInInspector] public UnityAction clickAction;
        [HideInInspector] public Vector3 originalScale;
    }

    private class EventData
    {
        public string eventId;
        public string description;
        public int diceLimit;
        public bool triggersImmediately; // Whether to play immediately or enqueue
        public bool RequiresTeam => diceLimit > 0;
    }

    [Header("Event Buttons")]
    [SerializeField] private Transform eventButtonContainer;
    [SerializeField] private Sprite buttonNormalSprite;
    [SerializeField] private Sprite buttonActiveSprite;
    [SerializeField] private List<EventButtonMapping> buttonMappings = new List<EventButtonMapping>();
    [SerializeField] private float activeButtonScaleMultiplier = 1.08f;

    [Header("Panel References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private Button closeButton;

    [Header("Content References")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button confirmButton;

    [Header("Team Selection")]
    [SerializeField] private Button assignButton; // Button to open team selection
    [SerializeField] private TeamSelectionPanel teamSelectionPanel; // Reference to team selection panel

    [Header("Panel Animation")] 
    [SerializeField] private float panelSlideDuration = 0.3f;
    [SerializeField] private AnimationCurve panelSlideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float panelWidth = 400f;
    [SerializeField] private float panelMargin = 20f;

    private Canvas canvas;
    private CanvasGroup panelCanvasGroup;

    private EventButtonMapping activeButton;
    private EventData currentEventData;

    private bool isPanelOpen;
    private bool isPanelAnimating;
    private Coroutine panelAnimationCoroutine;

    private bool isConfirmed;

    private void Awake()
    {
        // Find the root canvas in the scene
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        if (canvas == null)
        {
            LogController.LogError("EventPanelManager: No Canvas found in scene!");
        }

        if (panelRoot != null && panelRectTransform == null)
        {
            panelRectTransform = panelRoot.GetComponent<RectTransform>();
        }

        if (panelRoot != null)
        {
            panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panelRoot.AddComponent<CanvasGroup>();
            }
            panelRoot.SetActive(false);
            panelCanvasGroup.alpha = 0f;
        }

        // Auto-find TeamSelectionPanel if not assigned
        if (teamSelectionPanel == null)
        {
            teamSelectionPanel = FindObjectOfType<TeamSelectionPanel>();
        }

        InitializeButtonMappings();

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
        foreach (var mapping in buttonMappings)
        {
            if (mapping?.button != null && mapping.clickAction != null)
            {
                mapping.button.onClick.RemoveListener(mapping.clickAction);
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }
    }

    private void InitializeButtonMappings()
    {
        if (eventButtonContainer != null)
        {
            var buttons = eventButtonContainer.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0 && buttonMappings.Count == 0)
            {
                buttonMappings = new List<EventButtonMapping>();
                foreach (var btn in buttons)
                {
                    buttonMappings.Add(new EventButtonMapping { button = btn });
                }
            }
        }

        foreach (var mapping in buttonMappings)
        {
            if (mapping == null || mapping.button == null)
            {
                continue;
            }

            mapping.originalScale = mapping.button.transform.localScale;
            mapping.image = mapping.button.GetComponent<Image>();
            if (mapping.image == null)
            {
                mapping.image = mapping.button.targetGraphic as Image;
            }
            if (mapping.image != null)
            {
                mapping.originalSprite = mapping.image.sprite;
            }
            mapping.clickAction = () => OnEventButtonClicked(mapping);
            mapping.button.onClick.RemoveListener(mapping.clickAction);
            mapping.button.onClick.AddListener(mapping.clickAction);
        }
    }

    private void OnValidate()
    {
        if (panelRoot != null && panelRectTransform == null)
        {
            panelRectTransform = panelRoot.GetComponent<RectTransform>();
        }
    }

    private void OnEventButtonClicked(EventButtonMapping mapping)
    {
        if (mapping == null)
        {
            return;
        }

        if (isPanelAnimating)
        {
            return;
        }

        if (activeButton == mapping && isPanelOpen)
        {
            ClosePanel();
            return;
        }

        OpenPanel(mapping);
    }

    private void OpenPanel(EventButtonMapping mapping)
    {
        if (mapping == null || mapping.button == null)
        {
            return;
        }

        activeButton = mapping;
        SetAllButtonsInactive();
        SetButtonState(mapping, true);

        // Get eventId from button name (set by EventButtonsManager) or from mapping
        string eventId = !string.IsNullOrEmpty(mapping.button.name) ? mapping.button.name : mapping.eventId;
        currentEventData = FetchEventData(eventId);
        
        // Check if this event was already confirmed
        bool wasConfirmed = EventTracker.Instance != null && 
                           EventTracker.Instance.IsEventConfirmed(mapping.eventId);
        
        isConfirmed = wasConfirmed;
        
        if (wasConfirmed)
        {
            // Event was previously confirmed, restore that state
            if (assignButton != null)
            {
                assignButton.interactable = false;
            }
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }
            LogController.Log($"Opening panel for already-confirmed event: {mapping.eventId}");
        }
        else
        {
            // Event not yet confirmed, allow interaction
            if (assignButton != null)
            {
                assignButton.interactable = true;
            }
            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }
        
        ApplyEventDataToUI();
        
        PositionPanel(mapping.button.GetComponent<RectTransform>());

        if (panelAnimationCoroutine != null)
        {
            StopCoroutine(panelAnimationCoroutine);
        }
        panelAnimationCoroutine = StartCoroutine(AnimatePanel(true));

        isPanelOpen = true;
    }

    public void ClosePanel()
    {
        if (!isPanelOpen || isPanelAnimating)
        {
            return;
        }

        if (panelAnimationCoroutine != null)
        {
            StopCoroutine(panelAnimationCoroutine);
        }
        panelAnimationCoroutine = StartCoroutine(AnimatePanel(false));

        SetAllButtonsInactive();
        activeButton = null;
        currentEventData = null;
        isPanelOpen = false;
    }

    private void SetAllButtonsInactive()
    {
        foreach (var mapping in buttonMappings)
        {
            if (mapping != null)
            {
                SetButtonState(mapping, false);
            }
        }
    }

    private void SetButtonState(EventButtonMapping mapping, bool active)
    {
        if (mapping == null || mapping.button == null)
        {
            return;
        }

        if (mapping.image == null)
        {
            mapping.image = mapping.button.GetComponent<Image>();
            if (mapping.image == null)
            {
                mapping.image = mapping.button.targetGraphic as Image;
            }
        }

        if (mapping.image == null)
        {
            return;
        }

        if (active)
        {
            mapping.image.sprite = buttonActiveSprite != null ? buttonActiveSprite : mapping.originalSprite;
            mapping.button.transform.localScale = mapping.originalScale * activeButtonScaleMultiplier;
        }
        else
        {
            mapping.image.sprite = buttonNormalSprite != null ? buttonNormalSprite : mapping.originalSprite;
            mapping.button.transform.localScale = mapping.originalScale;
        }
    }

    private EventData FetchEventData(string eventId)
    {
        EventData data = new EventData
        {
            eventId = eventId,
            description = $"No description found for {eventId}.",
            diceLimit = 0,
            triggersImmediately = true // Default to immediate
        };

        DialogueEvent dialogueDefinition = DialogueManager.Instance != null
            ? DialogueManager.Instance.GetDialogueDefinition(eventId)
            : null;

        if (dialogueDefinition != null)
        {
            data.description = ExtractDialogueDescription(dialogueDefinition, data.description);
            data.diceLimit = Mathf.Max(0, dialogueDefinition.diceLimit);
            data.triggersImmediately = dialogueDefinition.triggersImmediately;
        }

        return data;
    }

    private string ExtractDialogueDescription(DialogueEvent dialogue, string fallback)
    {
        if (dialogue == null)
        {
            return fallback;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo descField = dialogue.GetType().GetField("desc", flags);
        if (descField != null && descField.FieldType == typeof(string))
        {
            string value = descField.GetValue(dialogue) as string;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        PropertyInfo descProperty = dialogue.GetType().GetProperty("desc", flags);
        if (descProperty != null && descProperty.PropertyType == typeof(string))
        {
            string value = descProperty.GetValue(dialogue) as string;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        if (!string.IsNullOrWhiteSpace(dialogue.title))
        {
            return dialogue.title;
        }

        return fallback;
    }

    private void ApplyEventDataToUI()
    {
        if (currentEventData == null)
        {
            return;
        }

        if (descriptionText != null)
        {
            descriptionText.text = currentEventData.description;
        }

        bool requiresTeam = currentEventData.RequiresTeam;
        
        // Show/hide assign button based on diceLimit
        if (assignButton != null)
        {
            assignButton.gameObject.SetActive(requiresTeam);
        }
        
        // Configure team selection panel if team is required
        if (requiresTeam && teamSelectionPanel != null)
        {
            teamSelectionPanel.SetCurrentEvent(currentEventData.eventId, currentEventData.diceLimit, assignButton);
        }

    }

    private void PositionPanel(RectTransform buttonRect)
    {
        if (panelRectTransform == null || canvas == null || buttonRect == null)
        {
            LogController.LogWarning($"PositionPanel failed: panelRect={panelRectTransform != null}, canvas={canvas != null}, buttonRect={buttonRect != null}");
            return;
        }

        // Calculate average position from button's anchors
        float anchorAverageX = (buttonRect.anchorMin.x + buttonRect.anchorMax.x) * 0.5f;
        float anchorAverageY = (buttonRect.anchorMin.y + buttonRect.anchorMax.y) * 0.5f;
        bool isButtonOnLeft = anchorAverageX < 0.5f;

        // Set panel anchors to match button's average position
        panelRectTransform.anchorMin = new Vector2(anchorAverageX, anchorAverageY);
        panelRectTransform.anchorMax = new Vector2(anchorAverageX, anchorAverageY);

        // Set pivot based on which side the button is on
        // If button on left, panel expands to the right (pivot at left edge)
        // If button on right, panel expands to the left (pivot at right edge)
        panelRectTransform.pivot = new Vector2(isButtonOnLeft ? 0f : 1f, 0.5f);

        // Use fixed panel width
        panelRectTransform.sizeDelta = new Vector2(panelWidth, panelRectTransform.sizeDelta.y);

        // Calculate horizontal offset
        // Panel should be positioned next to the button with margin
        float buttonHalfWidth = buttonRect.rect.width * 0.5f;
        float xOffset = isButtonOnLeft 
            ? buttonHalfWidth + panelMargin  // Button on left, panel goes right
            : -(buttonHalfWidth + panelMargin); // Button on right, panel goes left

        // Position panel relative to button's anchored position
        panelRectTransform.anchoredPosition = new Vector2(xOffset + buttonRect.anchoredPosition.x, buttonRect.anchoredPosition.y);
    }

    private IEnumerator AnimatePanel(bool slideIn)
    {
        if (panelRoot == null || panelRectTransform == null || panelCanvasGroup == null)
        {
            yield break;
        }

        isPanelAnimating = true;

        if (slideIn)
        {
            panelRoot.SetActive(true);
        }

        Vector3 startScale = slideIn ? new Vector3(0f, 1f, 1f) : Vector3.one;
        Vector3 endScale = slideIn ? Vector3.one : new Vector3(0f, 1f, 1f);
        float startAlpha = slideIn ? 0f : 1f;
        float endAlpha = slideIn ? 1f : 0f;

        float elapsed = 0f;
        while (elapsed < panelSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panelSlideDuration);
            float eased = panelSlideCurve.Evaluate(t);

            panelRectTransform.localScale = Vector3.Lerp(startScale, endScale, eased);
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);
            yield return null;
        }

        panelRectTransform.localScale = endScale;
        panelCanvasGroup.alpha = endAlpha;

        if (!slideIn)
        {
            panelRoot.SetActive(false);
        }

        isPanelAnimating = false;
    }

    private void OnConfirmClicked()
    {
        if (isConfirmed || currentEventData == null)
        {
            return;
        }

        isConfirmed = true;

        // Disable confirm button (visual feedback that event is confirmed)
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }

        // Handle dialogue based on triggersImmediately flag
        if (!string.IsNullOrEmpty(currentEventData.eventId))
        {
            if (currentEventData.triggersImmediately)
            {
                // Play dialogue immediately
                DialogueManager.PlayDialogueEvent(currentEventData.eventId, "MapScene");
                LogController.Log($"Playing dialogue event '{currentEventData.eventId}' immediately.");
            }
            else
            {
                // Enqueue dialogue for end of round - get team selection from TeamSelectionPanel if needed
                if (currentEventData.RequiresTeam && teamSelectionPanel != null)
                {
                    // Get saved dice assignment, pet, and item
                    Dictionary<string, int> assignedDices = teamSelectionPanel.GetSavedDiceAssignment();
                    int petIndex = teamSelectionPanel.GetSelectedPetIndex();
                    int itemIndex = teamSelectionPanel.GetSelectedItemIndex();
                    
                    // Queue dialogue for end of round
                    if (DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.EnqueueDialogueEvent(currentEventData.eventId);
                        LogController.Log($"Queued dialogue event '{currentEventData.eventId}' for end of round. Queue count: {DialogueManager.Instance.GetQueueCount()}");
                    }

                    // Track event confirmation with team data
                    if (EventTracker.Instance != null)
                    {
                        // Store dice assignment, pet and item indices
                        List<string> teamData = new List<string>();
                        
                        // Add dice data
                        foreach (var kvp in assignedDices)
                        {
                            if (kvp.Value > 0)
                            {
                                teamData.Add($"dice:{kvp.Key}:{kvp.Value}");
                            }
                        }
                        
                        // Add pet index
                        if (petIndex >= 0)
                        {
                            teamData.Add($"pet:{petIndex}");
                        }
                        
                        // Add item index
                        if (itemIndex >= 0)
                        {
                            teamData.Add($"item:{itemIndex}");
                        }
                        
                        EventTracker.Instance.ConfirmEvent(currentEventData.eventId, teamData);
                    }
                    
                    LogController.Log($"Event '{currentEventData.eventId}' confirmed with team selection. Panel remains open.");
                }
                else
                {
                    // No team required - just enqueue
                    if (DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.EnqueueDialogueEvent(currentEventData.eventId);
                        LogController.Log($"Queued dialogue event '{currentEventData.eventId}' for end of round (no team). Queue count: {DialogueManager.Instance.GetQueueCount()}");
                    }
                    
                    // Track event confirmation without team data
                    if (EventTracker.Instance != null)
                    {
                        EventTracker.Instance.ConfirmEvent(currentEventData.eventId, new List<string>());
                    }
                }
                DialogueListManager.Instance.RemoveDialogue(currentEventData.eventId);
            }
        }
    }
}
