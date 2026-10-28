# 특성(Trait) 카탈로그 — 조건·효과 연결

> 특성 하나가 어떤 `TraitCondition` + `TraitEffect` + 타이밍으로 묶여 있는지 **특성 단위**로 정리한 문서. 구조·계약·로드 파이프라인은 [05_trait_system.md](../05_trait_system.md), 특성 제작 절차는 [trait_howto.md](../../howto/trait_howto.md), 재사용 가능한 조건·효과 목록은 [trait.md](./trait.md)를 본다.
> 새 특성(`TraitData` 에셋)을 만들면 이 문서에 섹션을 추가한다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> **현재 수록: 9개** — 3001 불경한 빛, 3002 신속의 기도, 3003 요새화, 3004 빠른 발, 3005 맹공, 3006 저격, 3007 공격 신호, 3008 응수, 9001 돌격 화살통(장애물 전용). `Assets/Game/1_Datas/3.Traits/`의 에셋 전부다.

## 9001 · 돌격 화살통 (장애물 전용)

에셋: `Assets/Game/1_Datas/3.Traits/돌격 화살통.asset` — `ObstacleData` `돌격 화살통`의 Traits에 연결 ([08_terrain_obstacle.md](../08_terrain_obstacle.md))

| 항목 | 값 |
|---|---|
| Id | 9001 |
| 설명 (`Description`) | 이 장애물을 파괴한 용병이 모든 적 용병을 추가로 공격합니다. |
| 조건 | `SelfDeathCondition` — 구독 `KillGA`, 판정 `KillGA.Token == owner` (owner = 장애물) |
| 타이밍 | PRE (`reactionTiming: 0`) |
| 효과 | `KillerAttackAllOpponentsEffect` |
| 효과 동작 | `KillGA.Killer`가 영웅이면 `EnemyViews` 전체, 적이면 `HeroViews` 전체에 `Killer.BaseDamage`만큼 `DealDamageGA(Targets)` 예약. 코어는 공격하지 않음 |
| 파생 GA | `DealDamageGA` |
| 코드 | `Traits/Conditions/SelfDeathCondition.cs`, `Traits/KillerAttackAllOpponentsEffect.cs` |
| 원작 확인 필요 | 피해량(처치자 공격력 가정), 적이 파괴했을 때의 동작, 코어 포함 여부 |

**에디터 설정**: `Condition`=`SelfDeathCondition`(`reactionTiming`=PRE), `Effect`=`KillerAttackAllOpponentsEffect`. `ConditionName`/`EffectName`에 클래스명을 적어 둔다.

## 9002 · 야전 공구함 (장애물 전용)

에셋: `Assets/Game/1_Datas/3.Traits/야전 공구함.asset` — `ObstacleData` `야전 공구함`(Id 3002)의 Traits에 연결 ([terrain_catalog.md](../Terrain/terrain_catalog.md))

| 항목 | 값 |
|---|---|
| Id | 9002 (`OwnerType` 2) |
| 설명 (`Description`) | 피해를 받을 때마다 공격한 영웅에게 공격력 +1, 체력 +1을 부여합니다. |
| 조건 | `SelfDamagedCondition` — 구독 `DealDamageGA`, 판정 `DamagedTargets.Contains(owner)` (owner = 장애물, 생존 여부 무관). `FiltersByCaster` = false |
| 타이밍 | POST (`reactionTiming: 1`) — 피해 적용 뒤 `DamagedTargets`가 채워진 상태, `KillGA` 수행(구독 해제) 전 |
| 효과 | `ModifyStatAttackerEffect` — `modifiers = [DAMAGE +1, HEALTH +1]` |
| 효과 동작 | `DealDamageGA.Caster`가 `CombatantView`면 그 유닛에게 `ModifyStatGA(DAMAGE, 1)`, `ModifyStatGA(HEALTH, 1)` 예약 |
| 파생 GA | `ModifyStatGA` |
| 코드 | `Traits/Conditions/SelfDamagedCondition.cs`, `Traits/ModifyStatAttackerEffect.cs`, `1.Systems/StatSystem.cs` |
| 원작 확인 필요 | 공격한 적에게도 부여하는지 (설명은 "영웅", 현재는 적에게도 부여), 파괴 타격 포함 여부 |

**연결 흐름**

```
DealDamageGA(Caster = 공격자, 대상 = 장애물) 수행: DamagedTargets.Add(장애물)
DealDamageGA POST
 └ Trait.Reaction  (FiltersByCaster == false → Caster != owner 필터 건너뜀)
    └ SelfDamagedCondition.SubConditionIsMat  (owner가 DamagedTargets에 있음)
       └ ModifyStatAttackerEffect.PerformReaction  (Caster가 CombatantView일 때만)
          └ AddReaction(ModifyStatGA, DAMAGE +1 / HEALTH +1, 대상=공격자)
```

**주의·한계**
- 효과가 예약하는 `ModifyStatGA`는 조건의 구독 대상(`DealDamageGA`)이 아니므로 무한 루프는 없다. 발동 횟수 상한은 없다.
- 장애물이 소유자일 때 파괴 타격에서도 발동한다는 순서 가정(POST 판정 → `KillGA`)은 Play에서 확인하지 않았다.
- `StatSystem`이 씬에 배치되어 있어야 동작한다. Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`SelfDamagedCondition`(`reactionTiming`=POST), `Effect`=`ModifyStatAttackerEffect`(`modifiers`=[DAMAGE 1, HEALTH 1]). `Image`는 비어 있다(장애물 특성은 표시 UI 없음).

## 3003 · 요새화

에셋: `Assets/Game/1_Datas/3.Traits/요새화.asset`

| 항목 | 값 |
|---|---|
| Id | 3003 |
| 설명 (`Description`) | 자신의 주변에 손에 있던 아군 용병이 배치될 때마다 체력+2를 얻습니다. |
| 조건 | `AllyPlacedNearbyCondition` — 구독 `PlayCardGA`, 판정: owner가 영웅이고 `TargetPos`가 owner와 1~`range`칸(자기 배치 제외) |
| 타이밍 | POST (`reactionTiming: 1`) — `PlayCardGA` performer가 `AddToken`을 끝낸 뒤라 owner 위치 조회가 가능 |
| 조건 수치 | `range` = 1 |
| 효과 | `ModifyStatTraitEffect` — `modifiers` = [HEALTH +2] |
| 효과 동작 | owner 자신을 대상으로 `ModifyStatGA`를 예약 → `StatSystem`이 `CombatantView.ModifyStat`으로 최대 체력과 현재 체력을 함께 +2 |
| 파생 GA | `ModifyStatGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/AllyPlacedNearbyCondition.cs`, `Traits/ModifyStatTraitEffect.cs`, `1.Systems/StatSystem.cs` |

**연결 흐름**

```
PlayCardGA POST  (IHaveCaster 아님 → 소유자 필터 전원 통과)
 └ Trait.Reaction
    └ AllyPlacedNearbyCondition.SubConditionIsMat  (영웅 owner, 1~range칸 내 타인 배치만 통과)
       └ ModifyStatTraitEffect.PerformReaction
          └ AddReaction(ModifyStatGA)  → StatSystem → CombatantView.ModifyStat(HEALTH, 2)
```

**주의·한계**
- `StatSystem`이 씬에 배치되어 있어야 `ModifyStatGA`가 처리된다 (`--- SYSTEMS ---` 아래).
- 손패 카드 배치(`PlayCardGA`)만 감지한다. 전투 전 초기 배치와 웨이브 몬스터 생성은 제외, 몬스터 owner는 발동하지 않는다.
- 체력 증가는 최대·현재 체력 모두 +2 (`StatType.HEALTH` 주석 기준). "체력+2"가 최대 체력만인지 현재 체력 회복인지는 (원작 확인 필요).
- 영웅 `CombatantData.Traits` 등록과 Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`AllyPlacedNearbyCondition`(`reactionTiming`=POST, `range`=1), `Effect`=`ModifyStatTraitEffect`(`modifiers` = [`statType`=HEALTH, `amount`=2]).

## 3005 · 맹공

에셋: `Assets/Game/1_Datas/3.Traits/맹공.asset`

| 항목 | 값 |
|---|---|
| Id | 3005 |
| 설명 (`Description`) | 적 용병 또는 우두머리를 공격한 뒤, 공격력+1을 얻습니다. 전투당 4번 발동됩니다. |
| 조건 | `AttackedOpponentCondition` — 구독 `DealDamageGA` + `AttackCoreGA`, 판정: 코어 공격은 무조건 통과, `DealDamageGA`는 대상이 상대 진영일 때 통과 (소유자 필터는 `Trait.Reaction`이 `Caster == owner`로 처리) |
| 타이밍 | POST (`reactionTiming: 1`) — 공격이 끝난 뒤 반응 |
| 효과 | `ModifyStatTraitEffect` — `modifiers` = [DAMAGE +1], `maxTriggers` = 4 |
| 효과 동작 | owner 자신을 대상으로 `ModifyStatGA(DAMAGE, 1)`를 예약 → `StatSystem`이 `BaseDamage`를 증가. 조건을 통과한 횟수가 4번에 도달하면 이후 공격에는 효과가 실행되지 않는다 |
| 파생 GA | `ModifyStatGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/AttackedOpponentCondition.cs`, `Traits/ModifyStatTraitEffect.cs`, `Models/Trait.cs`(횟수 카운트), `Interfaces/ILimitedTrigger.cs`(상한 노출) |

**연결 흐름**

```
DealDamageGA / AttackCoreGA  POST
 └ Trait.Reaction  (IHaveCaster → Caster != owner 이면 무시)
    └ AttackedOpponentCondition.SubConditionIsMat  (코어 공격 또는 상대 진영 대상)
       └ 횟수 검사: effect is ILimitedTrigger, MaxTriggers(4) > 0 && triggerCount >= 4 이면 종료, 아니면 triggerCount++
          └ ModifyStatTraitEffect.PerformReaction
             └ AddReaction(ModifyStatGA DAMAGE +1) → StatSystem → CombatantView.ModifyStat
```

**주의·한계**
- 설명의 "전투당 4번"은 구현상 **매치 전체에서 유닛당 4번**이다 (카운트 리셋 없음). 원작의 "전투" 단위는 (원작 확인 필요).
- 카운트는 조건을 통과한 `Trait.Reaction` 횟수다. 다중 대상 `DealDamageGA`는 GA 1개 = 1회이고 코어 공격도 1회다.
- 유닛이 제거되었다 다시 배치되면 새 `Trait`이라 카운트가 0부터 다시 시작한다.
- 효과가 예약하는 `ModifyStatGA`는 조건의 구독 대상이 아니므로 무한 루프는 없다.
- DAMAGE 분기와 횟수 상한은 Play 검증 전이다.

**에디터 설정**: `Condition`=`AttackedOpponentCondition`(`reactionTiming`=POST), `Effect`=`ModifyStatTraitEffect`(`modifiers` = [DAMAGE 1], `maxTriggers`=4). 영웅 `기병`이 이미 이 특성을 갖고 있다.

## 3004 · 빠른 발

에셋: `Assets/Game/1_Datas/3.Traits/빠른 발.asset`

| 항목 | 값 |
|---|---|
| Id | 3004 |
| 설명 (`Description`) | 배치 시 바로 앞에 적 용병이 없다면 공격력+1, 속도+2를 얻습니다. |
| 조건 | `SelfPlacedCondition` — 구독 `PlayCardGA`, 판정: owner 위치 == `TargetPos` (손패 카드 배치만) + `requireNoOpponentInFront` = true이면 정면 1칸(`owner.FrontDirection`)이 상대 진영 유닛이 아닐 것 |
| 타이밍 | POST (`reactionTiming: 1`) — `PlayCardGA` performer가 `AddToken`을 끝낸 뒤라 owner 위치·정면 조회가 가능 |
| 조건 수치 | `requireNoOpponentInFront` = true |
| 효과 | `ModifyStatTraitEffect` — `modifiers` = [DAMAGE +1, SPEED +2] |
| 효과 동작 | 항목마다 owner 자신을 대상으로 `ModifyStatGA`를 예약(DAMAGE → SPEED 순) → `StatSystem`이 `CombatantView.ModifyStat`으로 `BaseDamage`·`Speed`를 증가 |
| 파생 GA | `ModifyStatGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/SelfPlacedCondition.cs`, `Traits/ModifyStatTraitEffect.cs`, `1.Systems/StatSystem.cs` |

**연결 흐름**

```
PlayCardGA POST  (IHaveCaster 아님 → 소유자 필터 전원 통과)
 └ Trait.Reaction
    └ SelfPlacedCondition.SubConditionIsMat
       ├ owner 위치 == TargetPos 인 유닛만 통과
       └ requireNoOpponentInFront: 정면 1칸(owner + FrontDirection)이 EnemyView가 아니면 통과
          └ ModifyStatTraitEffect.PerformReaction
             ├ AddReaction(ModifyStatGA DAMAGE +1) ┐
             └ AddReaction(ModifyStatGA SPEED  +2) ┴→ StatSystem → CombatantView.ModifyStat
```

**주의·한계**
- 배치 순간 한 번만 판정한다. 이후 정면 상황이 바뀌어도 이미 얻은 스탯은 유지된다.
- "적 용병이 없다"는 정면 1칸의 `EnemyView`만 불충족으로 본다. 빈칸·아군·그리드 범위 밖은 충족이다.
- 손패 카드 배치(`PlayCardGA`)만 감지한다. 전투 전 초기 배치와 웨이브 몬스터 생성은 제외이며, 몬스터 owner는 카드 배치가 없어 발동하지 않는다.
- DAMAGE·SPEED 분기는 이번에 처음 실제로 쓰인다. 속도 증가가 자동 전투 큐 정렬에 반영되는 시점은 Play 확인이 필요하다.
- `StatSystem`이 씬에 배치되어 있어야 동작한다. Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`SelfPlacedCondition`(`reactionTiming`=POST, `requireNoOpponentInFront`=true), `Effect`=`ModifyStatTraitEffect`(`modifiers` = [DAMAGE 1, SPEED 2]). 영웅 `매복자`가 이미 이 특성을 갖고 있다.

## 3001 · 불경한 빛

에셋: `Assets/Game/1_Datas/3.Traits/불경한 빛.asset`

| 항목 | 값 |
|---|---|
| Id | 3001 |
| 설명 (`Description`) | 사망 시 자신의 속도만큼 모든 적 용병 및 우두머리에게 피해를 줍니다. |
| 조건 | `SelfDeathCondition` — 구독 `KillGA`, 판정 `KillGA.Token == owner` |
| 타이밍 | PRE (`reactionTiming: 0`) — `KillGA` performer가 토큰을 파괴하면 `OnDisable`이 구독을 해제하므로 POST에서는 놓칠 수 있다 |
| 효과 | `DamageAllOpponentsBySpeedEffect` |
| 효과 동작 | `owner.Speed`를 피해량으로 (a) 상대 진영 전원에 `DealDamageGA`(`Targets` 오버로드, 공격 모션 없음), (b) 상대 코어에 `AttackCoreGA(owner, amount)`를 예약. owner가 `HeroView`면 상대는 `EnemyViews`/적 코어, 아니면 `HeroViews`/플레이어 코어. 상대 유닛이 없으면 (a)는 건너뛴다 |
| 파생 GA | `DealDamageGA`, `AttackCoreGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/SelfDeathCondition.cs`, `Traits/DamageAllOpponentsBySpeedEffect.cs` |

**연결 흐름**

```
KillGA(owner) PRE
 └ Trait.Reaction  (KillGA는 IHaveCaster 아님 → 소유자 필터 전원 통과)
    └ SelfDeathCondition.SubConditionIsMat  (Token == owner 인 유닛만 통과)
       └ DamageAllOpponentsBySpeedEffect.PerformReaction
          ├ AddReaction(DealDamageGA)   상대 진영 전원
          └ AddReaction(AttackCoreGA)   상대 코어
 └ (이후) owner 사망 처리 (KillGA performer)
```

**주의·한계**
- 이 효과를 `AttackedOpponentCondition`/`DealDamageCondition`과 묶으면 효과가 예약한 `DealDamageGA`/`AttackCoreGA`가 같은 특성을 다시 트리거해 **무한 루프**가 된다.
- `AttackCoreGA`는 owner의 공격 모션을 재생한다. owner는 PRE 시점이라 아직 그리드에 있어 위치 조회는 가능하다.
- 몬스터 피해(`DealDamageGA`)는 `DamageCaculator` 보정(약화/취약 등)을 거치지만 코어 피해(`AttackCoreGA`)는 거치지 않아 실제 피해량이 다를 수 있다 (원작 확인 필요).
- `Speed`는 효과 실행(예약) 시점의 값이다.
- 영웅 `CombatantData.Traits`에 등록하는 에디터 작업은 아직 하지 않았다 (유닛당 최대 2개).
- Play 테스트는 아직 하지 않았다.

**에디터 설정**: `TraitData`의 `Condition`=`SelfDeathCondition`(`reactionTiming`=PRE), `Effect`=`DamageAllOpponentsBySpeedEffect`. 작업용 변수 `ConditionName`/`EffectName`에는 각 클래스명을 적어 둔다.

## 3002 · 신속의 기도

에셋: `Assets/Game/1_Datas/3.Traits/신속의 기도.asset`

| 항목 | 값 |
|---|---|
| Id | 3002 |
| 설명 (`Description`) | 전장에 있는 동안 자신의 속도만큼 다른 모든 아군 용병의 속도를 증가시킵니다. 같은 특성을 가진 용병에게는 적용되지 않습니다. |
| 조건 | `AuraTriggerCondition` — 구독 `PlayCardGA` + `KillGA`, 판정: 배치된 토큰이 owner와 같은 진영(자신 포함) / `KillGA.Token == owner` |
| 타이밍 | `PlayCardGA`는 POST (`reactionTiming: 1`, `AddToken` 이후라 위치 조회 가능), `KillGA`는 조건 클래스에서 PRE 고정 (POST는 `OnDisable` 구독 해제로 놓칠 수 있음) |
| 효과 | `AllySpeedAuraEffect` — 노출 수치 없음 |
| 효과 동작 | 배치: owner 배치 시 나머지 아군 전원, 다른 아군 배치 시 그 유닛에게 `owner.Speed`만큼 `ModifyStatGA(SPEED)` 예약, 부여량을 효과 사본의 `speedGrants`(Trait별 사본이라 유닛별 장부)에 기록. 사망: 기록된 부여분을 살아 있는 대상마다 `ModifyStatGA(SPEED, -기록량)`로 회수. owner의 특성 중 같은 효과 클래스를 쓰는 특성(= 3002)의 Id를 `CombatantView.TryGetTraitIdByEffect`로 첫 부여 때 조회해 캐시하고, 그 특성을 가진 유닛(`CombatantView.HasTrait`)과 `owner.Speed <= 0`은 제외 |
| 파생 GA | `ModifyStatGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/AuraTriggerCondition.cs`, `Traits/AllySpeedAuraEffect.cs`, `3.Views/CombatantView.cs`(`HasTrait`), `Models/Trait.cs`(조건·효과 사본), `1.Systems/StatSystem.cs` |

**연결 흐름**

```
PlayCardGA POST / KillGA PRE  (둘 다 IHaveCaster 아님 → 소유자 필터 전원 통과)
 └ Trait.Reaction
    └ AuraTriggerCondition.SubConditionIsMat  (owner==null이면 false / 같은 진영 배치 / Token==owner)
       └ AllySpeedAuraEffect.PerformReaction
          ├ PlayCardGA: 대상 선정(HasTrait 제외) → 효과 사본의 speedGrants에 기록 → AddReaction(ModifyStatGA SPEED +Speed)
          └ KillGA:     speedGrants 순회 → AddReaction(ModifyStatGA SPEED -기록량) 후 비움
```

**주의·한계**
- 손패 카드 배치(`PlayCardGA`)만 감지한다. 전투 전 초기 배치(`TokenSetup`)·웨이브 생성은 발동하지 않아, 초기 배치 영웅이 이 특성을 가져도 오라가 없다.
- 부여량은 **배치 시점 `owner.Speed` 스냅샷**이다. 이후 owner 속도 변화는 반영되지 않는다 (원작 확인 필요).
- 회수 시 기록량을 그대로 빼므로, 그 사이 대상 속도가 줄었으면(예: 공격 신호) 0 미만이 될 수 있다 (`ModifyStat` 자체엔 하한이 없지만 `CombatantView.Speed` setter가 0으로 보정한다). 이 경우 회수량이 부여량보다 커져 속도가 원래보다 낮아질 수 있다.
- "같은 특성" 판정 단위는 **효과 클래스**다. 서로 다른 두 특성이 `AllySpeedAuraEffect`를 쓰면 같은 특성으로 취급한다(현재는 3002 하나뿐).
- `HasTrait`는 `CombatantData.Traits` 기준이라 `MaxTraitCount` 초과로 잘린 특성도 보유로 본다.
- 두 GA를 구독하는 조건은 `ActionSystem.wrapperMap`이 델리게이트 키를 덮어써 해제 시 구독이 목록에 남을 수 있다 (`AttackedOpponentCondition`도 동일). 인프라라 고치지 않고 조건에서 `owner == null`로 무시한다.
- `StatSystem`이 씬에 있어야 동작한다. Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`AuraTriggerCondition`(`reactionTiming`=POST), `Effect`=`AllySpeedAuraEffect`(설정 값 없음). `Image`는 연결되어 있다. 영웅 `CombatantData.Traits`에 등록하는 에디터 작업은 아직 하지 않았다 (유닛당 최대 2개).

## 3006 · 저격

에셋: `Assets/Game/1_Datas/3.Traits/저격.asset`

| 항목 | 값 |
|---|---|
| Id | 3006 |
| 설명 (`Description`) | 전장에 있는 동안 고정을 얻고, 자신의 차례에 무작위 적 용병을 공격합니다. 적 용병이 없다면, 적 깃발 또는 우두머리를 공격합니다. 매 라운드 종료 시 공격력이 1 감소하지만, 1 미만으로 내려가지 않습니다. |
| 조건 | `AutoBattleTurnCondition` — 구독 `AutoBattleTurnGA`, 항상 true (`AutoBattleTurnGA`는 `IHaveCaster`가 아니라 모든 소유자에게 통과) |
| 타이밍 | POST (`reactionTiming: 1`) — 자동 전투의 모든 공격이 끝난 뒤 라운드 종료로 보고 감소 |
| 효과 | `SniperEffect` — `damageDecay` = 1, `minDamage` = 1 |
| 효과 동작 | (a) 조건 통과 시 `Mathf.Min(damageDecay, owner.BaseDamage - minDamage)`가 0 이하면 무시, 아니면 `ModifyStatGA(DAMAGE, -감소량)` 예약. (b) 자동 전투 차례에 `Battle()`이 `TryBattle`을 호출: 상대 진영 유닛 중 무작위 1명에게 `DealDamageGA(owner.BaseDamage, 대상, owner, true, true)`, 상대가 없으면 `AttackCoreGA(owner, true)`를 예약(둘 다 원거리 플래그라 제자리에서 화살 발사)하고 true를 반환해 기본 이동·정면 공격을 건너뜀 |
| 파생 GA | `ModifyStatGA`, `DealDamageGA`, `AttackCoreGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/AutoBattleTurnCondition.cs`, `Traits/SniperEffect.cs`, `Interfaces/IBattleOverride.cs`, `Models/Trait.cs`(`TryOverrideBattle`), `3.Views/CombatantView.cs`(`Battle`), `1.Systems/StatSystem.cs` |

**연결 흐름**

```
자동 전투 차례: AutoBattleGAPerformer → unit.Battle()
 └ CombatantView.Battle: Trait.TryOverrideBattle → SniperEffect.TryBattle
    ├ 상대 유닛 있음: AddReaction(DealDamageGA, 무작위 1명)
    └ 상대 유닛 없음: AddReaction(AttackCoreGA)
    → true 반환, 기본 이동·정면 공격 생략

AutoBattleTurnGA POST  (IHaveCaster 아님 → 소유자 필터 전원 통과)
 └ Trait.Reaction → AutoBattleTurnCondition (항상 true)
    └ SniperEffect.PerformReaction → AddReaction(ModifyStatGA DAMAGE -1)  (BaseDamage > minDamage일 때만)
```

**주의·한계**
- **고정**은 별도 구현이 없다. 저격 행동이 이동을 하지 않으므로 자동 전투에서 제자리를 지키고, 플레이어 조작 이동은 그대로 가능하다 (사용자 결정, 원작 확인 필요).
- "적 깃발 또는 우두머리"는 이 코드의 상대 코어(`AttackCoreGA`)로 해석했다.
- 라운드 종료는 자동 전투 턴 종료 후(`AutoBattleTurnGA` POST)로 본다. 몬스터 턴은 포함되지 않는다.
- 공격 신호의 `ExtraAttackGA`는 `Battle()`을 거치지 않으므로 정면 공격 그대로다.
- 공격 모션은 `ProjectileSystem.PlayArrowShot`(`GetArrowBezierTween`)의 화살이다. 칸 사이 유닛을 통과해 날아가고 비행 시간은 `ProjectileSystem`의 `arrowSecondsPerTile`/`arrowMinDuration`으로 조정한다. 코어 공격은 코어 오브젝트 위치가 아니라 같은 행의 상대 쪽 그리드 끝까지 쏜다. 화살 사운드는 넣지 않았다 (Play 확인).
- `AttackCoreGA`는 `DamageCaculator` 보정을 받지 않아 유닛 공격과 피해량이 다를 수 있다 (원작 확인 필요).
- 속도 0은 행동 안 함이 아니라 가장 늦게 행동하는 최하위 속도다.
- `StatSystem`이 씬에 있어야 공격력 감소가 동작한다. Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`AutoBattleTurnCondition`(`reactionTiming`=POST), `Effect`=`SniperEffect`(`damageDecay`=1, `minDamage`=1). `Image`는 연결되어 있다. 영웅 `CombatantData.Traits`에 등록하는 에디터 작업은 아직 하지 않았다 (유닛당 최대 2개).

## 3007 · 공격 신호

에셋: `Assets/Game/1_Datas/3.Traits/공격 신호.asset`

| 항목 | 값 |
|---|---|
| Id | 3007 |
| 설명 (`Description`) | 적 용병 또는 우두머리를 공격한 뒤, 추가 공격 차례를 얻고 자신의 속도를 2 소모합니다. 남은 속도가 2 미만이더라도 속도가 0이 될 때까지 반복합니다. |
| 조건 | `AttackedOpponentCondition` — 구독 `DealDamageGA` + `AttackCoreGA`, 판정: 코어 공격은 무조건 통과, `DealDamageGA`는 대상이 상대 진영일 때 통과 (소유자 필터는 `Trait.Reaction`이 `Caster == owner`로 처리) |
| 타이밍 | POST (`reactionTiming: 1`) — 공격이 끝난 뒤 반응 |
| 효과 | `ExtraAttackEffect` — `speedCost` = 2 |
| 효과 동작 | `owner.Speed`가 `speedCost` 미만이면 무시. 아니면 (a) owner 자신에게 `ModifyStatGA(SPEED, -speedCost)`, (b) `ExtraAttackGA(owner)`를 순서대로 예약. `ExtraAttackGA` performer(`AutoBattleSystem`)가 `CombatantView.ReserveAttack(현재 위치)`로 이동 없이 정면 공격(`DealDamageGA` 또는 `AttackCoreGA`)을 예약 |
| 파생 GA | `ModifyStatGA`, `ExtraAttackGA`, (연쇄) `DealDamageGA`/`AttackCoreGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/AttackedOpponentCondition.cs`, `Traits/ExtraAttackEffect.cs`, `2.GameActions/ExtraAttackGA.cs`, `1.Systems/AutoBattleSystem.cs`, `3.Views/CombatantView.cs`(`ReserveAttack`) |

**연결 흐름**

```
DealDamageGA / AttackCoreGA  POST   (기본 공격 또는 추가 공격)
 └ Trait.Reaction  (IHaveCaster → Caster != owner 이면 무시)
    └ AttackedOpponentCondition.SubConditionIsMat  (코어 공격 또는 상대 진영 대상)
       └ ExtraAttackEffect.PerformReaction  (Speed < speedCost 이면 종료)
          ├ AddReaction(ModifyStatGA SPEED -2)  → StatSystem
          └ AddReaction(ExtraAttackGA)
             └ AutoBattleSystem.ExtraAttackGAPerformer → CombatantView.ReserveAttack
                └ AddReaction(DealDamageGA / AttackCoreGA)  → 위 흐름 반복
```

**주의·한계**
- 속도 소모는 **영구**다 (복구 로직 없음). 속도가 0이 되면 다음 `CacheCombatants`부터 해당 유닛은 가장 늦게 행동한다 (행동은 계속한다).
- 속도가 `speedCost`(2) 이상일 때만 발동하고 매번 2만 소모한다 (속도 3 → 추가 공격 1회, 속도 1 남음 / 속도 5 → 추가 2회, 속도 1 남음). 설명의 "2 미만이더라도 0까지 반복"은 이 규칙과 맞지 않는다 (Play 테스트에서 속도 3이 추가 2회가 되어 규칙을 바꿈).
- 정면에 대상이 없고 코어 공격 조건도 아니면 `ReserveAttack`이 GA를 만들지 않아 속도가 남아도 연쇄가 끝난다.
- 코어 공격(`AttackCoreGA`)도 조건을 만족하므로 코어를 쳐도 연쇄한다. 코어가 파괴되어 `IsGameClear`/`IsGameOver`이면 `ExtraAttackGA` performer가 중단한다.
- 연쇄는 매 회 속도가 줄어 반드시 끝난다 (효과가 예약한 `ModifyStatGA`/`ExtraAttackGA`는 조건 구독 대상이 아님). 다만 `ExtraAttackGA`가 만드는 `DealDamageGA`가 다시 조건을 트리거하는 것은 의도된 반복이다.
- `ModifyStatGA`의 음수 증감을 SPEED에 처음 쓴다 (`StatSystem`은 사망 처리 없이 `Speed`에 더한다). `Speed < speedCost`면 발동하지 않아 0 미만이 되지 않는다 (`Speed` setter도 0 미만을 0으로 보정).
- `StatSystem`이 씬에 배치되어 있어야 동작한다. Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`AttackedOpponentCondition`(`reactionTiming`=POST), `Effect`=`ExtraAttackEffect`(`speedCost`=2). `Image`는 연결되어 있다. 영웅 `CombatantData.Traits`에 등록하는 에디터 작업은 아직 하지 않았다 (유닛당 최대 2개).

## 3008 · 응수

에셋: `Assets/Game/1_Datas/3.Traits/Cores/응수.asset`

| 항목 | 값 |
|---|---|
| Id | 3008 |
| 설명 (`Description`) | 피격 시 반격해 자신에게 피해를 준 용병을 공격합니다. |
| 조건 | `CoreAttackedCondition` — 구독 `AttackCoreGA`, 판정: owner가 `HeroCaptainView`이고 `owner.CurrentHealth > 0`이며 `AttackCoreGA.IsMissed`가 false(빗나가지 않아 피해가 들어감)이고 `AttackCoreGA.Caster`가 `HeroView`가 아닐 것. `FiltersByCaster` = false |
| 타이밍 | POST (`reactionTiming: 1`) — `CoreSystem`이 용병대장 체력을 깎은 뒤 반응하므로 `CurrentHealth > 0` 검사가 피해 반영 후 값이다 |
| 효과 | `RetaliateEffect` — 노출 수치 없음 |
| 효과 동작 | `AttackCoreGA.Caster`가 `IDamageable`이면 그 공격자에게 `DealDamageGA(owner.BaseDamage, 공격자, owner, true)`(기본 공격)를 예약. `DamageSystem`이 `Caster is HeroCaptainView`이면 Iso 오른쪽 근접 모션을 재생한 뒤 타격 시점에 피해를 적용 |
| 파생 GA | `DealDamageGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/CoreAttackedCondition.cs`, `Traits/RetaliateEffect.cs`, `Models/TraitCondition.cs`(`FiltersByCaster`), `Models/Trait.cs`(필터 분기), `3.Views/HeroCaptainView.cs`, `1.Systems/CoreSystem.cs`, `1.Systems/Skills/DamageSystem.cs` |

**연결 흐름**

```
AttackCoreGA(적 → 아군 코어) 수행: CoreSystem이 HeroCaptain.ReceiveCoreDamage
AttackCoreGA POST
 └ Trait.Reaction  (FiltersByCaster == false → Caster != owner 필터 건너뜀)
    └ CoreAttackedCondition.SubConditionIsMat  (owner는 HeroCaptainView, 체력 > 0, 빗나간 공격 아님, Caster가 HeroView 아님)
       └ RetaliateEffect.PerformReaction  (Caster가 IDamageable일 때만)
          └ AddReaction(DealDamageGA, 대상=공격자, Caster=용병대장, 기본 공격)
             └ DamageSystem: HeroCaptainView 근접 모션 → 피해 적용
```

**주의·한계**
- 소유자는 `HeroCaptainData`(지그먼트, Id 9001)의 `Traits`에 등록된 용병대장뿐이다. `HeroCaptainView`는 그리드 밖이고 `Forward`가 없어 자동 전투를 하지 않으므로, 반격이 용병대장의 유일한 공격이다.
- 코어 체력이 0이 된 공격에는 반격하지 않는다 (`CurrentHealth > 0` 검사). 이때는 곧 패배 처리된다.
- `HeroView`(영웅)가 적 코어를 공격하는 `AttackCoreGA`는 `Caster is not HeroView`로 제외된다.
- 반격 피해량은 `owner.BaseDamage`(지그먼트 `Damage` 3)이고 `DamageCaculator` 보정 적용 여부는 `DealDamageGA` 경로를 그대로 따른다.
- 효과가 예약한 `DealDamageGA`는 이 조건의 구독 대상(`AttackCoreGA`)이 아니므로 무한 루프는 없다.
- 공격자가 `IDamageable`이 아니면 (`Caster`가 용병대장·영웅 외 비피격 토큰) 효과가 무시된다.
- `Image`가 비어 있다 (`<Image>`가 `fileID: 0`). Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`CoreAttackedCondition`(`reactionTiming`=POST), `Effect`=`RetaliateEffect`(설정 값 없음). 영웅 `지그먼트`(`10.Cores/HeroCaptains/지그먼트.asset`)의 `Traits`에 연결되어 있다.

## 3009 · 축복

에셋: `Assets/Game/1_Datas/3.Traits/축복.asset`

| 항목 | 값 |
|---|---|
| Id | 3009 |
| 설명 (`Description`) | 사망 시, 자신의 공격력 절반 만큼 전장에 있는 다른 모든 아군 영웅들의 공격력을 증가시킵니다. |
| 조건 | `SelfDeathCondition` — 구독 `KillGA`, 판정 `KillGA.Token == owner` |
| 타이밍 | PRE (`reactionTiming: 0`) — 불경한 빛과 같은 이유로 POST에서는 놓칠 수 있고, PRE에는 owner가 살아 있어 `BaseDamage`를 읽을 수 있다 |
| 효과 | `ShareDamageWithAlliesEffect` — `divisor`(2) |
| 효과 동작 | `amount = owner.BaseDamage / divisor`(정수 내림). `amount <= 0`이거나 대상이 없으면 무시. 아니면 owner를 제외한 `HeroViews` 전원(owner가 적이면 `EnemyViews`)에 `ModifyStatGA(DAMAGE, amount, targets, owner)`를 1개 예약 |
| 파생 GA | `ModifyStatGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/SelfDeathCondition.cs`, `Traits/ShareDamageWithAlliesEffect.cs`, `1.Systems/StatSystem.cs` |

**연결 흐름**

```
KillGA(owner) PRE
 └ Trait.Reaction  (KillGA는 IHaveCaster 아님 → 소유자 필터 전원 통과)
    └ SelfDeathCondition.SubConditionIsMat  (Token == owner 인 유닛만 통과)
       └ ShareDamageWithAlliesEffect.PerformReaction
          └ AddReaction(ModifyStatGA, DAMAGE +amount, 대상=owner 제외 아군 전원)
 └ (이후) owner 사망 처리 (KillGA performer)
```

**주의·한계**
- 공격력 "절반"의 반올림 규칙은 원작을 몰라 **내림**으로 가정했다 (원작 확인 필요). 공격력 1이면 증가 없음.
- 증가는 영구이고 지속시간·회수 로직은 없다 (설명에 지속 언급 없음).
- 효과가 예약하는 `ModifyStatGA`는 조건의 구독 대상(`KillGA`)이 아니므로 무한 루프는 없다.
- `StatSystem`이 씬에 배치되어 있어야 동작한다. 영웅 `CombatantData.Traits`에 등록하는 에디터 작업은 아직 하지 않았다 (유닛당 최대 2개). Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`SelfDeathCondition`(`reactionTiming`=PRE), `Effect`=`ShareDamageWithAlliesEffect`(`divisor`=2). `Image`는 연결되어 있다.

## 3010 · 투지

에셋: `Assets/Game/1_Datas/3.Traits/투지.asset`

| 항목 | 값 |
|---|---|
| Id | 3010 |
| 설명 (`Description`) | 피격 시 생존할 경우 공격력 +1을 얻습니다. |
| 조건 | `SelfSurvivedDamageCondition` — 구독 `DealDamageGA`, 판정: `DamagedTargets.Contains(owner) && owner.CurrentHealth > 0`. `FiltersByCaster` = false |
| 타이밍 | POST (`reactionTiming: 1`) — 피해 적용 뒤의 `CurrentHealth`로 생존을 판정한다. 사망 시 `KillGA`는 이 GA의 PostReactions에 들어가므로 조건 판정이 먼저 실행된다 |
| 효과 | `ModifyStatTraitEffect` — `modifiers = [DAMAGE +1]`, `maxTriggers` 0(무제한) |
| 효과 동작 | owner 자신에게 `ModifyStatGA(DAMAGE, 1)` 1개 예약 |
| 파생 GA | `ModifyStatGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/SelfSurvivedDamageCondition.cs`, `Traits/ModifyStatTraitEffect.cs`, `2.GameActions/DealDamageGA.cs`(`DamagedTargets`), `1.Systems/Skills/DamageSystem.cs`, `1.Systems/StatSystem.cs` |

**연결 흐름**

```
DealDamageGA 수행: DamageSystem이 target.Damage() 후 DamagedTargets.Add(target)  (회피로 건너뛴 대상은 추가 안 함)
DealDamageGA POST
 └ Trait.Reaction  (FiltersByCaster == false → Caster != owner 필터 건너뜀)
    └ SelfSurvivedDamageCondition.SubConditionIsMat  (owner가 DamagedTargets에 있고 체력 > 0)
       └ ModifyStatTraitEffect.PerformReaction
          └ AddReaction(ModifyStatGA, DAMAGE +1, 대상=owner)
```

**주의·한계**
- 계약 변경: `DealDamageGA`에 `DamagedTargets`를 추가했다(가산적). 피해량이 0인 피격도 `Damage()`가 호출되면 기록된다 (원작에서 0 피해를 피격으로 보는지는 원작 확인 필요).
- 코어(`AttackCoreGA`) 피격은 구독하지 않는다. 용병대장 소유로 쓰려면 별도 조건이 필요하다.
- 효과가 예약하는 `ModifyStatGA`는 조건의 구독 대상(`DealDamageGA`)이 아니므로 무한 루프는 없다.
- 증가는 영구이고 발동 횟수 상한이 없다 (설명에 상한 언급 없음).
- `StatSystem`이 씬에 배치되어 있어야 동작한다. 영웅 `CombatantData.Traits`에 등록하는 에디터 작업은 아직 하지 않았다. Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`SelfSurvivedDamageCondition`(`reactionTiming`=POST), `Effect`=`ModifyStatTraitEffect`(`modifiers`=[DAMAGE, 1], `maxTriggers`=0). `Image`는 연결되어 있다.

## 3011 · 의식 (몬스터용)

에셋: `Assets/Game/1_Datas/3.Traits/의식.asset`

| 항목 | 값 |
|---|---|
| Id | 3011 (`OwnerType` 1) |
| 설명 (`Description`) | 1라운드 생존 시 의식 집행인에게 공격력 +1, 체력 +1을 부여합니다. |
| 조건 | `TurnCondition` — 구독 `TurnGA`, `turnType` = Enemy (라운드 종료 = Enemy 턴 종료, `MasteredRumor`와 같은 기준). `TurnGA`는 `IHaveCaster`가 아니라 모든 소유자에게 통과 |
| 타이밍 | POST (`reactionTiming: 1`) |
| 효과 | `ModifyStatEnemyByIdEffect` — `targetEnemyId` 2004(개구리교 의식 집행관), `modifiers = [DAMAGE +1, HEALTH +1]` |
| 효과 동작 | 살아 있는 `EnemyViews` 중 `Id == 2004`인 전원을 대상으로 `ModifyStatGA(DAMAGE, 1)`, `ModifyStatGA(HEALTH, 1)` 예약 (caster = owner) |
| 파생 GA | `ModifyStatGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/TurnCondition.cs`, `Traits/ModifyStatEnemyByIdEffect.cs`, `1.Systems/StatSystem.cs` |

**연결 흐름**

```
TurnGA(Enemy) POST  (라운드 종료)
 └ Trait.Reaction  (TurnGA는 IHaveCaster가 아님 → 소유자 필터 없음)
    └ TurnCondition.SubConditionIsMat  (TurnGA.Type == Enemy)
       └ ModifyStatEnemyByIdEffect.PerformReaction
          └ AddReaction(ModifyStatGA, DAMAGE +1 / HEALTH +1, 대상=Id 2004 몬스터 전원)
```

**주의·한계**
- "1라운드 생존 시"는 **매 라운드 종료마다** 발동하는 것으로 해석했다 (사용자 확인). 소유자가 죽으면 `CombatantView.OnDisable`이 구독을 해제하므로 "생존"은 자동으로 충족된다. 발동 횟수 상한은 없다.
- 요청 문구의 "의식 집행인"은 에셋명 "개구리교 의식 집행관"(EnemyData Id 2004)과 표기가 다르다. Id로 연결했다.
- 소유자가 집행관 자신이면 의미가 없다 — 집행관이 아닌 몬스터에 등록한다.
- 효과가 예약하는 `ModifyStatGA`는 조건의 구독 대상(`TurnGA`)이 아니므로 무한 루프는 없다.
- `StatSystem`이 씬에 배치되어 있어야 동작한다. 몬스터 `CombatantData.Traits`에 등록하는 에디터 작업은 아직 하지 않았다. Play 테스트는 아직 하지 않았다.

**에디터 설정**: `Condition`=`TurnCondition`(`turnType`=Enemy, `reactionTiming`=POST), `Effect`=`ModifyStatEnemyByIdEffect`(`targetEnemyId`=2004, `modifiers`=[DAMAGE 1, HEALTH 1]). `Image`는 연결되어 있다.

## 3012 · 집중 (영웅·몬스터 공용)

에셋: `Assets/Game/1_Datas/3.Traits/집중.asset`

| 항목 | 값 |
|---|---|
| Id | 3012 (`OwnerType` 1) |
| 설명 (`Description`) | 배치 시 2라운드 동안 기절을 얻습니다. |
| 조건 | `SelfDeployedCondition` — 구독 `PlayCardGA` + `SpawnWaveGA` + `TurnGA`. 영웅은 손패 카드 배치(owner 위치 == `TargetPos`), 몬스터는 웨이브 생성(`SpawnWaveGA.SpawnedEnemies`에 owner 포함) 또는 전투 시작 때 깔리는 초기 몬스터(`TurnGA.Type == StartBattle` && owner is `EnemyView`) |
| 타이밍 | POST (`reactionTiming: 1`) — AddToken 이후라 위치 조회 가능, 새 몬스터는 `SpawnWaveGAPerformer` 안에서 이미 구독됨 |
| 효과 | `AddStatusTraitEffect` — `statusEffectType` = STUN(6), `amount` = 2 |
| 효과 동작 | owner 자신에게 `AddStatusEffectGA(STUN, 2)` 예약 |
| 파생 GA | `AddStatusEffectGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Traits/Conditions/SelfDeployedCondition.cs`, `Traits/AddStatusTraitEffect.cs`, `2.GameActions/SpawnWaveGA.cs`(`SpawnedEnemies`), `1.Systems/WaveSystem.cs` |

**연결 흐름**

```
영웅: PlayCardGA POST ─┐
몬스터: SpawnWaveGA POST ┤  (SpawnWaveGAPerformer가 AddEnemys 전후 EnemyViews 비교로 SpawnedEnemies 기록)
초기 몬스터: TurnGA(StartBattle) POST ┤
                        └ Trait.Reaction  (세 GA 모두 IHaveCaster 아님 → 소유자 필터 전원 통과)
                           └ SelfDeployedCondition.SubConditionIsMat
                              ├ PlayCardGA : GetTokenPosition(owner) == TargetPos
                              ├ SpawnWaveGA: owner is EnemyView && SpawnedEnemies.Contains(owner)
                              └ TurnGA     : Type == StartBattle && owner is EnemyView  (웨이브 몬스터는 이 시점 뒤에 생성되어 겹치지 않음)
                                 └ AddStatusTraitEffect.PerformReaction
                                    └ AddReaction(AddStatusEffectGA, STUN 2, 대상=owner)
```

**주의·한계**
- "2라운드"는 **스택 2 = 자동 전투 턴 2회**로 해석했다 (사용자 확인). 기절은 `AutoBattleTurnGA` POST마다 몬스터(`EnemySystem`)·영웅(`HeroSystem`) 모두 −1 된다. 영웅 −1은 이 특성 때문에 추가했다.
- 몬스터는 **생성될 때** 발동한다 (사용자 확정). 웨이브 몬스터는 `SpawnWaveGA`, 전투 시작 때 `TokenSetup`으로 깔리는 초기 몬스터는 GA를 거치지 않아 `TurnGA(StartBattle)` POST를 생성 직후의 **대리 시점**으로 쓴다 (생성된 그 프레임이 아니라 첫 플레이어 턴 연출 뒤, 첫 자동 전투 전). 처음에는 초기 몬스터를 제외했으나 집행관(2004)이 첫 웨이브에 있어 기절을 못 얻는 문제가 확인되어 포함했다. 보상 영웅 후보(`RewardSelectSystem.AddHero`)는 GA를 거치지 않아 여전히 제외.
- 웨이브는 `TurnGA(Player)` PRE에서 생성되므로 기절은 플레이어 턴 시작 전에 부여되고, 이후 자동 전투 턴 2회 동안 행동하지 못한다.
- 기절한 유닛은 반격 등 반응 특성은 막지 못한다 (STUN 한계 — [status_effect_catalog.md](../StatusEffect/status_effect_catalog.md)).
- 씬 `StatusEffectSystem.effectDatas`에 기절이 등록되어 있어야 한다 (`GameDemoScene`만 등록됨).
- `SpawnWaveGA`에 필드를 추가했다 (계약 변경, 사용자 승인) — `action_command.md` 갱신. 영웅 `CombatantData.Traits`에 등록하는 에디터 작업은 하지 않았다 (유닛당 최대 2개). Play 테스트는 하지 않았다.

**에디터 설정**: `Condition`=`SelfDeployedCondition`(`reactionTiming`=POST), `Effect`=`AddStatusTraitEffect`(`statusEffectType`=STUN, `amount`=2). 코드로 반영 완료. `Image`·`Description`·`OwnerType`은 기존 값 유지. 몬스터는 `개구리교 의식 집행관.asset`(Id 2004)의 `Traits`에 등록했다.
