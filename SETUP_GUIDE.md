# Robo Rescue - Setup Guide

This guide walks you through setting up all prefabs and the Level 01 scene in Unity Editor.

## Prerequisites

1. Open the project in Unity 6
2. Ensure all scripts have compiled without errors
3. Import TextMeshPro if prompted

---

## Part 1: Configure Tags

1. Go to **Edit > Project Settings > Tags and Layers**
2. Add **Player** tag if not already present
3. Verify layers are configured:
   - Layer 6: Player
   - Layer 7: Platform
   - Layer 8: Ladder
   - Layer 9: Hazard
   - Layer 10: Collectible
   - Layer 11: Enemy
   - Layer 12: OneWay

---

## Part 2: Configure Collision Matrix

1. Go to **Edit > Project Settings > Physics 2D**
2. Scroll to **Layer Collision Matrix**
3. Configure collisions:
   - **Player** collides with: Platform, OneWay, Enemy, Hazard, Collectible
   - **Player** does NOT collide with: Ladder (handled by trigger)
   - **Enemy** collides with: Platform, OneWay, Player
   - **Ladder** is trigger-only, no collisions

---

## Part 3: Create Player Prefab

### 3.1 Create Player GameObject

1. **Hierarchy** > Right-click > **Create Empty** > Name: `Player`
2. Set **Tag**: `Player`
3. Set **Layer**: `Player`
4. **Position**: (0, 1, 0)

### 3.2 Add Components to Player

1. **Add Component** > **Rigidbody2D**
   - Gravity Scale: 1
   - Mass: 1
   - Constraints: Freeze Rotation Z ✓

2. **Add Component** > **Capsule Collider 2D**
   - Size: (0.5, 1.0)
   - Direction: Vertical

3. **Add Component** > **Sprite Renderer**
   - Sprite: `Assets/Art/robot_assets/player_idle_1.png`
   - Order in Layer: 1

4. **Add Component** > **Animator**
   - Create Animation Controller: `Assets/Art/Animations/PlayerAnimator.controller`

5. **Add Component** > **Player Controller (Script)**
   - Move Speed: 5
   - Jump Force: 15
   - Climb Speed: 3
   - Ground Layer: Platform, OneWay
   - Wrench Duration: 10

6. **Add Component** > **Player Health (Script)**
   - Max Lives: 3
   - Invincibility Duration: 2
   - Respawn Delay: 1

7. **Add Component** > **Player Input**
   - Actions: Select `InputSystem_Actions`
   - Default Map: Player
   - Behavior: Invoke Unity Events
   - Link events:
     - Player/Move → PlayerController.OnMove
     - Player/Jump → PlayerController.OnJump

### 3.3 Create Ground Check Child

1. Right-click **Player** > **Create Empty** > Name: `GroundCheck`
2. **Position**: (0, -0.5, 0)
3. Assign to PlayerController's **Ground Check** field

### 3.4 Save as Prefab

1. Drag **Player** from Hierarchy to `Assets/Prefabs/Player/`
2. Name: `Player.prefab`

---

## Part 4: Create Enemy Prefabs

### 4.1 ScrapRoller Prefab

1. **Hierarchy** > Right-click > **2D Object > Sprites > Circle**
2. Rename to `ScrapRoller`
3. Set **Layer**: `Enemy`
4. **Position**: (0, 0, 0)

**Components:**
1. **Rigidbody2D**
   - Gravity Scale: 1
   - Mass: 1
   - Collision Detection: Continuous

2. **Circle Collider 2D**
   - Radius: 0.5

3. **Sprite Renderer**
   - Sprite: `Assets/Art/robot_assets/enemy_scrap_roller.png`

4. **Add Component** > **Scrap Roller (Script)**
   - Roll Speed: 3
   - Torque Multiplier: 10
   - Wall Layer: Platform, OneWay

5. Save as Prefab: `Assets/Prefabs/Enemies/ScrapRoller.prefab`

### 4.2 RollerSpawner Prefab

1. **Hierarchy** > Right-click > **Create Empty** > Name: `RollerSpawner`
2. **Position**: (0, 0, 0)

**Components:**
1. **Add Component** > **Roller Spawner (Script)**
   - Roller Prefab: Drag `ScrapRoller.prefab`
   - Spawn Interval: 3
   - Max Active Rollers: 5
   - Spawn Right: ✓

2. Save as Prefab: `Assets/Prefabs/Enemies/RollerSpawner.prefab`

---

## Part 5: Create Props & Collectibles

### 5.1 LadderZone Prefab

1. **Hierarchy** > Right-click > **Create Empty** > Name: `LadderZone`
2. Set **Layer**: `Ladder`

**Components:**
1. **Add Component** > **Box Collider 2D**
   - Is Trigger: ✓
   - Size: (1, 4)

2. **Add Component** > **Ladder Zone (Script)**

3. **Optional**: Add ladder sprites as children for visuals
   - Add ladder_top.png, ladder_middle.png, ladder_bottom.png sprites

4. Save as Prefab: `Assets/Prefabs/Props/LadderZone.prefab`

### 5.2 KillZone Prefab

1. **Hierarchy** > Right-click > **Create Empty** > Name: `KillZone`

**Components:**
1. **Add Component** > **Box Collider 2D**
   - Is Trigger: ✓
   - Size: (100, 2)

2. **Add Component** > **Kill Zone (Script)**
   - Destroy Enemies: ✓

3. Save as Prefab: `Assets/Prefabs/Props/KillZone.prefab`

### 5.3 WrenchPickup Prefab

1. **Hierarchy** > Right-click > **2D Object > Sprites > Circle**
2. Rename to `WrenchPickup`
3. Set **Layer**: `Collectible`

**Components:**
1. **Circle Collider 2D**
   - Is Trigger: ✓
   - Radius: 0.3

2. **Sprite Renderer**
   - Sprite: `Assets/Art/robot_assets/wrench.png`

3. **Add Component** > **Wrench Pickup (Script)**
   - Score Value: 50
   - Bob Speed: 1
   - Bob Height: 0.2

4. Save as Prefab: `Assets/Prefabs/Collectibles/WrenchPickup.prefab`

### 5.4 BatteryPickup Prefab

1. Create similar to Wrench
2. Sprite: `battery.png`
3. Add **Battery Pickup (Script)**
4. Save as: `Assets/Prefabs/Collectibles/BatteryPickup.prefab`

### 5.5 MicrochipPickup Prefab

1. Create similar to Wrench
2. Sprite: `microchip.png`
3. Add **Microchip Pickup (Script)**
4. Save as: `Assets/Prefabs/Collectibles/MicrochipPickup.prefab`

### 5.6 Goal Prefab

1. **Hierarchy** > Right-click > **2D Object > Sprite**
2. Rename to `Goal`
3. **Position**: (0, 0, 0)

**Components:**
1. **Sprite Renderer**
   - Sprite: `Assets/Art/robot_assets/small_robot_friend.png`

2. **Box Collider 2D**
   - Is Trigger: ✓
   - Size: (1, 1)

3. **Add Component** > **Goal (Script)**
   - Next Scene Name: "Level02" (or leave empty to loop)
   - Completion Score: 1000
   - Time Bonus: 500

4. Save as Prefab: `Assets/Prefabs/Props/Goal.prefab`

---

## Part 6: Create UI Canvas

### 6.1 Main Canvas

1. **Hierarchy** > Right-click > **UI > Canvas**
2. Rename to `GameCanvas`

**Canvas Settings:**
- Render Mode: Screen Space - Overlay
- Canvas Scaler:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080
  - Match: 0.5

### 6.2 HUD Panel

1. Right-click **GameCanvas** > **UI > Panel**
2. Rename to `HUD`
3. Anchor: Top-Left
4. Width: 400, Height: 150

**Add HUD Elements:**

1. **Score Text** (TextMeshPro)
   - Anchor: Top-Left
   - Text: "SCORE: 000000"
   - Font Size: 36

2. **Lives Icon** (Image)
   - Sprite: `hud_icon_lives.png`
   - Width: 32, Height: 32

3. **Lives Text** (TextMeshPro)
   - Text: "x 3"
   - Font Size: 32

4. **Timer Text** (TextMeshPro)
   - Anchor: Top-Right
   - Text: "180"
   - Font Size: 40

5. **Add Component to HUD Panel** > **HUD Controller (Script)**
   - Assign all text fields

### 6.3 Mobile Controls Panel

1. Right-click **GameCanvas** > **UI > Panel**
2. Rename to `MobileControls`
3. Anchor: Full stretch
4. Make panel transparent (Alpha = 0)

**Virtual Joystick:**
1. Right-click **MobileControls** > **UI > Image**
2. Rename to `Joystick`
3. Anchor: Bottom-Left
4. Position: (150, 150)
5. Width/Height: 200

**Joystick Background:**
- Create circle sprite or use UI sprite
- Color: Semi-transparent white

**Joystick Handle (Child):**
1. Right-click **Joystick** > **UI > Image**
2. Name: `Handle`
3. Anchor: Center
4. Width/Height: 80
5. Color: White

**Add Component to Joystick** > **Virtual Joystick (Script)**
- Control Path: `<Gamepad>/leftStick`
- Movement Range: 50
- Assign Background and Handle

**Jump Button:**
1. Right-click **MobileControls** > **UI > Button - TextMeshPro**
2. Rename to `JumpButton`
3. Anchor: Bottom-Right
4. Position: (-150, 150)
5. Width/Height: 120
6. Text: "JUMP"

**Add Component to JumpButton** > **Virtual Button (Script)**
- Control Path: `<Gamepad>/buttonSouth`

### 6.4 Save Canvas as Prefab

1. Drag **GameCanvas** to `Assets/Prefabs/UI/`
2. Name: `Canvas.prefab`

---

## Part 7: Setup Level01 Scene

### 7.1 Scene Basics

1. Open **Level01.unity** scene
2. Delete default Main Camera and Directional Light if present

### 7.2 Camera Setup

1. **Create** > **Camera**
2. Name: `Main Camera`
3. Tag: `MainCamera`
4. Position: (0, 5, -10)
5. Projection: Orthographic
6. Size: 8
7. Background: Dark color (e.g., #1a1a2e)

**Add Component** > **Camera Follow 2D (Script)**
- Auto Find Player: ✓
- Smooth Speed: 5
- Use Dead Zone: ✓
- Dead Zone Size: (2, 1)
- Use Bounds: ✓
- Min Bounds: (-10, 0)
- Max Bounds: (10, 20)

### 7.3 GameManager

1. **Create Empty** > Name: `GameManager`
2. **Add Component** > **Game Manager (Script)**
- Starting Lives: 3
- Level Time Limit: 180
- Use Timer: ✓

### 7.4 Create Platforms (Classic DK Level 1 Layout)

**Using Tilemaps:**

1. **Hierarchy** > Right-click > **2D Object > Tilemap > Rectangular**
2. Rename Grid to `Platforms`
3. Select **Tilemap** child

**Configure Tilemap:**
- **Layer**: Platform
- **Add Component** > **Tilemap Collider 2D**
- **Add Component** > **Composite Collider 2D**
  - Used By Composite: ✓ (on Tilemap Collider)
- **Add Component** > **Rigidbody2D** (auto-added with Composite)
  - Body Type: Static

**Paint Platforms:**
1. Open **Tile Palette** (Window > 2D > Tile Palette)
2. Select platform tiles from `Assets/Art/Tilemaps/`
3. Paint the classic Donkey Kong level structure:

**Bottom Platform** (y = 0):
- Full width horizontal platform
- Position: (-8, 0) to (8, 0)

**Platform 2** (y = 4):
- Diagonal slope from bottom-right to mid-left
- Position: (8, 4) to (-2, 6)
- Gap at left end for ladder

**Platform 3** (y = 8):
- Diagonal slope from bottom-left to mid-right  
- Position: (-8, 8) to (2, 10)
- Gap at right end

**Platform 4** (y = 12):
- Diagonal slope from bottom-right to mid-left
- Position: (8, 12) to (-2, 14)

**Top Platform** (y = 16):
- Horizontal platform
- Position: (-8, 16) to (8, 16)

### 7.5 Place Ladders

**Ladder placement matching DK:**

1. Drag `LadderZone.prefab` into scene
2. Scale BoxCollider to match ladder height
3. Add visual ladder sprites as children

**Ladder positions:**
- Bottom to Platform 2: (6, 0) to (6, 4)
- Platform 2 to Platform 3: (-6, 6) to (-6, 8)
- Platform 3 to Platform 4: (4, 10) to (4, 12)
- Platform 4 to Top: (-4, 14) to (-4, 16)
- Optional: Short ladders for variety

### 7.6 Place Player

1. Drag `Player.prefab` into scene
2. Position: (-6, 1, 0) (bottom-left start)
3. Ensure Ground Check is positioned correctly

### 7.7 Place Goal

1. Drag `Goal.prefab` into scene
2. Position: (0, 17, 0) (top platform center)

### 7.8 Place Spawner

1. Drag `RollerSpawner.prefab` into scene
2. Position: (8, 17, 0) (top-right corner)
3. Configure:
   - Spawn Right: ✗ (spawn moving left)
   - Spawn Interval: 3

### 7.9 Place KillZone

1. Drag `KillZone.prefab` into scene
2. Position: (0, -5, 0)
3. Scale BoxCollider: (100, 2)

### 7.10 Add Collectibles (Optional)

1. Drag `WrenchPickup.prefab` into scene
2. Position on platforms: (3, 5, 0), (-3, 9, 0), etc.
3. Drag `BatteryPickup.prefab` for extra life
4. Position: (0, 13, 0)

### 7.11 Add Canvas

1. Drag `Canvas.prefab` into scene
2. Ensure it's at root of hierarchy
3. Check mobile controls are only visible on touch devices

---

## Part 8: Testing

1. **Play** the scene
2. Test:
   - ✓ Player movement (WASD/Arrows)
   - ✓ Jump (Space)
   - ✓ Ladder climbing
   - ✓ Enemy spawning and collision
   - ✓ Collectible pickup
   - ✓ Reaching goal
   - ✓ Falling into kill zone
   - ✓ HUD updates
   - ✓ Mobile controls (in Game view, test touch simulation)

---

## Part 9: Build Settings

1. **File > Build Settings**
2. Add `Level01` scene
3. Platform: Android or iOS
4. Configure player settings:
   - Orientation: Landscape
   - Graphics API: Auto
   - Minimum API Level: Android 7.0+ / iOS 12.0+

---

## Troubleshooting

**Scripts don't appear in Add Component:**
- Ensure all scripts compiled without errors
- Check Console for errors

**Player falls through platforms:**
- Check Layer Collision Matrix
- Ensure Platform layer is set on tilemaps

**Ladder doesn't work:**
- Ensure LadderZone has Is Trigger checked
- Check Player tag is set
- Verify Layer is "Ladder"

**Mobile controls don't work:**
- Install Input System package
- Enable new Input System in Project Settings
- Ensure On-Screen controls are set up properly

---

## Next Steps

After completing Phase 1:
- Create Level 02
- Add pause menu
- Add main menu
- Implement save system
- Polish animations
- Add sound effects and music
- Build and test on mobile devices

---

Good luck building Robo Rescue! 🤖

