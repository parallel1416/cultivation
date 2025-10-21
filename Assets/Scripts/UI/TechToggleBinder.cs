using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Synchronises Tower scene tech toggles with the tech tree state and visuals.
/// </summary>
public class TechToggleBinder : MonoBehaviour
{
    [Serializable]
    private struct TechToggleEntry
    {
        public Toggle toggle;
        public string techId;
    }

    private sealed class ToggleBinding
    {
        public Toggle Toggle;
        public string TechId;
        public Image BackgroundImage;
        public Material OriginalMaterial;
        public RectTransform RectTransform;
        public UnityAction<bool> Listener;
    }

    [Header("Toggle → TechID Mapping")]
    [SerializeField] private List<TechToggleEntry> toggleMappings = new List<TechToggleEntry>();

    [Header("Visual Assets")]
    [SerializeField] private Sprite lockedStateSprite; // node_unlit.png
    [SerializeField] private Sprite[] activeStateSprites; // node1.png … node6.png ordered ascending
    [SerializeField] private Material availablePulseMaterial; // Pulse.mat

    [Header("Height Mapping")]
    [SerializeField] private float heightInterval = 1000f;

    [Header("Events")]
    [SerializeField] private UnityEvent onTechUnlocked;

    private readonly Dictionary<Toggle, ToggleBinding> bindings = new Dictionary<Toggle, ToggleBinding>();
    private readonly Dictionary<Toggle, float> toggleYPositions = new Dictionary<Toggle, float>();

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
        foreach (var binding in bindings.Values)
        {
            if (binding.Toggle != null && binding.Listener != null)
            {
                binding.Toggle.onValueChanged.RemoveListener(binding.Listener);
            }
        }

        bindings.Clear();
        toggleYPositions.Clear();
    }

    private void InitializeMappings()
    {
        bindings.Clear();
        toggleYPositions.Clear();

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

            if (bindings.ContainsKey(entry.toggle))
            {
                Debug.LogWarning($"TechToggleBinder: Duplicate toggle entry detected for {entry.toggle.name}.", this);
                continue;
            }

            var toggle = entry.toggle;
            var binding = new ToggleBinding
            {
                Toggle = toggle,
                TechId = entry.techId.Trim(),
                BackgroundImage = ResolveBackgroundImage(toggle),
                RectTransform = toggle.transform as RectTransform,
            };

            if (binding.BackgroundImage != null)
            {
                binding.OriginalMaterial = binding.BackgroundImage.material;
            }

            binding.Listener = isOn => OnToggleValueChanged(toggle, isOn);

            bindings.Add(toggle, binding);
            toggle.onValueChanged.AddListener(binding.Listener);
        }
    }

    private void SyncToggleStatesFromTechTree()
    {
        if (TechManager.Instance == null)
        {
            Debug.LogError("TechToggleBinder: TechManager instance not found. Ensure TechManager is initialised before syncing toggles.");
            return;
        }

        CacheTogglePositions();

        isSyncingState = true;

        foreach (var binding in bindings.Values)
        {
            if (binding.Toggle == null)
            {
                continue;
            }

            TechNode node = TechManager.Instance.GetTechNode(binding.TechId);
            bool isUnlocked = node != null && node.isUnlocked;
            bool isAvailable = !isUnlocked && TechManager.Instance.CanUnlockTech(binding.TechId);

            ApplyState(binding, isUnlocked, isAvailable);
        }

        isSyncingState = false;
    }

    private void OnToggleValueChanged(Toggle toggle, bool isOn)
    {
        if (isSyncingState)
        {
            return;
        }

        if (!bindings.TryGetValue(toggle, out ToggleBinding binding))
        {
            Debug.LogWarning("TechToggleBinder: Toggle not registered in runtime map.");
            return;
        }

        if (!isOn)
        {
            // Disallow turning off via UI.
            SyncToggleStatesFromTechTree();
            return;
        }

        if (TechManager.Instance == null)
        {
            Debug.LogError("TechToggleBinder: TechManager instance not found.");
            SyncToggleStatesFromTechTree();
            return;
        }

        bool canUnlock = TechManager.Instance.CanUnlockTech(binding.TechId);
        if (!canUnlock)
        {
            SyncToggleStatesFromTechTree();
            return;
        }

        bool unlocked = TechManager.Instance.UnlockTech(binding.TechId);
        if (!unlocked)
        {
            SyncToggleStatesFromTechTree();
            return;
        }

        onTechUnlocked?.Invoke();

        SyncToggleStatesFromTechTree();
    }

    private void ApplyState(ToggleBinding binding, bool isUnlocked, bool isAvailable)
    {
        // Avoid triggering OnValueChanged while syncing.
        binding.Toggle.SetIsOnWithoutNotify(isUnlocked);

        if (isUnlocked)
        {
            binding.Toggle.interactable = false;
            ApplyActiveVisual(binding);
        }
        else
        {
            binding.Toggle.interactable = true;
            ApplyLockedVisual(binding, isAvailable);
        }
    }

    private void ApplyActiveVisual(ToggleBinding binding)
    {
        if (binding.BackgroundImage == null)
        {
            return;
        }

        Sprite activeSprite = GetActiveSpriteFor(binding.Toggle);
        if (activeSprite != null)
        {
            binding.BackgroundImage.sprite = activeSprite;
        }

        binding.BackgroundImage.material = binding.OriginalMaterial;
        binding.BackgroundImage.SetMaterialDirty();
    }

    private void ApplyLockedVisual(ToggleBinding binding, bool isAvailable)
    {
        if (binding.BackgroundImage == null)
        {
            return;
        }

        if (lockedStateSprite != null)
        {
            binding.BackgroundImage.sprite = lockedStateSprite;
        }

        Material targetMaterial = isAvailable && availablePulseMaterial != null
            ? availablePulseMaterial
            : binding.OriginalMaterial;

        binding.BackgroundImage.material = targetMaterial;
        binding.BackgroundImage.SetMaterialDirty();
    }

    private void CacheTogglePositions()
    {
        toggleYPositions.Clear();

        foreach (var binding in bindings.Values)
        {
            RectTransform rect = binding.RectTransform != null
                ? binding.RectTransform
                : binding.Toggle != null ? binding.Toggle.transform as RectTransform : null;

            if (rect == null)
            {
                continue;
            }

            float y = rect.anchoredPosition.y;
            toggleYPositions[binding.Toggle] = y;
        }
    }

    private Sprite GetActiveSpriteFor(Toggle toggle)
    {
        if (activeStateSprites == null || activeStateSprites.Length == 0)
        {
            return lockedStateSprite;
        }

        if (!toggleYPositions.TryGetValue(toggle, out float yPosition))
        {
            return activeStateSprites[activeStateSprites.Length - 1];
        }

        float interval = Mathf.Max(1f, heightInterval);
        float clampedHeight = Mathf.Max(0f, yPosition);
        int spriteIndex = Mathf.FloorToInt(clampedHeight / interval);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, activeStateSprites.Length - 1);
        return activeStateSprites[spriteIndex];
    }

    private static Image ResolveBackgroundImage(Toggle toggle)
    {
        if (toggle == null)
        {
            return null;
        }

        Image img = toggle.targetGraphic as Image;
        if (img != null)
        {
            return img;
        }

        if (toggle.TryGetComponent(out img))
        {
            return img;
        }

        return toggle.GetComponentInChildren<Image>();
    }
}
