using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cycles through a list of sprites uniformly at a specified interval.
/// Works with both UI Image and SpriteRenderer components.
/// </summary>
public class SpriteCycle : MonoBehaviour
{
    [Header("Sprite List")]
    [SerializeField] private Sprite[] sprites;

    [Header("Cycle Settings")]
    [SerializeField] private float cycleInterval = 0.5f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool reverseOnLoop = false;

    [Header("Control")]
    [SerializeField] private bool isPlaying = true;

    private Image imageComponent;
    private SpriteRenderer spriteRendererComponent;
    private int currentIndex = 0;
    private float nextCycleTime;
    private bool isReversing = false;

    private void Awake()
    {
        // Try to find Image component (for UI)
        imageComponent = GetComponent<Image>();

        // Try to find SpriteRenderer component (for world objects)
        if (imageComponent == null)
        {
            spriteRendererComponent = GetComponent<SpriteRenderer>();
        }

        if (imageComponent == null && spriteRendererComponent == null)
        {
            Debug.LogError("SpriteCycle: No Image or SpriteRenderer component found on this GameObject!");
            enabled = false;
            return;
        }

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("SpriteCycle: No sprites assigned to cycle through!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            ResetCycle();
            UpdateSprite();
        }
    }

    private void Update()
    {
        if (!isPlaying || sprites == null || sprites.Length == 0)
        {
            return;
        }

        if (Time.time >= nextCycleTime)
        {
            AdvanceToNextSprite();
            nextCycleTime = Time.time + cycleInterval;
        }
    }

    /// <summary>
    /// Advance to the next sprite in the cycle
    /// </summary>
    private void AdvanceToNextSprite()
    {
        if (reverseOnLoop && isReversing)
        {
            // Move backward
            currentIndex--;
            if (currentIndex < 0)
            {
                if (loop)
                {
                    currentIndex = 1; // Start going forward again
                    isReversing = false;
                }
                else
                {
                    currentIndex = 0;
                    isPlaying = false; // Stop if not looping
                }
            }
        }
        else
        {
            // Move forward
            currentIndex++;
            if (currentIndex >= sprites.Length)
            {
                if (loop)
                {
                    if (reverseOnLoop)
                    {
                        currentIndex = sprites.Length - 2;
                        isReversing = true;
                    }
                    else
                    {
                        currentIndex = 0; // Loop back to start
                    }
                }
                else
                {
                    currentIndex = sprites.Length - 1;
                    isPlaying = false; // Stop if not looping
                }
            }
        }

        UpdateSprite();
    }

    /// <summary>
    /// Update the sprite on the component
    /// </summary>
    private void UpdateSprite()
    {
        if (sprites == null || currentIndex < 0 || currentIndex >= sprites.Length)
        {
            return;
        }

        Sprite currentSprite = sprites[currentIndex];
        
        if (currentSprite == null)
        {
            // If sprite is null, set alpha to 0 to make it invisible
            if (imageComponent != null)
            {
                Color color = imageComponent.color;
                color.a = 0f;
                imageComponent.color = color;
            }
            else if (spriteRendererComponent != null)
            {
                Color color = spriteRendererComponent.color;
                color.a = 0f;
                spriteRendererComponent.color = color;
            }
        }
        else
        {
            // If sprite is not null, set sprite and restore alpha to 1
            if (imageComponent != null)
            {
                imageComponent.sprite = currentSprite;
                Color color = imageComponent.color;
                color.a = 1f;
                imageComponent.color = color;
            }
            else if (spriteRendererComponent != null)
            {
                spriteRendererComponent.sprite = currentSprite;
                Color color = spriteRendererComponent.color;
                color.a = 1f;
                spriteRendererComponent.color = color;
            }
        }
    }

    /// <summary>
    /// Start playing the sprite cycle
    /// </summary>
    public void Play()
    {
        isPlaying = true;
        nextCycleTime = Time.time + cycleInterval;
    }

    /// <summary>
    /// Stop playing the sprite cycle
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Pause the sprite cycle
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Resume the sprite cycle from current position
    /// </summary>
    public void Resume()
    {
        isPlaying = true;
        nextCycleTime = Time.time + cycleInterval;
    }

    /// <summary>
    /// Reset cycle to the first sprite
    /// </summary>
    public void ResetCycle()
    {
        currentIndex = 0;
        isReversing = false;
        nextCycleTime = Time.time + cycleInterval;
        UpdateSprite();
    }

    /// <summary>
    /// Jump to a specific sprite index
    /// </summary>
    public void SetSpriteIndex(int index)
    {
        if (sprites == null || index < 0 || index >= sprites.Length)
        {
            Debug.LogWarning($"SpriteCycle: Invalid sprite index {index}");
            return;
        }

        currentIndex = index;
        UpdateSprite();
    }

    /// <summary>
    /// Set the cycle interval
    /// </summary>
    public void SetCycleInterval(float interval)
    {
        cycleInterval = Mathf.Max(0.01f, interval);
    }

    /// <summary>
    /// Set whether the cycle should loop
    /// </summary>
    public void SetLoop(bool shouldLoop)
    {
        loop = shouldLoop;
    }

    /// <summary>
    /// Set the sprites array at runtime
    /// </summary>
    public void SetSprites(Sprite[] newSprites)
    {
        sprites = newSprites;
        ResetCycle();
    }

    /// <summary>
    /// Get the current sprite index
    /// </summary>
    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    /// <summary>
    /// Get the total number of sprites
    /// </summary>
    public int GetSpriteCount()
    {
        return sprites != null ? sprites.Length : 0;
    }

    /// <summary>
    /// Check if currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cycleInterval < 0.01f)
        {
            cycleInterval = 0.01f;
        }
    }
#endif
}
