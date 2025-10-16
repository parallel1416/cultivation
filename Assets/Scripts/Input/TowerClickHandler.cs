using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// When clicking TowerPreview, smoothly scale up the image, scale up background faster for dolly zoom,
/// and move all children of Canvas/placesContainer outward.
/// </summary>
public class TowerClickHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform towerPreview; // Assign the TowerPreview image transform
    [SerializeField] private Transform background; // Assign the background image transform
    [SerializeField] private Transform placesContainer; // Assign Canvas/placesContainer
    [SerializeField] private float towerScaleTarget = 2.0f;
    [SerializeField] private float backgroundScaleTarget = 3.0f;
    [SerializeField] private float scaleDuration = 0.7f;
    [SerializeField] private float placesMoveDistance = 8f;
    [SerializeField] private float placesMoveDuration = 0.5f;

    private Vector3[] placesOriginalPositions;
    private bool isAnimating = false;

    private void Awake()
    {
        CachePlacesPositions();
    }

    private void OnEnable()
    {
        CachePlacesPositions();
    }

    private void CachePlacesPositions()
    {
        if (placesContainer == null) return;
        int count = placesContainer.childCount;
        if (placesOriginalPositions == null || placesOriginalPositions.Length != count)
        {
            placesOriginalPositions = new Vector3[count];
        }
        for (int i = 0; i < count; i++)
        {
            placesOriginalPositions[i] = placesContainer.GetChild(i).localPosition;
        }
    }

    private void Update()
    {
        if (towerPreview == null || isAnimating) return;
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to towerPreview
            Vector3 mousePos = Input.mousePosition;
            RectTransform rt = towerPreview.GetComponent<RectTransform>();
            if (rt != null)
            {
                Canvas canvas = rt.GetComponentInParent<Canvas>();
                if (canvas != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mousePos, canvas.worldCamera, out Vector2 localPoint))
                {
                    if (rt.rect.Contains(localPoint))
                    {
                        StartCoroutine(DollyZoomEffect());
                    }
                }
            }
            else
            {
                // Fallback for non-UI objects
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);
                if (Vector2.Distance(worldPoint, towerPreview.position) < 1f) // crude hit
                {
                    StartCoroutine(DollyZoomEffect());
                }
            }
        }
    }

    private System.Collections.IEnumerator DollyZoomEffect()
    {
    isAnimating = true;
    float elapsed = 0f;
    Vector3 towerStart = towerPreview != null ? towerPreview.localScale : Vector3.one;
    Vector3 towerEnd = towerStart * towerScaleTarget;
    Vector3 bgStart = background != null ? background.localScale : Vector3.one;
    Vector3 bgEnd = background != null ? background.localScale * backgroundScaleTarget : Vector3.one * backgroundScaleTarget;

        // Calculate outward directions for places
        int placesCount = placesContainer != null ? placesContainer.childCount : 0;
        Vector3[] placesStart = new Vector3[placesCount];
        Vector3[] placesEnd = new Vector3[placesCount];
        for (int i = 0; i < placesCount; i++)
        {
            Transform place = placesContainer.GetChild(i);
            Vector3 origin = placesOriginalPositions != null && i < placesOriginalPositions.Length
                ? placesOriginalPositions[i]
                : place.localPosition;
            placesStart[i] = origin;
            Vector3 dir = (origin - Vector3.zero).normalized;
            if (dir == Vector3.zero) dir = Vector3.up;
            placesEnd[i] = origin + dir * placesMoveDistance;
        }

        // Optional: fade out overlay
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

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            float tBg = Mathf.Clamp01(elapsed / (scaleDuration * 0.7f)); // background scales faster
            if (towerPreview != null)
                towerPreview.localScale = Vector3.Lerp(towerStart, towerEnd, t);
            if (background != null)
                background.localScale = Vector3.Lerp(bgStart, bgEnd, tBg);
            for (int i = 0; i < placesCount; i++)
            {
                if (placesContainer != null)
                    placesContainer.GetChild(i).localPosition = Vector3.Lerp(placesStart[i], placesEnd[i], t);
            }
            if (fade != null)
                fade.alpha = t * 0.8f; // fade in overlay
            yield return null;
        }
        // Snap to final
        if (towerPreview != null)
            towerPreview.localScale = towerEnd;
        if (background != null)
            background.localScale = bgEnd;
        for (int i = 0; i < placesCount; i++)
        {
            if (placesContainer != null)
                placesContainer.GetChild(i).localPosition = placesEnd[i];
        }
        if (fade != null) fade.alpha = 1f;

        // Wait a short moment, then load TowerScene
        yield return new WaitForSeconds(0.1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("TowerScene");
        // (TowerScene should handle its own fade-in)
        isAnimating = false;
    }
}
