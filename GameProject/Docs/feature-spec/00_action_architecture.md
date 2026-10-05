# ActionSystem 아키텍처

> 이 문서는 이 프로젝트의 이벤트 기반 커맨드 패턴(`ActionSystem`)이 실제로 어떻게 동작하는지, 그리고 새 게임 로직을 추가할 때 무엇을(Performer/Reaction/AddReaction) 언제 써야 하는지를 정리한 참고 문서다. 앞으로 이 흐름 안에서 개발할 때 이 문서를 먼저 참고한다.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/General/ActionSystem/ActionSystem.cs` | 실행 엔진 본체 (Flow, 등록 API) |
| `Assets/Game/0_Scripts/General/ActionSystem/GameAction.cs` | 모든 액션의 데이터 베이스 클래스 |
| `Assets/Game/0_Scripts/General/ActionSystem/ReactionTiming.cs` | 구독 타이밍 enum (`PRE`, `POST`) |

## 1. 한 줄 개요

모든 상태 변화는 `GameAction`(순수 데이터)으로 표현되고, `ActionSystem`이 이를 큐가 아닌 **단일 슬롯 락**으로 받아 **Pre → Perform → Post** 3단계로 실행한다. 각 단계마다 "실제로 상태를 바꾸는 단 하나의 처리자"(Performer)와 "부가로 반응하는 여러 구독자"(Reaction)가 별도로 실행되며, 이 실행 도중에 새 `GameAction`을 끼워 넣어(`AddReaction`) 액션 체인을 만들 수 있다.

## 2. 3가지 핵심 구성 요소

### 2-1. GameAction — 데이터

```csharp
// GameAction.cs
public abstract class GameAction
{
    public List<(GameAction, System.Action)> PreReactions { get; private set; } = new();
    public List<(GameAction, System.Action)> PerformReactions { get; private set; } = new();
    public List<(GameAction, System.Action)> PostReactions { get; private set; } = new();
}
```

- `abstract class`. 로직이나 가상 메서드는 없고, **3단계 각각에 끼워 넣을 자식 액션 리스트**만 갖는다.
- 하위 클래스는 접미사 `GA`를 붙이고, 필요한 데이터 필드 + 생성자만 자유롭게 정의한다.
  - `TurnGA` : `public TurnType Type { get; private set; }`
  - `SpendAPGA` : `public int Amount { get; set; }` (기본값 1)
  - `RefillAPGA`, `RefillMPGA` : 필드 없는 "트리거 전용" 마커 액션

### 2-2. Performer — "이 액션 타입의 상태를 실제로 바꾸는 단일 소유자"

```csharp
public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
public static void DetachPerformer<T>() where T : GameAction
```

- 내부적으로 `Dictionary<Type, Func<GameAction, IEnumerator>>` — **타입당 정확히 1개**. 같은 타입에 두 번째로 `AttachPerformer`를 호출하면 예외 없이 조용히 덮어쓴다(`ActionSystem.cs:113`).
- 등록된 Performer가 없는 타입의 액션은 Performer 단계에서 아무 일도 하지 않고 그냥 지나간다(예외 없음).
- **언제 쓰는가**: 어떤 자원/상태(AP, HP, 턴 등)를 "소유"한 시스템이, 그 자원을 바꾸는 액션 타입에 등록한다. 1:1 관계.
  - 예: `APSystem`이 `SpendAPGA`/`RefillAPGA`에 등록 → `CurrentAP` 값을 실제로 바꾸는 유일한 코드.

```csharp
// APSystem.cs
private void OnEnable()
{
    ActionSystem.AttachPerformer<SpendAPGA>(SpendAPPerformer);
    ActionSystem.AttachPerformer<RefillAPGA>(RefillAPPerformer);
}
private void OnDisable()
{
    ActionSystem.DetachPerformer<SpendAPGA>();
    ActionSystem.DetachPerformer<RefillAPGA>();
}
private IEnumerator SpendAPPerformer(SpendAPGA spendAPGA)
{
    CurrentAP -= spendAPGA.Amount;
    apUI.SpendAPUI(CurrentAP);
    yield return null;
}
```

값 검증(예: "AP가 충분한가?")은 Reaction이 아니라 **일반 public 메서드**(`HasEnoughAP()`)로 노출해서, 액션을 생성하기 *전에* 호출자가 직접 체크하는 방식을 쓴다.

### 2-3. Reaction — "이 액션이 벌어질 때 부가로 반응하고 싶다"

```csharp
public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
```

- `ReactionTiming`은 `PRE`, `POST` 단 두 값뿐이다. **Perform 시점에는 전역 구독이 불가능**하다(아래 6절 참고).
- 내부적으로 `Dictionary<Type, List<Action<GameAction>>>` — 같은 타입에 **여러 개** 등록 가능(N:1). Performer와 반대.
- 콜백 안에서 `if (조건이 아니면) return;` 식으로 필터링하는 것이 일반적 패턴이다.

**PRE 예 1 — 값 선반영**: `TurnSystem.TurnGAPreReaction`은 Performer가 실행되기 전에 `currentTurn`부터 갱신해서, 같은 Flow 안에서 나중에 실행되는 Performer/다른 Reaction들이 항상 최신 턴 값을 참조하게 만든다.

```csharp
private void TurnGAPreReaction(TurnGA turnGA)
{
    currentTurn = turnGA.Type;  //턴 타입 변경
}
```

**PRE 예 2 — 조건부 사이드이펙트**: `HeroSystem.EnemyTurnPreReaction`은 `TurnGA`의 타입이 `Enemy`일 때만(그 외엔 즉시 `return`) 적 턴이 시작되기 직전에 영웅들의 지속효과(SE)를 정리한다. `TurnGA`를 소유/처리하는 시스템은 `TurnSystem`이지만, `HeroSystem`은 그 액션에 자기 로직만 얹는다.

```csharp
// HeroSystem.cs
private void OnEnable()
{
    ActionSystem.SubscribeReaction<TurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
}
private void EnemyTurnPreReaction(TurnGA turnGA)
{
    if (turnGA.Type != TurnType.Enemy) return;
    foreach (var hero in HeroViews) hero.ReduceSEWhenMyTurnEnd();
}
```

**POST 예 — 후처리/다음 액션 트리거**: `TurnSystem.TurnGAPostReaction`은 `Enemy` 턴의 Performer와 그 하위 반응까지 전부 끝난 뒤에 실행되어, 다음 `Player` 턴 액션을 자동으로 체이닝한다.

```csharp
private void TurnGAPostReaction(TurnGA turnGA)
{
    if (turnGA.Type == TurnType.Enemy)
    {
        TurnGA playerTurnGA = new(TurnType.Player);
        ActionSystem.Instance.AddReaction(playerTurnGA);
    }
}
```

### 2-4. AddReaction — 현재 실행 트리에 새 액션 끼워넣기(체이닝/중첩)

```csharp
public void AddReaction(GameAction gameAction, System.Action OnReactionFinished = null)
```

- "지금 진행 중인 단계"가 가리키는 공유 리스트(`reactions` 필드)에 새 액션을 추가한다. 그 단계의 `PerformReactions()`가 리스트를 순회하며 각 항목을 **재귀적으로 `Flow()`**에 넘기므로, 새로 추가된 액션도 자신만의 Pre → Perform → Post 3단계를 완전히 거친다.
- `Perform()`과의 차이: `Perform()`은 `IsPerforming` 락을 새로 거는 **최상위 진입점**이고, `AddReaction()`은 이미 진행 중인 Flow 안에 끼워 넣는 것이라 **락을 걸지 않는다**. 즉 Performer나 Reaction 콜백 내부에서는 `Perform()`이 아니라 `AddReaction()`을 쓴다.
- 예: `HeroSystem.PlayHeroTurnPerformer`가 AP/MP 리필 액션을 체이닝:

```csharp
public IEnumerator PlayHeroTurnPerformer()
{
    ...
    RefillAPGA refillAPGA = new();
    ActionSystem.Instance.AddReaction(refillAPGA);

    RefillMPGA refillMPGA = new();
    ActionSystem.Instance.AddReaction(refillMPGA);
    yield return null;
}
```

### 2-5. Perform — 최상위 진입점

```csharp
public void Perform(GameAction action, System.Action OnPerformFinished = null)
```

- `IsPerforming == true`면 **조용히 무시**하고 반환한다(`ActionSystem.cs:27`). 대기 큐도 예외도 없는 **단일 슬롯 락**이다.
- 호출자가 직접 `ActionSystem.Instance.IsPerforming`을 체크해서 중복 입력을 막아야 한다. 실제로 `MoveSystem`, `SkillSystem`이 입력 처리 전에 이 플래그를 가드로 쓴다.
- **용도**: 유저 입력 등 완전히 새로운 행동이 시작되는 최초 트리거 지점에서만 호출한다. Performer/Reaction 콜백 내부에서는 쓰지 않는다(대신 `AddReaction`).

## 3. 실행 흐름 (Flow)

`Perform(action)`을 호출하면 다음 순서로 정확히 실행된다 (`ActionSystem.cs:41~65`):

```
Perform(action)
 └ IsPerforming = true
 └ Flow(action):
     1. reactions = action.PreReactions
     2. PerformSubscribers(preSubs)        ← 전역 PRE 구독자, 동기 실행(코루틴 아님)
     3. PerformReactions()                 ← PreReactions 리스트를 재귀 Flow() (AddReaction으로 쌓인 것)
     4. reactions = action.PerformReactions
     5. PerformPerformer(action)           ← 등록된 단일 Performer 코루틴 실행 (실제 상태 변경)
     6. PerformReactions()                 ← PerformReactions 리스트 재귀 Flow() (Performer 안에서 AddReaction한 것)
     7. reactions = action.PostReactions
     8. PerformSubscribers(postSubs)       ← 전역 POST 구독자, 동기 실행
     9. PerformReactions()                 ← PostReactions 리스트 재귀 Flow()
 └ IsPerforming = false
 └ OnPerformFinished?.Invoke()
```

즉 한 액션당 순서는 **`PRE 구독자 → PRE 중첩액션들 → Performer → Perform 중첩액션들 → POST 구독자 → POST 중첩액션들`** 이다.

- `PerformSubscribers`만 유일하게 동기(즉시 실행) 메서드이고, 나머지(`Flow`, `PerformPerformer`, `PerformReactions`)는 모두 `IEnumerator` 코루틴이다. async/await은 쓰지 않는다.
- 각 단계 진입 전에 `yield return WaitUntilEndPause`가 있어 `ActionSystem.Instance.SetPause(true)`로 전역 일시정지를 걸면 단계 사이에서 대기한다.
- **취소(Cancel) 메커니즘은 없다.** 한 번 시작된 `Flow`는 끝까지 실행된다.

## 4. 종합 예시 — `TurnGA` (Performer + PreReaction + PostReaction 동시 사용)

```csharp
// TurnSystem.cs
protected void OnEnable()
{
    ActionSystem.AttachPerformer<TurnGA>(TurnGAPerformer);
    ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
}
private void OnDisable()
{
    ActionSystem.DetachPerformer<TurnGA>();
    ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
}

private IEnumerator TurnGAPerformer(TurnGA turnGA)
{
    if (turnGA.Type == TurnType.StartBattle)
    {
        TurnGA playerTurnGA = new(TurnType.Player);
        ActionSystem.Instance.AddReaction(playerTurnGA);
    }
    else if (turnGA.Type == TurnType.Enemy)
    {
        ...연출 재생...
    }
    else if (turnGA.Type == TurnType.Player)
    {
        ...연출 재생...
        yield return HeroSystem.Instance.PlayHeroTurnPerformer();
    }
}

private void TurnGAPreReaction(TurnGA turnGA) => currentTurn = turnGA.Type;

private void TurnGAPostReaction(TurnGA turnGA)
{
    if (turnGA.Type == TurnType.Enemy)
    {
        TurnGA playerTurnGA = new(TurnType.Player);
        ActionSystem.Instance.AddReaction(playerTurnGA);
    }
}
```

세 API의 역할이 정확히 나뉜다:

- **PreReaction** — Performer가 실행되기 전에 `currentTurn` 값을 먼저 갱신한다. 이렇게 해야 같은 Flow 안에서 이후에 실행되는 로직들이 항상 최신 턴을 본다.
- **Performer** — `TurnGA`가 소유한 실제 로직: 턴 타입에 따라 분기하고, 연출을 재생하고, `Player` 턴이면 `HeroSystem`에 실제 턴 처리를 위임한다(`Enemy` 턴은 연출만 재생). `StartBattle` 처리 중에는 다음 `Player` 턴을 즉시 체이닝한다.
- **PostReaction** — Performer와 그 하위 반응까지 전부 끝난 뒤 실행되어, `Enemy` 턴이 끝나면 자동으로 `Player` 턴을 체이닝한다(순환 구조).

**주의**: `HeroSystem.PlayHeroTurnPerformer`는 이름에 "Performer"가 들어가지만 `ActionSystem.AttachPerformer`로 등록된 것이 **아니다**. `TurnGAPerformer` 내부에서 `yield return`으로 직접 위임 호출되는 일반 코루틴 헬퍼일 뿐이며, 등록된 딕셔너리 어디에도 존재하지 않는다.

## 5. Performer vs Reaction 선택 기준

| 판단 질문 | Performer | Reaction (PRE/POST) |
|---|---|---|
| 이 액션 타입의 상태를 누가 실제로 바꾸는가? (1:1 소유) | ✅ | |
| 여러 시스템이 동시에 반응해야 하는가? (N:1) | | ✅ |
| 조건부로 일부 케이스만 반응해야 하는가? | | ✅ `if (...) return;` |
| Performer 실행 전에 값을 선반영해야 하는가? | | ✅ PRE |
| Performer + 그 하위 반응까지 다 끝난 뒤 후처리/다음 액션 트리거가 필요한가? | | ✅ POST |
| 새로운 `GameAction`을 유발해야 하는가? | `AddReaction` (Performer/Reaction 어디서든 호출 가능) | 동일 |

## 6. 주의사항 / 함정

- **`Perform()`은 큐가 아니라 락이다.** 호출 전 반드시 `ActionSystem.Instance.IsPerforming`을 체크해서 중복 호출을 막아야 한다(`MoveSystem`, `SkillSystem` 패턴 참고).
- **취소(Cancel) 메커니즘이 없다.** 한 번 시작한 `Flow` 코루틴은 중간에 멈출 수 없다.
- **Performer는 타입당 1개뿐이다.** 같은 타입에 두 번째로 `AttachPerformer`를 호출하면 예외 없이 조용히 덮어써진다 — 두 시스템이 같은 액션 타입의 Performer를 등록하려 하면 버그가 소리 없이 발생한다.
- `preSubs`/`postSubs`/`performers`는 `static`이다. `OnEnable`/`OnDisable`에서 정확히 대칭으로 등록/해제하지 않으면 씬을 재진입할 때 구독이 중복 누적된다.
- **`PerformReactions` 단계(Perform 시점)에는 전역 `SubscribeReaction`을 걸 수 없다.** `ReactionTiming`은 `PRE`/`POST`뿐이다. Perform 시점에 반응이 필요하면, 그 순간 해당 액션 인스턴스에 개별적으로 `AddReaction`을 호출해야 한다(전역 구독 불가, 인스턴스 단위로만 가능).
- Reaction 콜백을 순회하는 도중 같은 타입/타이밍에 대해 `UnsubscribeReaction`을 호출해도 안전하다 — 즉시 제거되지 않고 `willRemoveSubs`에 쌓여 순회가 끝난 뒤 제거된다.

## 7. 새 GameAction 추가 시 체크리스트

1. `XxxGA : GameAction` 정의 — 필요한 데이터 필드 + 생성자만 작성한다. 로직은 넣지 않는다.
2. 이 액션의 상태를 실제로 바꿀 **단일 소유 시스템**을 정하고, 그 시스템의 `OnEnable`/`OnDisable`에 `AttachPerformer<XxxGA>`/`DetachPerformer<XxxGA>`를 대칭으로 등록한다.
3. 다른 시스템이 부가 반응이 필요하면, 그 시스템의 `OnEnable`/`OnDisable`에 `SubscribeReaction<XxxGA>`/`UnsubscribeReaction<XxxGA>`(PRE 또는 POST)를 대칭으로 등록한다.
4. 이 로직이 다른 액션을 유발해야 하면, 처리 코드 안에서 `new XxxGA(...)` 생성 후 `ActionSystem.Instance.AddReaction(...)`을 호출한다.
5. 유저 입력 등 최초 트리거 지점에서 `IsPerforming`을 체크한 뒤 `ActionSystem.Instance.Perform(new XxxGA(...))`를 호출한다.
