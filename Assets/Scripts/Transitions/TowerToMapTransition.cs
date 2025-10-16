using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles shrinking the tower and switching to the map scene with a reverse dolly zoom effect.
/// Attach to an empty GameObject in TowerScene. Assign references in Inspector.
/// </summary>
public class TowerToMapTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform tower; // Assign the tower image transform
    [SerializeField] private Transform background; // Assign the background image transform
    [SerializeField] private Button backButton; // Assign the BackButton
    [SerializeField] private float towerShrinkTarget = 0.5f;
    [SerializeField] private float backgroundShrinkTarget = 0.33f;
    [SerializeField] private float shrinkDuration = 0.7f;
    [SerializeField] private string mapSceneName = "MapScene";

    private Vector3 towerOriginalScale;
    private Vector3 backgroundOriginalScale;
    private bool isAnimating = false;

    private void Awake()
    {
        if (tower != null) towerOriginalScale = tower.localScale;
        if (background != null) backgroundOriginalScale = background.localScale;
        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
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
        Vector3 towerStart = towerOriginalScale;
        Vector3 towerEnd = towerOriginalScale * towerShrinkTarget;
        Vector3 bgStart = background != null ? backgroundOriginalScale : Vector3.one;
        Vector3 bgEnd = background != null ? backgroundOriginalScale * backgroundShrinkTarget : Vector3.one * backgroundShrinkTarget;

        // Optional: fade overlay
        CanvasGroup fade = null;
        if (background != null)
        {
            fade = background.GetComponentInParent<CanvasGroup>();
            if (fade == null)
            {
                GameObject go = new GameObject("FadeOverlay");
                go.transform.SetParent(background.parent, false);
                fade = go.AddComponent<CanvasGroup>();
                fade.transform.SetAsLastSibling();
                fade.alpha = 0f;
                RectTransform r = go.AddComponent<RectTransform>();
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            }
        }

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shrinkDuration);
            if (tower != null)
                tower.localScale = Vector3.Lerp(towerStart, towerEnd, t);
            if (background != null)
                background.localScale = Vector3.Lerp(bgStart, bgEnd, t);
            if (fade != null)
                fade.alpha = t * 0.8f;
            yield return null;
        }
        if (tower != null) tower.localScale = towerEnd;
        if (background != null) background.localScale = bgEnd;
        if (fade != null) fade.alpha = 1f;

    // Wait a short moment, then load MapScene
    yield return new WaitForSeconds(0.1f);
    MapZoomInOnLoad.MarkComingFromTowerScene();
    SceneManager.LoadScene(mapSceneName);
        // (MapScene should handle its own zoom-in)
        isAnimating = false;
    }
}