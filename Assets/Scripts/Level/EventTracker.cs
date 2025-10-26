using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores team assignment data for a confirmed event.
/// </summary>
[System.Serializable]
public class EventTeamData
{
    public string eventId;
    public List<string> assignedMemberIds = new List<string>();
    public bool isConfirmed;
}

/// <summary>
/// Tracks confirmed events and their team assignments within a turn.
/// Persists across scene transitions using DontDestroyOnLoad.
/// </summary>
public class EventTracker : MonoBehaviour
{
    private static EventTracker _instance;
    public static EventTracker Instance => _instance;

    // Track confirmed events in current turn
    private Dictionary<string, EventTeamData> confirmedEvents = new Dictionary<string, EventTeamData>();
    private int trackedTurn = -1;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Records a confirmed event with its team assignments.
    /// </summary>
    public void ConfirmEvent(string eventId, List<string> assignedMemberIds)
    {
        // Check if turn has changed, clear old data if needed
        if (TurnManager.Instance != null)
        {
            int currentTurn = TurnManager.Instance.CurrentTurn;
            if (trackedTurn != currentTurn)
            {
                // New turn started, clear previous turn's data
                ClearTrackedEvents();
                trackedTurn = currentTurn;
            }
        }

        EventTeamData data = new EventTeamData
        {
            eventId = eventId,
            assignedMemberIds = new List<string>(assignedMemberIds),
            isConfirmed = true
        };

        confirmedEvents[eventId] = data;
        Debug.Log($"EventTracker: Confirmed event '{eventId}' with {assignedMemberIds.Count} team members");
    }

    /// <summary>
    /// Checks if an event has been confirmed in this turn.
    /// </summary>
    public bool IsEventConfirmed(string eventId)
    {
        return confirmedEvents.ContainsKey(eventId);
    }

    /// <summary>
    /// Gets team data for a confirmed event.
    /// </summary>
    public EventTeamData GetEventData(string eventId)
    {
        if (confirmedEvents.TryGetValue(eventId, out EventTeamData data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// Gets all confirmed event IDs.
    /// </summary>
    public List<string> GetConfirmedEventIds()
    {
        return new List<string>(confirmedEvents.Keys);
    }

    /// <summary>
    /// Clears all tracked events (called at turn end or turn start).
    /// </summary>
    public void ClearTrackedEvents()
    {
        confirmedEvents.Clear();
        Debug.Log("EventTracker: Cleared all tracked events");
    }

    /// <summary>
    /// Called when turn advances - clears old turn data.
    /// </summary>
    public void OnTurnAdvance()
    {
        ClearTrackedEvents();
        if (TurnManager.Instance != null)
        {
            trackedTurn = TurnManager.Instance.CurrentTurn;
        }
    }
}
