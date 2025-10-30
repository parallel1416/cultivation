using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Binds tech nodes to dialogue events in the Tower scene.
/// When a tech is unlocked, immediately plays the associated dialogue event.
/// After dialogue ends, returns to Tower scene.
/// </summary>
public class TechDialogueBinder : MonoBehaviour
{
    [Serializable]
    private class TechDialogueMapping
    {
        [Tooltip("Tech node ID (must match TechManager tech node IDs)")]
        public string techId;
        
        [Tooltip("Dialogue event ID to play when this tech is unlocked")]
        public string dialogueEventId;
        
        [Tooltip("Should this dialogue play only once?")]
        public bool playOnce = true;
        
        [HideInInspector]
        public bool hasPlayed = false;
    }

    [Header("Tech → Dialogue Mappings")]
    [SerializeField] private List<TechDialogueMapping> techDialogueMappings = new List<TechDialogueMapping>();

    [Header("Events")]
    [SerializeField] private UnityEvent onDialogueTriggered;

    private Dictionary<string, TechDialogueMapping> mappingLookup = new Dictionary<string, TechDialogueMapping>();
    private bool isListening = false;

    private void Awake()
    {
        // Build lookup dictionary
        mappingLookup.Clear();
        foreach (var mapping in techDialogueMappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.techId))
            {
                Debug.LogWarning("TechDialogueBinder: Empty tech ID in mapping list.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(mapping.dialogueEventId))
            {
                Debug.LogWarning($"TechDialogueBinder: Empty dialogue event ID for tech '{mapping.techId}'.");
                continue;
            }

            if (mappingLookup.ContainsKey(mapping.techId))
            {
                Debug.LogWarning($"TechDialogueBinder: Duplicate tech ID '{mapping.techId}' in mapping list.");
                continue;
            }

            mappingLookup.Add(mapping.techId, mapping);
        }

        Debug.Log($"TechDialogueBinder: Initialized with {mappingLookup.Count} tech-dialogue mappings.");
    }

    private void OnEnable()
    {
        StartListening();
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void StartListening()
    {
        if (isListening)
        {
            return;
        }

        if (TechManager.Instance == null)
        {
            Debug.LogWarning("TechDialogueBinder: TechManager instance not found. Cannot start listening.");
            return;
        }

        // Subscribe to tech unlock event
        TechManager.Instance.OnTechUnlocked += OnTechUnlocked;
        isListening = true;

        Debug.Log("TechDialogueBinder: Started listening to tech unlock events.");
    }

    private void StopListening()
    {
        if (!isListening)
        {
            return;
        }

        if (TechManager.Instance != null)
        {
            TechManager.Instance.OnTechUnlocked -= OnTechUnlocked;
        }

        isListening = false;
    }

    private void OnTechUnlocked(string techId)
    {
        if (string.IsNullOrWhiteSpace(techId))
        {
            return;
        }

        // Check if this tech has a dialogue mapping
        if (!mappingLookup.TryGetValue(techId, out TechDialogueMapping mapping))
        {
            // No dialogue bound to this tech
            return;
        }

        // Check if already played (if playOnce is enabled)
        if (mapping.playOnce && mapping.hasPlayed)
        {
            Debug.Log($"TechDialogueBinder: Dialogue '{mapping.dialogueEventId}' for tech '{techId}' has already been played.");
            return;
        }

        // Mark as played
        mapping.hasPlayed = true;

        // Play dialogue event immediately
        PlayDialogueForTech(mapping);
    }

    private void PlayDialogueForTech(TechDialogueMapping mapping)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError($"TechDialogueBinder: DialogueManager not found. Cannot play dialogue '{mapping.dialogueEventId}' for tech '{mapping.techId}'.");
            return;
        }

        // Get dialogue definition
        DialogueEvent dialogueDefinition = DialogueManager.Instance.GetDialogueDefinition(mapping.dialogueEventId);
        if (dialogueDefinition == null)
        {
            Debug.LogError($"TechDialogueBinder: Dialogue event '{mapping.dialogueEventId}' not found in DialogueManager.");
            return;
        }

        // Trigger event
        onDialogueTriggered?.Invoke();

        // Play dialogue immediately (returns to Tower scene after completion)
        DialogueManager.PlayDialogueEvent(mapping.dialogueEventId, "TowerScene");

        Debug.Log($"TechDialogueBinder: Playing dialogue '{mapping.dialogueEventId}' for tech '{mapping.techId}'. Will return to TowerScene after completion.");
    }

    /// <summary>
    /// Manually trigger dialogue for a specific tech (useful for testing or special cases)
    /// </summary>
    public void ManuallyTriggerDialogue(string techId)
    {
        if (string.IsNullOrWhiteSpace(techId))
        {
            Debug.LogWarning("TechDialogueBinder: Cannot manually trigger dialogue - tech ID is empty.");
            return;
        }

        if (!mappingLookup.TryGetValue(techId, out TechDialogueMapping mapping))
        {
            Debug.LogWarning($"TechDialogueBinder: No dialogue mapping found for tech '{techId}'.");
            return;
        }

        PlayDialogueForTech(mapping);
    }

    /// <summary>
    /// Reset the "hasPlayed" flag for all mappings (useful for testing)
    /// </summary>
    public void ResetAllPlayedFlags()
    {
        foreach (var mapping in techDialogueMappings)
        {
            mapping.hasPlayed = false;
        }

        Debug.Log("TechDialogueBinder: Reset all 'hasPlayed' flags.");
    }

    /// <summary>
    /// Check if a tech has a dialogue mapping
    /// </summary>
    public bool HasDialogueMapping(string techId)
    {
        return !string.IsNullOrWhiteSpace(techId) && mappingLookup.ContainsKey(techId);
    }

    /// <summary>
    /// Get the dialogue event ID for a tech
    /// </summary>
    public string GetDialogueEventId(string techId)
    {
        if (string.IsNullOrWhiteSpace(techId) || !mappingLookup.TryGetValue(techId, out TechDialogueMapping mapping))
        {
            return null;
        }

        return mapping.dialogueEventId;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Validate mappings in editor
        HashSet<string> seenTechIds = new HashSet<string>();
        
        foreach (var mapping in techDialogueMappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.techId))
            {
                continue;
            }

            if (seenTechIds.Contains(mapping.techId))
            {
                Debug.LogWarning($"TechDialogueBinder: Duplicate tech ID '{mapping.techId}' detected in mappings.", this);
            }
            else
            {
                seenTechIds.Add(mapping.techId);
            }
        }
    }
#endif
}
