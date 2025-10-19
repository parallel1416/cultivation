using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Binds UI toggles in the Tower scene to specific technology IDs defined in the TechManager's tech tree.
/// When a toggle is switched on, the script attempts to unlock the corresponding technology.
/// </summary>
public class TechToggleBinder : MonoBehaviour
{
    [Serializable]
    private struct TechToggleEntry
    {
        public Toggle toggle;
        public string techId;
    }

    [Header("Toggle → TechID Mapping")]
    [SerializeField] private List<TechToggleEntry> toggleMappings = new List<TechToggleEntry>();

    private readonly Dictionary<Toggle, string> runtimeMap = new Dictionary<Toggle, string>();
    private readonly Dictionary<Toggle, UnityAction<bool>> listenerMap = new Dictionary<Toggle, UnityAction<bool>>();
    private bool isSyncingState;

    private void Awake()
    {
        InitializeMappings();
    }

    private void OnEnable()
    {
        SyncToggleStatesFromTechTree();
    }

    private void OnDestroy()
    {
        foreach (var kvp in listenerMap)
        {
            if (kvp.Key != null)
            {
                kvp.Key.onValueChanged.RemoveListener(kvp.Value);
            }
        }

        listenerMap.Clear();
        runtimeMap.Clear();
    }

    private void InitializeMappings()
    {
        runtimeMap.Clear();
        listenerMap.Clear();

        foreach (var entry in toggleMappings)
        {
            if (entry.toggle == null)
            {
                Debug.LogWarning("TechToggleBinder: Toggle reference is missing in the mapping list.", this);
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.techId))
            {
                Debug.LogWarning($"TechToggleBinder: TechID is empty for toggle {entry.toggle.name}.", this);
                continue;
            }

            if (runtimeMap.ContainsKey(entry.toggle))
            {
                Debug.LogWarning($"TechToggleBinder: Duplicate toggle entry detected for {entry.toggle.name}.", this);
                continue;
            }

            var toggle = entry.toggle;
            runtimeMap.Add(toggle, entry.techId);

            UnityAction<bool> handler = isOn => OnToggleValueChanged(toggle, isOn);
            listenerMap.Add(toggle, handler);
            toggle.onValueChanged.AddListener(handler);
        }
    }

    private void SyncToggleStatesFromTechTree()
    {
        if (TechManager.Instance == null)
        {
            Debug.LogError("TechToggleBinder: TechManager instance not found. Ensure TechManager is initialized before syncing toggles.");
            return;
        }

        isSyncingState = true;

        foreach (var kvp in runtimeMap)
        {
            Toggle toggle = kvp.Key;
            string techId = kvp.Value;
            bool isUnlocked = TechManager.Instance.IsTechUnlocked(techId);
            toggle.isOn = isUnlocked;
            toggle.interactable = !isUnlocked; // Prevent re-locking already unlocked techs
        }

        isSyncingState = false;
    }

    private void OnToggleValueChanged(Toggle toggle, bool isOn)
    {
        if (isSyncingState)
            return;

        HandleToggle(toggle, isOn);
    }

    private void HandleToggle(Toggle toggle, bool isOn)
    {
        if (!runtimeMap.TryGetValue(toggle, out string techId))
        {
            Debug.LogWarning("TechToggleBinder: Toggle not registered in runtime map.");
            return;
        }

        if (TechManager.Instance == null)
        {
            Debug.LogError("TechToggleBinder: TechManager instance not found.");
            SyncToggleStatesFromTechTree();
            return;
        }

        if (!isOn)
        {
            // Disallow turning off once unlocked. Re-sync state to match tech tree.
            SyncToggleStatesFromTechTree();
            return;
        }

        bool unlocked = TechManager.Instance.UnlockTech(techId);

        if (!unlocked)
        {
            // Unlock failed; revert toggle state.
            SyncToggleStatesFromTechTree();
            return;
        }

        // Successful unlock: disable toggle to prevent further interaction.
        toggle.isOn = true;
        toggle.interactable = false;
    }
}
