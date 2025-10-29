using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CheckCondition
{
    public int difficultyClass = 0;
    public string checkWhat = "";
    public string stringId = ""; // techID or tagID, special disciple dice id
}

/// <summary>
/// CheckResult class for generating and storing check results.
/// Used in both check and multicheck sentences.
/// </summary>
public class CheckResult
{
    public bool isSuccess;
    public string description;
    public DiceResult diceResult; // Store dice result to avoid re-rolling

    public CheckResult(bool isSuccess, string description)
    {
        this.isSuccess = isSuccess;
        this.description = description;
        this.diceResult = null;
    }

    public CheckResult()
    {
        this.isSuccess = false;
        this.description = "";
        this.diceResult = null;
    }
}

/// <summary>
/// Part of DialogueManager handling check sentences
/// </summary>
public partial class DialogueManager : MonoBehaviour
{
    [SerializeField] private string currentMoneyDesc = "当前灵石：";
    [SerializeField] private string TechDesc = "科技：";
    [SerializeField] private string UnlockedDesc = "已解锁";
    [SerializeField] private string lockedDesc = "未解锁";
    [SerializeField] private string reportBug = "请向开发者报告BUG！";

    [SerializeField] private string checkSuccessDesc = "检定成功！";
    [SerializeField] private string checkFailureDesc = "检定失败！";
    [SerializeField] private string diceCheckDesc = "掷骰";
    [SerializeField] private string moneyCheckDesc = "灵石";
    [SerializeField] private string techCheckDesc = "科技";
    [SerializeField] private string globalTagCheckDesc = "全局标签";
    [SerializeField]
    private IReadOnlyList<string> strCheckWhats = new List<string>()
    {
        "tech",
        "globalTag",
        "SpecialDice"
    };

    // Temporary storage for dice result to avoid re-rolling
    private DiceResult cachedDiceResult;

    private CheckResult HandleCheck(CheckCondition condition)
    {
        // Local methods to get assigning situation of current event, only used for diceroll checks
        Dictionary<string, int> GetAssignedDices()
        {
            if (currentEvent == null || EventTracker.Instance == null)
            {
                return new Dictionary<string, int>();
            }

            EventTeamData teamData = EventTracker.Instance.GetEventData(currentEvent.id);
            if (teamData == null || teamData.assignedMemberIds.Count == 0)
            {
                return new Dictionary<string, int>();
            }

            // Parse dice assignments from team data
            Dictionary<string, int> diceAssignment = new Dictionary<string, int>
            {
                { "Normal", 0 },
                { "Jingshi", 0 },
                { "Jianjun", 0 },
                { "Yuezheng", 0 }
            };

            foreach (string data in teamData.assignedMemberIds)
            {
                if (data.StartsWith("dice:"))
                {
                    string[] parts = data.Split(':');
                    if (parts.Length == 3)
                    {
                        string diceType = parts[1];
                        if (int.TryParse(parts[2], out int count))
                        {
                            diceAssignment[diceType] = count;
                        }
                    }
                }
            }

            return diceAssignment;
        }

        string GetAssignedAnimal()
        {
            if (currentEvent == null || EventTracker.Instance == null)
            {
                return "";
            }

            EventTeamData teamData = EventTracker.Instance.GetEventData(currentEvent.id);
            if (teamData == null || teamData.assignedMemberIds.Count == 0)
            {
                return "";
            }

            // Find pet assignment (format: "pet:0", "pet:1", "pet:2")
            foreach (string data in teamData.assignedMemberIds)
            {
                if (data.StartsWith("pet:"))
                {
                    string[] parts = data.Split(':');
                    if (parts.Length == 2)
                    {
                        return parts[1]; // Return pet index as string
                    }
                }
            }

            return "";
        }

        string GetAssignedItem()
        {
            if (currentEvent == null || EventTracker.Instance == null)
            {
                return "";
            }

            EventTeamData teamData = EventTracker.Instance.GetEventData(currentEvent.id);
            if (teamData == null || teamData.assignedMemberIds.Count == 0)
            {
                return "";
            }

            // Find item assignment (format: "item:0", "item:1", etc.)
            foreach (string data in teamData.assignedMemberIds)
            {
                if (data.StartsWith("item:"))
                {
                    string[] parts = data.Split(':');
                    if (parts.Length == 2)
                    {
                        return parts[1]; // Return item index as string
                    }
                }
            }

            return "";
        }

        string GetComparison(bool isTrue) => isTrue ? ">=" : "<";
        string GetSuccessFailureDesc(bool isTrue) => isTrue ? checkSuccessDesc : checkFailureDesc;
        string GetTechUnlockDesc(bool isTrue) => isTrue ? UnlockedDesc : lockedDesc;


        string checkWhat = condition.checkWhat;

        // for some checkWhat values(yes or no check), lock dc to 1
        int dc = strCheckWhats.Contains(condition.checkWhat) ? 1 : condition.difficultyClass;
        string stringId = condition.stringId;

        // check results
        bool isSuccess = false;
        string resultDesc = "";

        CheckResult checkResult = new CheckResult();

        switch (checkWhat)
        {
            case "diceroll":
            case "dice":
            case "diceRoll":
                DiceResult diceResult;
                
                // Use cached dice result if available, otherwise calculate
                if (cachedDiceResult != null)
                {
                    diceResult = cachedDiceResult;
                }
                else
                {
                    // Fallback: calculate if not cached (shouldn't happen in normal flow)
                    Dictionary<string, int> assignedDices = GetAssignedDices();
                    string assignedAnimal = GetAssignedAnimal();
                    string assignedItem = GetAssignedItem();
                    diceResult = DiceRollManager.Instance.GetDiceResult(assignedDices, assignedAnimal, assignedItem);
                }

                int totalResult = diceResult.result;
                isSuccess = totalResult >= dc;
                resultDesc = diceResult.checkDescription;

                checkResult.isSuccess = isSuccess;
                checkResult.description = $"{resultDesc} {GetComparison(isSuccess)} {dc}\n{diceCheckDesc}{GetSuccessFailureDesc(isSuccess)}";
                checkResult.diceResult = diceResult; // Store dice result
                break;


            case "money":
                int money = LevelManager.Instance.Money;
                isSuccess = money >= dc;
                resultDesc = $"{currentMoneyDesc}{money}";

                checkResult.isSuccess = isSuccess;
                checkResult.description = $"{resultDesc} {GetComparison(isSuccess)} {dc}\n{diceCheckDesc}{GetSuccessFailureDesc(isSuccess)}";
                break;

            case "tech": 
                isSuccess = TechManager.Instance.IsTechUnlocked(stringId);
                string techName = TechManager.Instance.GetTechName(stringId);

                checkResult.isSuccess = isSuccess;
                checkResult.description = $"{TechDesc} [{techName}] {GetTechUnlockDesc(isSuccess)}\n{techCheckDesc}{GetSuccessFailureDesc(isSuccess)}";
                break;

            case "globaltag":
            case "globalTag":
                isSuccess = GlobalTagManager.Instance.GetTagValue(stringId);
                string tagStatus = GlobalTagManager.Instance.GetTagDescription(stringId);

                checkResult.isSuccess = isSuccess;
                checkResult.description = $"{GetSuccessFailureDesc(isSuccess)} {tagStatus}";
                break;

            default:
                LogController.LogError($"Invalid checkWhat: {checkWhat}, check fails by default");
                break;
        }
        return checkResult;
    }

    private void PlayCheckSentence(DialogueSentence sentence)
    {
        CheckCondition condition = sentence.checkCondition;

        // Check if this is a dice roll check
        string checkWhat = condition.checkWhat;
        if (checkWhat == "diceroll" || checkWhat == "dice" || checkWhat == "diceRoll")
        {
            // Show dice panel before rolling
            ShowDicePanelBeforeCheck(sentence, condition);
        }
        else
        {
            // Non-dice checks proceed immediately
            ExecuteCheck(sentence, condition);
        }
    }

    private void ShowDicePanelBeforeCheck(DialogueSentence sentence, CheckCondition condition)
    {
        // Calculate dice result ONCE here and cache it
        Dictionary<string, int> assignedDices = GetAssignedDicesFromEvent();
        string assignedAnimal = GetAssignedAnimalFromEvent();
        string assignedItem = GetAssignedItemFromEvent();

        // Get the dice result and cache it
        cachedDiceResult = null;
        if (DiceRollManager.Instance != null)
        {
            cachedDiceResult = DiceRollManager.Instance.GetDiceResult(assignedDices, assignedAnimal, assignedItem);
        }

        // Get difficulty class
        int difficulty = condition.difficultyClass;

        // Find DialogueUI
        DialogueUI dialogueUI = FindObjectOfType<DialogueUI>();
        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueManager: DialogueUI not found, skipping dice panel");
            ExecuteCheck(sentence, condition);
            return;
        }

        // Show dice panel with the pre-calculated dice result and difficulty
        dialogueUI.ShowDicePanel(cachedDiceResult, difficulty, () =>
        {
            // User clicked continue, now execute the check (which will use cachedDiceResult)
            ExecuteCheck(sentence, condition);
            // Clear cache after use
            cachedDiceResult = null;
        });
    }

    private void ExecuteCheck(DialogueSentence sentence, CheckCondition condition)
    {
        CheckResult checkResult = HandleCheck(condition);
        bool isSuccess = checkResult.isSuccess;
        string resultDescription = checkResult.description;

        // set this value to false to create a hidden check
        if (sentence.showCheckResult) OutputDialogue(resultDescription);

        // decide jump target: success or failure
        string targetID = isSuccess ? sentence.successTarget : sentence.failureTarget;

        StartCooldown();

        HandleSentenceIndex(targetID);

        if (currentSentenceIndex < currentEvent.sentences.Count)
        {
            PlayCurrentSentence();
        }
        else
        {
            PlayNextEvent();
        }
    }

    // Helper methods to get team data from current event
    private Dictionary<string, int> GetAssignedDicesFromEvent()
    {
        if (currentEvent == null || EventTracker.Instance == null)
        {
            return new Dictionary<string, int>();
        }

        EventTeamData teamData = EventTracker.Instance.GetEventData(currentEvent.id);
        if (teamData == null || teamData.assignedMemberIds.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        Dictionary<string, int> diceAssignment = new Dictionary<string, int>
        {
            { "Normal", 0 },
            { "Jingshi", 0 },
            { "Jianjun", 0 },
            { "Yuezheng", 0 }
        };

        foreach (string data in teamData.assignedMemberIds)
        {
            if (data.StartsWith("dice:"))
            {
                string[] parts = data.Split(':');
                if (parts.Length == 3)
                {
                    string diceType = parts[1];
                    if (int.TryParse(parts[2], out int count))
                    {
                        diceAssignment[diceType] = count;
                    }
                }
            }
        }

        return diceAssignment;
    }

    private string GetAssignedAnimalFromEvent()
    {
        if (currentEvent == null || EventTracker.Instance == null)
        {
            return "";
        }

        EventTeamData teamData = EventTracker.Instance.GetEventData(currentEvent.id);
        if (teamData == null || teamData.assignedMemberIds.Count == 0)
        {
            return "";
        }

        foreach (string data in teamData.assignedMemberIds)
        {
            if (data.StartsWith("pet:"))
            {
                string[] parts = data.Split(':');
                if (parts.Length == 2)
                {
                    return parts[1];
                }
            }
        }

        return "";
    }

    private string GetAssignedItemFromEvent()
    {
        if (currentEvent == null || EventTracker.Instance == null)
        {
            return "";
        }

        EventTeamData teamData = EventTracker.Instance.GetEventData(currentEvent.id);
        if (teamData == null || teamData.assignedMemberIds.Count == 0)
        {
            return "";
        }

        foreach (string data in teamData.assignedMemberIds)
        {
            if (data.StartsWith("item:"))
            {
                string[] parts = data.Split(':');
                if (parts.Length == 2)
                {
                    return parts[1];
                }
            }
        }

        return "";
    }
}