using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DialogueManager : MonoBehaviour
{
    /// <summary>
    /// temporarily handle input for dialogue playback debugging
    /// should be deleted or replaced by proper UI input handling later
    /// </summary>
    private void Update()
    {
        if (!isPlaying) return;

        if (isInChoiceMode)
        {
            HandleChoiceInput();
        }
        else if (!isOnCooldown && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            PlayNextSentence();
        }
    }

    /// <summary>
    /// Start dialogue playback from the queue, bound to next turn method ( TurnManager.NextTurn() )
    /// </summary>
    public void StartDialoguePlayback()
    {
        if (dialogueQueue.Count > 0 && !isPlaying)
        {
            isPlaying = true;
            currentEvent = dialogueQueue.Dequeue();
            if (!string.IsNullOrEmpty(currentEvent.title))
            {
                OutputTitle($"=== {currentEvent.title} ===");
            }
            BuildIdIndexMap();
            currentSentenceIndex = 0;
            PlayCurrentSentence();
        }
    }

    /// <summary>
    /// 4 types of sentence playback: default, choice, check, multicheck
    /// </summary>
    private void PlayCurrentSentence()
    {
        if (currentEvent == null || currentSentenceIndex >= currentEvent.sentences.Count)
        {
            PlayNextEvent();
            return;
        }

        DialogueSentence sentence = currentEvent.sentences[currentSentenceIndex];

        switch (sentence.type)
        {
            case "choice":
                PlayChoiceSentence(sentence);
                break;

            case "check":
                PlayCheckSentence(sentence);
                break;

            case "multicheck":
                PlayMultiCheckSentence(sentence);
                break;

            default:
                PlayDefaultSentence(sentence);
                break;
        }

        ExecuteSentenceEffects(sentence);
    }

    private void PlayDefaultSentence(DialogueSentence sentence)
    {
        string output = FormatDialogueOutput(sentence);
        OutputDialogue(output);

        StartCooldown();
    }   

    private void HandleSentenceIndex(string targetID)
    {
        if (string.IsNullOrEmpty(targetID))
        {
            currentSentenceIndex++; // no target, sequencedly play by default
            return;
        }
        if (targetID == "END") // if set to END(reserved word), neglect all and end current event
        {
            currentSentenceIndex = currentEvent.sentences.Count;
        }
        else if (idToIndexMap.ContainsKey(targetID))
        {
            int targetIndex = idToIndexMap[targetID];

            if (targetIndex >= 0 && targetIndex < currentEvent.sentences.Count)
            {
                currentSentenceIndex = targetIndex;
                LogController.Log($"Successfully jumped to: {targetID} (Index: {targetIndex})");
            }
            else
            {
                LogController.LogError($"targetIndex out of range: {targetID} -> {targetIndex}, play the next sentence by default");
                currentSentenceIndex++;
            }
        }
        else
        {
            LogController.LogError($"targetID not exist: {targetID}, play the next sentence by default");
            currentSentenceIndex++;
        }
    }

    /// <summary>
    /// Sequencedly play the next sentence, controlled by mouse click or space key
    /// </summary>
    private void PlayNextSentence()
    {
        string targetID = currentEvent.sentences[currentSentenceIndex].target;

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

    /// <summary>
    /// End current dialogue event and start the next one, from its first sentence
    /// </summary>
    private void PlayNextEvent()
    {
        idToIndexMap.Clear();

        if (dialogueQueue.Count > 0)
        {
            currentEvent = dialogueQueue.Dequeue();
            BuildIdIndexMap();
            currentSentenceIndex = 0;
            PlayCurrentSentence();
        }
        else
        {
            // All dialogue events in queue have completed, it's time for the next turn!
            EndDialoguePlayback();
        }
    }

    private void EndDialoguePlayback()
    {
        isPlaying = false;
        isInChoiceMode = false;
        currentEvent = null;
        currentSentenceIndex = 0;
        idToIndexMap.Clear();
        LogController.Log("All DialogueEvent playback completed");
    }

    private string FormatDialogueOutput(DialogueSentence sentence)
    {
        if (string.IsNullOrEmpty(sentence.speaker))
        {
            // Narration by default (no speaker, no quotation marks)
            // Sample Style: 你醒了，但不知道自己身在何处。
            return sentence.text;
        }
        else
        {
            // Character dialogue: “speaker: “text”” (Full-width quotation marks)
            // Sample Style: 主角：“何意味？”
            return $"{sentence.speaker}：“{sentence.text}”";
        }
    }

    private void OutputTitle(string title)
    {
        // temporarily use Debug.Log for output
        // should be replaced by proper UI display later
        Debug.Log(title);
    }
    private void OutputDialogue(string message)
    {
        // temporarily use Debug.Log for output
        // should be replaced by proper UI display later
        Debug.Log(message);
    }
}
