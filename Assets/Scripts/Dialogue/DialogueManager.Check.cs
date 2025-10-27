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

    public CheckResult(bool isSuccess, string description)
    {
        this.isSuccess = isSuccess;
        this.description = description;
    }

    public CheckResult()
    {
        this.isSuccess = false;
        this.description = "";
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

    private CheckResult HandleCheck(CheckCondition condition)
    {
        // UNFINISHED!!!!!!!!!!
        // local methods to get assigning situation of current event, only used for diceroll checks
        Dictionary<string, int> GetAssignedDices()
        {
            return new Dictionary<string, int>();
        }
        string GetAssignedAnimal()
        {
            return "";
        }
        string GetAssignedItem()
        {
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
                Dictionary<string, int> assignedDices = GetAssignedDices();
                string assignedAnimal = GetAssignedAnimal();
                string assignedItem = GetAssignedItem();

                // Throw the dice!
                DiceResult diceResult = DiceRollManager.Instance.GetDiceResult(assignedDices, assignedAnimal, assignedItem);

                int totalResult = diceResult.result;
                isSuccess = totalResult >= dc;
                resultDesc = diceResult.checkDescription;

                checkResult.isSuccess = isSuccess;
                checkResult.description = $"{resultDesc} {GetComparison(isSuccess)} {dc}\n{diceCheckDesc}{GetSuccessFailureDesc(isSuccess)}";
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
}