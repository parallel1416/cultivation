using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Resource bar that displays money and disciple count.
/// Attach this to the ResourceBar GameObject in your scene.
/// </summary>
public class ResourceBar : MonoBehaviour
{
    private static ResourceBar _instance;
    public static ResourceBar Instance => _instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI discipleText;

    [Header("Dropdown Menu")]
    [SerializeField] private GameObject dropdownMenu;
    [SerializeField] private Toggle pplToggle;
    [SerializeField] private Toggle petToggle;
    [SerializeField] private Toggle itemToggle;
    [SerializeField] private Toggle optionToggle;
    [SerializeField] private Transform contentContainer; // Horizontal layout container
    [SerializeField] private GameObject descriptionItemPrefab; // Prefab for list items (deprecated, now using images)

    [Header("Entity Icons")]
    [Tooltip("People icons in order: zm, js, jj, yz, mr")]
    [SerializeField] private Sprite[] peopleIcons = new Sprite[5]; // zm, js, jj, yz, mr
    [Tooltip("Pet icons in order: mouse, hen, sheep")]
    [SerializeField] private Sprite[] petIcons = new Sprite[3]; // mouse, hen, sheep
    [Tooltip("Item icons (6 items)")]
    [SerializeField] private Sprite[] itemIcons = new Sprite[6];
    [SerializeField] private Color blackedOutColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Color for unowned entities

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float slideDistance = 300f; // How far to slide out
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform dropdownRect;
    private RectTransform pplToggleRect;
    private RectTransform petToggleRect;
    private RectTransform itemToggleRect;
    private RectTransform optionToggleRect;
    
    private bool isAnimating = false;
    private float animationTimer = 0f;
    private bool targetState = false;
    
    private float menuHiddenX;
    private float menuVisibleX;
    private float toggleHiddenX;
    private float toggleVisibleX;
    
    private Toggle currentActiveToggle;
    private DropdownCategory currentCategory;
    private bool isClosingMenu = false; // Flag to prevent toggle from processing click

    private enum DropdownCategory
    {
        People,
        Pet,
        Item,
        Option
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup dropdown menu and toggle rects
        if (dropdownMenu != null)
        {
            dropdownRect = dropdownMenu.GetComponent<RectTransform>();
            if (dropdownRect != null)
            {
                menuHiddenX = dropdownRect.anchoredPosition.x;
                menuVisibleX = menuHiddenX + slideDistance;
                dropdownRect.anchoredPosition = new Vector2(menuHiddenX, dropdownRect.anchoredPosition.y);
            }
            dropdownMenu.SetActive(false);
        }

        // Setup toggle RectTransforms
        if (pplToggle != null)
        {
            pplToggleRect = pplToggle.GetComponent<RectTransform>();
        }
        if (petToggle != null)
        {
            petToggleRect = petToggle.GetComponent<RectTransform>();
        }
        if (itemToggle != null)
        {
            itemToggleRect = itemToggle.GetComponent<RectTransform>();
        }
        if (optionToggle != null)
        {
            optionToggleRect = optionToggle.GetComponent<RectTransform>();
        }

        // Store toggle hidden position (assumes all toggles start at same X)
        if (pplToggleRect != null)
        {
            toggleHiddenX = pplToggleRect.anchoredPosition.x;
            toggleVisibleX = toggleHiddenX + slideDistance;
        }

        // Setup toggle listeners - just update content when toggled
        if (pplToggle != null)
        {
            pplToggle.onValueChanged.AddListener((isOn) => OnToggleClicked(pplToggle, DropdownCategory.People, isOn));
        }

        if (petToggle != null)
        {
            petToggle.onValueChanged.AddListener((isOn) => OnToggleClicked(petToggle, DropdownCategory.Pet, isOn));
        }

        if (itemToggle != null)
        {
            itemToggle.onValueChanged.AddListener((isOn) => OnToggleClicked(itemToggle, DropdownCategory.Item, isOn));
        }

        if (optionToggle != null)
        {
            optionToggle.onValueChanged.AddListener((isOn) => OnToggleClicked(optionToggle, DropdownCategory.Option, isOn));
        }
    }

    private void Start()
    {
        // Auto-find text components if not assigned
        if (moneyText == null)
        {
            moneyText = transform.Find("MoneyText")?.GetComponent<TextMeshProUGUI>();
        }

        if (discipleText == null)
        {
            discipleText = transform.Find("DiscipleText")?.GetComponent<TextMeshProUGUI>();
        }

        if (moneyText == null || discipleText == null)
        {
            Debug.LogError("ResourceBar: Failed to find MoneyText or DiscipleText components!");
        }

        UpdateDisplay();
    }

    private void Update()
    {
        // Check for clicks outside to close menu
        if (dropdownMenu != null && dropdownMenu.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI())
            {
                isClosingMenu = true;
                CloseMenu();
                // Reset flag after a frame to allow normal toggle behavior again
                StartCoroutine(ResetClosingFlag());
            }
        }

        if (isAnimating)
        {
            animationTimer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(animationTimer / animationDuration);
            float curveValue = slideCurve.Evaluate(progress);

            // Animate menu
            if (dropdownRect != null)
            {
                Vector2 menuPos = dropdownRect.anchoredPosition;
                if (targetState) // Sliding out (show)
                {
                    menuPos.x = Mathf.Lerp(menuHiddenX, menuVisibleX, curveValue);
                }
                else // Sliding in (hide)
                {
                    menuPos.x = Mathf.Lerp(menuVisibleX, menuHiddenX, curveValue);
                    if (progress < 0.1f) // Log at start of animation
                    {
                        Debug.Log($"[ResourceBar Update] Sliding IN - progress: {progress:F3}, lerping from {menuVisibleX} to {menuHiddenX}, current X: {menuPos.x:F2}");
                    }
                }
                dropdownRect.anchoredPosition = menuPos;
            }

            // Animate active toggle
            if (currentActiveToggle != null)
            {
                RectTransform toggleRect = currentActiveToggle.GetComponent<RectTransform>();
                if (toggleRect != null)
                {
                    Vector2 togglePos = toggleRect.anchoredPosition;
                    if (targetState) // Sliding out
                    {
                        togglePos.x = Mathf.Lerp(toggleHiddenX, toggleVisibleX, curveValue);
                    }
                    else // Sliding in
                    {
                        togglePos.x = Mathf.Lerp(toggleVisibleX, toggleHiddenX, curveValue);
                    }
                    toggleRect.anchoredPosition = togglePos;
                }
            }

            // Animation complete
            if (progress >= 1f)
            {
                isAnimating = false;
                if (!targetState)
                {
                    Debug.Log($"[ResourceBar] Slide-IN animation complete. Final menu X: {dropdownRect.anchoredPosition.x}");
                    dropdownMenu.SetActive(false);
                    currentActiveToggle = null;
                }
            }
        }
    }

    /// <summary>
    /// Update the resource display with current values from LevelManager
    /// </summary>
    public void UpdateDisplay()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("ResourceBar: LevelManager instance not found.");
            return;
        }

        if (moneyText != null)
        {
            moneyText.text = $"{LevelManager.Instance.Money}";
        }

        if (discipleText != null)
        {
            discipleText.text = LevelManager.Instance.Disciples.ToString();
        }
    }

    /// <summary>
    /// Refresh the dropdown content if it's currently open
    /// Call this when items/disciples/pets change
    /// </summary>
    public void RefreshDropdown()
    {
        if (dropdownMenu == null || !dropdownMenu.activeSelf)
        {
            return; // Dropdown not open, no need to refresh
        }

        // Determine which category is currently selected and refresh it
        if (pplToggle != null && pplToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.People);
        }
        else if (petToggle != null && petToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.Pet);
        }
        else if (itemToggle != null && itemToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.Item);
        }
        else if (optionToggle != null && optionToggle.isOn)
        {
            UpdateDropdownContent(DropdownCategory.Option);
        }
    }

    /// <summary>
    /// Reset the closing menu flag after a frame
    /// </summary>
    private System.Collections.IEnumerator ResetClosingFlag()
    {
        yield return null; // Wait one frame
        isClosingMenu = false;
    }

    /// <summary>
    /// Check if pointer is over any of our UI elements (menu or toggles)
    /// </summary>
    private bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Check if clicking on menu or any toggle or their children
            if (result.gameObject == dropdownMenu || 
                (pplToggle != null && (result.gameObject == pplToggle.gameObject || IsChildOf(result.gameObject.transform, pplToggle.transform))) ||
                (petToggle != null && (result.gameObject == petToggle.gameObject || IsChildOf(result.gameObject.transform, petToggle.transform))) ||
                (itemToggle != null && (result.gameObject == itemToggle.gameObject || IsChildOf(result.gameObject.transform, itemToggle.transform))) ||
                (optionToggle != null && (result.gameObject == optionToggle.gameObject || IsChildOf(result.gameObject.transform, optionToggle.transform))) ||
                IsChildOf(result.gameObject.transform, dropdownMenu.transform))
            {
                Debug.Log($"[ResourceBar] Click detected on UI element: {result.gameObject.name}");
                return true;
            }
        }

        Debug.Log("[ResourceBar] Click detected OUTSIDE menu/toggles - will close menu");
        return false;
    }

    /// <summary>
    /// Check if a transform is a child of a parent transform
    /// </summary>
    private bool IsChildOf(Transform child, Transform parent)
    {
        Transform current = child;
        while (current != null)
        {
            if (current == parent)
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    /// <summary>
    /// Close the menu and slide everything back
    /// </summary>
    private void CloseMenu()
    {
        if (currentActiveToggle != null)
        {
            Debug.Log("ResourceBar: Closing menu due to outside click.");
            // Manually trigger slide-in without going through toggle
            targetState = false;
            isAnimating = true;
            animationTimer = 0f;
            
            // Ensure we're starting from the visible position
            if (dropdownRect != null)
            {
                dropdownRect.anchoredPosition = new Vector2(menuVisibleX, dropdownRect.anchoredPosition.y);
                Debug.Log($"[ResourceBar] Menu position set to visible: {menuVisibleX}");
            }
            
            RectTransform toggleRect = currentActiveToggle.GetComponent<RectTransform>();
            if (toggleRect != null)
            {
                toggleRect.anchoredPosition = new Vector2(toggleVisibleX, toggleRect.anchoredPosition.y);
                Debug.Log($"[ResourceBar] Toggle position set to visible: {toggleVisibleX}");
            }
            
            Debug.Log($"[ResourceBar] Animation will lerp from visible ({menuVisibleX}) to hidden ({menuHiddenX})");
            
            // Turn off toggle AFTER starting animation to avoid event
            currentActiveToggle.isOn = false;
        }
    }

    /// <summary>
    /// Handle toggle state changes - slide out when toggled on, slide in when toggled off
    /// </summary>
    private void OnToggleClicked(Toggle toggle, DropdownCategory category, bool isOn)
    {
        Debug.Log($"[ResourceBar] OnToggleClicked - Toggle: {toggle.name}, Category: {category}, isOn: {isOn}, isClosingMenu: {isClosingMenu}");
        
        if (dropdownMenu == null || dropdownRect == null) return;

        // If we're closing the menu, ignore ALL toggle events (both on and off)
        if (isClosingMenu)
        {
            Debug.Log("[ResourceBar] Ignoring toggle event during menu close");
            return;
        }

        if (isOn)
        {
            // If a different toggle was active, slide it back first
            if (currentActiveToggle != null && currentActiveToggle != toggle)
            {
                RectTransform oldToggleRect = currentActiveToggle.GetComponent<RectTransform>();
                if (oldToggleRect != null)
                {
                    oldToggleRect.anchoredPosition = new Vector2(toggleHiddenX, oldToggleRect.anchoredPosition.y);
                }
            }

            // Show menu and update content
            dropdownMenu.SetActive(true);
            currentActiveToggle = toggle;
            currentCategory = category;
            UpdateDropdownContent(category);

            // Start slide-out animation
            targetState = true;
            isAnimating = true;
            animationTimer = 0f;

            // Reset positions before animating
            dropdownRect.anchoredPosition = new Vector2(menuHiddenX, dropdownRect.anchoredPosition.y);
            RectTransform toggleRect = toggle.GetComponent<RectTransform>();
            if (toggleRect != null)
            {
                toggleRect.anchoredPosition = new Vector2(toggleHiddenX, toggleRect.anchoredPosition.y);
            }
        }
        else
        {
            // Toggle turned off - slide back in
            if (currentActiveToggle == toggle)
            {
                Debug.Log($"[ResourceBar] Slide-IN triggered - toggle turned off");
                Debug.Log($"[ResourceBar] Current positions before reset - Menu X: {dropdownRect.anchoredPosition.x}, Toggle X: {toggle.GetComponent<RectTransform>()?.anchoredPosition.x}");
                
                targetState = false;
                isAnimating = true;
                animationTimer = 0f;
                
                // Ensure we're starting from the visible position
                if (dropdownRect != null)
                {
                    dropdownRect.anchoredPosition = new Vector2(menuVisibleX, dropdownRect.anchoredPosition.y);
                    Debug.Log($"[ResourceBar] Menu position set to visible: {menuVisibleX}");
                }
                
                RectTransform toggleRect = toggle.GetComponent<RectTransform>();
                if (toggleRect != null)
                {
                    toggleRect.anchoredPosition = new Vector2(toggleVisibleX, toggleRect.anchoredPosition.y);
                    Debug.Log($"[ResourceBar] Toggle position set to visible: {toggleVisibleX}");
                }
                
                Debug.Log($"[ResourceBar] Animation will lerp from visible ({menuVisibleX}) to hidden ({menuHiddenX})");
                Debug.Log($"[ResourceBar] targetState: {targetState}, isAnimating: {isAnimating}");
            }
        }
    }

    /// <summary>
    /// Update dropdown content based on selected category
    /// </summary>
    private void UpdateDropdownContent(DropdownCategory category)
    {
        if (contentContainer == null)
        {
            Debug.LogError("ResourceBar: Content container not assigned!");
            return;
        }

        // Clear existing content
        ClearContent();

        // Add images based on category
        switch (category)
        {
            case DropdownCategory.People:
                CreatePeopleIcons();
                break;
            case DropdownCategory.Pet:
                CreatePetIcons();
                break;
            case DropdownCategory.Item:
                CreateItemIcons();
                break;
            case DropdownCategory.Option:
                // Options still use text for now
                CreateOptionText();
                break;
        }
    }

    /// <summary>
    /// Create people icons: zm, js, jj, yz, mr
    /// </summary>
    private void CreatePeopleIcons()
    {
        if (LevelManager.Instance == null || peopleIcons.Length < 5)
        {
            Debug.LogError("ResourceBar: LevelManager not found or people icons not assigned!");
            return;
        }

        // Order: zm (normal disciples), js (jingshi), jj (jianjun), yz (yuezheng), mr (disciples again/placeholder)
        // Check status: zm = ActiveDisciples > 0, js = StatusJingshi >= 0, jj = StatusJianjun >= 0, yz = StatusYuezheng >= 0
        bool[] owned = new bool[5];
        owned[0] = LevelManager.Instance.ActiveDisciples > 0; // zm (normal disciples)
        owned[1] = LevelManager.Instance.StatusJingshi >= 0; // js (jingshi)
        owned[2] = LevelManager.Instance.StatusJianjun >= 0; // jj (jianjun)
        owned[3] = LevelManager.Instance.StatusYuezheng >= 0; // yz (yuezheng)
        owned[4] = LevelManager.Instance.ActiveDisciples > 0; // mr (use disciples as placeholder)

        for (int i = 0; i < peopleIcons.Length; i++)
        {
            CreateIconImage(peopleIcons[i], owned[i]);
        }
    }

    /// <summary>
    /// Create pet icons: mouse, hen, sheep
    /// </summary>
    private void CreatePetIcons()
    {
        if (LevelManager.Instance == null || petIcons.Length < 3)
        {
            Debug.LogError("ResourceBar: LevelManager not found or pet icons not assigned!");
            return;
        }

        bool[] owned = new bool[3];
        owned[0] = LevelManager.Instance.StatusMouse >= 0; // mouse
        owned[1] = LevelManager.Instance.StatusChicken >= 0; // hen/chicken
        owned[2] = LevelManager.Instance.StatusSheep >= 0; // sheep

        for (int i = 0; i < petIcons.Length; i++)
        {
            CreateIconImage(petIcons[i], owned[i]);
        }
    }

    /// <summary>
    /// Create item icons (6 items)
    /// </summary>
    private void CreateItemIcons()
    {
        if (ItemManager.Instance == null || itemIcons.Length < 6)
        {
            Debug.LogError("ResourceBar: ItemManager not found or item icons not assigned!");
            return;
        }

        // Item order should match itemIcons array order
        string[] itemIds = new string[] 
        { 
            "zhi_kui_lei", 
            "yu_chan_tui", 
            "dian_fan_tie", 
            "wu_que_jing", 
            "cheng_fu_fu",
            "" // 6th item placeholder if needed
        };

        for (int i = 0; i < itemIcons.Length; i++)
        {
            bool owned = false;
            if (i < itemIds.Length && !string.IsNullOrEmpty(itemIds[i]))
            {
                owned = ItemManager.Instance.GetItemQuantity(itemIds[i]) > 0;
            }
            CreateIconImage(itemIcons[i], owned);
        }
    }

    /// <summary>
    /// Create an icon image in the content container
    /// </summary>
    private void CreateIconImage(Sprite icon, bool isOwned)
    {
        if (icon == null)
        {
            Debug.LogWarning("ResourceBar: Icon sprite is null!");
            return;
        }

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(contentContainer, false);

        Image img = iconObj.AddComponent<Image>();
        img.sprite = icon;
        img.preserveAspect = true;

        // Set color based on ownership
        img.color = isOwned ? Color.white : blackedOutColor;

        // Set size (adjust as needed)
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100); // Adjust icon size as needed
    }

    /// <summary>
    /// Create option text (legacy text-based display for options)
    /// </summary>
    private void CreateOptionText()
    {
        if (LevelManager.Instance == null)
        {
            CreateDescriptionItem("LevelManager not found");
            return;
        }

        CreateDescriptionItem("Options coming soon");
        CreateDescriptionItem($"Current Turn: {TurnManager.Instance?.CurrentTurn ?? 0}");
        CreateDescriptionItem($"Money: {LevelManager.Instance.Money}");
        CreateDescriptionItem($"Disciples: {LevelManager.Instance.Disciples}");
    }

    /// <summary>
    /// Create a description item in the content container (for text-based items like Options)
    /// </summary>
    private void CreateDescriptionItem(string description)
    {
        GameObject item;

        if (descriptionItemPrefab != null)
        {
            // Use prefab if provided
            item = Instantiate(descriptionItemPrefab, contentContainer);
        }
        else
        {
            // Create simple text item if no prefab
            item = new GameObject("DescriptionItem");
            item.transform.SetParent(contentContainer, false);
            
            TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
        }

        // Set text content
        TextMeshProUGUI textComponent = item.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = description;
        }
        else
        {
            // Try to find text in children
            textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = description;
            }
        }
    }

    /// <summary>
    /// Clear all content items from the container
    /// </summary>
    private void ClearContent()
    {
        if (contentContainer == null) return;

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners (no dropdown button anymore)
    }
}