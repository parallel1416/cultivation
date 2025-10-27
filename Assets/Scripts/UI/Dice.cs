using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Animates a dice roll by rapidly switching numbers in a text box and adding random movement,
/// then stops at the specified result number.
/// </summary>
public class Dice : MonoBehaviour
{
    [Header("Dice Configuration")]
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private int totalFaceCount = 6; // Default d6

    [Header("Animation Settings")]
    [SerializeField] private float rollDuration = 1.5f; // Total animation time
    [SerializeField] private float numberChangeDuration = 0.05f; // Time between number changes
    [SerializeField] private float movementRange = 20f; // Max distance to move in pixels
    [SerializeField] private float movementSpeed = 5f; // Speed of random movement
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // Slows down over time

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isRolling = false;
    private int currentResult = -1;

    private void Awake()
    {
        if (numberText == null)
        {
            numberText = GetComponent<TextMeshProUGUI>();
        }

        if (numberText != null)
        {
            rectTransform = numberText.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalPosition = rectTransform.anchoredPosition;
            }
        }
        else
        {
            Debug.LogError("Dice: No TextMeshProUGUI component found!");
        }

        // Initialize with default text
        if (numberText != null)
        {
            numberText.text = "?";
        }
    }

    /// <summary>
    /// Roll the dice and animate to the specified result
    /// </summary>
    /// <param name="result">The number to show (1-based, e.g., 1-6 for d6)</param>
    public void Roll(int result)
    {
        if (isRolling)
        {
            Debug.LogWarning("Dice: Already rolling, ignoring new roll request");
            return;
        }

        if (result < 1 || result > totalFaceCount)
        {
            Debug.LogError($"Dice: Invalid result {result}. Must be between 1 and {totalFaceCount}");
            return;
        }

        currentResult = result;
        StartCoroutine(RollAnimation());
    }

    /// <summary>
    /// Roll the dice with a callback when animation completes
    /// </summary>
    public void Roll(int result, System.Action onComplete)
    {
        if (isRolling)
        {
            Debug.LogWarning("Dice: Already rolling, ignoring new roll request");
            return;
        }

        if (result < 1 || result > totalFaceCount)
        {
            Debug.LogError($"Dice: Invalid result {result}. Must be between 1 and {totalFaceCount}");
            return;
        }

        currentResult = result;
        StartCoroutine(RollAnimationWithCallback(onComplete));
    }

    private IEnumerator RollAnimation()
    {
        isRolling = true;

        float elapsed = 0f;
        float nextNumberChangeTime = 0f;
        Vector2 targetPosition = originalPosition;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rollDuration;
            float speedMultiplier = speedCurve.Evaluate(t);

            // Change number rapidly at the beginning, slower near the end
            if (elapsed >= nextNumberChangeTime)
            {
                // Show random number during animation
                int randomNumber = Random.Range(1, totalFaceCount + 1);
                if (numberText != null)
                {
                    numberText.text = randomNumber.ToString();
                }

                // Adjust number change speed based on curve
                nextNumberChangeTime = elapsed + (numberChangeDuration / Mathf.Max(speedMultiplier, 0.1f));
            }

            // Random movement - pick new target occasionally
            if (Random.value < 0.1f * speedMultiplier) // Less frequent targets near the end
            {
                targetPosition = originalPosition + new Vector2(
                    Random.Range(-movementRange, movementRange),
                    Random.Range(-movementRange, movementRange)
                ) * speedMultiplier;
            }

            // Smoothly move toward target position
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(
                    rectTransform.anchoredPosition,
                    targetPosition,
                    Time.deltaTime * movementSpeed
                );
            }

            yield return null;
        }

        // Stop at the final result
        if (numberText != null)
        {
            numberText.text = currentResult.ToString();
        }

        // Return to original position
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        isRolling = false;
        Debug.Log($"Dice: Finished rolling, result = {currentResult}");
    }

    private IEnumerator RollAnimationWithCallback(System.Action onComplete)
    {
        yield return StartCoroutine(RollAnimation());
        onComplete?.Invoke();
    }

    /// <summary>
    /// Set the dice to show a specific number without animation
    /// </summary>
    public void SetNumber(int number)
    {
        if (number < 1 || number > totalFaceCount)
        {
            Debug.LogError($"Dice: Invalid number {number}. Must be between 1 and {totalFaceCount}");
            return;
        }

        if (numberText != null)
        {
            numberText.text = number.ToString();
            currentResult = number;
        }
    }

    /// <summary>
    /// Reset dice to original position
    /// </summary>
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    /// <summary>
    /// Check if dice is currently rolling
    /// </summary>
    public bool IsRolling => isRolling;

    /// <summary>
    /// Get the current result (or -1 if not rolled yet)
    /// </summary>
    public int CurrentResult => currentResult;

    /// <summary>
    /// Get the total face count for this dice
    /// </summary>
    public int TotalFaceCount => totalFaceCount;

    /// <summary>
    /// Update the original position (useful if dice is moved in the scene)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
    }
}
