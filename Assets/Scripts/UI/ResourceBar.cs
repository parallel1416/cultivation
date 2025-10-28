using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Resource bar that displays money and disciple count.
/// Attach this to the ResourceBar GameObject in your scene.
/// </summary>
public class ResourceBar : MonoBehaviour
{
    private static ResourceBar _instance;
    public static ResourceBar Instance => _instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI discipleText;

    [Header("Dropdown Menu")]
    [SerializeField] private Button dropdownButton;
    [SerializeField] private GameObject dropdownMenu;
    [SerializeField] private Toggle pplToggle;
    [SerializeField] private Toggle petToggle;
    [SerializeField] private Toggle itemToggle;
    [SerializeField] private Toggle optionToggle;
    [SerializeField] private Transform contentContainer; // Vertical layout container
    [SerializeField] private GameObject descriptionItemPrefab; // Prefab for list items

    private enum DropdownCategory
    {
        People,
        Pet,
        Item,
        Option
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup dropdown menu
        if (dropdownMenu != null)
        {
            dropdownMenu.SetActive(false);
        }

        // Setup dropdown button
        if (dropdownButton != null)
        {
            dropdownButton.onClick.AddListener(ToggleDropdown);
        }

        // Setup toggle listeners
        if (pplToggle != null)
        {
            pplToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdateDropdownContent(DropdownCategory.People); });
        }

        if (petToggle != null)
        {
            petToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdateDropdownContent(DropdownCategory.Pet); });
        }

        if (itemToggle != null)
        {
            itemToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdateDropdownContent(DropdownCategory.Item); });
        }

        if (optionToggle != null)
        {
            optionToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdateDropdownContent(DropdownCategory.Option); });
        }
    }

    private void Start()
    {
        // Auto-find text components if not assigned
        if (moneyText == null)
        {
            moneyText = transform.Find("MoneyText")?.GetComponent<TextMeshProUGUI>();
        }

        if (discipleText == null)
        {
            discipleText = transform.Find("DiscipleText")?.GetComponent<TextMeshProUGUI>();
        }

        if (moneyText == null || discipleText == null)
        {
            Debug.LogError("ResourceBar: Failed to find MoneyText or DiscipleText components!");
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Update the resource display with current values from LevelManager
    /// </summary>
    public void UpdateDisplay()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("ResourceBar: LevelManager instance not found.");
            return;
        }

        if (moneyText != null)
        {
            moneyText.text = $"{LevelManager.Instance.Money}";
        }

        if (discipleText != null)
        {
            discipleText.text = LevelManager.Instance.Disciples.ToString();
        }
    }

    /// <summary>
    /// Refresh the dropdown content if it's currently open
    /// Call this when items/disciples/pets change
    /// </summary>
    public void RefreshDropdown()
    {
        if (dropdownMenu == null || !dropdownMenu.activeSelf)
        {
            return; // Dropdown not open, no need to refresh
        }

        // Determine which category is currently selected and refresh it
        if (pplToggle != null && pplToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.People);
        }
        else if (petToggle != null && petToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.Pet);
        }
        else if (itemToggle != null && itemToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.Item);
        }
        else if (optionToggle != null && optionToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.Option);
        }
    }

    /// <summary>
    /// Toggle dropdown menu visibility
    /// </summary>
    private void ToggleDropdown()
    {
        if (dropdownMenu == null) return;

        bool newState = !dropdownMenu.activeSelf;
        dropdownMenu.SetActive(newState);

        if (newState)
        {
            // Set default toggle to People and update content
            if (pplToggle != null && !pplToggle.isOn)
            {
                pplToggle.isOn = true;
            }
            else
            {
                // If already on, manually trigger update
                UpdateDropdownContent(DropdownCategory.People);
            }
        }
    }

    /// <summary>
    /// Update dropdown content based on selected category
    /// </summary>
    private void UpdateDropdownContent(DropdownCategory category)
    {
        if (contentContainer == null)
        {
            Debug.LogError("ResourceBar: Content container not assigned!");
            return;
        }

        // Clear existing content
        ClearContent();

        // Get data based on category
        List<string> descriptions = GetDescriptionsForCategory(category);

        // Populate content
        foreach (string description in descriptions)
        {
            CreateDescriptionItem(description);
        }

        Debug.Log($"ResourceBar: Updated dropdown content for {category} category with {descriptions.Count} items.");
    }

    /// <summary>
    /// Get descriptions for the selected category based on global status
    /// </summary>
    private List<string> GetDescriptionsForCategory(DropdownCategory category)
    {
        List<string> descriptions = new List<string>();

        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("ResourceBar: LevelManager not found.");
            return descriptions;
        }

        switch (category)
        {
            case DropdownCategory.People:
                // Get owned disciples
                int normalDisciples = LevelManager.Instance.ActiveDisciples;
                descriptions.Add($"Normal Disciples: {normalDisciples}");

                // Check special disciples
                if (LevelManager.Instance.StatusJingshi >= 0)
                {
                    string status = LevelManager.Instance.StatusJingshi == 1 ? "Available" : "In Action";
                    descriptions.Add($"Jingshi: {status}");
                }

                if (LevelManager.Instance.StatusJianjun >= 0)
                {
                    string status = LevelManager.Instance.StatusJianjun == 1 ? "Available" : "In Action";
                    descriptions.Add($"Jianjun: {status}");
                }

                if (LevelManager.Instance.StatusYuezheng >= 0)
                {
                    string status = LevelManager.Instance.StatusYuezheng == 1 ? "Available" : "In Action";
                    descriptions.Add($"Yuezheng: {status}");
                }
                break;

            case DropdownCategory.Pet:
                // Get owned pets (Mouse, Chicken, Sheep)
                if (LevelManager.Instance.StatusMouse >= 0)
                {
                    string status = LevelManager.Instance.StatusMouse == 1 ? "Available" : "In Action";
                    descriptions.Add($"Mouse: {status}");
                }

                if (LevelManager.Instance.StatusChicken >= 0)
                {
                    string status = LevelManager.Instance.StatusChicken == 1 ? "Available" : "In Action";
                    descriptions.Add($"Chicken: {status}");
                }

                if (LevelManager.Instance.StatusSheep >= 0)
                {
                    string status = LevelManager.Instance.StatusSheep == 1 ? "Available" : "In Action";
                    descriptions.Add($"Sheep: {status}");
                }

                if (descriptions.Count == 0)
                {
                    descriptions.Add("No pets owned");
                }
                break;

            case DropdownCategory.Item:
                // Get owned items from ItemManager
                if (ItemManager.Instance == null)
                {
                    descriptions.Add("ItemManager not found");
                }
                else
                {
                    var items = ItemManager.Instance.Items;
                    bool hasAnyItems = false;

                    foreach (var item in items)
                    {
                        if (item.Value > 0)
                        {
                            hasAnyItems = true;
                            string itemName = GetItemDisplayName(item.Key);
                            descriptions.Add($"{itemName}: {item.Value}");
                        }
                    }

                    if (!hasAnyItems)
                    {
                        descriptions.Add("No items in inventory");
                    }
                }
                break;

            case DropdownCategory.Option:
                // Display game options/settings
                descriptions.Add("Options coming soon");
                descriptions.Add($"Current Turn: {TurnManager.Instance?.CurrentTurn ?? 0}");
                descriptions.Add($"Money: {LevelManager.Instance.Money}");
                descriptions.Add($"Disciples: {LevelManager.Instance.Disciples}");
                break;
        }

        return descriptions;
    }

    /// <summary>
    /// Create a description item in the content container
    /// </summary>
    private void CreateDescriptionItem(string description)
    {
        GameObject item;

        if (descriptionItemPrefab != null)
        {
            // Use prefab if provided
            item = Instantiate(descriptionItemPrefab, contentContainer);
        }
        else
        {
            // Create simple text item if no prefab
            item = new GameObject("DescriptionItem");
            item.transform.SetParent(contentContainer, false);
            
            TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
        }

        // Set text content
        TextMeshProUGUI textComponent = item.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = description;
        }
        else
        {
            // Try to find text in children
            textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = description;
            }
        }
    }

    /// <summary>
    /// Clear all content items from the container
    /// </summary>
    private void ClearContent()
    {
        if (contentContainer == null) return;

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Convert item ID to display name
    /// </summary>
    private string GetItemDisplayName(string itemId)
    {
        switch (itemId)
        {
            case "zhi_kui_lei":
                return "Paper Puppet (纸傀儡)";
            case "yu_chan_tui":
                return "Jade Cicada Shell (玉蝉蜕)";
            case "dian_fan_tie":
                return "Dianfan Iron (颠凡铁)";
            case "wu_que_jing":
                return "Perfect Mirror (无缺镜)";
            case "cheng_fu_fu":
                return "Burden Talisman (承负符)";
            default:
                return itemId; // Fallback to ID if name not found
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (dropdownButton != null)
        {
            dropdownButton.onClick.RemoveListener(ToggleDropdown);
        }
    }
}