[README.md](https://github.com/user-attachments/files/33834798/README.md)
# Unity Game Prototype

> **Status:** Early-stage prototype / paused development

A Unity game prototype focused on building a responsive player controller and exploring a **ScriptableObject-based finite state machine (FSM)** for organizing player behavior.

The project did not progress into a complete game. Most of the work went into the player-control foundation: movement and input handling, animations, and collision interactions.

## Implemented Features

- **Player controller:** Core player controls and movement behavior.
- **ScriptableObject-based FSM:** A state-machine approach to managing player behavior and control logic.
- **Animations:** Player animations integrated with the controller.
- **Collision handling:** Collision interactions incorporated into player behavior.

## Technical Focus

### ScriptableObject-Based Finite State Machine

The main architectural experiment in this project was using Unity **ScriptableObjects** to represent and organize player states. This approach provided a way to separate player behavior into distinct states rather than placing all control logic in one large script.

### Player Movement and Feedback

Most of the prototype's development effort went into making the player controls feel consistent and integrating them with animations and collisions. These systems served as the foundation for gameplay that was not yet built.

## Built With

- **Unity** — Game engine
- **C#** — Gameplay scripting
- **ScriptableObjects** — State-machine architecture

## Running the Project

1. Clone or download this repository.
2. Open **Unity Hub** and choose **Add project from disk** (or the equivalent option in your version).
3. Select the project folder and open it with a compatible Unity Editor version. Check `ProjectSettings/ProjectVersion.txt` for the version used by the project.
4. Open the prototype's scene in the Unity Editor and enter **Play** mode to try the player controller.

> **Note:** This is a development prototype, not a packaged or finished game. The repository may require its original Unity version or dependencies to open correctly.

## Development Status

This project is **incomplete** and is shared primarily to demonstrate work on player-control systems, Unity animation/collision integration, and state-based architecture.

Possible areas for future development include level design, gameplay objectives, menus, and additional interactions. These are ideas, **not implemented features**.

## Purpose

This repository is part of my programming portfolio and documents an early exploration of game development in Unity—especially organizing gameplay code with a finite state machine and building a reliable player controller.
