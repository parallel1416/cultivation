using UnityEngine;

/// <summary>
/// Makes an object rotate back and forth between two angles smoothly.
/// Uses sine wave for smooth oscillation motion.
/// </summary>
public class OscillateRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float minAngle = -45f;
    [SerializeField] private float maxAngle = 45f;

    [Header("Oscillation Settings")]
    [SerializeField] private float frequency = 1f;
    [SerializeField] private AnimationCurve oscillationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Phase Settings")]
    [Tooltip("Starting phase offset (0-1). Use different values for multiple objects to desync them")]
    [SerializeField] private float phaseOffset = 0f;
    [SerializeField] private bool randomizePhase = false;

    [Header("Control")]
    [SerializeField] private bool isEnabled = true;

    private Quaternion startRotation;
    private float currentPhase;

    private void Start()
    {
        // Store the starting rotation
        startRotation = transform.localRotation;

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

        // Map curved value back to (-1 to 1) range
        float oscillationValue = (curvedValue * 2f) - 1f;

        // Calculate current angle
        float currentAngle = Mathf.Lerp(minAngle, maxAngle, (oscillationValue + 1f) * 0.5f);

        // Apply rotation
        Quaternion targetRotation = startRotation * Quaternion.AngleAxis(currentAngle, rotationAxis.normalized);
        transform.localRotation = targetRotation;
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
    /// Reset to starting rotation
    /// </summary>
    public void ResetRotation()
    {
        if (startRotation != Quaternion.identity)
        {
            transform.localRotation = startRotation;
        }
        currentPhase = randomizePhase ? Random.Range(0f, 1f) * Mathf.PI * 2f : phaseOffset * Mathf.PI * 2f;
    }

    /// <summary>
    /// Set the angle range at runtime
    /// </summary>
    public void SetAngleRange(float min, float max)
    {
        minAngle = min;
        maxAngle = max;
    }

    /// <summary>
    /// Set the frequency at runtime
    /// </summary>
    public void SetFrequency(float newFrequency)
    {
        frequency = Mathf.Max(0.01f, newFrequency);
    }

    /// <summary>
    /// Set the rotation axis at runtime
    /// </summary>
    public void SetRotationAxis(Vector3 axis)
    {
        rotationAxis = axis;
    }

    /// <summary>
    /// Set a new starting rotation
    /// </summary>
    public void SetStartRotation(Quaternion rotation)
    {
        startRotation = rotation;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (frequency < 0.01f)
        {
            frequency = 0.01f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw rotation range visualization
        if (!Application.isPlaying)
        {
            startRotation = transform.localRotation;
        }

        Vector3 worldPosition = transform.position;
        Vector3 normalizedAxis = rotationAxis.normalized;

        // Draw min angle
        Quaternion minRot = startRotation * Quaternion.AngleAxis(minAngle, normalizedAxis);
        Vector3 minDir = minRot * Vector3.right * 0.5f;
        
        // Draw max angle
        Quaternion maxRot = startRotation * Quaternion.AngleAxis(maxAngle, normalizedAxis);
        Vector3 maxDir = maxRot * Vector3.right * 0.5f;

        // Draw the arc
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(worldPosition, worldPosition + minDir);
        Gizmos.DrawLine(worldPosition, worldPosition + maxDir);

        // Draw center line
        Gizmos.color = Color.green;
        Vector3 centerDir = startRotation * Vector3.right * 0.5f;
        Gizmos.DrawLine(worldPosition, worldPosition + centerDir);
    }
#endif
}
