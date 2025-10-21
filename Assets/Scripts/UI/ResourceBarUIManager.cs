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
    [SerializeField] private Image moneyIcon;
    [SerializeField] private Image discipleIcon;

    private bool isInitialized = false;

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
            resourceBarPanel.SetActive(false);
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

        // bind components
        moneyText = resourceBarInstance.transform.Find("MoneyText")?.GetComponent<TextMeshProUGUI>();
        discipleText = resourceBarInstance.transform.Find("DiscipleText")?.GetComponent<TextMeshProUGUI>();
        moneyIcon = resourceBarInstance.transform.Find("MoneyIcon")?.GetComponent<Image>();
        discipleIcon = resourceBarInstance.transform.Find("DiscipleIcon")?.GetComponent<Image>();

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
        if (!isInitialized || resourceBarPanel == null) return;

        moneyText.text = LevelManager.Instance.Money.ToString();
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
}