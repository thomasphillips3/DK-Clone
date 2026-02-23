# Robo Rescue - Setup Progress Checklist

Use this checklist to track your progress through the Unity Editor setup.

## ✅ Phase 1: Scripts & Configuration (COMPLETED)

- [x] Configure Unity layers (Player, Platform, Ladder, Hazard, Collectible, Enemy, OneWay)
- [x] Set Physics2D gravity to (0, -25)
- [x] Create PlayerController.cs
- [x] Create PlayerHealth.cs
- [x] Create ScrapRoller.cs
- [x] Create RollerSpawner.cs
- [x] Create LadderZone.cs
- [x] Create KillZone.cs
- [x] Create Goal.cs
- [x] Create WrenchPickup.cs
- [x] Create BatteryPickup.cs
- [x] Create MicrochipPickup.cs
- [x] Create GameManager.cs
- [x] Create CameraFollow2D.cs
- [x] Create HUDController.cs
- [x] Create VirtualJoystick.cs
- [x] Create VirtualButton.cs
- [x] Configure InputSystem_Actions (already configured)

---

## 📦 Phase 2: Prefabs (REQUIRES UNITY EDITOR)

See SETUP_GUIDE.md Part 3-5 for detailed instructions.

### Player Prefab
- [ ] Create Player GameObject with tag "Player" and layer "Player"
- [ ] Add Rigidbody2D component (configured)
- [ ] Add CapsuleCollider2D component (configured)
- [ ] Add SpriteRenderer component (with player_idle_1.png)
- [ ] Add Animator component
- [ ] Add PlayerController script (configured)
- [ ] Add PlayerHealth script (configured)
- [ ] Add PlayerInput component (linked to InputSystem_Actions)
- [ ] Create GroundCheck child GameObject
- [ ] Save as prefab to Assets/Prefabs/Player/Player.prefab

### Enemy Prefabs
- [ ] Create ScrapRoller prefab with CircleCollider2D, Rigidbody2D, sprite
- [ ] Add ScrapRoller script (configured)
- [ ] Save to Assets/Prefabs/Enemies/ScrapRoller.prefab
- [ ] Create RollerSpawner prefab
- [ ] Add RollerSpawner script (link ScrapRoller prefab)
- [ ] Save to Assets/Prefabs/Enemies/RollerSpawner.prefab

### Props & Collectibles
- [ ] Create LadderZone prefab with BoxCollider2D (trigger), layer "Ladder"
- [ ] Add LadderZone script
- [ ] Save to Assets/Prefabs/Props/LadderZone.prefab
- [ ] Create KillZone prefab with BoxCollider2D (trigger)
- [ ] Add KillZone script
- [ ] Save to Assets/Prefabs/Props/KillZone.prefab
- [ ] Create WrenchPickup prefab with sprite, CircleCollider2D (trigger)
- [ ] Add WrenchPickup script
- [ ] Save to Assets/Prefabs/Collectibles/WrenchPickup.prefab
- [ ] Create BatteryPickup prefab
- [ ] Add BatteryPickup script
- [ ] Save to Assets/Prefabs/Collectibles/BatteryPickup.prefab
- [ ] Create MicrochipPickup prefab
- [ ] Add MicrochipPickup script
- [ ] Save to Assets/Prefabs/Collectibles/MicrochipPickup.prefab
- [ ] Create Goal prefab with small_robot_friend sprite
- [ ] Add Goal script
- [ ] Save to Assets/Prefabs/Props/Goal.prefab

---

## 🎨 Phase 3: UI Canvas (REQUIRES UNITY EDITOR)

See SETUP_GUIDE.md Part 6 for detailed instructions.

### Main Canvas
- [ ] Create Canvas GameObject
- [ ] Configure Canvas Scaler (1920x1080 reference)
- [ ] Create HUD Panel (top section)
- [ ] Add Score TextMeshPro element
- [ ] Add Lives icon and text elements
- [ ] Add Timer TextMeshPro element
- [ ] Add HUDController script to HUD panel
- [ ] Link all text fields to HUDController

### Mobile Controls
- [ ] Create MobileControls Panel (transparent)
- [ ] Create Joystick with background and handle images
- [ ] Add VirtualJoystick script to Joystick
- [ ] Configure control path: `<Gamepad>/leftStick`
- [ ] Create Jump button
- [ ] Add VirtualButton script to Jump button
- [ ] Configure control path: `<Gamepad>/buttonSouth`
- [ ] Test visibility (hidden on desktop, shown on mobile)

### Save Canvas
- [ ] Save Canvas as prefab to Assets/Prefabs/UI/Canvas.prefab

---

## 🗺️ Phase 4: Level01 Scene (REQUIRES UNITY EDITOR)

See SETUP_GUIDE.md Part 7 for detailed instructions.

### Scene Setup
- [ ] Open Level01.unity scene
- [ ] Create Main Camera with CameraFollow2D script
- [ ] Create GameManager GameObject with GameManager script
- [ ] Configure Physics2D Layer Collision Matrix

### Platforms & Tilemaps
- [ ] Create Grid > Tilemap for platforms
- [ ] Set tilemap to "Platform" layer
- [ ] Add TilemapCollider2D
- [ ] Add CompositeCollider2D
- [ ] Add Rigidbody2D (Static)
- [ ] Paint bottom platform (y=0)
- [ ] Paint 4 diagonal platforms (classic DK layout)
- [ ] Paint top platform (y=16)

### Ladders
- [ ] Place LadderZone prefabs at strategic locations
- [ ] Bottom to Platform 2 (x=6)
- [ ] Platform 2 to Platform 3 (x=-6)
- [ ] Platform 3 to Platform 4 (x=4)
- [ ] Platform 4 to Top (x=-4)
- [ ] Add visual ladder sprites (optional)

### Game Objects
- [ ] Place Player prefab at starting position (-6, 1, 0)
- [ ] Place Goal prefab at top center (0, 17, 0)
- [ ] Place RollerSpawner prefab at top-right (8, 17, 0)
- [ ] Place KillZone prefab below level (0, -5, 0)
- [ ] Place WrenchPickup prefabs (3-5 scattered)
- [ ] Place BatteryPickup prefab (1 hidden)
- [ ] Place MicrochipPickup prefabs (optional bonus)

### Final Scene Setup
- [ ] Add Canvas prefab to scene
- [ ] Configure camera bounds for level
- [ ] Set GameManager timer limit (180s)
- [ ] Save scene

---

## 🧪 Phase 5: Testing (REQUIRES UNITY EDITOR)

### Desktop Controls
- [ ] Test player movement (WASD/Arrows)
- [ ] Test jump (Space)
- [ ] Test ladder climbing (W/S when on ladder)
- [ ] Test ladder exit by jumping
- [ ] Test ground detection (can't jump mid-air)
- [ ] Test coyote time (jump shortly after leaving edge)
- [ ] Test variable jump height (release Space early)

### Enemy & Combat
- [ ] Test enemy spawning at intervals
- [ ] Test enemy rolling and wall bouncing
- [ ] Test enemy damage to player
- [ ] Test player invincibility after damage
- [ ] Test wrench power-up activation
- [ ] Test wrench destroying enemies

### Collectibles
- [ ] Test wrench pickup grants power-up
- [ ] Test battery pickup adds life
- [ ] Test microchip pickup adds score
- [ ] Test all pickups play effects/sounds

### Level Mechanics
- [ ] Test reaching goal completes level
- [ ] Test falling into kill zone respawns player
- [ ] Test player respawn position
- [ ] Test death after losing all lives

### UI & Systems
- [ ] Test HUD score updates
- [ ] Test HUD lives display updates
- [ ] Test HUD timer counts down
- [ ] Test timer running out causes damage
- [ ] Test camera follows player smoothly
- [ ] Test camera bounds prevent showing outside level
- [ ] Test GameManager persists between scenes (if multiple levels)

### Mobile Controls
- [ ] Switch Game view to mobile resolution
- [ ] Test virtual joystick appears
- [ ] Test virtual jump button appears
- [ ] Test joystick controls movement
- [ ] Test jump button triggers jump
- [ ] Verify controls hidden on desktop build

---

## 🚀 Phase 6: Build & Deploy (OPTIONAL)

### Build Configuration
- [ ] Add Level01 to Build Settings
- [ ] Select Android or iOS platform
- [ ] Configure Player Settings (company name, app name, icons)
- [ ] Set orientation to Landscape
- [ ] Configure graphics settings
- [ ] Set minimum API level

### Build
- [ ] Build for Android
- [ ] Test on Android device
- [ ] Build for iOS
- [ ] Test on iOS device

### Polish (Future Phases)
- [ ] Add sound effects
- [ ] Add background music
- [ ] Create additional levels
- [ ] Add pause menu
- [ ] Add main menu
- [ ] Add game over screen
- [ ] Implement save system
- [ ] Add achievements

---

## 📝 Notes & Issues

Use this space to track issues or notes during setup:

```
[Date] [Issue/Note]
Example:
2025-11-11 - Camera bounds need adjustment for wider level
2025-11-11 - Enemy spawn rate too fast, reduced to 4 seconds
```

---

## 🎯 Quick Win Checklist

If you're short on time, complete these core items for a minimal playable version:

**Minimum Viable Product:**
1. [ ] Create Player prefab with PlayerController
2. [ ] Create basic platform tilemap
3. [ ] Create 1 ladder
4. [ ] Create Goal at top
5. [ ] Create KillZone at bottom
6. [ ] Place Player, Camera, GameManager in scene
7. [ ] Test movement, jump, climb, goal

This gets you a playable vertical climbing game. Add enemies and collectibles after!

---

**Current Status:**  
✅ All scripts created and compiled  
⏳ Awaiting Unity Editor setup for prefabs and scene  

**Next Step:** Open Unity Editor and follow SETUP_GUIDE.md starting at Part 2.

