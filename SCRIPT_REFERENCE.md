# Script Reference Guide

Quick reference for all scripts and their purposes.

## Player Scripts

### PlayerController.cs
**Location:** `Assets/Scripts/Player/`

**Purpose:** Handles all player movement, jumping, climbing, and power-up state.

**Key Settings:**
- Move Speed: 5
- Jump Force: 15
- Climb Speed: 3
- Ground Check: Reference to child GameObject
- Ground Layer: Platform + OneWay

**Input Actions Required:**
- OnMove(InputAction.CallbackContext)
- OnJump(InputAction.CallbackContext)

### PlayerHealth.cs
**Location:** `Assets/Scripts/Player/`

**Purpose:** Manages player lives, damage, invincibility, and respawn.

**Key Settings:**
- Max Lives: 3
- Invincibility Duration: 2s
- Respawn Delay: 1s

**Events:**
- OnLivesChanged(int)
- OnDeath()
- OnRespawn()

---

## Enemy Scripts

### ScrapRoller.cs
**Location:** `Assets/Scripts/Enemies/`

**Purpose:** Rolling enemy that moves horizontally and bounces off walls.

**Required Components:**
- Rigidbody2D
- CircleCollider2D

**Key Settings:**
- Roll Speed: 3
- Wall Layer: Platform + OneWay
- Max Lifetime: 30s

### RollerSpawner.cs
**Location:** `Assets/Scripts/Enemies/`

**Purpose:** Spawns roller enemies at intervals.

**Key Settings:**
- Roller Prefab: Reference to ScrapRoller prefab
- Spawn Interval: 3s
- Max Active Rollers: 5
- Spawn Right: bool

---

## Level Mechanics Scripts

### LadderZone.cs
**Location:** `Assets/Scripts/Level/`

**Purpose:** Trigger zone that allows player to climb.

**Required Components:**
- BoxCollider2D (Is Trigger: ✓)

**Layer:** Ladder

### KillZone.cs
**Location:** `Assets/Scripts/Level/`

**Purpose:** Kills/respawns player who falls off platforms.

**Required Components:**
- BoxCollider2D (Is Trigger: ✓)

**Key Settings:**
- Instant Kill: false (default)
- Destroy Enemies: true

### Goal.cs
**Location:** `Assets/Scripts/Level/`

**Purpose:** Level completion trigger.

**Required Components:**
- Collider2D (Is Trigger: ✓)

**Key Settings:**
- Next Scene Name: string
- Completion Score: 1000
- Time Bonus: 500

---

## Collectible Scripts

### WrenchPickup.cs
**Location:** `Assets/Scripts/Collectibles/`

**Purpose:** Power-up that grants temporary invincibility.

**Layer:** Collectible

**Key Settings:**
- Score Value: 50
- Bob Speed: 1
- Bob Height: 0.2

### BatteryPickup.cs
**Location:** `Assets/Scripts/Collectibles/`

**Purpose:** Gives player an extra life.

**Layer:** Collectible

**Key Settings:**
- Score Value: 100

### MicrochipPickup.cs
**Location:** `Assets/Scripts/Collectibles/`

**Purpose:** Bonus points collectible.

**Layer:** Collectible

**Key Settings:**
- Score Value: 200

---

## Manager Scripts

### GameManager.cs
**Location:** `Assets/Scripts/Managers/`

**Purpose:** Singleton managing game state, score, lives, timer.

**Pattern:** Singleton (auto-creates, DontDestroyOnLoad)

**Key Settings:**
- Starting Lives: 3
- Level Time Limit: 180s
- Use Timer: true

**Events:**
- OnScoreChanged(int)
- OnLivesChanged(int)
- OnTimeChanged(float)
- OnGameStart()
- OnGameOver()
- OnLevelCompleted()

**Public Methods:**
- AddScore(int)
- StartGame()
- RestartLevel()
- LoadLevel(string)

### CameraFollow2D.cs
**Location:** `Assets/Scripts/Systems/`

**Purpose:** Smooth camera following with dead zone and bounds.

**Required Component:**
- Camera

**Key Settings:**
- Auto Find Player: true
- Smooth Speed: 5
- Use Dead Zone: true
- Dead Zone Size: (2, 1)
- Use Bounds: true
- Min/Max Bounds: Configure per level

---

## UI Scripts

### HUDController.cs
**Location:** `Assets/Scripts/UI/`

**Purpose:** Updates HUD elements (score, lives, timer).

**Required:**
- TextMeshPro (import if prompted)

**References Needed:**
- Score Text
- Lives Text
- Timer Text
- Life Icons (array of Images)
- Power-Up Indicator

### VirtualJoystick.cs
**Location:** `Assets/Scripts/UI/`

**Purpose:** On-screen joystick for mobile input.

**Required Components:**
- RectTransform (background)
- RectTransform (handle)

**Inherits:** OnScreenControl (Unity Input System)

**Key Settings:**
- Control Path: `<Gamepad>/leftStick`
- Movement Range: 50
- Dynamic Joystick: false

### VirtualButton.cs
**Location:** `Assets/Scripts/UI/`

**Purpose:** On-screen button for mobile input.

**Inherits:** OnScreenControl (Unity Input System)

**Key Settings:**
- Control Path: `<Gamepad>/buttonSouth`
- Visual feedback colors

---

## Script Dependencies

### Player Prefab Needs:
1. PlayerController.cs
2. PlayerHealth.cs
3. Rigidbody2D
4. CapsuleCollider2D
5. SpriteRenderer
6. Animator
7. PlayerInput component
   - Actions: InputSystem_Actions
   - Behavior: Invoke Unity Events

### GameManager (Scene GameObject):
- GameManager.cs (singleton, only one per game)

### Camera (Scene GameObject):
- Camera component
- CameraFollow2D.cs

### Canvas (Scene GameObject):
- Canvas
- Canvas Scaler
- Graphic Raycaster
- HUDController.cs (on HUD panel)
- VirtualJoystick.cs (on joystick element)
- VirtualButton.cs (on button element)

---

## Common Issues & Solutions

### "PlayerController.OnMove not found"
- Ensure PlayerInput component is added
- Set Behavior to "Invoke Unity Events"
- Manually assign OnMove and OnJump in Inspector

### "GameManager.Instance is null"
- Create GameManager GameObject in scene
- Attach GameManager.cs script
- GameManager persists across scenes

### "Ladder climbing doesn't work"
- Player needs "Player" tag
- LadderZone needs "Ladder" layer
- BoxCollider2D must have Is Trigger ✓

### "Player falls through platforms"
- Check Physics2D Layer Collision Matrix
- Ensure Platform layer collides with Player layer
- Tilemap needs Rigidbody2D set to Static

### "Mobile controls don't appear"
- OnScreenControl requires Unity Input System package
- Enable new Input System in Project Settings > Player
- Controls auto-hide on desktop, show on mobile

---

## Testing Checklist

- [ ] Player moves left/right (WASD/Arrows)
- [ ] Player jumps (Space)
- [ ] Player climbs ladders (W/S or Up/Down on ladder)
- [ ] Player can exit ladder by jumping
- [ ] Ground detection works (can only jump when grounded)
- [ ] Coyote time works (jump shortly after leaving platform)
- [ ] Jump buffer works (jump registers before landing)
- [ ] Variable jump height (release button early)
- [ ] Enemies spawn at intervals
- [ ] Enemies roll and bounce off walls
- [ ] Enemy collision damages player
- [ ] Player becomes invincible after taking damage
- [ ] Wrench power-up destroys enemies on contact
- [ ] Battery gives extra life
- [ ] Microchip gives bonus points
- [ ] Goal completes level
- [ ] Kill zone respawns player
- [ ] Timer counts down
- [ ] HUD updates correctly
- [ ] Camera follows player smoothly
- [ ] Camera respects bounds
- [ ] Mobile controls work (touch simulation in Game view)

---

## Animation States (To Be Created)

### Player Animator Controller:
- Idle (Speed = 0, IsGrounded = true)
- Walk (Speed > 0, IsGrounded = true)
- Jump (VelocityY > 0)
- Fall (VelocityY < 0, IsGrounded = false)
- Climb (IsClimbing = true)
- Death (trigger: Death)

**Parameters:**
- Float: Speed
- Bool: IsGrounded
- Bool: IsClimbing
- Float: VelocityY
- Bool: HasPowerUp
- Trigger: Death

---

## Performance Tips

1. Use object pooling for frequently spawned objects (enemies, particles)
2. Limit max active enemies (already implemented in spawner)
3. Use Composite Collider on Tilemaps (reduces collider count)
4. Set Rigidbody2D to Static for non-moving platforms
5. Use Continuous collision detection for fast-moving objects

---

## Mobile Optimization

1. Reduce orthographic camera size for mobile (6 instead of 8)
2. Simplify particle effects on mobile
3. Limit max active enemies to 3 on low-end devices
4. Use sprite atlases to reduce draw calls
5. Test on actual devices, not just editor

---

## Next Features to Implement

### Phase 2:
- Pause menu
- Game over screen
- Level transition animations
- Sound effects integration
- Music playback

### Phase 3:
- Main menu
- Level select
- Settings menu
- Save/load system
- Achievements

### Phase 4:
- Multiple levels
- Boss fights
- Power-up variations
- Leaderboards (optional)
- Cloud save (optional)

---

For detailed setup instructions, see SETUP_GUIDE.md

