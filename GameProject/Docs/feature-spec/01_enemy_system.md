# EnemySystem 아키텍처

> 이 문서는 몬스터 턴이 어떻게 판단되고 실행되는지, 그리고 `EnemySystem`이 협력하는 하위 요소(`EnemyTurnGA`/`AttackHeroGA`/`Enemy`/`EnemyAction`/`EnemyView`)가 각각 무엇을 책임지는지를 정리한 참고 문서다. `ActionSystem`(Performer/Reaction/AddReaction) 자체의 동작 원리는 다루지 않는다 — 그건 [00_action_architecture.md](./00_action_architecture.md)를 먼저 본다. 개별 몬스터의 스탯·수치 밸런싱도 이 문서 범위 밖이다.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/1.Systems/EnemySystem.cs` | 몬스터 턴 오케스트레이터(Singleton) — GA 2종의 Performer + `TurnGA` Reaction 2종 소유 |
| `Assets/Game/0_Scripts/2.GameActions/EnemyTurnGA.cs` | "몬스터 1마리의 턴" 데이터 (payload: `EnemyView`) |
| `Assets/Game/0_Scripts/2.GameActions/AttackHeroGA.cs` | "몬스터 → 영웅 공격" 데이터 (범위 or 단일 좌표 + 피해량 + 시전자) |
| `Assets/Game/0_Scripts/Models/Enemy.cs` | 몬스터 AI 두뇌 (추상) — 판단(`JudgeActAction`)·재검증(`PerformAction`) 계약 |
| `Assets/Game/0_Scripts/Models/EnemyAction.cs` | 몬스터가 수행하는 개별 행동 (추상) — 연출(`PlayEnemyAction`) 계약 |
| `Assets/Game/0_Scripts/3.Views/EnemyView.cs` | 몬스터 개체의 런타임 상태(MovePoint, NextAction) + 예고 UI |
| `Assets/Game/0_Scripts/1.Systems/TurnSystem.cs` | (외부 의존) 몬스터 턴 진입점을 소유 — `EnemySystem`은 `TurnGA(Enemy)`의 Performer를 갖지 않는다 |

구현 예시로 인용하는 구체 클래스: `Assets/Game/0_Scripts/Enemies/Larva.cs`(`Enemy` 구현), `Assets/Game/0_Scripts/Enemies/EnemyActions/AttackEA.cs`(`EnemyAction` 구현).

## 1. 한 줄 개요

`EnemySystem`은 몬스터 목록을 직접 들고 있지 않고(`TokenSystem.EnemyViews`에 위임) AI 판단 로직도 갖지 않는 **얇은 오케스트레이터**다. "무엇을 할지" 판단은 각 `EnemyView`가 들고 있는 `Enemy`/`EnemyAction` 다형성 객체에 위임하고, `EnemySystem`은 그 판단을 **한 턴 앞서 예약**해뒀다가(telegraph) 다음 몬스터 턴에 순서대로 실행·중계하는 역할만 한다.

## 2. EnemySystem과 4개 협력 요소

```csharp
// EnemySystem.cs
private void OnEnable()
{
    ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnGAPerformer);
    ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroGAPerformer);
    ActionSystem.SubscribeReaction<TurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    ActionSystem.SubscribeReaction<TurnGA>(BattleStartPostReaction, ReactionTiming.POST);
}
```

`EnemySystem`은 `TurnGA` 자체의 Performer가 아니라 **POST 리액션 2개**만 구독한다 — 턴 전환 자체(Performer)는 `TurnSystem` 소유이고, `EnemySystem`은 거기에 "몬스터 다음 행동 예약"이라는 자기 로직만 얹는다(`00_action_architecture.md` §2-3의 N:1 Reaction 패턴과 동일).

### 2-1. EnemyTurnGA — "몬스터 1마리의 턴" 데이터

```csharp
public class EnemyTurnGA : GameAction
{
    public EnemyView EnemyView { get; private set; }
}
```

필드 하나짜리 순수 데이터. `EnemySystem.PlayEnemyTurnPerformer`가 살아있는 몬스터 수만큼 생성해 큐잉한다.

### 2-2. AttackHeroGA — "몬스터 → 영웅 공격" 데이터

```csharp
public class AttackHeroGA : GameAction, IHaveCaster
{
    public Token Caster { get; private set; }
    public float DamageAmount { get; private set; }
    public List<Vector2Int> AttackArea { get; private set; }   // 범위 공격용
    public Vector2Int AttackPosition { get; private set; }     // 단일 타겟 공격용
}
```

생성자가 2개(범위/단일)라 `AttackArea`와 `AttackPosition` 중 실제로는 하나만 채워진다 — Performer(2-2절 하단)도 `AttackArea != null` 분기로 구분해서 처리한다.

### 2-3. Enemy — AI 두뇌 (추상)

```csharp
public abstract class Enemy
{
    public abstract EnemyAction JudgeActAction(EnemyView enemy);              // 다음 할 행동 미리 판단
    public abstract bool PerformAction(EnemyView enemy, EnemyAction nextAction); // 실행 직전 재검증
    public abstract Enemy Clone();
}
```

- `JudgeActAction`: **판단 시점**(턴 종료/전투 시작)에 호출. "다음 턴에 뭘 할지"만 정하고 아직 실행하지 않는다.
- `PerformAction`: **실행 시점**(자기 턴)에 호출. 예약해둔 행동이 지금도 유효한지 재검증하고, 필요하면 `EnemyAction`에 타겟 좌표 등을 채워 넣는다. `false`를 반환하면 그 몬스터는 이번 턴에 아무 것도 하지 않는다.
- `Clone()`: `EnemyView.SetUp`이 `EnemyData.Enemy.Clone()`으로 호출 — 몬스터 스폰마다 AI 인스턴스를 복제한다.

### 2-4. EnemyAction — 개별 행동 (추상)

```csharp
public abstract class EnemyAction
{
    public List<Vector2Int> targetRange { get; set; }
    public Vector2Int targetPosition { get; set; }
    public abstract Sequence PlayEnemyAction(EnemyView enemy);   // 연출 + 후속 GA 체이닝
    public abstract EnemyAction Clone();
}
```

`targetRange`/`targetPosition`은 `Enemy.PerformAction`이 실행 직전에 채워주는 값이다(주석: "Enemy에서 PlayEnemyAction 실행 전에 받음"). `PlayEnemyAction`은 자기 연출(Tween/Sequence)을 재생하면서 그 안에서 `ActionSystem.Instance.AddReaction`으로 `AttackHeroGA` 등 실제 효과 GA를 끼워 넣는 것이 일반적 패턴이다(5절 예시 참고).

### 2-5. EnemyView — 개체별 런타임 상태 + 예고 UI

```csharp
public EnemyAction NextAction
{
    get => nextAction;
    private set { nextAction = value; UpdateNextActionUI(); }   // 아이콘/텍스트 즉시 갱신
}
public int CurrentMovePoint { get; private set; }
```

`SetNextAction`으로 값이 바뀌는 순간 `UpdateNextActionUI()`가 실행돼 몬스터 머리 위 예고 아이콘/텍스트가 갱신된다 — 즉 "판단"과 "플레이어에게 보여주기"가 `EnemyView` 안에서 자동으로 묶여 있다.

## 3. 턴 흐름 (트리거부터 끝까지)

```
[전투 시작] TurnGA(StartBattle) POST
 └ EnemySystem.BattleStartPostReaction
    └ 모든 EnemyView: Enemy.JudgeActAction() → SetNextAction (최초 예고 아이콘 표시)

[몬스터 턴] TurnSystem.TurnGAPerformer가 TurnType.Enemy 분기 진입
 └ 턴 팝업 연출 대기 후 yield return EnemySystem.Instance.PlayEnemyTurnPerformer()
    1. 1초 대기 + 로그("몬스터s턴 시작")
    2. EnemysTurnGAPreReaction() 직접 호출 (Reaction 아님, 그냥 private 메서드)
       └ 모든 EnemyView: ResetMovePoint(), ReduceSEWhenMyTurnStart() (방어막 스택 제거)
    3. Enemise(=TokenSystem.EnemyViews) 순회하며 EnemyTurnGA 생성 → AddReaction
       └ 지금 시점은 TurnGA(Enemy)의 Perform 단계 중이라, `reactions`가 TurnGA.PerformReactions를
         가리키고 있음(00_action_architecture.md §3) → 큐잉된 EnemyTurnGA들이 전부 거기 쌓인다
    (PlayEnemyTurnPerformer 종료 → TurnGAPerformer 종료 → PerformPerformer(TurnGA) 종료)

 └ ActionSystem.Flow(TurnGA) 계속 진행 → PerformReactions() 호출
    └ 방금 쌓인 EnemyTurnGA들을 리스트 순서대로 하나씩 재귀 Flow() 처리
      **= 몬스터는 병렬이 아니라 완전 순차 처리** (한 마리 Pre/Perform/Post가 끝나야 다음으로)

    [몬스터 1마리분] EnemyTurnGAPerformer
     └ enemy.NextAction이 있으면:
        a) enemy.Enemy.PerformAction(enemy, nextAction) — 실행 직전 재검증
        b) true면 nextAction.PlayEnemyAction(enemy) → Sequence 반환, WaitForCompletion
           (그 안에서 AddReaction한 후속 GA가 있으면 이 EnemyTurnGA의 PerformReactions로 들어가
            이 몬스터 차례 안에서 함께 처리된다 — 예: AttackHeroGA)

    [AttackHeroGA 발생 시] AttackHeroGAPerformer
     └ AttackArea(범위) 또는 AttackPosition(단일)에서 TokenSystem.API.GetTokenByPosition으로
       HeroView만 걸러 타겟 목록 구성 → 있으면 DealDamageGA를 AddReaction
       (실제 피해 적용은 DealDamageGA/DamageSystem 영역, 이 문서 범위 밖)

 └ 전부 끝나면 TurnGA(Enemy) 자신의 POST 단계 (전역 구독자, 동기 실행)
    ├ EnemySystem.EnemyTurnPostReaction
    │  └ 모든 EnemyView: ReduceSEWhenMyTurnEnd()(취약/약화 감소)
    │     + Enemy.JudgeActAction() → SetNextAction ("다음 몬스터 턴"용 예고 갱신)
    └ TurnSystem.TurnGAPostReaction
       └ TurnGA(Player) 생성 → AddReaction (다음 턴 체이닝)
```

## 4. 핵심 설계: 판단(Judge)과 실행(Perform)의 1턴 시차

이 시스템에서 몬스터가 "이번 턴에 뭘 할지"는 **이번 턴에 정해지지 않는다.** 최초 1회는 전투 시작 시(`BattleStartPostReaction`), 이후로는 매번 **직전 몬스터 턴이 끝나는 시점**(`EnemyTurnPostReaction`)에 다음 몬스터 턴 몫을 미리 계산해 `SetNextAction`으로 박아둔다. `EnemyView.NextAction`의 setter가 UI 갱신을 동반하므로, 이 예약은 곧바로 플레이어에게 "다음에 뭘 당할지" 아이콘으로 노출된다 — 플레이어 턴 내내 그 정보를 보고 대응을 결정할 수 있게 하려는 의도로 읽힌다.

같은 몬스터 턴 안에서는 그 예약이 `Enemy.PerformAction`으로 한 번 더 재검증된 뒤에야 실제로 실행된다 — "예고 시점에는 유효했지만 실행 시점엔 무효화된" 상황(대상 사망/이탈 등)을 걸러내기 위한 지점으로 보인다(단, 5절 참고: 현재 유일한 구현체는 이 재검증을 실제로 하지 않는다).

## 5. 구체 예시로 보는 한 사이클 — Larva + AttackEA

```csharp
// Larva.cs — 현재 유일한 Enemy 구현체
public override EnemyAction JudgeActAction(EnemyView enemy)
    => GetEnemyAction(enemy, typeof(AttackEA));   // 무조건 공격만 예약

public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
    => true;   // 사거리·대상 탐색 로직은 전부 주석 처리된 프로토타입 상태
```

```csharp
// AttackEA.cs
public override Sequence PlayEnemyAction(EnemyView enemy)
{
    ...
    AttackHeroGA attackHeroGA = singleTarget
        ? new(enemy, Damage_Amount, targetPosition)
        : new(enemy, Damage_Amount, targetRange);
    ActionSystem.Instance.AddReaction(attackHeroGA);          // 실제 공격 판정

    DOAnimationGA animationGA = new(backTween);
    ActionSystem.Instance.AddReaction(animationGA);           // 복귀 연출

    return squ;   // 공격 이동 Tween — EnemyTurnGAPerformer가 이 Sequence 완료를 기다림
}
```

한 사이클: `Larva.JudgeActAction`이 `AttackEA`를 예약 → UI에 공격 아이콘 표시 → 몬스터 턴이 오면 `Larva.PerformAction`(항상 통과) → `AttackEA.PlayEnemyAction`이 공격 이동 연출을 재생하며 그 안에서 `AttackHeroGA`(실제 피해 판정용)와 `DOAnimationGA`(복귀 연출)를 같은 `EnemyTurnGA`의 Perform 단계에 추가로 끼워 넣음 → `AttackHeroGAPerformer`가 좌표로 `HeroView`를 찾아 `DealDamageGA`를 발생.

## 6. 주의사항 / 함정

- **`EnemysTurnGAPreReaction`은 이름과 달리 `ActionSystem` 리액션이 아니다.** `SubscribeReaction`에 등록된 적이 없고, `PlayEnemyTurnPerformer` 안에서 그냥 직접 호출되는 private 메서드다(`EnemySystem.cs:34`, 정의는 `EnemySystem.cs:128`). 네이밍만 보고 리액션 타이밍(PRE/POST)을 추정하면 안 된다.
- **`EnemySystem`은 `TurnGA(Enemy)`의 Performer를 갖지 않는다.** 몬스터 턴의 실제 진입점은 `TurnSystem.TurnGAPerformer`(`TurnSystem.cs:38-44`)이고, `EnemySystem.PlayEnemyTurnPerformer`는 거기서 직접 호출되는 public 코루틴일 뿐이다 — "몬스터 턴이 언제 시작되는가"는 이 파일만 봐서는 알 수 없다.
- **몬스터 처리는 병렬이 아니라 완전 순차다**(3절 참고) — 몬스터 수만큼 연출 대기 시간이 선형으로 늘어난다. 몹이 많은 웨이브의 턴 소요 시간을 추정할 때 고려해야 한다.
- **`Enemy.PerformAction`의 재검증 로직이 현재 사실상 없다.** 유일한 구현체 `Larva.PerformAction`(`Larva.cs:15-42`)은 사거리 계산·대상 탐색 코드가 전부 주석 처리되어 있고 무조건 `true`만 반환한다 — 대상이 죽었거나 범위를 벗어난 경우를 실전에서 걸러내지 못하는 프로토타입 상태.
- **`Larva.Clone()`이 `new Larva()`가 아니라 `new Scarecrow()`를 반환한다**(`Larva.cs:46`). `EnemyView.SetUp`(`EnemyView.cs:47`)이 스폰마다 `Enemy.Clone()`을 호출하는 구조라, 이 상태로는 라바로 스폰된 개체가 스캐어크로우 AI로 판단하게 된다. 의도적인 것인지 오탈자인지는 이 파일들만으로 확정할 수 없다.
- **`enemy.NextAction`이 `null`이면 `EnemyTurnGAPerformer`는 조용히 아무 것도 하지 않는다**(`EnemySystem.cs:52` 분기). `JudgeActAction`이 한 번도 호출되지 않은 몬스터(예: 전투 중 새로 소환된 경우)는 첫 턴에 그냥 넘어간 것처럼 보일 수 있다 — 소환 시점에 `SetNextAction`을 별도로 채워줄 필요가 있는지 확인 필요.

## 7. 새 몬스터/행동 추가 시 체크리스트

1. `Enemy`를 상속한 새 클래스에서 `JudgeActAction`(다음 행동 판단)과 `PerformAction`(실행 직전 재검증 — `Larva`처럼 스텁으로 남기지 않고 실제 유효성 검사를 채운다)을 구현한다.
2. `Clone()`이 **자기 자신의** 새 인스턴스를 반환하는지 확인한다 (6절의 `Larva.Clone()` 오탈자 참고).
3. 이 몬스터가 쓸 `EnemyAction` 서브클래스들을 `EnemyData.EnemyActions`에 등록한다 — `EnemyView.SetUp`이 이 리스트를 복제해 `Actions`로 보관한다(`EnemyView.cs:44-55`).
4. 새 `EnemyAction.PlayEnemyAction`은 필요한 후속 GA(`AttackHeroGA`, `DOAnimationGA` 등)를 `ActionSystem.Instance.AddReaction`으로 체이닝하고, 자신의 연출 `Sequence`를 반환해야 한다 — `EnemyTurnGAPerformer`가 그 완료를 기다린다.
5. "공격" 외의 새로운 몬스터→영웅 상호작용이 필요하면, 기존 `AttackHeroGA`를 재사용할지 새 GA + Performer(`EnemySystem` 또는 신규 시스템)를 추가할지부터 결정한다 — §2-2 계약을 먼저 확인한다.
