using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class DialogueManager: MonoBehaviour
{
    private void PlayMultiCheckSentence(DialogueSentence sentence)
    {
        if (sentence.multiCheckConditions == null || sentence.multiCheckConditions.Count == 0)
        {
            LogController.LogError("Multi-check sentence has no conditions!");
            PlayNextSentence();
            return;
        }

        // check all conditions
        List<bool> conditionResults = new List<bool>();
        string resultDescription = "所有检定结果:\n";

        for (int i = 0; i < sentence.multiCheckConditions.Count; i++)
        {
            var condition = sentence.multiCheckConditions[i];
            int dc = condition.checkWhat == "tech" ? 1 : GetDifficultyClass(condition.difficultyClass);
            int checkResult = GetCheckResult(condition.checkWhat, condition.stringId);
            bool isSuccess = checkResult >= dc;
            conditionResults.Add(isSuccess);

            resultDescription += $" - 条件{i + 1}: {GenerateCheckResultDescription(condition, dc, checkResult)}\n";
        }

        // set this value to false to create a hidden check
        if (sentence.showCheckResult) OutputDialogue(resultDescription);

        // sort target by priority, high priority target will be checked first
        var sortedTargets = sentence.multiCheckTargets.OrderByDescending(t => t.priority).ToList();
        string targetID = null;

        foreach (var target in sortedTargets)
        {
            bool allRequiredMet = true;

            foreach (int conditionIndex in target.requiredConditionIndices)
            {
                if (conditionIndex < 0 || conditionIndex >= conditionResults.Count || !conditionResults[conditionIndex])
                {
                    allRequiredMet = false;
                    break;
                }
            }

            if (allRequiredMet)
            {
                targetID = target.targetID;
                LogController.Log($"Multi-check matched target: {target.description} (Priority: {target.priority})");
                break;
            }
        }

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
