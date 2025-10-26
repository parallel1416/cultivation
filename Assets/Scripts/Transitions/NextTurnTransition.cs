using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the transition when clicking the next turn button:
/// Container spins 90 degrees, turn number increments, fade to black, then load map scene.
/// </summary>
public class NextTurnTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private RectTransform containerTransform;
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Spin Settings")]
    [SerializeField] private float spinAngle = 90f;
    [SerializeField] private float spinDuration = 0.5f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "MapScene";

    private bool isTransitioning = false;

    private void Awake()
    {
        // Auto-find button if not assigned
        if (nextTurnButton == null)
        {
            nextTurnButton = GetComponent<Button>();
        }

        // Setup fade canvas if assigned
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.AddListener(OnNextTurnClicked);
        }
    }

    private void OnDisable()
    {
        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.RemoveListener(OnNextTurnClicked);
        }
    }

    private void OnNextTurnClicked()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("NextTurnTransition: Already transitioning, ignoring click");
            return;
        }

        Debug.Log("NextTurnTransition: Starting transition sequence");
        StartCoroutine(TransitionSequence());
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // Disable button to prevent multiple clicks
        if (nextTurnButton != null)
        {
            nextTurnButton.interactable = false;
        }

        // Step 1: Spin container 90 degrees
        if (containerTransform != null)
        {
            Debug.Log("NextTurnTransition: Spinning container");
            yield return AnimateSpin(
                containerTransform,
                spinAngle,
                spinDuration,
                spinCurve
            );
        }

        // Step 2: Advance to next turn (handled by TurnManager.NextTurn)
        if (TurnManager.Instance != null)
        {
            int previousTurn = TurnManager.Instance.CurrentTurn;
            TurnManager.Instance.NextTurn();
            Debug.Log($"NextTurnTransition: Turn advanced from {previousTurn} to {TurnManager.Instance.CurrentTurn}");
        }
        else
        {
            Debug.LogWarning("NextTurnTransition: TurnManager not found, skipping turn advancement");
        }

        // Step 3: Fade to black
        if (fadeCanvasGroup != null)
        {
            Debug.Log("NextTurnTransition: Fading to black");
            fadeCanvasGroup.blocksRaycasts = true;
            yield return AnimateFade(
                fadeCanvasGroup,
                0f,
                1f,
                fadeDuration,
                fadeCurve
            );
        }

        // Step 4: Load map scene
        Debug.Log($"NextTurnTransition: Loading scene '{targetSceneName}'");
        SceneManager.LoadScene(targetSceneName);
    }

    private IEnumerator AnimateSpin(RectTransform target, float angle, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        Quaternion startRotation = target.localRotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, angle);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curved = curve.Evaluate(t);

            target.localRotation = Quaternion.Lerp(startRotation, endRotation, curved);

            yield return null;
        }

        target.localRotation = endRotation;
    }

    private IEnumerator AnimateFade(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curved = curve.Evaluate(t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curved);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}
