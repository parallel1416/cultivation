using System.Collections;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles spinning animation for a circular menu UI element.
/// Can be triggered manually, automatically on enable, or by button click.
/// </summary>
public class CircularMenuSpin : MonoBehaviour
{
    [Header("Button Reference")]
    [SerializeField] private Button startButton;
    [SerializeField] private string startButtonName = "StartButton";

    [Header("Rotation Settings")]
    [SerializeField] private float totalRotation = 360f; // Total degrees to rotate
    [SerializeField] private float spinDuration = 2.5f;
    [SerializeField] private bool spinClockwise = true;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Curve will be mirrored for deceleration

    [Header("Auto Play")]
    [SerializeField] private bool playOnEnable = false;

    private RectTransform rectTransform;
    private bool isSpinning = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogError("CircularMenuSpin: No RectTransform found on GameObject", this);
        }
    }

    private void Start()
    {
        ResolveStartButton();
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            StartSpin();
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    private void OnStartButtonClicked()
    {
        StartSpin();
    }

    private void ResolveStartButton()
    {
        if (startButton != null)
        {
            Debug.Log($"CircularMenuSpin: Start button assigned in Inspector: {startButton.name}");
            startButton.onClick.AddListener(OnStartButtonClicked);
            return;
        }

        // Try to find by name
        if (!string.IsNullOrEmpty(startButtonName))
        {
            GameObject buttonObj = GameObject.Find(startButtonName);
            if (buttonObj != null)
            {
                startButton = buttonObj.GetComponent<Button>();
                if (startButton != null)
                {
                    Debug.Log($"CircularMenuSpin: Start button found by name: {startButton.name}");
                }
            }
        }

        if (startButton == null)
        {
            Debug.LogWarning($"CircularMenuSpin: Could not find a Button named '{startButtonName}'. Spin will not trigger on button click.", this);
            return;
        }

        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    /// <summary>
    /// Starts the spinning animation.
    /// </summary>
    public void StartSpin()
    {
        if (rectTransform == null)
        {
            Debug.LogWarning("CircularMenuSpin: Cannot spin, RectTransform is null");
            return;
        }

        if (isSpinning)
        {
            Debug.LogWarning("CircularMenuSpin: Already spinning");
            return;
        }

        StartCoroutine(SpinCoroutine());
    }

    /// <summary>
    /// Stops the spinning animation.
    /// </summary>
    public void StopSpin()
    {
        StopAllCoroutines();
        isSpinning = false;
    }

    private IEnumerator SpinCoroutine()
    {
        if (rectTransform == null || spinDuration <= 0f)
        {
            yield break;
        }

        isSpinning = true;
        float elapsed = 0f;
        float direction = spinClockwise ? -1f : 1f;
        float startAngle = rectTransform.localEulerAngles.z;

        Debug.Log($"CircularMenuSpin: Starting spin. Initial angle: {startAngle}");

        while (elapsed < spinDuration)
        {
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float currentRotation;
            
            if (t < 0.5f)
            {
                // First half: Accelerate (0 to 180 degrees)
                float accelT = t * 2f; // Map 0-0.5 to 0-1
                float accelProgress = accelerationCurve.Evaluate(accelT);
                currentRotation = 180f * accelProgress; // 0 to 180 degrees
            }
            else
            {
                // Second half: Decelerate (180 to 360 degrees)
                // X-flip the curve: evaluate at (1 - t) instead of t
                float decelT = (t - 0.5f) * 2f; // Map 0.5-1 to 0-1
                float decelProgress = 1 - accelerationCurve.Evaluate(1f - decelT);
                currentRotation = 180f + (180f * decelProgress); // 180 to 360 degrees
            }
            
            float targetAngle = startAngle + (currentRotation * direction);
            
            // Set the angle directly to avoid accumulation errors
            Vector3 euler = rectTransform.localEulerAngles;
            euler.z = targetAngle;
            rectTransform.localEulerAngles = euler;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Set final angle precisely to avoid floating point errors
        Vector3 finalEuler = rectTransform.localEulerAngles;
        finalEuler.z = startAngle + (totalRotation * direction);
        rectTransform.localEulerAngles = finalEuler;

        isSpinning = false;
        Debug.Log($"CircularMenuSpin: Spin complete. Rotated {totalRotation} degrees. Final angle: {rectTransform.localEulerAngles.z}");
    }
}
