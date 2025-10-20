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

        GameObject levelmanagerObj = new GameObject("LevelManager");
        levelmanagerObj.AddComponent<LevelManager>();

        GameObject turnmanagerObj = new GameObject("TurnManager");
        turnmanagerObj.AddComponent<TurnManager>();

        GameObject dialoguemanagerObj = new GameObject("DialogueManager");
        dialoguemanagerObj.AddComponent<DialogueManager>();

        GameObject globaltagmanagerObj = new GameObject("GlobalTagManager");
        globaltagmanagerObj.AddComponent<GlobalTagManager>();
    }

    
}
