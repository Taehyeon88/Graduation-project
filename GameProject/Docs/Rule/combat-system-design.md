# 전투 시스템 기획서

> 현재 코드(`Assets/Game/0_Scripts`)를 분석해 역추출한 **전투 시스템 스냅샷**이다. 기획 의도가 아니라 "지금 구현된 동작"을 기록한다.
> 규칙 원문은 [Rule/combat-rules.md](Rule/combat-rules.md), 개별 항목 상세는 `feature-spec/` 하위 카탈로그를 본다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> 기준 시점: 2026-11-03 / 브랜치 `main`.

---

## 1. 한눈에 보기

- **장르**: 체스 영감 덱빌딩 로그라이트. 플레이어는 카드(= 용병)를 AP로 그리드에 **배치**하고, 전투는 **속도순 자동 진행**된다.
- **승패**: 상대 **코어**(적 = 깃발, 아군 = 용병대장)의 체력을 0으로 만들면 승리/패배. 유닛 전멸은 승패 조건이 아니다.
- **한 라운드** = `플레이어 턴(준비) → 자동 전투 턴(속도순 행동) → 몬스터 턴(라운드 종료 연출)`.
- **플레이어가 개입하는 수단** (플레이어 턴 한정): 카드 배치, 영웅 이동, 카드 뽑기, 플레이어 스킬.

---

## 2. 전투판

| 항목 | 내용 |
|---|---|
| 크기 | 4×4 그리드, 좌표 `(x, y)` (규칙 문서 기준) |
| 진영 방향 | 영웅은 `+x`(오른쪽), 몬스터는 `-x`(왼쪽)로 전진 |
| 배치 구간 | 라인(y)당 1칸. 영웅 기본 `x=0`, 몬스터 기본 `x=3`. `StageData`/중립지대에 따라 바뀜 |
| 중립지대 | 이동·배치 불가 칸. 기본 배치 칸에 있으면 그 라인의 배치 위치가 **한 칸 앞으로** 이동. 중립 배치 소문을 가진 영웅만 중립지대에 배치 가능 |
| 장애물 | 파괴 가능한 `ObstacleView`. 스스로 행동하지 않으며 영웅·몬스터 모두의 공격 대상 |
| 발판(지형) | 풀숲·언덕 (§9) |
| 코어 | 그리드 **밖**에 존재. 적 코어 = `EnemyFlagView`(체력은 `StageData.EnemyCoreHealth`, 기본 10), 아군 코어 = `HeroCaptainView`(용병대장) |

스테이지 시작 시 `MatchSetupSystem`이 `맵 → 중립지대 → 장애물 → 발판 → 진영 배치 구간 → 1웨이브 몬스터 → 코어 → 덱 → 유물 → 전투 시작` 순으로 구성한다.

---

## 3. 전투 유닛

### 3-1. 종류

| 종류 | 클래스 | 전진 방향 | 공격 대상 | 이동 한계(자동 전투) |
|---|---|---|---|---|
| 영웅(용병) | `HeroView` | `+x` | 몬스터, 장애물 | 해당 라인 몬스터 배치 위치 **직전** 칸 |
| 몬스터 | `EnemyView` | `-x` | 영웅, 장애물 | 해당 라인 영웅 배치 위치 **직후** 칸 |
| 장애물 | `ObstacleView` | 없음 | — | 행동 안 함 |
| 용병대장(아군 코어) | `HeroCaptainView` | 없음 | — | 자동 전투 안 함. 특성(응수)으로만 반격 |

### 3-2. 스탯 (`CombatantData`)

| 스탯 | 의미 | 하한 |
|---|---|---|
| 체력 `Health` | 0 이하가 되면 사망(`KillGA`). 최대 체력 이상 회복 불가 | 최대 체력 ≥ 1 |
| 공격력 `Damage` | 기본 공격 피해량 | 영웅·몬스터 1, 그 외 0 |
| 속도 `Speed` | 자동 전투 행동 순서. 높을수록 먼저 | 0 (데이터 값 `-1`이면 **이동 불가**: 전투 큐 제외, UI "-", 속도 증감 무시) |
| 특성 `Traits` | 유닛당 최대 **2개** (초과분은 무시하고 에러 로그) | — |

영웅(`Hero`)은 덱에 든 개체별로 `체력/공격력/속도/치명타 보너스`, 획득 특성, 소문(영웅당 1개)을 **런 단위로 누적**한다. 카드 한 장 = 영웅 한 명이며, AP 비용은 `HeroData.APCost`.

---

## 4. 턴 구조

### 4-1. 흐름

```
[StartBattle]  AP +5, 카드 3장 드로우(startDrawAmount, "임시 값 - 원작 확인 필요")
     │
     ▼
[플레이어 턴]  ◄──────────────────────────────────────────┐
  · 0.5초 대기 → 턴 번호 +1 → 턴 팝업                      │
  · AP +3 (최대 5)                                          │
  · 웨이브 카운트 확인 / 필요 시 다음 웨이브 소환            │
  · 이동 기록 초기화(라운드 시작 위치, 배치 영웅 기록 등)    │
  · 플레이어 행동: 카드 배치 / 영웅 이동 / 카드 뽑기 / 스킬  │
  · [턴 종료 버튼]                                           │
     ▼                                                       │
[자동 전투 턴]  속도순 큐로 모든 영웅·몬스터가 차례로 행동    │
  · PRE : 영웅의 취약·약화 스택 1 감소                       │
  · POST: 영웅·몬스터 기절 스택 1 감소, 영웅별 치명타 굴림   │
     ▼  (적 코어 파괴 시 승리 → 여기서 중단)                 │
[몬스터 턴]  팝업 연출만 있고 실제 행동은 없음(라운드 종료 훅)─┘
```

- `TurnType`: `GameSetUp`, `StartBattle`, `Player`, `Enemy`, `AutoBattle`.
- 몬스터의 실제 행동은 몬스터 턴이 아니라 **자동 전투 턴에서 영웅과 한 큐로** 처리된다. `Enemy` 턴은 "라운드 종료" 신호로 쓰인다(특성 `TurnCondition`의 라운드 종료 기준도 `Enemy` 턴).
- 모든 흐름은 `ActionSystem`의 `GameAction` 파이프라인(PRE 리액션 → Performer → POST 리액션)으로 진행된다 ([feature-spec/00_action_architecture.md](feature-spec/00_action_architecture.md)).

### 4-2. 승패 처리

| 조건 | 처리 |
|---|---|
| 적 코어 체력 0 | `GameClearGA` → 승리. 용병대장 체력을 다음 스테이지로 이어감, 그리드 정리, 보상 용병 후보 제시 |
| 아군 코어(용병대장) 체력 0 | `GameOverGA` → 패배 UI |
| 둘 중 하나라도 확정 | 자동 전투 큐를 비워 즉시 종료, 이후 턴 진행 중단 |

용병대장 체력은 **런 단위로 이월**되며 스테이지 시작 시 `RestoreHealth`로 복원된다(현재 체력은 1~최대 사이로 보정).

---

## 5. 플레이어 턴 행동

### 5-1. AP와 카드

| 규칙 | 값 |
|---|---|
| 최대 AP | 5 (초과분은 획득되지 않음) |
| 전투 시작 AP | +5 |
| 플레이어 턴 시작 AP | +3 |
| 카드 사용 | 카드의 `APCost`만큼 AP 소모, 카드는 **버린 더미**로 |
| 카드 뽑기 | 버튼 클릭 시 AP 1 소모 후 1장 드로우 (규칙 문서 기준) |
| 턴 시작/종료 | 자동 드로우·자동 버리기 **없음** |
| 시작 드로우 | 전투 시작 시 `startDrawAmount`장 (규칙 문서: 3장). 값은 인스펙터 설정 |

**카드 배치**: 카드를 선택하거나 드래그해 배치 가능 타일(영웅 배치 구간 중 빈 칸 + 중립 배치 소문 시 중립지대)에 놓는다. 선택 후 우클릭 등으로 취소 가능. 사용은 `reservedCards` 큐에 쌓였다가 `ActionSystem`이 유휴일 때 순서대로 실행된다.

### 5-2. 영웅 수동 이동 (`MoveSystem`)

- 라운드 시작 위치를 중심으로 **3×3(8방향)** 안에서만 이동. 경로는 BFS, 범위 밖 우회 금지.
- 이동 불가 칸: 그리드 밖 / 유닛·장애물·중립지대 점유 칸 / **몬스터 배치 구간**. (영웅 배치 구간은 빈 칸이면 이동 가능, 시작 위치는 복귀 목적지라 항상 허용)
- **AP**: 시작 위치를 처음 떠날 때 1 소모. 3×3 안 이동은 추가 비용 없음. 시작 위치로 돌아오면 1 환급(소모한 적이 없으면 환급 없음).
- 시작 위치의 영웅이 이동 모드에 들어가려면 AP ≥ 1 필요(이미 떠난 영웅은 AP 0이어도 이동 가능).
- **카드로 배치된 영웅은 배치된 라운드에 이동 불가.** 다음 라운드부터 가능. 전투 시작 전 초기 배치 영웅은 1라운드부터 가능.
- 카드 타겟 모드 중에는 이동 모드 진입 불가(반대도 동일).
- 이동 예약 후 AP가 부족해지면 이동은 취소된다.

### 5-3. 플레이어 스킬 (`PlayerSkillSystem`)

- 스킬은 **런 전체에서 1개**, 공통 AP 1, **턴당 1회**, 전투당 `MaxUses`회.
- 사용 가능 조건: 플레이어 턴 + 액션 수행 중 아님 + 이번 턴 미사용 + 잔여 횟수 + AP.
- 현재 수록: **날카로운 단도** — 적 용병 또는 적 코어를 지정해 피해 2, **회피 무시**, 장애물 지정 불가. 시전자가 없어 힘/치명타/약화 보정을 받지 않는다. ([PlayerSkill/player_skill_catalog.md](feature-spec/PlayerSkill/player_skill_catalog.md))

---

## 6. 자동 전투

### 6-1. 행동 순서 (`AutoBattleSystem`)

1. 자동 전투 시작 시 그리드 위의 **이동 가능** `CombatantView` 전원을 `Speed` 내림차순으로 캐싱(한 번만 정렬).
2. 한 번에 1명씩 꺼내 행동. 행동이 예약한 GA가 끝난 뒤 다음 유닛이 행동(`AutoBattleGA` 연쇄).
3. 이미 죽었거나 제거된 유닛은 큐에서 건너뜀.
4. 승패가 확정되면 큐를 비우고 종료.

**동속 처리**: 최고 속도 그룹이 2명 이상이면 서로 겹치지 않는 1~6 주사위를 각자 굴려 최댓값이 먼저 행동. 그룹이 7명 이상이면 무작위 1명.

**속도 0**은 "행동 안 함"이 아니라 **가장 늦게 행동**. 속도 `-1`(이동 불가)만 큐에서 제외된다.

**기절**: 차례가 오면 행동하지 않고 건너뜀(추가 공격 차례도 불가). 스택은 자동 전투 턴 종료 시 영웅·몬스터 모두 1 감소.

### 6-2. 한 유닛의 행동 (`CombatantView.Battle`)

```
1. 특성이 행동을 대체하면(예: 저격) 기본 행동 없이 종료
2. 이동 한계 전이고 정면 1칸이 비어 있으면 → 1칸 전진(MoveGA)
3. 전진했다고 가정한 위치 기준으로:
     · 정면에 공격 대상이 있으면      → 기본 공격(DealDamageGA, 공격력)
     · 이동 한계이고 정면이 비어 있으면 → 상대 코어 공격(AttackCoreGA)
     · 그 외(정면이 아군 등 막힘)       → 아무것도 안 함
```

- 한 번의 행동에서 **이동은 최대 1칸**, 그 뒤 공격 판정까지 한 번에 수행한다.
- 정면에 아군이 있으면 이동도 공격도 못 하고 대기한다.
- 정면이 장애물이면 장애물을 공격한다(영웅·몬스터 공통).
- 공격 연출: 근접은 돌진 모션 후 타격 시점에 피해 적용, 원거리(저격)는 제자리에서 화살 도착 시점에 피해 적용.

### 6-3. 추가 공격 (`ExtraAttackGA`)

특성 "공격 신호"가 예약. 이동 없이 현재 위치에서 정면 공격만 다시 예약한다. 기절하면 불가, 승패 확정 시 중단.

---

## 7. 피해 계산

### 7-1. 유닛 피해 (`DamageCaculator.GetDamage`)

```
피해 = 기본 피해(공격력 또는 GA 지정량)
     + 힘(POWER) 스택
     + 치명타(CRITICAL) 보유 시 공격자 공격력 × 0.5
피해 × 0.75   ← 공격자가 취약(WEAK) 스택 보유 시
피해 × 1.5    ← 대상이 약화(VULNERABLE) 스택 보유 시
최종 = 올림(Ceil)
```

- 시전자가 없는 피해(플레이어 스킬)는 공격자 보정 생략.
- 대상 쪽 보정(약화)은 `CombatantView` 대상에만 적용.

### 7-2. 코어 피해 (`CoreSystem.AttackCoreGA`)

- 영웅 → 적 코어, 몬스터 → 아군 코어. 피해량 = `Amount` 또는 공격자 `BaseDamage`, 공격자 보정(힘/치명타/취약)은 동일하게 적용, **대상 보정은 없음**.
- `IsMissed` 공격은 모션만 재생하고 피해·승패 판정 생략("면역" 표시).
- 코어는 방어막이 없다 — 용병대장은 체력만 직접 차감(`ReceiveCoreDamage`).
- 코어 체력이 0이 되면 즉시 `GameClearGA` 또는 `GameOverGA` 예약.

### 7-3. 피해 처리 순서 (`DamageSystem.DealDamageGA`)

1. (기본 공격이면) 공격 모션 재생 → 타격 시점까지 대기
2. 대상별로 **회피** 검사 → 회피 보유 시 스택 1 소모, 피해·피격 연출 생략 (`IgnoreDodge`면 건너뜀)
3. 피해 계산 → 피격 VFX → `Damage()` 적용 → `DamagedTargets`에 기록
4. 체력 0이면 `KillGA`를 GA의 POST 리액션에 등록 (피격 흔들림 연출 후 토큰 제거)
5. 피해가 실제로 적용된 공격 1회마다 시전자의 **치명타 스택 1 소모**(다중 대상은 전원 적용 후 1회만)

---

## 8. 상태효과 (`StatusEffectType`)

| 상태 | 효과 | 감소 시점 |
|---|---|---|
| 힘 `POWER` | 공격 피해 +스택 | 자동 소모 없음 |
| 취약 `WEAK` | 이 유닛이 주는 피해 ×0.75 | 영웅: 자동 전투 턴 **시작 시** 1 감소 |
| 약화 `VULNERABLE` | 이 유닛이 받는 피해 ×1.5 | 영웅: 자동 전투 턴 **시작 시** 1 감소 |
| 회피 `DODGE` | 다음 1회 피해 무효 | 피해를 막을 때 1 소모 |
| 치명타 `CRITICAL` | 공격 시 공격력의 50% 추가 피해 | 피해가 적용된 공격 1회마다 1 소모 |
| 기절 `STUN` | 자동 전투 차례의 모든 행동 불가 | 자동 전투 턴 종료 시 영웅·몬스터 1 감소 |

- **치명타 획득**: 자동 전투 턴 종료마다 영웅별로 `Critical`(%) 확률 굴림 → 성공 시 CRITICAL 1스택.
- 상태 UI 표시용 데이터는 `StatusEffectSystem.effectDatas`에 등록되어 있어야 한다. 상세: [StatusEffect/status_effect_catalog.md](feature-spec/StatusEffect/status_effect_catalog.md).

---

## 9. 지형과 장애물

| 종류 | 효과 |
|---|---|
| 풀숲 | 올라선 용병에게 **회피 1** 부여, 떠나면 그 발판이 준 회피를 회수 |
| 언덕 | 이동 기준칸이 언덕이면 **바로 뒤 칸을 제외한 모든 빈 칸**(몬스터 배치 구간 제외)으로 이동 가능. 기본 3×3 밖으로 이동하면 공격력 +1, 속도 +1 **영구**(영웅당 라운드 1회) |
| 돌격 화살통 (장애물) | 파괴한 용병이 모든 적을 추가 공격(처치자 공격력만큼) |
| 야전 공구함 (장애물) | 피해를 받을 때마다 공격자에게 공격력 +1, 체력 +1 |

상세: [Terrain/terrain_catalog.md](feature-spec/Terrain/terrain_catalog.md), [08_terrain_obstacle.md](feature-spec/08_terrain_obstacle.md).

---

## 10. 특성 · 소문 · 유물

| 계층 | 범위 | 비고 |
|---|---|---|
| 특성(Trait) | 유닛당 최대 2개. `조건(Condition) + 효과(Effect)`를 `ActionSystem` 리액션으로 구독 | 현재 12종(영웅·몬스터·장애물·용병대장). 목록은 [Trait/trait_catalog.md](feature-spec/Trait/trait_catalog.md) |
| 소문(Rumor) | 영웅당 1개, 유닛별 사본 | 강력한 / 유연한 / 숙달된 ([Rumor/rumor_catalog.md](feature-spec/Rumor/rumor_catalog.md)) |
| 유물(Perk) | 런 단위 소유, 전투 시작 시 구독 | 현재 검푸른 망토 1종 ([Perk/perk_catalog.md](feature-spec/Perk/perk_catalog.md)) |

특성의 행동 대체(`IBattleOverride`) 예: **저격** — 이동·정면 공격 대신 무작위 적(없으면 적 코어)을 원거리 공격하고 매 라운드 공격력 −1(최소 1).
특성 발동 기준 타이밍 요약: 사망 계열은 `KillGA` **PRE**(토큰 제거 전), 피격·공격 후 계열은 `DealDamageGA`/`AttackCoreGA` **POST**.

---

## 11. 몬스터 웨이브 (`WaveSystem`)

- 스테이지 `WaveData`: 웨이브별 몬스터 수, 웨이브 간 대기 턴, 몬스터별 생성 라인.
- 첫 웨이브는 전투 시작 전에 소환. 이후 **플레이어 턴 시작 PRE**마다 대기 카운트를 확인해 0 이하이면 `SpawnWaveGA`로 소환한 뒤 카운트를 1 감소.
- 소환 위치 = 라인의 몬스터 배치 위치. 배치 위치가 없거나 이미 점유된 몬스터는 **생성되지 않는다**(경고 로그).
- 마지막 웨이브 소환 후에는 더 이상 소환하지 않는다.

---

## 12. 구현 구조 요약

| 역할 | 클래스 |
|---|---|
| 액션 파이프라인 | `ActionSystem`, `GameAction` (PRE/POST 리액션) |
| 턴 | `TurnSystem` (`TurnGA`, `AutoBattleTurnGA`) |
| 자동 전투 | `AutoBattleSystem` (`AutoBattleGA`, `ExtraAttackGA`), `CombatantView.Battle/ReserveAttack` |
| 피해 | `DamageSystem` (`DealDamageGA`, `KillGA`), `DamageCaculator`, `CoreSystem` (`AttackCoreGA`) |
| 이동 | `MoveSystem` (`PerformMoveGA`, `MoveGA`), `KnockBackProcessor` (`KnockBackGA`) |
| 자원 | `APSystem` (`SpendAPGA`, `RefillAPGA`), `CardSystem` (`DrawCardsGA`, `PlayCardGA`) |
| 진영 | `HeroSystem`, `EnemySystem`, `WaveSystem` |
| 부가 | `StatusEffectSystem`, `StatSystem`(`ModifyStatGA`), `TerrainSystem`, `PerkSystem`, `PlayerSkillSystem` |
| 종료 | `GameSystem` (`GameClearGA`, `GameOverGA`) |

---

## 13. 관찰된 불일치 · 확인 필요 항목

코드를 읽으며 발견한 것들이다. **고치지 않았고**, 의도인지 확인이 필요하다.

| # | 내용 | 위치 |
|---|---|---|
| 1 | `WEAK`/`VULNERABLE`의 이름이 서로 뒤바뀐 듯한 표기. 열거형 주석은 `WEAK`="취약", `VULNERABLE`="약화"인데 계산 함수 주석은 `CalculateWeak`="취약 연산"(공격자 ×0.75), `CalculateVulnerable`="약화 연산"(피격자 ×1.5). 일반적인 용어(취약 = 받는 피해 증가, 약화 = 주는 피해 감소)와 **반대**다 | `StatusEffectType.cs`, `DamageCaculator.cs` |
| 2 | 취약·약화 스택의 자동 감소(`ReduceSEWhenMyTurnEnd`)가 **영웅에게만** 호출된다. 몬스터에게 걸린 취약/약화는 줄지 않는다 | `HeroSystem.cs`, `EnemySystem.cs` |
| 3 | 취약·약화는 스택 수와 무관하게 **보유 여부**만 보고 고정 배율을 적용한다 (스택은 지속 턴 역할) | `DamageCaculator.cs` |
| 4 | 속도 순서는 자동 전투 시작 시 **1회 캐싱**한다. 전투 도중 속도가 바뀌어도(공격 신호 등) 이번 큐 순서는 바뀌지 않는다 | `AutoBattleSystem.CacheCombatants` |
| 5 | 코어 공격은 대상 보정을 받지 않아 같은 공격력이라도 유닛 공격과 피해량이 다를 수 있다 | `CoreSystem.cs` |
| 6 | 몬스터 턴(`Enemy`)에는 행동이 없고 팝업과 라운드 종료 훅 역할만 한다. 원작도 같은 구조인지 | `TurnSystem.cs` |
| 7 | 시작 드로우 장수(`startDrawAmount`)가 인스펙터 값이며 "임시 값 - 원작 확인 필요"로 표시되어 있다 | `CardSystem.cs` |
| 8 | 배치 가능한 몬스터 생성 위치가 점유되어 있으면 해당 몬스터는 소환되지 않고 사라진다(대기열에 남지 않음) | `WaveSystem.BuildSpawn` |
| 9 | 4×4 크기·AP 수치는 `combat-rules.md` 기준이며 코드의 `gridWidth/Height`, `DrawCardUI` 비용은 이번에 직접 확인하지 않았다 | — |

---

## 14. 관련 문서

- [Rule/combat-rules.md](Rule/combat-rules.md) — 전투 규칙 원문
- [game-design.md](game-design.md) — 전체 기획서
- [feature-spec.md](feature-spec.md) — 챕터 목차
- [feature-spec/00_action_architecture.md](feature-spec/00_action_architecture.md), [feature-spec/action/action_command.md](feature-spec/action/action_command.md) — GA 계약
- [feature-spec/03_wave_system.md](feature-spec/03_wave_system.md) — 웨이브
