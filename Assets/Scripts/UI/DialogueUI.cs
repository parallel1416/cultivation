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

    [Header("UI Settings")]
    [SerializeField] private string narratorName = "Narrator"; // Name for narration lines
    [SerializeField] private bool autoScrollToBottom = true; // Auto-scroll to newest dialogue
    [SerializeField] private float scrollDelay = 0.1f; // Delay before scrolling (to let layout update)

    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private List<string> currentChoiceTexts = new List<string>();
    private bool hasSubscribedToEvents = false;
    private System.Text.StringBuilder dialogueHistory = new System.Text.StringBuilder();
    private Coroutine pendingScrollCoroutine;

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
        
    hasSubscribedToEvents = true;
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
        
        hasSubscribedToEvents = false;
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

        // Initialize title container - start visible at full opacity
        if (titleContainer != null)
        {
            titleContainer.alpha = 1f;
            titleContainer.gameObject.SetActive(true);
        }

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

        // Auto-start dialogue if enabled (slight delay to ensure subscriptions complete)
        if (autoStartOnEnable && !string.IsNullOrEmpty(dialogueEventId))
        {
            yield return null; // wait one more frame
            StartDialogue();
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

        // Format the line as "Speaker: text" and append to history
        string speakerDisplay = string.IsNullOrEmpty(speaker) ? narratorName : speaker;
        string formattedLine = $"<b>{speakerDisplay}:</b> {text}";

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
            !DialogueManager.Instance.IsOnCooldown())
        {
            // Any mouse click or space key advances dialogue
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                DialogueManager.Instance.AdvanceDialogue();
            }
        }
    }
}
