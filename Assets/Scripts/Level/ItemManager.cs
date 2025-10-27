using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private static ItemManager _instance;
    public static ItemManager Instance => _instance;

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

    private Dictionary<string, int> items = new Dictionary<string, int>();
    public Dictionary<string, int> Items => items;

    private void RegisterItem(string itemId)
    {
        if (items.ContainsKey(itemId))
        {
            LogController.LogError($"Conflict itemID: {itemId}");
            return;
        }
        items[itemId] = 0;
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
    }

    public bool ConsumeItem(string itemId, int quantity)
    {
        if (items.ContainsKey(itemId))
        {
            if (IsItemEnough(itemId, quantity))
            {
                items[itemId] -= quantity;
                LogController.Log($"Consumed {quantity} of item {itemId}. Total now: {items[itemId]}");
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
            LogController.LogError($"No such item: {itemId}");
            return 0;
        }
    }

    public void ApplySaveData(SaveData saveData) => items = saveData.items;
}
