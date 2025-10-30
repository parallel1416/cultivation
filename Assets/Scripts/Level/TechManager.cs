using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TechNode
{
    public string id = "";
    public string name = "";
    public string description = "";
    public int cost = 0;
    public List<string> prerequisites = new List<string>();
    public int unlockState = 0;
    public bool canDismantle = false;
    public int dismantleCost = 0;
}

[System.Serializable]
public class TechTreeData
{
    public List<TechNode> nodes;
    public List<TechConnection> connections;

    [System.Serializable]
    public class TechConnection
    {
        public string fromNodeId;
        public string toNodeId;
    }
}

public partial class TechManager : MonoBehaviour
{
    private static TechManager _instance;
    public static TechManager Instance => _instance;

    [SerializeField] private string techTreeFileName = "TechTree";

    // Dictionary to store all technology nodes, with their IDs as keys for quick access.
    private Dictionary<string, TechNode> techNodes = new Dictionary<string, TechNode>();
    public Dictionary<string, TechNode> TechNodes => techNodes;

    // Event fired when a tech is successfully unlocked
    public event Action<string> OnTechUnlocked;

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
    /// Loads the tech tree data from a JSON file located in the Resources folder. 
    /// Only called at new game starts
    /// When load game from a save, use ApplySaveData() instead
    /// </summary>
    public void LoadTechTree()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(techTreeFileName);
        if (jsonFile != null)
        {
            TechTreeData treeData = JsonUtility.FromJson<TechTreeData>(jsonFile.text);
            InitializeTechTree(treeData);
        }
        else
        {
            LogController.LogError($"TechManager: Techtree JSON not found! It should be here: Resources/{techTreeFileName}.json");
        }
    }

    /// <summary>
    /// Initialize tech tree and nodes
    /// Only called by LoadTechTree() at new game starts
    /// </summary>
    private void InitializeTechTree(TechTreeData treeData)
    {
        techNodes.Clear();

        foreach (TechNode node in treeData.nodes)
        {
            if (!string.IsNullOrEmpty(node.id))
            {
                if (techNodes.ContainsKey(node.id))
                {
                    LogController.LogWarning($"TechManager: Conflict tag! ID: {node.id}");
                }
                else
                {
                    techNodes[node.id] = node;
                }
            }
            else LogController.LogError("TechManager: Tech node without ID found!");
        }
    }


    /// <summary>
    /// tech operations
    /// </summary>

    public bool UnlockTech(string techId)
    {
        // subscribe tech tree UI refresh later
        if (!CanUnlockTech(techId)) return false;
        TechNode tech = techNodes[techId];

        if (!LevelManager.Instance.SpendMoney(tech.cost))
        {
            LogController.LogError($"TechManager: Insufficient money for unlocking: {tech.name}, how can this happen??");
            return false;
        }

        tech.unlockState = 1;
        ApplyUnlockTechEffect(techId);
        LogController.Log($"TechManager: Tech unlocked!: {tech.name} ({tech.id})");

        // Fire the unlock event
        OnTechUnlocked?.Invoke(techId);

        return true;
    }

    public bool DismantleTech(string techId)
    {
        // subscribe tech tree UI refresh later
        if (!canDismatleTech(techId)) return false;
        TechNode tech = techNodes[techId];

        if (!LevelManager.Instance.SpendMoney(tech.cost))
        {
            LogController.LogError($"TechManager: Insufficient money for dismantling: {tech.name}, how can this happen??");
            return false;
        }

        tech.unlockState = -1; // -1 or <0 for dismantled
        ApplyDismantleTechEffect(techId);
        LogController.Log($"TechManager: Tech dismantled: {tech.name} ({tech.id})");

        return true;
    }


    // bool checks 

    public bool CanUnlockTech(string techId)
    {
        if (!techNodes.TryGetValue(techId, out TechNode tech))
        {
            LogController.LogError($"TechManager: Tech ID not exist: {techId}");
            return false;
        }

        if (IsTechUnlocked(tech))
        {
            LogController.LogError($"TechManager: Tech is already unlocked: {tech.name} ({techId})");
            return false;
        }

        if (IsTechDismantled(tech))
        {
            LogController.LogError($"TechManager: Tech is regrettably dismantled, cannot unlock: {tech.name} ({techId})");
            return false;
        }

        if (!ArePrerequisitesUnlocked(tech))
        {
            LogController.LogError($"TechManager: Prerequisites not met for tech: {tech.name} ({techId})");
            return false;
        }

        if (LevelManager.Instance.Money < tech.cost)
        {
            LogController.LogError($"TechManager: Not enough money to unlock tech: {tech.name} ({techId})");
            return false;
        }

        return true;
    }

    public bool canDismatleTech(string techId)
    {
        if (!techNodes.TryGetValue(techId, out TechNode tech))
        {
            LogController.LogError($"TechManager: Tech ID not exist: {techId}");
            return false;
        }

        if (!tech.canDismantle)
        {
            LogController.LogError($"TechManager: Tech is not a dismantle-able one: {tech.name} ({techId})");
            return false;
        }

        if (IsTechDismantled(tech))
        {
            LogController.LogError($"TechManager: Tech is already dismantled: {tech.name} ({techId})");
            return false;
        }

        if (!IsTechUnlocked(tech))
        {
            LogController.LogError($"TechManager: Tech is not even unlocked, cannot dismantle: {tech.name} ({techId})");
            return false;
        }

        if (LevelManager.Instance.Money < tech.dismantleCost)
        {
            LogController.LogError($"TechManager: Not enough money to dismantle tech: {tech.name} ({techId})");
            return false;
        }

        return true;
    }

    private bool ArePrerequisitesUnlocked(TechNode tech)
    {
        if (tech.prerequisites == null || tech.prerequisites.Count == 0)
        {
            return true;
        }

        foreach (string prereqId in tech.prerequisites)
        {
            if (!techNodes.TryGetValue(prereqId, out TechNode prereq) || !IsTechUnlocked(prereq))
            {
                return false;
            }
        }

        return true;
    }

    // Only for debugging
    public void DebugTechTreeStatus()
    {
        LogController.Log("=== Techtree Status ===");

        foreach (var kvp in techNodes)
        {
            TechNode node = kvp.Value;
            string status = IsTechUnlocked(node) ? "✓ Unlock" : "✗ Locked"; // Alignment
            LogController.Log($"{status} | {node.name} ({node.id})");
        }

        LogController.Log("========================");
    }

    public void ApplySaveData(SaveData saveData) => techNodes = saveData.techNodes;

    public bool IsTechUnlocked(TechNode node) => 
        node != null && node.unlockState > 0;
    public bool IsTechUnlocked(string techId) =>
        techNodes.ContainsKey(techId) && techNodes[techId].unlockState > 0;

    public bool IsTechDismantled(TechNode node) =>
        node != null && node.unlockState < 0;
    public bool IsTechDismantled(string techId) =>
        techNodes.ContainsKey(techId) && techNodes[techId].unlockState < 0;

    public TechNode GetTechNode(string techId) =>
        techNodes.ContainsKey(techId) ? techNodes[techId] : null;

    public string GetTechName(string techId) =>
        techNodes.ContainsKey(techId) ? techNodes[techId].name : "UnknownTech";
}