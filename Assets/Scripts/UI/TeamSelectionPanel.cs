using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages team selection panel with pet and item slots.
/// New simplified version: AssignButton -> TeamPanel -> PetSlot/ItemSlot -> Previews with toggles
/// </summary>
public class TeamSelectionPanel : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject teamPanelRoot;
    [SerializeField] private Button closeButton; // Abandons selection
    [SerializeField] private Button continueButton; // Saves selection
    
    // Current event context
    private Button currentAssignButton; // Dynamically assigned per event
    private string currentEventId;
    private int currentDiceLimit;

    [Header("Slots")]
    [SerializeField] private Button petSlot;
    [SerializeField] private Button itemSlot;

    [Header("People Preview")]
    [SerializeField] private GameObject pplPreviewRoot;
    [SerializeField] private Transform pplArea; // Container for people buttons
    [SerializeField] private Transform pplContainer; // Container for assigned people sprites
    [SerializeField] private Button[] normalPplButtons; // Dynamically sized based on normal disciple count
    [SerializeField] private Button jingshiButton;
    [SerializeField] private Button jianjunButton;
    [SerializeField] private Button yuezhengButton;

    [Header("Pet Preview")]
    [SerializeField] private GameObject petPreviewRoot;
    [SerializeField] private Toggle[] petToggles = new Toggle[3]; // Fixed 3 pets

    [Header("Item Preview")]
    [SerializeField] private GameObject itemPreviewRoot;
    [SerializeField] private Toggle[] itemToggles = new Toggle[5]; // Fixed 5 items
    
    // Auto-retrieved image components and sprites
    private Image petSlotImage;
    private Image itemSlotImage;
    private Image[] petToggleImages = new Image[3];
    private Image[] itemToggleImages = new Image[5];
    private Sprite[] petSprites = new Sprite[3]; // Auto-retrieved from toggles
    private Sprite[] itemSprites = new Sprite[5]; // Auto-retrieved from toggles
    
    // People system
    private List<Image> normalPplImages = new List<Image>();
    private List<Sprite> normalPplSprites = new List<Sprite>();
    private Image jingshiImage;
    private Image jianjunImage;
    private Image yuezhengImage;
    private Sprite jingshiSprite;
    private Sprite jianjunSprite;
    private Sprite yuezhengSprite;
    private List<string> assignedPeople = new List<string>(); // "normal_0", "jingshi", etc.
    private Color grayedOutColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Default Sprites")]
    [SerializeField] private Sprite emptyPetSlotSprite;
    [SerializeField] private Sprite emptyItemSlotSprite;

    // Selection state
    private int selectedPetIndex = -1; // -1 means no selection
    private int selectedItemIndex = -1;

    // Temp state (before confirm)
    private int tempPetIndex = -1;
    private int tempItemIndex = -1;

    private void Awake()
    {
        // Auto-retrieve image components from slots
        if (petSlot != null)
        {
            petSlotImage = petSlot.GetComponent<Image>();
            petSlot.onClick.AddListener(OnPetSlotClicked);
        }
        
        if (itemSlot != null)
        {
            itemSlotImage = itemSlot.GetComponent<Image>();
            itemSlot.onClick.AddListener(OnItemSlotClicked);
        }

        // Auto-retrieve image components from toggles
        for (int i = 0; i < petToggles.Length; i++)
        {
            if (petToggles[i] != null)
            {
                petToggleImages[i] = petToggles[i].GetComponent<Image>();
                // Get sprite from the image component
                if (petToggleImages[i] != null)
                {
                    petSprites[i] = petToggleImages[i].sprite;
                }
            }
        }
        
        for (int i = 0; i < itemToggles.Length; i++)
        {
            if (itemToggles[i] != null)
            {
                itemToggleImages[i] = itemToggles[i].GetComponent<Image>();
                // Get sprite from the image component
                if (itemToggleImages[i] != null)
                {
                    itemSprites[i] = itemToggleImages[i].sprite;
                }
            }
        }

        // Auto-retrieve people button images and sprites
        if (normalPplButtons != null)
        {
            for (int i = 0; i < normalPplButtons.Length; i++)
            {
                if (normalPplButtons[i] != null)
                {
                    Image img = normalPplButtons[i].GetComponent<Image>();
                    normalPplImages.Add(img);
                    normalPplSprites.Add(img != null ? img.sprite : null);
                    
                    int index = i;
                    normalPplButtons[i].onClick.AddListener(() => OnNormalPersonClicked(index));
                }
            }
        }
        
        if (jingshiButton != null)
        {
            jingshiImage = jingshiButton.GetComponent<Image>();
            jingshiSprite = jingshiImage != null ? jingshiImage.sprite : null;
            jingshiButton.onClick.AddListener(() => OnSpecialPersonClicked("jingshi"));
        }
        
        if (jianjunButton != null)
        {
            jianjunImage = jianjunButton.GetComponent<Image>();
            jianjunSprite = jianjunImage != null ? jianjunImage.sprite : null;
            jianjunButton.onClick.AddListener(() => OnSpecialPersonClicked("jianjun"));
        }
        
        if (yuezhengButton != null)
        {
            yuezhengImage = yuezhengButton.GetComponent<Image>();
            yuezhengSprite = yuezhengImage != null ? yuezhengImage.sprite : null;
            yuezhengButton.onClick.AddListener(() => OnSpecialPersonClicked("yuezheng"));
        }

        // Setup button listeners (assign button handled dynamically)
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);

        // Setup toggle listeners (ToggleGroup in scene handles mutual exclusivity)
        SetupToggleListeners(petToggles, OnPetToggleChanged);
        SetupToggleListeners(itemToggles, OnItemToggleChanged);

        // Initially hide everything
        if (teamPanelRoot != null)
            teamPanelRoot.SetActive(false);
        
        if (pplPreviewRoot != null)
            pplPreviewRoot.SetActive(false);
        
        if (petPreviewRoot != null)
            petPreviewRoot.SetActive(false);
        
        if (itemPreviewRoot != null)
            itemPreviewRoot.SetActive(false);
    }

    private void SetupToggleListeners(Toggle[] toggles, UnityEngine.Events.UnityAction<bool> callback)
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i] == null) continue;

            int index = i; // Capture for closure
            toggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    callback?.Invoke(isOn);
                }
            });
        }
    }

    #region Public Methods

    /// <summary>
    /// Called by EventPanelManager to set up this panel for a specific event
    /// </summary>
    public void SetCurrentEvent(string eventId, int diceLimit, Button assignButton)
    {
        currentEventId = eventId;
        currentDiceLimit = diceLimit;
        
        // Remove old listener if any
        if (currentAssignButton != null)
        {
            currentAssignButton.onClick.RemoveListener(OnAssignButtonClicked);
        }
        
        // Set new assign button
        currentAssignButton = assignButton;
        if (currentAssignButton != null)
        {
            currentAssignButton.onClick.AddListener(OnAssignButtonClicked);
        }
        
        Debug.Log($"TeamSelectionPanel: Configured for event '{eventId}' with diceLimit {diceLimit}");
    }

    #endregion

    #region Button Callbacks

    private void OnAssignButtonClicked()
    {
        // Open team panel
        if (teamPanelRoot != null)
            teamPanelRoot.SetActive(true);

        // Reset temp state to current selection
        tempPetIndex = selectedPetIndex;
        tempItemIndex = selectedItemIndex;

        // Clear assigned people
        assignedPeople.Clear();
        ClearPplContainer();

        // Update people area visibility
        UpdatePeopleArea();

        // Update slot images
        UpdateSlotDisplay();

        Debug.Log("TeamSelectionPanel: Team panel opened");
    }

    private void OnCloseButtonClicked()
    {
        // Abandon selection, close panel
        if (teamPanelRoot != null)
            teamPanelRoot.SetActive(false);

        if (pplPreviewRoot != null)
            pplPreviewRoot.SetActive(false);

        if (petPreviewRoot != null)
            petPreviewRoot.SetActive(false);

        if (itemPreviewRoot != null)
            itemPreviewRoot.SetActive(false);

        // Clear assigned people
        assignedPeople.Clear();
        ClearPplContainer();

        Debug.Log("TeamSelectionPanel: Selection abandoned");
    }

    private void OnContinueButtonClicked()
    {
        // Validate: Check if diceLimit is fulfilled
        if (assignedPeople.Count != currentDiceLimit)
        {
            Debug.LogWarning($"TeamSelectionPanel: Cannot continue - need {currentDiceLimit} people, have {assignedPeople.Count}");
            return;
        }

        // Save selection
        selectedPetIndex = tempPetIndex;
        selectedItemIndex = tempItemIndex;

        // Close panel
        if (teamPanelRoot != null)
            teamPanelRoot.SetActive(false);

        if (pplPreviewRoot != null)
            pplPreviewRoot.SetActive(false);

        if (petPreviewRoot != null)
            petPreviewRoot.SetActive(false);

        if (itemPreviewRoot != null)
            itemPreviewRoot.SetActive(false);

        Debug.Log($"TeamSelectionPanel: Selection saved - Pet: {selectedPetIndex}, Item: {selectedItemIndex}, People: {assignedPeople.Count}");
    }

    private void OnPetSlotClicked()
    {
        // Enable pet preview
        if (petPreviewRoot != null)
            petPreviewRoot.SetActive(true);

        // Hide item preview
        if (itemPreviewRoot != null)
            itemPreviewRoot.SetActive(false);

        // Hide people preview
        if (pplPreviewRoot != null)
            pplPreviewRoot.SetActive(false);

        // Update toggles based on ownership
        UpdatePetToggles();

        Debug.Log("TeamSelectionPanel: Pet preview opened");
    }

    private void OnItemSlotClicked()
    {
        // Enable item preview
        if (itemPreviewRoot != null)
            itemPreviewRoot.SetActive(true);

        // Hide pet preview
        if (petPreviewRoot != null)
            petPreviewRoot.SetActive(false);

        // Hide people preview
        if (pplPreviewRoot != null)
            pplPreviewRoot.SetActive(false);

        // Update toggles based on ownership
        UpdateItemToggles();

        Debug.Log("TeamSelectionPanel: Item preview opened");
    }

    #endregion

    #region People Callbacks

    private void OnNormalPersonClicked(int index)
    {
        string personId = $"normal_{index}";
        
        // Check if already assigned
        if (assignedPeople.Contains(personId))
        {
            // Remove from assignment
            assignedPeople.Remove(personId);
            RemoveFromPplContainer(personId);
            
            // Restore button color
            if (index < normalPplImages.Count && normalPplImages[index] != null)
            {
                normalPplImages[index].color = Color.white;
            }
            
            Debug.Log($"TeamSelectionPanel: Removed normal person {index}");
        }
        else
        {
            // Check if we can add more people
            if (assignedPeople.Count >= currentDiceLimit)
            {
                Debug.LogWarning($"TeamSelectionPanel: Cannot add more people - limit is {currentDiceLimit}");
                return;
            }
            
            // Add to assignment
            assignedPeople.Add(personId);
            AddToPplContainer(personId, index < normalPplSprites.Count ? normalPplSprites[index] : null);
            
            // Gray out button
            if (index < normalPplImages.Count && normalPplImages[index] != null)
            {
                normalPplImages[index].color = grayedOutColor;
            }
            
            Debug.Log($"TeamSelectionPanel: Added normal person {index}");
        }
    }

    private void OnSpecialPersonClicked(string personType)
    {
        // Check if already assigned
        if (assignedPeople.Contains(personType))
        {
            // Remove from assignment
            assignedPeople.Remove(personType);
            RemoveFromPplContainer(personType);
            
            // Restore button color
            Image img = GetSpecialPersonImage(personType);
            if (img != null)
            {
                img.color = Color.white;
            }
            
            Debug.Log($"TeamSelectionPanel: Removed {personType}");
        }
        else
        {
            // Check if we can add more people
            if (assignedPeople.Count >= currentDiceLimit)
            {
                Debug.LogWarning($"TeamSelectionPanel: Cannot add more people - limit is {currentDiceLimit}");
                return;
            }
            
            // Add to assignment
            assignedPeople.Add(personType);
            AddToPplContainer(personType, GetSpecialPersonSprite(personType));
            
            // Gray out button
            Image img = GetSpecialPersonImage(personType);
            if (img != null)
            {
                img.color = grayedOutColor;
            }
            
            Debug.Log($"TeamSelectionPanel: Added {personType}");
        }
    }

    #endregion

    #region Toggle Callbacks

    private void OnPetToggleChanged(bool isOn)
    {
        if (!isOn) return;

        // Find which toggle is on
        for (int i = 0; i < petToggles.Length; i++)
        {
            if (petToggles[i] != null && petToggles[i].isOn)
            {
                tempPetIndex = i;
                UpdateSlotDisplay();
                Debug.Log($"TeamSelectionPanel: Pet {i} selected");
                return;
            }
        }
    }

    private void OnItemToggleChanged(bool isOn)
    {
        if (!isOn) return;

        // Find which toggle is on
        for (int i = 0; i < itemToggles.Length; i++)
        {
            if (itemToggles[i] != null && itemToggles[i].isOn)
            {
                tempItemIndex = i;
                UpdateSlotDisplay();
                Debug.Log($"TeamSelectionPanel: Item {i} selected");
                return;
            }
        }
    }

    #endregion

    #region People Management

    private void UpdatePeopleArea()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("TeamSelectionPanel: LevelManager not found");
            return;
        }

        // Update normal people buttons visibility
        int normalCount = LevelManager.Instance.ActiveDisciples;
        for (int i = 0; i < normalPplButtons.Length; i++)
        {
            if (normalPplButtons[i] != null)
            {
                normalPplButtons[i].gameObject.SetActive(i < normalCount);
                
                // Reset color
                if (i < normalPplImages.Count && normalPplImages[i] != null)
                {
                    normalPplImages[i].color = Color.white;
                }
            }
        }

        // Update special people visibility
        // Jingshi: -1 = doesn't exist, 0 = occupied, 1 = available
        if (jingshiButton != null)
        {
            int jingshiStatus = LevelManager.Instance.StatusJingshi;
            bool exists = jingshiStatus >= 0;
            bool available = jingshiStatus == 1;
            
            jingshiButton.gameObject.SetActive(exists && available);
            if (jingshiImage != null) jingshiImage.color = Color.white;
        }

        if (jianjunButton != null)
        {
            int jianjunStatus = LevelManager.Instance.StatusJianjun;
            bool exists = jianjunStatus >= 0;
            bool available = jianjunStatus == 1;
            
            jianjunButton.gameObject.SetActive(exists && available);
            if (jianjunImage != null) jianjunImage.color = Color.white;
        }

        if (yuezhengButton != null)
        {
            int yuezhengStatus = LevelManager.Instance.StatusYuezheng;
            bool exists = yuezhengStatus >= 0;
            bool available = yuezhengStatus == 1;
            
            yuezhengButton.gameObject.SetActive(exists && available);
            if (yuezhengImage != null) yuezhengImage.color = Color.white;
        }

        Debug.Log($"TeamSelectionPanel: Updated people area - Normal: {normalCount}, Jingshi: {LevelManager.Instance.StatusJingshi}, Jianjun: {LevelManager.Instance.StatusJianjun}, Yuezheng: {LevelManager.Instance.StatusYuezheng}");
    }

    private void AddToPplContainer(string personId, Sprite sprite)
    {
        if (pplContainer == null || sprite == null) return;

        GameObject spriteObj = new GameObject($"Assigned_{personId}");
        spriteObj.transform.SetParent(pplContainer, false);
        
        Image img = spriteObj.AddComponent<Image>();
        img.sprite = sprite;
        img.SetNativeSize();

        Debug.Log($"TeamSelectionPanel: Added {personId} to container");
    }

    private void RemoveFromPplContainer(string personId)
    {
        if (pplContainer == null) return;

        Transform child = pplContainer.Find($"Assigned_{personId}");
        if (child != null)
        {
            Destroy(child.gameObject);
            Debug.Log($"TeamSelectionPanel: Removed {personId} from container");
        }
    }

    private void ClearPplContainer()
    {
        if (pplContainer == null) return;

        foreach (Transform child in pplContainer)
        {
            Destroy(child.gameObject);
        }
        
        Debug.Log("TeamSelectionPanel: Cleared people container");
    }

    private Image GetSpecialPersonImage(string personType)
    {
        switch (personType.ToLower())
        {
            case "jingshi": return jingshiImage;
            case "jianjun": return jianjunImage;
            case "yuezheng": return yuezhengImage;
            default: return null;
        }
    }

    private Sprite GetSpecialPersonSprite(string personType)
    {
        switch (personType.ToLower())
        {
            case "jingshi": return jingshiSprite;
            case "jianjun": return jianjunSprite;
            case "yuezheng": return yuezhengSprite;
            default: return null;
        }
    }

    #endregion

    #region Update Display

    private void UpdateSlotDisplay()
    {
        // Update pet slot image
        if (petSlotImage != null)
        {
            if (tempPetIndex >= 0 && tempPetIndex < petSprites.Length && petSprites[tempPetIndex] != null)
            {
                petSlotImage.sprite = petSprites[tempPetIndex];
            }
            else
            {
                petSlotImage.sprite = emptyPetSlotSprite;
            }
        }

        // Update item slot image
        if (itemSlotImage != null)
        {
            if (tempItemIndex >= 0 && tempItemIndex < itemSprites.Length && itemSprites[tempItemIndex] != null)
            {
                itemSlotImage.sprite = itemSprites[tempItemIndex];
            }
            else
            {
                itemSlotImage.sprite = emptyItemSlotSprite;
            }
        }
    }

    private void UpdatePetToggles()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("TeamSelectionPanel: LevelManager not found");
            return;
        }

        // Pet status from LevelManager: statusMouse, statusChicken, statusSheep
        // -1 = not owned, >= 0 = owned
        int[] petStatuses = new int[]
        {
            LevelManager.Instance.StatusMouse,
            LevelManager.Instance.StatusChicken,
            LevelManager.Instance.StatusSheep
        };

        for (int i = 0; i < petToggles.Length; i++)
        {
            if (petToggles[i] == null) continue;

            bool owned = i < petStatuses.Length && petStatuses[i] >= 0;
            petToggles[i].interactable = owned;

            // Set toggle state based on temp selection
            if (tempPetIndex == i)
            {
                petToggles[i].SetIsOnWithoutNotify(true);
            }
            else
            {
                petToggles[i].SetIsOnWithoutNotify(false);
            }

            // Sprite is already set from Awake, no need to update
        }
    }

    private void UpdateItemToggles()
    {
        // Item status needs to be added to LevelManager or GlobalTagManager
        // For now, assume all items are available (placeholder)
        // TODO: Connect to actual item ownership system

        for (int i = 0; i < itemToggles.Length; i++)
        {
            if (itemToggles[i] == null) continue;

            // Placeholder: all items available
            bool owned = true; // TODO: Check actual ownership
            itemToggles[i].interactable = owned;

            // Set toggle state based on temp selection
            if (tempItemIndex == i)
            {
                itemToggles[i].SetIsOnWithoutNotify(true);
            }
            else
            {
                itemToggles[i].SetIsOnWithoutNotify(false);
            }

            // Sprite is already set from Awake, no need to update
        }
    }

    #endregion

    #region Public Accessors

    /// <summary>
    /// Get currently selected pet index (-1 if none)
    /// </summary>
    public int GetSelectedPetIndex() => selectedPetIndex;

    /// <summary>
    /// Get currently selected item index (-1 if none)
    /// </summary>
    public int GetSelectedItemIndex() => selectedItemIndex;

    /// <summary>
    /// Check if a valid team is selected (has at least one pet or item)
    /// </summary>
    public bool HasValidSelection() => selectedPetIndex >= 0 || selectedItemIndex >= 0;

    /// <summary>
    /// Check if dice limit is fulfilled
    /// </summary>
    public bool IsDiceLimitFulfilled() => assignedPeople.Count == currentDiceLimit;

    /// <summary>
    /// Get assigned people for dice rolling
    /// </summary>
    public Dictionary<string, int> GetAssignedDices()
    {
        Dictionary<string, int> dices = new Dictionary<string, int>
        {
            { "Normal", 0 },
            { "Jingshi", 0 },
            { "Jianjun", 0 },
            { "Yuezheng", 0 }
        };

        foreach (string personId in assignedPeople)
        {
            if (personId.StartsWith("normal_"))
            {
                dices["Normal"]++;
            }
            else
            {
                string key = char.ToUpper(personId[0]) + personId.Substring(1).ToLower();
                if (dices.ContainsKey(key))
                {
                    dices[key] = 1;
                }
            }
        }

        return dices;
    }

    /// <summary>
    /// Get current event ID
    /// </summary>
    public string GetCurrentEventId() => currentEventId;

    /// <summary>
    /// Get current dice limit
    /// </summary>
    public int GetCurrentDiceLimit() => currentDiceLimit;

    #endregion
}
