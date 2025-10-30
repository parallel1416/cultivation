using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DialogueManager : MonoBehaviour
{
    /// <summary>
    /// temporarily handle input for dialogue playback debugging
    /// should be deleted or replaced by proper UI input handling later
    /// </summary>
    //private void Update()
    //{
    //    if (!isPlaying) return;

    //    if (isInChoiceMode)
    //    {
    //        HandleChoiceInput();
    //    }
    //    else if (!isOnCooldown && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
    //    {
    //        PlayNextSentence();
    //    }
    //}

    /// <summary>
    /// Start dialogue playback from the queue, bound to next turn method ( TurnManager.NextTurn() )
    /// </summary>
    public void StartDialoguePlayback()
    {
        LogController.Log($"StartDialoguePlayback() called. Queue count: {dialogueQueue.Count}, isPlaying: {isPlaying}");
        
        if (dialogueQueue.Count > 0 && !isPlaying)
        {
            // Check if we're already in DialogScene
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            LogController.Log($"Current scene: {currentScene}");
            
            if (currentScene == "DialogScene")
            {
                // Already in DialogScene, start playing immediately
                isPlaying = true;
                currentEvent = dialogueQueue.Dequeue();
                LogController.Log($"Playing dialogue event: {currentEvent.id}");
                
                // Apply visual and audio settings from event
                ApplyEventSettings(currentEvent);
                
                if (!string.IsNullOrEmpty(currentEvent.title))
                {
                    string titleText = $"=== {currentEvent.title} ===";
                    OutputTitle(titleText);
                    OutputTitleUI(titleText);
                }
                BuildIdIndexMap();
                currentSentenceIndex = 0;
                PlayCurrentSentence();
            }
            else
            {
                // Not in DialogScene, load it first (dialogue will auto-start via DialogueUI)
                LogController.Log("Loading DialogScene to play queued dialogues");
                UnityEngine.SceneManagement.SceneManager.LoadScene("DialogScene");
            }
        }
        else if (dialogueQueue.Count == 0)
        {
            LogController.Log("StartDialoguePlayback called but queue is empty");
        }
        else if (isPlaying)
        {
            LogController.Log("StartDialoguePlayback called but dialogue is already playing");
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
        // Apply per-sentence visual changes if specified
        if (!string.IsNullOrEmpty(sentence.background))
        {
            TriggerBackgroundChange(sentence.background);
        }

        if (!string.IsNullOrEmpty(sentence.portrait))
        {
            TriggerPortraitChange(sentence.portrait);
        }

        string output = FormatDialogueOutput(sentence);
        OutputDialogue(output);
        string speaker = sentence?.speaker ?? string.Empty;
        string text = sentence?.text ?? string.Empty;
        OutputDialogueUI(speaker, text);

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
            
            // Apply visual and audio settings for new event
            ApplyEventSettings(currentEvent);
            
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
        TriggerDialogueEnd();
    }

    private string FormatDialogueOutput(DialogueSentence sentence)
    {
        if (string.IsNullOrEmpty(sentence.speaker))
        {
            return sentence.text;
        }
        else
        {
            return $"{sentence.speaker}����{sentence.text}��";
        }
    }

    private void OutputTitle(string title)
    {
        // temporarily use Debug.Log for output
        // should be replaced by proper UI display later
        // Debug.Log(title);
    }
    private void OutputDialogue(string message)
    {
        // temporarily use Debug.Log for output
        // should be replaced by proper UI display later
        // Debug.Log(message);
    }

    /// <summary>
    /// Apply visual and audio settings from dialogue event (event-level only)
    /// </summary>
    private void ApplyEventSettings(DialogueEvent dialogueEvent)
    {
        if (dialogueEvent == null)
        {
            return;
        }

        // Clear portrait at the start of each event (will be set by sentences as needed)
        TriggerPortraitChange("");

        // Trigger background change (event-level)
        if (!string.IsNullOrEmpty(dialogueEvent.background))
        {
            TriggerBackgroundChange(dialogueEvent.background);
        }

        // Trigger music change (event-level)
        if (!string.IsNullOrEmpty(dialogueEvent.music))
        {
            TriggerMusicChange(dialogueEvent.music);
        }
    }
}
