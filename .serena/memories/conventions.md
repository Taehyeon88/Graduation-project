# Conventions

## ActionSystem Pattern (central architecture)

All game logic flows through `ActionSystem` (singleton). Every discrete action is a `GameAction` subclass (pure data, suffix `GA`).

**Execution flow per action:**
1. PRE subscribers (`SubscribeReaction<T>(..., ReactionTiming.PRE)`)
2. Performer (`AttachPerformer<T>(IEnumerator)`) — the main logic coroutine
3. POST subscribers (`SubscribeReaction<T>(..., ReactionTiming.POST)`)
4. Reactions added via `AddReaction()` during any phase execute as chained sub-flows.

**System lifecycle pattern:**
```csharp
void OnEnable()  { ActionSystem.AttachPerformer<MyGA>(MyPerformer); }
void OnDisable() { ActionSystem.DetachPerformer<MyGA>(); }
```

**Reaction chaining:**
- `ActionSystem.Instance.AddReaction(ga)` — appends to current reaction list
- `ga.PostReactions.Add((nextGA, callback))` — chains after a specific GA finishes

## Naming
- GameAction subclasses: `VerbNounGA` (e.g. `PlayCardGA`, `DealDamageGA`)
- Effect subclasses: `VerbNounEffect` (e.g. `DealDamageEffect`, `HealEffect`)
- System classes: `NounSystem` (e.g. `CardSystem`, `HeroSystem`)
- View classes: `NounView` (e.g. `HeroView`, `EnemyView`, `CardView`)
- Data (ScriptableObject): `NounData` (e.g. `CardData`, `EnemyData`)
- Enum files: one enum per file, named after the enum

## Effect Pattern
`Effect` (abstract ScriptableObject-serializable) implements `GetGameAction(EffectInfo) → GameAction`.
`PerformEffectGA` wraps an `Effect` + `EffectInfo` for deferred execution via `EffectSystem`.

## EffectInfo
Struct with 6 constructors depending on targeting context (Grid-based vs CombatantView-based vs Self).
Key fields: `targets`, `caster`, `targetPoses`, `cardType`, `cardSubType`. Unused fields are null.

## Singleton
All Systems extend `Singleton<T>`. `GameSystem` calls `DontDestroyOnLoad()` to persist across scenes.

## PlayCardGA / PlayCardEffectGA Two-Phase Convention (리팩토링 완료)
카드 실행은 두 개의 별도 GameAction으로 분리됨:
- `PlayCardGA`: 카드 제거, 마나 소비, SelfEffects 적용 (Part1)
- `PlayCardEffectGA`: GridTargetMode 이펙트 실행 (Part2, PlayCardGA.PostReactions로 체인)
`CardSystem`에 `PlayCardPerformer`와 `PlayCardEffectPerformer` 두 퍼포머가 등록됨.
