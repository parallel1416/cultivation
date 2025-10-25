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
/// Central manager for event buttons, the detail panel, and team member assignment UI in the Map scene.
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

    private class TeamSlot
    {
        public GameObject root;
        public Button button;
        public Image portraitImage;
        public string assignedPersonId;
        public Color originalColor;
    }

    private class PortraitButtonData
    {
        public Button button;
        public Image image;
        public Color originalColor;
        public string personId;
    }

    private class PersonData
    {
        public string id;
        public string displayName;
        public Sprite portrait;
    }

    private class EventData
    {
        public string eventId;
        public string description;
        public int diceLimit;
        public List<PersonData> availablePeople;
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
    [SerializeField] private GameObject teamMemberArea;
    [SerializeField] private Transform teamSlotContainer;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject teamSlotPrefab;

    [Header("Team Slot Styling")]
    [SerializeField] private Color slotEmptyColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color slotFilledColor = Color.white;

    [Header("Selection Panel")] 
    [SerializeField] private GameObject selectionPanelRoot;
    [SerializeField] private RectTransform selectionPanelRect;
    [SerializeField] private CanvasGroup selectionPanelCanvasGroup;
    [SerializeField] private Transform selectionGrid;
    [SerializeField] private GameObject portraitButtonPrefab;
    [SerializeField] private float selectionSlideDuration = 0.25f;
    [SerializeField] private AnimationCurve selectionSlideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private Color selectionAvailableColor = Color.white;
    [SerializeField] private Color selectionTakenColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Panel Animation")] 
    [SerializeField] private float panelSlideDuration = 0.3f;
    [SerializeField] private AnimationCurve panelSlideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float panelWidthRatio = 0.4f;
    [SerializeField] private float panelMargin = 20f;

    private Canvas canvas;
    private CanvasGroup panelCanvasGroup;

    private EventButtonMapping activeButton;
    private EventData currentEventData;
    private readonly List<TeamSlot> teamSlots = new List<TeamSlot>();
    private readonly Dictionary<string, PortraitButtonData> portraitButtons = new Dictionary<string, PortraitButtonData>();

    private bool isPanelOpen;
    private bool isPanelAnimating;
    private Coroutine panelAnimationCoroutine;

    private bool isSelectionAnimating;
    private Coroutine selectionAnimationCoroutine;
    private TeamSlot activeSelectionSlot;

    private bool isConfirmed;

    private void Awake()
    {
        // selection panel anchoring will be ensured after rect is resolved

        canvas = GetComponentInParent<Canvas>();

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

        if (selectionPanelRoot != null && selectionPanelRect == null)
        {
            selectionPanelRect = selectionPanelRoot.GetComponent<RectTransform>();
        }

        if (selectionPanelRoot != null)
        {
            if (selectionPanelCanvasGroup == null)
            {
                selectionPanelCanvasGroup = selectionPanelRoot.GetComponent<CanvasGroup>();
                if (selectionPanelCanvasGroup == null)
                {
                    selectionPanelCanvasGroup = selectionPanelRoot.AddComponent<CanvasGroup>();
                }
            }
            selectionPanelRoot.SetActive(false);
            selectionPanelCanvasGroup.alpha = 0f;
        }

        // Now that selectionPanelRect is available, ensure it's anchored to the left
        EnsureSelectionPanelAnchoredLeft();

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
                    buttonMappings.Add(new EventButtonMapping
                    {
                        button = btn,
                        eventId = btn.name
                    });
                }
            }
        }

        foreach (var mapping in buttonMappings)
        {
            if (mapping == null || mapping.button == null)
            {
                continue;
            }

            mapping.image = mapping.button.GetComponent<Image>();
            if (mapping.image == null)
            {
                mapping.image = mapping.button.targetGraphic as Image;
            }
            if (mapping.image != null)
            {
                mapping.originalSprite = mapping.image.sprite;
            }

            mapping.originalScale = mapping.button.transform.localScale;

            EventButtonMapping capturedMapping = mapping;
            mapping.clickAction = () => OnEventButtonClicked(capturedMapping);
            mapping.button.onClick.AddListener(mapping.clickAction);

            SetButtonState(mapping, false);
        }
    }

    private void OnValidate()
    {
        if (selectionPanelRoot != null && selectionPanelCanvasGroup == null)
        {
            selectionPanelCanvasGroup = selectionPanelRoot.GetComponent<CanvasGroup>();
        }

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

        isConfirmed = false;
        SetTeamAreaInteractable(true);
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }

        currentEventData = FetchEventData(mapping.eventId);
        
        // If event has no dice limit (diceLimit = 0), go directly to DialogScene
        if (currentEventData != null && currentEventData.diceLimit == 0)
        {
            LoadDialogueScene(mapping.eventId);
            return;
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

        ToggleSelectionPanel(false);
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
            availablePeople = new List<PersonData>()
        };

        DialogueEvent dialogueDefinition = DialogueManager.Instance != null
            ? DialogueManager.Instance.GetDialogueDefinition(eventId)
            : null;

        if (dialogueDefinition != null)
        {
            data.description = ExtractDialogueDescription(dialogueDefinition, data.description);
            data.diceLimit = Mathf.Max(0, dialogueDefinition.diceLimit);
        }

        data.availablePeople = RequestAvailablePeople(eventId);

        // event data loaded
        return data;
    }

    private string ExtractDialogueDescription(DialogueEvent dialogue, string fallback)
    {
        if (dialogue == null)
        {
            return fallback;
        }

        // Attempt to read a public field/property named "desc" using reflection.
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

    private List<PersonData> RequestAvailablePeople(string eventId)
    {
        // TODO: replace with the actual backend call (e.g., EventBackend.GetPeopleList(eventId)).
        // Placeholder implementation returns three empty slots.
        var people = new List<PersonData>();
        for (int i = 0; i < 6; i++)
        {
            people.Add(new PersonData
            {
                id = $"person_{i}",
                displayName = $"Member {i + 1}",
                portrait = null
            });
        }
        return people;
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
        if (teamMemberArea != null)
        {
            teamMemberArea.SetActive(requiresTeam);
        }

        ClearTeamSlots();

        if (requiresTeam)
        {
            PopulateTeamSlots(currentEventData.diceLimit);
        }

        ToggleSelectionPanel(false);
    }

    private void PopulateTeamSlots(int slotCount)
    {
        if (teamSlotPrefab == null || teamSlotContainer == null)
        {
            return;
        }

        slotCount = Mathf.Max(0, slotCount);

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(teamSlotPrefab, teamSlotContainer);
            Button slotButton = slotObj.GetComponent<Button>();
            if (slotButton == null)
            {
                slotButton = slotObj.AddComponent<Button>();
            }

            Image portraitImage = slotObj.GetComponent<Image>();
            if (portraitImage == null)
            {
                portraitImage = slotObj.AddComponent<Image>();
            }

            portraitImage.sprite = null;
            portraitImage.color = slotEmptyColor;

            TeamSlot slot = new TeamSlot
            {
                root = slotObj,
                button = slotButton,
                portraitImage = portraitImage,
                assignedPersonId = string.Empty,
                originalColor = portraitImage.color
            };

            TeamSlot capturedSlot = slot;
            slotButton.onClick.AddListener(() => OnSlotClicked(capturedSlot));
            teamSlots.Add(slot);
        }

        // Force layout rebuild to evenly space slots
        if (teamSlotContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(teamSlotContainer as RectTransform);
        }

        // created team slots
    }

    private void ClearTeamSlots()
    {
        foreach (var slot in teamSlots)
        {
            if (slot?.button != null)
            {
                slot.button.onClick.RemoveAllListeners();
            }

            if (slot?.root != null)
            {
                Destroy(slot.root);
            }

            if (slot != null)
            {
                slot.button = null;
                slot.portraitImage = null;
                slot.root = null;
            }
        }

        teamSlots.Clear();
        activeSelectionSlot = null;
    }

    private void OnSlotClicked(TeamSlot slot)
    {
        if (slot == null || isConfirmed)
        {
            return;
        }

        // Check if slot object still exists
        if (slot.root == null || slot.button == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(slot.assignedPersonId))
        {
            ClearSlot(slot);
            return;
        }

        activeSelectionSlot = slot;
        PopulateSelectionPanel();
        ToggleSelectionPanel(true);
    }

    private void ClearSlot(TeamSlot slot)
    {
        if (slot == null)
        {
            return;
        }

        string personId = slot.assignedPersonId;
        slot.assignedPersonId = string.Empty;
        slot.portraitImage.sprite = null;
        slot.portraitImage.color = slotEmptyColor;

    // cleared slot assignment

        activeSelectionSlot = null;
        ToggleSelectionPanel(false);
        UpdateSelectionButtonStates();

        if (!string.IsNullOrEmpty(personId) && portraitButtons.TryGetValue(personId, out PortraitButtonData portraitButton))
        {
            portraitButton.button.interactable = true;
            portraitButton.image.color = portraitButton.originalColor;
        }
    }

    private void PopulateSelectionPanel()
    {
    // Ensure selection grid layout is present and configured
    EnsureSelectionGridLayout();

        if (selectionGrid == null || currentEventData == null)
        {
            return;
        }

        if (portraitButtonPrefab == null)
        {
            return;
        }

        ClearSelectionPanelContent();

        foreach (var person in currentEventData.availablePeople)
        {
            GameObject portraitObj = Instantiate(portraitButtonPrefab, selectionGrid);
            Button portraitButton = portraitObj.GetComponent<Button>();
            if (portraitButton == null)
            {
                portraitButton = portraitObj.AddComponent<Button>();
            }

            Image portraitImage = portraitObj.GetComponent<Image>();
            if (portraitImage == null)
            {
                portraitImage = portraitObj.AddComponent<Image>();
            }

            portraitImage.sprite = person.portrait;
            portraitImage.color = selectionAvailableColor;

            PortraitButtonData data = new PortraitButtonData
            {
                button = portraitButton,
                image = portraitImage,
                originalColor = portraitImage.color,
                personId = person.id
            };

            string capturedId = person.id;
            portraitButton.onClick.AddListener(() => OnPortraitSelected(capturedId));

            portraitButtons[capturedId] = data;
        }
        UpdateSelectionButtonStates();

        // Rebuild layout to ensure even spacing
        if (selectionGrid != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectionGrid as RectTransform);
        }
    }

    private void ClearSelectionPanelContent()
    {
        // Clear existing children
        foreach (Transform child in selectionGrid)
        {
            Destroy(child.gameObject);
        }
        portraitButtons.Clear();
    }

    private void OnPortraitSelected(string personId)
    {
        if (activeSelectionSlot == null || string.IsNullOrEmpty(personId))
        {
            return;
        }

        PersonData person = currentEventData.availablePeople.Find(p => p.id == personId);
        if (person == null)
        {
            return;
        }

        AssignPersonToSlot(activeSelectionSlot, person);
        ToggleSelectionPanel(false);
    }

    private void AssignPersonToSlot(TeamSlot slot, PersonData person)
    {
        if (slot == null || person == null)
        {
            return;
        }

        slot.assignedPersonId = person.id;
        slot.portraitImage.sprite = person.portrait;
        slot.portraitImage.color = slotFilledColor;

        if (portraitButtons.TryGetValue(person.id, out PortraitButtonData portraitButton))
        {
            portraitButton.button.interactable = false;
            portraitButton.image.color = selectionTakenColor;
        }

        UpdateSelectionButtonStates();
    }

    private void UpdateSelectionButtonStates()
    {
        foreach (var pair in portraitButtons)
        {
            bool assigned = IsPersonAssigned(pair.Key);
            pair.Value.button.interactable = !assigned;
            pair.Value.image.color = assigned ? selectionTakenColor : pair.Value.originalColor;
        }
    }

    private bool IsPersonAssigned(string personId)
    {
        foreach (var slot in teamSlots)
        {
            if (slot != null && slot.assignedPersonId == personId)
            {
                return true;
            }
        }
        return false;
    }

    private void ToggleSelectionPanel(bool show)
    {
        if (selectionPanelRoot == null || selectionPanelRect == null || selectionPanelCanvasGroup == null)
        {
            return;
        }

        if (selectionAnimationCoroutine != null)
        {
            StopCoroutine(selectionAnimationCoroutine);
        }
        selectionAnimationCoroutine = StartCoroutine(AnimateSelectionPanel(show));
    }

    private IEnumerator AnimateSelectionPanel(bool show)
    {

    isSelectionAnimating = true;

        if (show)
        {
            selectionPanelRoot.SetActive(true);
        }

        float width = selectionPanelRect.rect.width;
        Vector2 start = selectionPanelRect.anchoredPosition;
        Vector2 end = start;

        if (show)
        {
            start = new Vector2(-width, selectionPanelRect.anchoredPosition.y);
            end = new Vector2(0f, selectionPanelRect.anchoredPosition.y);
        }
        else
        {
            start = selectionPanelRect.anchoredPosition;
            end = new Vector2(-width, selectionPanelRect.anchoredPosition.y);
        }

        float elapsed = 0f;
        while (elapsed < selectionSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / selectionSlideDuration);
            float eased = selectionSlideCurve.Evaluate(t);

            selectionPanelRect.anchoredPosition = Vector2.Lerp(start, end, eased);
            selectionPanelCanvasGroup.alpha = show ? eased : 1f - eased;
            yield return null;
        }

        selectionPanelRect.anchoredPosition = end;
        selectionPanelCanvasGroup.alpha = show ? 1f : 0f;

        if (!show)
        {
            selectionPanelRoot.SetActive(false);
            ClearSelectionPanelContent();
            activeSelectionSlot = null;
        }

        isSelectionAnimating = false;
    }

    private void PositionPanel(RectTransform buttonRect)
    {
        if (panelRectTransform == null || canvas == null || buttonRect == null)
        {
            return;
        }

        float canvasWidth = (canvas.transform as RectTransform).rect.width;
        float panelWidth = canvasWidth * panelWidthRatio;
        panelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);

        Vector3[] corners = new Vector3[4];
        buttonRect.GetWorldCorners(corners);
        Vector2 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
        Vector2 screenTopRight = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);
        Vector2 buttonCenterScreen = (screenBottomLeft + screenTopRight) * 0.5f;

        bool isButtonOnLeft = buttonCenterScreen.x < Screen.width * 0.5f;

        panelRectTransform.pivot = new Vector2(isButtonOnLeft ? 0f : 1f, 0.5f);
        panelRectTransform.anchorMin = new Vector2(isButtonOnLeft ? 0f : 1f, 0.5f);
        panelRectTransform.anchorMax = new Vector2(isButtonOnLeft ? 0f : 1f, 0.5f);

        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, buttonCenterScreen, canvas.worldCamera, out Vector2 buttonLocalPos);

        float xOffset = isButtonOnLeft ? (buttonRect.rect.width * 0.5f + panelMargin) : -(buttonRect.rect.width * 0.5f + panelMargin);
        panelRectTransform.anchoredPosition = new Vector2(buttonLocalPos.x + xOffset, buttonLocalPos.y);
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
            ClearTeamSlots();
            ToggleSelectionPanel(false);
        }

        isPanelAnimating = false;
    }

    private void OnConfirmClicked()
    {
        if (isConfirmed)
        {
            return;
        }

        isConfirmed = true;

        // TODO: send selection data to backend when available.
        List<string> selectedMembers = GetSelectedTeamMembers();

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }

        SetTeamAreaInteractable(false);
        ToggleSelectionPanel(false);
    }

    private List<string> GetSelectedTeamMembers()
    {
        List<string> ids = new List<string>();
        foreach (var slot in teamSlots)
        {
            if (!string.IsNullOrEmpty(slot.assignedPersonId))
            {
                ids.Add(slot.assignedPersonId);
            }
        }
        return ids;
    }

    private void SetTeamAreaInteractable(bool interactable)
    {
        foreach (var slot in teamSlots)
        {
            if (slot?.button != null)
            {
                slot.button.interactable = interactable;
            }
        }

        if (teamMemberArea != null)
        {
            CanvasGroup cg = teamMemberArea.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = teamMemberArea.AddComponent<CanvasGroup>();
            }
            cg.interactable = interactable;
            cg.blocksRaycasts = interactable;
            cg.alpha = interactable ? 1f : 0.5f;
        }
    }

    private void EnsureSelectionGridLayout()
    {
        if (selectionGrid == null)
        {
            return;
        }

        var rect = selectionGrid as RectTransform;
        if (rect == null) return;

        GridLayoutGroup grid = selectionGrid.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = selectionGrid.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(64f, 64f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
        }
    }

    private void EnsureSelectionPanelAnchoredLeft()
    {
        if (selectionPanelRect == null)
        {
            return;
        }

        // Anchor to left center and set pivot so sliding from -width -> 0 works
        selectionPanelRect.pivot = new Vector2(0f, 0.5f);
        selectionPanelRect.anchorMin = new Vector2(0f, 0.5f);
        selectionPanelRect.anchorMax = new Vector2(0f, 0.5f);
        // Place it off-screen to the left initially
        float width = selectionPanelRect.rect.width;
        selectionPanelRect.anchoredPosition = new Vector2(-width, selectionPanelRect.anchoredPosition.y);
    }

    private void LoadDialogueScene(string eventId)
    {
        // Enqueue the dialogue event so it's ready when DialogScene loads
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.EnqueueDialogueEvent(eventId);
        }
        else
        {
            Debug.LogError("DialogueManager.Instance is null. Cannot enqueue dialogue for event: " + eventId);
            return;
        }

        // Load DialogScene
        SceneManager.LoadScene("DialogScene");
    }
}
