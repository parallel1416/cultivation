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

    public int CurrentTurn => currentTurn;

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
        currentTurn++;
        // LevelManager.Instance.GenerateResourcesPerTurn();
        LogController.Log($"Current Turn: {currentTurn}");
    }

    public void ResetTurns(int targetTurn)
    {
        currentTurn = targetTurn;
        LogController.Log($"Turn reset to: {currentTurn}");
    }
}