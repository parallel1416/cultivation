using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages dynamic assignment of dialogue events to event buttons in MapScene.
/// Reads from DialogueListManager and updates button visibility and event IDs.
/// </summary>
public class EventButtonsManager : MonoBehaviour
{
    [Header("Event Button References")]
    [SerializeField] private Button event1Button;
    [SerializeField] private Button event2Button;
    [SerializeField] private Button event3Button;
    [SerializeField] private Button event4Button;
    [SerializeField] private Button event5Button;

    [Header("Buggy State Event Buttons")]
    [SerializeField] private Button buggyEvent1Button;
    [SerializeField] private Button buggyEvent2Button;
    [SerializeField] private Button buggyEvent3Button;
    [SerializeField] private Button buggyEvent4Button;
    [SerializeField] private Button buggyEvent5Button;

    private List<Button> eventButtons = new List<Button>();
    private List<Button> buggyEventButtons = new List<Button>();
    private Dictionary<Button, string> buttonToEventId = new Dictionary<Button, string>();

    private void Awake()
    {
        // Collect all normal button references
        eventButtons.Add(event1Button);
        eventButtons.Add(event2Button);
        eventButtons.Add(event3Button);
        eventButtons.Add(event4Button);
        eventButtons.Add(event5Button);

        // Collect all buggy button references
        buggyEventButtons.Add(buggyEvent1Button);
        buggyEventButtons.Add(buggyEvent2Button);
        buggyEventButtons.Add(buggyEvent3Button);
        buggyEventButtons.Add(buggyEvent4Button);
        buggyEventButtons.Add(buggyEvent5Button);
    }

    private void Start()
    {
        // Initialize turn dialogues if not already set up
        if (DialogueListManager.Instance != null)
        {
            // Check if dialogues are already set up for current turn
            if (DialogueListManager.Instance.CurrentTurnDialogues.Count == 0)
            {
                Debug.Log("EventButtonsManager: No dialogues found, calling SetUpTurnDialogues()");
                DialogueListManager.Instance.SetUpTurnDialogues();
            }
        }
        
        // Update event buttons when MapScene loads
        UpdateEventButtons();
    }

    private void OnEnable()
    {
        // Also update when re-enabled (in case we return from DialogScene)
        UpdateEventButtons();
    }

    /// <summary>
    /// Updates event buttons based on current turn's dialogue list.
    /// Assigns first 5 events from DialogueListManager to event1-event5 buttons.
    /// If buggy state is active, uses buggy buttons instead.
    /// </summary>
    public void UpdateEventButtons()
    {
        if (DialogueListManager.Instance == null)
        {
            Debug.LogWarning("EventButtonsManager: DialogueListManager.Instance is null!");
            return;
        }

        List<string> currentDialogues = DialogueListManager.Instance.CurrentTurnDialogues;

        if (currentDialogues == null)
        {
            Debug.LogWarning("EventButtonsManager: CurrentTurnDialogues is null!");
            return;
        }

        // Check if in buggy state
        bool isBuggy = LevelManager.Instance != null && LevelManager.Instance.IsBuggy;

        // Determine which set of buttons to use
        List<Button> activeButtons = isBuggy ? buggyEventButtons : eventButtons;
        List<Button> inactiveButtons = isBuggy ? eventButtons : buggyEventButtons;

        // Disable the inactive button set
        foreach (Button button in inactiveButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }

        // Clear previous mappings
        buttonToEventId.Clear();

        // Assign events to active buttons (up to 5)
        for (int i = 0; i < activeButtons.Count; i++)
        {
            if (activeButtons[i] == null)
            {
                Debug.LogWarning($"EventButtonsManager: {(isBuggy ? "Buggy " : "")}Event button {i + 1} is not assigned!");
                continue;
            }

            if (i < currentDialogues.Count)
            {
                // Event available - enable button and assign event ID
                string eventId = currentDialogues[i];
                activeButtons[i].gameObject.SetActive(true);
                activeButtons[i].interactable = true;
                activeButtons[i].name = eventId; // Set button name to event ID for EventPanelManager
                buttonToEventId[activeButtons[i]] = eventId;

                Debug.Log($"{(isBuggy ? "Buggy " : "")}Event button {i + 1} assigned to event: {eventId}");
            }
            else
            {
                // No event for this button - disable it
                activeButtons[i].gameObject.SetActive(false);
                Debug.Log($"{(isBuggy ? "Buggy " : "")}Event button {i + 1} disabled (no event available)");
            }
        }

        Debug.Log($"EventButtonsManager: Updated {currentDialogues.Count} event buttons for turn {TurnManager.Instance?.CurrentTurn} (Buggy: {isBuggy})");
    }

    /// <summary>
    /// Gets the event ID assigned to a specific button.
    /// </summary>
    public string GetEventIdForButton(Button button)
    {
        if (buttonToEventId.TryGetValue(button, out string eventId))
        {
            return eventId;
        }
        return null;
    }

    /// <summary>
    /// Manually refresh event buttons (call after completing an event).
    /// </summary>
    public void RefreshEventButtons()
    {
        UpdateEventButtons();
    }
}
