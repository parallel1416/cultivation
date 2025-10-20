using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GlobalTag
{
    public string tagID = "";
    public bool isTrue = false;
    public string descriptionTrue = "";
    public string descriptionFalse = "";
}

[System.Serializable]
public class GlobalTagData
{
    public List<GlobalTag> tags = new List<GlobalTag>();
}

public class GlobalTagManager : MonoBehaviour
{
    private static GlobalTagManager _instance;
    public static GlobalTagManager Instance => _instance;

    private Dictionary<string, GlobalTag> tagMap = new Dictionary<string, GlobalTag>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGlobalTags();
    }

    /// <summary>
    /// Load tags from json file
    /// </summary>
    private void LoadGlobalTags()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("GlobalTags");
        if (jsonFile != null)
        {
            GlobalTagData tagData = JsonUtility.FromJson<GlobalTagData>(jsonFile.text);
            InitializeTagDictionary(tagData);
            LogController.Log($"{tagMap.Count} global tags loaded!");
        }
        else
        {
            LogController.LogError("Global tag JSON file not found! It should be here: Resources/GlobalTags.json");
        }
    }

    /// <summary>
    /// Initialize tag map
    /// </summary>
    private void InitializeTagDictionary(GlobalTagData tagData)
    {
        tagMap.Clear();

        foreach (GlobalTag tag in tagData.tags)
        {
            if (!string.IsNullOrEmpty(tag.tagID))
            {
                if (tagMap.ContainsKey(tag.tagID))
                {
                    LogController.LogWarning($"Conflict tag! ID: {tag.tagID}");
                }
                else
                {
                    tagMap[tag.tagID] = tag;
                }
            }
        }
    }

    /// <summary>
    /// set tag true or false
    /// </summary>
    public void SetTag(string tagID, bool value)
    {
        if (tagMap.ContainsKey(tagID))
        {
            tagMap[tagID].isTrue = value;
            LogController.Log($"Set {tagID} to: {value}");
        }
        else
        {
            LogController.LogError($"Tag not found: {tagID}");
        }
    }

    public void EnableTag(string tagID)
    {
        SetTag(tagID, true);
    }

    public void DisableTag(string tagID)
    {
        SetTag(tagID, false);
    }

    /// <summary>
    /// Flip tag value
    /// </summary>
    public void ToggleTag(string tagID)
    {
        if (tagMap.ContainsKey(tagID))
        {
            SetTag(tagID, !tagMap[tagID].isTrue);
        }
        else
        {
            LogController.LogError($"Tag not found: {tagID}");
        }
    }

    /// <summary>
    /// Get tag value
    /// </summary>
    public bool GetTagValue(string tagID)
    {
        if (tagMap.ContainsKey(tagID))
        {
            return tagMap[tagID].isTrue;
        }
        else
        {
            LogController.LogError($"Tag not found: {tagID}");
            return false;
        }
    }

    /// <summary>
    /// get tag desc according to tag value
    /// </summary>
    public string GetTagDescription(string tagID)
    {
        if (tagMap.ContainsKey(tagID))
        {
            GlobalTag tag = tagMap[tagID];
            return tag.isTrue ? tag.descriptionTrue : tag.descriptionFalse;
        }
        else
        {
            LogController.LogError($"Tag not found: {tagID}");
            return "标签不存在！请联系开发者反馈bug。";
        }
    }

    /// <summary>
    /// test for tag existance
    /// </summary>
    public bool HasTag(string tagID)
    {
        return tagMap.ContainsKey(tagID);
    }

    /// <summary>
    /// get all tag IDs
    /// only for debugging, might be useless
    /// </summary>
    public List<string> GetAllTagIDs()
    {
        return new List<string>(tagMap.Keys);
    }

    public List<string> GetEnabledTagIDs()
    {
        List<string> enabledTags = new List<string>();

        foreach (var kvp in tagMap)
        {
            if (kvp.Value.isTrue)
            {
                enabledTags.Add(kvp.Key);
            }
        }

        return enabledTags;
    }

    public Dictionary<string, bool> GetAllTagStates()
    {
        Dictionary<string, bool> states = new Dictionary<string, bool>();

        foreach (var kvp in tagMap)
        {
            states[kvp.Key] = kvp.Value.isTrue;
        }

        return states;
    }

    /// <summary>
    /// print all tag state for debugging
    /// </summary>
    public void DebugAllTags()
    {
        LogController.Log("=== Global Tags ===");

        foreach (var kvp in tagMap)
        {
            GlobalTag tag = kvp.Value;
            string status = tag.isTrue ? "✓" : "✗";
            string description = tag.isTrue ? tag.descriptionTrue : tag.descriptionFalse;

            LogController.Log($"{status} {tag.tagID}: {description}");
        }

        LogController.Log("====================");
    }
}