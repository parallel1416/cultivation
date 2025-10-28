# Sound System - Quick Setup Guide

## 🎵 Step-by-Step Setup Instructions

### Part 1: SoundManager Setup (5 minutes)

1. **Create SoundManager GameObject**
   - In your first scene (e.g., MainMenu or InitScene)
   - GameObject → Create Empty
   - Name it "SoundManager"
   - Add Component → SoundManager

2. **Configure SoundManager**
   - The component will auto-create audio sources
   - Set volumes (optional):
     - BGM Volume: 0.7 (default)
     - SFX Volume: 1.0 (default)
     - Special Volume: 0.8 (default)
   - Fade duration: 1 second (default)

3. **Add Common Sound Effects (Optional)**
   - Drag AudioClips into these fields:
     - Button Click Sound
     - Button Hover Sound
     - Dialogue Advance Sound
     - Choice Select Sound
   - If left empty, sounds will need to be loaded from Resources

### Part 2: Resource Organization (10 minutes)

1. **Create Folder Structure**
   ```
   Assets/
     Resources/
       Music/          (for BGM files)
       SFX/            (for sound effects)
       Backgrounds/    (for dialogue backgrounds)
       Portraits/      (for character portraits)
   ```

2. **Import Audio Files**
   - Place BGM files in `Resources/Music/`
   - Place SFX files in `Resources/SFX/`
   - Recommended formats:
     - BGM: MP3 or OGG (compressed)
     - SFX: WAV (uncompressed)

3. **Import Image Files**
   - Place backgrounds in `Resources/Backgrounds/`
   - Place portraits in `Resources/Portraits/`
   - Format: PNG (supports transparency)

4. **Configure Import Settings**
   - Select all images
   - Inspector → Texture Type: Sprite (2D and UI)
   - Click Apply

### Part 3: DialogueUI Setup (5 minutes)

1. **Open DialogScene**
   - Scene file location: Find your DialogScene

2. **Setup Background Image**
   - Create UI Image GameObject (if not exists):
     - Canvas → UI → Image
     - Name it "BackgroundImage"
     - RectTransform: Stretch to full screen
     - Move to back (order in hierarchy)
   
3. **Setup Portrait Image**
   - Create UI Image GameObject (if not exists):
     - Canvas → UI → Image
     - Name it "PortraitImage"
     - Position on left or right side
     - Set desired size (e.g., 400x600)

4. **Assign References**
   - Find DialogueUI component in scene
   - Drag BackgroundImage to "Background Image" field
   - Drag PortraitImage to "Portrait Image" field
   - Optional: Assign default sprites

### Part 4: Update Dialogue Files (2 minutes per file)

1. **Edit Existing Dialogues**
   - Open JSON files in `Assets/Resources/Dialogues/`
   
2. **Add Media Fields**
   ```json
   {
       "desc": "Event description",
       "title": "Event Title",
       "background": "Backgrounds/your_image_name",
       "portrait": "Portraits/character_name",
       "music": "Music/your_music_name",
       "sentences": [...]
   }
   ```
   
   **Important**: 
   - Don't include file extensions
   - Path is relative to Resources folder
   - Use forward slashes `/`

3. **Test Dialogue**
   - Play the scene
   - Verify background/portrait appear
   - Check if music plays

### Part 5: Add Button Sounds (2 methods)

#### Method A: Manual (Individual Buttons)
1. Select a Button GameObject
2. Add Component → UIButtonSound
3. Configure:
   - ✓ Play Click Sound
   - ✓ Play Hover Sound (optional)

#### Method B: Batch (All Buttons at Once)
1. Create empty GameObject in scene
2. Add Component → ButtonSoundUtility
3. Configure settings:
   - ✓ Include Inactive Buttons
   - ✓ Play Click Sound
   - ✓ Play Hover Sound (optional)
4. Click "Add UIButtonSound to All Buttons"
5. Delete ButtonSoundUtility GameObject (no longer needed)

### Part 6: Testing (5 minutes)

1. **Test SoundManager**
   - Play the game
   - Check Console for "SoundManager" logs
   - Verify it persists between scenes

2. **Test BGM**
   - Add test code to any script:
     ```csharp
     void Start() {
         SoundManager.Instance.PlayBGM("Music/test_theme");
     }
     ```

3. **Test Button Sounds**
   - Click any button with UIButtonSound
   - Should hear click sound
   - Hover to test hover sound

4. **Test Dialogue**
   - Start a dialogue with media fields
   - Verify:
     - ✓ Background changes
     - ✓ Portrait appears
     - ✓ Music plays/changes

## 🎮 Usage Examples

### In Code

```csharp
// Play BGM
SoundManager.Instance.PlayBGM("Music/battle_theme");

// Crossfade to new music
SoundManager.Instance.CrossfadeBGM("Music/victory_theme");

// Play SFX
SoundManager.Instance.PlaySFX("SFX/sword_slash");

// Play special effect
SoundManager.Instance.PlaySpecial("SFX/level_up");

// Stop music
SoundManager.Instance.StopBGM(fadeOut: true);
```

### In Dialogue JSON

```json
{
    "desc": "A peaceful moment",
    "title": "Garden Scene",
    "background": "Backgrounds/garden",
    "portrait": "Portraits/mentor",
    "music": "Music/peaceful",
    "sentences": [
        {
            "speaker": "Mentor",
            "text": "Welcome to the garden."
        }
    ]
}
```

## ❓ Troubleshooting

### No Sound Playing
- ✓ Check if SoundManager exists in scene
- ✓ Verify AudioListener exists in scene
- ✓ Check volume settings aren't zero
- ✓ Verify audio files are in Resources folder

### Background Not Showing
- ✓ Check DialogueUI has BackgroundImage assigned
- ✓ Verify image is in Resources/Backgrounds/
- ✓ Check Texture Type is "Sprite (2D and UI)"
- ✓ Look for errors in Console

### Portrait Not Showing
- ✓ Check DialogueUI has PortraitImage assigned
- ✓ Verify image is in Resources/Portraits/
- ✓ Check if portrait path in JSON is correct
- ✓ Ensure Image component is enabled

### Music Not Changing
- ✓ Verify music file is in Resources/Music/
- ✓ Check path in dialogue JSON (no extension)
- ✓ Look for "SoundManager:" logs in Console
- ✓ Ensure SoundManager persists between scenes

## 📋 Checklist

Setup Complete When:
- [ ] SoundManager GameObject exists in first scene
- [ ] Resources folders created (Music, SFX, Backgrounds, Portraits)
- [ ] Audio/image files imported
- [ ] DialogueUI has BackgroundImage assigned
- [ ] DialogueUI has PortraitImage assigned
- [ ] At least one dialogue has media fields
- [ ] Button sounds work (test by clicking)
- [ ] Background music plays in dialogue
- [ ] Background image appears in dialogue
- [ ] Portrait appears in dialogue

## 🚀 Next Steps

After basic setup:
1. Create your own BGM tracks
2. Record/find UI sound effects
3. Create background artwork
4. Draw/commission character portraits
5. Update all dialogue files with media
6. Add button sounds to all scenes
7. Tune volume levels for your game
8. Consider adding audio settings menu

## 📚 Additional Resources

- Full documentation: `SoundSystem_README.md`
- Example dialogue: `example_with_media.json`
- Script locations:
  - SoundManager: `Assets/Scripts/Managers/SoundManager.cs`
  - UIButtonSound: `Assets/Scripts/UI/UIButtonSound.cs`
  - ButtonSoundUtility: `Assets/Scripts/Editor/ButtonSoundUtility.cs`
