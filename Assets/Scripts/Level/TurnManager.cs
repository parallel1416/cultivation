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
        DialogueManager.Instance.StartDialoguePlayback();
        currentTurn++;
        resetActionPoints();
        // LevelManager.Instance.GenerateResourcesPerTurn();
        LogController.Log($"Current Turn: {currentTurn}");
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