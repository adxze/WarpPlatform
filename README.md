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

## 🎮Scripts and Features

The advanced 2D platformer mechanics including progressive ability unlocks, teleportation system, level management, and dynamic audio are powered by a comprehensive scripting system that creates a unique gameplay experience.

| Script                         | Description                                                                              |
| ------------------------------ | ---------------------------------------------------------------------------------------- |
| `PlayerController.cs`          | Advanced player movement with progressive ability unlock system (double jump, wall jump, sprint, dash, slide) |
| `TeleportManager.cs`           | Core teleportation system allowing players to warp between active teleporters using F key |
| `Teleporter.cs`                | Base teleporter functionality with visual effects, cooldowns, and momentum preservation |
| `MovingTeleporter.cs`          | Dynamic teleporter that moves between waypoints, extending base teleporter functionality |
| `GameManager.cs`               | Comprehensive level management with save system, camera boundaries, and teleporter activation |
| `DirectionalBooster.cs`        | Physics-based boost pads with customizable directions and force application methods |
| `AudioManager.cs`              | adapts background music and SFX based on current scene |
| `SaveSystem.cs`                | Progress persistence allowing players to continue from their last reached level |

<br>

## 🔴About

WarpPlatform is a 2D platformer where you unlock new movement abilities and navigate levels using teleportation mechanics. Players progress through stages by mastering abilities like double jumping, wall jumping, dashing, and sliding while using strategic teleportation between portals to solve platforming challenges.
<br>

## 👤Developer & Contributions

- adxze (Game Developer & Systems Designer)
  <br>

## 📁Files description

```
├── WarpPlatform                      # Contains everything needed for WarpPlatform to work.
   ├── Assets                         # Contains every assets that have been worked with unity to create the game like the scripts and the art.
      ├── Animtion                    # Contains every animation clip and animator controller that played when the game start.
      ├── Code                        # Contains all scripts needed to make the game work like PlayerController scripts.
      ├── DataSaving                  # Contains save system scripts for progress persistence.
      ├── Font                        # Contains all fonts used for UI text rendering.
      ├── Materials                   # Contains all the material for the game.
      ├── Music&sfx                   # Contains every sound used for the game like music and sound effects.
      ├── Other                       # Contains miscellaneous assets and resources.
      ├── Scenes                      # Contains all scenes that exist in the game for it to interconnected with each other like MainMenu and Game.
      ├── Settings                    # Contains game configuration and settings files.
      ├── Sprite                      # Contains every sprites used in the game.
      ├── TextMesh Pro                # Contains TextMeshPro assets for advanced text rendering.
      ├── TileSet                     # Contains tilemap assets for level construction.
      ├── UI Toolkit                  # Contains user interface components and styling assets.
   ├── Packages                       # Contains game packages that responsible for managing external libraries and packages used in your project.
├── README.md                         # The description of WarpPlatform file from About til the developers and the contribution for this game.
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
