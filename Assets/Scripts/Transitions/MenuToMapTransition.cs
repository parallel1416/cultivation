using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the animated transition from Menu to Map scene
/// Animates obstacles moving out of the way to reveal the map
/// </summary>
public class MenuToMapTransition : TransitionAnimator
{
    [Header("Obstacle References")]
    [SerializeField] private Transform[] obstacles;
    
    [Header("Movement Settings")]
    [SerializeField] private Vector2[] targetOffsets; // Where each obstacle moves to (off-screen)
    [SerializeField] private float staggerDelay = 0.1f; // Delay between obstacle animations
    
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup tapToStartText;

    private Vector2[] originalPositions;

    protected override void Awake()
    {
        base.Awake();
        
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

    /// <summary>
    /// Plays the menu to map transition animation
    /// </summary>
    public IEnumerator PlayTransition(float duration = 2.0f)
    {
        // Fade out tap to start text
        if (tapToStartText != null)
        {
            StartCoroutine(FadeCanvasGroup(tapToStartText, 1f, 0f, 0.3f));
        }

        yield return new WaitForSeconds(0.2f);

        // Animate obstacles
        if (obstacles != null && obstacles.Length > 0)
        {
            // Start all obstacle animations with slight stagger
            for (int i = 0; i < obstacles.Length; i++)
            {
                if (obstacles[i] != null)
                {
                    StartCoroutine(AnimateObstacle(i, duration - 0.5f));
                    yield return new WaitForSeconds(staggerDelay);
                }
            }

            // Wait for animations to complete
            yield return new WaitForSeconds(duration - 0.5f - (staggerDelay * obstacles.Length));
        }
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

        if (tapToStartText != null)
        {
            tapToStartText.alpha = 1f;
        }
    }

    // Editor helper
    private void OnValidate()
    {
        // Auto-populate target offsets if not set
        if (obstacles != null && (targetOffsets == null || targetOffsets.Length != obstacles.Length))
        {
            targetOffsets = new Vector2[obstacles.Length];
            
            // Set default offsets based on index
            for (int i = 0; i < obstacles.Length; i++)
            {
                switch (i)
                {
                    case 0: targetOffsets[i] = new Vector2(-15f, 0f); break; // Left
                    case 1: targetOffsets[i] = new Vector2(15f, 0f); break;  // Right
                    case 2: targetOffsets[i] = new Vector2(0f, 15f); break;  // Up
                    case 3: targetOffsets[i] = new Vector2(0f, -15f); break; // Down
                    default: targetOffsets[i] = new Vector2(-15f, 0f); break;
                }
            }
        }
    }
}
