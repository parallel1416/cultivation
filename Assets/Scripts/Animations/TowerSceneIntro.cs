using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the intro animation for the Tower scene.
/// Trees move up into position with staggered timing, then fade in tree3 and nodes container.
/// </summary>
public class TowerSceneIntro : MonoBehaviour
{
    [Header("Tree References")]
    [SerializeField] private RectTransform tree1;
    [SerializeField] private RectTransform tree2;
    [SerializeField] private RectTransform tree3;

    [Header("UI References")]
    [SerializeField] private CanvasGroup nodesContainer;

    [Header("Tree1 Animation")]
    [SerializeField] private float tree1Delay = 0f;
    [SerializeField] private float tree1Duration = 0.8f;
    [SerializeField] private float tree1Distance = 300f; // pixels to move up
    [SerializeField] private AnimationCurve tree1Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Tree2 Animation")]
    [SerializeField] private float tree2Delay = 0.3f;
    [SerializeField] private float tree2Duration = 0.5f; // faster than tree1
    [SerializeField] private float tree2Distance = 250f;
    [SerializeField] private AnimationCurve tree2Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Tree3 Fade Animation")]
    [SerializeField] private float tree3Delay = 0.6f;
    [SerializeField] private float tree3Duration = 0.6f;
    [SerializeField] private AnimationCurve tree3Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Nodes Container Fade")]
    [SerializeField] private float nodesFadeDelay = 0.8f;
    [SerializeField] private float nodesFadeDuration = 0.5f;
    [SerializeField] private AnimationCurve nodesFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector2 tree1StartPos;
    private Vector2 tree2StartPos;
    private CanvasGroup tree1CanvasGroup;
    private CanvasGroup tree2CanvasGroup;
    private CanvasGroup tree3CanvasGroup;

    private void Awake()
    {
        // Store initial positions and setup canvas groups for trees 1 and 2
        if (tree1 != null)
        {
            tree1StartPos = tree1.anchoredPosition;
            // Offset tree1 down by the distance it will move
            tree1.anchoredPosition = new Vector2(tree1StartPos.x, tree1StartPos.y - tree1Distance);
            
            // Setup canvas group for fade
            tree1CanvasGroup = tree1.GetComponent<CanvasGroup>();
            if (tree1CanvasGroup == null)
            {
                tree1CanvasGroup = tree1.gameObject.AddComponent<CanvasGroup>();
            }
            tree1CanvasGroup.alpha = 0f;
        }

        if (tree2 != null)
        {
            tree2StartPos = tree2.anchoredPosition;
            // Offset tree2 down by the distance it will move
            tree2.anchoredPosition = new Vector2(tree2StartPos.x, tree2StartPos.y - tree2Distance);
            
            // Setup canvas group for fade
            tree2CanvasGroup = tree2.GetComponent<CanvasGroup>();
            if (tree2CanvasGroup == null)
            {
                tree2CanvasGroup = tree2.gameObject.AddComponent<CanvasGroup>();
            }
            tree2CanvasGroup.alpha = 0f;
        }

        // Setup tree3 canvas group for fade
        if (tree3 != null)
        {
            tree3CanvasGroup = tree3.GetComponent<CanvasGroup>();
            if (tree3CanvasGroup == null)
            {
                tree3CanvasGroup = tree3.gameObject.AddComponent<CanvasGroup>();
            }
            tree3CanvasGroup.alpha = 0f;
        }

        // Setup nodes container
        if (nodesContainer != null)
        {
            nodesContainer.alpha = 0f;
        }
    }

    private void Start()
    {
        PlayIntroAnimation();
    }

    /// <summary>
    /// Manually trigger the intro animation. Can be called from other scripts.
    /// </summary>
    public void PlayIntroAnimation()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // Start all animations with their respective delays
        if (tree1 != null && tree1CanvasGroup != null)
        {
            StartCoroutine(AnimateTreeMoveAndFade(tree1, tree1CanvasGroup, tree1StartPos, tree1Delay, tree1Duration, tree1Curve));
        }

        if (tree2 != null && tree2CanvasGroup != null)
        {
            StartCoroutine(AnimateTreeMoveAndFade(tree2, tree2CanvasGroup, tree2StartPos, tree2Delay, tree2Duration, tree2Curve));
        }

        if (tree3 != null && tree3CanvasGroup != null)
        {
            StartCoroutine(AnimateTreeFade(tree3CanvasGroup, tree3Delay, tree3Duration, tree3Curve));
        }

        if (nodesContainer != null)
        {
            StartCoroutine(AnimateNodesFade(nodesContainer, nodesFadeDelay, nodesFadeDuration, nodesFadeCurve));
        }

        yield return null;
    }

    private IEnumerator AnimateTreeMoveAndFade(RectTransform tree, CanvasGroup canvasGroup, Vector2 targetPos, float delay, float duration, AnimationCurve curve)
    {
        // Wait for delay
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Vector2 startPos = tree.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = curve.Evaluate(t);

            tree.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);
            canvasGroup.alpha = eased;
            yield return null;
        }

        tree.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;
    }

    private IEnumerator AnimateTreeFade(CanvasGroup treeGroup, float delay, float duration, AnimationCurve curve)
    {
        // Wait for delay
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = curve.Evaluate(t);

            treeGroup.alpha = eased;
            yield return null;
        }

        treeGroup.alpha = 1f;
    }

    private IEnumerator AnimateNodesFade(CanvasGroup nodesGroup, float delay, float duration, AnimationCurve curve)
    {
        // Wait for delay
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = curve.Evaluate(t);

            nodesGroup.alpha = eased;
            yield return null;
        }

        nodesGroup.alpha = 1f;
    }
}
