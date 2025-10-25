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
        // for these checkWhat values(yes or no check), lock dc to 1
        int dc = condition.checkWhat switch { string str when str == "tech" || str == "globalTag" || str == "specialDice" => 1, _ => condition.difficultyClass};
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

    private int GetCheckResult(string checkWhat, string stringId)
    {
        switch (checkWhat)
        {
            case "diceroll":
                return DiceRollManager.Instance.GetDiceResult().result;

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
        string successText = isSuccess ? "�ɹ���" : "ʧ�ܣ�"; // Full-width exclamation mark for better compatibility with Chinese fonts
        string comparison = isSuccess ? ">=" : "<";

        string checkDescription = "";
        switch (checkWhat)
        {
            case "diceroll":
                checkDescription = $"DC = {dc}, 1d6 = {checkResult}";
                break;

            case "money":
                checkDescription = $"������Ҫ{dc}����ʯ����ǰ��ʯ���� = {checkResult} {comparison} {dc}";
                break;

            case "tech":
                string techName = TechManager.Instance.GetTechName(condition.stringId);
                string techStatus = isSuccess ? "�ѽ���" : "δ����";
                checkDescription = $"�Ƽ� [{techName}] {techStatus}";
                break;

            case "globaltag":
                string tagId = condition.stringId;
                string tagStatus = GlobalTagManager.Instance.GetTagDescription(tagId);
                checkDescription = $"{tagStatus}"; // example:"�������xxx��"
                break;

            default:
                checkDescription = $"��Ч���Ŀ��({checkWhat})�����鵱ǰ�¼�JSON�ļ���{currentEvent.id}";
                break;
        }

        // Sample Style: [ �ɹ���] ( DC = 5, 1d6 = 6 >= 5 )
        return $"[ {successText}] ( {checkDescription} )";
    }

}