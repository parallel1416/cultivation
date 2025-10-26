using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages turn-based game logic
/// </summary>
public class TurnManager : MonoBehaviour
{
    // Singleton instance
    private static TurnManager _instance;
    public static TurnManager Instance => _instance;

    // Only for debugging in inspector
    [SerializeField] private int currentTurn = 1;
    [SerializeField] private int actionPointPerTurn = 4;

    private int actionPoint = 0;

    public int CurrentTurn => currentTurn;
    public int ActionPoint => actionPoint;

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

    public void NextTurn()
    {
        LogController.Log($"=== NextTurn() called. Current turn before increment: {currentTurn} ===");
        
        // Clear event tracker for new turn
        if (EventTracker.Instance != null)
        {
            EventTracker.Instance.OnTurnAdvance();
        }

        // Play queued dialogues from previous turn
        if (DialogueManager.Instance != null)
        {
            int queueCount = DialogueManager.Instance.GetQueueCount();
            LogController.Log($"DialogueManager queue count: {queueCount}");
            
            if (queueCount > 0)
            {
                DialogueManager.Instance.StartDialoguePlayback();
            }
            else
            {
                LogController.Log("No dialogues queued for playback");
            }
        }
        else
        {
            Debug.LogError("DialogueManager.Instance is null!");
        }
        
        // Advance turn
        currentTurn++;
        resetActionPoints();
        
        LogController.Log($"Turn incremented to: {currentTurn}");
        
        // Setup new turn's dialogue events
        if (DialogueListManager.Instance != null)
        {
            DialogueListManager.Instance.SetUpTurnDialogues();
            LogController.Log($"Setup dialogues for turn {currentTurn}. Available events: {DialogueListManager.Instance.CurrentTurnDialogues.Count}");
        }
        else
        {
            Debug.LogError("DialogueListManager.Instance is null!");
        }
        
        // LevelManager.Instance.GenerateResourcesPerTurn();
        LogController.Log($"=== Turn {currentTurn} started ===");
    }

    public void ResetTurns(int targetTurn)
    {
        currentTurn = targetTurn;
        LogController.Log($"Turn reset to: {currentTurn}");
    }

    public bool ConsumeActionPoint(int amount)
    {
        if (actionPoint >= amount)
        {
            actionPoint -= amount;
            LogController.Log($"Consumed {amount} action points. Remaining: {actionPoint}");
            return true;
        }
        LogController.LogError("Not enough action points to consume.");
        return false;
    }

    private void resetActionPoints()
    {
        actionPoint = actionPointPerTurn;
    }
}