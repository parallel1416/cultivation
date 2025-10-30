using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the Turn Scene display, showing turn number and resource changes.
/// Tracks resources at the start of the turn and compares them at the end to display changes.
/// </summary>
public class TurnSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnText; // Main text showing turn number and changes

    [Header("Settings")]
    [SerializeField] private float displayDelay = 0.5f; // Delay before showing turn info

    // Previous save data for comparison
    private SaveData previousSaveData;

    private void Start()
    {
        // Initialize text
        if (turnText == null)
        {
            Debug.LogError("TurnSceneManager: TurnText not assigned!");
            return;
        }

        StartCoroutine(InitializeAndDisplay());
    }

    private IEnumerator InitializeAndDisplay()
    {
        // Wait a frame to ensure all managers are initialized
        yield return null;

        // Get previous save data from SaveManager
        if (SaveManager.Instance != null)
        {
            previousSaveData = SaveManager.Instance.GetPreviousSaveData();
            
            if (previousSaveData == null)
            {
                Debug.Log("TurnSceneManager: No previous save data found (first turn)");
            }
            else
            {
                Debug.Log($"TurnSceneManager: Retrieved previous save - Turn: {previousSaveData.turn}, Money: {previousSaveData.money}");
            }
        }
        else
        {
            Debug.LogWarning("TurnSceneManager: SaveManager not found!");
        }

        // Wait for display delay
        yield return new WaitForSeconds(displayDelay);

        // Display turn information
        DisplayTurnInfo();
    }

    /// <summary>
    /// Display turn number and calculate resource changes
    /// </summary>
    private void DisplayTurnInfo()
    {
        if (TurnManager.Instance == null)
        {
            Debug.LogError("TurnSceneManager: TurnManager not found!");
            return;
        }

        if (LevelManager.Instance == null)
        {
            Debug.LogError("TurnSceneManager: LevelManager not found!");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Display turn number
        int currentTurn = TurnManager.Instance.CurrentTurn;
        sb.AppendLine($"<size=48><b>第 {currentTurn} 回合</b></size>");
        sb.AppendLine();

        // If no previous save data (first turn), just display current state
        if (previousSaveData == null)
        {
            sb.AppendLine("<color=#FFD700>初始状态</color>");
            sb.AppendLine();
            sb.AppendLine($"<b>灵石:</b> {LevelManager.Instance.Money}");
            sb.AppendLine($"<b>弟子:</b> {LevelManager.Instance.Disciples}");
            sb.AppendLine($"<b>可用弟子:</b> {LevelManager.Instance.ActiveDisciples}");

            // Display items
            if (ItemManager.Instance != null && ItemManager.Instance.Items.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>本回合获得物品:</b>");
                foreach (var item in ItemManager.Instance.Items)
                {
                    if (item.Value > 0)
                    {
                        string itemName = GetItemDisplayName(item.Key);
                        sb.AppendLine($"  • {itemName}: {item.Value}");
                    }
                }
            }
        }
        else
        {
            // Compare and display changes
            bool hasChanges = false;

            // Money changes
            int moneyChange = LevelManager.Instance.Money - previousSaveData.money;
            if (moneyChange != 0)
            {
                hasChanges = true;
                string changeText = FormatResourceChange("灵石", previousSaveData.money, LevelManager.Instance.Money, moneyChange);
                sb.AppendLine(changeText);
            }

            // Disciple changes
            int discipleChange = LevelManager.Instance.Disciples - previousSaveData.disciples;
            if (discipleChange != 0)
            {
                hasChanges = true;
                string changeText = FormatResourceChange("弟子", previousSaveData.disciples, LevelManager.Instance.Disciples, discipleChange);
                sb.AppendLine(changeText);
            }

            // Note: ActiveDisciples is not saved (it resets each turn), so we don't compare it

            // Special disciple status changes
            AppendSpecialDiscipleChanges(sb, ref hasChanges);

            // Pet status changes
            AppendPetChanges(sb, ref hasChanges);

            // Item changes
            AppendItemChanges(sb, ref hasChanges);

            if (!hasChanges)
            {
                sb.AppendLine("<color=#888888><i>本回合四平八稳，无资源变动。</i></color>");
            }
        }

        turnText.text = sb.ToString();
    }

    /// <summary>
    /// Append special disciple status changes to the text
    /// </summary>
    private void AppendSpecialDiscipleChanges(System.Text.StringBuilder sb, ref bool hasChanges)
    {
        if (LevelManager.Instance == null || previousSaveData == null) return;

        // Jingshi
        if (LevelManager.Instance.StatusJingshi != previousSaveData.statusJingshi)
        {
            hasChanges = true;
            sb.AppendLine(FormatStatusChange("经师", previousSaveData.statusJingshi, LevelManager.Instance.StatusJingshi));
        }

        // Jianjun
        if (LevelManager.Instance.StatusJianjun != previousSaveData.statusJianjun)
        {
            hasChanges = true;
            sb.AppendLine(FormatStatusChange("剑君", previousSaveData.statusJianjun, LevelManager.Instance.StatusJianjun));
        }

        // Yuezheng
        if (LevelManager.Instance.StatusYuezheng != previousSaveData.statusYuezheng)
        {
            hasChanges = true;
            sb.AppendLine(FormatStatusChange("乐正", previousSaveData.statusYuezheng, LevelManager.Instance.StatusYuezheng));
        }
    }

    /// <summary>
    /// Append pet status changes to the text
    /// </summary>
    private void AppendPetChanges(System.Text.StringBuilder sb, ref bool hasChanges)
    {
        if (LevelManager.Instance == null || previousSaveData == null) return;

        // Mouse
        if (LevelManager.Instance.StatusMouse != previousSaveData.statusMouse)
        {
            hasChanges = true;
            sb.AppendLine(FormatStatusChange("灵鼠", previousSaveData.statusMouse, LevelManager.Instance.StatusMouse));
        }

        // Chicken
        if (LevelManager.Instance.StatusChicken != previousSaveData.statusChicken)
        {
            hasChanges = true;
            sb.AppendLine(FormatStatusChange("凤雏", previousSaveData.statusChicken, LevelManager.Instance.StatusChicken));
        }

        // Sheep
        if (LevelManager.Instance.StatusSheep != previousSaveData.statusSheep)
        {
            hasChanges = true;
            sb.AppendLine(FormatStatusChange("獬豸", previousSaveData.statusSheep, LevelManager.Instance.StatusSheep));
        }
    }

    /// <summary>
    /// Append item quantity changes to the text
    /// </summary>
    private void AppendItemChanges(System.Text.StringBuilder sb, ref bool hasChanges)
    {
        if (ItemManager.Instance == null || previousSaveData == null) return;

        // Check all items in current ItemManager
        foreach (var item in ItemManager.Instance.Items)
        {
            int previousQuantity = previousSaveData.items.ContainsKey(item.Key) ? previousSaveData.items[item.Key] : 0;
            int currentQuantity = item.Value;

            if (currentQuantity != previousQuantity)
            {
                hasChanges = true;
                string itemName = GetItemDisplayName(item.Key);
                int change = currentQuantity - previousQuantity;
                string changeText = FormatResourceChange(itemName, previousQuantity, currentQuantity, change);
                sb.AppendLine(changeText);
            }
        }

        // Check for items that existed before but are now gone
        foreach (var previousItem in previousSaveData.items)
        {
            if (!ItemManager.Instance.Items.ContainsKey(previousItem.Key) && previousItem.Value > 0)
            {
                hasChanges = true;
                string itemName = GetItemDisplayName(previousItem.Key);
                string changeText = FormatResourceChange(itemName, previousItem.Value, 0, -previousItem.Value);
                sb.AppendLine(changeText);
            }
        }
    }

    /// <summary>
    /// Format resource change text with color coding
    /// </summary>
    private string FormatResourceChange(string resourceName, int oldValue, int newValue, int change)
    {
        string changeColor = change > 0 ? "#00FF00" : "#FF0000"; // Green for gain, red for loss
        string changeSign = change > 0 ? "+" : "";
        
        return $"<b>{resourceName}:</b> {oldValue} → {newValue} <color={changeColor}>({changeSign}{change})</color>";
    }

    /// <summary>
    /// Format status change text (for disciples/pets)
    /// </summary>
    private string FormatStatusChange(string entityName, int oldStatus, int newStatus)
    {
        string oldStatusText = GetStatusText(oldStatus);
        string newStatusText = GetStatusText(newStatus);

        if (oldStatus < 0 && newStatus >= 0)
        {
            // Newly acquired
            return $"<b>{entityName}:</b> <color=#00FF00>获得：{newStatusText}</color>";
        }
        else if (oldStatus >= 0 && newStatus < 0)
        {
            // Lost
            return $"<b>{entityName}:</b> <color=#FF0000>失去</color>";
        }
        else
        {
            // Status changed
            return $"<b>{entityName}:</b> {oldStatusText} → {newStatusText}";
        }
    }

    /// <summary>
    /// Get status text for special disciples/pets
    /// </summary>
    private string GetStatusText(int status)
    {
        switch (status)
        {
            case -1:
                return "未拥有";
            case 0:
                return "行动中";
            case 1:
                return "可用";
            default:
                return "未知";
        }
    }

    /// <summary>
    /// Get display name for item ID
    /// </summary>
    private string GetItemDisplayName(string itemId)
    {
        switch (itemId)
        {
            case "zhi_kui_lei":
                return "纸傀儡";
            case "yu_chan_tui":
                return "玉蝉蜕";
            case "dian_fan_tie":
                return "颠凡铁";
            case "wu_que_jing":
                return "无缺镜";
            case "cheng_fu_fu":
                return "承负符";
            default:
                return itemId;
        }
    }
}
