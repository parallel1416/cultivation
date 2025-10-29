using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Swaps GameObject images and text colors based on LevelManager.IsBuggy state.
/// Add this to any GameObject in any scene to automatically swap images and text colors.
/// </summary>
public class BuggyImageSwapper : MonoBehaviour
{
    [System.Serializable]
    public class ImageSwapEntry
    {
        [Tooltip("The GameObject with an Image component to swap")]
        public GameObject targetObject;
        
        [Tooltip("Image to use when IsBuggy is false")]
        public Sprite normalImage;
        
        [Tooltip("Image to use when IsBuggy is true")]
        public Sprite buggyImage;
    }

    [System.Serializable]
    public class TextColorEntry
    {
        [Tooltip("The TextMeshProUGUI component to change color")]
        public TextMeshProUGUI targetText;
        
        [Tooltip("Text color to use when IsBuggy is false")]
        public Color normalColor = Color.white;
        
        [Tooltip("Text color to use when IsBuggy is true")]
        public Color buggyColor = Color.red;
    }

    [System.Serializable]
    public class GameObjectToggleEntry
    {
        [Tooltip("The GameObject to enable/disable")]
        public GameObject targetObject;
        
        [Tooltip("Should this object be active when IsBuggy is true?")]
        public bool activeWhenBuggy = true;
    }

    [Header("Image Swap Entries")]
    [SerializeField] private List<ImageSwapEntry> swapEntries = new List<ImageSwapEntry>();

    [Header("Text Color Entries")]
    [SerializeField] private List<TextColorEntry> textColorEntries = new List<TextColorEntry>();

    [Header("GameObject Enable/Disable Entries")]
    [SerializeField] private List<GameObjectToggleEntry> gameObjectToggleEntries = new List<GameObjectToggleEntry>();

    [Header("Settings")]
    [SerializeField] private bool swapOnStart = true;
    [SerializeField] private bool swapOnEnable = true;

    [Header("Debug Preview (Editor Only)")]
    [SerializeField] private bool enableDebugPreview = false;
    [SerializeField] private bool previewAsBuggyState = false;

    private void Start()
    {
        if (swapOnStart)
        {
            SwapImages();
        }
    }

    private void OnEnable()
    {
        if (swapOnEnable)
        {
            SwapImages();
        }
    }

    /// <summary>
    /// Perform the image swap and text color change based on current IsBuggy state
    /// </summary>
    public void SwapImages()
    {
        // Check for debug preview mode in editor
        #if UNITY_EDITOR
        if (enableDebugPreview)
        {
            if (previewAsBuggyState)
            {
                PreviewBuggy();
            }
            else
            {
                PreviewNormal();
            }
            return;
        }
        #endif

        if (LevelManager.Instance == null)
        {
            return;
        }

        bool isBuggy = LevelManager.Instance.IsBuggy;

        // Swap images
        foreach (var entry in swapEntries)
        {
            if (entry.targetObject == null)
            {
                continue;
            }

            Image imageComponent = entry.targetObject.GetComponent<Image>();
            if (imageComponent == null)
            {
                continue;
            }

            // Select the appropriate sprite based on IsBuggy state
            Sprite selectedSprite = isBuggy ? entry.buggyImage : entry.normalImage;

            if (selectedSprite == null)
            {
                continue;
            }

            imageComponent.sprite = selectedSprite;
        }

        // Enable/disable GameObjects FIRST (before changing text colors)
        foreach (var entry in gameObjectToggleEntries)
        {
            if (entry.targetObject == null)
            {
                continue;
            }

            // Determine if object should be active based on IsBuggy state
            bool shouldBeActive = isBuggy ? entry.activeWhenBuggy : !entry.activeWhenBuggy;
            entry.targetObject.SetActive(shouldBeActive);
        }

        // Change text colors AFTER GameObjects are toggled
        foreach (var entry in textColorEntries)
        {
            if (entry.targetText == null)
            {
                continue;
            }

            // Select the appropriate color based on IsBuggy state
            Color selectedColor = isBuggy ? entry.buggyColor : entry.normalColor;
            entry.targetText.color = selectedColor;
        }
    }

    /// <summary>
    /// Manually trigger a swap (useful for calling from buttons or events)
    /// </summary>
    public void RefreshSwap()
    {
        SwapImages();
    }

#if UNITY_EDITOR
    [ContextMenu("Preview Normal State")]
    private void PreviewNormal()
    {
        // Preview normal images
        foreach (var entry in swapEntries)
        {
            if (entry.targetObject != null && entry.normalImage != null)
            {
                Image img = entry.targetObject.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = entry.normalImage;
                }
            }
        }

        // Preview normal GameObject states (IsBuggy = false) - Do this BEFORE text colors
        foreach (var entry in gameObjectToggleEntries)
        {
            if (entry.targetObject != null)
            {
                bool shouldBeActive = !entry.activeWhenBuggy;
                entry.targetObject.SetActive(shouldBeActive);
            }
        }

        // Preview normal text colors - Do this AFTER GameObject toggles
        foreach (var entry in textColorEntries)
        {
            if (entry.targetText != null)
            {
                entry.targetText.color = entry.normalColor;
            }
        }
    }

    [ContextMenu("Preview Buggy State")]
    private void PreviewBuggy()
    {
        // Preview buggy images
        foreach (var entry in swapEntries)
        {
            if (entry.targetObject != null && entry.buggyImage != null)
            {
                Image img = entry.targetObject.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = entry.buggyImage;
                }
            }
        }

        // Preview buggy GameObject states (IsBuggy = true) - Do this BEFORE text colors
        foreach (var entry in gameObjectToggleEntries)
        {
            if (entry.targetObject != null)
            {
                bool shouldBeActive = entry.activeWhenBuggy;
                entry.targetObject.SetActive(shouldBeActive);
            }
        }

        // Preview buggy text colors - Do this AFTER GameObject toggles
        foreach (var entry in textColorEntries)
        {
            if (entry.targetText != null)
            {
                entry.targetText.color = entry.buggyColor;
            }
        }
    }
#endif
}
