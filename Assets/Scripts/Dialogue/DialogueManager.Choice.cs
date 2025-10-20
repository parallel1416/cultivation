using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DialogueManager : MonoBehaviour
{

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