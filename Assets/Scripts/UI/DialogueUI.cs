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
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogText;

    [Header("Choices References")]
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private Transform choiceParent; // Parent transform for instantiated choice buttons

    [Header("UI Settings")]
    [SerializeField] private string narratorName = ""; // Empty speaker name for narration
    [SerializeField] private Color narratorColor = Color.gray;
    [SerializeField] private Color speakerColor = Color.white;

    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private bool hasSubscribedToEvents = false;

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

        // Begin initialization routine that waits for DialogueManager instance
        StartCoroutine(InitializeWhenReady());
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

        DialogueManager.Instance.EnqueueDialogueEvent(eventId);
        DialogueManager.Instance.StartDialoguePlayback();
    }

    private void HandleTitleDisplay(string title)
    {
        // Display title - you could show this in a separate UI element
        // For now, we'll show it as dialogue with no speaker
        if (dialogContainer != null)
        {
            dialogContainer.SetActive(true);
        }

        if (speakerText != null)
        {
            speakerText.text = "";
            speakerText.gameObject.SetActive(false);
        }

        if (dialogText != null)
        {
            dialogText.text = title;
            dialogText.color = narratorColor;
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

        // Update speaker text
        if (speakerText != null)
        {
            if (string.IsNullOrEmpty(speaker))
            {
                // Narration - hide speaker name
                speakerText.text = narratorName;
                speakerText.gameObject.SetActive(false);
            }
            else
            {
                // Character dialogue - show speaker name
                speakerText.text = speaker;
                speakerText.gameObject.SetActive(true);
                speakerText.color = speakerColor;
            }
        }

        // Update dialog text
        if (dialogText != null)
        {
            dialogText.text = text;
            dialogText.color = string.IsNullOrEmpty(speaker) ? narratorColor : Color.white;
        }
    }

    private void HandleChoiceDisplay(string question, ChoiceOption[] choices)
    {
        // Display question as dialogue if present
        if (!string.IsNullOrEmpty(question))
        {
            if (dialogText != null)
            {
                dialogText.text = question;
            }
        }

        // Show choices container
        if (choicesContainer != null)
        {
            choicesContainer.SetActive(true);
        }

        // Clear existing choice buttons
        ClearChoiceButtons();

        // Create choice buttons
        if (choicePrefab != null && choiceParent != null && choices != null)
        {
            for (int i = 0; i < choices.Length; i++)
            {
                int choiceIndex = i; // Capture for lambda
                ChoiceOption choice = choices[i];

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
    }

    private void OnChoiceButtonClicked(int choiceIndex)
    {
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
