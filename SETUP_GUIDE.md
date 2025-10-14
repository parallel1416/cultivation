# Unity Setup Guide - Tower Game

## Quick Start: Setting Up Your Scenes

Follow these steps to implement the scene architecture in your Unity project.

---

## Phase 1: Project Setup

### 1. Verify Script Installation
All scripts should now be in your `Assets/Scripts/` folder:
- ✅ `Core/SceneTransitionManager.cs`
- ✅ `Core/TransitionAnimator.cs`
- ✅ `Transitions/MenuToMapTransition.cs`
- ✅ `Transitions/MapTowerTransition.cs`
- ✅ `Input/TowerClickHandler.cs`
- ✅ `Input/TowerDragController.cs`
- ✅ `UI/MenuController.cs`
- ✅ `UI/UIManager.cs`

### 2. Create Scene Files
1. In Unity, go to `File > New Scene`
2. Create 3 new scenes:
   - `MenuScene.unity` (save in `Assets/Scenes/`)
   - `MapScene.unity` (save in `Assets/Scenes/`)
   - `TowerScene.unity` (optional - see note below)

**Note**: For the smoothest transitions, I recommend the **single scene approach** where Map and Tower exist in the same scene. See "Recommended Approach" below.

---

## Phase 2: Scene Setup

### Option A: Single Scene Approach (RECOMMENDED)

#### Setup MapScene (contains both Map and Tower views)

**Scene Hierarchy:**
```
MapScene
├── Main Camera
├── SceneTransitionManager (Empty GameObject)
├── MapView (Empty GameObject)
│   ├── Background (Sprite)
│   ├── TowerPreview (Sprite + BoxCollider2D)
│   └── Decorations (Optional sprites)
├── TowerView (Empty GameObject - Inactive by default)
│   └── TowerFull (Large Sprite)
└── Canvas (UI)
    ├── MapUI (Panel - Active)
    └── TowerUI (Panel - Inactive)
        └── BackButton
```

**Step-by-step:**

1. **Main Camera**
   - Type: 2D Camera (Orthographic)
   - Background: Solid Color
   - Orthographic Size: 5
   - Position: (0, 0, -10)

2. **Create SceneTransitionManager GameObject**
   - Create empty GameObject: `SceneTransitionManager`
   - Add component: `SceneTransitionManager` script
   - This will persist across scene loads (DontDestroyOnLoad)

3. **Create MapView**
   - Create empty GameObject: `MapView`
   - Position: (0, 0, 0)
   
   **Add Background:**
   - Create child Sprite: `Background`
   - Set your map background sprite
   - Sorting Layer: Background (create if needed)
   - Order in Layer: 0
   
   **Add Tower Preview:**
   - Create child Sprite: `TowerPreview`
   - Set small tower sprite (100-200px tall)
   - Position: (0, 0, 0) or wherever you want tower on map
   - Add component: `BoxCollider2D`
   - Add component: `TowerClickHandler` script
   - Sorting Layer: Default
   - Order in Layer: 10

4. **Create TowerView**
   - Create empty GameObject: `TowerView`
   - Position: (0, 0, 0)
   - **Set to Inactive** (uncheck in Inspector)
   
   **Add Tower Full:**
   - Create child Sprite: `TowerFull`
   - Set large tower sprite (2048-4096px tall)
   - Position: (0, 0, 0)
   - Add component: `TowerDragController` script
   - Sorting Layer: Default
   - Order in Layer: 0
   - **Configure TowerDragController:**
     - Enable: false (will be enabled by transition)
     - Auto Calculate Bounds: true
     - Drag Speed: 1.0
     - Enable Inertia: true

5. **Create Transition Controller**
   - Select MapView (or create new empty GameObject)
   - Add component: `MapTowerTransition` script
   - **Configure MapTowerTransition:**
     - Main Camera: Drag Main Camera here
     - Map Orthographic Size: 5
     - Tower Orthographic Size: 15 (adjust based on your tower height)
     - Map View: Drag MapView GameObject
     - Tower View: Drag TowerView GameObject
     - Tower Preview Sprite: Drag TowerPreview SpriteRenderer
     - Tower Full Sprite: Drag TowerFull SpriteRenderer
     - Tower View Position: (0, 0, -10)
     - Map View Position: (0, 0, -10)

6. **Create UI Canvas**
   - Right-click in Hierarchy > UI > Canvas
   - Canvas settings:
     - Render Mode: Screen Space - Overlay
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920x1080
   
   **Add Map UI:**
   - Create child Panel: `MapUI`
   - Set active: true
   - Add any map-specific UI elements here
   
   **Add Tower UI:**
   - Create child Panel: `TowerUI`
   - Set active: false
   - Create child Button: `BackButton`
     - Position: Top-left corner
     - Text: "← Back"
   
   **Add UIManager:**
   - Select Canvas
   - Add component: `UIManager` script
   - Configure:
     - Back Button: Drag BackButton
     - Tower UI: Drag TowerUI panel
     - Map UI: Drag MapUI panel

---

### Option B: Multi-Scene Approach

If you prefer separate scenes (less smooth but more organized):

#### MapScene Setup
```
MapScene
├── Main Camera (same as above)
├── SceneTransitionManager
├── Background
├── TowerSprite (with TowerClickHandler)
└── Canvas (with MapUI)
```

#### TowerScene Setup
```
TowerScene
├── Main Camera (same settings)
├── TowerFull (with TowerDragController)
└── Canvas (with TowerUI and BackButton)
```

Then configure SceneTransitionManager to use `SceneManager.LoadSceneAsync()` instead of activating/deactivating GameObjects.

---

### MenuScene Setup

**Scene Hierarchy:**
```
MenuScene
├── Main Camera
├── SceneTransitionManager (if starting from menu)
├── Background
├── ObstacleContainer
│   ├── Obstacle1 (Sprite)
│   ├── Obstacle2 (Sprite)
│   ├── Obstacle3 (Sprite)
│   └── Obstacle4 (Sprite)
├── MenuTransitionController (Empty GameObject)
└── Canvas
    ├── Background Panel
    ├── TapToStartText
    └── PlayButton (optional)
```

**Step-by-step:**

1. **Main Camera**
   - Same settings as MapScene

2. **Create Obstacles**
   - Create empty GameObject: `ObstacleContainer`
   - Create 4 child Sprites positioned around the screen:
     - `Obstacle1` - Left side
     - `Obstacle2` - Right side
     - `Obstacle3` - Top
     - `Obstacle4` - Bottom
   - Make them large enough to cover the screen edges

3. **Add Transition Controller**
   - Create empty GameObject: `MenuTransitionController`
   - Add component: `MenuToMapTransition` script
   - **Configure:**
     - Obstacles: Drag all 4 obstacle GameObjects
     - Target Offsets (auto-generated, adjust if needed):
       - [0]: (-15, 0) - moves left
       - [1]: (15, 0) - moves right
       - [2]: (0, 15) - moves up
       - [3]: (0, -15) - moves down
     - Stagger Delay: 0.1

4. **Create UI**
   - Add Canvas
   - Create Text: `TapToStartText`
     - Center on screen
     - Text: "TAP TO START"
     - Add component: CanvasGroup
   - Optionally add Play Button

5. **Add Menu Controller**
   - Create empty GameObject: `MenuController`
   - Add component: `MenuController` script
   - Configure:
     - Play Button: Drag button (if using)
     - Tap To Start Indicator: Drag text GameObject
     - Use Button Or Tap Anywhere: false (for tap anywhere)

---

## Phase 3: Build Settings

1. Go to `File > Build Settings`
2. Add scenes in order:
   - MenuScene (index 0)
   - MapScene (index 1)
   - TowerScene (index 2 - if using separate scene)
3. Click "Add Open Scenes" or drag scenes from Project window

---

## Phase 4: Testing

### Test 1: Map ↔ Tower Transition
1. Open MapScene in Unity
2. Press Play
3. Click on the TowerPreview sprite
4. Should see smooth zoom animation
5. Tower should become draggable
6. Click Back button
7. Should zoom back out to map

### Test 2: Menu → Map Transition
1. Open MenuScene in Unity
2. Press Play
3. Click anywhere (or Play button)
4. Obstacles should animate away
5. Should load MapScene

### Debug Checklist:
- ✅ All scripts compile without errors
- ✅ References in Inspector are set (no "None" or "Missing")
- ✅ Colliders are present on clickable objects
- ✅ Sprites are assigned to SpriteRenderers
- ✅ Camera is set to Orthographic
- ✅ Canvas is set to Screen Space - Overlay

---

## Phase 5: Fine-Tuning

### Adjust Animation Timing
In `MapTowerTransition`:
- **Zoom Duration**: Increase for slower zoom (1.0 - 1.5s)
- **Crossfade Duration**: How long sprites blend (0.3 - 0.5s)
- **Crossfade Start Delay**: When crossfade starts during zoom (0.3 - 0.5s)

### Adjust Camera Zoom Level
- **Map Orthographic Size**: Higher = see more of map (default: 5)
- **Tower Orthographic Size**: Higher = see more of tower (default: 15)
- Calculate based on your tower sprite height and desired view

### Adjust Drag Settings
In `TowerDragController`:
- **Drag Speed**: Higher = faster drag response
- **Inertia Damping**: Lower = more slide (0.9 - 0.98)
- **Min/Max Y**: Set bounds for tower scrolling

### Animation Curves
- Select `MapTowerTransition` in Inspector
- Expand "Ease Curve" and "Zoom Curve"
- Adjust curve shapes for different animation feels
- Try presets: Linear, EaseIn, EaseOut, EaseInOut

---

## Common Issues & Solutions

### Issue: Tower doesn't respond to clicks
**Solution:**
- Ensure TowerPreview has a `BoxCollider2D`
- Check "Is Trigger" is unchecked
- Verify `TowerClickHandler` script is attached
- Make sure not clicking on UI elements

### Issue: Tower drag doesn't work
**Solution:**
- Check `TowerDragController` is enabled after transition
- Verify bounds (minY/maxY) are correct
- Check Auto Calculate Bounds is enabled
- Ensure tower sprite is large enough

### Issue: Transition doesn't happen
**Solution:**
- Check Console for errors
- Verify all script references are set in Inspector
- Ensure SceneTransitionManager exists in scene
- Check scene names match in Build Settings

### Issue: Camera doesn't zoom smoothly
**Solution:**
- Adjust animation curves in MapTowerTransition
- Increase transition duration
- Check camera positions are set correctly

### Issue: Sprites don't crossfade
**Solution:**
- Verify both sprite renderers are assigned
- Check sorting layers/orders
- Ensure sprites are visible initially (alpha = 1 and 0)

---

## Performance Tips

1. **Texture Settings:**
   - Tower texture: Max Size 4096, Compression: High Quality
   - Map/Menu sprites: Max Size 2048, Compression: Normal

2. **Sprite Atlas:**
   - Create sprite atlas for small UI elements
   - Reduces draw calls

3. **Camera Optimization:**
   - Use Culling Mask to hide inactive layers
   - Disable TowerView MeshRenderers when in map

4. **Physics:**
   - Tower collider can be disabled when not in tower view

---

## Next Steps

1. **Add Art:**
   - Replace placeholder sprites with real art
   - Create detailed tower texture
   - Design menu obstacles

2. **Add Audio:**
   - Click sounds
   - Whoosh sound for transitions
   - Background music

3. **Add Effects:**
   - Particle effects on tower click
   - Screen shake on transition
   - Glow effects on interactive elements

4. **Add Gameplay:**
   - Interactive elements on tower
   - Doors, windows, characters
   - Collectibles and puzzles

5. **Polish UI:**
   - Animated menu elements
   - Smooth button feedback
   - Loading indicators

---

## File Checklist

After setup, you should have:

```
Assets/
├── Scenes/
│   ├── MenuScene.unity ✓
│   ├── MapScene.unity ✓
│   └── TowerScene.unity (optional)
├── Scripts/
│   ├── Core/
│   │   ├── SceneTransitionManager.cs ✓
│   │   └── TransitionAnimator.cs ✓
│   ├── Transitions/
│   │   ├── MenuToMapTransition.cs ✓
│   │   └── MapTowerTransition.cs ✓
│   ├── Input/
│   │   ├── TowerClickHandler.cs ✓
│   │   └── TowerDragController.cs ✓
│   └── UI/
│       ├── MenuController.cs ✓
│       └── UIManager.cs ✓
├── Sprites/
│   ├── map_background.png (your art)
│   ├── tower_preview.png (your art)
│   ├── tower_full.png (your art)
│   └── menu_obstacles/ (your art)
└── Prefabs/ (optional)
    ├── UI/
    └── Transitions/
```

Good luck with your game! 🎮
