# EnemySystem 아키텍처

> 이 문서는 몬스터 턴이 어떻게 판단되고 실행되는지, 그리고 `EnemySystem`이 협력하는 하위 요소(`EnemyTurnGA`/`PlayEnemyEAGA`/`AttackHeroGA`/`Enemy`/`EnemyAction`/`EnemyView`)가 각각 무엇을 책임지는지를 정리한 참고 문서다. `ActionSystem`(Performer/Reaction/AddReaction) 자체의 동작 원리는 다루지 않는다 — 그건 [00_action_architecture.md](./00_action_architecture.md)를 먼저 본다. 개별 몬스터의 스탯·수치 밸런싱도 이 문서 범위 밖이다.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/1.Systems/EnemySystem.cs` | 몬스터 턴 오케스트레이터(Singleton) — GA 3종의 Performer + `TurnGA` Reaction 2종 소유 |
| `Assets/Game/0_Scripts/2.GameActions/EnemyTurnGA.cs` | "몬스터 1마리의 턴" 데이터 (payload: `EnemyView`) |
| `Assets/Game/0_Scripts/2.GameActions/PlayEnemyEAGA.cs` | "예약된 행동을 실제로 재생" 데이터 (payload: `EnemyView` + `EnemyAction`) — 이동이 끝난 뒤 공격 연출을 재생시키는 지연 트리거 |
| `Assets/Game/0_Scripts/2.GameActions/AttackHeroGA.cs` | "몬스터 → 영웅 공격" 데이터 (범위 or 단일 좌표 + 피해량 + 시전자) |
| `Assets/Game/0_Scripts/Models/Enemy.cs` | 몬스터 AI 두뇌 (추상) — 판단(`JudgeActAction`)·재검증/이동판단(`PerformAction`) 계약 |
| `Assets/Game/0_Scripts/Models/EnemyAction.cs` | 몬스터가 수행하는 개별 행동 (추상) — 연출(`PlayEnemyAction`) 계약 + **자체 타겟 탐색** 공용 헬퍼(`FindRandomHeroInRange`) |
| `Assets/Game/0_Scripts/3.Views/EnemyView.cs` | 몬스터 개체의 런타임 상태(`MovePoint`/`CurrentMovePoint`, `NextAction`) + 예고 UI |
| `Assets/Game/0_Scripts/1.Systems/TurnSystem.cs` | (외부 의존) 몬스터 턴 진입점을 소유 — `EnemySystem`은 `TurnGA(Enemy)`의 Performer를 갖지 않는다 |
| `Assets/Game/0_Scripts/2.GameActions/PerformMoveGA.cs`, `MoveGA.cs` | (외부 의존, 원래 Hero 이동용) 몬스터 이동에도 그대로 재사용 — `mover`가 `CombatantView`라 `EnemyView`도 그대로 들어간다. 상세는 `MoveSystem.cs` |

구현 예시로 인용하는 구체 클래스: `Assets/Game/0_Scripts/Enemies/Larva.cs`(이동형 `Enemy`), `Assets/Game/0_Scripts/Enemies/Scarecrow.cs`(제자리형 `Enemy`), `Assets/Game/0_Scripts/Enemies/EnemyActions/AttackEA.cs`(근접), `Assets/Game/0_Scripts/Enemies/EnemyActions/FireArrowEA.cs`(원거리).

## 1. 한 줄 개요

`EnemySystem`은 몬스터 목록을 직접 들고 있지 않고(`TokenSystem.EnemyViews`에 위임) AI 판단 로직도 갖지 않는 **얇은 오케스트레이터**다. "이동할지/어디로 갈지" 판단은 각 `EnemyView`가 들고 있는 `Enemy`에 위임하고, **"누구를 때릴지"는 `EnemyAction` 자신이 실행 순간에 직접 정한다.** `EnemySystem`은 그 판단을 **한 턴 앞서 예약**해뒀다가(telegraph) 다음 몬스터 턴에 "이동 → 공격" 순서로 중계하는 역할만 한다.

## 2. EnemySystem과 6개 협력 요소

```csharp
// EnemySystem.cs:13-20
private void OnEnable()
{
    ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnGAPerformer);
    ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroGAPerformer);
    ActionSystem.AttachPerformer<PlayEnemyEAGA>(PlayEnemyEAGAPerformer);
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

### 2-2. PlayEnemyEAGA — "예약된 행동을 실제로 재생" 데이터 (신규)

```csharp
// PlayEnemyEAGA.cs
public class PlayEnemyEAGA : GameAction
{
    public EnemyView Enemy { get; private set; }
    public EnemyAction Action { get; private set; }
}
```

`EnemyTurnGAPerformer`는 예전엔 `nextAction.PlayEnemyAction(enemy)`를 **직접 호출**했지만, 지금은 이 GA로 감싸서 `AddReaction`으로 큐잉만 한다(3절 참고). 이유는 "이동 후 공격" 순서를 보장하기 위해서다 — 자세한 메커니즘은 4-1절.

```csharp
// EnemySystem.cs:61-65
private IEnumerator PlayEnemyEAGAPerformer(PlayEnemyEAGA playEnemyEAGA)
{
    var motion = playEnemyEAGA.Action.PlayEnemyAction(playEnemyEAGA.Enemy);
    yield return motion?.WaitForCompletion();
}
```

`PlayEnemyAction`이 대상을 못 찾아 `null`을 반환해도 `motion?.WaitForCompletion()`이 null-conditional로 안전하게 처리한다 — 예외 없이 그냥 한 프레임 대기하고 지나간다.

### 2-3. AttackHeroGA — "몬스터 → 영웅 공격" 데이터

```csharp
public class AttackHeroGA : GameAction, IHaveCaster
{
    public Token Caster { get; private set; }
    public float DamageAmount { get; private set; }
    public List<Vector2Int> AttackArea { get; private set; }   // 범위 공격용
    public Vector2Int AttackPosition { get; private set; }     // 단일 타겟 공격용
}
```

생성자가 2개(범위/단일)라 `AttackArea`와 `AttackPosition` 중 실제로는 하나만 채워진다 — Performer(2-3절 하단)도 `AttackArea != null` 분기로 구분해서 처리한다.

### 2-4. Enemy — AI 두뇌 (추상)

```csharp
public abstract class Enemy
{
    public abstract EnemyAction JudgeActAction(EnemyView enemy);              // 다음 할 행동 미리 판단
    public abstract bool PerformAction(EnemyView enemy, EnemyAction nextAction); // 실행 직전 재검증 + 이동 판단
    public abstract Enemy Clone();
}
```

- `JudgeActAction`: **판단 시점**(턴 종료/전투 시작)에 호출. "다음 턴에 뭘 할지"만 정하고 아직 실행하지 않는다.
- `PerformAction`: **실행 시점**(자기 턴)에 호출. 이제 이 메서드의 책임은 구현체마다 다르다 —
  - 제자리형(`Scarecrow`): 재검증할 것도, 타겟을 정할 것도 없다(대상 탐색은 `EnemyAction`이 알아서 하므로) — 사실상 `return true;`.
  - 이동형(`Larva`): "이동할 필요가 있는가, 있다면 어느 방향/칸으로 갈 것인가"를 판단하고, 필요하면 이동 GA(`PerformMoveGA`)를 **직접 `AddReaction`으로 큐잉**한다. **더 이상 `EnemyAction.targetPosition`을 세팅하지 않는다** — 실제 공격 대상은 이동이 끝난 뒤 `EnemyAction.PlayEnemyAction`이 스스로 다시 정한다.
  - `false`를 반환하면 `PlayEnemyEAGA`가 큐잉되지 않아 이번 턴엔 공격을 시도하지 않는다(단, 그 전에 이미 큐잉해둔 이동은 `false`를 반환해도 그대로 실행된다 — 4-2절 참고).
- `Clone()`: `EnemyView.SetUp`이 `EnemyData.Enemy.Clone()`으로 호출 — 몬스터 스폰마다 AI 인스턴스를 복제한다. **반드시 자기 자신의 타입을 반환해야 한다**(과거 `Larva.Clone()`이 `new Scarecrow()`를 반환하던 오탈자가 있었다 — 지금은 수정됨. 새 `Enemy`를 추가할 때 같은 실수를 반복하지 않도록 7절 체크리스트에서 확인).

### 2-5. EnemyAction — 개별 행동 (추상) + 자체 타겟 탐색

```csharp
// EnemyAction.cs
public abstract class EnemyAction
{
    public List<Vector2Int> targetRange { get; protected set; }
    public Vector2Int targetPosition { get; protected set; }   // PlayEnemyAction 내부에서 스스로 탐색해 채움

    public abstract Sequence PlayEnemyAction(EnemyView enemy);   // 연출 + 자체 탐색 + 후속 GA 체이닝
    public abstract EnemyAction Clone();

    protected HeroView FindRandomHeroInRange(EnemyView enemy, int distance)
    {
        Vector2Int myPos = TokenSystem.Instance.API.GetTokenPosition(enemy);

        var heroesInRange = TokenSystem.Instance.HeroViews
            .Where(h => h != null && h.CurrentHealth > 0)
            .Where(h => TokenSystem.Instance.API.GetDistance(myPos, TokenSystem.Instance.API.GetTokenPosition(h)) <= distance)
            .ToList();

        if (heroesInRange.Count == 0) return null;
        return heroesInRange[UnityEngine.Random.Range(0, heroesInRange.Count)];
    }
}
```

과거엔 `targetRange`/`targetPosition`을 `Enemy.PerformAction`이 실행 직전에 채워줬다(주석: "Enemy에서 PlayEnemyAction 실행 전에 받음"). **지금은 반대다** — `PlayEnemyAction`이 호출되는 순간의 "**현재 실제 위치**" 기준으로 `FindRandomHeroInRange(enemy, distance)`를 호출해 사거리 내 생존 영웅 중 하나를 스스로 뽑아 `targetPosition`에 채운다. `Enemy.PerformAction`이 뭘 하든(혹은 아무것도 안 하든) 이 값은 `PlayEnemyAction`이 호출되는 즉시 덮어써진다.

이게 왜 중요한가: `PlayEnemyAction`은 (3절에서 보듯) **이동 GA가 이미 다 처리된 뒤**에 호출되므로, 여기서 탐색하는 "현재 위치"는 이동 전이 아니라 **이동 후** 위치다. 즉 이동 목적지를 계산할 때 노렸던 영웅과, 실제로 도착해서 때리는 영웅이 다를 수 있다(예: 무리 지어 있는 영웅 쪽으로 이동했는데 도착해보니 원래 노렸던 영웅보다 다른 영웅이 더 가깝거나 여럿이 사거리에 걸림) — 의도된 동작이다.

`distance` 인자는 호출부(`AttackEA`/`FireArrowEA`)가 자기 `Distance`(`IHaveDistance`) 값을 그대로 넘겨준다 — 두 구현 모두 "사거리 내 생존 영웅 랜덤 하나, 없으면 무시"라는 완전히 동일한 탐색 로직을 쓰기 때문에 이 헬퍼 하나로 충분하다.

### 2-6. EnemyView — 개체별 런타임 상태 + 예고 UI

```csharp
public EnemyAction NextAction
{
    get => nextAction;
    private set { nextAction = value; UpdateNextActionUI(); }   // 아이콘/텍스트 즉시 갱신
}
public int MovePoint { get; private set; }         // 최대 이동력
public int CurrentMovePoint { get; private set; }  // 현재 남은 이동력
```

`SetNextAction`으로 값이 바뀌는 순간 `UpdateNextActionUI()`가 실행돼 몬스터 머리 위 예고 아이콘/텍스트가 갱신된다. 이동력은 `MPSystem`(영웅 전용 공유 풀)과 별개로 **개체마다** `CurrentMovePoint`/`HasEnoughMovePoint(int)`/`SpendMovePoint(int)`로 관리된다 — 영웅과 달리 GameAction(`SpendMPGA` 같은)을 거치지 않고 `Enemy.PerformAction`이 이동을 결정하는 순간 **직접 메서드 호출**로 소모한다(`EnemyView.cs` 주석: "MP API는 몬스터만 사용"). `ResetMovePoint()`는 몬스터 턴이 시작될 때 `EnemySystem.EnemysTurnGAPreReaction`이 직접 호출한다(리액션 아님, 3절 참고).

## 3. 턴 흐름 (트리거부터 끝까지)

```
[전투 시작] TurnGA(StartBattle) POST
 └ EnemySystem.BattleStartPostReaction
    └ 모든 EnemyView: Enemy.JudgeActAction() → SetNextAction (최초 예고 아이콘 표시)

[몬스터 턴] TurnSystem.TurnGAPerformer가 TurnType.Enemy 분기 진입
 └ 턴 팝업 연출 대기 후 yield return EnemySystem.Instance.PlayEnemyTurnPerformer()
    1. 1초 대기 + 로그("몬스터s턴 시작")
    2. EnemysTurnGAPreReaction() 직접 호출 (Reaction 아님, 그냥 private 메서드)
       └ 모든 EnemyView: ResetMovePoint()(이동력 초기화), ReduceSEWhenMyTurnStart()
    3. Enemise(=TokenSystem.EnemyViews) 순회하며 EnemyTurnGA 생성 → AddReaction
       └ 지금 시점은 TurnGA(Enemy)의 Perform 단계 중이라, `reactions`가 TurnGA.PerformReactions를
         가리키고 있음(00_action_architecture.md §3) → 큐잉된 EnemyTurnGA들이 전부 거기 쌓인다
    (PlayEnemyTurnPerformer 종료 → TurnGAPerformer 종료 → PerformPerformer(TurnGA) 종료)

 └ ActionSystem.Flow(TurnGA) 계속 진행 → PerformReactions() 호출
    └ 방금 쌓인 EnemyTurnGA들을 리스트 순서대로 하나씩 재귀 Flow() 처리
      **= 몬스터는 병렬이 아니라 완전 순차 처리** (한 마리 Pre/Perform/Post가 끝나야 다음으로)

    [몬스터 1마리분] EnemyTurnGAPerformer (EnemySystem.cs:49-59)
     └ enemy.NextAction이 있으면:
        a) enemy.Enemy.PerformAction(enemy, nextAction) 호출
           — 이동형(Larva)이면 이 안에서 필요시 PerformMoveGA를 먼저 AddReaction으로 큐잉
             (→ 지금 시점 reactions는 이 EnemyTurnGA.PerformReactions를 가리키므로 여기에 쌓임)
        b) true를 반환하면, 그 다음으로 PlayEnemyEAGA(enemy, nextAction)를 AddReaction으로 큐잉
           → 같은 리스트에 [PerformMoveGA, PlayEnemyEAGA] 순서로 쌓인다
    (EnemyTurnGAPerformer 코루틴은 여기서 끝 — 더 이상 공격 연출을 직접 기다리지 않는다)

 └ Flow(EnemyTurnGA)가 계속 진행 → PerformReactions()가 방금 쌓인 리스트를 순서대로 재귀 Flow()
   **PerformMoveGA의 Flow가 완전히 끝나야(=이동 트윈까지 다 재생돼야) 다음 항목인 PlayEnemyEAGA의
   Flow가 시작된다** — 4-1절 참고. 이것이 "이동 후 공격" 순서를 보장하는 유일한 장치다.

    [PerformMoveGA 발생 시] MoveSystem.PerformMoveGAPerformer (기존 Hero 이동 로직 재사용)
     └ 경로를 순회하며 칸마다 MoveGA를 AddReaction → MoveGAPerformer가 TokenSystem.Main.MoveToken
       (mover가 HeroView일 때만 SpendMPGA 추가 — 몬스터는 이미 Enemy.PerformAction에서
        EnemyView.SpendMovePoint를 직접 호출했으므로 여기선 아무 것도 소모하지 않음)

    [PlayEnemyEAGA 발생 시] PlayEnemyEAGAPerformer (EnemySystem.cs:61-65)
     └ nextAction.PlayEnemyAction(enemy) 호출 → Sequence 반환, WaitForCompletion
        (PlayEnemyAction 내부: 지금(=이동 후) 위치 기준으로 FindRandomHeroInRange로 자체 탐색
         → 없으면 null 반환하고 끝, 있으면 AttackHeroGA/DOAnimationGA를 AddReaction으로 체이닝)

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

## 4. 핵심 설계

### 4-1. "이동 후 공격" 순서를 GA 큐잉만으로 보장하는 법

`Enemy.PerformAction`은 **동기 `bool` 메서드**라서 이동 애니메이션(코루틴)을 기다릴 수 없다. 그렇다고 `PerformAction` 안에서 이동 GA를 큐잉하고 곧바로 공격 연출을 직접 호출해버리면, `ActionSystem.AddReaction`은 "즉시 실행"이 아니라 "**현재 처리 중인 GA 자신의 리스트에 추가**"일 뿐이므로(`00_action_architecture.md` §2-4) 공격 연출이 먼저 재생되고 이동이 나중에 벌어지는 순서 역전이 생긴다.

해결책은 공격 연출 자체도 `PlayEnemyAction`을 직접 호출하지 않고 `PlayEnemyEAGA`라는 GA로 감싸서 큐잉하는 것이다. 그러면 `PerformMoveGA`와 `PlayEnemyEAGA`가 **같은 `EnemyTurnGA.PerformReactions` 리스트에 순서대로** 쌓이고, `PerformReactions()`는 그 리스트를 `foreach`로 돌며 각 항목의 `Flow()`를 **완전히 끝낸 뒤에야 다음으로 넘어간다**(`GameAction.cs`의 `PreReactions`/`PerformReactions`/`PostReactions`가 인스턴스별로 독립된 리스트라는 점이 이걸 가능하게 한다). 즉 "무엇을 먼저 `AddReaction`했는가"가 곧 "무엇이 먼저 완료되는가"를 그대로 결정한다 — 별도의 대기/락 장치 없이 **큐잉 순서 하나로 실행 순서를 보장**하는 패턴이다.

### 4-2. 판단(Judge)과 실행(Perform)의 1턴 시차

몬스터가 "이번 턴에 뭘 할지"는 **이번 턴에 정해지지 않는다.** 최초 1회는 전투 시작 시(`BattleStartPostReaction`), 이후로는 매번 **직전 몬스터 턴이 끝나는 시점**(`EnemyTurnPostReaction`)에 다음 몬스터 턴 몫을 미리 계산해 `SetNextAction`으로 박아둔다. `EnemyView.NextAction`의 setter가 UI 갱신을 동반하므로, 이 예약은 곧바로 플레이어에게 "다음에 뭘 당할지" 아이콘으로 노출된다.

같은 몬스터 턴 안에서는 그 예약이 `Enemy.PerformAction`으로 한 번 더 재검증된다. 다만 **`PerformAction`의 `false`가 더 이상 "이번 턴 완전 무행동"을 뜻하지 않는다**는 점에 주의 — 이동형(`Larva`)은 이동력이 부족해 사거리 안까지 못 들어가는 경우, 갈 수 있는 만큼만 전진하는 `PerformMoveGA`를 **이미 큐잉해둔 채로** `false`를 반환한다. 이 경우 이동은 그대로 실행되고, 공격(`PlayEnemyEAGA`)만 큐잉되지 않는다 — "재검증 실패 = 완전 정지"가 아니라 "재검증 실패 = 공격만 취소"로 의미가 넓어졌다.

## 5. 구체 예시로 보는 두 사이클

### 5-1. 이동형 — Larva + AttackEA

```csharp
// Larva.cs — 이동 방향만 판단, 공격 대상은 세팅하지 않음
public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
{
    if (nextAction is not AttackEA attackEA) return false;

    var heroes = TokenSystem.Instance.HeroViews.Where(h => h != null && h.CurrentHealth > 0).ToList();
    if (heroes.Count == 0) return false;

    // 가장 가까운 대상 탐색(동률 랜덤) → 이동 "방향" 결정용, 공격 대상 확정용 아님
    // 이미 사거리 안이면 이동 없이 true, 아니면 사거리 안에 들어오는 최단 경로로 PerformMoveGA 큐잉 후 true,
    // 그마저 안 되면 갈 수 있는 만큼만 전진(PerformMoveGA 큐잉)하고 false
}
```

```csharp
// AttackEA.cs — 이동이 끝난 뒤, 실제 위치 기준으로 스스로 대상을 정함
public override Sequence PlayEnemyAction(EnemyView enemy)
{
    HeroView target = FindRandomHeroInRange(enemy, Distance);
    if (target == null) return null;   // 사거리 내 대상 없음 → 무시

    targetPosition = TokenSystem.Instance.API.GetTokenPosition(target);

    ...
    AttackHeroGA attackHeroGA = singleTarget
        ? new(enemy, Damage_Amount, targetPosition)
        : new(enemy, Damage_Amount, targetRange);
    ActionSystem.Instance.AddReaction(attackHeroGA);

    DOAnimationGA animationGA = new(backTween);
    ActionSystem.Instance.AddReaction(animationGA);

    return squ;
}
```

한 사이클: `Larva.JudgeActAction`이 `AttackEA`를 예약 → UI에 공격 아이콘 표시 → 몬스터 턴이 오면 `Larva.PerformAction`이 이동 방향만 계산해 `PerformMoveGA`를 큐잉(필요하다면) → `EnemyTurnGAPerformer`가 이어서 `PlayEnemyEAGA` 큐잉 → 이동이 다 끝난 뒤 `AttackEA.PlayEnemyAction`이 **그 시점의 실제 위치**로 다시 사거리 내 대상을 탐색해 랜덤으로 하나 지정 → 있으면 공격 연출 + `AttackHeroGA`(피해 판정), 없으면 조용히 무행동.

### 5-2. 제자리형 — Scarecrow + FireArrowEA (조합 예시)

```csharp
// Scarecrow.cs — 이동이 없으므로 재검증/타겟팅 전부 EnemyAction에 위임
public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
{
    return true;
}
```

`FireArrowEA.PlayEnemyAction`도 `AttackEA`와 동일한 패턴(`FindRandomHeroInRange` 호출 → 없으면 `null` → 있으면 원거리 투사체 연출 + `AttackHeroGA`)이라, 어떤 `Enemy` 구현체와 조합해도(이동하든 안 하든) 그대로 재사용된다 — 이동 로직(`Enemy`)과 타겟팅/연출 로직(`EnemyAction`)이 서로 독립적으로 조합 가능해진 것이 이번 리팩토링의 핵심 효과다.

## 6. 주의사항 / 함정

- **`EnemysTurnGAPreReaction`은 이름과 달리 `ActionSystem` 리액션이 아니다.** `SubscribeReaction`에 등록된 적이 없고, `PlayEnemyTurnPerformer` 안에서 그냥 직접 호출되는 private 메서드다(`EnemySystem.cs:36`, 정의는 `EnemySystem.cs:133`). 네이밍만 보고 리액션 타이밍(PRE/POST)을 추정하면 안 된다.
- **`EnemySystem`은 `TurnGA(Enemy)`의 Performer를 갖지 않는다.** 몬스터 턴의 실제 진입점은 `TurnSystem.TurnGAPerformer`이고, `EnemySystem.PlayEnemyTurnPerformer`는 거기서 직접 호출되는 public 코루틴일 뿐이다.
- **몬스터 처리는 병렬이 아니라 완전 순차다**(3절 참고) — 몬스터 수만큼 연출 대기 시간이 선형으로 늘어난다.
- **`EnemyTurnGAPerformer`는 더 이상 공격 연출의 완료를 직접 기다리지 않는다.** `PlayEnemyEAGA`를 큐잉만 하고 `yield break`로 즉시 끝난다(`EnemySystem.cs:49-59`) — 실제 연출 대기는 `PlayEnemyEAGAPerformer`로 옮겨갔다. 몬스터 턴 하나가 "끝났다"는 것과 "공격 연출까지 다 재생됐다"는 것은 이제 서로 다른 코루틴 안에서 일어난다(다만 같은 `EnemyTurnGA` Flow 안에서 순차 처리되므로 전체 턴 소요 시간에는 영향 없음).
- **`targetRange`/`targetPosition`의 setter는 `protected`다.** `Enemy.PerformAction`(별개의 클래스 계층)에서 `nextAction.targetPosition = ...`처럼 예전 패턴으로 세팅하려 하면 이제 **컴파일 에러**로 막힌다 — "설정해도 `PlayEnemyAction`이 덮어써서 효과가 없다"는 정도가 아니라 애초에 `EnemyAction`(과 그 서브클래스) 밖에서는 대입 자체가 불가능하다. 새 `Enemy`를 짤 때 예전 패턴(대상 좌표를 직접 세팅)을 따라 하지 않도록 주의.
- **`PerformMoveGA`/`MoveGA`는 원래 Hero 전용으로 쓰이던 GA를 몬스터가 그대로 재사용하는 것이다.** `PerformMoveGAPerformer`가 `mover is HeroView`일 때만 `SpendMPGA`를 추가하도록 이미 되어 있어서, 몬스터가 이 GA를 큐잉해도 영웅 공유 MP 풀은 건드리지 않는다 — 몬스터 이동력 소모는 `Enemy.PerformAction`이 `EnemyView.SpendMovePoint`를 GA 없이 직접 호출해서 처리한다(2-6절).
- **`enemy.NextAction`이 `null`이면 `EnemyTurnGAPerformer`는 조용히 아무 것도 하지 않는다**(`EnemySystem.cs:54` 분기). `JudgeActAction`이 한 번도 호출되지 않은 몬스터(예: 전투 중 새로 소환된 경우)는 첫 턴에 그냥 넘어간 것처럼 보일 수 있다.

## 7. 새 몬스터/행동 추가 시 체크리스트

1. `Enemy`를 상속한 새 클래스에서 `JudgeActAction`(다음 행동 판단)과 `PerformAction`을 구현한다. 이동이 필요 없는 몬스터라면 `PerformAction`은 `return true;`로 충분하다(타겟팅은 `EnemyAction`이 알아서 한다). 이동이 필요하다면 방향/목적지만 계산해서 `PerformMoveGA`를 `AddReaction`으로 큐잉하고, **`targetPosition`은 세팅하지 않는다.**
2. `Clone()`이 **자기 자신의** 새 인스턴스를 반환하는지 확인한다.
3. 이 몬스터가 쓸 `EnemyAction` 서브클래스들을 `EnemyData.EnemyActions`에 등록한다 — `EnemyView.SetUp`이 이 리스트를 복제해 `Actions`로 보관한다.
4. 새 `EnemyAction.PlayEnemyAction`은 (a) `FindRandomHeroInRange(enemy, distance)` 같은 자체 탐색으로 대상을 스스로 정하고 없으면 `null`을 반환, (b) 있으면 필요한 후속 GA(`AttackHeroGA`, `DOAnimationGA` 등)를 `ActionSystem.Instance.AddReaction`으로 체이닝하고 자신의 연출 `Sequence`를 반환한다 — `PlayEnemyEAGAPerformer`가 그 완료를 기다린다. 대상 탐색 로직이 기존 `IHaveDistance` 기반 사거리 판정과 동일하다면 `EnemyAction` 베이스의 `FindRandomHeroInRange`를 그대로 재사용한다.
5. "공격" 외의 새로운 몬스터→영웅 상호작용이 필요하면, 기존 `AttackHeroGA`를 재사용할지 새 GA + Performer(`EnemySystem` 또는 신규 시스템)를 추가할지부터 결정한다 — §2-3 계약을 먼저 확인한다.
