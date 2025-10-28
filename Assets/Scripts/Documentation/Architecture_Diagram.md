# Sound System Architecture Diagram

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        Game Scene                            │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                   SoundManager (Singleton)              │ │
│  │  - Persists across all scenes (DontDestroyOnLoad)      │ │
│  │  - Manages 3 AudioSources: BGM, SFX, Special           │ │
│  │  - Caches loaded AudioClips for performance            │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Component Relationships

```
┌───────────────────────────────────────────────────────────────────┐
│                        Dialogue System                             │
│                                                                    │
│  ┌──────────────────┐       ┌─────────────────────────┐          │
│  │ DialogueManager  │       │   DialogueEvent.json    │          │
│  │                  │       │  - background: string   │          │
│  │ - Loads JSON     │──────▶│  - portrait: string     │          │
│  │ - Triggers events│       │  - music: string        │          │
│  └────────┬─────────┘       └─────────────────────────┘          │
│           │                                                        │
│           │ Events:                                                │
│           │ • OnBackgroundChange(string)                           │
│           │ • OnPortraitChange(string)                            │
│           │ • OnMusicChange(string)                               │
│           │                                                        │
│           ▼                                                        │
│  ┌──────────────────┐                                             │
│  │   DialogueUI     │                                             │
│  │                  │       ┌──────────────────┐                 │
│  │ - Subscribes to  │──────▶│  BackgroundImage │                 │
│  │   events         │       └──────────────────┘                 │
│  │ - Loads sprites  │       ┌──────────────────┐                 │
│  │   from Resources │──────▶│  PortraitImage   │                 │
│  │ - Calls SoundMgr │       └──────────────────┘                 │
│  └────────┬─────────┘                                             │
│           │                                                        │
│           │ SoundManager.CrossfadeBGM(musicPath)                  │
│           │                                                        │
│           ▼                                                        │
│  ┌──────────────────┐                                             │
│  │  SoundManager    │                                             │
│  │                  │                                             │
│  │ - Loads music    │       Resources/                            │
│  │ - Crossfades BGM │       ├─ Music/                             │
│  └──────────────────┘       ├─ Backgrounds/                       │
│                              └─ Portraits/                         │
└───────────────────────────────────────────────────────────────────┘
```

## Button Sound Flow

```
┌──────────────────────────────────────────────────────────┐
│                     UI Button System                      │
│                                                           │
│  ┌────────────────┐                                      │
│  │  Button (UI)   │                                      │
│  │                │                                      │
│  │  ┌──────────────────────────┐                        │
│  │  │   UIButtonSound          │                        │
│  │  │   - OnClick listener     │                        │
│  │  │   - OnHover listener     │                        │
│  │  └─────────┬────────────────┘                        │
│  │            │                                          │
│  └────────────┼──────────────────────────────────────── │
│               │                                          │
│               │ On Click/Hover                           │
│               │                                          │
│               ▼                                          │
│  ┌────────────────────────┐                             │
│  │    SoundManager        │                             │
│  │                        │                             │
│  │  PlayButtonClick()     │                             │
│  │  PlayButtonHover()     │                             │
│  │                        │                             │
│  │  ┌──────────────────┐  │                             │
│  │  │ SFX AudioSource  │  │                             │
│  │  └──────────────────┘  │                             │
│  └────────────────────────┘                             │
└──────────────────────────────────────────────────────────┘
```

## Resource Loading Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Resource Loading                          │
│                                                              │
│  1. Dialogue Event Starts                                   │
│     │                                                        │
│     ▼                                                        │
│  2. DialogueManager reads JSON fields                       │
│     │  - background: "Backgrounds/forest"                   │
│     │  - portrait: "Portraits/sage"                         │
│     │  - music: "Music/peaceful_theme"                      │
│     │                                                        │
│     ▼                                                        │
│  3. DialogueManager triggers events                         │
│     │                                                        │
│     ├────▶ OnBackgroundChange("Backgrounds/forest")         │
│     │     └─▶ DialogueUI.HandleBackgroundChange()           │
│     │         └─▶ Resources.Load<Sprite>(path)              │
│     │             └─▶ backgroundImage.sprite = loaded       │
│     │                                                        │
│     ├────▶ OnPortraitChange("Portraits/sage")               │
│     │     └─▶ DialogueUI.HandlePortraitChange()             │
│     │         └─▶ Resources.Load<Sprite>(path)              │
│     │             └─▶ portraitImage.sprite = loaded         │
│     │                                                        │
│     └────▶ OnMusicChange("Music/peaceful_theme")            │
│           └─▶ DialogueUI.HandleMusicChange()                │
│               └─▶ SoundManager.CrossfadeBGM(path)           │
│                   └─▶ Resources.Load<AudioClip>(path)       │
│                       └─▶ Fade out old, fade in new         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## SoundManager Audio Channels

```
┌──────────────────────────────────────────────────────────────┐
│                   SoundManager Structure                      │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │ Audio Channel 1: BGM (Background Music)                 │ │
│  │  - Loop: true                                           │ │
│  │  - Volume: 0.7                                          │ │
│  │  - Features: Fade in/out, Crossfade                     │ │
│  │  - Usage: Continuous music playback                     │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │ Audio Channel 2: SFX (Sound Effects)                    │ │
│  │  - Loop: false                                          │ │
│  │  - Volume: 1.0                                          │ │
│  │  - Features: One-shot playback                          │ │
│  │  - Usage: Button clicks, UI sounds, game effects        │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │ Audio Channel 3: Special (Important Effects)            │ │
│  │  - Loop: false                                          │ │
│  │  - Volume: 0.8                                          │ │
│  │  - Features: One-shot playback                          │ │
│  │  - Usage: Level up, achievements, critical events       │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                               │
│  Caching Layer:                                               │
│  ┌───────────────────────────────┐                           │
│  │ Dictionary<string, AudioClip> │                           │
│  │  - Stores loaded clips        │                           │
│  │  - Prevents re-loading        │                           │
│  │  - Key: Resource path         │                           │
│  └───────────────────────────────┘                           │
└──────────────────────────────────────────────────────────────┘
```

## File Organization

```
Assets/
├── Scripts/
│   ├── Managers/
│   │   └── SoundManager.cs ..................... [Core audio manager]
│   │
│   ├── UI/
│   │   ├── DialogueUI.cs ...................... [Handles media display]
│   │   └── UIButtonSound.cs ................... [Button sound component]
│   │
│   ├── Dialogue/
│   │   ├── DialogueManager.cs ................. [Event loading + new fields]
│   │   ├── DialogueManager.UI.cs .............. [New events]
│   │   └── DialogueManager.Playback.cs ........ [Triggers media changes]
│   │
│   ├── Editor/
│   │   └── ButtonSoundUtility.cs .............. [Batch operations tool]
│   │
│   └── Documentation/
│       ├── SoundSystem_README.md .............. [Full documentation]
│       ├── SoundSystem_QuickStart.md .......... [Setup guide]
│       └── Implementation_Summary.md .......... [This summary]
│
└── Resources/
    ├── Dialogues/
    │   ├── grass.json ......................... [Existing dialogue]
    │   └── example_with_media.json ............ [New example]
    │
    ├── Music/ ................................. [BGM files - to be added]
    ├── SFX/ ................................... [Sound effects - to be added]
    ├── Backgrounds/ ........................... [Dialogue backgrounds - to be added]
    └── Portraits/ ............................. [Character portraits - to be added]
```

## Event Flow Timeline

```
Time ──────────────────────────────────────────────────────────▶

  Dialogue Event Loaded
         │
         ├─ DialogueManager.StartDialoguePlayback()
         │
         ├─ DialogueManager.ApplyEventSettings()
         │      │
         │      ├─ TriggerBackgroundChange() ─────▶ DialogueUI.HandleBackgroundChange()
         │      │                                         │
         │      │                                         ├─ Load sprite from Resources
         │      │                                         └─ Update backgroundImage
         │      │
         │      ├─ TriggerPortraitChange() ───────▶ DialogueUI.HandlePortraitChange()
         │      │                                         │
         │      │                                         ├─ Load sprite from Resources
         │      │                                         └─ Update portraitImage
         │      │
         │      └─ TriggerMusicChange() ──────────▶ DialogueUI.HandleMusicChange()
         │                                                │
         │                                                └─ SoundManager.CrossfadeBGM()
         │                                                      │
         │                                                      ├─ Fade out current
         │                                                      ├─ Load new clip
         │                                                      └─ Fade in new
         │
         ├─ Display title (if present)
         │
         └─ Play first sentence
              │
              ├─ Display speaker + text
              │
              └─ Wait for player input ───▶ Continue dialogue...
```

## Integration Points

```
Your Game
    │
    ├─ Scenes
    │   ├─ Any Scene
    │   │   ├─ SoundManager (DontDestroyOnLoad)
    │   │   └─ Buttons with UIButtonSound
    │   │
    │   └─ DialogScene
    │       └─ DialogueUI
    │           ├─ BackgroundImage ──────▶ Displays loaded backgrounds
    │           ├─ PortraitImage ────────▶ Displays loaded portraits
    │           └─ Subscribes to events ─▶ Responds to dialogue changes
    │
    ├─ Resources
    │   ├─ Dialogues/*.json ─────────────▶ background/portrait/music fields
    │   ├─ Music/*.{mp3,ogg,wav} ────────▶ BGM files
    │   ├─ SFX/*.wav ────────────────────▶ Sound effect files
    │   ├─ Backgrounds/*.png ────────────▶ Background images
    │   └─ Portraits/*.png ──────────────▶ Portrait images
    │
    └─ Your Scripts
        └─ Can call SoundManager.Instance anywhere to play sounds
```

## API Call Examples

```csharp
// === In any MonoBehaviour script ===

// Play background music
SoundManager.Instance.PlayBGM("Music/town_theme");

// Change music smoothly
SoundManager.Instance.CrossfadeBGM("Music/battle_theme");

// Play sound effect
SoundManager.Instance.PlaySFX("SFX/sword_hit");

// Play special effect
SoundManager.Instance.PlaySpecial("SFX/level_up");

// Stop music
SoundManager.Instance.StopBGM(fadeOut: true);

// Change volumes
SoundManager.Instance.SetBGMVolume(0.5f);
SoundManager.Instance.SetSFXVolume(0.8f);

// Check current music
string currentBgm = SoundManager.Instance.GetCurrentBGM();
```

## Quick Reference

| Component | Purpose | Location |
|-----------|---------|----------|
| SoundManager | Global audio control | Any scene (persists) |
| UIButtonSound | Button sounds | Attach to Button GameObjects |
| DialogueUI | Media display | DialogScene |
| DialogueManager | Triggers media changes | Persistent (DontDestroyOnLoad) |
| ButtonSoundUtility | Batch button setup | Editor only |

| Resource Folder | Contents | Format |
|----------------|----------|--------|
| Resources/Music/ | Background music | MP3, OGG, WAV |
| Resources/SFX/ | Sound effects | WAV preferred |
| Resources/Backgrounds/ | Dialogue backgrounds | PNG, JPG |
| Resources/Portraits/ | Character portraits | PNG (transparency) |

| JSON Field | Type | Example | Effect |
|------------|------|---------|--------|
| background | string | "Backgrounds/forest" | Changes background image |
| portrait | string | "Portraits/sage" | Shows character portrait |
| music | string | "Music/peaceful" | Crossfades to new music |
