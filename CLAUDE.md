# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GoldenAge is a 1920s New York-themed science-magic fusion action RPG prototype built with **Unity 2022.3 LTS** using **Universal Render Pipeline (URP)**. The codebase is written in **C# (.NET Standard 2.1)**.

## Architecture

### Singleton-Based Manager Pattern

All core systems inherit from `Singleton<T>` (located in `Scripts/Core/Singleton.cs`). Access managers via `ManagerName.Instance`.

```
GameManager (central hub, game state control)
├── AudioManager     - BGM, SFX, Voice playback
├── CombatManager    - Battle state, damage calculation
├── DialogueManager  - NPC conversations, branching
├── QuestManager     - Quest tracking, objectives
├── VFXManager       - Visual effect pooling
├── SaveSystem       - JSON-based persistence
├── SceneLoader      - Async scene transitions
├── GameSettings     - Audio/graphics/gameplay config
└── ObjectPool       - Reusable object caching
```

### Game States

`GameManager.CurrentState` controls what systems are active:
- `MainMenu`, `Exploration`, `Dialogue`, `Combat`, `Paused`, `Cutscene`
- Use `GameManager.Instance.CanPlayerInput()` to check if player can act

### Namespaces

| Namespace | Purpose |
|-----------|---------|
| `GoldenAge.Core` | Managers, singletons, input |
| `GoldenAge.Player` | Movement, stats, combat, interaction |
| `GoldenAge.Combat` | Damage, enemies, spawning, attacks |
| `GoldenAge.Dialogue` | Conversation system |
| `GoldenAge.Quest` | Quest tracking |
| `GoldenAge.Inventory` | Item management |
| `GoldenAge.UI` | HUD, menus, popups |
| `GoldenAge.NPC` | NPC controllers |
| `GoldenAge.Environment` | Pickups, interactables |
| `GoldenAge.Tutorial` | Onboarding |
| `GoldenAge.Editor` | Editor-only tools |
| `GoldenAge.Utilities` | Debug commands |

### Key Interfaces

- `IDamageable` - Implement for anything that can receive damage
- `IInteractable` - Implement for E-key interaction targets

### ScriptableObject Data

All configurable game data lives in `Assets/_Project/Data/`:
- `AttackData` - Damage, cooldowns, VFX references
- `CharacterData` - Stats templates
- `ItemData` - Item definitions with effects
- `DialogueData` - Conversation trees
- `QuestData` - Objectives and rewards

### Combat System

Fusion combo: Tesla (Q) + Ether (R) within 3 seconds triggers Fusion Blast. Tracked via `PlayerCombat.teslaHitRecently` and `etherHitRecently` flags.

Damage formula: `Max(1, baseDamage * (1 + levelBonus) - defense)`

## Unity Project Structure

```
unity/Assets/_Project/
├── Scripts/         # All C# code (60+ files)
│   ├── Core/        # Managers, singletons
│   ├── Player/      # Player components
│   ├── Combat/      # Combat logic
│   ├── Dialogue/    # Dialogue system
│   ├── Quest/       # Quest system
│   ├── Inventory/   # Inventory system
│   ├── UI/          # UI components
│   ├── NPC/         # NPC controllers
│   ├── Environment/ # Pickups, interactables
│   ├── Tutorial/    # Tutorial system
│   ├── Editor/      # Editor-only tools
│   └── Utilities/   # Debug commands
├── Data/            # ScriptableObjects
├── Prefabs/         # Reusable prefabs
└── Scenes/          # Game scenes
```

## Editor Tools

Access via Unity menu **GoldenAge >**:
- **Create All Default Data** - Generate ScriptableObject instances
- **Scene Setup** - Create standard scene hierarchy
- **Prefab Creator** - Generate Player/Enemy/NPC prefabs
- **Material Creator** - Generate proxy materials for testing

## Documentation

Planning documents in `docs/` folder (Korean):
- `골든에이지_TDD_프로토타입.md` - Technical Design Document
- `골든에이지_시스템명세_프로토타입.md` - System Specifications
- `골든에이지_레벨디자인_프로토타입.md` - Level Design
- `골든에이지_애셋리스트_프로토타입.md` - Asset List
- `에셋_수집_가이드.md` - Asset Collection Guide

## Controls

| Key | Action |
|-----|--------|
| WASD | Movement |
| Shift | Sprint |
| Left Click | Melee Attack |
| Q | Tesla Shock (Skill 1) |
| R | Ether Wave (Skill 2) |
| E | Interact |
| ESC | Pause |
| I | Inventory |

## Required Unity Packages

- Input System (New)
- Cinemachine
- TextMeshPro
- AI Navigation (NavMesh)

## Adding New Systems

1. Create class inheriting from `Singleton<T>` if it's a manager
2. Place in appropriate namespace under `Scripts/`
3. For data-driven content, create ScriptableObject in `Data/`
4. Hook into `GameManager` state changes if needed via events
