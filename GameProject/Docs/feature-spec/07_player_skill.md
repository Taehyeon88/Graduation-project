# 07. PlayerSkill — 플레이어 스킬 (날카로운 단도)

> 플레이어(용병대장 측)가 UI 버튼으로 직접 쓰는 스킬. **영웅이 쓰는 옛 수동 스킬(`SkillSystem`, [02_skill_system.md](02_skill_system.md), 제거됨)과 별개**이며 `Skill`/`SkillData`/`Effect` 체인을 쓰지 않는다.
> 원작(Master of Piece) 규칙은 코드로 확인하지 못했다. 아래 `(원작 확인 필요)` 항목은 임시 결정이다.

## 규칙

- **날카로운 단도**: 적 용병 또는 우두머리를 지정해 2의 피해. 회피를 무시한다. 장애물은 지정할 수 없다.
- 사용 횟수 제한 **전투당 2회** (`StartBattle` 시 초기화).
- AP **1** 소모 (모든 플레이어 스킬 공통 상수 `PlayerSkillSystem.APCost`). AP가 부족하면 사용할 수 없다.
- **모든 플레이어 스킬은 한 턴에 1번만** 사용 가능 (플레이어 턴 시작 시 리셋). 플레이어 스킬은 현재 1개뿐이다.
- 사용 방법: Bottom Canvas의 `UseSkillUI` 버튼을 **클릭 후 드래그해서 대상 위에 놓는다**.

## 결정 사항 / 원작 확인 필요

- **우두머리 = 적 코어(`EnemyFlagView`)**. 피해는 `AttackCoreGA`로 처리한다. (`trait_catalog.md`의 "적 깃발/우두머리 = 상대 코어" 해석과 같다.)
- **회피 무시 시 대상의 DODGE 스택은 소모하지 않고** 피해만 적용한다. (원작 확인 필요)
- 횟수 제한의 단위(전투당 / 런당)는 **전투당**으로 정했다. (원작 확인 필요)
- 플레이어 스킬은 시전 용병이 없어 `Caster`가 null이다 → 힘(POWER)/약화는 적용되지 않고 대상의 취약만 적용된다. 이 처리가 원작과 같은지 (원작 확인 필요)
- 파괴 가능 장애물(`ObstacleView`)은 토큰이지만 `IsValidTarget`이 `EnemyView`/`EnemyFlagView`만 허용하는 허용 목록 방식이라 지정되지 않는다(중립지대는 토큰이 아님). 새 토큰 종류를 추가할 때도 이 허용 목록 방식을 유지한다 — [08_terrain_obstacle.md](08_terrain_obstacle.md).

## 구성

| 파일 | 역할 |
|---|---|
| `Models/PlayerSkill.cs` | 추상 베이스. `IsValidTarget(Token)`, `CreateAction(Token)`만 정의 (스킬별 차이) |
| `PlayerSkills/SharpDagger.cs` | 날카로운 단도. `damage`(기본 2) 필드. `EnemyView` → `DealDamageGA(…, ignoreDodge: true)`, `EnemyFlagView` → `AttackCoreGA(null, damage)` |
| `5.Data/PlayerSkillData.cs` | SO. `Id`, `Description`, `Image`, `MaxUses`, `[SerializeReference, SR] PlayerSkill Skill`. 에셋: `1_Datas/10.PlayerSkills/SharpDagger.asset` |
| `1.Systems/PlayerSkillSystem.cs` | `Singleton`. 공통 규칙(AP·턴당 1회·전투당 횟수·플레이어 턴 한정) 판정, `CanUse()`, `TryUse(Token)`, `RemainingUses`, `SkillData` |
| `UI/UseSkillUI.cs` | 드래그 입력. 놓은 위치의 토큰을 찾아 `TryUse` 호출 |

**전용 GA는 없다.** `UseSkillUI`가 `PlayerSkillSystem.TryUse`를 직접 호출하고, `TryUse`가 스킬이 만든 GA(`DealDamageGA`/`AttackCoreGA`)의 `PreReactions`에 `SpendAPGA(1)`을 붙여 `ActionSystem.Perform`한다.

## 흐름

1. `UseSkillUI.OnBeginDrag` — `Interactions`의 `IsSetUpHero`/`IsHeroMoveMode`/`IsCardTargetMode`/`IsPileViewOpen`이 꺼져 있고 `PlayerSkillSystem.CanUse()`가 true일 때만 드래그를 인정.
2. `OnEndDrag` — 대상 판정: 먼저 `EnemySystem.Instance.EnemyFlag.Model.bounds`(그리드 밖이라 스프라이트 영역으로 판정), 아니면 `MouseIsoTilePosition(1)` → `IsBound` → `GetTokenByPosition`.
3. `PlayerSkillSystem.TryUse(target)` — `CanUse()` && `Skill.IsValidTarget(target)` 실패 시 AP·횟수 미소모. 성공 시 `CreateAction` → `SpendAPGA` PreReaction 부착 → `RemainingUses--`, `usedThisTurn = true` → `Perform`.
4. 리셋 — `TurnGA` PRE: `StartBattle`이면 `RemainingUses = MaxUses` + 턴 플래그 해제, `Player`이면 턴 플래그 해제.

## 기존 코드에 가한 변경

- `DealDamageGA.IgnoreDodge` 추가(6인자 생성자 오버로드). `DamageSystem.DealDamageGAPerformer`의 회피 분기 두 곳(다중/단일 대상)이 `IgnoreDodge`이면 회피를 건너뛴다.
- `CoreSystem.AttackCoreGAPerformer`: `Caster == null`이면 플레이어 스킬로 보고 모션 없이 적 코어에 `Amount`만큼 피해(게임 클리어 판정은 기존 경로 그대로).
- `CoreAttackedCondition`: `Caster != null` 조건 추가 — 플레이어 스킬이 적 코어를 칠 때 용병대장의 응수 특성이 잘못 발동하지 않도록.

## 알려진 제약

- 횟수·턴 플래그는 `Perform` 직전에 갱신한다. 스킬 실행이 중간에 실패해도 소모된다(현재 실패 경로 없음).
- 드래그 중 대상 하이라이트, 남은 횟수 표시, 사용 불가 시 버튼 비활성 표시는 없다.
- 플레이어 스킬이 2개 이상이 되면 `PlayerSkillSystem`(단일 `skillData`/`RemainingUses`)의 구조 변경이 필요하다.
