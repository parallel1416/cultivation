using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Part of DialogueManager handling check sentences
/// </summary>
public partial class DialogueManager : MonoBehaviour
{
    private void PlayCheckSentence(DialogueSentence sentence)
    {
        CheckCondition condition = sentence.checkCondition;
        int dc = condition.checkWhat == "tech" ? 1 : GetDifficultyClass(condition.difficultyClass);
        int checkResult = GetCheckResult(condition.checkWhat, condition.stringId);
        string resultDescription = GenerateCheckResultDescription(condition, dc, checkResult);

        // set this value to false to create a hidden check
        if (sentence.showCheckResult) OutputDialogue(resultDescription);

        // decide jump target: success or failure
        bool isSuccess = checkResult >= dc;
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

    private int GetDifficultyClass(string dcString)
    {
        if (int.TryParse(dcString, out int result))
        {
            return result;
        }
        else
        {
            LogController.LogError($"Wrong DC Syntax: {dcString}, set to 0 by default");
            return 0;
        }
    }

    private int GetCheckResult(string checkWhat, string stringId)
    {
        switch (checkWhat)
        {
            case "diceroll":
                return random.Next(1, 7);

            case "money":
                return LevelManager.Instance.Money;

            case "tech":
                // For tech check, return 1 if unlocked, 0 if not
                bool isUnlocked = TechManager.Instance.IsTechUnlocked(stringId);
                return isUnlocked ? 1 : 0;

            case "globaltag":
                bool tagStatus = GlobalTagManager.Instance.GetTagValue(stringId);
                return tagStatus ? 1 : 0;

            default:
                LogController.LogError($"Invalid checkWhat: {checkWhat}, result set to 0 by default");
                return 0;
        }
    }

    /// <summary>
    /// Generate dialogue output describing check result, instead of sentence text
    /// </summary>
    private string GenerateCheckResultDescription(CheckCondition condition, int dc, int checkResult)
    {
        string checkWhat = condition.checkWhat;
        bool isSuccess = checkResult >= dc;
        string successText = isSuccess ? "成功！" : "失败！"; // Full-width exclamation mark for better compatibility with Chinese fonts
        string comparison = isSuccess ? ">=" : "<";

        string checkDescription = "";
        switch (checkWhat)
        {
            case "diceroll":
                checkDescription = $"DC = {dc}, 1d6 = {checkResult}";
                break;

            case "money":
                checkDescription = $"至少需要{dc}个灵石，当前灵石数量 = {checkResult} {comparison} {dc}";
                break;

            case "tech":
                string techName = TechManager.Instance.GetTechName(condition.stringId);
                string techStatus = isSuccess ? "已解锁" : "未解锁";
                checkDescription = $"科技 [{techName}] {techStatus}";
                break;

            case "globaltag":
                string tagId = condition.stringId;
                string tagStatus = GlobalTagManager.Instance.GetTagDescription(tagId);
                checkDescription = $"{tagStatus}"; // example:"你帮助了xxx。"
                break;

            default:
                checkDescription = $"无效检测目标({checkWhat})！请检查当前事件JSON文件：{currentEvent.id}";
                break;
        }

        // Sample Style: [ 成功！] ( DC = 5, 1d6 = 6 >= 5 )
        return $"[ {successText}] ( {checkDescription} )";
    }

}