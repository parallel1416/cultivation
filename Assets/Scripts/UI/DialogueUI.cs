using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles dialogue UI display in DialogScene.
/// Subscribes to DialogueManager events and updates UI components accordingly.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue Setup")]
    [SerializeField] private string dialogueEventId = ""; // ID of the dialogue event to play
    [SerializeField] private bool autoStartOnEnable = false; // Auto-start dialogue when scene loads

    [Header("Dialog Panel References")]
    [SerializeField] private GameObject dialogContainer;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText; // Now shows full history
    [SerializeField] private ScrollRect dialogScrollRect; // For scrollable history

    [Header("Title References")]
    [SerializeField] private CanvasGroup titleContainer; // Container for title display
    [SerializeField] private TextMeshProUGUI titleText; // Text component for title
    [SerializeField] private float titleFadeDuration = 2f; // Duration for title fade animation

    [Header("Choices References")]
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private Transform choiceParent; // Parent transform for instantiated choice buttons

    [Header("Visual Settings")]
    [SerializeField] private Image backgroundImage; // Background image component
    [SerializeField] private Image portraitImage; // Portrait image component
    [SerializeField] private Sprite defaultBackgroundSprite; // Default background if none specified
    [SerializeField] private Sprite defaultPortraitSprite; // Default portrait if none specified

    [Header("Dice Panel References")]
    [SerializeField] private GameObject dicePanel;
    [SerializeField] private Transform diceContainer; // Container for dice displays
    [SerializeField] private Image petImage; // Display assigned pet image
    [SerializeField] private Image itemImage; // Display assigned item image
    [SerializeField] private Button diceContinueButton; // Button to roll dice and continue
    [SerializeField] private GameObject dicePrefab; // Prefab for individual dice (with image + text)
    
    [Header("Dice Sprites")]
    [SerializeField] private Sprite d4Sprite;
    [SerializeField] private Sprite d6Sprite;
    [SerializeField] private Sprite d8Sprite;
    [SerializeField] private Sprite d10Sprite;
    [SerializeField] private Sprite d12Sprite;
    [SerializeField] private Sprite d20Sprite;
    
    [Header("Pet/Item Sprites")]
    [SerializeField] private Sprite mouseSprite;
    [SerializeField] private Sprite chickenSprite;
    [SerializeField] private Sprite sheepSprite;
    [SerializeField] private Sprite paperPuppetSprite;
    [SerializeField] private Sprite jadeCicadaSprite;
    [SerializeField] private Sprite fanfanScrollSprite;
    [SerializeField] private Sprite perfectMirrorSprite;
    [SerializeField] private Sprite burdenTalismanSprite;
    
    [Header("Animation Settings")]
    [SerializeField] private float diceAnimationDelay = 0.5f; // Delay between showing each dice
    [SerializeField] private float resultTransitionDelay = 0.8f; // Delay between result stages
    [SerializeField] private Color originalResultColor = Color.white;
    [SerializeField] private Color itemResultColor = Color.yellow;
    [SerializeField] private Color animalResultColor = Color.green;

    [Header("UI Settings")]
    [SerializeField] private string narratorName = "Narrator"; // Name for narration lines
    [SerializeField] private bool autoScrollToBottom = true; // Auto-scroll to newest dialogue
    [SerializeField] private float scrollDelay = 0.1f; // Delay before scrolling (to let layout update)

    [Header("Scene Transition Settings")]
    [SerializeField] private bool fadeOutOnDialogueEnd = false; // Fade to black when dialogue ends
    [SerializeField] private float fadeOutDuration = 1f; // Duration of fade to black
    [SerializeField] private string returnSceneName = "MapScene"; // Scene to return to after dialogue ends

    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private List<string> currentChoiceTexts = new List<string>();
    private bool hasSubscribedToEvents = false;
    private System.Text.StringBuilder dialogueHistory = new System.Text.StringBuilder();
    private Coroutine pendingScrollCoroutine;
    private CanvasGroup fadeOverlay;
    private Action onDiceContinueCallback; // Callback when dice continue is clicked
    private List<GameObject> currentDiceObjects = new List<GameObject>(); // Track created dice objects
    private bool isDiceAnimationComplete = false; // Track if animation is done
    private bool isTitleShowing = false; // Track if title is currently displaying
    private bool isDicePanelShowing = false; // Track if dice panel is active

    private void Awake()
    {
        // Subscribe to events as early as possible
        SubscribeToEvents();
    }

    private void OnEnable()
    {
        // Re-subscribe if needed (in case manager was recreated)
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        if (pendingScrollCoroutine != null)
        {
            StopCoroutine(pendingScrollCoroutine);
            pendingScrollCoroutine = null;
        }
    }

    private void SubscribeToEvents()
    {
        if (hasSubscribedToEvents || DialogueManager.Instance == null)
        {
            return;
        }

        DialogueManager.Instance.OnTitleDisplay += HandleTitleDisplay;
        DialogueManager.Instance.OnDialogueDisplay += HandleDialogueDisplay;
        DialogueManager.Instance.OnChoiceDisplay += HandleChoiceDisplay;
        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        DialogueManager.Instance.OnChoiceStart += HandleChoiceStart;
        DialogueManager.Instance.OnChoiceEnd += HandleChoiceEnd;
        DialogueManager.Instance.OnBackgroundChange += HandleBackgroundChange;
        DialogueManager.Instance.OnPortraitChange += HandlePortraitChange;
        DialogueManager.Instance.OnMusicChange += HandleMusicChange;
        
        hasSubscribedToEvents = true;

        // Setup dice continue button listener
        if (diceContinueButton != null)
        {
            diceContinueButton.onClick.AddListener(OnDiceContinueClicked);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (!hasSubscribedToEvents || DialogueManager.Instance == null)
        {
            return;
        }

        DialogueManager.Instance.OnTitleDisplay -= HandleTitleDisplay;
        DialogueManager.Instance.OnDialogueDisplay -= HandleDialogueDisplay;
        DialogueManager.Instance.OnChoiceDisplay -= HandleChoiceDisplay;
        DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        DialogueManager.Instance.OnChoiceStart -= HandleChoiceStart;
        DialogueManager.Instance.OnChoiceEnd -= HandleChoiceEnd;
        DialogueManager.Instance.OnBackgroundChange -= HandleBackgroundChange;
        DialogueManager.Instance.OnPortraitChange -= HandlePortraitChange;
        DialogueManager.Instance.OnMusicChange -= HandleMusicChange;
        
        hasSubscribedToEvents = false;

        // Cleanup dice continue button listener
        if (diceContinueButton != null)
        {
            diceContinueButton.onClick.RemoveListener(OnDiceContinueClicked);
        }
    }

    private void Start()
    {
        // Initialize UI state
        if (dialogContainer != null)
        {
            dialogContainer.SetActive(false);
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }

        if (dicePanel != null)
        {
            dicePanel.SetActive(false);
        }

        // Initialize title container - start visible at full opacity
        if (titleContainer != null)
        {
            titleContainer.alpha = 1f;
            titleContainer.gameObject.SetActive(true);
        }

        // Setup fade overlay for scene transitions
        SetupFadeOverlay();

        // Clear dialogue history
        ClearDialogueHistory();

        // Initialize scroll rect
        InitializeScrollRect();

        // Begin initialization routine that waits for DialogueManager instance
        StartCoroutine(InitializeWhenReady());
    }

    private void InitializeScrollRect()
    {
        if (dialogScrollRect == null)
        {
            return;
        }

        dialogScrollRect.horizontal = false;
        dialogScrollRect.vertical = true;
        dialogScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        dialogScrollRect.movementType = ScrollRect.MovementType.Clamped;
        dialogScrollRect.verticalNormalizedPosition = 0f;
        RefreshContentLayout();
    }

    private void ClearDialogueHistory()
    {
        dialogueHistory.Clear();

        if (dialogText != null)
        {
            dialogText.text = string.Empty;
        }

        if (pendingScrollCoroutine != null)
        {
            StopCoroutine(pendingScrollCoroutine);
            pendingScrollCoroutine = null;
        }
    }

    private IEnumerator InitializeWhenReady()
    {
        // Wait until DialogueManager instance becomes available
        while (DialogueManager.Instance == null)
        {
            yield return null;
        }

        // Ensure subscription now that instance exists
        SubscribeToEvents();

        // Check if there are already queued dialogues (from map scene events)
        bool hasQueuedDialogues = DialogueManager.Instance.GetQueueCount() > 0;

        // Auto-start dialogue only if:
        // 1. autoStartOnEnable is true
        // 2. dialogueEventId is not empty
        // 3. No dialogues are already queued (priority to queued events)
        if (autoStartOnEnable && !string.IsNullOrEmpty(dialogueEventId) && !hasQueuedDialogues)
        {
            yield return null; // wait one more frame
            LogController.Log($"Auto-starting dialogue: {dialogueEventId}");
            StartDialogue();
        }
        else if (hasQueuedDialogues)
        {
            LogController.Log($"Skipping auto-start, {DialogueManager.Instance.GetQueueCount()} dialogue(s) already queued");
            // Start playback of queued dialogues
            yield return null;
            DialogueManager.Instance.StartDialoguePlayback();
        }
        else if (autoStartOnEnable && string.IsNullOrEmpty(dialogueEventId))
        {
            LogController.LogWarning("autoStartOnEnable is true but dialogueEventId is empty");
        }
    }

    /// <summary>
    /// Manually start the dialogue. Call this from external scripts or buttons.
    /// </summary>
    public void StartDialogue()
    {
        if (DialogueManager.Instance == null)
        {
            throw new InvalidOperationException("DialogueManager instance not found.");
        }

        if (string.IsNullOrEmpty(dialogueEventId))
        {
            throw new InvalidOperationException("No dialogue event ID assigned to DialogueUI.");
        }
        
        // Ensure we're subscribed to events
        SubscribeToEvents();

        // Validate dialogue exists before enqueuing
        var definition = DialogueManager.Instance.GetDialogueDefinition(dialogueEventId);
        if (definition == null)
        {
            throw new ArgumentException($"Dialogue definition not found for id '{dialogueEventId}'. Expected file at Resources/Dialogues/{dialogueEventId}.json");
        }

        ClearDialogueHistory();
        InitializeScrollRect();

        // Enqueue the event (GetDialogueDefinition already loads it, but enqueue is still needed)
        DialogueManager.Instance.EnqueueDialogueEvent(dialogueEventId);
        DialogueManager.Instance.StartDialoguePlayback();
    }

    /// <summary>
    /// Change the dialogue event and start it
    /// </summary>
    public void StartDialogue(string eventId)
    {
        if (DialogueManager.Instance == null)
        {
            throw new InvalidOperationException("DialogueManager instance not found.");
        }

        if (string.IsNullOrEmpty(eventId))
        {
            throw new ArgumentException("No dialogue event ID provided.");
        }
        
        dialogueEventId = eventId;
        
        // Ensure we're subscribed to events
        SubscribeToEvents();

        var definition = DialogueManager.Instance.GetDialogueDefinition(eventId);
        if (definition == null)
        {
            throw new ArgumentException($"Dialogue definition not found for id '{eventId}'. Expected file at Resources/Dialogues/{eventId}.json");
        }

        ClearDialogueHistory();
        InitializeScrollRect();

        DialogueManager.Instance.EnqueueDialogueEvent(eventId);
        DialogueManager.Instance.StartDialoguePlayback();
    }

    private void HandleTitleDisplay(string title)
    {
        // Display title in separate container with fade animation
        if (titleContainer != null && titleText != null)
        {
            titleText.text = title;
            isTitleShowing = true; // Block dialogue advancement while title is showing
            StartCoroutine(ShowAndFadeTitle());
        }
    }

    private void HandleDialogueDisplay(string speaker, string text)
    {
        // Show dialog container
        if (dialogContainer != null)
        {
            dialogContainer.SetActive(true);
        }

        // Hide choices container during regular dialogue
        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }

        // Format the line - if speaker is empty, just show text without speaker formatting
        string formattedLine;
        if (string.IsNullOrEmpty(speaker))
        {
            // Narrator/narration - no speaker prefix
            formattedLine = text;
        }
        else
        {
            // Character dialogue - show "Speaker: text"
            formattedLine = $"<b>{speaker}:</b> {text}";
        }

        AppendToHistory(formattedLine);
    }

    private void AppendToHistory(string line)
    {
        // Add line break if not the first line
        if (dialogueHistory.Length > 0)
        {
            dialogueHistory.AppendLine();
            dialogueHistory.AppendLine(); // Double line break for spacing
        }

        dialogueHistory.Append(line);

        // Update the text display
        if (dialogText != null)
        {
            dialogText.text = dialogueHistory.ToString();
        }

        RefreshContentLayout();

        if (autoScrollToBottom && dialogScrollRect != null)
        {
            QueueScrollToBottom();
        }
    }

    private void AppendPlayerChoice(string choiceText)
    {
        if (string.IsNullOrWhiteSpace(choiceText))
        {
            return;
        }

        AppendToHistory($"<color=#7EE0FF><b>You:</b> {choiceText}</color>");
    }

    private void QueueScrollToBottom()
    {
        if (!autoScrollToBottom)
        {
            return;
        }

        if (pendingScrollCoroutine != null)
        {
            StopCoroutine(pendingScrollCoroutine);
        }

        pendingScrollCoroutine = StartCoroutine(ScrollToBottomRoutine());
    }

    private IEnumerator ScrollToBottomRoutine()
    {
        if (scrollDelay > 0f)
        {
            yield return new WaitForSeconds(scrollDelay);
        }
        else
        {
            yield return null;
        }

        Canvas.ForceUpdateCanvases();

        if (dialogScrollRect != null)
        {
            if (dialogScrollRect.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(dialogScrollRect.content);
            }

            dialogScrollRect.verticalNormalizedPosition = 0f;
        }

        pendingScrollCoroutine = null;
    }

    private void RefreshContentLayout()
    {
        if (dialogScrollRect == null)
        {
            return;
        }

        // Ensure content is anchored to top-stretch so it grows downward
        if (dialogScrollRect.content != null)
        {
            RectTransform content = dialogScrollRect.content;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            
            // Ensure text is also anchored to top
            if (dialogText != null)
            {
                RectTransform textRect = dialogText.rectTransform;
                textRect.anchorMin = new Vector2(0f, 1f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.pivot = new Vector2(0.5f, 1f);
                textRect.anchoredPosition = Vector2.zero;
            }
        }

        Canvas.ForceUpdateCanvases();

        if (dialogText != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogText.rectTransform);
        }

        if (dialogScrollRect.content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogScrollRect.content);
        }
    }

    private IEnumerator ShowAndFadeTitle()
    {
        if (titleContainer == null)
        {
            isTitleShowing = false;
            yield break;
        }

        // Title is already visible at full opacity from Start()
        // Just wait a moment before fading
        yield return new WaitForSeconds(1.5f);

        // Fade out
        float elapsed = 0f;
        while (elapsed < titleFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / titleFadeDuration;
            titleContainer.alpha = 1f - t;
            yield return null;
        }

        // Hide after fade completes
        titleContainer.alpha = 0f;
        titleContainer.gameObject.SetActive(false);
        isTitleShowing = false; // Re-enable dialogue advancement
    }

    private void HandleChoiceDisplay(string question, ChoiceOption[] choices)
    {
        // Display question as dialogue if present
        if (!string.IsNullOrEmpty(question))
        {
            AppendToHistory($"<b>{question}</b>");
        }

        // Show choices container
        if (choicesContainer != null)
        {
            choicesContainer.SetActive(true);
        }

        // Clear existing choice buttons
        ClearChoiceButtons();
        currentChoiceTexts.Clear();

        // Create choice buttons
        if (choicePrefab != null && choiceParent != null && choices != null)
        {
            for (int i = 0; i < choices.Length; i++)
            {
                int choiceIndex = i; // Capture for lambda
                ChoiceOption choice = choices[i];

                currentChoiceTexts.Add(choice.text);

                GameObject choiceObj = Instantiate(choicePrefab, choiceParent);
                currentChoiceButtons.Add(choiceObj);

                // Setup button
                Button choiceButton = choiceObj.GetComponent<Button>();
                if (choiceButton != null)
                {
                    choiceButton.onClick.AddListener(() => OnChoiceButtonClicked(choiceIndex));
                }

                // Setup text
                TextMeshProUGUI choiceTextComponent = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
                if (choiceTextComponent != null)
                {
                    choiceTextComponent.text = choice.text;
                }
                else
                {
                    // Fallback to Text component if TMP not found
                    Text choiceTextLegacy = choiceObj.GetComponentInChildren<Text>();
                    if (choiceTextLegacy != null)
                    {
                        choiceTextLegacy.text = choice.text;
                    }
                }
            }

            // Force layout rebuild
            if (choiceParent is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }
    }

    private void HandleChoiceStart()
    {
        // Additional UI handling when choices start (if needed)
    }

    private void HandleChoiceEnd()
    {
        // Clear choices when selection is made
        ClearChoiceButtons();

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }
    }

    private void HandleDialogueEnd()
    {
        // Hide all UI when dialogue ends
        if (dialogContainer != null)
        {
            dialogContainer.SetActive(false);
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }

        ClearChoiceButtons();

        // Clear dialogue history for next dialogue
        ClearDialogueHistory();

        // Fade to black and return to previous scene if enabled
        if (fadeOutOnDialogueEnd)
        {
            StartCoroutine(FadeOutAndEndScene());
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (GameObject choiceButton in currentChoiceButtons)
        {
            if (choiceButton != null)
            {
                Destroy(choiceButton);
            }
        }
        currentChoiceButtons.Clear();
        currentChoiceTexts.Clear();
    }

    private void OnChoiceButtonClicked(int choiceIndex)
    {
        if (choiceIndex >= 0 && choiceIndex < currentChoiceTexts.Count)
        {
            AppendPlayerChoice(currentChoiceTexts[choiceIndex]);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SelectChoice(choiceIndex);
        }
    }

    /// <summary>
    /// Click anywhere in the scene to advance dialogue (except during choices)
    /// </summary>
    private void Update()
    {
        if (DialogueManager.Instance != null && 
            DialogueManager.Instance.IsPlayingDialogue() && 
            !DialogueManager.Instance.IsInChoiceMode() &&
            !DialogueManager.Instance.IsOnCooldown() &&
            !isTitleShowing && // Don't advance while title is showing
            !isDicePanelShowing) // Don't advance while dice panel is open
        {
            // Any mouse click or space key advances dialogue
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                DialogueManager.Instance.AdvanceDialogue();
            }
        }
    }

    /// <summary>
    /// Setup a fade overlay for scene transitions
    /// </summary>
    private void SetupFadeOverlay()
    {
        if (fadeOverlay != null) return;

        // Find or create a canvas for the fade overlay
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("DialogueUI: No Canvas found in scene for fade overlay");
            return;
        }

        GameObject overlay = new GameObject("DialogueFadeOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        overlay.transform.SetAsLastSibling(); // Ensure it's on top

        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = overlay.AddComponent<Image>();
        image.color = Color.black;

        fadeOverlay = overlay.AddComponent<CanvasGroup>();
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
    }

    /// <summary>
    /// Fade to black and return to the specified scene
    /// </summary>
    private IEnumerator FadeOutAndEndScene()
    {
        if (fadeOverlay == null)
        {
            SetupFadeOverlay();
        }

        if (fadeOverlay == null)
        {
            Debug.LogError("DialogueUI: Failed to create fade overlay");
            yield break;
        }

        // Get return scene name from DialogueManager, fallback to returnSceneName field
        string sceneToLoad = returnSceneName;
        if (DialogueManager.Instance != null)
        {
            string managerReturnScene = DialogueManager.Instance.GetReturnSceneName();
            if (!string.IsNullOrEmpty(managerReturnScene))
            {
                sceneToLoad = managerReturnScene;
            }
        }

        // Block input during fade
        fadeOverlay.blocksRaycasts = true;

        // Fade to black
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            fadeOverlay.alpha = t;
            yield return null;
        }

        fadeOverlay.alpha = 1f;

        // Small delay at full black
        yield return new WaitForSeconds(0.2f);

        // Load the return scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Public method to trigger fade out and scene end manually
    /// Can be called from anywhere in DialogScene
    /// </summary>
    public void EndDialogueScene()
    {
        StartCoroutine(FadeOutAndEndScene());
    }

    /// <summary>
    /// Show dice panel with assigned team data and perform animated dice roll
    /// </summary>
    public void ShowDicePanel(Dictionary<string, int> assignedDices, string petId, string itemId, Action onContinue)
    {
        if (dicePanel == null)
        {
            Debug.LogError("DialogueUI: Dice panel not assigned!");
            onContinue?.Invoke();
            return;
        }

        // Store callback
        onDiceContinueCallback = onContinue;
        isDiceAnimationComplete = false;
        isDicePanelShowing = true; // Block dialogue advancement while dice panel is showing

        // Show dice panel
        dicePanel.SetActive(true);

        // Hide other UI
        if (dialogContainer != null)
        {
            dialogContainer.SetActive(false);
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }

        // Clear existing dice
        ClearDiceDisplays();

        // Display assigned pet image
        if (petImage != null)
        {
            if (!string.IsNullOrEmpty(petId))
            {
                petImage.sprite = GetPetSprite(petId);
                petImage.enabled = true;
            }
            else
            {
                petImage.enabled = false;
            }
        }

        // Display assigned item image
        if (itemImage != null)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                itemImage.sprite = GetItemSprite(itemId);
                itemImage.enabled = true;
            }
            else
            {
                itemImage.enabled = false;
            }
        }

        // Disable continue button until animation completes
        if (diceContinueButton != null)
        {
            diceContinueButton.interactable = false;
        }

        // Get dice result and start animation
        if (DiceRollManager.Instance != null)
        {
            DiceResult diceResult = DiceRollManager.Instance.GetDiceResult(assignedDices, petId, itemId);
            StartCoroutine(AnimateDiceRoll(diceResult));
        }
        else
        {
            Debug.LogError("DialogueUI: DiceRollManager not found!");
            EnableContinueButton();
        }

        Debug.Log($"DialogueUI: Showing dice panel with {assignedDices?.Count ?? 0} dice types");
    }

    /// <summary>
    /// Hide dice panel
    /// </summary>
    public void HideDicePanel()
    {
        if (dicePanel != null)
        {
            dicePanel.SetActive(false);
        }

        isDicePanelShowing = false; // Re-enable dialogue advancement

        // Show dialog container again
        if (dialogContainer != null)
        {
            dialogContainer.SetActive(true);
        }

        ClearDiceDisplays();
    }

    /// <summary>
    /// Animate dice roll showing original → item effect → pet effect
    /// </summary>
    private IEnumerator AnimateDiceRoll(DiceResult diceResult)
    {
        if (diceContainer == null || diceResult == null)
        {
            EnableContinueButton();
            yield break;
        }

        int diceCount = diceResult.oldResult.Count;

        // Create all dice objects first
        for (int i = 0; i < diceCount; i++)
        {
            GameObject diceObj = CreateDiceObject(i, diceResult.sizes[i]);
            currentDiceObjects.Add(diceObj);
            yield return new WaitForSeconds(diceAnimationDelay);
        }

        // Phase 1: Show original results
        for (int i = 0; i < diceCount; i++)
        {
            UpdateDiceDisplay(currentDiceObjects[i], diceResult.oldResult[i], originalResultColor);
        }

        yield return new WaitForSeconds(resultTransitionDelay);

        // Phase 2: Item effect - flash item image and update results
        if (itemImage != null && itemImage.enabled)
        {
            StartCoroutine(FlashImage(itemImage));
        }

        for (int i = 0; i < diceCount; i++)
        {
            if (diceResult.oldResult[i] != diceResult.itemResult[i])
            {
                UpdateDiceDisplay(currentDiceObjects[i], diceResult.itemResult[i], itemResultColor);
            }
        }

        yield return new WaitForSeconds(resultTransitionDelay);

        // Phase 3: Pet effect - flash pet image and update to final results
        if (petImage != null && petImage.enabled)
        {
            StartCoroutine(FlashImage(petImage));
        }

        for (int i = 0; i < diceCount; i++)
        {
            if (diceResult.itemResult[i] != diceResult.animalResult[i])
            {
                UpdateDiceDisplay(currentDiceObjects[i], diceResult.animalResult[i], animalResultColor);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Animation complete - enable continue button
        EnableContinueButton();
    }

    /// <summary>
    /// Flash an image to indicate effect activation
    /// </summary>
    private IEnumerator FlashImage(Image image)
    {
        if (image == null) yield break;

        Color originalColor = image.color;
        
        // Flash bright
        for (int i = 0; i < 2; i++)
        {
            image.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            image.color = originalColor;
            yield return new WaitForSeconds(0.15f);
        }
    }

    /// <summary>
    /// Create a dice display object with image and text
    /// </summary>
    private GameObject CreateDiceObject(int index, int faceCount)
    {
        GameObject diceObj;

        if (dicePrefab != null)
        {
            // Use prefab if provided
            diceObj = Instantiate(dicePrefab, diceContainer);
        }
        else
        {
            // Create simple GameObject with Image and Text
            diceObj = new GameObject($"Dice_{index}");
            diceObj.transform.SetParent(diceContainer, false);

            // Add horizontal layout
            HorizontalLayoutGroup layout = diceObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 10f;

            // Create image child
            GameObject imageObj = new GameObject("DiceImage");
            imageObj.transform.SetParent(diceObj.transform, false);
            Image diceImage = imageObj.AddComponent<Image>();
            diceImage.sprite = GetDiceSprite(faceCount);
            LayoutElement imageLayout = imageObj.AddComponent<LayoutElement>();
            imageLayout.preferredWidth = 40;
            imageLayout.preferredHeight = 40;

            // Create text child
            GameObject textObj = new GameObject("DiceText");
            textObj.transform.SetParent(diceObj.transform, false);
            TextMeshProUGUI diceText = textObj.AddComponent<TextMeshProUGUI>();
            diceText.text = "?";
            diceText.fontSize = 24;
            diceText.color = Color.white;
            diceText.alignment = TextAlignmentOptions.Left;
        }

        // Set dice image sprite based on face count
        Image img = diceObj.GetComponentInChildren<Image>();
        if (img != null)
        {
            img.sprite = GetDiceSprite(faceCount);
        }

        return diceObj;
    }

    /// <summary>
    /// Update dice display with new result and color
    /// </summary>
    private void UpdateDiceDisplay(GameObject diceObj, int result, Color color)
    {
        if (diceObj == null) return;

        TextMeshProUGUI text = diceObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = result.ToString();
            text.color = color;
        }
    }

    /// <summary>
    /// Enable continue button after animation
    /// </summary>
    private void EnableContinueButton()
    {
        isDiceAnimationComplete = true;
        if (diceContinueButton != null)
        {
            diceContinueButton.interactable = true;
        }
    }

    /// <summary>
    /// Clear all dice display objects
    /// </summary>
    private void ClearDiceDisplays()
    {
        foreach (GameObject diceObj in currentDiceObjects)
        {
            if (diceObj != null)
            {
                Destroy(diceObj);
            }
        }
        currentDiceObjects.Clear();
    }

    /// <summary>
    /// Get dice sprite based on number of faces
    /// </summary>
    private Sprite GetDiceSprite(int faceCount)
    {
        switch (faceCount)
        {
            case 4: return d4Sprite;
            case 6: return d6Sprite;
            case 8: return d8Sprite;
            case 10: return d10Sprite;
            case 12: return d12Sprite;
            case 20: return d20Sprite;
            default: return d6Sprite; // Default to d6
        }
    }

    /// <summary>
    /// Get pet sprite from ID
    /// </summary>
    private Sprite GetPetSprite(string petId)
    {
        switch (petId)
        {
            case "0": return mouseSprite;
            case "1": return chickenSprite;
            case "2": return sheepSprite;
            default: return null;
        }
    }

    /// <summary>
    /// Get item sprite from ID
    /// </summary>
    private Sprite GetItemSprite(string itemId)
    {
        switch (itemId)
        {
            case "0": return paperPuppetSprite;
            case "1": return jadeCicadaSprite;
            case "2": return fanfanScrollSprite;
            case "3": return perfectMirrorSprite;
            case "4": return burdenTalismanSprite;
            default: return null;
        }
    }

    /// <summary>
    /// Called when dice continue button is clicked
    /// </summary>
    private void OnDiceContinueClicked()
    {
        HideDicePanel();

        // Invoke callback to continue dialogue/perform roll
        onDiceContinueCallback?.Invoke();
        onDiceContinueCallback = null;
    }

    #region Visual and Audio Handlers

    /// <summary>
    /// Handle background image change
    /// </summary>
    private void HandleBackgroundChange(string backgroundPath)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning("DialogueUI: Background image component not assigned");
            return;
        }

        if (string.IsNullOrEmpty(backgroundPath))
        {
            // Use default background
            if (defaultBackgroundSprite != null)
            {
                backgroundImage.sprite = defaultBackgroundSprite;
            }
            return;
        }

        // Load background sprite from Resources
        Sprite backgroundSprite = Resources.Load<Sprite>(backgroundPath);
        if (backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            Debug.Log($"DialogueUI: Background changed to {backgroundPath}");
        }
        else
        {
            Debug.LogWarning($"DialogueUI: Background sprite not found: {backgroundPath}");
            // Fallback to default
            if (defaultBackgroundSprite != null)
            {
                backgroundImage.sprite = defaultBackgroundSprite;
            }
        }
    }

    /// <summary>
    /// Handle portrait image change
    /// </summary>
    private void HandlePortraitChange(string portraitPath)
    {
        if (portraitImage == null)
        {
            Debug.LogWarning("DialogueUI: Portrait image component not assigned");
            return;
        }

        if (string.IsNullOrEmpty(portraitPath))
        {
            // Hide portrait or use default
            if (defaultPortraitSprite != null)
            {
                portraitImage.sprite = defaultPortraitSprite;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
            return;
        }

        // Load portrait sprite from Resources
        Sprite portraitSprite = Resources.Load<Sprite>(portraitPath);
        if (portraitSprite != null)
        {
            portraitImage.sprite = portraitSprite;
            portraitImage.enabled = true;
            Debug.Log($"DialogueUI: Portrait changed to {portraitPath}");
        }
        else
        {
            Debug.LogWarning($"DialogueUI: Portrait sprite not found: {portraitPath}");
            // Fallback to default or hide
            if (defaultPortraitSprite != null)
            {
                portraitImage.sprite = defaultPortraitSprite;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Handle music change - uses SoundManager
    /// </summary>
    private void HandleMusicChange(string musicPath)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("DialogueUI: SoundManager instance not found");
            return;
        }

        if (string.IsNullOrEmpty(musicPath))
        {
            // Stop current music
            SoundManager.Instance.StopBGM(true);
            return;
        }

        // Play BGM with crossfade
        SoundManager.Instance.CrossfadeBGM(musicPath);
        Debug.Log($"DialogueUI: Music changed to {musicPath}");
    }

    #endregion
}

