using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main game controller - handles initialization and global game state
/// This script is optional but useful for managing game-wide systems
/// </summary>
public class Main : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool startFromMenu = true;
    
    void Start()
    {
        // Initialize game systems
        InitializeGame();
    }

    void Update()
    {
        // Global game logic here (if needed)
    }

    private void InitializeGame()
    {
        Debug.Log("Game initialized!");
        
        // Ensure SceneTransitionManager exists
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager not found! Make sure it exists in the scene.");
        }

        // Set initial scene state if needed
        // SceneTransitionManager.Instance.SetInitialScene(startFromMenu ? GameScene.Menu : GameScene.Map);
    }
}
