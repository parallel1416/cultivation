using System;
using UnityEngine;

/// <summary>
/// UI event extension for DialogueManager - provides callbacks for UI components
/// </summary>
public partial class DialogueManager : MonoBehaviour
{
    // Events for UI to subscribe to
    public event Action<string> OnTitleDisplay;
    public event Action<string, string> OnDialogueDisplay; // speaker, text
    public event Action<string, ChoiceOption[]> OnChoiceDisplay; // question, choices
    public event Action OnDialogueEnd;
    public event Action OnChoiceStart;
    public event Action OnChoiceEnd;
    
    // Events for visual and audio changes
    public event Action<string> OnBackgroundChange; // background resource path
    public event Action<string> OnPortraitChange; // portrait resource path
    public event Action<string> OnMusicChange; // music resource path

    /// <summary>
    /// Called by UI to advance dialogue when using UI-based input instead of keyboard
    /// </summary>
    public void AdvanceDialogue()
    {
        if (!isPlaying || isInChoiceMode || isOnCooldown)
        {
            return;
        }

        PlayNextSentence();
    }

    /// <summary>
    /// Called by UI when player selects a choice
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (!isInChoiceMode)
        {
            return;
        }

        DialogueSentence currentSentence = currentEvent.sentences[currentSentenceIndex];
        if (currentSentence.choices == null || choiceIndex < 0 || choiceIndex >= currentSentence.choices.Count)
        {
            LogController.LogError($"Invalid choice index: {choiceIndex}");
            return;
        }

        HandleChoiceSelection(choiceIndex);
    }

    // Replace OutputTitle to also trigger UI event
    private void OutputTitleUI(string title)
    {
        OnTitleDisplay?.Invoke(title);
    }

    // Trigger dialogue UI event
    private void OutputDialogueUI(string speaker, string text)
    {
        OnDialogueDisplay?.Invoke(speaker, text);
    }

    // Trigger choice UI event
    private void OutputChoiceUI(DialogueSentence sentence)
    {
        string question = sentence.question ?? "";
        ChoiceOption[] choices = sentence.choices?.ToArray() ?? new ChoiceOption[0];

        OnChoiceStart?.Invoke();
        OnChoiceDisplay?.Invoke(question, choices);
    }

    private void TriggerDialogueEnd()
    {
        OnDialogueEnd?.Invoke();
    }

    private void TriggerChoiceEnd()
    {
        OnChoiceEnd?.Invoke();
    }

    // Trigger visual/audio changes
    private void TriggerBackgroundChange(string backgroundPath)
    {
        OnBackgroundChange?.Invoke(backgroundPath);
    }

    private void TriggerPortraitChange(string portraitPath)
    {
        OnPortraitChange?.Invoke(portraitPath);
    }

    private void TriggerMusicChange(string musicPath)
    {
        OnMusicChange?.Invoke(musicPath);
    }
}
