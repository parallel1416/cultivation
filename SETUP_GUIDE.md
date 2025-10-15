# Unity Setup Guide

## 1. Create Scenes

`File > New Scene`, create and save:
- `MenuScene.unity` → `Assets/Scenes/`
- `MapScene.unity` → `Assets/Scenes/`
- `TowerScene.unity` → `Assets/Scenes/`

---

## 2. MapScene Setup

### Hierarchy
```
MapScene
├── Main Camera (Orthographic, Size: 5)
├── SceneTransitionManager (Empty + SceneTransitionManager.cs)
├── Background (Sprite)
├── TowerPreview (Sprite + BoxCollider2D + TowerClickHandler.cs)
└── Canvas (Screen Space Overlay, 1920x1080)
    └── MapUI (Panel)
```

### Steps
1. Main Camera: Set to Orthographic, Size = 5, Position (0, 0, -10)
2. Create empty `SceneTransitionManager`, add `SceneTransitionManager.cs`
3. Create `Background` sprite
4. Create `TowerPreview` sprite at center
   - Add `BoxCollider2D`
   - Add `TowerClickHandler.cs`
   - Set Transition Duration: 1.0
5. Create Canvas → Add child Panel `MapUI`

---

## 3. TowerScene Setup

### Hierarchy
```
TowerScene
├── Main Camera (Orthographic, Size: 15)
├── TowerFull (Large Sprite + TowerDragController.cs)
└── Canvas
    └── TowerUI (Panel)
        └── BackButton (Button)
```

### Steps
1. Main Camera: Orthographic, Size = 15, Position (0, 0, -10)
2. Create `TowerFull` sprite
   - Add `TowerDragController.cs`
   - Set: Drag Speed 1.0, Smooth Time 0.1, Auto Calculate Bounds = true
3. Create Canvas → Panel `TowerUI` → Button `BackButton`
4. Add `UIManager.cs` to Canvas
   - Assign BackButton and TowerUI in Inspector

---

## 4. MenuScene Setup

### Hierarchy
```
MenuScene
├── Main Camera (Orthographic, Size: 5)
├── Background (Sprite)
├── ObstacleContainer
│   ├── Obstacle1 (Sprite, Position: -10, 0)
│   ├── Obstacle2 (Sprite, Position: 10, 0)
│   ├── Obstacle3 (Sprite, Position: 0, 10)
│   └── Obstacle4 (Sprite, Position: 0, -10)
├── MenuTransitionController (Empty + MenuToMapTransition.cs)
└── Canvas
    └── CircularMenu (Empty + RectTransform + CircularMenuController.cs)
        ├── StartIcon (Image + Button)
        ├── SettingsIcon (Image + Button)
        ├── ExitIcon (Image + Button)
        └── AboutIcon (Image + Button)
```

### Steps
1. Main Camera: Same as MapScene
2. Create `Background` sprite
3. Create empty `ObstacleContainer`, add 4 child sprites at edges
4. Create empty `MenuTransitionController`
   - Add `MenuToMapTransition.cs`
   - Assign 4 obstacles
   - Set Target Offsets: [0]=(-15,0), [1]=(15,0), [2]=(0,15), [3]=(0,-15)
5. Create Canvas
6. Create empty child `CircularMenu` under Canvas
   - Position: (0, -200)
   - Add `CircularMenuController.cs`
   - Set: degreesPerOption = 37, smoothTime = 0.5
7. Create 4 Image children under CircularMenu:
   - `StartIcon`, `SettingsIcon`, `ExitIcon`, `AboutIcon`
   - Each: Size (120, 120), Add Button component

---

## 5. Build Settings

1. `File > Build Settings`
2. Add scenes in order:
   - MenuScene (index 0)
   - MapScene (index 1)
   - TowerScene (index 2)

---

## 6. Test

### Map → Tower
1. Open MapScene, Play
2. Click TowerPreview
3. Should zoom and load TowerScene
4. Drag tower vertically
5. Click Back → returns to MapScene

### Menu → Map
1. Open MenuScene, Play
2. Tap/click → obstacles animate
3. Should load MapScene

---

## File Structure

```
Assets/
├── Scenes/
│   ├── MenuScene.unity
│   ├── MapScene.unity
│   └── TowerScene.unity
├── Scripts/
│   ├── Core/
│   │   ├── SceneTransitionManager.cs
│   │   └── TransitionAnimator.cs
│   ├── Transitions/
│   │   ├── MenuToMapTransition.cs
│   │   └── MapTowerTransition.cs
│   ├── Input/
│   │   ├── TowerClickHandler.cs
│   │   └── TowerDragController.cs
│   └── UI/
│       ├── CircularMenuController.cs
│       ├── MenuController.cs
│       └── UIManager.cs
└── Sprites/
    ├── Map/ (background, tower_preview)
    ├── Tower/ (tower_full)
    └── Menu/ (obstacles, icons)
```
