# 특성(Trait) 부품 레지스트리 — 재사용 가능한 조건·효과

> `/trait` 커맨드와 [trait_howto.md](../../howto/trait_howto.md)가 **새 특성을 만들기 전에 가장 먼저 대조하는 문서**. 이미 있는 `TraitCondition`/`TraitEffect`를 재사용하는 것이 우선이고, 맞는 부품이 없을 때만 새로 만든다.
> 구조·계약은 [05_trait_system.md](../05_trait_system.md), 완성된 특성별 연결은 [trait_catalog.md](./trait_catalog.md).
> **새 조건/효과를 만들면 반드시 이 문서의 표에 행을 추가한다.** 값은 코드와 `Assets/Game/1_Datas/3.Traits/*.asset`을 읽어 확인한 것만 적는다.

## 1. 재사용 판단 규칙
1. **완전 일치** → 그대로 사용.
2. **구독 GA·판정은 같고 수치만 다름** → 기존 클래스 + 인스펙터(`SerializeField`) 값만 바꾼다.
3. **판정 일부만 겹침** → 확장(필드 추가)할지 새 클래스로 만들지 사용자에게 먼저 묻는다.
4. **없음** → 새로 만들고 아래 표에 등록한다.

## 2. 조건 (`TraitCondition`) — 위치 `Assets/Game/0_Scripts/Traits/Conditions/`

| 클래스 | 구독 GA | 타이밍 | 판정 | 노출 필드 | 사용 특성 |
|---|---|---|---|---|---|
| `DealDamageCondition` | `DealDamageGA` | 인스펙터 (사용 예: POST) | 항상 true — 소유자 필터(`Caster == owner`)만 적용 | 없음 | 1 피묻은 도끼 |
| `TurnCondition` | `TurnGA` | 인스펙터 (사용 예: POST) | `TurnGA.Type == turnType` | `turnType` (`TurnType`: Enemy/Player/StartBattle/GameSetUp/AutoBattle) | 2 근육 트로피 (StartBattle) |
| `AutoBattleTurnCondition` | `AutoBattleTurnGA` | 인스펙터 (사용 예: PRE) | 항상 true | 없음 | 3 용사의 갑옷, 3006 저격 (POST) |
| `SelfDeathCondition` | `KillGA` | **PRE 필수** (POST는 구독 해제로 놓칠 수 있음) | `KillGA.Token == owner` | 없음 | 3001 불경한 빛 |
| `SelfPlacedCondition` | `PlayCardGA` | POST | owner 위치 == `TargetPos` (손패 카드 배치만). `requireNoOpponentInFront`가 켜져 있으면 추가로 정면 1칸(`owner.FrontDirection`)이 상대 진영 유닛이 아닐 때만 충족 (빈칸·아군·범위 밖은 충족) | `requireNoOpponentInFront` (기본 false) | 3004 빠른 발 (true) |
| `AllyPlacedNearbyCondition` | `PlayCardGA` | POST | owner가 영웅이고 `TargetPos`가 owner와 1~`range`칸(8방향, 자기 배치 제외) | `range` (기본 1) | 3003 요새화 |
| `AttackedOpponentCondition` | `DealDamageGA` + `AttackCoreGA` | POST | 대상이 상대 진영이거나 코어 공격 | 없음 | 3005 맹공, 3007 공격 신호 |
| `AuraTriggerCondition` | `PlayCardGA`(인스펙터 타이밍, 권장 POST) + `KillGA`(**PRE 고정**) | PlayCardGA만 인스펙터 | 배치: `TargetPos`의 토큰이 owner와 같은 진영(자신 포함). 사망: `KillGA.Token == owner`. owner가 파괴된 뷰면 false | 없음 | 3002 신속의 기도 |

## 3. 효과 (`TraitEffect`) — 위치 `Assets/Game/0_Scripts/Traits/` (조건과 달리 하위 폴더 없음)

| 클래스 | 예약 GA | 노출 필드 | 동작 | 사용 특성 |
|---|---|---|---|---|
| `HealOnDamageEffect` | `HealGA` | `addedAmount`(5), `healAmount`(5) | 확률 판정 통과 시 owner 위치에 `healAmount` 회복 | 1 피묻은 도끼 |
| `AddStatusTraitEffect` | `AddStatusEffectGA` | `statusEffectType`, `amount`(1) | owner 자신에게 상태이상 `amount` 스택 부여 | 2 근육 트로피 (POWER 1), 3 용사의 갑옷 (ARMOR 3) |
| `ModifyStatTraitEffect` | `ModifyStatGA` | `modifiers` (목록: `statType`(`StatType`: HEALTH/DAMAGE/SPEED) + `amount`), `maxTriggers`(0 = 무제한, `ILimitedTrigger` 구현) | owner 자신의 스탯을 항목마다 `amount`만큼 증가, 항목당 `ModifyStatGA` 1개 예약 (HEALTH는 최대·현재 체력 함께) | 3003 요새화 ([HEALTH 2]), 3004 빠른 발 ([DAMAGE 1, SPEED 2]), 3005 맹공 ([DAMAGE 1], `maxTriggers` 4) |
| `DamageAllOpponentsBySpeedEffect` | `DealDamageGA`(Targets) + `AttackCoreGA` | 없음 | owner 진영의 상대 전원과 상대 코어에 `owner.Speed`만큼 피해 | 3001 불경한 빛 |
| `ExtraAttackEffect` | `ModifyStatGA`(SPEED, 음수) + `ExtraAttackGA` | `speedCost`(2) | `owner.Speed`가 `speedCost` 미만이면 무시. 아니면 `speedCost`만큼 속도를 영구 감소시키고 이동 없이 정면 공격을 한 번 더 예약 (추가 공격이 조건을 다시 충족하면 속도가 `speedCost` 미만이 될 때까지 반복). 권장 POST | 3007 공격 신호 |
| `AllySpeedAuraEffect` | `ModifyStatGA`(SPEED, 부여는 양수·회수는 음수) | 없음 | `PlayCardGA`: owner가 배치되면 나머지 아군 전원, 다른 아군이 배치되면 그 유닛에게 `owner.Speed`만큼 SPEED 증가 (owner의 특성 중 같은 효과 클래스를 쓰는 특성을 가진 유닛·`owner.Speed <= 0`은 제외 — 그 특성 Id는 `CombatantView.TryGetTraitIdByEffect`로 첫 부여 때 조회해 캐시), 부여량을 효과 사본의 `speedGrants`에 기록. `KillGA`(owner 사망): 기록한 부여분을 살아 있는 대상에서 회수. 권장 조건 `AuraTriggerCondition` | 3002 신속의 기도 |
| `SniperEffect` (`IBattleOverride` 구현) | `ModifyStatGA`(DAMAGE 음수) + `DealDamageGA`(Target, 기본 공격) / `AttackCoreGA` | `damageDecay`(1), `minDamage`(1) | `PerformReaction`: `Mathf.Min(damageDecay, owner.BaseDamage - minDamage)`가 0 이하면 무시, 아니면 공격력을 그만큼 감소. `TryBattle`(자동 전투 차례): 상대 진영 유닛 중 무작위 1명을 `owner.BaseDamage`로 공격, 상대가 없으면 상대 코어 공격 (둘 다 `IsRangedAttack`=true라 제자리에서 화살 발사 모션). 항상 true를 반환해 `CombatantView.Battle`의 기본 이동·정면 공격을 건너뜀. 권장 조건 `AutoBattleTurnCondition` (POST) | 3006 저격 |

**발동 횟수 상한**: `TraitEffect` 베이스에는 상한 필드가 없다. 상한이 필요한 효과는 자기 클래스에 `maxTriggers` 데이터를 두고 `ILimitedTrigger`를 구현한다 (현재 `ModifyStatTraitEffect`만 구현). 카운트는 `Trait` 런타임 객체가 유닛별로 보유하고 리셋 없이 누적된다 (계약은 [05_trait_system.md](../05_trait_system.md) §2). 다른 효과에 "N번 발동"이 필요하면 그 효과에 같은 방식으로 추가한다.

`StatusEffectType`: POWER(힘), ARMOR(방어), WEAK(취약), VULNERABLE(약화).

## 4. 조건만 연결되고 효과가 비어 있는 에셋 (관찰)
`3.Traits/`의 아래 에셋은 `Effect`가 비어 있다 (`rid: -2`). 효과가 정해지면 이 문서와 [trait_catalog.md](./trait_catalog.md)에 반영한다.
- 없음 (3006 저격까지 모두 연결됨)

## 5. 사용 시 주의가 필요한 부품
- `ModifyStatGA` + `StatType`(HEALTH/DAMAGE/SPEED): Performer는 `StatSystem`(씬의 `--- SYSTEMS ---` 아래에 GameObject로 배치해야 동작). HEALTH는 요새화, DAMAGE·SPEED는 빠른 발에서 쓰이며 세 분기 모두 Play 검증은 아직이다. 음수 증감은 사망 처리가 없어 **양수 증가 전용**으로 쓴다.

- `ModifyStatGA`의 DAMAGE 음수 증감은 3006 저격이 처음 쓴다. `ModifyStat`에 하한이 없어 하한은 효과(`minDamage`)가 보장한다.
- `IBattleOverride`를 구현한 효과는 `Trait.TryOverrideBattle`로 자동 전투 행동을 대체한다. 특성이 여럿이면 먼저 true를 반환한 쪽이 이기고 나머지는 실행되지 않는다. `ExtraAttackGA`(공격 신호)는 `Battle()`을 거치지 않아 대체되지 않는다.

## 6. 관찰된 불일치 (고치지 않고 보고만)
- 에셋 파일명: `번호_이름`(1·2·3번)과 이름만(3001번대)이 섞여 있다. `Trait.Title`은 `_` 뒤쪽을 잘라 만든다.
- Id 체계: 1~3과 3001~ 두 구간. 마지막으로 확인된 Id는 3007이다.
- `1_피묻은 도끼.asset`의 필드명이 `added_Amount`/`heal_Amount`로 저장되어 있다(`FormerlySerializedAs` 이전 이름이라 재저장 시 갱신됨).
