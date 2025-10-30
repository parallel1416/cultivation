using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages loading and accessing character, spirit, and treasure descriptions from JSON
/// </summary>
public class DescriptionManager : MonoBehaviour
{
    private static DescriptionManager _instance;
    public static DescriptionManager Instance => _instance;

    [System.Serializable]
    public class EntityDescription
    {
        public string id;
        public string name;
        public string description;
        public string effect; // Only for spirits and treasures
    }

    [System.Serializable]
    public class DescriptionData
    {
        public List<EntityDescription> characters = new List<EntityDescription>();
        public List<EntityDescription> spirits = new List<EntityDescription>();
        public List<EntityDescription> treasures = new List<EntityDescription>();
    }

    private DescriptionData descriptionData;
    private Dictionary<string, EntityDescription> characterMap = new Dictionary<string, EntityDescription>();
    private Dictionary<string, EntityDescription> spiritMap = new Dictionary<string, EntityDescription>();
    private Dictionary<string, EntityDescription> treasureMap = new Dictionary<string, EntityDescription>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDescriptions();
    }

    private void LoadDescriptions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Descriptions");
        if (jsonFile != null)
        {
            descriptionData = JsonUtility.FromJson<DescriptionData>(jsonFile.text);
            
            // Build lookup dictionaries
            foreach (var character in descriptionData.characters)
            {
                characterMap[character.id] = character;
            }
            
            foreach (var spirit in descriptionData.spirits)
            {
                spiritMap[spirit.id] = spirit;
            }
            
            foreach (var treasure in descriptionData.treasures)
            {
                treasureMap[treasure.id] = treasure;
            }

            LogController.Log($"DescriptionManager: Loaded {characterMap.Count} characters, {spiritMap.Count} spirits, {treasureMap.Count} treasures");
        }
        else
        {
            LogController.LogError("DescriptionManager: Could not load Descriptions.json from Resources folder!");
        }
    }

    public EntityDescription GetCharacterDescription(string id)
    {
        return characterMap.ContainsKey(id) ? characterMap[id] : null;
    }

    public EntityDescription GetSpiritDescription(string id)
    {
        return spiritMap.ContainsKey(id) ? spiritMap[id] : null;
    }

    public EntityDescription GetTreasureDescription(string id)
    {
        return treasureMap.ContainsKey(id) ? treasureMap[id] : null;
    }

    public string FormatDescription(EntityDescription entity)
    {
        if (entity == null) return "Unknown";

        string result = $"<b>{entity.name}</b>\n{entity.description}";
        
        if (!string.IsNullOrEmpty(entity.effect))
        {
            result += $"\n<color=#FFD700>效果：{entity.effect}</color>";
        }

        return result;
    }
}
