using UnityEngine;

/// <summary>
/// Makes an object rotate erratically with random direction and speed changes,
/// and randomly scale between large, normal, and small sizes.
/// </summary>
public class ErraticRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float minRotationSpeed = 30f;
    [SerializeField] private float maxRotationSpeed = 180f;
    [SerializeField] private bool canReverseDirection = true;

    [Header("Scale Settings")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float largeScale = 1.5f;
    [SerializeField] private float smallScale = 0.7f;
    [SerializeField] private float scaleTransitionSpeed = 2f;

    [Header("Timing Settings")]
    [SerializeField] private float minBehaviorChangeInterval = 1f;
    [SerializeField] private float maxBehaviorChangeInterval = 5f;

    [Header("Control")]
    [SerializeField] private bool isEnabled = true;

    private enum ScaleState
    {
        Normal,
        Large,
        Small
    }

    private float currentRotationSpeed;
    private int rotationDirection; // 1 or -1
    private ScaleState targetScaleState;
    private float targetScale;
    private float currentScale;
    private float nextBehaviorChangeTime;
    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
        currentScale = normalScale;
        
        // Initialize with random values
        RandomizeRotation();
        RandomizeScale();
        ScheduleNextBehaviorChange();
    }

    private void Update()
    {
        if (!isEnabled)
        {
            return;
        }

        // Apply rotation
        float rotationAmount = currentRotationSpeed * rotationDirection * Time.deltaTime;
        transform.Rotate(rotationAxis.normalized, rotationAmount, Space.Self);

        // Smoothly interpolate scale
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleTransitionSpeed);
        transform.localScale = initialScale * currentScale;

        // Check if it's time to change behavior
        if (Time.time >= nextBehaviorChangeTime)
        {
            RandomizeBehavior();
            ScheduleNextBehaviorChange();
        }
    }

    /// <summary>
    /// Randomize both rotation and scale
    /// </summary>
    private void RandomizeBehavior()
    {
        // Randomly decide whether to change rotation, scale, or both
        float randomChoice = Random.value;
        
        if (randomChoice < 0.33f)
        {
            // Change only rotation
            RandomizeRotation();
        }
        else if (randomChoice < 0.66f)
        {
            // Change only scale
            RandomizeScale();
        }
        else
        {
            // Change both
            RandomizeRotation();
            RandomizeScale();
        }
    }

    /// <summary>
    /// Randomize rotation speed and direction
    /// </summary>
    private void RandomizeRotation()
    {
        currentRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        
        if (canReverseDirection)
        {
            rotationDirection = Random.value > 0.5f ? 1 : -1;
        }
        else
        {
            rotationDirection = 1;
        }
    }

    /// <summary>
    /// Randomly pick one of the three scale states
    /// </summary>
    private void RandomizeScale()
    {
        int randomScale = Random.Range(0, 3);
        
        switch (randomScale)
        {
            case 0:
                targetScaleState = ScaleState.Large;
                targetScale = largeScale;
                break;
            case 1:
                targetScaleState = ScaleState.Normal;
                targetScale = normalScale;
                break;
            case 2:
                targetScaleState = ScaleState.Small;
                targetScale = smallScale;
                break;
        }
    }

    /// <summary>
    /// Schedule the next behavior change at a random interval
    /// </summary>
    private void ScheduleNextBehaviorChange()
    {
        float interval = Random.Range(minBehaviorChangeInterval, maxBehaviorChangeInterval);
        nextBehaviorChangeTime = Time.time + interval;
    }

    /// <summary>
    /// Enable the erratic behavior
    /// </summary>
    public void Enable()
    {
        isEnabled = true;
    }

    /// <summary>
    /// Disable the erratic behavior
    /// </summary>
    public void Disable()
    {
        isEnabled = false;
    }

    /// <summary>
    /// Reset to initial state
    /// </summary>
    public void ResetToInitialState()
    {
        transform.localScale = initialScale;
        currentScale = normalScale;
        targetScale = normalScale;
        currentRotationSpeed = 0f;
    }

    /// <summary>
    /// Set custom rotation speed range
    /// </summary>
    public void SetRotationSpeedRange(float min, float max)
    {
        minRotationSpeed = min;
        maxRotationSpeed = max;
        RandomizeRotation();
    }

    /// <summary>
    /// Set custom scale values
    /// </summary>
    public void SetScaleValues(float normal, float large, float small)
    {
        normalScale = normal;
        largeScale = large;
        smallScale = small;
    }

    /// <summary>
    /// Set custom behavior change interval range
    /// </summary>
    public void SetBehaviorChangeInterval(float min, float max)
    {
        minBehaviorChangeInterval = min;
        maxBehaviorChangeInterval = max;
    }

    /// <summary>
    /// Force an immediate behavior change
    /// </summary>
    public void ForceBehaviorChange()
    {
        RandomizeBehavior();
        ScheduleNextBehaviorChange();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure min/max values are valid
        if (minRotationSpeed < 0f) minRotationSpeed = 0f;
        if (maxRotationSpeed < minRotationSpeed) maxRotationSpeed = minRotationSpeed;
        
        if (minBehaviorChangeInterval < 0.1f) minBehaviorChangeInterval = 0.1f;
        if (maxBehaviorChangeInterval < minBehaviorChangeInterval) maxBehaviorChangeInterval = minBehaviorChangeInterval;
        
        if (normalScale <= 0f) normalScale = 1f;
        if (largeScale < normalScale) largeScale = normalScale * 1.5f;
        if (smallScale <= 0f) smallScale = normalScale * 0.5f;
        
        if (scaleTransitionSpeed < 0.1f) scaleTransitionSpeed = 0.1f;
    }
#endif
}
