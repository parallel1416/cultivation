using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Decide and manage the list of dialogues in current turn.
/// </summary>
public class DialogueListManager : MonoBehaviour
{
    private static DialogueListManager _instance;
    public static DialogueListManager Instance => _instance;

    private List<string> currentTurnDialogues = new List<string>();

    // Get this for refresh, can be used after turn starts or player leave the techtree scene, etc.
    public List<string> CurrentTurnDialogues => currentTurnDialogues;

    // Tech needs
    [SerializeField] private string cultivationTechLevelId_1 = "farm_1";
    [SerializeField] private string cultivationTechLevelId_2 = "farm_2";
    [SerializeField] private string cultivationTechLevelId_3 = "farm_3";

    [SerializeField] private string jingshiTech = "recruit";
    [SerializeField] private string jianjunTech = "recruit_special_1";
    [SerializeField] private string yuezhengTech = "recruit_special_3";

    // Tutorial dialogue lists (deprecated)
    //[SerializeField]
    //private readonly List<string> TutorialDialogues = new List<string>()
    //{
    //    // �̶̳Ի�
    //    "grass",
    //    "mouse",
    //    "stars"
    //};

    // Dialogue random pools
    [SerializeField]
    private readonly List<string> ExpeditionDialoguePool = new List<string>()
    { 
        // ��ɽ����
    };

    // Mainline dialogue events, show on event map, trigger when player clicks
    [SerializeField]
    IReadOnlyDictionary<int, List<string>> mainlineInTurnDialogueEvents = new Dictionary<int, List<string>>()
    {
        // turn number, dialogue IDs
        { 7, new List<string> { "moren_1" } },
        { 9, new List<string> { "buzhe_1" } },
        { 11, new List<string> { "moren_2" } },
        { 12, new List<string> { "buzhe_2" } }
    };

    // Mainline dialogue events, not show on event map, trigger at turn starts automatically
    [SerializeField]
    IReadOnlyDictionary<int, List<string>> mainlineAtOnceDialogueEvents = new Dictionary<int, List<string>>()
    {
        // turn number, dialogue IDs
        { 2, new List<string> { "stars" } },
        { 4, new List<string> { "zhangmen_exit" } },
        { 5, new List<string> { "letter" } }
    };

    // Special disciple dialogues
    [SerializeField] private int specialDisciplePersonalDialogueInterval = 2;
    private int jingshiEvent1TargetTurn;
    private int jingshiEvent2TargetTurn;
    private int jianjunEvent1TargetTurn;
    private int jianjunEvent2TargetTurn;
    private int yuezhengEvent1TargetTurn;
    private int yuezhengEvent2TargetTurn;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Basic methods for add dialogue events to an "available list" of event map scene
    /// </summary>

    public void ClearTurnDialogues() => currentTurnDialogues.Clear();
    public void AddDialogue(string dialogue) => currentTurnDialogues.Add(dialogue);
    public void AddDialogues(List<string> dialogues) => currentTurnDialogues.AddRange(dialogues);

    /// <summary>
    /// Make sure this method will only be called in SetUpTurnDialogues();
    /// calls within turns may cause duplicate dialogues or a different dialogue event from pool after an in-turn refresh.
    /// </summary>
    public void AddDialogueFromPool(List<string> dialoguePool)
    {
        if (dialoguePool == null || dialoguePool.Count == 0)
        {
            LogController.Log("AddDialogueFromPool: Pool is null or empty, skipping");
            return;
        }
        
        int randomIndex = Random.Range(0, dialoguePool.Count);
        AddDialogue(dialoguePool[randomIndex]);
        LogController.Log($"Added dialogue from pool: {dialoguePool[randomIndex]}");
    }


    /// <summary>
    /// SetUpDialogues methods, called when turn starts or at dialogue event refresh(e.g. after)
    /// </summary>


    /// <summary>
    /// CORE!
    /// Paste "paper slips" to event map scene UI at turn starts.
    /// Please only call once at every turn start.
    /// </summary>
    public void SetUpTurnDialogues()
    {
        currentTurnDialogues.Clear();
        
        if (TurnManager.Instance == null)
        {
            LogController.LogError("SetUpTurnDialogues: TurnManager.Instance is null!");
            return;
        }
        
        int turnNumber = TurnManager.Instance.CurrentTurn;
        LogController.Log($"SetUpTurnDialogues for turn {turnNumber}");
        
        //SetUpTutorialDialogues(turnNumber);
        SetUpMainlineDialogues(turnNumber);
        SetUpCultivationDialogues();
        SetUpExpeditionDialogues();
        SetUpSpecialDiscipleDialogues(turnNumber);
        
        LogController.Log($"SetUpTurnDialogues complete. Total dialogues: {currentTurnDialogues.Count}");
    }


    // deprecated
    //private void SetUpTutorialDialogues(int currentTurn)
    //{
    //    // if (currentTurn == 1) AddDialogues(TutorialDialogues);
    //}

    private void SetUpMainlineDialogues(int currentTurn)
    {
        if (!mainlineInTurnDialogueEvents.ContainsKey(currentTurn))
        {
            return; // No mainline dialogues for this turn
        }
        
        List<string> dialogues = mainlineInTurnDialogueEvents[currentTurn];
        if (dialogues == null) return;
        foreach (string dialogue in dialogues)
        {
            AddDialogue(dialogue);
        }
    }

    private void SetUpCultivationDialogues()
    {
        if (TechManager.Instance == null)
        {
            LogController.LogWarning("SetUpCultivationDialogues: TechManager.Instance is null, skipping");
            return;
        }
        
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_3))
        {
            AddDialogue("farm_3");
            return;
        }
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_2))
        {
            AddDialogue("farm_2");
            return;
        }
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_1))
        {
            AddDialogue("farm_1");
            return;
        }
    }

    private void SetUpExpeditionDialogues()
    {
        AddDialogueFromPool(ExpeditionDialoguePool);
    }

    /// <summary>
    /// stupid, simple, but stable
    /// </summary>
    private void SetUpSpecialDiscipleDialogues(int currentTurn)
    {
        if (TechManager.Instance == null || GlobalTagManager.Instance == null)
        {
            LogController.LogWarning("SetUpSpecialDiscipleDialogues: TechManager or GlobalTagManager is null, skipping");
            return;
        }
        
        // personal dialogue event phase 1 target confirm
        if (TechManager.Instance.IsTechUnlocked(jingshiTech) && 
            !GlobalTagManager.Instance.GetTagValue("jingshi_event_1_triggered")) 
            jingshiEvent1TargetTurn = currentTurn;

        if (TechManager.Instance.IsTechUnlocked(jianjunTech) &&
            !GlobalTagManager.Instance.GetTagValue("jianjun_event_1_triggered"))
            jianjunEvent1TargetTurn = currentTurn;

        if (TechManager.Instance.IsTechUnlocked(yuezhengTech) &&
            !GlobalTagManager.Instance.GetTagValue("yuezheng_event_1_triggered"))
            yuezhengEvent1TargetTurn = currentTurn;

        // personal dialogue event phase 2 target confirm
        if (GlobalTagManager.Instance.GetTagValue("jingshi_event_1_triggered"))
            jingshiEvent2TargetTurn = currentTurn + specialDisciplePersonalDialogueInterval - 1;
        if (GlobalTagManager.Instance.GetTagValue("jianjun_event_1_triggered"))
            jianjunEvent2TargetTurn = currentTurn + specialDisciplePersonalDialogueInterval - 1;
        if (GlobalTagManager.Instance.GetTagValue("yuezheng_event_1_triggered"))
            yuezhengEvent2TargetTurn = currentTurn + specialDisciplePersonalDialogueInterval - 1;

        // add personal dialogue events to corresponding current turn dialogues
        // phase 1
        if (currentTurn >= jingshiEvent1TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jingshi_event_1_triggered"))
            AddDialogue("jingshi_event_1");
        if (currentTurn == jianjunEvent1TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jianjun_event_1_triggered"))
            AddDialogue("jianjun_event_1");
        if (currentTurn == yuezhengEvent1TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("yuezheng_event_1_triggered"))
            AddDialogue("yuezheng_event_1");

        // phase 2
        if (currentTurn == jingshiEvent2TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jingshi_event_2_triggered"))
            AddDialogue("jingshi_event_2");
        if (currentTurn == jianjunEvent2TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jianjun_event_2_triggered"))
            AddDialogue("jianjun_event_2");
        if (currentTurn == yuezhengEvent2TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("yuezheng_event_2_triggered"))
            AddDialogue("yuezheng_event_2");
    }



    /// <summary>
    /// CORE!
    /// Automatically play dialogue events at turn starts.
    /// Will not paste any "paper slips" to event map scene UI.
    /// </summary>
    private void PushToPlayMainlineDialogues(int currentTurn)
    {
        List<string> dialogues = mainlineAtOnceDialogueEvents[currentTurn];
        if (dialogues == null) return;
        foreach (string dialogue in dialogues)
        {
            DialogueManager.PlayDialogueEvent(dialogue); // Directly play in dialogue scene
        }
    }
}
