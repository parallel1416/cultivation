using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles fading out TowerScene and switching back to MapScene.
/// Attach to an empty GameObject in TowerScene. Assign references in Inspector.
/// </summary>
public class TowerToMapTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform fadeParent; // Parent for fade overlay (e.g., Canvas root)
    [SerializeField] private Button backButton; // Optional manual assignment
    [SerializeField] private string backButtonName = "BackButton";
    [SerializeField] private float fadeDuration = 0.7f;
    [SerializeField] private string mapSceneName = "MapScene";

    private bool isAnimating = false;
    private CanvasGroup fadeOverlay;

    private void Awake()
    {
        SetupFadeOverlay();
    }

    private void Start()
    {
        ResolveBackButton();
    }

    private void OnDestroy()
    {
        if (backButton != null) backButton.onClick.RemoveListener(OnBackPressed);
    }

    private void OnBackPressed()
    {
        if (!isAnimating)
            StartCoroutine(ShrinkAndSwitch());
    }

    private System.Collections.IEnumerator ShrinkAndSwitch()
    {
        isAnimating = true;
        float elapsed = 0f;
        if (fadeOverlay == null)
        {
            SetupFadeOverlay();
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            if (fadeOverlay != null)
                fadeOverlay.alpha = t;
            yield return null;
        }

        if (fadeOverlay != null)
            fadeOverlay.alpha = 1f;

        yield return new WaitForSeconds(0.1f);

        MapZoomInOnLoad.MarkComingFromTowerScene();
        SceneManager.LoadScene(mapSceneName);
        isAnimating = false;
    }

    private void ResolveBackButton()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackPressed);
            return;
        }

        if (!string.IsNullOrEmpty(backButtonName))
        {
            GameObject buttonObj = GameObject.Find(backButtonName);
            if (buttonObj != null)
            {
                backButton = buttonObj.GetComponent<Button>();
            }
        }

        if (backButton == null)
        {
            Debug.LogError($"TowerToMapTransition could not find a Button named '{backButtonName}'.", this);
            return;
        }

        backButton.onClick.AddListener(OnBackPressed);
    }

    private void SetupFadeOverlay()
    {
        if (fadeOverlay != null) return;

        Transform parent = fadeParent != null ? fadeParent : transform;
        GameObject overlay = new GameObject("FadeOverlay");
        overlay.transform.SetParent(parent, false);
        overlay.transform.SetAsLastSibling();

        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = overlay.AddComponent<Image>();
        image.color = Color.black;

        fadeOverlay = overlay.AddComponent<CanvasGroup>();
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
    }
}