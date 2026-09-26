# HeroSystem 아키텍처

> 이 문서는 플레이어 턴이 시작될 때 무슨 일이 벌어지는지, 그리고 "현재 선택된 영웅" 상태가 어떻게 관리되는지를 정리한 참고 문서다. `ActionSystem` 자체의 동작 원리는 [00_action_architecture.md](./00_action_architecture.md), 몬스터 쪽 대칭 구조는 [01_enemy_system.md](./01_enemy_system.md)를 먼저 본다. AP/MP UI 세부 구현, 스킬 실행 파이프라인(`SkillSystem`의 타겟 지정·연출), `HeroView`의 전투 스탯 계산은 이 문서 범위 밖이다.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/1.Systems/HeroSystem.cs` | 파사드(Singleton) — 플레이어 턴 시작 훅 + "현재 선택 영웅" 상태 관리 |
| `Assets/Game/0_Scripts/1.Systems/APSystem.cs` | (외부 소유) AP 자원 — `RefillAPGA`/`SpendAPGA` Performer |
| `Assets/Game/0_Scripts/1.Systems/MPSystem.cs` | (외부 소유) MP 자원 — `RefillMPGA`/`SpendMPGA` Performer |
| `Assets/Game/0_Scripts/1.Systems/SkillSystem.cs` | (외부 소유) 스킬 UI/실행 — `CurrentHero` 변경 시 갱신 대상, AP 소비 트리거원 |
| `Assets/Game/0_Scripts/1.Systems/MoveSystem.cs` | (외부 소유) 이동 처리 — MP 소비 트리거원 |
| `Assets/Game/0_Scripts/3.Views/HeroView.cs` | 영웅 개체 런타임 상태(`Hero`/`Skills`/`Perks`), `CombatantView` 상속 |
| `Assets/Game/0_Scripts/SubSystems/GridSelector.cs` | (외부) 그리드 클릭 입력 — `CurrentHero` 설정 진입점 |
| `Assets/Game/0_Scripts/1.Systems/MatchSetupSystem.cs` | (외부) 전투 준비 — 최초 `CurrentHero` 자동 지정 |
| `Assets/Game/0_Scripts/1.Systems/TurnSystem.cs` | (외부 소유) `PlayHeroTurnPerformer` 호출자 |
| `Assets/Game/0_Scripts/UI/TurnEndUI.cs` | (외부) 플레이어 턴 종료 버튼 — `TurnGA(Enemy)` 최초 트리거 |

## 1. 한 줄 개요

`HeroSystem`은 [01_enemy_system.md](./01_enemy_system.md)의 `EnemySystem`과 대칭적인 위치에 있지만 소유 범위는 훨씬 작다 — **자기 이름의 `GameAction` Performer가 하나도 없고**, "플레이어 턴 시작 훅"(`TurnSystem`이 직접 호출하는 public 코루틴)과 "현재 선택된 영웅"이라는 서로 무관한 두 가지 상태만 책임진다. 실제 자원 증감(AP/MP)은 `APSystem`/`MPSystem`이, 스킬 실행은 `SkillSystem`이, 이동은 `MoveSystem`이 각자 소유하며, `HeroSystem`은 턴 시작 시점에 "리필하라"는 트리거만 걸어준다.

## 2. HeroSystem의 두 책임

```csharp
// HeroSystem.cs
private void OnEnable()
{
    ActionSystem.SubscribeReaction<TurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
}
```

`OnEnable`에 `AttachPerformer` 호출이 전혀 없다는 점이 `EnemySystem`/`APSystem`/`MPSystem`과의 핵심 차이다 — `HeroSystem`은 어떤 `GameAction` 타입의 "실제 상태를 바꾸는 유일한 소유자"도 아니다.

### 2-1. 턴 훅 — PlayHeroTurnPerformer & EnemyTurnPreReaction

- `PlayHeroTurnPerformer`는 `EnemySystem.PlayEnemyTurnPerformer`와 같은 성격이다: `ActionSystem`에 Performer로 등록된 게 아니라, `TurnSystem.TurnGAPerformer`가 `TurnType.Player` 분기에서 직접 `yield return`으로 호출하는 public 코루틴이다(`HeroSystem.cs:42`, 호출부는 `TurnSystem.cs:55`).
- 내부 순서: 모든 `HeroView`에 `ReduceSEWhenMyTurnStart()`(방어막 스택 제거 — `EnemySystem`과 동일한 `CombatantView` 공용 메서드) → `RefillAPGA`/`RefillMPGA`를 `AddReaction`으로 체이닝. 이 메서드 자체가 [00_action_architecture.md](./00_action_architecture.md) §2-4의 "AddReaction으로 체이닝" 대표 예시로 이미 인용되어 있다.
- `RefillAPGA`/`RefillMPGA`는 필드 없는 트리거 전용 마커 액션이고, `APSystem.RefillAPGAPerformer`/`MPSystem.RefillMPGAPerformer`가 각각 `CurrentAP = MaxAP`, `CurrentMP = MaxMP`로 되돌리고 UI를 갱신한다 — `HeroSystem`은 이 수치 자체를 들고 있지 않다.
- `EnemyTurnPreReaction`은 `TurnGA`의 PRE에 구독되어 있고, `Type`이 `Enemy`일 때만 동작한다(그 외엔 `return`) — 즉 **"몬스터 턴이 시작되기 직전 = 플레이어 턴이 끝나는 시점"**에 모든 `HeroView`의 `ReduceSEWhenMyTurnEnd()`(취약/약화 감소)를 실행한다. 이 메서드는 [00_action_architecture.md](./00_action_architecture.md) §2-3 "PRE 예 2"의 대표 예시이기도 하다.
- 두 훅 다 `HeroView`를 단순 순회할 뿐, `EnemySystem`의 `EnemyTurnGA`처럼 "영웅 1명당 GA 하나씩" 체이닝하지 않는다 — 플레이어 턴에는 그런 "영웅 턴" 그릇이 없다. 각 영웅의 실제 행동(이동/스킬)은 턴 동안 자유 입력으로 처리되고, 그 자체가 개별 GA(`SpendMPGA`/`PlaySkillGA` 등)로만 표현된다.

### 2-2. 선택 상태 — CurrentHero

```csharp
public HeroView CurrentHero
{
    get => currentHero;
    set
    {
        if (value == null || currentHero == value) return;
        currentHero = value;
        // 선택 스프라이트 UI 갱신
        SkillSystem.Instance.UpdateSkillsUI(currentHero); // 스킬 UI 갱신 + 재정렬
    }
}
```

- 이 프로퍼티는 `ActionSystem`/`GameAction`과 무관한 순수 C# 상태다 — GA로 표현되지 않고, 누가 언제 호출하든 즉시 동기적으로 바뀐다.
- 설정 지점은 2곳뿐이다: `MatchSetupSystem.StartSetting()`이 전투 시작 직전 첫 번째 `HeroView`를 자동 지정하고(`MatchSetupSystem.cs:42`), 이후 `GridSelector.UpdateSelectedToken`이 그리드에서 영웅을 클릭할 때마다 갱신한다(`GridSelector.cs:99`).
- `GridSelector.SelectToken`은 `TurnType.GameSetUp` 중이거나 스킬 타겟 모드 중이면 클릭 처리를 아예 반환한다(`GridSelector.cs:63-64`) — 이 두 경우를 뺀 나머지(몬스터 턴 포함)에는 선택 전환 자체를 막지 않는다(4절 참고).

## 3. 턴 흐름 시퀀스 (Player 턴)

```
[플레이어 턴] TurnSystem.TurnGAPerformer가 TurnType.Player 분기 진입
 └ 1초 대기 → 턴 팝업 연출 대기 → yield return HeroSystem.Instance.PlayHeroTurnPerformer()
    1. 로그("플레이어 턴 시작")
    2. 모든 HeroView: ReduceSEWhenMyTurnStart()
    3. RefillAPGA를 AddReaction → APSystem.RefillAPGAPerformer: CurrentAP = MaxAP + UI
    4. RefillMPGA를 AddReaction → MPSystem.RefillMPGAPerformer: CurrentMP = MaxMP + UI
    (yield return null로 종료 — TurnGA(Player)의 PerformReactions에 쌓인 두 액션이
     Performer 종료 직후 순서대로 처리된다, 00_action_architecture.md §3 참고)

[자유 입력 구간] — 플레이어가 그리드/스킬 UI를 조작하는 동안
 - GridSelector.SelectToken → HeroSystem.CurrentHero 갱신 (영웅 선택 전환)
 - MoveSystem: 이동 확정 시 SpendMPGA(path.Count)를 AddReaction (MoveSystem.cs:120)
 - SkillSystem: 스킬 사용 확정 시 SpendAPGA()를 AddReaction (SkillSystem.cs:275)
 - (이동/스킬 가능 여부는 APSystem.HasEnoughAP / MPSystem.HasEnoughMP로 액션 생성 전에 직접 체크
   — 00_action_architecture.md §2-2가 설명하는 "Reaction이 아니라 public 메서드로 검증" 패턴)

[플레이어 턴 종료] TurnEndUI 버튼 클릭
 └ ActionSystem.Instance.Perform(new TurnGA(TurnType.Enemy))  (TurnEndUI.cs:25-30)

[TurnGA(Enemy)의 PRE 단계] — 몬스터 턴 시작 직전
 └ HeroSystem.EnemyTurnPreReaction (Type==Enemy 필터 통과)
    └ 모든 HeroView: ReduceSEWhenMyTurnEnd() (취약/약화 감소)
```

## 4. 주의사항 / 함정

- **`selected_Hero_UI`가 `public Image` 필드다**(`HeroSystem.cs:8`). `convention.md` §1(`- 필드 노출은 [SerializeField] private만. public 필드 금지.`)과 어긋난다 — 인스펙터 연결이 이미 되어 있다면 `[SerializeField] private`로 바꿔도 참조는 유지되지만, 되돌리기 전에 먼저 알려드리는 것으로 갈음한다(CLAUDE.md 지침).
- **`CurrentHero` 전환에 턴 타입 가드가 없다.** 세터 자체도, `GridSelector.SelectToken`의 가드도 "GameSetUp이거나 스킬 타겟 모드"만 막는다 — 몬스터 턴(`TurnType.Enemy`) 중에도 영웅을 다시 선택해 스킬 UI를 재정렬하는 것 자체는 코드상 막혀있지 않다. 실제 행동(이동/스킬 발동)이 몬스터 턴 중 막히는지는 `MoveSystem`/`SkillSystem` 쪽 가드에 달려 있고 이 문서 범위 밖이다.
- **`HeroSystem`에는 "영웅 1명당 턴 GA" 개념이 없다** — `EnemySystem.EnemyTurnGA`와 비교하면 비대칭. 개별 영웅의 행동 순서 강제나 재시도 로직이 필요해지는 시점이 오면 새로 들여올지 검토가 필요하다(현재는 자유 입력이라 불필요).
- **`TurnEndUI.TurnEnd()`는 `IsPerforming` 가드 없이 `ActionSystem.Instance.Perform(...)`을 직접 호출한다**(`TurnEndUI.cs:25-30`) — [00_action_architecture.md](./00_action_architecture.md) §2-5가 명시한 "호출자가 직접 `IsPerforming`을 체크해야 한다"는 관례(`MoveSystem`/`SkillSystem`은 이 체크를 함)와 다르다. `Perform` 자체가 단일 슬롯 락이라 버튼 연타 시 두 번째 호출은 조용히 무시되지만, 별도 피드백은 없다.

## 5. 새로운 영웅 자원/훅 추가 시 체크리스트

1. 새 턴 시작/종료 훅이 필요하면 Performer로 만들지 Reaction으로 만들지부터 정한다 — "이 자원을 실제로 바꾸는 유일한 소유자"면 Performer(`APSystem`/`MPSystem` 패턴), "기존 턴 GA에 부가 로직만 얹는 것"이면 Reaction(`HeroSystem.EnemyTurnPreReaction` 패턴).
2. 새 자원 소비 GA(`SpendXxxGA`)는 실제 소비가 확정되는 지점(`MoveSystem`/`SkillSystem`처럼 유저 행동이 확정되는 곳)에서 `ActionSystem.Instance.AddReaction`으로 생성한다 — `HeroSystem` 자신은 소비를 트리거하지 않는다(리필만 담당).
3. `CurrentHero` 변경에 반응해야 하는 새 UI/시스템이 생기면, 세터(`HeroSystem.cs:13-28`) 안에 직접 호출을 추가하기 전에 먼저 알린다 — 이미 `HeroSystem → SkillSystem` 단방향 의존이 세터 안에 박혀 있어, 더 늘리면 이 프로퍼티가 여러 시스템의 진입점이 되어버린다.
