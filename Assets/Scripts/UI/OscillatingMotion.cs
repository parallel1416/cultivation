using UnityEngine;

/// <summary>
/// Makes a GameObject oscillate smoothly up and down (or in any direction).
/// Can be attached to any GameObject to create a floating/bobbing effect.
/// </summary>
public class OscillatingMotion : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [Tooltip("The distance the object will move from its starting position")]
    [SerializeField] private float amplitude = 0.5f;
    
    [Tooltip("How fast the object oscillates (cycles per second)")]
    [SerializeField] private float frequency = 1f;
    
    [Tooltip("Direction of oscillation")]
    [SerializeField] private Vector3 direction = Vector3.up;
    
    [Header("Phase Settings")]
    [Tooltip("Starting phase offset (0-1). Use different values for multiple objects to desync them")]
    [SerializeField] private float phaseOffset = 0f;
    
    [Tooltip("Randomize phase on start to desync multiple objects automatically")]
    [SerializeField] private bool randomizePhase = false;
    
    [Header("Motion Curve")]
    [Tooltip("Use a custom animation curve for non-sine wave motion (leave empty for sine wave)")]
    [SerializeField] private AnimationCurve customCurve;
    
    [Header("Control")]
    [Tooltip("Enable or disable the oscillation")]
    [SerializeField] private bool isEnabled = true;
    
    private Vector3 startPosition;
    private float currentPhase;
    
    private void Start()
    {
        // Store the starting position
        startPosition = transform.localPosition;
        
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
        
        // Calculate oscillation using sine wave or custom curve
        float oscillationValue;
        
        if (customCurve != null && customCurve.length > 0)
        {
            // Use custom curve
            float normalizedTime = (Time.time * frequency + phaseOffset) % 1f;
            oscillationValue = customCurve.Evaluate(normalizedTime) * amplitude;
        }
        else
        {
            // Use sine wave
            float time = Time.time * frequency * Mathf.PI * 2f + currentPhase;
            oscillationValue = Mathf.Sin(time) * amplitude;
        }
        
        // Apply oscillation to position
        Vector3 offset = direction.normalized * oscillationValue;
        transform.localPosition = startPosition + offset;
    }
    
    /// <summary>
    /// Enable the oscillation
    /// </summary>
    public void Enable()
    {
        isEnabled = true;
    }
    
    /// <summary>
    /// Disable the oscillation and return to start position
    /// </summary>
    public void Disable()
    {
        isEnabled = false;
        transform.localPosition = startPosition;
    }
    
    /// <summary>
    /// Reset to starting position without disabling
    /// </summary>
    public void ResetPosition()
    {
        startPosition = transform.localPosition;
        currentPhase = randomizePhase ? Random.Range(0f, 1f) * Mathf.PI * 2f : phaseOffset * Mathf.PI * 2f;
    }
    
    /// <summary>
    /// Change the amplitude at runtime
    /// </summary>
    public void SetAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
    }
    
    /// <summary>
    /// Change the frequency at runtime
    /// </summary>
    public void SetFrequency(float newFrequency)
    {
        frequency = newFrequency;
    }
    
    /// <summary>
    /// Change the direction at runtime
    /// </summary>
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw oscillation range in editor
        Vector3 origin = Application.isPlaying ? startPosition : transform.localPosition;
        Vector3 worldOrigin = transform.parent != null ? transform.parent.TransformPoint(origin) : origin;
        
        Vector3 normalizedDir = direction.normalized;
        Vector3 maxPos = worldOrigin + normalizedDir * amplitude;
        Vector3 minPos = worldOrigin - normalizedDir * amplitude;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(minPos, maxPos);
        Gizmos.DrawWireSphere(maxPos, 0.05f);
        Gizmos.DrawWireSphere(minPos, 0.05f);
        Gizmos.DrawWireSphere(worldOrigin, 0.03f);
    }
#endif
}
