using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base class for scene transition animations
/// Provides common utilities and animation curves
/// </summary>
public abstract class TransitionAnimator : MonoBehaviour
{
    [Header("Animation Curves")]
    [SerializeField] protected AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected AnimationCurve zoomCurve;

    protected virtual void Awake()
    {
        // Create default zoom curve if not set
        if (zoomCurve == null || zoomCurve.length == 0)
        {
            zoomCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 0),
                new Keyframe(0.5f, 0.5f, 2, 2),
                new Keyframe(1, 1, 0, 0)
            );
        }
    }

    /// <summary>
    /// Lerp with custom animation curve
    /// </summary>
    protected float CurveLerp(float start, float end, float t, AnimationCurve curve)
    {
        float curveValue = curve.Evaluate(t);
        return Mathf.Lerp(start, end, curveValue);
    }

    /// <summary>
    /// Lerp Vector3 with custom animation curve
    /// </summary>
    protected Vector3 CurveLerp(Vector3 start, Vector3 end, float t, AnimationCurve curve)
    {
        float curveValue = curve.Evaluate(t);
        return Vector3.Lerp(start, end, curveValue);
    }

    /// <summary>
    /// Lerp Color with custom animation curve
    /// </summary>
    protected Color CurveLerp(Color start, Color end, float t, AnimationCurve curve)
    {
        float curveValue = curve.Evaluate(t);
        return Color.Lerp(start, end, curveValue);
    }

    /// <summary>
    /// Smoothly animate a float value
    /// </summary>
    protected IEnumerator AnimateFloat(float from, float to, float duration, AnimationCurve curve, System.Action<float> onUpdate)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = CurveLerp(from, to, t, curve);
            onUpdate?.Invoke(value);
            yield return null;
        }
        
        // Ensure final value is set
        onUpdate?.Invoke(to);
    }

    /// <summary>
    /// Smoothly animate a Vector3 value
    /// </summary>
    protected IEnumerator AnimateVector3(Vector3 from, Vector3 to, float duration, AnimationCurve curve, System.Action<Vector3> onUpdate)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 value = CurveLerp(from, to, t, curve);
            onUpdate?.Invoke(value);
            yield return null;
        }
        
        // Ensure final value is set
        onUpdate?.Invoke(to);
    }

    /// <summary>
    /// Fade a CanvasGroup
    /// </summary>
    protected IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        
        canvasGroup.alpha = to;
    }

    /// <summary>
    /// Fade a SpriteRenderer
    /// </summary>
    protected IEnumerator FadeSpriteRenderer(SpriteRenderer spriteRenderer, float from, float to, float duration)
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;
        Color color = spriteRenderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(from, to, t);
            spriteRenderer.color = color;
            yield return null;
        }
        
        color.a = to;
        spriteRenderer.color = color;
    }
}
