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
    private CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        // Auto-find button if not assigned
        if (nextTurnButton == null)
        {
            nextTurnButton = GetComponent<Button>();
        }

        // Create fade overlay
        CreateFadeOverlay();
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

        // Step 2: Advance to next turn (turn+1)
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
        else
        {
            Debug.LogWarning("NextTurnTransition: Fade canvas group not created, skipping fade");
            // Wait a brief moment to simulate fade duration
            yield return new WaitForSeconds(fadeDuration);
        }

        // Step 4: Load map scene
        Debug.Log($"NextTurnTransition: Loading scene '{targetSceneName}'");
        SceneManager.LoadScene(targetSceneName);
    }

    private void CreateFadeOverlay()
    {
        // Find canvas by searching up the parent hierarchy
        Canvas canvas = GetComponentInParent<Canvas>();
        
        // If not found, try searching from root up
        if (canvas == null)
        {
            Transform current = transform;
            while (current != null && canvas == null)
            {
                canvas = current.GetComponent<Canvas>();
                current = current.parent;
            }
        }
        
        // If still not found, try to find any canvas in the scene
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        if (canvas == null)
        {
            Debug.LogError("NextTurnTransition: No Canvas found in parent hierarchy or scene");
            return;
        }

        Debug.Log($"NextTurnTransition: Found canvas '{canvas.name}'");

        // Create fade overlay GameObject
        GameObject fadeObject = new GameObject("FadeOverlay");
        fadeObject.transform.SetParent(canvas.transform, false);

        // Setup RectTransform to cover entire screen
        RectTransform rectTransform = fadeObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Add Image component for black color
        UnityEngine.UI.Image image = fadeObject.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;

        // Add CanvasGroup for fading
        fadeCanvasGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        // Set as last sibling to be on top
        rectTransform.SetAsLastSibling();

        Debug.Log("NextTurnTransition: Fade overlay created");
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
