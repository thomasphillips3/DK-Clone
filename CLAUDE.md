# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**time off 3** is a 3D interactive album experience built in Unity 6 (6000.2.10f1). It serves as an unconventional music promotion tool for 8 lofi hip hop & funk instrumental tracks. The player explores an abandoned 70s/80s music studio in first person while the album plays. Gameplay is optional — the music is the product.

## Build & Run

- **Open in Unity Hub** with Unity 6000.2.10f1
- **Play in Editor**: Open any `Room_XX_*.unity` scene and press Play, or start from `Boot.unity`
- **Build**: Use Unity MCP `manage_build` action or File > Build Settings
- **Scene load order**: Boot → Menu → Room_01..08
- **No automated test suite**
- **No CLI build commands** — builds through Unity Editor or MCP tooling

## Architecture

### Scene Flow
- **Boot.unity** — Initializes singletons (GameManager, AlbumAudioManager), transitions to Menu
- **Menu.unity** — Track selection with 3 navigation mode options (Menu/Connected/Sequential)
- **Room_01..08** — One scene per track, each a different area of the abandoned studio

### Three Navigation Modes
1. **Menu** — Select a track from the menu, loads that room
2. **Connected** — All rooms linked by corridors, walk between them, audio crossfades
3. **Sequential** — Album plays start to finish, scenes auto-transition

### Core Audio Pipeline
`AlbumAudioManager` → `AlbumPlaybackController` → per-room `RoomManager`

- **AlbumAudioManager** (singleton) — Two AudioSources for A/B crossfading. DSP-time beat detection. Events: `OnBeat`, `OnTrackChanged`, `OnTrackEnded`. ZERO sound effects anywhere.
- **AlbumPlaybackController** — Coordinates audio with navigation mode and scene loading
- Audio NEVER stops or glitches during scene transitions. This is the #1 priority.

### Player
- **StudioExplorer** — First-person CharacterController. WASD + mouse look. Walk only (no run/jump).
- **InteractionController** — Raycast-based interaction with `IInteractable` objects

### Procedural Detail System (ISS-inspired)
- **SeedUtility** — Deterministic value from position + seed (adapted from Infinity Square Space)
- **ObjectPool** — Generic pool for clutter objects
- **RoomDetailPlacer** — Grid-based procedural placement of dust, cobwebs, small props

### The 8 Rooms
| Track | Room | Scene |
|-------|------|-------|
| 01 while it counts | Live Room | Room_01_LiveRoom |
| 02 my kinda crazy | Control Room | Room_02_ControlRoom |
| 03 away | Vocal Booth | Room_03_VocalBooth |
| 04 take u down | Equipment Closet | Room_04_EquipmentCloset |
| 05 underthespelll | Tape Machine Room | Room_05_TapeMachineRoom |
| 06 ghosting | Lounge | Room_06_Lounge |
| 07 what we imagined | Echo Chamber | Room_07_EchoChamber |
| 08 any other day | Rooftop | Room_08_Rooftop |

### Data Layer (ScriptableObjects)
- **AlbumConfig** — Master config: album title, artist, array of TrackData
- **TrackData** — Per-track: audio clip, BPM, colors, room type, seed

## Key Rules

- **Audio is sacred** — Gameplay never affects playback. No sound effects. Only music.
- **Gameplay is optional** — Player doesn't have to interact. Can just listen.
- **Interactions are visual-only** — Plug cables, flip switches = visual feedback, no audio change
- **No scoring, no health, no death** — This is not a traditional game

## Key Patterns

- **Singletons**: GameManager, AlbumAudioManager survive scene loads via `DontDestroyOnLoad`
- **Events**: `Action` delegates for decoupled communication (`OnBeat`, `OnTrackChanged`)
- **ScriptableObject config**: Track data and album config are assets, not hardcoded
- **ISS-inspired procedural**: Seed-based deterministic detail placement

## Project Settings

- **3D project** with built-in render pipeline
- **Physics**: 3D (CharacterController for player)
- **Input System**: New Input System (`AlbumControls.inputactions`) — Move, Look, Interact, Pause
- **ProBuilder**: Used for room geometry

## Credits

Design & Engineering: JR | Music & Sound: Bombest Music | Creative Direction: tomdabomb
