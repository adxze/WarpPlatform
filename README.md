<table>
  <tr>
    <td align="left" width="50%">
      <img width="100%" alt="gif1" src="https://github.com/adxze/adxze/blob/main/0604(2).gif">
    </td>
    <td align="right" width="50%">
      <img width="100%" alt="gif2" src="https://github.com/adxze/adxze/blob/main/0604(3).gif">
    </td>
  </tr>
</table>

## Scene Flow 

```mermaid
flowchart LR
  mm[Main Menu]
  gp[Gameplay]
  es[End Screen]

  mm -- "Click Play" --> gp
  gp -- "Game Over" --> es
  es -- "Start Over" --> gp
  es -- "Main Menu" --> mm

```
## Layer Design 

```mermaid
---
config:
  theme: neutral
  look: neo
---
graph TD
    subgraph "Game Initialization"
        Start([Game Start])
        Boot[Boot Layer]
        SaveCheck{Save Data<br/>Exists?}
    end
    subgraph "Main Menu System"
        MM[Main Menu]
        Settings[Settings Menu]
        LevelSelect[Level Select]
    end
    subgraph "Gameplay Flow"
        GP[Gameplay Scene]
        Pause[Pause Menu]
        Teleport[Teleportation System]
        AbilityUnlock[Ability Unlock]
    end
    subgraph "Level Progression"
        Level1[Level 1]
        Level2[Level 2]
        Level3[Level 3]
        MoreLevels[...]
    end
    subgraph "End States"
        GameOver[Game Over]
        LevelComplete[Level Complete]
        ES[End Screen]
    end
    Start --> Boot
    Boot --> SaveCheck
    SaveCheck -->|No Save| MM
    SaveCheck -->|Has Save| LevelSelect
    MM -->|Play| GP
    MM -->|Settings| Settings
    MM -->|Level Select| LevelSelect
    Settings --> MM
    LevelSelect --> GP
    GP --> Pause
    GP --> Teleport
    GP --> AbilityUnlock
    Pause -->|Resume| GP
    Pause -->|Main Menu| MM
    Teleport --> GP
    AbilityUnlock --> GP
    GP --> Level1
    Level1 -->|Complete| Level2
    Level2 -->|Complete| Level3
    Level3 -->|Complete| MoreLevels
    GP -->|Death/Failure| GameOver
    GP -->|Level Complete| LevelComplete
    LevelComplete -->|Continue| GP
    GameOver --> ES
    ES -->|Start Over| GP
    ES -->|Main Menu| MM
    classDef initStyle fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef menuStyle fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef gameplayStyle fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef levelStyle fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef endStyle fill:#ffebee,stroke:#b71c1c,stroke-width:2px
    class Start,Boot,SaveCheck initStyle
    class MM,Settings,LevelSelect menuStyle
    class GP,Pause,Teleport,AbilityUnlock gameplayStyle
    class Level1,Level2,Level3,MoreLevels levelStyle
    class GameOver,LevelComplete,ES endStyle

```


## 🎮Scripts and Features

The advanced 2D platformer mechanics including progressive ability unlocks, teleportation system, level management, and dynamic audio are powered by a comprehensive scripting system that creates a unique gameplay experience.

| 📂 Name | 🎬 Scene | 📋 Responsibility |
|---------|----------|-------------------|
| `PlayerController.cs` | **Gameplay** | - Advanced player movement system<br/>- Progressive ability unlock (double jump, wall jump, sprint, dash, slide)<br/>- Handle player input and physics |
| `TeleportManager.cs` | **Gameplay** | - Core teleportation system management<br/>- Allow players to warp between active teleporters using J key<br/>- Manage teleporter states and cooldowns |
| `Teleporter.cs` | **Gameplay** | - Base teleporter functionality with visual effects<br/>- Handle teleporter activation and momentum preservation<br/>- Manage teleporter cooldowns |
| `MovingTeleporter.cs` | **Gameplay** | - Dynamic teleporter that moves between waypoints<br/>- Extend base teleporter functionality<br/>- Handle moving teleporter physics |
| `GameManager.cs` | **Gameplay** | - Comprehensive level management with save system<br/>- Manage camera boundaries and teleporter activation<br/>- Handle game state transitions |
| `DirectionalBooster.cs` | **Gameplay** | - Physics-based boost pads with customizable directions<br/>- Apply force using different methods<br/>- Handle boost pad visual effects |
| `AudioManager.cs` | **Main Menu**<br/>**Gameplay** <br/> **End Credit**| - Adapt background music and SFX based on current scene<br/>- Set audio volume and mute controls<br/>- Manage audio transitions |
| `SaveSystem.cs` | **Persistent** | - Progress persistence allowing players to continue from last reached level<br/>- Store and load setting data<br/>- Handle save data validation |

<br>

## 🔴About

WarpPlatform is a 2D platformer where you unlock new movement abilities and navigate levels using teleportation mechanics. Players progress through stages by mastering abilities like double jumping, wall jumping, dashing, and sliding while using strategic teleportation between portals to solve platforming challenges.
<br>

## 👤Developer & Contributions

- adxze (Game Developer & Systems Designer)
  <br>

## Game Flow Chart


```mermaid
---
config:
  theme: redux
  look: neo
---
flowchart TD
  start([Game Start])
  start --> input{Player input}
  input -->|"Move or Jump"| move[Apply physics movement]
  move --> abil{Ability unlocked}
  abil -->|Yes| doAbility[Use ability]
  abil -->|No| cont1[Continue]
  input -->|"Teleport key J"| tpChk[Check teleporters]
  tpChk --> tpOK{Teleporter valid}
  tpOK -->|Yes| doTp[Teleport player]
  tpOK -->|No| cont2[Continue]
  move --> hitBoost{Hit booster}
  hitBoost -->|Yes| doBoost[Apply booster force]
  hitBoost -->|No| cont3[Continue]
  doAbility --> loop[Continue loop]
  doTp --> loop
  doBoost --> loop
  cont1 --> collide{End reached or hazard}
  cont2 --> collide
  cont3 --> collide
  loop --> collide
  collide -->|Hazard| respawn[Respawn at checkpoint]
  collide -->|Level end| save[Save progress]
  respawn --> start
  save --> next[Load next level]
  next --> start


```


<br>

## Event Signal Diagram


```mermaid
classDiagram
    %% --- Core Gameplay ---
    class PlayerController {
        +OnJump()
        +OnDash()
        +OnSlide()
        +OnWallJump()
        +OnAbilityUnlocked(abilityName: string)
    }

    class TeleportManager {
        +OnTeleportStart()
        +OnTeleportComplete()
    }

    class Teleporter {
        +OnPlayerEnter()
        +OnPlayerExit()
    }

    class MovingTeleporter {
        +OnReachWaypoint(index: int)
    }

    class DirectionalBooster {
        +OnBoostApplied(direction: vector2, force: float)
    }

    %% --- Systems ---
    class GameManager {
        +OnLevelStart(levelName: string)
        +OnLevelComplete(levelName: string)
        +OnPlayerDeath()
    }

    class AudioManager {
        +OnPlayBGM(trackName: string)
        +OnPlaySFX(effectName: string)
    }

    class SaveSystem {
        +OnSave(slot: int)
        +OnLoad(slot: int)
    }

    %% --- Relations (who emits what) ---
    PlayerController --> TeleportManager : emits
    PlayerController --> DirectionalBooster : triggers
    TeleportManager --> Teleporter : controls
    GameManager --> SaveSystem : calls
    GameManager --> AudioManager : triggers



```
<br>

## 🎯Game Controls

The following controls are available for gameplay and ability progression:

| Key Binding | Function                    |
| ----------- | --------------------------- |
| A/D         | Move left/right             |
| W/Space     | Jump / Double Jump*         |
| J           | Teleport to available portal|
| Shift       | Sprint*                     |
| E           | Dash*                       |
| Ctrl        | Slide*                      |
| Esc         | Pause menu                  |

**Note: Abilities marked with * must be unlocked through gameplay progression or not added yet* 

<br>

## ⚡Key Features

**Physics-Based Movement**: Realistic movement system with proper momentum, gravity, and force calculations that create smooth and responsive character control.

**Teleportation Mechanics**: Master the unique J-key teleportation system to warp between active portals, including moving teleporters that add dynamic puzzle elements.

**Level Management**: Seamless level transitions with automatic save system, dynamic camera boundaries, and context-sensitive teleporter activation.

<br>

## 🕹️Play Game

<a href="#">Play Now</a>
<br>




![Platform Demo](https://raw.githubusercontent.com/adxze/adxze/main/PlatfromSlide.png)
