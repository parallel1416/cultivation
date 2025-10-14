# 2D Unity Game - Scene Architecture Design

## Project: Tower Game with Seamless Scene Transitions

---

## Scene Overview

### 1. **MenuScene** (Starting Scene)
- **Purpose**: Game entry point with animated obstacle removal
- **Camera**: Orthographic 2D camera
- **Key Elements**:
  - Background layer
  - Obstacle sprites (barriers, rocks, etc.)
  - UI elements (tap to start indicator)

### 2. **MapScene** (Main Hub)
- **Purpose**: Overview map with clickable tower sprite
- **Camera**: Orthographic 2D camera
- **Key Elements**:
  - Map background
  - Tower sprite (clickable) at center
  - Other map decorations/UI

### 3. **TowerScene** (Detail View)
- **Purpose**: Close-up draggable tower view
- **Camera**: Orthographic 2D camera with larger orthographic size
- **Key Elements**:
  - Large vertical tower texture
  - Drag controls
  - Interactive elements on tower

---

## Technical Architecture

### Core Systems

#### 1. Scene Management System
- **SceneTransitionManager** (Singleton)
  - Manages all scene transitions
  - Handles loading/unloading
  - Coordinates with animation managers
  - Prevents multiple simultaneous transitions

#### 2. Animation System
- **TransitionAnimator** (Abstract base class)
  - Provides common animation framework
  - Coroutine-based animations
  - Callback support for transition completion

- **MenuToMapTransition** (Inherits TransitionAnimator)
  - Animates obstacles moving out
  - Reveals map scene underneath

- **MapToTowerTransition** (Inherits TransitionAnimator)
  - Camera zoom animation
  - Crossfade between scenes
  - Position interpolation

#### 3. Input Handling
- **TowerClickHandler** (MapScene)
  - Detects clicks on tower sprite
  - Triggers transition to TowerScene

- **TowerDragController** (TowerScene)
  - Handles vertical dragging
  - Clamps tower position
  - Smooth drag physics

---

## Scene Transition Methods

### Method 1: Single Scene with Camera Animation (RECOMMENDED for Map → Tower)

**Advantages**:
- Truly seamless
- No loading times
- Perfect for zoom transitions
- Shared game objects possible

**Implementation**:
1. Both Map and Tower exist in the same scene
2. Tower sprite at map scale is a small preview
3. Full tower texture is hidden/inactive initially
4. On click:
   - Animate camera position to tower location
   - Animate camera orthographic size (zoom)
   - Crossfade between preview sprite and full tower
   - Enable tower drag controls
5. Transition takes 0.5-1.0 seconds

**Technical Details**:
```
Initial State (Map View):
- Camera Position: (0, 0, -10)
- Camera Ortho Size: 5
- Tower Preview: Active, Scale 1
- Tower Full: Inactive

Final State (Tower View):
- Camera Position: (0, towerYOffset, -10)
- Camera Ortho Size: 15
- Tower Preview: Inactive
- Tower Full: Active
```

### Method 2: Additive Scene Loading with Shared Camera

**Advantages**:
- Better organization
- Can unload Map resources
- Still smooth transition

**Implementation**:
1. Load TowerScene additively
2. Animate shared camera between scenes
3. Unload MapScene after transition

### Method 3: Render Texture Crossfade (Menu → Map)

**Advantages**:
- Perfect for obstacle reveal effect
- Can layer scenes
- Smooth visual transition

**Implementation**:
1. Capture Map scene to RenderTexture
2. Display as sprite behind obstacles in Menu
3. Animate obstacles off-screen
4. Switch to actual Map scene
5. Use DontDestroyOnLoad for transition canvas

---

## Detailed Component Breakdown

### Scene 1: MenuScene

```
MenuScene
├── Main Camera
├── Canvas (Screen Space - Overlay)
│   ├── Background Image
│   └── TapToStart Text
├── ObstacleContainer
│   ├── Obstacle1 (Sprite)
│   ├── Obstacle2 (Sprite)
│   ├── Obstacle3 (Sprite)
│   └── Obstacle4 (Sprite)
├── MapPreview (RenderTexture displayed as sprite)
└── MenuTransitionController (Script)
```

**Scripts**:
- `MenuTransitionController.cs` - Manages obstacle animations
- `ObstacleAnimator.cs` - Individual obstacle movement

### Scene 2: MapScene

```
MapScene
├── Main Camera
├── Background
│   └── MapBackground (Sprite)
├── TowerContainer
│   └── TowerSprite (Sprite + BoxCollider2D)
│       └── TowerClickHandler (Script)
├── MapDecorations
│   ├── Tree1, Tree2, etc.
│   └── Other decorative elements
├── Canvas (UI)
│   └── HUD elements
└── SceneTransitionManager (Script)
```

**Scripts**:
- `TowerClickHandler.cs` - Detects tower clicks
- `MapToTowerTransition.cs` - Zoom animation logic

### Scene 3: TowerScene

**Option A: Separate Scene**
```
TowerScene
├── Main Camera (starts at Map camera position/size)
├── TowerContainer
│   └── TowerFull (Large Sprite)
│       └── TowerDragController (Script)
├── InteractiveElements
│   ├── Door1, Door2 (buttons/triggers)
│   └── Windows, etc.
└── Canvas (UI)
    └── Back Button
```

**Option B: Same Scene as Map (RECOMMENDED)**
```
MapScene (Combined)
├── Main Camera
├── MapView (Active initially)
│   ├── Background
│   ├── TowerSprite (preview)
│   └── Decorations
├── TowerView (Inactive initially)
│   └── TowerFull (Large Sprite)
│       └── TowerDragController (Script)
└── TransitionController (Script)
```

**Scripts**:
- `TowerDragController.cs` - Vertical drag controls
- `TowerToMapTransition.cs` - Reverse zoom animation

---

## Animation Timeline Specifications

### Menu → Map Transition (2-3 seconds)

```
Time 0.0s: User taps screen
Time 0.1s: Fade out "Tap to Start" text
Time 0.2s: Begin obstacle animations
    - Obstacle1: Move left, 1.5s duration
    - Obstacle2: Move right, 1.5s duration
    - Obstacle3: Move up, 1.5s duration
    - Obstacle4: Move down, 1.5s duration
    - Use AnimationCurve.EaseInOut
Time 1.7s: Obstacles fully off-screen
Time 1.8s: Load MapScene
Time 2.0s: Fade in MapScene elements
```

### Map → Tower Transition (0.8-1.2 seconds)

```
Time 0.0s: User clicks tower
Time 0.05s: Play click feedback (sound/particle)
Time 0.1s: Begin camera animation
    - Lerp Camera.orthographicSize: 5 → 15
    - Lerp Camera.position.y: 0 → tower offset
    - Duration: 0.8s
    - Curve: AnimationCurve.EaseInOutCubic
Time 0.4s: Begin crossfade
    - Fade out tower preview sprite
    - Fade in full tower sprite
    - Duration: 0.4s (overlaps with zoom)
Time 0.9s: Enable tower drag controls
```

### Tower → Map Transition (0.6-0.8 seconds)

```
Time 0.0s: User presses back button
Time 0.0s: Disable tower drag controls
Time 0.1s: Begin reverse animation
    - Lerp Camera.orthographicSize: 15 → 5
    - Lerp Camera.position.y: tower offset → 0
    - Duration: 0.6s
Time 0.3s: Begin reverse crossfade
    - Fade in tower preview sprite
    - Fade out full tower sprite
Time 0.7s: Re-enable map interactions
```

---

## Key Technical Considerations

### 1. Camera Settings
- **2D Camera**: Use Orthographic projection
- **Clear Flags**: Solid Color
- **Orthographic Size**: 
  - Menu/Map: 5 units
  - Tower: 15-20 units (adjust based on tower height)

### 2. Sprite Setup
- **Tower Preview Sprite**: 100-200 pixels height, PPU 100
- **Tower Full Sprite**: 2048-4096 pixels height, PPU 100
- **Compression**: Use appropriate texture compression
- **Max Size**: 4096 or 8192 for tower texture

### 3. Performance Optimization
- Use sprite atlases for small sprites
- Pool particle effects
- Disable renderers instead of destroying
- Use object pooling for UI elements

### 4. Input Handling
- Support both mouse and touch input
- Use Physics2D raycasts for sprite clicking
- Implement drag threshold to differentiate tap vs drag

### 5. State Management
- Track current scene state (Menu/Map/Tower)
- Prevent input during transitions
- Save/load player progress appropriately

---

## Implementation Order

1. **Phase 1: Core Setup**
   - Create all three scenes
   - Set up basic scene structure
   - Import placeholder sprites

2. **Phase 2: Scene Transition System**
   - Create SceneTransitionManager
   - Implement basic scene loading

3. **Phase 3: Map ↔ Tower Transition**
   - Implement single-scene approach (recommended)
   - Create camera animation system
   - Add tower click detection
   - Implement zoom animation

4. **Phase 4: Tower Drag Controls**
   - Implement drag controller
   - Add bounds clamping
   - Test smoothness

5. **Phase 5: Menu → Map Transition**
   - Create obstacle animation system
   - Implement render texture preview (optional)
   - Add obstacle reveal animation

6. **Phase 6: Polish**
   - Add sound effects
   - Add particle effects
   - Tune animation curves
   - Add UI feedback

---

## Code Architecture Diagram

```
SceneTransitionManager (Singleton)
    ├── Handles scene state
    ├── Prevents concurrent transitions
    └── Provides transition API

TransitionAnimator (Abstract)
    ├── Coroutine management
    ├── Callback support
    └── Animation utilities

MenuToMapTransition : TransitionAnimator
    └── Obstacle reveal animation

MapTowerTransition : TransitionAnimator
    ├── Camera zoom animation
    ├── Sprite crossfade
    └── Bidirectional support

InputManager
    ├── TowerClickHandler (Map)
    └── TowerDragController (Tower)

CameraController
    ├── SmoothFollow
    ├── ZoomControl
    └── BoundsRestriction
```

---

## Recommended Animation Curves

### For Obstacle Movement (Menu → Map)
```csharp
AnimationCurve.EaseInOut(0, 0, 1, 1)
// Or custom: starts slow, speeds up, ends slow
```

### For Camera Zoom (Map ↔ Tower)
```csharp
// Smooth acceleration/deceleration
AnimationCurve custom with keys:
- (0, 0) - tangent: 0
- (0.5, 0.5) - tangent: 2
- (1, 1) - tangent: 0
```

### For Crossfade
```csharp
AnimationCurve.Linear(0, 0, 1, 1)
// Simple linear fade works best
```

---

## File Structure

```
Assets/
├── Scenes/
│   ├── MenuScene.unity
│   ├── MapScene.unity
│   └── TowerScene.unity (optional)
├── Scripts/
│   ├── Core/
│   │   ├── SceneTransitionManager.cs
│   │   ├── TransitionAnimator.cs
│   │   └── GameManager.cs
│   ├── Transitions/
│   │   ├── MenuToMapTransition.cs
│   │   ├── MapTowerTransition.cs
│   │   └── TransitionEffects.cs
│   ├── Input/
│   │   ├── TowerClickHandler.cs
│   │   └── TowerDragController.cs
│   ├── Camera/
│   │   └── CameraController.cs
│   └── UI/
│       ├── MenuController.cs
│       └── UIManager.cs
├── Sprites/
│   ├── Map/
│   ├── Tower/
│   └── Menu/
├── Prefabs/
│   ├── Cameras/
│   ├── UI/
│   └── Transitions/
└── Materials/
    └── TransitionMaterial.mat (for special effects)
```

---

## Next Steps

1. Review this architecture
2. Choose single-scene vs multi-scene approach for Map/Tower
3. Create placeholder sprites
4. Implement core transition system
5. Build and test each transition individually
6. Polish and optimize

This architecture provides flexibility while maintaining seamless transitions!
