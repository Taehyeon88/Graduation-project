# 유물(Perk) 만들기 — 조건 → 효과 → 에셋 → 등록

> 유물 하나를 처음부터 끝까지 만드는 레시피. **유저가 소유**하고 전투 중 조건에 반응하는 유물 전용이다 (유닛이 가지는 것은 [trait_howto.md](trait_howto.md)). 구조는 Trait과 거의 같지만 소유자(`owner`)가 없다. 커스텀 슬래시 커맨드 `/perk 유물명, 조건+효과 설명`이 이 문서의 절차를 따라 조건·효과·에셋을 만든다. 첫 사례는 **검푸른 망토**(아군 깃발에 대한 적 용병의 공격이 빗나감, 전투당 1번)이며 아래 예시 코드는 모두 이 유물의 실제 코드다.

## 1. 언제 쓰나
"~할 때 ~한다" 형태의 유물을 새로 만들 때. 예: "아군 깃발에 대한 적 용병의 공격이 빗나갑니다. 전투당 1번 발동됩니다."
- 조건만 새로 필요하면 §5만, 효과만 새로 필요하면 §6만 수행한다.
- 이미 있는 조건·효과 조합만 바꾸는 경우는 §7(에셋)만 보면 된다.

## 2. Trait과의 차이 (먼저 읽기)
| 항목 | Trait | Perk |
|---|---|---|
| 소유자 | 유닛 (`CombatantView`) | 유저 (소유자 없음) |
| 조건 판정 | `SubConditionIsMat(action, owner)` | `SubConditionIsMat(action)` |
| Caster 필터 | `FiltersByCaster`로 `Caster != owner`를 자동 제외 | **없음** — 조건이 `Caster`/`Target`을 직접 판정 |
| 효과 | `PerformReaction(action, owner)` | `PerformReaction(action)` |
| 발동 횟수 | 효과가 `ILimitedTrigger` 구현, 카운트 리셋 없음 | `PerkData.MaxTriggersPerBattle`, 전투 시작 때 `Perk.ResetBattleCount()` |
| 등록 | `CombatantView.SetUpTraits` | `GameSystem.Perks` → `PerkSystem.SetUp()` |
| 구독 해제 | `CombatantView.OnDisable` | `PerkSystem.OnDisable` |

## 3. 사전 읽기 (관련 부분만)
- `Assets/Game/0_Scripts/Models/PerkCondition.cs`, `PerkEffect.cs` — 계약
- `Assets/Game/0_Scripts/Models/Perk.cs` — `Reaction` 순서 (조건 → 횟수 상한 → 효과)
- `Assets/Game/0_Scripts/5.Data/PerkData.cs` — SO 필드
- `Assets/Game/0_Scripts/1.Systems/PerkSystem.cs` — 등록·카운트 리셋
- `Assets/Game/0_Scripts/Perks/Conditions/AllyCoreAttackedByEnemyCondition.cs`, `Perks/Effects/MissCoreAttackEffect.cs` — 기존 조건·효과 예
- `Assets/Game/0_Scripts/General/ActionSystem/ActionSystem.cs` — PRE/POST 흐름 (GA 취소 기능 없음)
- 구독·수정할 GA 파일 (`2.GameActions/`)과 그 GA의 Performer
- `Docs/convention.md` (코드 규칙)

## 4. 구조 한눈에
```
GameSystem.Perks (List<PerkData>, 씬 전환 후에도 유지)
  → MatchSetupSystem.StartSetting: PerkSystem.Instance.SetUp()   // TurnGA(StartBattle) 직전
  → Perk 생성(조건·효과 Clone) → Perk.OnAdd() = 조건이 GA 구독
  → GA 발생 → Perk.Reaction: 조건 판정 → MaxTriggersPerBattle 검사 → triggerCount++ → 효과
  → TurnGA(StartBattle) PRE: PerkSystem이 모든 Perk.ResetBattleCount()
  → 씬 재로드: PerkSystem.OnDisable이 모든 Perk.OnRemove()
```
`PerkSystem` GameObject는 `--- SYSTEMS ---` 아래에 이미 배치되어 있다 (유물마다 새로 둘 필요 없음).

## 5. 조건 단계 (`PerkCondition`)

### 5-1. 문장을 "조건 + 효과"로 분해
| 예시 문장 | 구독 GA | 타이밍 | 판정 | 효과 |
|---|---|---|---|---|
| 적 용병이 아군 깃발을 공격할 때 (검푸른 망토) | `AttackCoreGA` | PRE | `Caster is EnemyView` | `IsMissed = true` |
| 아군이 피해를 받은 뒤 ~ | `DealDamageGA` | POST | `Target`이 아군 | (효과에 따라) |

문장에 해당하는 GA가 없으면 **새 GA를 만들기 전에 사용자에게 알린다.** 같은 의미의 조건이 `Perks/Conditions/`에 이미 있으면 새로 만들지 않고 재사용한다.

### 5-2. 판정 규칙
- Perk에는 소유자 필터가 없다. **`Caster`/`Target`을 조건에서 직접 판정한다.**
- `AttackCoreGA`/`DealDamageGA`의 `Caster`는 **null일 수 있다** (플레이어 스킬 = `Caster == null`). `Caster is X` 패턴은 null을 자동으로 거르므로 이 형태를 쓴다. `as`로 받은 뒤 바로 멤버를 읽지 않는다.
- 아군/적 구분은 `HeroView`(아군 영웅) / `EnemyView`(적 용병)로 한다.

### 5-3. 타이밍 결정
- **무효화·값 변경은 PRE.** Performer가 실행되기 전이어야 `IsMissed` 같은 플래그를 Performer가 읽는다. POST로 두면 이미 피해가 적용된 뒤라 효과가 없다.
- 사후 반응(피해를 받은 뒤 회복 등)은 POST.
- 타이밍은 클래스에 박지 않고 `reactionTiming`(SO 인스펙터 값)을 그대로 `SubscribeReaction`에 넘긴다. 권장 타이밍은 클래스 주석에 적는다. enum 값은 **PRE = 0, POST = 1**.

### 5-4. 클래스 작성
- 위치 `Assets/Game/0_Scripts/Perks/Conditions/`, **한 파일에 클래스 1개**, 파일명 = 클래스명 (`~Condition`).
- `PerkCondition` 상속. `SubscribeCondition`/`UnsubscribeCondition`은 **같은 GA 타입을 대칭**으로 등록·해제한다.
- 판정 수치는 `[SerializeField] private`으로 노출한다. 매직넘버 금지.
- 파일 첫머리에 한 줄 주석으로 조건 의미를 적는다.

골격 예시 (`AllyCoreAttackedByEnemyCondition`):
```csharp
using System;

//적 용병이 아군 코어(용병대장)를 공격할 때 (Caster 없음 = 플레이어 스킬, HeroView = 아군 영웅이라 제외)
public class AllyCoreAttackedByEnemyCondition : PerkCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action)
    {
        var attackCoreGA = action as AttackCoreGA;
        return attackCoreGA.Caster is EnemyView;
    }
}
```

## 6. 효과 단계 (`PerkEffect`)

### 6-1. 두 가지 패턴
| 패턴 | 하는 일 | 예 |
|---|---|---|
| (a) GA 데이터 변경형 | **PRE**에서 진행 중인 GA의 플래그를 바꾸고, 그 GA의 Performer가 플래그를 읽어 동작을 바꾼다 | 검푸른 망토 (`AttackCoreGA.IsMissed`) |
| (b) 후속 GA 예약형 | `ActionSystem.Instance.AddReaction(새 GA)`로 예약만 한다. 수행은 해당 GA의 Performer | Trait 효과와 동일 |

### 6-2. (a) GA 데이터 변경형 — 계약 변경이므로 먼저 알린다
`ActionSystem`에는 **GA 취소 기능이 없다.** 그래서 무효화는 "GA에 플래그를 두고 Performer가 피해만 건너뛰는" 방식으로 한다 (유닛 회피 `DODGE`가 `DamageSystem` 안에서 피해만 건너뛰는 것과 같은 방식).
- GA에 `public bool IsX { get; set; }`을 추가하는 것은 `convention.md` 6절의 "GA = 순수 데이터(`private set`)" 계약에서 벗어난다. **구현 전에 차이를 짚고 승인받는다.**
- Performer 수정은 해당 구간만 최소로 한다. 검푸른 망토는 `CoreSystem.AttackCoreGAPerformer`에서 피해 적용~승패 판정을 `if (!attackCoreGA.IsMissed) { ... }`로 감쌌고, **모션·화살·복귀 대기는 그대로 재생**된다.
- 이미 쓸 수 있는 플래그가 있으면 새로 만들지 말고 재사용한다 (현재: `AttackCoreGA.IsMissed`, `DealDamageGA.IgnoreDodge`는 반대 방향의 플래그).
- 같은 GA 종류를 처리하는 Performer가 여러 곳이면 모두 확인한다 (유닛 대상 `DealDamageGA`는 `DamageSystem`, 코어 대상 `AttackCoreGA`는 `CoreSystem`).

### 6-3. 클래스 작성
- 위치 `Assets/Game/0_Scripts/Perks/Effects/`, **한 파일에 클래스 1개**, 파일명 = 클래스명 (`~Effect`).
- `PerkEffect` 상속, `PerformReaction(GameAction action)`을 구현한다. 조건을 통과한 뒤에 호출되므로 효과 안에서 조건을 다시 검사하지 않는다.
- 수치는 `[SerializeField] private`으로 노출한다. 매직넘버 금지.
- 참조형 상태(Dictionary 등)를 가지면 `Clone()`을 오버라이드해 새로 만든다 (기본은 얕은 복사).
- **발동 횟수 제한은 효과가 아니라 `PerkData.MaxTriggersPerBattle`로 둔다.** 카운터를 효과에 두지 않는다.

골격 예시 (`MissCoreAttackEffect`):
```csharp
//코어 공격을 빗나가게 함 (CoreSystem이 IsMissed를 보고 모션만 재생하고 피해는 건너뜀)
public class MissCoreAttackEffect : PerkEffect
{
    public override void PerformReaction(GameAction action)
    {
        var attackCoreGA = action as AttackCoreGA;
        attackCoreGA.IsMissed = true;
    }
}
```

## 7. 에셋 단계 (`PerkData`)

`PerkData` 에셋은 **사용자가 요청할 때만** 만든다. 요청이 없으면 아래 인스펙터 설정 방법만 안내한다.

- 위치 `Assets/Game/1_Datas/4.Perks/`. 메뉴 `Create > Data > Perk`.
- 필드: `Id`, `Image`, `Description`, `MaxTriggersPerBattle`(전투당 상한, **0이면 무제한**), `Condition`, `Effect`.
- `Condition`/`Effect`는 `[SerializeReference, SR]` 필드라 인스펙터에서 구현체를 드롭다운으로 고른다. 조건을 고르면 그 안의 `Reaction Timing`(PRE/POST)도 이 에셋에서 정한다 (§5-3 기준).
- **만든 에셋은 `GameSystem`(`--- GAMEMASTER ---`)의 `Perks` 리스트에 넣어야 실제로 적용된다.**

### Unity MCP로 만들 때 (검푸른 망토에서 검증된 절차)
1. `manage_scriptable_object` `create` — `type_name: PerkData`, `folder_path: Assets/Game/1_Datas/4.Perks`, `asset_name`. 같은 호출의 `patches`로 일반 필드를 지정한다 (`propertyPath`는 `<Id>k__BackingField`, `<Description>k__BackingField`, `<MaxTriggersPerBattle>k__BackingField`).
2. `Condition`/`Effect`는 MCP patch로 넣을 수 없다. **`.asset` 파일의 `references.RefIds`를 직접 편집한다.**
   ```yaml
     <Condition>k__BackingField:
       rid: 7851381000000000001
     <Effect>k__BackingField:
       rid: 7851381000000000002
     references:
       version: 2
       RefIds:
       - rid: 7851381000000000001
         type: {class: AllyCoreAttackedByEnemyCondition, ns: , asm: Assembly-CSharp}
         data:
           reactionTiming: 0
       - rid: 7851381000000000002
         type: {class: MissCoreAttackEffect, ns: , asm: Assembly-CSharp}
         data:
   ```
   `rid`는 에셋 안에서만 유일하면 된다. 기존 `-2`(빈 참조) 항목은 지운다. `reactionTiming: 0`이 PRE다.
3. `refresh_unity` (`scope: assets`, `mode: force`)로 리임포트한다.
4. `GameSystem` 컴포넌트에 등록: `find_gameobjects`(`by_component`, `GameSystem`)로 ID를 찾고, `manage_components` `set_property`로 `<Perks>k__BackingField`에 `[{"path": "Assets/Game/1_Datas/4.Perks/<이름>.asset"}]`를 지정한다. 이 호출은 **기존 리스트를 덮어쓴다** — 이미 들어 있는 유물이 있으면 함께 넣는다.
5. `mcpforunity://scene/gameobject/{id}/component/GameSystem`을 읽어 `Perks`에 반영되었는지, `.asset`의 `rid`/`class`가 올바른지 확인한다. 씬을 수정했으므로 `manage_scene save`가 필요하다.

## 8. 씬/에디터 작업
- 새 유물마다 필요한 씬 작업은 **`GameSystem.Perks` 등록뿐**이다 (`PerkSystem`은 1회 배치 완료).
- 에디터 작업은 클릭 순서를 안내하고 멈추는 것이 프로젝트 기본이다. 사용자가 명시적으로 요청할 때만 MCP로 직접 수행한다.

## 9. 문서 갱신
- Perk 전용 카탈로그·feature-spec 챕터는 **아직 없다.** 만든 유물을 어디에 기록할지는 사용자에게 확인한다 (파일 신설은 요청 시).
- 조건·효과 클래스를 새로 만들었다면 완료 보고에 목록을 남긴다. 다른 문서의 낡은 서술(`game-design.md`/`feature-spec`의 옛 `Perk` 표기 = 현재의 Trait)은 고치지 말고 보고만 한다.

## 10. 검증
1. `refresh_unity`(`compile: request`, `mode: force`) → `read_console`로 에러·경고 0 확인. 컴파일 직후에는 이전 상태를 읽을 수 있으므로 에러가 `해당 타입을 찾을 수 없음`류면 다시 한 번 갱신 후 재확인한다.
2. 기존 `Perk*`/`Trait*` 클래스가 함께 컴파일되는지 확인.
3. §7-5의 `GameSystem.Perks` 반영, `.asset` YAML 확인.
4. Play 확인이 필요한 항목은 직접 한 척 하지 말고 사용자에게 안내한다. 검푸른 망토 기준: 적 용병의 첫 공격은 피해 0·모션 재생, 두 번째부터 정상 피해, 영웅/플레이어 스킬의 적 깃발 공격은 영향 없음.

## 11. 함정·알려진 한계
- **`Reaction Timing`을 POST로 두면** 무효화 계열은 이미 피해가 적용된 뒤라 효과가 없다. 에셋의 값을 반드시 확인한다.
- **"전투당"의 기준은 `TurnType.StartBattle`이다.** 현재 `StartBattle`은 스테이지(씬) 시작당 1회라 사실상 스테이지당 1번이다. 여러 전투를 한 스테이지에서 치르게 되면 리셋 시점을 다시 정해야 한다 (원작 확인 필요).
- 발동 횟수는 **조건을 통과한 경우에만** 증가한다 (`Perk.Reaction`: 조건 → 상한 검사 → 증가 → 효과).
- `Caster == null`인 GA가 있다 (플레이어 스킬의 `AttackCoreGA`/`DealDamageGA`). 이는 `convention.md` 7절의 "Caster가 항상 있는 전투 GA" 전제와 어긋나는 예외다. 조건에서 반드시 null을 처리한다.
- 무효화형은 **공격 모션이 먼저 재생**된다. 모션 전에 막으려면 Performer 구조를 바꿔야 하므로 별도 협의가 필요하다.
- `PerkSystem`이 씬에 없으면 `MatchSetupSystem`에서 `NullReference`가 난다.
- `GameSystem.Perks`에 같은 에셋을 두 번 넣으면 두 번 구독된다 (중복 방지 없음).
- 에셋 `Image`는 임시 에셋 사용 (아트는 범위 제외). 검푸른 망토는 비어 있다.
- 원작 규칙을 모르는 항목은 임의로 단정하지 말고 `(원작 확인 필요)`로 표기하고 임시값을 SO로 노출해 둔다.

## 12. 완료 보고 형식
- 만든 조건·효과 클래스와 구독 GA, 권장 타이밍 (PRE/POST)
- 계약 변경 여부 (GA 플래그 추가, Performer 수정이 있으면 어떤 파일을 고쳤는지)
- 에셋을 만들었는지 여부와 `GameSystem.Perks` 등록 여부
- Console 결과 (에러·경고 수)
- 남긴 한계와 Play 확인 항목
