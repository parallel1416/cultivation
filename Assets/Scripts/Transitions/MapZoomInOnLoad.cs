using UnityEngine;
/// <summary>
/// On MapScene load, if coming from TowerScene, starts zoomed in and animates to normal scale.
/// Attach to an empty GameObject in MapScene. Assign references in Inspector.
/// </summary>
public class MapZoomInOnLoad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform towerPreview;
    [SerializeField] private Transform background;
    [SerializeField] private float towerZoomedScale = 2.0f;
    [SerializeField] private float backgroundZoomedScale = 3.0f;
    [SerializeField] private float zoomDuration = 0.7f;

    private Vector3 towerNormalScale = Vector3.one;
    private Vector3 backgroundNormalScale = Vector3.one;
    private bool shouldZoomIn;

    private static bool comingFromTowerScene;

    static MapZoomInOnLoad()
    {
        comingFromTowerScene = false;
    }

    private void Awake()
    {
        if (towerPreview != null)
            towerNormalScale = towerPreview.localScale;
        if (background != null)
            backgroundNormalScale = background.localScale;

    shouldZoomIn = comingFromTowerScene;
        if (shouldZoomIn)
        {
            SetScale(towerPreview, towerNormalScale * towerZoomedScale);
            SetScale(background, backgroundNormalScale * backgroundZoomedScale);
        }
        else
        {
            SetScale(towerPreview, towerNormalScale);
            SetScale(background, backgroundNormalScale);
        }
    }

    private void Start()
    {
        if (!shouldZoomIn)
        {
            comingFromTowerScene = false;
            return;
        }

        StartCoroutine(ZoomOutEffect());
    }

    private System.Collections.IEnumerator ZoomOutEffect()
    {
        float elapsed = 0f;
        Vector3 towerStart = towerNormalScale * towerZoomedScale;
        Vector3 towerEnd = towerNormalScale;
        Vector3 backgroundStart = backgroundNormalScale * backgroundZoomedScale;
        Vector3 backgroundEnd = backgroundNormalScale;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            SetScale(towerPreview, Vector3.Lerp(towerStart, towerEnd, t));
            SetScale(background, Vector3.Lerp(backgroundStart, backgroundEnd, t));
            yield return null;
        }

        SetScale(towerPreview, towerEnd);
        SetScale(background, backgroundEnd);
    comingFromTowerScene = false;
    shouldZoomIn = false;
    }

    private static void SetScale(Transform target, Vector3 scale)
    {
        if (target != null)
            target.localScale = scale;
    }

    public static void MarkComingFromTowerScene()
    {
        comingFromTowerScene = true;
    }

    public static void ResetFlag()
    {
        comingFromTowerScene = false;
    }
}
