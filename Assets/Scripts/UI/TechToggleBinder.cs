using System;
using System.Collections;
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
        public ColorBlock OriginalColors;
        public TMPro.TextMeshProUGUI TextComponent; // For updating tech name
    }

    [Header("Toggle → TechID Mapping")]
    [SerializeField] private List<TechToggleEntry> toggleMappings = new List<TechToggleEntry>();

    [Header("Visual Assets")]
    [SerializeField] private Sprite lockedStateSprite; // node_unlit.png
    [SerializeField] private Sprite[] activeStateSprites; // node1.png … node6.png ordered ascending
    [SerializeField] private Material availablePulseMaterial; // Pulse.mat

    [Header("Height Mapping")]
    [SerializeField] private float heightInterval = 1000f;

    [Header("Description Panel")]
    [SerializeField] private GameObject descriptionPanelRoot;
    [SerializeField] private RectTransform descriptionPanelRect;
    [SerializeField] private Image descriptionPanelImage; // Background image for sprite switching
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] private float descPanelWidth = 300f;
    [SerializeField] private float descPanelMargin = 20f;
    [SerializeField] private float hoverDelay = 0.2f;
    [SerializeField] private float descPanelYThreshold = 0f; // Y position threshold
    [SerializeField] private Sprite descPanelSpriteAbove; // Image when Y > threshold
    [SerializeField] private Sprite descPanelSpriteBelow; // Image when Y <= threshold

    [Header("Events")]
    [SerializeField] private UnityEvent onTechUnlocked;

    private readonly Dictionary<Toggle, ToggleBinding> bindings = new Dictionary<Toggle, ToggleBinding>();
    private readonly Dictionary<Toggle, float> toggleYPositions = new Dictionary<Toggle, float>();

    private bool isSyncingState;
    private Canvas canvas;
    private CanvasGroup descPanelCanvasGroup;
    private Image descPanelBackgroundImage;
    private Toggle currentHoveredToggle;
    private Coroutine hoverDelayCoroutine;

    private void Awake()
    {
        // Find canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        // Setup description panel
        if (descriptionPanelRoot != null)
        {
            if (descriptionPanelRect == null)
            {
                descriptionPanelRect = descriptionPanelRoot.GetComponent<RectTransform>();
            }

            descPanelCanvasGroup = descriptionPanelRoot.GetComponent<CanvasGroup>();
            if (descPanelCanvasGroup == null)
            {
                descPanelCanvasGroup = descriptionPanelRoot.AddComponent<CanvasGroup>();
            }

            // Get background image component - use serialized field if assigned, otherwise auto-find
            if (descriptionPanelImage != null)
            {
                descPanelBackgroundImage = descriptionPanelImage;
            }
            else
            {
                descPanelBackgroundImage = descriptionPanelRoot.GetComponent<Image>();
                if (descPanelBackgroundImage == null)
                {
                    Debug.LogWarning("TechToggleBinder: No Image component assigned or found on descriptionPanelRoot. Assign 'Description Panel Image' field or add an Image component to enable sprite switching.");
                }
            }

            descriptionPanelRoot.SetActive(false);
            descPanelCanvasGroup.alpha = 0f;
        }

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

        if (hoverDelayCoroutine != null)
        {
            StopCoroutine(hoverDelayCoroutine);
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
                OriginalColors = toggle.colors,
                TextComponent = ResolveTextComponent(toggle),
            };

            if (binding.BackgroundImage != null)
            {
                binding.OriginalMaterial = binding.BackgroundImage.material;
            }

            // Update toggle text to tech name
            UpdateToggleText(binding);

            binding.Listener = isOn => OnToggleValueChanged(toggle, isOn);

            bindings.Add(toggle, binding);
            toggle.onValueChanged.AddListener(binding.Listener);

            // Add hover event triggers
            AddHoverListeners(toggle);
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
            bool unlockState = node != null && node.unlockState == 1;
            bool isAvailable = !unlockState && TechManager.Instance.CanUnlockTech(binding.TechId);

            ApplyState(binding, unlockState, isAvailable);
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
            Debug.LogWarning($"TechToggleBinder: Toggle {toggle.name} not registered in runtime map.");
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

        ResourceBar.Instance?.UpdateDisplay();
        onTechUnlocked?.Invoke();

        SyncToggleStatesFromTechTree();
    }

    private void ApplyState(ToggleBinding binding, bool unlockState, bool isAvailable)
    {
        // Avoid triggering OnValueChanged while syncing.
        binding.Toggle.SetIsOnWithoutNotify(unlockState);

        if (unlockState)
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

        ApplyDisabledColor(binding.Toggle, Color.white);
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

        binding.Toggle.colors = binding.OriginalColors;
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

    private static void ApplyDisabledColor(Toggle toggle, Color targetColor)
    {
        if (toggle == null)
        {
            return;
        }

        ColorBlock colors = toggle.colors;
        colors.normalColor = targetColor;
        colors.highlightedColor = targetColor;
        colors.pressedColor = targetColor;
        colors.selectedColor = targetColor;
        colors.disabledColor = targetColor;
        toggle.colors = colors;

        if (toggle.targetGraphic != null)
        {
            toggle.targetGraphic.color = targetColor;
        }
    }

    private static TMPro.TextMeshProUGUI ResolveTextComponent(Toggle toggle)
    {
        if (toggle == null)
        {
            return null;
        }

        // Try to find TextMeshProUGUI component in children
        TMPro.TextMeshProUGUI tmp = toggle.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            return tmp;
        }

        // Fallback to legacy Text component (convert if needed)
        UnityEngine.UI.Text legacyText = toggle.GetComponentInChildren<UnityEngine.UI.Text>();
        if (legacyText != null)
        {
            Debug.LogWarning($"TechToggleBinder: Toggle '{toggle.name}' uses legacy Text component. Consider upgrading to TextMeshProUGUI.");
        }

        return null;
    }

    private void UpdateToggleText(ToggleBinding binding)
    {
        if (binding == null || binding.TextComponent == null)
        {
            return;
        }

        if (TechManager.Instance == null)
        {
            Debug.LogWarning($"TechToggleBinder: TechManager not available when updating text for toggle '{binding.Toggle?.name}'");
            return;
        }

        TechNode node = TechManager.Instance.GetTechNode(binding.TechId);
        if (node == null)
        {
            Debug.LogWarning($"TechToggleBinder: Tech node not found for ID '{binding.TechId}' on toggle '{binding.Toggle?.name}'");
            return;
        }

        // Update the text to the tech name
        string techName = !string.IsNullOrEmpty(node.name) ? node.name : binding.TechId;
        binding.TextComponent.text = techName;
    }

    private void AddHoverListeners(Toggle toggle)
    {
        if (toggle == null)
        {
            return;
        }

        // Add EventTrigger component if it doesn't exist
        UnityEngine.EventSystems.EventTrigger eventTrigger = toggle.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = toggle.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        // Add PointerEnter event
        UnityEngine.EventSystems.EventTrigger.Entry pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((data) => { OnTogglePointerEnter(toggle); });
        eventTrigger.triggers.Add(pointerEnter);

        // Add PointerExit event
        UnityEngine.EventSystems.EventTrigger.Entry pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((data) => { OnTogglePointerExit(toggle); });
        eventTrigger.triggers.Add(pointerExit);
    }

    private void OnTogglePointerEnter(Toggle toggle)
    {
        if (toggle == null || descriptionPanelRoot == null)
        {
            return;
        }

        currentHoveredToggle = toggle;

        // Start hover delay coroutine
        if (hoverDelayCoroutine != null)
        {
            StopCoroutine(hoverDelayCoroutine);
        }
        hoverDelayCoroutine = StartCoroutine(ShowDescriptionPanelAfterDelay(toggle));
    }

    private void OnTogglePointerExit(Toggle toggle)
    {
        if (currentHoveredToggle == toggle)
        {
            currentHoveredToggle = null;
        }

        // Cancel hover delay
        if (hoverDelayCoroutine != null)
        {
            StopCoroutine(hoverDelayCoroutine);
            hoverDelayCoroutine = null;
        }

        HideDescriptionPanel();
    }

    private IEnumerator ShowDescriptionPanelAfterDelay(Toggle toggle)
    {
        yield return new WaitForSeconds(hoverDelay);

        // Check if still hovering
        if (currentHoveredToggle == toggle)
        {
            ShowDescriptionPanel(toggle);
        }
    }

    private void ShowDescriptionPanel(Toggle toggle)
    {
        if (toggle == null || descriptionPanelRoot == null || !bindings.TryGetValue(toggle, out ToggleBinding binding))
        {
            return;
        }

        // Get tech node data
        if (TechManager.Instance == null)
        {
            return;
        }

        TechNode node = TechManager.Instance.GetTechNode(binding.TechId);
        if (node == null)
        {
            return;
        }

        // Update description text
        if (descriptionText != null)
        {
            string description = !string.IsNullOrEmpty(node.description) ? node.description : "无描述 (No description)";
            
            // Add tech info
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>{node.name}</b>");
            sb.AppendLine(description);
            sb.AppendLine($"<color=#FFD700>消耗: {node.cost} 灵石</color>");
            
            // Show prerequisites
            if (node.prerequisites != null && node.prerequisites.Count > 0)
            {
                sb.Append("<color=#888888>前置: ");
                for (int i = 0; i < node.prerequisites.Count; i++)
                {
                    string prereqId = node.prerequisites[i];
                    TechNode prereqNode = TechManager.Instance.GetTechNode(prereqId);
                    string prereqName = prereqNode != null ? prereqNode.name : prereqId;
                    sb.Append(prereqName);
                    if (i < node.prerequisites.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append("</color>");
            }
            
            descriptionText.text = sb.ToString();
        }

        // Position panel
        PositionDescriptionPanel(toggle.GetComponent<RectTransform>());

        // Show panel
        descriptionPanelRoot.SetActive(true);
        if (descPanelCanvasGroup != null)
        {
            descPanelCanvasGroup.alpha = 1f;
        }
    }

    private void HideDescriptionPanel()
    {
        if (descriptionPanelRoot != null)
        {
            descriptionPanelRoot.SetActive(false);
            if (descPanelCanvasGroup != null)
            {
                descPanelCanvasGroup.alpha = 0f;
            }
        }
    }

    private void PositionDescriptionPanel(RectTransform toggleRect)
    {
        if (descriptionPanelRect == null || canvas == null || toggleRect == null)
        {
            return;
        }

        Vector2 toggleAnchoredPos = toggleRect.anchoredPosition;
        bool isToggleOnLeft = toggleAnchoredPos.x < 0;

        descriptionPanelRect.pivot = new Vector2(isToggleOnLeft ? 0f : 1f, 0.5f);
        descriptionPanelRect.anchorMin = new Vector2(0.5f, 0f);
        descriptionPanelRect.anchorMax = new Vector2(0.5f, 0f);

        // Use fixed panel width
        descriptionPanelRect.sizeDelta = new Vector2(descPanelWidth, descriptionPanelRect.sizeDelta.y);

        float toggleHalfWidth = toggleRect.rect.width * 0.5f;
        float xOffset = isToggleOnLeft 
            ? toggleHalfWidth + descPanelMargin
            : -(toggleHalfWidth + descPanelMargin);

        descriptionPanelRect.anchoredPosition = new Vector2(toggleAnchoredPos.x + xOffset, toggleAnchoredPos.y);

        // Update sprite based on Y position threshold
        UpdateDescriptionPanelSprite(toggleAnchoredPos.y);
    }

    private void UpdateDescriptionPanelSprite(float yPosition)
    {
        if (descPanelBackgroundImage == null)
        {
            return;
        }

        // Check if both sprites are assigned
        if (descPanelSpriteAbove == null && descPanelSpriteBelow == null)
        {
            return; // No sprites assigned, skip
        }

        // Update sprite based on threshold
        if (yPosition > descPanelYThreshold)
        {
            // Above threshold - use descPanelSpriteAbove
            if (descPanelSpriteAbove != null)
            {
                descPanelBackgroundImage.sprite = descPanelSpriteAbove;
            }
        }
        else
        {
            // Below or equal to threshold - use descPanelSpriteBelow
            if (descPanelSpriteBelow != null)
            {
                descPanelBackgroundImage.sprite = descPanelSpriteBelow;
            }
        }
    }

    /// <summary>
    /// Debug raycast target settings for a toggle
    /// </summary>
    private void DebugRaycastTarget(Toggle toggle)
    {
        if (toggle == null) return;

        Image targetImage = toggle.targetGraphic as Image;
        if (targetImage != null)
        {
            Debug.Log($"TechToggleBinder: Raycast debug for {toggle.name}:\n" +
                      $"  - Image.raycastTarget: {targetImage.raycastTarget}\n" +
                      $"  - Image.enabled: {targetImage.enabled}\n" +
                      $"  - GameObject.activeInHierarchy: {targetImage.gameObject.activeInHierarchy}\n" +
                      $"  - Image.color.a: {targetImage.color.a}\n" +
                      $"  - Toggle.interactable: {toggle.interactable}\n" +
                      $"  - Toggle.enabled: {toggle.enabled}");
        }
        else
        {
            Debug.LogWarning($"TechToggleBinder: No Image target graphic found for {toggle.name}");
        }

        // Check for blocking UI elements
        Canvas parentCanvas = toggle.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Debug.Log($"TechToggleBinder: Toggle {toggle.name} is under Canvas: {parentCanvas.name}, sortingOrder: {parentCanvas.sortingOrder}");
        }

        // Check for CanvasGroup blocking
        CanvasGroup[] canvasGroups = toggle.GetComponentsInParent<CanvasGroup>();
        foreach (var cg in canvasGroups)
        {
            if (!cg.interactable || cg.blocksRaycasts == false)
            {
                Debug.LogWarning($"TechToggleBinder: CanvasGroup '{cg.name}' is blocking interaction - interactable: {cg.interactable}, blocksRaycasts: {cg.blocksRaycasts}");
            }
        }
    }
}
