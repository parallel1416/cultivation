# Sound System Documentation

## Overview
The game now includes a comprehensive sound management system with support for background music (BGM), sound effects (SFX), and special audio effects.

## Components

### 1. SoundManager (Singleton)
**Location**: `Assets/Scripts/Managers/SoundManager.cs`

A persistent audio manager that handles all sound playback across scenes.

#### Features:
- **BGM Management**: Play, stop, fade in/out, and crossfade background music
- **SFX Playback**: One-shot sound effects for UI and gameplay
- **Special Effects**: Separate audio source for important sound effects
- **Volume Control**: Independent volume settings for BGM, SFX, and special effects
- **Resource Caching**: Automatically caches loaded audio clips for better performance

#### Setup:
1. Create an empty GameObject in your first scene (e.g., "SoundManager")
2. Add the `SoundManager` component
3. The manager will persist across all scenes via DontDestroyOnLoad

#### Inspector Settings:
- **Audio Sources**: Optional references (auto-created if not assigned)
- **Volume Settings**: Default volumes for BGM (0.7), SFX (1.0), Special (0.8)
- **Common Sound Effects**: Assign AudioClips for button clicks, hovers, dialogue sounds
- **BGM Transition Settings**: Fade duration (default 1 second)

#### Usage Examples:

```csharp
// Play background music with fade in
SoundManager.Instance.PlayBGM("Music/main_theme", fadeIn: true);

// Crossfade to new music
SoundManager.Instance.CrossfadeBGM("Music/battle_theme");

// Play sound effect
SoundManager.Instance.PlaySFX("SFX/button_click");

// Play special effect
SoundManager.Instance.PlaySpecial("SFX/dice_roll");

// Change volumes
SoundManager.Instance.SetBGMVolume(0.5f);
SoundManager.Instance.SetSFXVolume(0.8f);

// Common UI sounds (requires AudioClips assigned in Inspector)
SoundManager.Instance.PlayButtonClick();
SoundManager.Instance.PlayButtonHover();
SoundManager.Instance.PlayDialogueAdvance();
SoundManager.Instance.PlayChoiceSelect();
```

### 2. UIButtonSound Component
**Location**: `Assets/Scripts/UI/UIButtonSound.cs`

Automatically adds click and hover sounds to UI buttons.

#### Setup:
1. Add component to any Button GameObject
2. Configure settings in Inspector:
   - **Play Click Sound**: Enable/disable click sound
   - **Play Hover Sound**: Enable/disable hover sound
   - **Custom Click Sound**: Optional AudioClip (uses SoundManager default if null)
   - **Custom Hover Sound**: Optional AudioClip (uses SoundManager default if null)

#### Usage:
Simply attach to any Button. It will automatically call SoundManager on click/hover.

### 3. Dialogue System Integration
**Updated Files**: 
- `DialogueManager.cs` - Added background, portrait, music fields to DialogueEvent
- `DialogueManager.UI.cs` - Added events for visual/audio changes
- `DialogueManager.Playback.cs` - Triggers changes when events start
- `DialogueUI.cs` - Handles background/portrait/music changes

#### Dialogue JSON Format:
```json
{
    "desc": "Event description",
    "title": "Event Title",
    "background": "Backgrounds/forest_scene",
    "portrait": "Portraits/character_name",
    "music": "Music/dialogue_theme",
    "triggersImmediately": true,
    "major": false,
    "sentences": [
        {
            "text": "Dialogue text here..."
        }
    ]
}
```

#### DialogueUI Setup:
1. Assign **Background Image** component in Inspector
2. Assign **Portrait Image** component in Inspector
3. Optional: Assign default background/portrait sprites
4. Music changes automatically via SoundManager

## Resource Organization

### Recommended Folder Structure:
```
Assets/
  Resources/
    Music/
      main_theme.wav
      battle_theme.wav
      dialogue_theme.wav
    SFX/
      button_click.wav
      button_hover.wav
      dialogue_advance.wav
      choice_select.wav
      dice_roll.wav
    Backgrounds/
      forest_scene.png
      mountain_scene.png
    Portraits/
      character_name.png
      npc_name.png
```

### Supported Audio Formats:
- WAV (recommended for SFX)
- MP3 (good for BGM)
- OGG (good compression for BGM)

### Supported Image Formats:
- PNG (recommended for transparency)
- JPG (for backgrounds without transparency)

## Integration Guide

### Step 1: Setup SoundManager
1. Create SoundManager GameObject in your initial scene
2. Assign common sound effects in Inspector
3. Set default volumes

### Step 2: Add Audio Resources
1. Create Resources folders as shown above
2. Import your audio files into Resources/Music and Resources/SFX
3. Import background/portrait images into Resources/Backgrounds and Resources/Portraits

### Step 3: Update Dialogue Events
1. Open your dialogue JSON files in `Assets/Resources/Dialogues/`
2. Add `background`, `portrait`, and `music` fields
3. Reference resource paths without file extensions

Example:
```json
{
    "desc": "A mysterious encounter",
    "title": "Forest Path",
    "background": "Backgrounds/forest",
    "portrait": "Portraits/mysterious_stranger",
    "music": "Music/mysterious_theme",
    "sentences": [...]
}
```

### Step 4: Setup DialogueUI
1. Open DialogScene
2. Find DialogueUI component
3. Assign BackgroundImage and PortraitImage references
4. Optional: Assign default sprites

### Step 5: Add Button Sounds
For any button that should play sounds:
1. Select the Button GameObject
2. Add Component → UIButtonSound
3. Configure click/hover settings

## API Reference

### SoundManager Methods

#### BGM Control
- `PlayBGM(string bgmName, bool fadeIn = true)` - Play background music
- `StopBGM(bool fadeOut = true)` - Stop current music
- `CrossfadeBGM(string newBgmName)` - Smoothly transition to new music
- `GetCurrentBGM()` - Get name of currently playing BGM

#### Sound Effects
- `PlaySFX(string sfxName)` - Play sound effect by resource path
- `PlaySFX(AudioClip clip)` - Play sound effect from AudioClip
- `PlaySpecial(string specialName)` - Play special effect by resource path
- `PlaySpecial(AudioClip clip)` - Play special effect from AudioClip

#### Common UI Sounds
- `PlayButtonClick()` - Play button click sound
- `PlayButtonHover()` - Play button hover sound
- `PlayDialogueAdvance()` - Play dialogue advance sound
- `PlayChoiceSelect()` - Play choice selection sound

#### Volume Control
- `SetBGMVolume(float volume)` - Set BGM volume (0-1)
- `SetSFXVolume(float volume)` - Set SFX volume (0-1)
- `SetSpecialVolume(float volume)` - Set special effects volume (0-1)

### DialogueManager Events

New events for UI subscriptions:
- `OnBackgroundChange(string backgroundPath)` - Triggered when background should change
- `OnPortraitChange(string portraitPath)` - Triggered when portrait should change
- `OnMusicChange(string musicPath)` - Triggered when music should change

## Performance Tips

1. **Audio Compression**: Use compressed formats (MP3/OGG) for BGM, uncompressed (WAV) for short SFX
2. **Preload Common Sounds**: Assign frequently used sounds in SoundManager Inspector
3. **Resource Caching**: SoundManager automatically caches loaded clips
4. **Image Optimization**: Compress background/portrait images appropriately
5. **Lazy Loading**: Music and images load on-demand from Resources folder

## Troubleshooting

### No Sound Playing
- Check if SoundManager GameObject exists and has the component
- Verify audio files are in Resources folder
- Check volume settings in SoundManager Inspector
- Ensure AudioListener exists in scene

### Music Not Changing
- Verify music path in dialogue JSON (without file extension)
- Check Resources/Music folder structure
- Look for errors in Console about missing files

### Background/Portrait Not Showing
- Check if DialogueUI has image components assigned
- Verify image paths in dialogue JSON
- Ensure images are in Resources folder
- Check image import settings (Texture Type: Sprite 2D)

### Button Sounds Not Working
- Verify UIButtonSound component is attached
- Check if SoundManager exists in scene
- Ensure button click sound is assigned in SoundManager

## Future Enhancements

Possible additions:
- Sound categories with separate mixers
- Audio ducking (lower BGM volume during dialogue)
- 3D spatial audio support
- Audio pooling for frequent sounds
- Save/load audio settings
- Audio fade curves customization
