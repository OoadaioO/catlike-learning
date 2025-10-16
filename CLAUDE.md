# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 2022.3.62f1 project containing tutorials from catlikecoding.com, focusing on Unity development fundamentals, procedural generation, and game mechanics. The project is organized as a collection of tutorial implementations following the catlikecoding tutorial series.

## Project Structure

The Assets folder contains numbered tutorial sections:
- `0_Basic/` - Unity fundamentals (Game Objects, Graphs, Mathematical Surfaces, Performance)
- `1_PseudoRandomNoise/` - Noise generation algorithms and visualization
- `2_ProceduralMeshes/` - Mesh generation and modification (Square Grid, Triangle Grid, etc.)
- `3_PseudoRandomSurfaces/` - Surface generation using noise
- `4_Prototypes/` - Game prototypes and mechanics
- `6_TowerDefense/` - Complete tower defense game implementation

Each tutorial folder contains subfolders for individual lessons, with Scripts, Materials, Prefabs, and Scene files as needed.

## Development Commands

Unity projects don't have traditional build/test commands. Development is done through the Unity Editor:

- **Open Project**: Launch Unity Hub and open this project folder
- **Run Scene**: Press Play button in Unity Editor or Ctrl+P (Windows)/Cmd+P (Mac)
- **Build**: File → Build Settings → Build (or Ctrl+Shift+B / Cmd+Shift+B)

## Code Architecture

The codebase follows Unity's component-based architecture:

1. **MonoBehaviour Scripts**: Located in Scripts folders within each tutorial, these components attach to GameObjects
2. **Editor Scripts**: Found in Editor subfolders, provide custom inspector functionality
3. **ScriptableObjects**: Used for configuration data (materials, settings, game balance)
4. **Namespace Organization**: Scripts use appropriate namespaces matching their tutorial section

Key architectural patterns observed:
- Factory pattern for enemy spawning (TowerDefense)
- Collection classes for managing groups of objects
- Configuration classes separate from behavior logic
- Animation systems using Unity's Animator component

## Unity-Specific Considerations

- Uses Universal Render Pipeline (URP) 14.0.12
- TextMeshPro for UI text rendering
- NaughtyAttributes package for enhanced inspector functionality
- Collections package for performance-critical code
- Standard Unity modules for physics, audio, UI, and animation

## Working with Unity Code

When modifying Unity scripts:
1. Always check existing scripts in the same tutorial section for naming conventions
2. Follow Unity's lifecycle methods (Awake, Start, Update, etc.) appropriately
3. Use SerializeField for private variables that need inspector exposure
4. Consider performance implications in Update methods
5. Use Unity's built-in types (Vector3, Quaternion, etc.) rather than recreating them