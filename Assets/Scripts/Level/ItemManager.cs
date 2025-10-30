using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private static ItemManager _instance;
    public static ItemManager Instance => _instance;

    [SerializeField] private static IReadOnlyList<string> itemRegisterList = new List<string>
    {
        "zhi_kui_lei",
        "yu_chan_tui",
        "dian_fan_tie",
        "wu_que_jing",
        "cheng_fu_fu",
        "jian_pu_can_zhang",
        "fei_guang_jian_fu"
    };

    [SerializeField] private int paperPuppetDiceSize = 4;

    private Dictionary<string, int> items = new Dictionary<string, int>();

    public int PaperPuppetDiceSize => paperPuppetDiceSize;
    public Dictionary<string, int> Items => items;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void RegisterItem(string itemId)
    {
        if (items.ContainsKey(itemId))
        {
            LogController.LogError($"Conflict itemID: {itemId}");
            return;
        }
        items[itemId] = 0;
    }

    /// <summary>
    /// Generate an all 0 but complete item dictionary 
    /// </summary>
    public void RegisterAllItems()
    {
        items.Clear();
        foreach (string id in itemRegisterList)
        {
            // no conflict prevention, make sure itemRegisterList has no duplicate IDs
            RegisterItem(id);
        }
    }

    public void AddItem(string itemId, int quantity)
    {
        if (items.ContainsKey(itemId))
        {
            items[itemId] += quantity;
        }
        else
        {
            LogController.LogError($"No such item: {itemId}");
        }
        LogController.Log($"Added {quantity} of item {itemId}. Total now: {items[itemId]}");
        
        // Update ResourceBar display if it exists
        if (ResourceBar.Instance != null)
        {
            ResourceBar.Instance.RefreshDropdown();
        }
    }

    public bool ConsumeItem(string itemId, int quantity)
    {
        if (items.ContainsKey(itemId))
        {
            if (IsItemEnough(itemId, quantity))
            {
                items[itemId] -= quantity;
                LogController.Log($"Consumed {quantity} of item {itemId}. Total now: {items[itemId]}");
                
                // Update ResourceBar display if it exists
                if (ResourceBar.Instance != null)
                {
                    ResourceBar.Instance.RefreshDropdown();
                }
                
                return true;
            }
            else
            {
                LogController.LogError($"Not enough quantity of item {itemId} to consume. Requested: {quantity}, Available: {items[itemId]}");
                return false;
            }
        }
        else
        {
            LogController.LogError($"No such item: {itemId}");
            return false;
        }
    }

    /// <summary>
    /// Public method to check if item quantity is enough
    /// </summary>
    public bool IsItemEnough(string itemId, int quantity)
    {
        if (items.ContainsKey(itemId))
        {
            return items[itemId] >= quantity;
        }
        else
        {
            LogController.LogError($"No such item: {itemId}");
            return false;
        }
    }

    /// <summary>
    /// Retrieves the quantity of a specific item by its ID.
    /// Logs an error if the item does not exist and returns 0.
    /// </summary>
    public int GetItemQuantity(string itemId)
    {
        if (items.ContainsKey(itemId))
        {
            return items[itemId];
        }
        else
        {
            // Don't log error for uninitialized items - they're just not owned yet
            // Only log if the itemId is not even in the register list
            if (!itemRegisterList.Contains(itemId))
            {
                LogController.LogError($"No such item: {itemId}");
            }
            return 0;
        }
    }

    public void ApplySaveData(SaveData saveData) => items = saveData.items;
}
