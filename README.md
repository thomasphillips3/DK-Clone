# 🤖 Robo Rescue — Unity Platformer Project

A modern re-imagining of the original *Donkey Kong* arcade game — rebuilt from scratch in Unity 6 using 2D physics, tilemaps, and mobile-ready controls.  
You play as a **cute robot built from spare parts**, climbing a scrapyard tower to rescue a small companion bot from a magnetic overlord.

---

## 🎯 Project Goals

- **Learn Unity 2D fundamentals** through a complete playable clone.
- Replace all legacy art with **original open-source pixel assets**.
- Deploy cross-platform (Android + iOS) using your **Unity MCP server pipeline**.
- Showcase clean engineering practices — prefabs, layers, physics matrix, and modular scripts.
- Serve as a foundation for future games and plugin experiments.

---

## 🧩 Core Concept

| Original DK Element | Robo Rescue Equivalent |
|----------------------|------------------------|
| Mario | The Player Robot |
| Pauline | The Small Friend Bot |
| Donkey Kong | Magnet Arm / Boss Machine |
| Barrels | Rolling Scrap Gears |
| Hammer | Wrench Power-Up |
| Oil Barrel + Flames | Power Cells + Sparks |
| Ladders | Rebar Ladders |
| Girders | Rusted Metal Platforms |

The player ascends multiple levels, dodging hazards, grabbing power-ups, and eventually reaching their friend at the top.

---

## 🪛 Game Feel

- Tight, snappy jump arcs  
- Short ladder climb acceleration curve  
- Heavy, mechanical impact sounds  
- Lo-fi, warm, and analog tone (as if the whole scrapyard hums)  

---

## 🗂️ Project Structure

```
Assets/
│
├── Art/
│   ├── robot_assets/
│   ├── Sprites/
│   └── Tilemaps/
│
├── Audio/
│   ├── sfx/
│   └── music/
│
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Props/
│   ├── Collectibles/
│   └── UI/
│
├── Scenes/
│   ├── Level01.unity
│   └── Level02.unity
│
├── Scripts/
│   ├── Player/
│   ├── Enemies/
│   ├── Collectibles/
│   ├── Managers/
│   └── Systems/
│
└── UI/
    ├── Canvas.prefab
    └── HUD/
```

---

## ⚙️ Project Settings Summary

### Layers
`Player`, `Platform`, `Ladder`, `Hazard`, `Collectible`, `Enemy`, `OneWay`.

### Collision Matrix
- Player ↔ Platform, OneWay, Enemy, Hazard, Collectible ✅  
- Player ↔ Ladder handled by trigger (no collision)  
- Enemy ↔ Platform, OneWay ✅  
- Gravity (0, –25)  

---

## 🎮 Gameplay Systems

### Player Controller
- Rigidbody-based 2D movement  
- Jump and climb ladders  
- Temporary wrench power-up destroys enemies  
- Death and respawn handled via GameManager  

### Enemy Logic
- Scrap rollers use Rigidbody2D to roll and reverse on impact.  
- Spawner instantiates rollers every few seconds.  
- Future: flying drone variant using physics + AI pathing.

### Level Mechanics
- Tilemap platforms with CompositeCollider2D.  
- Ladders as trigger zones (enable climb state).  
- Power cells emit light and spawn sparks (hazards).  
- One-way platforms use PlatformEffector2D.

### Collectibles
- Wrench = temporary invincibility/damage boost.  
- Battery = extra life.  
- Microchip = bonus points.

### Goal / Win Condition
- Reaching the small robot friend triggers level complete → next scene or restart loop.

---

## 💻 Code Overview

| Script | Purpose |
|---------|----------|
| `PlayerController.cs` | Handles movement, jump, climb, power-up. |
| `ScrapRoller.cs` | Barrel/gear enemy logic. |
| `RollerSpawner.cs` | Periodic spawning of rolling enemies. |
| `KillZone.cs` | Resets player if fallen. |
| `WrenchPickup.cs` + `WrenchBuff.cs` | Power-up effect. |
| `Goal.cs` | Detects level completion. |
| `GameManager.cs` | Manages score, lives, timer, and scenes. |
| `CameraFollow2D.cs` | Smooth dead-zone camera follow. |

---

## 🧱 Level Design Flow

1. **Create Tile Palette**  
   Paint rusted metal platforms, slopes, and walls using `GroundTilemap`.
2. **Add Ladders**  
   Empty GameObjects with BoxCollider2D (trigger) on `Ladder` layer.
3. **Place Hazards**  
   Sparks or rotating saws — `Hazard` layer.
4. **Add Goal Bot**  
   `Goal.cs` trigger on topmost platform.
5. **Position Spawner**  
   Top corner, emits `ScrapRoller` prefabs down ramps.
6. **Adjust Lighting + Camera Bounds**  
   Optional: 2D lights or vignette for depth.

---

## 🧩 Mobile Integration (Unity MCP)

- Build via **MCP Build Automation** for Android + iOS.  
- Mobile input handled via on-screen buttons or Unity’s new Input System.  
- Cloud Save stores high-scores and unlocks per player profile.  
- Analytics disabled in dev mode; optional for release.

---

## 🎨 Art Style

- Pixel-art scale: **32 × 32 px** grid.  
- Color palette: muted steel, neon accent blues, warm yellows.  
- Inspired by retro arcades, but everything feels hand-built from junkyard scrap.

---

## 🔊 Audio Direction

- SFX: mechanical clicks, servo whirs, spark zaps, reverb’d clangs.  
- Music: dusty lofi electro — think Dilla-meets-Mega Man.  
- BGM loops composed in Logic Pro and exported as `.ogg`.

---

## 🧠 Learning Objectives

- Master Unity 2D Tilemaps & Composite Colliders.  
- Understand Physics 2D collision layers and triggers.  
- Build reusable Prefabs and modular scripts.  
- Use the new Input System for both desktop and mobile.  
- Implement structured game state management.  
- Integrate Unity MCP for CI/CD mobile builds.

---

## 🛠️ Next Phases

**Phase 1 – Core Mechanics**  
✅ Player movement, ladders, enemies, goals.  
🕹️ Playable Level 01.

**Phase 2 – Game Loop**  
Lives, score, UI, power-ups, respawn.

**Phase 3 – Art & Audio Pass**  
Replace placeholders, add ambient sound + music.

**Phase 4 – Mobile UX**  
Touch controls, resolution scaling, safe area layout.

**Phase 5 – Polish & Release**  
Menus, save system, achievements.

---

## 💾 Version Control

- Keep assets modular.  
- Use `.gitignore` for Library, Temp, Obj, and Builds.  
- Commit scenes and prefabs separately.  
- Optional: MCP hooks for automated builds and test devices.

---

## 📚 References

- Unity Manual → 2D Tilemap & Collider Setup  
- Unity Input System Guide (2023+)  
- “Donkey Kong” (1981) reference level geometry  
- “Celeste” devlog for jump feel inspiration

---

## 🧩 Credits

**Design & Engineering:** JR  
**Music & Sound:** Bombest Music  
**Creative Direction:** “tomdabomb”  
**Engine:** Unity 6 (2025)  
**Builds:** MCP Server (Android / iOS)

---

> “Born from junk. Powered by rhythm.  
>  Climb, repair, and rise again.” — *Robo Rescue tagline*
