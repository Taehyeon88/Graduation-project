# Core

Turn-based roguelike deck-building game. Unity 6 (6000.3.12f1). All game scripts under `Assets/Game/0_Scripts/`.

## Source Map

| Path | Role |
|------|------|
| `Assets/Game/0_Scripts/General/` | ActionSystem, Singleton<T>, Utility (BFS, Random, etc.) |
| `Assets/Game/0_Scripts/1.Systems/` | ~30 game systems (each Singleton<T>, registers Performers/Reactions on OnEnable) |
| `Assets/Game/0_Scripts/2.GameActions/` | ~25 GameAction subclasses (pure data, no logic) |
| `Assets/Game/0_Scripts/3.Views/` | Visual representations (HeroView, EnemyView, CardView, etc.) |
| `Assets/Game/0_Scripts/4.Creators/` | Factory MonoBehaviours for runtime object creation |
| `Assets/Game/0_Scripts/5.Data/` | ScriptableObject data assets (CardData, EnemyData, HeroData, RoomData, etc.) |
| `Assets/Game/0_Scripts/Effects/` | Effect subclasses — implement `GetGameAction(EffectInfo)` |
| `Assets/Game/0_Scripts/Enemies/` | Enemy AI classes + EnemyAction subclasses |
| `Assets/Game/0_Scripts/Models/` | Pure C# model classes (Card, Enemy, AoE, Dice, Perk, etc.) |
| `Assets/Game/0_Scripts/Enums/` | Enum definitions |
| `Assets/Game/0_Scripts/Interfaces/` | Interfaces (IHaveCaster, IUseCondition, IUseCustomRangeVG, etc.) |
| `Assets/Game/0_Scripts/UI/` | UI MonoBehaviour components |

## Scenes
- `GameDemoScene` — main battle scene (reloaded on level transition)
- `StartScene` — title/start screen
- `DevelopScene/` — dev-only scenes (not shipped)

## Key Invariants
- `GameSystem` is DontDestroyOnLoad; holds player HP, deck, level, game state across scenes.
- All Systems inherit `Singleton<T>` and register/unregister ActionSystem hooks in `OnEnable`/`OnDisable`.
- See `mem:conventions` for architecture patterns.
- See `mem:tech_stack` for packages and versions.
