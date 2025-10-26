using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the entrance animation for TurnScene.
/// Background zooms out first, then front elements, then container spins 90 degrees.
/// </summary>
public class TurnSceneIntro : MonoBehaviour
{
    [Header("Animation Targets")]
    [SerializeField] private RectTransform backgroundTransform;
    [SerializeField] private RectTransform frontTransform;
    [SerializeField] private RectTransform containerTransform;

    [Header("Background Zoom Settings")]
    [SerializeField] private float backgroundStartScale = 1.5f;
    [SerializeField] private float backgroundEndScale = 1f;
    [SerializeField] private float backgroundZoomDuration = 0.8f;
    [SerializeField] private AnimationCurve backgroundZoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Front Zoom Settings")]
    [SerializeField] private float frontStartScale = 1.5f;
    [SerializeField] private float frontEndScale = 1f;
    [SerializeField] private float frontZoomDuration = 0.6f;
    [SerializeField] private float frontZoomDelay = 0.3f; // Delay after background starts
    [SerializeField] private AnimationCurve frontZoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Container Spin Settings")]
    [SerializeField] private float spinAngle = 90f;
    [SerializeField] private float spinDuration = 0.5f;
    [SerializeField] private float spinDelay = 0.4f; // Delay after front zoom completes
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Container Fade Settings")]
    [SerializeField] private float containerFadeDuration = 0.5f;
    [SerializeField] private AnimationCurve containerFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Auto Play")]
    [SerializeField] private bool playOnStart = true;

    private CanvasGroup containerCanvasGroup;

    private void Start()
    {
        if (playOnStart)
        {
            // Setup container CanvasGroup for fade-in
            if (containerTransform != null)
            {
                containerCanvasGroup = containerTransform.GetComponent<CanvasGroup>();
                if (containerCanvasGroup == null)
                {
                    containerCanvasGroup = containerTransform.gameObject.AddComponent<CanvasGroup>();
                }
            }

            PlayIntroAnimation();
        }
    }

    /// <summary>
    /// Plays the full intro animation sequence.
    /// Can be called manually if playOnStart is false.
    /// </summary>
    public void PlayIntroAnimation()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // Initialize container fade (start invisible)
        if (containerCanvasGroup != null)
        {
            containerCanvasGroup.alpha = 0f;
        }

        // Initialize scales
        if (backgroundTransform != null)
        {
            backgroundTransform.localScale = Vector3.one * backgroundStartScale;
        }

        if (frontTransform != null)
        {
            frontTransform.localScale = Vector3.one * frontStartScale;
        }

        if (containerTransform != null)
        {
            containerTransform.localRotation = Quaternion.identity;
        }

        // Start container fade-in immediately
        Coroutine containerFade = null;
        if (containerCanvasGroup != null)
        {
            containerFade = StartCoroutine(AnimateFade(
                containerCanvasGroup,
                0f,
                1f,
                containerFadeDuration,
                containerFadeCurve
            ));
        }

        // Start background zoom
        Coroutine backgroundZoom = null;
        if (backgroundTransform != null)
        {
            backgroundZoom = StartCoroutine(AnimateZoom(
                backgroundTransform,
                backgroundStartScale,
                backgroundEndScale,
                backgroundZoomDuration,
                backgroundZoomCurve
            ));
        }

        // Wait for front zoom delay
        yield return new WaitForSeconds(frontZoomDelay);

        // Start front zoom (overlaps with background)
        Coroutine frontZoom = null;
        if (frontTransform != null)
        {
            frontZoom = StartCoroutine(AnimateZoom(
                frontTransform,
                frontStartScale,
                frontEndScale,
                frontZoomDuration,
                frontZoomCurve
            ));
        }

        // Wait for background zoom to complete
        if (backgroundZoom != null)
        {
            yield return backgroundZoom;
        }

        // Wait for front zoom to complete
        if (frontZoom != null)
        {
            yield return frontZoom;
        }

        // Wait before spinning
        yield return new WaitForSeconds(spinDelay);

        // Spin container
        if (containerTransform != null)
        {
            yield return AnimateSpin(
                containerTransform,
                spinAngle,
                spinDuration,
                spinCurve
            );
        }

        Debug.Log("TurnSceneIntro: Animation sequence complete");
    }

    private IEnumerator AnimateZoom(RectTransform target, float startScale, float endScale, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curved = curve.Evaluate(t);

            float scale = Mathf.Lerp(startScale, endScale, curved);
            target.localScale = Vector3.one * scale;

            yield return null;
        }

        target.localScale = Vector3.one * endScale;
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
