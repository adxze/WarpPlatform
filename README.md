## Developer & Contributions

adxze (Game Developer & Systems Designer)
  <br>

## About

WarpPlatform is a 2D platformer where you unlock new movement abilities and navigate levels using teleportation mechanics. Players progress through stages by mastering abilities like double jumping, wall jumping, dashing, and sliding while using strategic teleportation between portals to solve platforming challenges.
<br>

## Key Features

**Physics-Based Movement**: Fast paced movement system with proper momentum, gravity, and force calculations that create smooth and responsive character control.

**Teleportation Mechanics**: the unique J-key teleportation system to warp between active portals, including moving teleporters that add dynamic puzzle elements.


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
## Layer / module Design 

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


## Modules and Features

The advanced 2D platformer mechanics including progressive ability unlocks, teleportation system, level management, and dynamic audio are powered by a comprehensive scripting system that creates a unique gameplay experience.

| 📂 Name | 🎬 Scene | 📋 Responsibility |
|---------|----------|-------------------|
| **MainMenu** | **Main Menu** | - Show main menu UI<br/>- Load gameplay scene when player click play button<br/>- Exit game when player exit the game |
| **Setting** | **Main Menu**<br/>**Gameplay** | - Show setting menu (UI)<br/>- Set audio settings<br/>- Configure game preferences |
| **Audio** | **Main Menu**<br/>**Gameplay** | - Play audio (BGM & SFX)<br/>- Set audio volume<br/>- Set audio mute & unmute |
| **PlayerController** | **Gameplay** | - Move player with progressive abilities<br/>- Handle double jump, wall jump, sprint, dash, slide<br/>- Process player input and physics |
| **TeleportationSystem** | **Gameplay** | - Handle teleportation between portals using J key<br/>- Manage teleporter activation and cooldowns<br/>- Support moving teleporters with waypoints |
| **LevelManager** | **Gameplay** | - Manage level progression and boundaries<br/>- Handle camera boundaries and transitions<br/>- Activate teleporters based on context |
| **GameplayMenu** | **Gameplay** | - Handle and show pause game<br/>- Go to main menu when user click main menu button<br/>- Exit game when player click exit game |
| **GameOver** | **Gameplay** | - Show game over panel<br/>- Handle retry & return to main menu<br/>- Manage end screen transitions |
| **PowerUpSystem** | **Gameplay** | - Store and manage power up data available in game<br/>- Spawn power ups throughout levels<br/>- Detect power up triggers and apply effects |
| **PhysicsSystem** | **Gameplay** | - Handle directional boost pads<br/>- Apply physics-based movement forces<br/>- Manage collision detection and responses |
| **SaveSystem** | **Persistent** | - Store setting data and game progress<br/>- Save and load setting data<br/>- Enable progress persistence across sessions |


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





<br>

## Play The Game

<a href="#">Play Now</a>
<br>




![Platform Demo](https://raw.githubusercontent.com/adxze/adxze/main/PlatfromSlide.png)
