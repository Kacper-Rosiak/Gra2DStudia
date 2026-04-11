# Gra2DStudia - Project Overview

A 2D RPG game developed in Unity 6, featuring turn-based combat, player progression, and an inventory system. The codebase follows a structured MVC-like architecture with a clear separation between logic and presentation.

## Architecture & Core Systems

- **MVC Pattern**: Scripts are organized into `Model` (logic/data) and `View` (UI/presentation) subdirectories within functional modules (Combat, Player, Economy, etc.).
- **Combat System**:
  - Uses the **Strategy Pattern** for character-specific abilities (`IAbilityStrategy`).
  - Uses the **Command Pattern** for executing combat actions (`ICombatCommand`).
  - Managed by `CombatManager`, which handles turns, initiative, and state transitions via C# events.
- **Player System**: Stats-driven player model (`PlayerStats`) that integrates with the combat system.
- **Save System**: `SaveManager` handles persistence using JSON/binary files in the persistent data path.
- **UI System**: Modular UI controllers (Inventory, Save, Tab) managing UGUI and TextMesh Pro components.

## Technical Stack

- **Engine**: Unity 6 (Version 6000.3.10f1).
- **Rendering**: Universal Render Pipeline (URP) for 2D.
- **Input**: Unity New Input System.
- **Packages**: Cinemachine, TextMesh Pro, 2D Animation, Aseprite Importer.

## Development Conventions

- **Naming Standards**:
  - **Classes/Public Members**: `PascalCase`.
  - **Private Fields**: `_camelCase` (e.g., `_initiativeQueue`).
- **Decoupling**: Extensive use of Interfaces (`ICombatEntity`, `ICombatCommand`) and C# Actions/Events for communication between systems to maintain loose coupling.
- **Assets**: Sprites and animations are often imported from Aseprite.

## Building and Running

- **Unity Editor**: Open the project folder with Unity 6000.3.10f1+.
- **Main Scenes**:
  - `Assets/MenuGlowne.unity`: Main entry point for the game.
  - `Assets/Scenes/CampScene.unity`: Gameplay hub/testing area.
- **Testing**: `CombatConsoleTester.cs` is used for debugging combat logic without full UI integration.

## Key Directories

- `Assets/Scripts/Combat`: Turn-based logic, commands, and ability strategies.
- `Assets/Scripts/Player`: Player stats, class data, and player entity logic.
- `Assets/Scripts/Menu`: UI controllers for inventory, saving, and navigation.
- `Assets/Prefabs`: Reusable game objects for entities, UI, and environment.
