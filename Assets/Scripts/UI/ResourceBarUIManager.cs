using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceBarUIManager : MonoBehaviour
{
    private static ResourceBarUIManager _instance;
    public static ResourceBarUIManager Instance => _instance;

    [Header("UI References")]
    [SerializeField] private GameObject resourceBarPanel;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI discipleText;
    [Header("Auto Binding")]
    [SerializeField] private string autoFindPanelName = "ResourceBar";

    private bool isInitialized = false;
    private bool hasAttemptedAutoBind;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (resourceBarPanel != null)
        {
            resourceBarPanel.SetActive(false);
        }
    }

    private void Start()
    {
        TryAutoBindExistingPanel();
    }

    /// <summary>
    /// bind resource bar in current sceme
    /// </summary>
    public void BindResourceBar(GameObject resourceBarInstance)
    {
        if (resourceBarInstance == null)
        {
            Debug.LogError("Resource bar instance not exist!");
            return;
        }

        resourceBarPanel = resourceBarInstance;

        // bind components (direct lookup first)
        moneyText = resourceBarInstance.transform.Find("MoneyText")?.GetComponent<TextMeshProUGUI>();
        discipleText = resourceBarInstance.transform.Find("DiscipleText")?.GetComponent<TextMeshProUGUI>();

        string[] moneyTextCandidates = { "MoneyText", "Money" };
        string[] discipleTextCandidates = { "DiscipleText", "Disciple" };

        if (moneyText == null)
        {
            moneyText = FindChildComponentByCandidates<TextMeshProUGUI>(resourceBarInstance.transform, moneyTextCandidates);
        }

        if (discipleText == null)
        {
            discipleText = FindChildComponentByCandidates<TextMeshProUGUI>(resourceBarInstance.transform, discipleTextCandidates);
        }

        // temporarily allows image sprite to be null
        if (moneyText == null || discipleText == null)
        {
            Debug.LogError("Failed to find required UI components in resource bar!");
            return;
        }

        isInitialized = true;
        resourceBarPanel.SetActive(true);
        UpdateResourceDisplay();
    }

    /// <summary>
    /// Update display when resource value changes
    /// should have been changed into Event Bus-related logic, but eh, forget about it.
    /// </summary>
    public void UpdateResourceDisplay()
    {
        TryAutoBindExistingPanel();

        if (!isInitialized || resourceBarPanel == null)
        {
            return;
        }

        if (LevelManager.Instance == null)
        {
            Debug.LogError("[ResourceBarUIManager] LevelManager instance not found.");
            return;
        }

        moneyText.text = $"Money Left: {LevelManager.Instance.Money}";
        discipleText.text = LevelManager.Instance.Disciples.ToString();
    }

    /// <summary>
    /// Hide resource bar when return to main menu
    /// </summary>
    public void HideResourceBar()
    {
        if (resourceBarPanel != null)
            resourceBarPanel.SetActive(false);

        isInitialized = false;
    }

    public bool IsResourceBarActive()
    {
        return isInitialized && resourceBarPanel != null && resourceBarPanel.activeInHierarchy;
    }

    private void TryAutoBindExistingPanel()
    {
        if (isInitialized)
        {
            return;
        }

        if (resourceBarPanel == null && !hasAttemptedAutoBind)
        {
            GameObject found = !string.IsNullOrWhiteSpace(autoFindPanelName) ? GameObject.Find(autoFindPanelName) : null;
            resourceBarPanel = found;

            hasAttemptedAutoBind = true;
        }

        if (resourceBarPanel != null && !isInitialized)
        {
            BindResourceBar(resourceBarPanel);
        }
    }

    private static T FindChildComponentByCandidates<T>(Transform root, string[] candidateNames) where T : Component
    {
        if (root == null || candidateNames == null || candidateNames.Length == 0)
        {
            return null;
        }

        var components = root.GetComponentsInChildren<T>(true);
        foreach (var component in components)
        {
            foreach (string candidate in candidateNames)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (component.name.Equals(candidate, System.StringComparison.OrdinalIgnoreCase))
                {
                    return component;
                }
            }
        }

        // fallback partial match
        foreach (var component in components)
        {
            foreach (string candidate in candidateNames)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (component.name.IndexOf(candidate, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return component;
                }
            }
        }

        return null;
    }
}