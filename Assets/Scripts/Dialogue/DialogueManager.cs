using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class DialogueEvent
{
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
    public string difficultyClass = "0";
    public string checkWhat = "";
    public string successTarget = "";
    public string failureTarget = "";
}

[System.Serializable]
public class ChoiceOption
{
    public string text = "";
    public string target = "";
}

public class DialogueManager : MonoBehaviour
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
    /// Start dialogue playback from the queue, bound to next turn method ( TurnManager.NextTurn() )
    /// </summary>
    public void StartDialoguePlayback()
    {
        if (dialogueQueue.Count > 0 && !isPlaying)
        {
            isPlaying = true;
            currentEvent = dialogueQueue.Dequeue();
            BuildIdIndexMap();
            currentSentenceIndex = 0;
            PlayCurrentSentence();
        }
    }

    /// <summary>
    /// Load DialogueEvent from json file
    /// </summary>
    private DialogueEvent LoadDialogueEvent(string eventId)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Dialogues/{eventId}");
        if (jsonFile != null)
        {
            return JsonUtility.FromJson<DialogueEvent>(jsonFile.text);
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
    /// 3 types of sentence playback: default, choice, check
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

            default:
                PlayDefaultSentence(sentence);
                break;
        }
    }

    private void PlayDefaultSentence(DialogueSentence sentence)
    {
        string output = FormatDialogueOutput(sentence);
        OutputDialogue(output);

        StartCooldown();
    }

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

            // temporary, might be deprecated in the future, might not
            isInChoiceMode = true; 
            LogController.Log("press key (1-" + sentence.choices.Count + ") to select answer");
        }
        else
        {
            LogController.LogError("no choice within sentence!");
        }
    }

    private void PlayCheckSentence(DialogueSentence sentence)
    {
        int dc = GetDifficultyClass(sentence.difficultyClass);
        int checkResult = GetCheckResult(sentence.checkWhat);
        string resultDescription = GenerateCheckResultDescription(dc, checkResult, sentence.checkWhat);

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
        string successText = isSuccess ? "成功！" : "失败！"; // Full-width exclamation mark for better compatibility with Chinese fonts

        string checkDescription = "";
        switch (checkWhat)
        {
            case "diceroll":
                checkDescription = $"1d6 = {checkResult}";
                break;

            case "money":
                checkDescription = $"当前灵石数量 = {checkResult}";
                break;

            default:
                checkDescription = $"无效检测目标，默认设置为 {checkResult}";
                break;
        }

        string comparison = isSuccess ? ">=" : "<";

        // Sample Style: [ 成功！] ( DC = 5, 1d6 = 6 >= 5 )
        return $"[ {successText}] ( DC = {dc}, {checkDescription} {comparison} {dc} )";
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

    private void OutputDialogue(string message)
    {
        // temporarily use Debug.Log for output
        // should be replaced by proper UI display later
        Debug.Log(message);
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