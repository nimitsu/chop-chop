# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 (6000.0.51f1) 3D adventure/cooking game project called "Chop Chop" using the Universal Render Pipeline (URP). The project follows an event-driven architecture with extensive use of ScriptableObjects.

## Core Architecture

### Event System
The project uses a custom EventChannel ScriptableObject pattern for decoupled communication:
- `VoidEventChannelSO` - Events with no parameters
- `IntEventChannelSO`, `BoolEventChannelSO`, etc. - Typed event channels
- Events are raised via `channel.RaiseEvent()` and subscribed in `OnEnable/OnDisable`

### State Machine
Custom state machine implementation in `Assets/Scripts/StateMachine/`:
- `StateMachine` - Core state machine
- `State` - Base state class with Enter/Exit/Tick methods
- `StateTransition` - Handles transitions between states with conditions

### ScriptableObject Architecture
Heavy use of ScriptableObjects for:
- Game configuration (`GameSceneSO`, `LocationSO`)
- Runtime sets (`ActorSO`, `ItemSO`)
- Audio system (`AudioCueSO`, `AudioCueEventChannelSO`)
- Factory patterns (`SoundEmitterFactorySO`)
- Object pooling (`ComponentPoolSO`)

## Key Systems

### Scene Management
- `SceneLoader` handles scene transitions
- Scenes organized into: Locations/, Managers/, Menus/
- Uses Addressables for async loading

### Dialogue System
- `DialogueManager` controls conversations
- `DialogueDataSO` stores dialogue data
- Custom UI for dialogue display

### Save System
- Located in `Assets/Scripts/SaveSystem/`
- Handles game state persistence
- Uses Unity's JsonUtility

### Input System
- Uses Unity's new Input System
- Input actions defined in `GameInput.cs`
- Event-based input handling

## Development Commands

Unity projects don't have traditional build commands. Use Unity Editor for:
- **Play Mode**: Test in editor (Ctrl/Cmd + P)
- **Build**: File → Build Settings → Build
- **Addressables**: Window → Asset Management → Addressables → Build

## Code Style Guidelines

Follow the Unity team coding standards (from global CLAUDE.md):
- **Naming**: PascalCase for classes/methods, camelCase for variables
- **Private fields**: Prefix with underscore `_fieldName`
- **Braces**: Allman style (opening brace on new line)
- **Events**: Use R3/Observable pattern when applicable

### Common Patterns

```csharp
public class ExampleBehaviour : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private VoidEventChannelSO _onPlayerDeathChannel;
    
    private void OnEnable()
    {
        _onPlayerDeathChannel.OnEventRaised += HandlePlayerDeath;
    }
    
    private void OnDisable()
    {
        _onPlayerDeathChannel.OnEventRaised -= HandlePlayerDeath;
    }
}
```

## Important Directories

- `Assets/Scripts/` - All game code
- `Assets/ScriptableObjects/` - SO instances and configurations  
- `Assets/Art/` - Visual assets and materials
- `Assets/Prefabs/` - Reusable game objects
- `Assets/Scenes/` - Game levels and managers

## Working with Addressables

This project uses Addressables for asset management:
- Asset groups in `Assets/AddressableAssetsData/`
- Load assets with `Addressables.LoadAssetAsync<T>()`
- Always release loaded assets when done

## Notes

- No automated tests exist - test in Play Mode
- Uses custom Toon shading materials extensively
- Localization system integrated (supports en, fr, it)
- Heavy use of Cinemachine for camera control