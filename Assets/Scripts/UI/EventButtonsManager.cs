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

    private List<Button> eventButtons = new List<Button>();
    private Dictionary<Button, string> buttonToEventId = new Dictionary<Button, string>();

    private void Awake()
    {
        // Collect all button references
        eventButtons.Add(event1Button);
        eventButtons.Add(event2Button);
        eventButtons.Add(event3Button);
        eventButtons.Add(event4Button);
        eventButtons.Add(event5Button);
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

        // Clear previous mappings
        buttonToEventId.Clear();

        // Assign events to buttons (up to 5)
        for (int i = 0; i < eventButtons.Count; i++)
        {
            if (eventButtons[i] == null)
            {
                Debug.LogWarning($"EventButtonsManager: Event button {i + 1} is not assigned!");
                continue;
            }

            if (i < currentDialogues.Count)
            {
                // Event available - enable button and assign event ID
                string eventId = currentDialogues[i];
                eventButtons[i].gameObject.SetActive(true);
                eventButtons[i].interactable = true;
                eventButtons[i].name = eventId; // Set button name to event ID for EventPanelManager
                buttonToEventId[eventButtons[i]] = eventId;

                Debug.Log($"Event button {i + 1} assigned to event: {eventId}");
            }
            else
            {
                // No event for this button - disable it
                eventButtons[i].gameObject.SetActive(false);
                Debug.Log($"Event button {i + 1} disabled (no event available)");
            }
        }

        Debug.Log($"EventButtonsManager: Updated {currentDialogues.Count} event buttons for turn {TurnManager.Instance?.CurrentTurn}");
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
