using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the animated transition from Menu to Map scene.
/// </summary>
public class MenuToMapTransition : TransitionAnimator
{
    [Header("Button Reference")]
    [SerializeField] private Button startButton;

    [Header("Move Elements")]
    [SerializeField] private RectTransform title;
    [SerializeField] private RectTransform smoke;
    [SerializeField] private RectTransform hand;
    [SerializeField] private RectTransform flame;

    [Header("Move Offsets")]
    [SerializeField] private Vector2 titleOffset = new Vector2(0f, 240f);
    [SerializeField] private Vector2 smokeOffset = new Vector2(0f, 200f);
    [SerializeField] private Vector2 handOffset = new Vector2(0f, -260f);
    [SerializeField] private Vector2 flameOffset = new Vector2(0f, -220f);

    [Header("Scale Elements")]
    [SerializeField] private RectTransform menuBackground;
    [SerializeField] private RectTransform outerRing;
    [SerializeField] private RectTransform outerFlames;

    [Header("Scale Multipliers")]
    [SerializeField] private Vector3 menuBackgroundScaleMultiplier = new Vector3(1.15f, 1.15f, 1f);
    [SerializeField] private Vector3 outerRingScaleMultiplier = new Vector3(1.3f, 1.3f, 1f);
    [SerializeField] private Vector3 outerFlamesScaleMultiplier = new Vector3(1.3f, 1.3f, 1f);

    [Header("Timing")]
    [SerializeField] private float movementDuration = 1.4f;
    [SerializeField] private float scaleDuration = 1.6f;
    [SerializeField] private float fadeDelay = 0.3f;
    [SerializeField] private float fadeDuration = 1.2f;

    private CanvasGroup fadeCanvasGroup;

    private Vector2 titleStartPos;
    private Vector2 smokeStartPos;
    private Vector2 handStartPos;
    private Vector2 flameStartPos;

    private Vector3 menuBackgroundStartScale;
    private Vector3 outerRingStartScale;
    private Vector3 outerFlamesStartScale;

    protected override void Awake()
    {
        base.Awake();
        CreateFadeOverlay();
        CacheStartStates();
    }

    private void OnEnable()
    {
        ResetElements();
    }

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    private void CacheStartStates()
    {
        if (title != null) titleStartPos = title.anchoredPosition;
        if (smoke != null) smokeStartPos = smoke.anchoredPosition;
        if (hand != null) handStartPos = hand.anchoredPosition;
        if (flame != null) flameStartPos = flame.anchoredPosition;

        if (menuBackground != null) menuBackgroundStartScale = menuBackground.localScale;
        if (outerRing != null) outerRingStartScale = outerRing.localScale;
        if (outerFlames != null) outerFlamesStartScale = outerFlames.localScale;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }
    }

    private void ResetElements()
    {
        if (title != null) title.anchoredPosition = titleStartPos;
        if (smoke != null) smoke.anchoredPosition = smokeStartPos;
        if (hand != null) hand.anchoredPosition = handStartPos;
        if (flame != null) flame.anchoredPosition = flameStartPos;

        if (menuBackground != null) menuBackground.localScale = menuBackgroundStartScale;
        if (outerRing != null) outerRing.localScale = outerRingStartScale;
        if (outerFlames != null) outerFlames.localScale = outerFlamesStartScale;

        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;

        if (startButton != null)
        {
            startButton.interactable = true;
        }
    }

    private void OnStartButtonClicked()
    {
        GameManager.StartNewGame();
        StartCoroutine(TransitionToMapScene());
    }

    private IEnumerator TransitionToMapScene()
    {
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        int runningAnimations = 0;

        IEnumerator WrapRoutine(IEnumerator routine)
        {
            yield return routine;
            runningAnimations--;
        }

        void Launch(IEnumerator routine)
        {
            if (routine == null)
            {
                return;
            }

            runningAnimations++;
            StartCoroutine(WrapRoutine(routine));
        }

        Launch(AnimateAnchoredPosition(title, titleStartPos, titleStartPos + titleOffset, movementDuration));
        Launch(AnimateAnchoredPosition(smoke, smokeStartPos, smokeStartPos + smokeOffset, movementDuration));
        Launch(AnimateAnchoredPosition(hand, handStartPos, handStartPos + handOffset, movementDuration));
        Launch(AnimateAnchoredPosition(flame, flameStartPos, flameStartPos + flameOffset, movementDuration));

        Launch(AnimateScale(menuBackground, menuBackgroundStartScale, menuBackgroundScaleMultiplier, scaleDuration));
        Launch(AnimateScale(outerRing, outerRingStartScale, outerRingScaleMultiplier, scaleDuration));
        Launch(AnimateScale(outerFlames, outerFlamesStartScale, outerFlamesScaleMultiplier, scaleDuration));

        while (runningAnimations > 0)
        {
            yield return null;
        }

        if (fadeDelay > 0f)
        {
            yield return new WaitForSeconds(fadeDelay);
        }

        if (fadeCanvasGroup != null)
        {
            yield return FadeCanvasGroup(fadeCanvasGroup, fadeCanvasGroup.alpha, 1f, fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        SceneManager.LoadScene("MapScene");
    }

    private IEnumerator AnimateAnchoredPosition(RectTransform target, Vector2 start, Vector2 end, float duration)
    {
        if (target == null || duration <= 0f)
        {
            yield break;
        }

        Vector3 from = new Vector3(start.x, start.y, 0f);
        Vector3 to = new Vector3(end.x, end.y, 0f);

        yield return AnimateVector3(
            from,
            to,
            duration,
            easeCurve,
            pos => target.anchoredPosition = new Vector2(pos.x, pos.y)
        );
    }

    private IEnumerator AnimateScale(RectTransform target, Vector3 startScale, Vector3 scaleMultiplier, float duration)
    {
        if (target == null || duration <= 0f)
        {
            yield break;
        }

        Vector3 endScale = Vector3.Scale(startScale, scaleMultiplier);

        yield return AnimateVector3(
            startScale,
            endScale,
            duration,
            easeCurve,
            scale => target.localScale = scale
        );
    }

    private void CreateFadeOverlay()
    {
        // Find canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogError("MenuToMapTransition: No Canvas found in scene");
            return;
        }

        // Create fade overlay GameObject
        GameObject fadeObject = new GameObject("FadeOverlay_MenuToMap");
        fadeObject.transform.SetParent(canvas.transform, false);

        // Setup RectTransform to cover entire screen
        RectTransform rectTransform = fadeObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Add Image component for black color
        Image image = fadeObject.AddComponent<Image>();
        image.color = Color.black;

        // Add CanvasGroup for fading
        fadeCanvasGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        // Set as last sibling to be on top
        rectTransform.SetAsLastSibling();

        Debug.Log("MenuToMapTransition: Fade overlay created");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure multipliers stay positive and keep Z at 1 for UI elements.
        menuBackgroundScaleMultiplier = SanitizeMultiplier(menuBackgroundScaleMultiplier);
        outerRingScaleMultiplier = SanitizeMultiplier(outerRingScaleMultiplier);
        outerFlamesScaleMultiplier = SanitizeMultiplier(outerFlamesScaleMultiplier);
    }

    private static Vector3 SanitizeMultiplier(Vector3 value)
    {
        value.x = Mathf.Max(0.01f, value.x);
        value.y = Mathf.Max(0.01f, value.y);
        value.z = Mathf.Approximately(value.z, 0f) ? 1f : value.z;
        return value;
    }
#endif
}
