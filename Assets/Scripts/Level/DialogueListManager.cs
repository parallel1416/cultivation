using System.Collections.Generic;
using System.Linq;
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

    // No routine turn numbers
    [SerializeField]
    private IReadOnlyList<int> noRoutineTurnNumbers = new List<int>()
    {
        13, 16
    };

    // Tech needs
    [SerializeField] private string cultivationTechLevelId_1 = "farm_1";
    [SerializeField] private string cultivationTechLevelId_2 = "farm_2";
    [SerializeField] private string cultivationTechLevelId_3 = "farm_3";

    [SerializeField] private string jingshiTech = "recruit";
    [SerializeField] private string jianjunTech = "recruit_special_1";
    [SerializeField] private string yuezhengTech = "recruit_special_3";

    // Dialogue random pools
    // Readonly and not auto-generate id pool for bigger room of modification
    [SerializeField]
    private IReadOnlyList<string> CultivationDialoguePool_1 = new List<string>()
    {
        "daily_normal_1",
        "daily_normal_2",
        "daily_normal_3",
        "daily_normal_4",
        "daily_normal_5",
        "daily_normal_6",
        "daily_normal_7",
        "daily_normal_8",
        "daily_normal_9",
        "daily_normal_10"
    };

    [SerializeField]
    private IReadOnlyList<string> CultivationDialoguePool_2 = new List<string>()
    {
        "daily_medium_1",
        "daily_medium_2",
        "daily_medium_3",
        "daily_medium_4",
        "daily_medium_5",
        "daily_medium_6",
        "daily_medium_7",
        "daily_medium_8",
        "daily_medium_9",
        "daily_medium_10"
    };

    [SerializeField]
    private IReadOnlyList<string> CultivationDialoguePool_3 = new List<string>()
    {
        "daily_high_1",
        "daily_high_2",
        "daily_high_3",
        "daily_high_4",
        "daily_high_5",
        "daily_high_6",
        "daily_high_7",
        "daily_high_8",
        "daily_high_9",
        "daily_high_10"
    };

    [SerializeField]
    private IReadOnlyList<string> CultivationDialoguePool_bug = new List<string>()
    {
        "daily_bug_1",
        "daily_bug_2",
        "daily_bug_3",
        "daily_bug_4",
        "daily_bug_5",
        "daily_bug_6",
        "daily_bug_7"
    };

    [SerializeField]
    private IReadOnlyList<string> AdventureDialoguePool = new List<string>()
    {
        "adventure_1",
        "adventure_2",
        "adventure_3",
        "adventure_4",
        "adventure_5",
        "adventure_6",
        "adventure_7",
        "adventure_8",
        "adventure_9",
        "adventure_10"
    };

    [SerializeField]
    private IReadOnlyList<string> AdventureDialoguePool_Bug = new List<string>()
    {
        "adventure_bug_1",
        "adventure_bug_2",
        "adventure_bug_3",
        "adventure_bug_4",
        "adventure_bug_5",
        "adventure_bug_6",
        "adventure_bug_7",
        "adventure_bug_8",
        "adventure_bug_9",
        "adventure_bug_10"
    };

    // Mainline dialogue events, show on event map, trigger when player clicks
    [SerializeField]
    private IReadOnlyDictionary<int, List<string>> mainlineInTurnDialogueEvents_Unconditional = new Dictionary<int, List<string>>()
    {
        // turn number, dialogue IDs
        { 7, new List<string> { "moren_1" } },
        { 9, new List<string> { "buzhe_1" } },
        { 11, new List<string> { "moren_2" } },
        { 12, new List<string> { "buzhe_2" } },
        { 16, new List<string> { "buzhe_3" } }
    };

    // Mainline dialogue events, not show on event map, trigger at turn starts automatically
    [SerializeField]
    private IReadOnlyDictionary<int, List<string>> mainlineAtOnceDialogueEvents = new Dictionary<int, List<string>>()
    {
        // turn number, dialogue IDs
        { 1, new List<string> { "entry"} },
        { 2, new List<string> { "stars" } },
        { 4, new List<string> { "zhangmen_exit" } },
        { 5, new List<string> { "letter" } },
        { 14, new List<string> { "xianhui_result" } },
        { 16, new List<string> { "transition", "master_1" } },
        { 20, new List<string> { "final" } },
    };

    // Special disciple dialogues
    [SerializeField] private int specialDisciplePersonalDialogueInterval = 2;
    private int jingshiEvent1TargetTurn;
    private int jingshiEvent2TargetTurn;
    private int jianjunEvent1TargetTurn;
    private int jianjunEvent2TargetTurn;
    private int yuezhengEvent1TargetTurn;
    private int yuezhengEvent2TargetTurn;

    private int morenHelpStartTurn;

    public bool HasMajorEventsNotConfirmed
    {
        get
        {
            if (HasMajorInList())
            {
                return true;
            }
            return false;
        }
        set { }
    }

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

    private void ClearTurnDialogues() => currentTurnDialogues.Clear();
    private void AddDialogue(string dialogue)
    {
        if (NowHasDialogue(dialogue)) return;
        currentTurnDialogues.Add(dialogue);
    }

    private void RemoveDialogue(string dialogue)
    {
        if (currentTurnDialogues == null) return;
        if (!NowHasDialogue(dialogue)) return;
        currentTurnDialogues.RemoveAll(x => x == dialogue);
    }

    private void RemoveDialoguePool(IReadOnlyList<string> dialogues)
    {
        if (currentTurnDialogues == null) return;
        foreach (string dialogue in dialogues)
            RemoveDialogue(dialogue);
    }

    public bool NowHasDialogue(string dialogue) => currentTurnDialogues.Contains(dialogue);
    public bool NowHasDialogueInPool(IReadOnlyList<string> dialogues)
    {
        foreach (string dialogue in dialogues)
        {
            if (NowHasDialogue(dialogue))
                return true;
        }
        return false;
    }


    /// <summary>
    /// Make sure this method will only be called in SetUpTurnDialogues();
    /// calls within turns may cause duplicate dialogues or a different dialogue event from pool after an in-turn refresh.
    /// </summary>
    public void AddDialogueFromPool(IReadOnlyList<string> dialoguePool)
    {
        if (dialoguePool == null || dialoguePool.Count == 0)
        {
            LogController.Log($"ListManager: Dialogue pool is null or empty, skipping");
            return;
        }

        if (NowHasDialogueInPool(dialoguePool))
        {
            LogController.Log("ListManager: Already have a dialogue from the same pool, skipping");
            return;
        }
        
        int randomIndex = Random.Range(0, dialoguePool.Count);
        string dialogue = dialoguePool[randomIndex];
        AddDialogue(dialogue);
        LogController.Log($"ListManager: Added dialogue from pool: {dialogue}");
    }


    /// <summary>
    /// SetUpDialogues methods, called when turn starts or at dialogue event refresh(e.g. after player quit from techtree scene)
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
        SetUpCultivationDialogues(turnNumber);
        SetUpAdventureDialogues(turnNumber);
        SetUpSpecialDiscipleDialogues(turnNumber);
        SetUpOtherDialogues(turnNumber);
        
        LogController.Log($"SetUpTurnDialogues complete. Total dialogues: {currentTurnDialogues.Count}");
    }

    private void SetUpMainlineDialogues(int currentTurn)
    {
        // XianMenShengHui
        if (currentTurn == 13)
        {
            if (!TechManager.Instance.IsTechUnlocked("story_2")) // not prepared
            {
                AddDialogue("xianhui_be"); // game over
                return;
            }
            AddDialogue("xianhui_0");
            AddDialogue("xianhui_1");
            AddDialogue("xianhui_2");
            AddDialogue("xianhui_3");
            AddDialogue("xianhui_4");
        }

        if (!mainlineInTurnDialogueEvents_Unconditional.ContainsKey(currentTurn))
        {
            return; // No unconditional mainline dialogues for this turn
        }
        
        List<string> dialogues = mainlineInTurnDialogueEvents_Unconditional[currentTurn];
        if (dialogues == null) return;
        foreach (string dialogue in dialogues)
        {
            AddDialogue(dialogue);
        }
    }

    private void SetUpCultivationDialogues(int turn)
    {
        if (noRoutineTurnNumbers.Contains(turn)) return;
        if (TechManager.Instance == null)
        {
            LogController.LogWarning("SetUpCultivationDialogues: TechManager.Instance is null, skipping");
            return;
        }
        
        if (LevelManager.Instance.IsBuggy)
        {
            AddDialogueFromPool(CultivationDialoguePool_bug);
            return;
        }
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_3))
        {
            AddDialogueFromPool(CultivationDialoguePool_3);
            return;
        }
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_2))
        {
            AddDialogueFromPool(CultivationDialoguePool_2);
            return;
        }
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_1))
        {
            AddDialogueFromPool(CultivationDialoguePool_1);
            return;
        }
    }

    private void SetUpAdventureDialogues(int turn)
    {
        if (noRoutineTurnNumbers.Contains(turn)) return;
        if (turn >= 14 && LevelManager.Instance.IsBuggy == true)
        {
            AddDialogueFromPool(AdventureDialoguePool_Bug);
            return;
        }
        AddDialogueFromPool(AdventureDialoguePool);
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
        if (jingshiEvent1TargetTurn != 0 &&
            currentTurn >= jingshiEvent1TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jingshi_event_1_triggered"))
            AddDialogue("jingshi_1");
        if (jianjunEvent1TargetTurn != 0 &&
            currentTurn >= jianjunEvent1TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jianjun_event_1_triggered"))
            AddDialogue("jianjun_1");
        if (yuezhengEvent1TargetTurn != 0 &&
            currentTurn >= yuezhengEvent1TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("yuezheng_event_1_triggered"))
            AddDialogue("yuezheng_1");

        // phase 2
        if (jingshiEvent2TargetTurn != 0 &&
            currentTurn >= jingshiEvent2TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jingshi_event_2_triggered"))
            AddDialogue("jingshi_2");
        if (jingshiEvent2TargetTurn != 0 &&
            currentTurn >= jianjunEvent2TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("jianjun_event_2_triggered"))
            AddDialogue("jianjun_2");
        if (jingshiEvent2TargetTurn != 0 &&
            currentTurn >= yuezhengEvent2TargetTurn &&
            !GlobalTagManager.Instance.GetTagValue("yuezheng_event_2_triggered"))
            AddDialogue("yuezheng_2");

        // phase 3
        if (currentTurn >= 15 &&
            GlobalTagManager.Instance.GetTagValue("jianjun_event_2_triggered") &&
            !GlobalTagManager.Instance.GetTagValue("jianjun_event_3_triggered"))
            AddDialogue("jianjun_3");
        if (currentTurn >= 16 &&
            GlobalTagManager.Instance.GetTagValue("jingshi_event_2_triggered") &&
            !GlobalTagManager.Instance.GetTagValue("jingshi_event_3_triggered"))
            AddDialogue("jingshi_3");
        if (currentTurn >= 17 &&
            GlobalTagManager.Instance.GetTagValue("yuezheng_event_2_triggered") &&
            !GlobalTagManager.Instance.GetTagValue("yuezheng_event_3_triggered"))
            AddDialogue("yuezheng_3");


        // phase 4
        if (currentTurn >= 18 &&
            GlobalTagManager.Instance.GetTagValue("jianjun_event_3_triggered") &&
            !GlobalTagManager.Instance.GetTagValue("jianjun_event_4_triggered"))
        if (currentTurn >= 18 &&
            GlobalTagManager.Instance.GetTagValue("momyz") &&
            !GlobalTagManager.Instance.GetTagValue("yuezheng_event_4_triggered"))
            AddDialogue("yuezheng_4");
    }

    private void SetUpOtherDialogues(int currentTurn)
    {
        if (currentTurn >= 14 &&
            currentTurn % 2 == 0 &&
            !GlobalTagManager.Instance.GetTagValue("moren_captured"))
            AddDialogue("moren_2.1");

        if (GlobalTagManager.Instance.GetTagValue("moren_captured") &&
            !GlobalTagManager.Instance.GetTagValue("moren_sacrificeable") &&
            !GlobalTagManager.Instance.GetTagValue("moren_ally"))
            AddDialogue("moren_3");

        if (GlobalTagManager.Instance.GetTagValue("moren_ally"))
        {
            if (morenHelpStartTurn == 0)
            {
                morenHelpStartTurn = currentTurn;
                AddDialogue("moren_4");
            }
                
            else if ((currentTurn - morenHelpStartTurn) % 2 == 0 &&
                     !GlobalTagManager.Instance.GetTagValue("moren_help_event_1_triggered"))
                AddDialogue("moren_4");
        }
    }


    /// <summary>
    /// CORE!
    /// Refresh "paper slips" on event map scene UI.
    /// </summary>
    public void RefreshDialogueList()
    {
        RefreshCultivation();
    }

    private void RefreshCultivation()
    {
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_3))
        {
            RemoveDialoguePool(CultivationDialoguePool_1);
            RemoveDialoguePool(CultivationDialoguePool_2);
            AddDialogueFromPool(CultivationDialoguePool_3);
            return;
        }
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_2))
        {
            RemoveDialogue("farm_1");
            AddDialogue("farm_2");
            return;
        }
        if (TechManager.Instance.IsTechUnlocked(cultivationTechLevelId_1))
        {
            AddDialogue("farm_1");
            return;
        }
    }


    /// <summary>
    /// CORE!
    /// Automatically play dialogue events at turn starts.
    /// Will not paste any "paper slips" to event map scene UI.
    /// </summary>
    public void PushToPlayMainlineDialogues()
    {
        int currentTurn = TurnManager.Instance.CurrentTurn;
        List<string> dialogues = new List<string>();
        if(mainlineAtOnceDialogueEvents != null) mainlineAtOnceDialogueEvents.TryGetValue(currentTurn, out dialogues);

        if (dialogues == null || dialogues.Count == 0) return;
        foreach (string dialogue in dialogues)
        {
            DialogueManager.PlayDialogueEvent(dialogue); // Directly play in dialogue scene
        }
    }

    private bool HasMajorInList()
    {
        if (currentTurnDialogues == null || currentTurnDialogues.Count == 0) return false;
        
        foreach(var dialogue in currentTurnDialogues)
        {
            if (DialogueManager.Instance.IsMajor(dialogue))
                return true;
        }
        return false;
    }

    private bool IsNoRoutineTurn(int turn) => noRoutineTurnNumbers.Contains(turn);
}
