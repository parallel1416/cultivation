using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core game manager that initializes essential game systems before any scene loads.
/// </summary>

public class GameManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeGame()
    {
        // create all core manager singleton instance
        GameObject techManagerObj = new GameObject("TechManager");
        techManagerObj.AddComponent<TechManager>();

        GameObject levelManagerObj = new GameObject("LevelManager");
        levelManagerObj.AddComponent<LevelManager>();

        GameObject turnManagerObj = new GameObject("TurnManager");
        turnManagerObj.AddComponent<TurnManager>();

        GameObject dialogueManagerObj = new GameObject("DialogueManager");
        dialogueManagerObj.AddComponent<DialogueManager>();

        GameObject globaltagManagerObj = new GameObject("GlobalTagManager");
        globaltagManagerObj.AddComponent<GlobalTagManager>();

        GameObject dialogueListManagerObj = new GameObject("DialogueListManager");
        dialogueListManagerObj.AddComponent<DialogueListManager>();

        GameObject diceRollManagerObj = new GameObject("DiceRollManager");
        diceRollManagerObj.AddComponent<DiceRollManager>();
    }

    
}
