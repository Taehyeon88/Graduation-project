# 플레이어 스킬(PlayerSkill) 카탈로그 — 이름·설명·대상·GA 연결

> 플레이어 스킬 하나가 어떤 `PlayerSkill` 구현체와 `PlayerSkillData` 에셋으로 정의되고, 어떤 대상에 어떤 GA를 만들어 실행하는지 **스킬 단위**로 정리한 문서. 구조·흐름·결정 사항은 [07_player_skill.md](../07_player_skill.md), 제작 절차는 [playerskill_howto.md](../../howto/playerskill_howto.md)를 본다.
> 새 플레이어 스킬(`PlayerSkill` 구현체 + `PlayerSkillData` 에셋)을 만들면 이 문서에 섹션을 추가한다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> **현재 수록: 1개** (날카로운 단도). 플레이어 스킬은 현재 1개만 존재하며, `PlayerSkillSystem`과 `UseSkillUI`도 1개 전제 구조다.

## 요약

| Id | 구현체 | 이름 | 설명 (`Description`) | 대상 | 실행 GA | 횟수 |
|---|---|---|---|---|---|---|
| 1 | `SharpDagger` | 날카로운 단도 | 적 용병 또는 우두머리를 지정해 2의 피해를 줍니다. 회피를 무시하며, 장애물은 지정할 수 없습니다. | `EnemyView`, `EnemyFlagView` | `DealDamageGA` / `AttackCoreGA` | 전투당 2회 |

**모든 플레이어 스킬 공통 규칙** (스킬 클래스가 아니라 `PlayerSkillSystem`이 처리)

| 규칙 | 값 | 위치 |
|---|---|---|
| AP 소모 | 1 (AP 부족 시 사용 불가) | `PlayerSkillSystem.APCost` (`const int`) — SO 필드가 아니라 공통 상수 |
| 턴당 사용 | 한 턴에 1번 (플레이어 턴 시작 시 리셋) | `PlayerSkillSystem.usedThisTurn` |
| 전투당 횟수 | `MaxUses` (`StartBattle` 시 초기화) | `PlayerSkillData.MaxUses` / `PlayerSkillSystem.RemainingUses` |
| 사용 가능 시점 | 플레이어 턴 && `ActionSystem` 수행 중 아님 | `PlayerSkillSystem.CanUse()` |
| 사용 방법 | Bottom Canvas의 `UseSkillUI` 버튼을 클릭 후 드래그해 대상 위에 놓기 | `UI/UseSkillUI.cs` |

- 에셋 위치: `Assets/Game/1_Datas/10.PlayerSkills/`. 구현체 위치: `Assets/Game/0_Scripts/PlayerSkills/`. 베이스: `Models/PlayerSkill.cs`.
- `Skill` 필드는 `[SerializeReference, SR]`라 에셋의 `.asset` 파일에서 `rid`/`type: {class: …}`로 구현체를 가리킨다.
- 에셋은 씬 `--- SYSTEMS ---/PlayerSkillSystem` 컴포넌트의 `Skill Data` 슬롯에 연결해야 쓸 수 있다 (현재 연결: `SharpDagger.asset`).
- 전용 GA는 없다. `UseSkillUI`가 `PlayerSkillSystem.TryUse`를 직접 호출하고, 스킬이 만든 GA의 `PreReactions`에 `SpendAPGA(1)`이 붙는다.

## 1 · 날카로운 단도 (SharpDagger)

에셋: `Assets/Game/1_Datas/10.PlayerSkills/SharpDagger.asset`

| 항목 | 값 |
|---|---|
| 구현체 | `SharpDagger : PlayerSkill` (`PlayerSkills/SharpDagger.cs`) |
| Id | 1 |
| 설명 (`Description`) | 적 용병 또는 우두머리를 지정해 2의 피해를 줍니다. 회피를 무시하며, 장애물은 지정할 수 없습니다. |
| 수치 | 피해 2 (`SharpDagger.damage`, 에셋 `data.damage`), 횟수 `MaxUses` = 2 |
| 지정 가능한 대상 | `EnemyView`(적 용병), `EnemyFlagView`(우두머리 = 적 코어). 그 외(아군·빈 칸)는 `IsValidTarget`이 거부 |
| 장애물 | 지정 불가 — 파괴 가능 장애물(`ObstacleView`)은 토큰이지만 `IsValidTarget`이 `EnemyView`/`EnemyFlagView`만 허용하는 허용 목록 방식이라 지정되지 않는다. 중립지대는 토큰이 아니라 대상 조회에 잡히지 않는다 ([08_terrain_obstacle.md](../08_terrain_obstacle.md)) |
| 실행 GA | 적 용병 → `DealDamageGA(2, target, null, false, false, ignoreDodge: true)` / 우두머리 → `AttackCoreGA(null, 2)` |
| 회피 처리 | `DealDamageGA.IgnoreDodge`가 true라 `DamageSystem`이 회피 분기를 건너뜀 → **DODGE 스택을 소모하지 않고 피해만 적용** (원작 확인 필요) |
| 피해 보정 | `Caster`가 null이라 힘(POWER)·약화는 적용되지 않고 대상의 취약만 적용 (원작 확인 필요) |
| 아이콘 | `Image` 비어 있음 (임시) |
| 코드 | `PlayerSkills/SharpDagger.cs`, `1.Systems/PlayerSkillSystem.cs`, `UI/UseSkillUI.cs`, `1.Systems/Skills/DamageSystem.cs`, `1.Systems/CoreSystem.cs` |

**연결 흐름**

```
UseSkillUI.OnBeginDrag  (Interactions 모드 플래그 꺼짐 && PlayerSkillSystem.CanUse())
UseSkillUI.OnEndDrag → GetTokenUnderMouse
 ├ EnemyFlag.Model.bounds 안  → EnemyFlagView
 └ MouseIsoTilePosition(1) → IsBound → GetTokenByPosition → Token (없으면 null)
 └ PlayerSkillSystem.TryUse(target)   (CanUse() && SharpDagger.IsValidTarget 실패 시 AP·횟수 미소모)
    ├ SharpDagger.CreateAction(target)
    ├ skillAction.PreReactions.Add(SpendAPGA(1))   → 실행 전에 AP 1 차감
    ├ RemainingUses--, usedThisTurn = true
    └ ActionSystem.Perform(skillAction)
       ├ EnemyView     : DealDamageGA → DamageSystem.DealDamageGAPerformer
       │                  └ IgnoreDodge → 회피 판정 건너뜀 → DamageCaculator(취약만) → PlayDamageVFX → Damage()
       │                     └ 체력 0 이하 → KillGA
       └ EnemyFlagView : AttackCoreGA(Caster=null) → CoreSystem.AttackCoreGAPerformer
                          └ 모션 생략 → EnemyCoreHealth −2 → UpdateHealth/PlayHitShake
                             └ EnemyCoreHealth ≤ 0 → GameClearGA   (IsMissed면 피해·승패 판정 생략)
```

리셋: `TurnGA` PRE 리액션 — `StartBattle`이면 `RemainingUses = MaxUses`·턴 플래그 해제, `Player`이면 턴 플래그 해제.

**주의·한계**
- 위 수치·회피 스택 비소모·취약만 적용·횟수 단위(전투당)는 **원작을 확인하지 못해 임시로 정한 값**이다 (`(원작 확인 필요)`). 횟수 제한의 단위가 런당이면 `PlayerSkillSystem`의 초기화 위치가 바뀐다.
- `Caster`가 null이라 다음이 성립한다. `CoreSystem`은 `Caster == null`을 플레이어 스킬 공격으로 보고 모션 없이 적 코어에 `Amount`만큼 피해를 준다(`Amount`가 null이면 0). `CoreAttackedCondition`은 `Caster != null` 가드가 있어 용병대장 응수 특성이 발동하지 않는다. 소유자 특성(`FiltersByCaster`)은 null Caster를 걸러내므로 반응하지 않는다.
- 횟수·턴 플래그는 `Perform` 직전에 소모된다. GA가 도중에 실패해도 소모된다 (현재 실패 경로 없음).
- 우두머리 드롭 판정은 콜라이더가 아니라 `EnemyFlagView.Model.bounds`(스프라이트) 기준이다. 스프라이트가 작거나 카메라가 원근이면 어긋날 수 있다.
- `UseSkillUI`는 영웅 배치·이동 모드·카드 대상 지정·더미 보기 중에는 드래그를 무시한다.
- 드래그 중 대상 하이라이트, 남은 횟수 텍스트, 사용 불가 시 버튼 비활성 표시, 우클릭 취소는 없다. 빈 칸·아군·장애물 위에서 놓으면 아무 일도 일어나지 않는다.
- **Play 테스트는 하지 않았다.** 컴파일과 에셋 재로딩만 확인했고, 드래그 판정·피해·코어 처리는 실제 플레이로 확인되지 않았다.

**에디터 설정**: 에셋 `Id`=1, `Description` 입력, `MaxUses`=2, `Skill`=`SharpDagger`(`damage`=2). 씬 `PlayerSkillSystem.Skill Data`에 연결 완료, 씬 `UseSkillUI` 오브젝트에 `UseSkillUI` 스크립트 부착 완료 (씬 저장은 사용자가 한다).
