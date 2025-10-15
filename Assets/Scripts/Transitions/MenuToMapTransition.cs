using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the animated transition from Menu to Map scene
/// Animates obstacles moving out of the way to reveal the map
/// Triggered by clicking Start button in circular menu
/// </summary>
public class MenuToMapTransition : TransitionAnimator
{
    [Header("Button Reference")]
    [SerializeField] private Button startButton;
    
    [Header("Obstacle References")]
    [SerializeField] private Transform obstacleContainer; // Canvas/ObstacleContainer
    private Transform[] obstacles;
    
    [Header("Movement Settings")]
    [SerializeField] private Vector2[] targetOffsets; // Where each obstacle moves to (off-screen)
    [SerializeField] private float staggerDelay = 0.1f; // Delay between obstacle animations
    [SerializeField] private float transitionDuration = 2.0f;

    private Vector2[] originalPositions;

    protected override void Awake()
    {
        base.Awake();
        
        // Get obstacles from container
        if (obstacleContainer != null)
        {
            obstacles = new Transform[obstacleContainer.childCount];
            for (int i = 0; i < obstacleContainer.childCount; i++)
            {
                obstacles[i] = obstacleContainer.GetChild(i);
            }
        }
        
        // Store original positions
        if (obstacles != null && obstacles.Length > 0)
        {
            originalPositions = new Vector2[obstacles.Length];
            for (int i = 0; i < obstacles.Length; i++)
            {
                if (obstacles[i] != null)
                {
                    originalPositions[i] = obstacles[i].position;
                }
            }
        }
    }

    private void Start()
    {
        // Hook up start button
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    /// <summary>
    /// Called when start button is clicked
    /// </summary>
    private void OnStartButtonClicked()
    {
        StartCoroutine(TransitionToMapScene());
    }

    /// <summary>
    /// Transition coroutine - animates obstacles and loads map scene
    /// </summary>
    private IEnumerator TransitionToMapScene()
    {
        // Animate obstacles
        if (obstacles != null && obstacles.Length > 0)
        {
            // Start all obstacle animations with slight stagger
            for (int i = 0; i < obstacles.Length; i++)
            {
                if (obstacles[i] != null)
                {
                    StartCoroutine(AnimateObstacle(i, transitionDuration - 0.5f));
                    yield return new WaitForSeconds(staggerDelay);
                }
            }

            // Wait for animations to complete
            yield return new WaitForSeconds(transitionDuration - 0.5f - (staggerDelay * obstacles.Length));
        }

        // Load map scene
        SceneManager.LoadScene("MapScene");
    }

    /// <summary>
    /// Animates a single obstacle moving off-screen
    /// </summary>
    private IEnumerator AnimateObstacle(int index, float duration)
    {
        if (index >= obstacles.Length || obstacles[index] == null) yield break;

        Transform obstacle = obstacles[index];
        Vector2 startPos = originalPositions[index];
        Vector2 endPos = startPos;

        // Calculate end position based on target offset
        if (targetOffsets != null && index < targetOffsets.Length)
        {
            endPos = startPos + targetOffsets[index];
        }
        else
        {
            // Default: move away from center
            Vector2 direction = (startPos - Vector2.zero).normalized;
            endPos = startPos + direction * 10f; // Move 10 units away
        }

        // Animate position
        yield return AnimateVector3(
            startPos,
            endPos,
            duration,
            easeCurve,
            (pos) => obstacle.position = pos
        );
    }

    /// <summary>
    /// Reset obstacles to original positions (for testing)
    /// </summary>
    public void ResetObstacles()
    {
        if (obstacles != null && originalPositions != null)
        {
            for (int i = 0; i < obstacles.Length && i < originalPositions.Length; i++)
            {
                if (obstacles[i] != null)
                {
                    obstacles[i].position = originalPositions[i];
                }
            }
        }
    }

    // Editor helper
    private void OnValidate()
    {
        // Auto-populate target offsets for 4 obstacles (default)
        if (targetOffsets == null || targetOffsets.Length == 0)
        {
            targetOffsets = new Vector2[4]
            {
                new Vector2(-15f, 0f),  // Left
                new Vector2(15f, 0f),   // Right
                new Vector2(0f, 15f),   // Up
                new Vector2(0f, -15f)   // Down
            };
        }
    }
}
