# action_command.md — GameAction 레지스트리

> 이 문서는 현재 코드에 **실제로 `Perform`/`AddReaction` 경로가 있는 모든 GameAction의 카탈로그**다 — 무엇이 있고, 누가 소유(Performer)하고, 누가 반응(PRE/POST)하고, 무엇을 유발(AddReaction)하는지의 스냅샷이며, 코드 생성·리뷰·컨벤션 체크 시 1차 참조 자료로 쓴다.
>
> - `ActionSystem`이 **왜/어떻게** 동작하는지(Flow 순서, Performer vs Reaction 선택 기준, 신규 GA 추가 절차, 함정)는 다루지 않는다 → [`feature-spec/00_action_architecture.md`](./feature-spec/00_action_architecture.md) 참고.
> - 네이밍/필드/폴더 등 일반 코드 컨벤션은 다루지 않는다 → [`convention.md`](../../convention.md) 참고.
> - **범위**: `StartBattleGA`, `EnemysTurnGA`, `PlayerMoveGA`, `PlayHeroVisualEffectGA`, `AddCardAbilityGA`, `EnemyTurnGA`는 정의만 있고 `Perform`/`AddReaction` 호출 경로가 코드 전체에서 확인되지 않는 죽은 코드라 이 카탈로그에서 제외했다 (`AddCardAbilityGA`는 `AddCardAbilityEffect.cs`가 생성은 하지만 Performer가 없어 사실상 no-op, `EnemyTurnGA`는 `EnemySystem` 제거로 Performer·호출 경로가 사라져 `EnemyTurnGA.cs`만 남음).
> - **작성 기준**: 현재 워킹트리 기준. AP/MP 자원 분리 리팩터링(`ManaSystem` → `APSystem`+`MPSystem`)이 아직 커밋되지 않은 상태를 그대로 반영했다.
> - 이 문서는 스냅샷이다. GameAction이 추가/변경/삭제되면 즉시 갱신한다 (자세한 규칙은 4절).

## 1. 마스터 인덱스

| GA | 카테고리 | Performer (소유 시스템) | 한 줄 설명 |
|---|---|---|---|
| [`TurnGA`](#turnga) | 턴/루프 | `TurnSystem` | 턴 전환의 축 — StartBattle → Player ↔ Enemy 순환 |
| [`SpendAPGA`](#spendapga) | 자원 | `APSystem` | AP(행동력) 소모 |
| [`RefillAPGA`](#refillapga) | 자원 | `APSystem` | AP 최대치로 리필 |
| [`SpendMPGA`](#spendmpga) | 자원 | `MPSystem` | MP(이동력) 소모 |
| [`RefillMPGA`](#refillmpga) | 자원 | `MPSystem` | MP 최대치로 리필 |
| [`MoveGA`](#movega) | 이동 | `MoveSystem` | 한 칸 이동(또는 넉백에 의한 이동) 실행 |
| [`PerformMoveGA`](#performmovega) | 이동 | `MoveSystem` | 경로 전체 이동 예약(칸마다 `MoveGA` 체이닝) |
| [`KnockBackGA`](#knockbackga) | 이동 | `KnockBackProcessor` | 특정 방향/거리로 넉백, 충돌 시 데미지 |
| [`DealDamageGA`](#dealdamagega) | 전투 | `DamageSystem` | 데미지 적용의 공통 최종 경로 |
| [`KillGA`](#killga) | 전투 | `DamageSystem` | 사망 처리(연출 대기 → 토큰 제거 → 승패 판정) |
| [`AttackEnemyGA`](#attackenemyga) | 전투 | `AttackEnemyProcessor` | 영웅 근접/원거리 공격(모션 + 데미지) |
| [`HealGA`](#healga) | 회복/상태 | `HealSystem` | 대상 회복 |
| [`AddStatusEffectGA`](#addstatuseffectga) | 회복/상태 | `StatusEffectSystem` | 상태이상(ARMOR/POWER 등) 스택 부여 |
| [`PlaySkillGA`](#playskillga) | 스킬 | `SkillSystem` | 스킬 사용 시작(최상위 트리거) |
| [`PerformEffectGA`](#performeffectga) | 스킬 | `EffectProcessor` | 스킬 내 Effect 1개 실행 |
| [`SplashGA`](#splashga) | 콤보 | `SplashProcessor` | 범위(스플래시) 공격 |
| [`ShoulderBashGA`](#shoulderbashga) | 콤보 | `ShoulderBashProcessor` | 돌진(이동+공격+넉백) 콤보 |
| [`ShieldBashGA`](#shieldbashga) | 콤보 | `ShieldBashProcessor` | 방패강타(데미지+자기 방어막) |
| [`DOAnimationGA`](#doanimationga) | 연출 | `AnimationSystem` | DOTween Sequence/Tween 재생 대기 |
| [`GameClearGA`](#gameclearga) | 종료 | `GameSystem` | 게임 클리어 |
| [`GameOverGA`](#gameoverga) | 종료 | `GameSystem` | 게임 오버 |
| [`SpawnWaveGA`](#spawnwavega) | 웨이브 | `WaveSystem` | 몬스터 턴마다 카운트다운, n턴 지나면 다음 웨이브 몬스터 추가 생성 |

## 2. 카테고리별 상세

### 2-1. 턴 / 게임 루프

#### `TurnGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/TurnGA.cs`
- **필드**: `TurnType Type`
- **Performer**: `TurnSystem.TurnGAPerformer` — `StartBattle`이면 `Player` 턴 즉시 체이닝, `Enemy`면 턴 팝업 연출만 재생(몬스터 행동 처리 없음), `Player`면 턴 팝업 연출 후 `HeroSystem.PlayHeroTurnPerformer`에 위임
- **PRE 구독자**:
  - `TurnSystem.TurnGAPreReaction` — `currentTurn` 값 선반영
  - `HeroSystem.EnemyTurnPreReaction` (조건: `Type == Enemy`) — 영웅들 "내 턴 종료" SE 감소
  - `HeroArmor` 퍽 (`PerkItem.Reaction` 경유, 조건: `Type == Enemy`) — 보유 영웅에게 `AddStatusEffectGA(ARMOR)` 예약
  - `WaveSystem.TurnGAPreReaction` (조건: `Type == Player`) — 웨이브 카운트다운(`turnsUntilNextWave`) 감소, 0 이하이면 `AddReaction(SpawnWaveGA)`
- **POST 구독자**:
  - `TurnSystem.TurnGAPostReaction` (조건: `Type == Enemy`) — `TurnGA(Player)` 체이닝 (턴 순환의 핵심)
  - `MuscleTrophy` 퍽 (조건: `Type == StartBattle`) — 보유 영웅에게 `AddStatusEffectGA(POWER)` 예약
- **파생 액션 (AddReaction)**: `StartBattle` 처리 중 Performer 내부에서 `TurnGA(Player)` 즉시 체이닝
- **트리거 지점**: `MatchSetupSystem.StartSetting()`에서 최초 `Perform(TurnGA(StartBattle))` 호출. 이후 `Enemy` 종료 POST에서 자동으로 다음 `TurnGA(Player)`가 체이닝되며 순환
- **관련 체인**: §3-1

### 2-2. 자원 (AP / MP)

#### `SpendAPGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/SpendAPGA.cs`
- **필드**: `int Amount` (기본값 1)
- **Performer**: `APSystem.SpendAPPerformer` — `CurrentAP -= Amount`, `APUI` 갱신
- **PRE/POST 구독자**: 없음
- **파생 액션**: 없음
- **트리거 지점**: `SkillSystem.PlaySkillPerformer`(스킬 사용 시 `AddReaction`)
- **관련 체인**: §3-2

#### `RefillAPGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/RefillAPGA.cs`
- **필드**: 없음 — 트리거 전용 마커
- **Performer**: `APSystem.RefillAPPerformer` — `CurrentAP = MaxAP`
- **트리거 지점**: `HeroSystem.PlayHeroTurnPerformer`(플레이어 턴 시작 시 `AddReaction`)

#### `SpendMPGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/SpendMPGA.cs`
- **필드**: `int Amount` (기본값 1)
- **Performer**: `MPSystem.SpendMPPerformer` — `CurrentMP -= Amount`
- **트리거 지점**: `MoveSystem.PerformMoveGAPerformer`(영웅 이동 시 경로 칸 수만큼)

#### `RefillMPGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/RefillMPGA.cs`
- **필드**: 없음 — 트리거 전용 마커
- **Performer**: `MPSystem.RefillMPPerformer` — `CurrentMP = MaxMP`
- **트리거 지점**: `HeroSystem.PlayHeroTurnPerformer`

### 2-3. 이동

#### `MoveGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/MoveGA.cs`
- **필드**: `CombatantView mover`, `Vector2Int movePosition`, `bool isKnockBack`(기본 false)
- **Performer**: `MoveSystem.MoveGAPerformer` — `TokenSystem`으로 실제 토큰 이동 + 이동 사운드 재생
- **트리거 지점**: `PerformMoveGA` Performer(경로 칸 수만큼), `KnockBackGA` Performer(밀려남 처리, `isKnockBack = true`)

#### `PerformMoveGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/PerformMoveGA.cs`
- **필드**: `CombatantView mover`, `List<Vector2Int> path`
- **Performer**: `MoveSystem.PerformMoveGAPerformer` — `mover`가 `HeroView`면 `SpendMPGA(path.Count)` 체이닝 + 경로 칸마다 `MoveGA` 체이닝
- **트리거 지점**: `MoveSystem.Update()`가 예약 큐(`reserved_hero_moves`)를 소비할 때 최초 `Perform()` 호출(플레이어 이동), `ShoulderBashProcessor.ShoulderBashGAPerformer`(돌진 이동 단계, `AddReaction`)
- **관련 체인**: §3-3

#### `KnockBackGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/KnockBackGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `CombatantView Caster`, `int Distance`, `Vector2Int TargetPos`, `Vector2Int Direction`
- **Performer**: `KnockBackProcessor.KnockBackGAPerformer` — 충돌 판정 후 밀려난 만큼 `MoveGA(isKnockBack: true)` 체이닝, 벽/유닛에 충돌하면 고정 데미지(1) `DealDamageGA` 체이닝
- **트리거 지점**: `ShoulderBashProcessor.ShoulderBashGAPerformer`(돌진 콤보의 마지막 단계)
- **관련 체인**: §3-3

### 2-4. 전투 — 데미지 / 사망

#### `DealDamageGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/DealDamageGA.cs` (`IHaveCaster` 구현)
- **필드**: `float Amount`, `List<IDamageable> Targets` / `IDamageable Target`(다중·단일 생성자), `Token Caster`
- **Performer**: `DamageSystem.DealDamagePerformer` — `DamageCaculator.GetDamage()`로 최종 수치 계산 + 피격 VFX + `target.Damage()` 호출(방어막 소모 처리, 체력 0 이하 시 `KillGA`를 이 액션의 `PostReactions`에 직접 추가)
- **POST 구독자**: `BloodyAxe` 퍽 (조건: `Caster == owner`) — 확률 조건 충족 시 `HealGA` 예약(자힐)
- **파생 액션**: `CombatantView.Damage()` 내부에서 체력 ≤ 0이면 `dealDamageGA.PostReactions.Add((KillGA, null))` — 전역 체이닝이 아니라 **이 인스턴스에 직접 종속**
- **트리거 지점**: 전투 계열 다수의 공통 최종 경로 — `AttackEnemyGAPerformer`, `ShieldBashGAPerformer`, `ShoulderBashGAPerformer`, `SplashGAPerformer`, `KnockBackGAPerformer`(충돌 시) 등
- **관련 체인**: §3-2, §3-3

#### `KillGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/KillGA.cs`
- **필드**: `Token Token`, `Tween Hit_Tween`
- **Performer**: `DamageSystem.KillGAPerformer` — 피격 흔들림 연출(`Hit_Tween`) 완료 대기 → 비주얼 그리드 정리 → 토큰 제거 → 적 전멸(`TokenSystem.EnemyViews.Count <= 0`)이면 `GameClearGA`, 영웅 전멸이면 `GameOverGA` 체이닝
- **트리거 지점**: 전역 `Perform`/`AddReaction`이 아니라, `DealDamageGA` 처리 중 `CombatantView.Damage()`가 체력 ≤ 0일 때 그 `DealDamageGA` 인스턴스의 `PostReactions`에 직접 추가되어서만 실행됨
- **관련 체인**: §3-2

#### `AttackEnemyGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/CardEffectRelative/AttackEnemyGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `List<Vector2Int> TargetPoses`, `float Amount`, `int Count`, `HeroView Caster`, `HeroAnimationType animationType`
- **Performer**: `AttackEnemyProcessor.AttackEnemyGAPerformer` — 근접/원거리 공격 모션(DOTween Sequence) 재생 → 모션 완료(`endTween`) 후 `DealDamageGA` 체이닝
- **트리거 지점**: `EffectProcessor.PerformEffectPerformer`가 `Effect.GetGameAction()`으로 생성(근접/원거리 공격 스킬의 Effect)
- **관련 체인**: §3-2

### 2-5. 회복 / 상태이상

#### `HealGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/HealGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `float Amount`, `List<Vector2Int> TargetPoses`, `List<CombatantView> Targets`(런타임에 위치→대상으로 채워짐), `HeroView Caster`
- **Performer**: `HealSystem.HealGAPerformer` — 위치를 대상으로 변환 후 순차 회복(연출 딜레이 포함)
- **트리거 지점**: `BloodyAxe` 퍽의 POST 리액션(자힐), 회복 스킬의 `Effect.GetGameAction()`

#### `AddStatusEffectGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/CardEffectRelative/AddStatusEffectGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `StatusEffectType StatusEffectType`, `int StackCount`(set 가능), `List<CombatantView> Targets`, `HeroView Caster`
- **트리거 지점**: 아래 목록에 `AddStatusEffectEffect.cs`(버프/디버프 스킬의 Effect) 추가 — `SETargetMode`에 따라 자신/대상/전체에 상태이상 부여
- **Performer**: `StatusEffectSystem.AddStatusEffectPerformer` — 대상별로 상태이상 스택 부여
- **트리거 지점**: `ShieldBashProcessor.ShieldBashGAPerformer`(해당 `DealDamageGA`의 `PostReactions`에 직접 종속), `HeroArmor` 퍽(PRE, ARMOR), `MuscleTrophy` 퍽(POST, POWER)

### 2-6. 스킬 실행 파이프라인

#### `PlaySkillGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/PlaySkillGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `Skill Skill`(set 가능), `List<Vector2Int> TargetPoses`, `HeroView Caster`
- **Performer**: `SkillSystem.PlaySkillPerformer` — `SpendAPGA` 체이닝 + `Skill.SkillAbility.Effects` 개수만큼 `PerformEffectGA` 체이닝
- **트리거 지점**: `SkillSystem.Update()`가 예약 큐(`reserved_Skills`)를 소비할 때 `IsPerforming` 가드 후 최초 `Perform()` 호출
- **관련 체인**: §3-2

#### `PerformEffectGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/PerformEffectGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `Effect Effect`(set 가능), `List<Vector2Int> TargetPoses`, `HeroView Caster`
- **Performer**: `EffectProcessor.PerformEffectPerformer` — `Effect.GetGameAction()`이 반환하는 구체적 GA(예: `AttackEnemyGA`, `SplashGA`, `HealGA` 등)를 그대로 체이닝
- **트리거 지점**: `PlaySkillGA` Performer(스킬의 Effect 개수만큼)
- **관련 체인**: §3-2

### 2-7. 카드 이펙트 전용 콤보

#### `SplashGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/CardEffectRelative/SplashGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `CombatantView Caster`, `List<Vector2Int> TargetPoses`, `RangeMode GridRangeMode`, `bool IsPentration`, `int Distance`, `float Damage`, `float SplashDamage`
- **Performer**: `SplashProcessor.SplashGAPerformer` — 대상별 직격 `DealDamageGA` 체이닝 + 주변 스플래시 대상 `DealDamageGA`를 직격 `DealDamageGA`의 `PostReactions`에 직접 종속
- **트리거 지점**: `PerformEffectGA`(`Effect.GetGameAction()`)
- **관련 체인**: §3-3

#### `ShoulderBashGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/CardEffectRelative/ShoulderBashGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `int Distance`, `int AttackDistance`, `float Damage`, `List<Vector2Int> TargetPoses`, `HeroView Caster`
- **Performer**: `ShoulderBashProcessor.ShoulderBashGAPerformer` — 경로 계산 후 `PerformMoveGA`(돌진 이동) → `DealDamageGA`(공격) → `KnockBackGA`(넉백) 순서로 형제 `AddReaction` 체이닝
- **트리거 지점**: `PerformEffectGA`(`Effect.GetGameAction()`)
- **관련 체인**: §3-3

#### `ShieldBashGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/CardEffectRelative/ShieldBashGA.cs` (`IHaveCaster` 명시적 구현)
- **필드**: `float Amount`, `List<Vector2Int> TargetPoses`, `HeroView Caster`
- **Performer**: `ShieldBashProcessor.ShieldBashGAPerformer` — `DealDamageGA` 체이닝 + `AddStatusEffectGA(ARMOR)`를 그 `DealDamageGA`의 `PostReactions`에 직접 종속
- **트리거 지점**: `PerformEffectGA`(`Effect.GetGameAction()`)
- **관련 체인**: §3-3

### 2-8. 연출

#### `DOAnimationGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/DOAnimationGA.cs`
- **필드**: `Sequence Sequence`, `Tween Tween` (생성자 2개, 둘 중 하나만 채워짐)
- **Performer**: `AnimationSystem.DOAnimationPerformer` — 전달된 `Tween`/`Sequence`를 재생하고 완료까지 대기
- **트리거 지점**: 현재 코드에서 `DOAnimationGA`를 생성하는 호출처가 없음 (몬스터 `EnemyAction` 제거로 유일한 호출처가 사라짐 — Performer만 등록된 상태)

### 2-9. 게임 종료

#### `GameClearGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/GameClearGA.cs`
- **필드**: 없음
- **Performer**: `GameSystem.GameClearPerformer` — `IsGameClear = true`, 보상 스킬 3장 산출(`RewardSystem`)
- **트리거 지점**: `DamageSystem.KillGAPerformer`(적 전멸 판정 시) — **현재 워킹트리 기준 주석 처리되어 비활성 상태**(`DamageSystem.cs:97-98`). Performer/등록은 살아있지만 이 경로에서 더 이상 호출되지 않음.

#### `GameOverGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/GameOverGA.cs`
- **필드**: 없음
- **Performer**: `GameSystem.GameOverPerformer` — `IsGameOver = true`, 게임오버 UI 표시
- **트리거 지점**: `DamageSystem.KillGAPerformer`(영웅 전멸 판정 시)

### 2-10. 웨이브

#### `SpawnWaveGA`
- **파일**: `Assets/Game/0_Scripts/2.GameActions/SpawnWaveGA.cs`
- **필드**: 없음 — 트리거 전용 마커
- **Performer**: `WaveSystem.SpawnWaveGAPerformer` — 다음 웨이브 인덱스(`nextWaveIndex`)만큼 `enemyDatas`에서 몬스터를 골라 `TokenMainAPI.AddEnemys`로 생성, `Remain_wave_count`/`nextWaveIndex`/`turnsUntilNextWave` 갱신
- **PRE/POST 구독자**: 없음
- **파생 액션**: 없음
- **트리거 지점**: `WaveSystem.TurnGAPreReaction`(`TurnGA` PRE 구독, 조건: `Type == Player`)에서 웨이브 카운트다운이 0 이하일 때 `AddReaction(new SpawnWaveGA())`

## 3. 대표 체인 다이어그램

### 3-1. 턴 순환 체인

```
[MatchSetupSystem.StartSetting] Perform(TurnGA(StartBattle))
 └ TurnSystem.TurnGAPerformer
     ├─ (PRE) TurnSystem: currentTurn = StartBattle
     ├─ AddReaction(TurnGA(Player))                         ← Performer 내부 즉시 체이닝
     └─ (POST) MuscleTrophy 퍽: AddReaction(AddStatusEffectGA(POWER))

TurnGA(Player)
 └ TurnSystem.TurnGAPerformer → HeroSystem.PlayHeroTurnPerformer
     └─ AddReaction(RefillAPGA), AddReaction(RefillMPGA)
     (이후 플레이어 입력으로 PlaySkillGA/PerformMoveGA가 별도로 Perform됨)

TurnGA(Enemy)                                                ← 플레이어가 턴 종료
 └ TurnSystem.TurnGAPerformer
     ├─ (PRE) HeroSystem.EnemyTurnPreReaction: 영웅 SE 감소
     ├─ (PRE) HeroArmor 퍽: AddReaction(AddStatusEffectGA(ARMOR))
     ├─ 턴 팝업 연출만 재생 (몬스터 행동 처리 없음)
     └─ (POST) TurnSystem.TurnGAPostReaction: AddReaction(TurnGA(Player))   ← 순환
```

### 3-2. 스킬 공격 전체 체인

```
[SkillSystem.Update] Perform(PlaySkillGA)
 └ SkillSystem.PlaySkillPerformer
     ├─ AddReaction(SpendAPGA)                → APSystem.SpendAPPerformer (AP 차감)
     └─ AddReaction(PerformEffectGA)          → EffectProcessor.PerformEffectPerformer
          └─ AddReaction(AttackEnemyGA)       → AttackEnemyProcessor.AttackEnemyGAPerformer
               (공격 모션 재생 대기)
               └─ AddReaction(DealDamageGA)   → DamageSystem.DealDamagePerformer
                    ├─ (POST 구독) BloodyAxe 퍽: 조건 충족 시 AddReaction(HealGA)
                    └─ target.Damage() 내부에서 체력<=0이면
                         dealDamageGA.PostReactions.Add(KillGA)   ← 인스턴스 직접 종속
                              └─ DamageSystem.KillGAPerformer
                                   └─ AddReaction(GameClearGA | GameOverGA)
```

### 3-3. 카드 콤보 체인 — 두 가지 후속 연결 패턴

```
(A) 형제 리액션으로 큐잉 — ShoulderBashGA
ShoulderBashProcessor.ShoulderBashGAPerformer
 ├─ AddReaction(PerformMoveGA)   [돌진 이동]
 ├─ AddReaction(DealDamageGA)    [공격]
 └─ AddReaction(KnockBackGA)     [넉백]
   → 셋 다 "현재 진행 중인 단계"의 형제로 등록되어 순서대로 실행(서로 독립적인 형제 액션)

(B) 특정 GA 인스턴스에 직접 종속 — ShieldBashGA / SplashGA
ShieldBashProcessor.ShieldBashGAPerformer
 ├─ AddReaction(DealDamageGA) → dealDamageGA
 └─ dealDamageGA.PostReactions.Add(AddStatusEffectGA(ARMOR))
   → ARMOR 부여는 반드시 "이 DealDamageGA의 POST 단계"에서만 실행됨

SplashProcessor.SplashGAPerformer도 동일 패턴:
 ├─ AddReaction(DealDamageGA) → dealDamageGA(직격)
 └─ dealDamageGA.PostReactions.Add(DealDamageGA(스플래시))
```

## 4. 문서 유지보수 규칙

- 이 문서는 스냅샷이다. 신규 `~GA`를 추가하거나 기존 GA의 필드/Performer/구독자/파생 액션이 바뀌면 즉시 1절(마스터 인덱스)과 2절(해당 카테고리 상세 블록)을 갱신한다.
- GA를 삭제할 때는 1절·2절에서 해당 행/블록을 제거하고, 3절 체인 다이어그램에 등장했다면 그 부분도 함께 정리한다.
- `ActionSystem` 자체의 동작 방식(Flow, PRE/POST 의미 등)이 바뀌면 이 문서가 아니라 [`feature-spec/00_action_architecture.md`](./feature-spec/00_action_architecture.md)를 갱신한다.
