using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DialogueManager : MonoBehaviour
{
    private void PlayChoiceSentence(DialogueSentence sentence)
    {
        if (!string.IsNullOrEmpty(sentence.question))
        {
            OutputDialogue(sentence.question);
        }

        if (sentence.choices != null && sentence.choices.Count > 0)
        {
            // List all options in the dialogue output
            for (int i = 0; i < sentence.choices.Count; i++)
            {
                OutputDialogue($"{i + 1}. {sentence.choices[i].text}");
            }

            // Trigger UI display
            OutputChoiceUI(sentence);

            // temporary, might be deprecated in the future, might not
            isInChoiceMode = true;
            LogController.Log("press key (1-" + sentence.choices.Count + ") to select answer");
        }
        else
        {
            LogController.LogError("no choice within sentence!");
        }
    }

    private void HandleChoiceInput()
    {
        DialogueSentence currentSentence = currentEvent.sentences[currentSentenceIndex];

        if (currentSentence.choices == null) return;

        for (int i = 0; i < currentSentence.choices.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                int choiceIndex = i;
                if (choiceIndex < currentSentence.choices.Count)
                {
                    HandleChoiceSelection(choiceIndex);
                    break;
                }
            }
        }
    }

    private void HandleChoiceSelection(int choiceIndex)
    {
        DialogueSentence currentSentence = currentEvent.sentences[currentSentenceIndex];
        ChoiceOption selectedChoice = currentSentence.choices[choiceIndex];

        OutputDialogue($"player selected choice {choiceIndex + 1}: {selectedChoice.text}");

        // Restore sequenced playback input
        isInChoiceMode = false;
        TriggerChoiceEnd();

        HandleSentenceIndex(selectedChoice.target);

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