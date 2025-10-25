using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles fading from MapScene to TowerScene when TreeButton is clicked.
/// Attach to an empty GameObject in MapScene and assign the TreeButton reference.
/// </summary>
public class MapToTowerTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button treeButton;
    [SerializeField] private string treeButtonName = "TreeButton";
    [SerializeField] private Transform fadeParent; // Parent for fade overlay (e.g., Canvas root)
    
    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.7f;
    [SerializeField] private string towerSceneName = "TowerScene";

    private bool isTransitioning = false;
    private CanvasGroup fadeOverlay;

    private void Awake()
    {
        SetupFadeOverlay();
    }

    private void Start()
    {
        ResolveTreeButton();
    }

    private void OnDestroy()
    {
        if (treeButton != null)
        {
            treeButton.onClick.RemoveListener(OnTreeButtonClicked);
        }
    }

    private void OnTreeButtonClicked()
    {
        Debug.Log("TreeButton clicked! Starting transition...");
        
        if (!isTransitioning)
        {
            StartCoroutine(FadeToTowerScene());
        }
        else
        {
            Debug.LogWarning("Transition already in progress, ignoring click.");
        }
    }

    private IEnumerator FadeToTowerScene()
    {
        isTransitioning = true;
        
        // Setup fade overlay
        if (fadeOverlay == null)
        {
            SetupFadeOverlay();
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
        }

        // Fade to black
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = t;
            }
            
            yield return null;
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f;
        }

        // Brief pause at full black
        yield return new WaitForSeconds(0.1f);

        // Load TowerScene
        SceneManager.LoadScene(towerSceneName);
        
        isTransitioning = false;
    }

    private void ResolveTreeButton()
    {
        if (treeButton != null)
        {
            Debug.Log($"TreeButton assigned in Inspector: {treeButton.name}");
            treeButton.onClick.AddListener(OnTreeButtonClicked);
            return;
        }

        // Try to find by name
        if (!string.IsNullOrEmpty(treeButtonName))
        {
            GameObject buttonObj = GameObject.Find(treeButtonName);
            if (buttonObj != null)
            {
                treeButton = buttonObj.GetComponent<Button>();
                Debug.Log($"TreeButton found by name: {treeButton.name}");
            }
        }

        if (treeButton == null)
        {
            Debug.LogError($"MapToTowerTransition could not find a Button named '{treeButtonName}'. Please assign it in the Inspector.", this);
            return;
        }

        treeButton.onClick.AddListener(OnTreeButtonClicked);
    }

    private void SetupFadeOverlay()
    {
        if (fadeOverlay != null) return;

        Transform parent = fadeParent != null ? fadeParent : transform;
        GameObject overlay = new GameObject("FadeOverlay_MapToTower");
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
