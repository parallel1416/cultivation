using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[System.Serializable]
public partial class DialogueEvent
{
    public string id = ""; // automatically set to the file name when loading
    public string desc = ""; // show simple description in event select interface
    public string title = ""; // display before all sentences, maybe need bigger font size?
    public int diceLimit = 0; // max dice(disciple) number you can assign to this event, 0 by default (like normal dialogue event), which means you can not assign any dice(disciple) to it
    public bool triggersImmediately = true; // immediately triggered events will show event UI at once, and not be added to event queue, which plays after player clicks the next turn button.
    public bool major = false; // major event will lock the next turn button and force player to handle this event 
    public List<DialogueSentence> sentences = new List<DialogueSentence>();
}

[System.Serializable]
public class DialogueSentence
{
    public string id = "";
    public string type = "";
    public string speaker = "";
    public string text = "";
    public string target = "";

    // choice
    public string question = "";
    public List<ChoiceOption> choices = new List<ChoiceOption>();

    // check
    public bool showCheckResult = true; // set this value to false to create a hidden check
    public CheckCondition checkCondition = new CheckCondition();
    public string successTarget = "";
    public string failureTarget = "";

    // multicheck
    public List<CheckCondition> multiCheckConditions = new List<CheckCondition>();
    public List<MultiCheckTarget> multiCheckTargets = new List<MultiCheckTarget>();

    // effect
    public List<DialogueEffect> effects = new List<DialogueEffect>();
}

[System.Serializable]
public class ChoiceOption
{
    public string text = "";
    public string target = "";
}

[System.Serializable]
public class CheckCondition
{
    public int difficultyClass = 0;
    public string checkWhat = "";
    public string stringId = ""; // techID or tagID, special disciple dice id
}

[System.Serializable]
public class DialogueEffect
{
    public bool showOutput = true;
    public string type = ""; // "money", "disciple", "globalTag", "item"
    public int intValue = 0; // int value for money, item and disciple, tagID for globalTag
    public string stringValue = ""; // tagID for globalTag and itemID for item
    public string operation = ""; // "+"(also works for activate a tag), "-"(also works for deactivate a tag)
}

[System.Serializable]
public class MultiCheckTarget
{
    public int priority = 0; // high priority targets are checked first
    public List<int> requiredConditionIndices = new List<int>(); // start from 1 instead of 0
    public string targetID = "";
    public string description = "";
}


/// <summary>
/// Core part of DialogueManager class
/// </summary>
public partial class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    public static DialogueManager Instance => _instance;

    private Queue<DialogueEvent> dialogueQueue = new Queue<DialogueEvent>();
    private DialogueEvent currentEvent;
    private int currentSentenceIndex;
    private bool isPlaying = false;
    private bool isOnCooldown = false;
    private bool isInChoiceMode = false;

    private float cooldownTime = 0.5f;

    private Dictionary<string, int> idToIndexMap = new Dictionary<string, int>();

    private System.Random random = new System.Random();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Add event to queue, which will be sequently played later
    /// bound to every event in event map
    /// </summary>
    public void EnqueueDialogueEvent(string eventId)
    {
        DialogueEvent dialogueEvent = LoadDialogueEvent(eventId);
        if (dialogueEvent != null)
        {
            dialogueQueue.Enqueue(dialogueEvent);
            LogController.Log($"DialogueEvent in queue: {eventId}");
        }
    }

    /// <summary>
    /// Returns the dialogue definition for the given event without enqueuing it.
    /// </summary>
    public DialogueEvent GetDialogueDefinition(string eventId)
    {
        return LoadDialogueEvent(eventId);
    }

    /// <summary>
    /// Load DialogueEvent from json file
    /// </summary>
    private DialogueEvent LoadDialogueEvent(string eventId)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Dialogues/{eventId}");
        if (jsonFile != null)
        {
            DialogueEvent dialogueEvent = JsonUtility.FromJson<DialogueEvent>(jsonFile.text);
            dialogueEvent.id = eventId;
            return dialogueEvent;
        }
        else
        {
            LogController.LogError($"DialogueEvent file not found: {eventId}");
            return null;
        }
    }

    /// <summary>
    /// Build a dictionary mapping sentence IDs(string) to their indices for quick access
    /// </summary>
    private void BuildIdIndexMap()
    {
        idToIndexMap.Clear();

        for (int i = 0; i < currentEvent.sentences.Count; i++)
        {
            var sentence = currentEvent.sentences[i];
            if (!string.IsNullOrEmpty(sentence.id))
            {
                if (idToIndexMap.ContainsKey(sentence.id))
                {
                    LogController.LogWarning($"Conflicting ID already exists: {sentence.id}");
                }
                else
                {
                    idToIndexMap[sentence.id] = i;
                }
            }
        }
    }  

    /// <summary>
    /// simple cooldown mechanism to prevent accidental fast skipping
    /// </summary>
    private void StartCooldown()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(CooldownCoroutine());
        }
    }

    /// <summary>
    /// cooldown time can be configured
    /// </summary>
    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }


    public bool IsPlayingDialogue()
    {
        return isPlaying;
    }

    public bool IsInChoiceMode()
    {
        return isInChoiceMode;
    }

    public void ClearDialogueQueue()
    {
        dialogueQueue.Clear();
        idToIndexMap.Clear();
        EndDialoguePlayback();
    }

    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
}