using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TechNode
{
    public string id;
    public string name;
    public string description;
    public int cost;
    public List<string> prerequisites;
    public bool isUnlocked;
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

public class TechManager : MonoBehaviour
{
    private static TechManager _instance;
    public static TechManager Instance => _instance;

    // Dictionary to store all technology nodes, with their IDs as keys for quick access.
    private Dictionary<string, TechNode> techNodes = new Dictionary<string, TechNode>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadTechTreeFromJson("TechTree");
    }

    // Loads the tech tree data from a JSON file located in the Resources folder. 
    private void LoadTechTreeFromJson(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            TechTreeData treeData = JsonUtility.FromJson<TechTreeData>(jsonFile.text);
            InitializeTechTree(treeData);
        }
        else
        {
            LogController.LogError($"Techtree Json not found: {fileName}");
        }
    }

    private void InitializeTechTree(TechTreeData treeData)
    {
        foreach (var node in treeData.nodes)
        {
            techNodes[node.id] = node;
        }
    }

    public bool UnlockTech(string techId)
    {
        if (!techNodes.ContainsKey(techId))
        {
            LogController.LogError($"Tech ID not exist: {techId}");
            return false;
        }

        TechNode tech = techNodes[techId];

        if (tech.isUnlocked)
        {
            LogController.LogError($"Tech already unlocked: {tech.name}");
            return false;
        }

        foreach (string prereqId in tech.prerequisites)
        {
            if (!techNodes.ContainsKey(prereqId) || !techNodes[prereqId].isUnlocked)
            {
                LogController.LogError($"Prerequisites not unlocked: {prereqId}");
                return false;
            }
        }

        if (!LevelManager.Instance.SpendMoney(tech.cost))
        {
            LogController.LogError($"Insufficient money: {tech.name}");
            return false;
        }

        tech.isUnlocked = true;
        LogController.Log($"Tech unlocked!: {tech.name}");

        return true;
    }

    // Only for debugging
    public void DebugTechTreeStatus()
    {
        Debug.Log("=== Techtree Status ===");

        foreach (var kvp in techNodes)
        {
            TechNode node = kvp.Value;
            string status = node.isUnlocked ? "✓ Unlock" : "✗ Locked"; // Alignment
            LogController.Log($"{status} | {node.name} ({node.id})");
        }

        LogController.Log("========================");
    }

    // check if a technology is unlocked
    public bool IsTechUnlocked(string techId) =>
        techNodes.ContainsKey(techId) && techNodes[techId].isUnlocked;

    public TechNode GetTechNode(string techId) =>
        techNodes.ContainsKey(techId) ? techNodes[techId] : null;

    public string GetTechName(string techId) =>
        techNodes.ContainsKey(techId) ? techNodes[techId].name : "WARNING: No such tech";
}