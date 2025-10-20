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
        int dc = sentence.checkCondition.checkWhat == "tech" ? 1 : GetDifficultyClass(sentence.checkCondition.difficultyClass);
        int checkResult = GetCheckResult(sentence.checkCondition.checkWhat);
        string resultDescription = GenerateCheckResultDescription(dc, checkResult, sentence.checkCondition.checkWhat);

        OutputDialogue(resultDescription);

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

    private int GetCheckResult(string checkWhat)
    {
        switch (checkWhat)
        {
            case "diceroll":
                return random.Next(1, 7);

            case "money":
                return LevelManager.Instance.Money;

            case "tech":
                // For tech check, return 1 if unlocked, 0 if not
                DialogueSentence currentSentence = currentEvent.sentences[currentSentenceIndex];
                bool isUnlocked = TechManager.Instance.IsTechUnlocked(currentSentence.checkCondition.stringId);
                return isUnlocked ? 1 : 0;

            case "globaltag":
                DialogueSentence sentence = currentEvent.sentences[currentSentenceIndex];
                bool tagStatus = GlobalTagManager.Instance.GetTagValue(sentence.checkCondition.stringId);
                return tagStatus ? 1 : 0;

            default:
                LogController.LogError($"Invalid checkWhat: {checkWhat}, result set to 0 by default");
                return 0;
        }
    }

    /// <summary>
    /// Generate dialogue output describing check result, instead of sentence text
    /// </summary>
    private string GenerateCheckResultDescription(int dc, int checkResult, string checkWhat)
    {
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
                string techName = TechManager.Instance.GetTechName(currentEvent.sentences[currentSentenceIndex].checkCondition.stringId);
                string techStatus = isSuccess ? "�ѽ���" : "δ����";
                checkDescription = $"�Ƽ� [{techName}] {techStatus}";
                break;

            case "globaltag":
                string tagId = currentEvent.sentences[currentSentenceIndex].checkCondition.stringId;
                string tagStatus = GlobalTagManager.Instance.GetTagDescription(tagId);
                checkDescription = $"{tagStatus}"; // example:"�������xxx��"
                break;

            default:
                checkDescription = $"��Ч���Ŀ�꣡���鵱ǰ�¼�JSON�ļ���{currentEvent.id}";
                break;
        }

        // Sample Style: [ �ɹ���] ( DC = 5, 1d6 = 6 >= 5 )
        return $"[ {successText}] ( {checkDescription} )";
    }

}