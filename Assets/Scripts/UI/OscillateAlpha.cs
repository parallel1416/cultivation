using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes an object's alpha oscillate smoothly between two values.
/// Works with UI Image, SpriteRenderer, and CanvasGroup components.
/// </summary>
public class OscillateAlpha : MonoBehaviour
{
    [Header("Alpha Settings")]
    [SerializeField] [Range(0f, 1f)] private float minAlpha = 0f;
    [SerializeField] [Range(0f, 1f)] private float maxAlpha = 1f;

    [Header("Oscillation Settings")]
    [SerializeField] private float frequency = 1f;
    [SerializeField] private AnimationCurve oscillationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Phase Settings")]
    [Tooltip("Starting phase offset (0-1). Use different values for multiple objects to desync them")]
    [SerializeField] private float phaseOffset = 0f;
    [SerializeField] private bool randomizePhase = false;

    [Header("Control")]
    [SerializeField] private bool isEnabled = true;

    private Image imageComponent;
    private SpriteRenderer spriteRendererComponent;
    private CanvasGroup canvasGroupComponent;
    private float currentPhase;
    private Color originalColor;

    private void Awake()
    {
        // Try to find components in order of priority
        canvasGroupComponent = GetComponent<CanvasGroup>();
        imageComponent = GetComponent<Image>();
        spriteRendererComponent = GetComponent<SpriteRenderer>();

        if (canvasGroupComponent == null && imageComponent == null && spriteRendererComponent == null)
        {
            Debug.LogWarning("OscillateAlpha: No CanvasGroup, Image, or SpriteRenderer component found on this GameObject!");
        }

        // Store original color
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
        else if (spriteRendererComponent != null)
        {
            originalColor = spriteRendererComponent.color;
        }
    }

    private void Start()
    {
        // Initialize phase
        if (randomizePhase)
        {
            currentPhase = Random.Range(0f, 1f) * Mathf.PI * 2f;
        }
        else
        {
            currentPhase = phaseOffset * Mathf.PI * 2f;
        }
    }

    private void Update()
    {
        if (!isEnabled)
        {
            return;
        }

        // Calculate oscillation value using sine wave
        float time = Time.time * frequency * Mathf.PI * 2f + currentPhase;
        float sineValue = Mathf.Sin(time);

        // Map sine value (-1 to 1) to (0 to 1) for the curve
        float normalizedValue = (sineValue + 1f) * 0.5f;

        // Apply animation curve if set
        float curvedValue;
        if (oscillationCurve != null && oscillationCurve.length > 0)
        {
            curvedValue = oscillationCurve.Evaluate(normalizedValue);
        }
        else
        {
            curvedValue = normalizedValue;
        }

        // Calculate current alpha
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, curvedValue);

        // Apply alpha
        UpdateAlpha(currentAlpha);
    }

    /// <summary>
    /// Update the alpha on the component
    /// </summary>
    private void UpdateAlpha(float alpha)
    {
        // CanvasGroup has priority (affects all children)
        if (canvasGroupComponent != null)
        {
            canvasGroupComponent.alpha = alpha;
        }
        else if (imageComponent != null)
        {
            Color color = imageComponent.color;
            color.a = alpha;
            imageComponent.color = color;
        }
        else if (spriteRendererComponent != null)
        {
            Color color = spriteRendererComponent.color;
            color.a = alpha;
            spriteRendererComponent.color = color;
        }
    }

    /// <summary>
    /// Enable the oscillation
    /// </summary>
    public void Enable()
    {
        isEnabled = true;
    }

    /// <summary>
    /// Disable the oscillation
    /// </summary>
    public void Disable()
    {
        isEnabled = false;
    }

    /// <summary>
    /// Reset to maximum alpha
    /// </summary>
    public void ResetAlpha()
    {
        UpdateAlpha(maxAlpha);
        currentPhase = randomizePhase ? Random.Range(0f, 1f) * Mathf.PI * 2f : phaseOffset * Mathf.PI * 2f;
    }

    /// <summary>
    /// Set the alpha range at runtime
    /// </summary>
    public void SetAlphaRange(float min, float max)
    {
        minAlpha = Mathf.Clamp01(min);
        maxAlpha = Mathf.Clamp01(max);
    }

    /// <summary>
    /// Set the frequency at runtime
    /// </summary>
    public void SetFrequency(float newFrequency)
    {
        frequency = Mathf.Max(0.01f, newFrequency);
    }

    /// <summary>
    /// Set a specific alpha value (temporarily overrides oscillation)
    /// </summary>
    public void SetAlpha(float alpha)
    {
        UpdateAlpha(Mathf.Clamp01(alpha));
    }

    /// <summary>
    /// Get the current alpha value
    /// </summary>
    public float GetCurrentAlpha()
    {
        if (canvasGroupComponent != null)
        {
            return canvasGroupComponent.alpha;
        }
        else if (imageComponent != null)
        {
            return imageComponent.color.a;
        }
        else if (spriteRendererComponent != null)
        {
            return spriteRendererComponent.color.a;
        }
        return 1f;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (frequency < 0.01f)
        {
            frequency = 0.01f;
        }

        if (minAlpha < 0f) minAlpha = 0f;
        if (minAlpha > 1f) minAlpha = 1f;
        if (maxAlpha < 0f) maxAlpha = 0f;
        if (maxAlpha > 1f) maxAlpha = 1f;

        // Ensure min is not greater than max
        if (minAlpha > maxAlpha)
        {
            float temp = minAlpha;
            minAlpha = maxAlpha;
            maxAlpha = temp;
        }
    }
#endif
}
