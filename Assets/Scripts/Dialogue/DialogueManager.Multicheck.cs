using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MultiCheckTarget
{
    public int priority = 0; // high priority targets are checked first
    public List<int> requiredConditionIndices = new List<int>(); // start from 1 instead of 0
    public string targetID = "";
    public string description = "";
}

public partial class DialogueManager: MonoBehaviour
{
    [SerializeField] private string multiCheckResultDesc = "所有检定结果:";
    [SerializeField] private string conditionDesc = "条件";
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
        string resultDescription = $"{multiCheckResultDesc}\n\n";

        for (int i = 0; i < sentence.multiCheckConditions.Count; i++)
        {
            CheckCondition condition = sentence.multiCheckConditions[i];
            CheckResult checkResult = HandleCheck(condition);
            conditionResults.Add(checkResult.isSuccess);

            resultDescription += $" - {conditionDesc}{i + 1}:\n{checkResult.description}\n\n";
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
                // make conditionIndex start from 1 for easier use
                int factualIndex = conditionIndex - 1;

                // skip invalid indices
                if (factualIndex < 0 || factualIndex >= conditionResults.Count)
                {
                    LogController.LogWarning($"ConditionIndex out of range! Please check json file {currentEvent.id}, sentence {sentence.id ?? "noID"}\nCheck will continue.");
                    break;
                }

                // only "fuse" when a valid index is false
                if (!conditionResults[factualIndex])
                {
                    allRequiredMet = false; // "fuse"
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
