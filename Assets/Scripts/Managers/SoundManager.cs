using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global sound manager for handling BGM, button clicks, and special sound effects.
/// Persists across all scenes using DontDestroyOnLoad.
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance => _instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // Background music
    [SerializeField] private AudioSource sfxSource; // Sound effects (buttons, UI)
    [SerializeField] private AudioSource specialSource; // Special effects (dialogue events, etc.)

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float specialVolume = 0.8f;

    [Header("Common Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip dialogueAdvanceSound;
    [SerializeField] private AudioClip choiceSelectSound;

    [Header("BGM Transition Settings")]
    [SerializeField] private float bgmFadeDuration = 1f;

    private Dictionary<string, AudioClip> bgmCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();
    private Coroutine bgmFadeCoroutine;
    private string currentBgmName = "";

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize audio sources if not assigned
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        // Create BGM source if not assigned
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        // Create SFX source if not assigned
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Create special source if not assigned
        if (specialSource == null)
        {
            GameObject specialObj = new GameObject("Special_Source");
            specialObj.transform.SetParent(transform);
            specialSource = specialObj.AddComponent<AudioSource>();
            specialSource.loop = false;
            specialSource.playOnAwake = false;
        }

        // Apply volume settings
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (specialSource != null) specialSource.volume = specialVolume;
    }

    #region BGM Methods

    /// <summary>
    /// Play BGM by resource path (e.g., "Music/menu_theme")
    /// </summary>
    public void PlayBGM(string bgmName, bool fadeIn = true)
    {
        if (string.IsNullOrEmpty(bgmName))
        {
            Debug.LogWarning("SoundManager: BGM name is empty");
            return;
        }

        // Don't restart if same BGM is already playing
        if (currentBgmName == bgmName && bgmSource.isPlaying)
        {
            return;
        }

        // Load BGM from cache or Resources
        AudioClip clip = LoadBGM(bgmName);
        if (clip == null)
        {
            Debug.LogError($"SoundManager: BGM not found: {bgmName}");
            return;
        }

        // Stop current fade if any
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        currentBgmName = bgmName;

        if (fadeIn)
        {
            bgmFadeCoroutine = StartCoroutine(FadeInBGM(clip));
        }
        else
        {
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// Stop BGM with optional fade out
    /// </summary>
    public void StopBGM(bool fadeOut = true)
    {
        if (bgmSource == null || !bgmSource.isPlaying)
        {
            return;
        }

        // Stop current fade if any
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        if (fadeOut)
        {
            bgmFadeCoroutine = StartCoroutine(FadeOutBGM());
        }
        else
        {
            bgmSource.Stop();
            currentBgmName = "";
        }
    }

    /// <summary>
    /// Crossfade to new BGM
    /// </summary>
    public void CrossfadeBGM(string newBgmName)
    {
        if (string.IsNullOrEmpty(newBgmName))
        {
            StopBGM(true);
            return;
        }

        AudioClip clip = LoadBGM(newBgmName);
        if (clip == null)
        {
            Debug.LogError($"SoundManager: BGM not found: {newBgmName}");
            return;
        }

        // Stop current fade if any
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        currentBgmName = newBgmName;
        bgmFadeCoroutine = StartCoroutine(CrossfadeBGMCoroutine(clip));
    }

    private AudioClip LoadBGM(string bgmName)
    {
        // Check cache first
        if (bgmCache.ContainsKey(bgmName))
        {
            return bgmCache[bgmName];
        }

        // Load from Resources
        AudioClip clip = Resources.Load<AudioClip>(bgmName);
        if (clip != null)
        {
            bgmCache[bgmName] = clip;
        }

        return clip;
    }

    private IEnumerator FadeInBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float elapsed = 0f;
        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, elapsed / bgmFadeDuration);
            yield return null;
        }

        bgmSource.volume = bgmVolume;
        bgmFadeCoroutine = null;
    }

    private IEnumerator FadeOutBGM()
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / bgmFadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        currentBgmName = "";
        bgmFadeCoroutine = null;
    }

    private IEnumerator CrossfadeBGMCoroutine(AudioClip newClip)
    {
        float halfDuration = bgmFadeDuration / 2f;
        
        // Fade out current BGM
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / halfDuration);
                yield return null;
            }
        }

        // Switch clip
        bgmSource.clip = newClip;
        bgmSource.Play();

        // Fade in new BGM
        float elapsed2 = 0f;
        while (elapsed2 < halfDuration)
        {
            elapsed2 += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, elapsed2 / halfDuration);
            yield return null;
        }

        bgmSource.volume = bgmVolume;
        bgmFadeCoroutine = null;
    }

    #endregion

    #region SFX Methods

    /// <summary>
    /// Play a one-shot sound effect by resource path
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        AudioClip clip = LoadSFX(sfxName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SoundManager: SFX not found: {sfxName}");
        }
    }

    /// <summary>
    /// Play a one-shot sound effect with AudioClip
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private AudioClip LoadSFX(string sfxName)
    {
        // Check cache first
        if (sfxCache.ContainsKey(sfxName))
        {
            return sfxCache[sfxName];
        }

        // Load from Resources
        AudioClip clip = Resources.Load<AudioClip>(sfxName);
        if (clip != null)
        {
            sfxCache[sfxName] = clip;
        }

        return clip;
    }

    #endregion

    #region Common UI Sounds

    /// <summary>
    /// Play button click sound
    /// </summary>
    public void PlayButtonClick()
    {
        if (buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    /// <summary>
    /// Play button hover sound
    /// </summary>
    public void PlayButtonHover()
    {
        if (buttonHoverSound != null)
        {
            sfxSource.PlayOneShot(buttonHoverSound);
        }
    }

    /// <summary>
    /// Play dialogue advance sound
    /// </summary>
    public void PlayDialogueAdvance()
    {
        if (dialogueAdvanceSound != null)
        {
            sfxSource.PlayOneShot(dialogueAdvanceSound);
        }
    }

    /// <summary>
    /// Play choice select sound
    /// </summary>
    public void PlayChoiceSelect()
    {
        if (choiceSelectSound != null)
        {
            sfxSource.PlayOneShot(choiceSelectSound);
        }
    }

    #endregion

    #region Special Effects

    /// <summary>
    /// Play special effect sound by resource path
    /// </summary>
    public void PlaySpecial(string specialName)
    {
        AudioClip clip = LoadSFX(specialName); // Reuse SFX cache
        if (clip != null)
        {
            specialSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SoundManager: Special effect not found: {specialName}");
        }
    }

    /// <summary>
    /// Play special effect with AudioClip
    /// </summary>
    public void PlaySpecial(AudioClip clip)
    {
        if (clip != null && specialSource != null)
        {
            specialSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// Set BGM volume (0-1)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// Set SFX volume (0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// Set special effects volume (0-1)
    /// </summary>
    public void SetSpecialVolume(float volume)
    {
        specialVolume = Mathf.Clamp01(volume);
        if (specialSource != null)
        {
            specialSource.volume = specialVolume;
        }
    }

    /// <summary>
    /// Get current BGM name
    /// </summary>
    public string GetCurrentBGM()
    {
        return currentBgmName;
    }

    #endregion
}
