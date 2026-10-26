# 유물(Perk) 카탈로그 — 이름·조건·효과·연결

> 유물 하나가 어떤 `PerkData` 에셋과 조건·효과로 정의되고, 어디서 읽혀 적용되는지 **유물 단위**로 정리한 문서. 제작 절차는 [perk_howto.md](../../howto/perk_howto.md)를 본다.
> 유물은 유닛이 아니라 **유저가 소유**하며 `GameSystem.Perks`에 든 에셋이 전투 씬마다 `PerkSystem`에 구독된다. 새 유물을 만들면 이 문서에 섹션을 추가한다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> **현재 수록: 1개** (검푸른 망토). 옛 문서의 `Perk` 표기(`game-design.md`, `01_hero_system.md` 등)는 현재의 Trait을 가리키던 이름이라 이 문서의 유물(Perk)과 다른 개념이다.

## 구조

| 구성 | 파일 | 역할 |
|---|---|---|
| SO | `5.Data/PerkData.cs` | `Id`, `Image`, `Description`, `MaxTriggersPerBattle`, `Condition`, `Effect` (`[SerializeReference, SR]`) |
| 런타임 | `Models/Perk.cs` | 조건·효과를 `Clone()`해 보유, `OnAdd`/`OnRemove`로 구독 관리, 전투당 발동 횟수 카운트 |
| 조건 | `Models/PerkCondition.cs` | `SubscribeCondition`/`UnsubscribeCondition`/`SubConditionIsMat(action)` — owner·Caster 필터 없음 |
| 효과 | `Models/PerkEffect.cs` | `PerformReaction(action)` |
| 시스템 | `1.Systems/PerkSystem.cs` | `Singleton`. `SetUp()`에서 `GameSystem.Perks`로 `Perk` 생성, `StartBattle` PRE에서 카운트 리셋, `OnDisable`에서 전부 해제 |
| 소유 목록 | `GameSystem.Perks` (`List<PerkData>`) | 씬 전환 후에도 유지 |
| 호출 | `MatchSetupSystem.StartSetting` | `StartBattle` 직전에 `PerkSystem.Instance.SetUp()` |

- 에셋 위치: `Assets/Game/1_Datas/4.Perks/`. 조건 코드 `0_Scripts/Perks/Conditions/`, 효과 코드 `0_Scripts/Perks/Effects/`.
- `Perk.Reaction` 순서: 조건 판정 → `MaxTriggersPerBattle` 검사(0이면 무제한) → 발동 횟수 증가 → 효과 실행.
- 씬에는 `PerkSystem` GameObject가 `--- SYSTEMS ---` 아래에 1개 있어야 한다 (`GameDemoScene`에 배치됨).

## 요약

| Id | 이름 | 설명 (`Description`) | 조건 | 타이밍 | 효과 | 전투당 상한 | 에셋 |
|---|---|---|---|---|---|---|---|
| 1 | 검푸른 망토 | 아군 깃발에 대한 적 용병의 공격이 빗나갑니다. 전투당 1번 발동됩니다. | `AllyCoreAttackedByEnemyCondition` | PRE | `MissCoreAttackEffect` | 1 | `DarkBlueCloak.asset` |

## 1 · 검푸른 망토 (DarkBlueCloak)

에셋: `Assets/Game/1_Datas/4.Perks/DarkBlueCloak.asset`

| 항목 | 값 |
|---|---|
| Id | 1 |
| 설명 (`Description`) | 아군 깃발에 대한 적 용병의 공격이 빗나갑니다. 전투당 1번 발동됩니다. |
| 조건 | `AllyCoreAttackedByEnemyCondition` — `AttackCoreGA` 구독, `Caster is EnemyView`일 때 통과 |
| 타이밍 | PRE (`reactionTiming: 0`) — 코어 피해가 적용되기 전에 플래그를 세워야 한다 |
| 효과 | `MissCoreAttackEffect` — `AttackCoreGA.IsMissed = true` |
| 효과 동작 | `CoreSystem.AttackCoreGAPerformer`가 `IsMissed`를 보고 공격 모션·화살은 그대로 재생하되 코어 피해·피격 흔들림·GameOver/GameClear 판정을 건너뛴다 |
| 전투당 상한 | `MaxTriggersPerBattle` = 1 |
| 리셋 | `TurnGA(TurnType.StartBattle)` PRE에서 `Perk.ResetBattleCount()` |
| 소유 등록 | `GameSystem.Perks`에 등록됨 (`--- GAMEMASTER ---`) |
| 아이콘 | `Image` 비어 있음 (임시 에셋 미지정) |
| 코드 | `Perks/Conditions/AllyCoreAttackedByEnemyCondition.cs`, `Perks/Effects/MissCoreAttackEffect.cs`, `2.GameActions/AttackCoreGA.cs`, `1.Systems/CoreSystem.cs`, `Models/Perk.cs`, `1.Systems/PerkSystem.cs` |

**연결 흐름**

```
적 용병 AttackCoreGA(Caster = EnemyView)
 ├ PRE 구독자 → Perk.Reaction
 │   ├ AllyCoreAttackedByEnemyCondition: Caster is EnemyView ?  (null=플레이어 스킬, HeroView=아군은 제외)
 │   ├ MaxTriggersPerBattle(1) 검사: 이번 전투에 이미 1번 발동했으면 중단
 │   └ MissCoreAttackEffect: attackCoreGA.IsMissed = true
 └ CoreSystem.AttackCoreGAPerformer
     ├ 공격 모션 / 화살 재생 (그대로)
     ├ IsMissed == true : 피해·흔들림·승패 판정 생략 → 복귀 대기만
     └ IsMissed == false: 코어 피해 적용 → 체력 0이면 GameOverGA
```

**주의·한계**
- **계약 변경**: `AttackCoreGA`에 `public bool IsMissed { get; set; }`을 추가했다. GA를 순수 데이터(`private set`)로 보는 `convention.md` 6절 계약과 어긋나며, "GA 취소 기능이 없는 `ActionSystem`에서 PRE 반응이 Performer 동작을 바꾸는" 유일한 사례다. 유닛 회피(`DODGE`)가 `DamageSystem` 안에서 피해만 건너뛰는 것과 같은 방식이다.
- 공격 모션이 먼저 재생된다 (근접 돌진·화살 모두). "MISS" 표시·연출·사운드는 없다 (아트 제외).
- 원거리 적 공격(`IsRangedAttack`)도 포함된다 — 화살은 날아가고 피해만 무효 (의도 확인 필요).
- **"전투당"은 `StartBattle` 기준**이다. 현재 `StartBattle`은 스테이지(씬) 시작당 1회라 사실상 스테이지당 1번이다 (원작 확인 필요).
- 대상은 아군 깃발(용병대장 코어)뿐이다. 유닛 대상 `DealDamageGA`는 영향을 받지 않는다.
- `Caster`가 null인 `AttackCoreGA`(플레이어 스킬)와 `HeroView`는 조건에서 제외된다. 이는 `convention.md` 7절의 "Caster가 항상 있는 전투 GA" 전제와 어긋나는 기존 예외(`PlayerSkill`)에 대응한 것이다.
- `AttackCoreGA`의 `IsMissed`는 `CoreSystem` Performer와 용병대장 `응수`의 `CoreAttackedCondition`이 읽는다. 빗나간 공격에는 피해가 없으므로 응수도 나가지 않는다.
- 에셋 `Reaction Timing`이 POST면 이미 피해가 적용된 뒤라 효과가 없다. 반드시 PRE(0)로 둔다.
- `GameSystem.Perks`에 같은 에셋을 두 번 넣으면 두 번 구독된다 (중복 방지 없음).
- **Play 테스트는 하지 않았다.** 컴파일·에셋 직렬화·`GameSystem.Perks` 반영만 확인했다. 확인 항목: 첫 적 공격은 피해 0·모션 재생, 두 번째부터 정상 피해, 영웅/플레이어 스킬의 적 깃발 공격은 영향 없음, `StartBattle` 이후 다시 1번 무효.

**에디터 설정**: 에셋 `Id`=1, `Max Triggers Per Battle`=1, `Condition`=`AllyCoreAttackedByEnemyCondition`(`Reaction Timing`=`PRE`), `Effect`=`MissCoreAttackEffect`. `GameSystem.Perks`에 등록 완료, 씬 `PerkSystem` 배치 완료.
