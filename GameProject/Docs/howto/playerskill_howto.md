# 플레이어 스킬(PlayerSkill) 만들기 — 분해 → 스킬 클래스 → 에셋 → 연결 → 문서

> 플레이어가 `UseSkillUI`를 드래그해서 쓰는 스킬 하나를 처음부터 끝까지 만드는 레시피. 구조·흐름·결정 사항은 [07_player_skill.md](../feature-spec/07_player_skill.md)가 갖고 있으므로 여기서는 **만드는 순서와 판단 기준**만 다룬다. 커스텀 슬래시 커맨드(예: `/playerskill 스킬명, 설명`)가 이 문서의 절차를 따라 스킬 클래스·에셋을 만든다. 첫 구현 사례는 **날카로운 단도**(`PlayerSkills/SharpDagger.cs`).
> 옛 영웅 수동 스킬(`SkillSystem`, `Skill`/`SkillData`/`Effect` 체인)과는 **별개**다. 그쪽 코드를 재사용하지 않는다.

## 1. 언제 쓰나
"대상 하나를 지정해 AP 1로 쓰는 플레이어 스킬"을 새로 만들거나 기존 스킬의 동작을 바꿀 때. 예: "적 용병 또는 우두머리를 지정해 2의 피해를 줍니다. 회피를 무시하며, 장애물은 지정할 수 없습니다."
- 아래 **공통 규칙은 `PlayerSkillSystem`이 처리**하므로 스킬 클래스에 넣지 않는다.
  - AP 1 소모 (`PlayerSkillSystem.APCost` const, 모든 플레이어 스킬 공통)
  - 한 턴에 1번만
  - 전투당 사용 횟수 (`PlayerSkillData.MaxUses`)
  - 플레이어 턴·액션 미수행 중에만
- 스킬 클래스가 정하는 것은 **① 지정 가능한 대상 ② 실행할 GameAction** 뿐이다.
- 이 문서의 전제는 **플레이어 스킬 1개**다. 두 번째 스킬을 *추가*하는 경우는 §6을 먼저 본다.

## 2. 사전 읽기 (관련 부분만)
- `Docs/feature-spec/07_player_skill.md` — 규칙·구성·흐름·알려진 제약
- `Assets/Game/0_Scripts/Models/PlayerSkill.cs` — 계약 (`IsValidTarget`, `CreateAction`)
- `Assets/Game/0_Scripts/PlayerSkills/SharpDagger.cs` — 가장 단순한 기존 예
- `Assets/Game/0_Scripts/1.Systems/PlayerSkillSystem.cs` — `CanUse`/`TryUse`, 턴·횟수 리셋
- `Assets/Game/0_Scripts/5.Data/PlayerSkillData.cs` — SO
- 사용할 GA 파일 (`2.GameActions/DealDamageGA.cs`, `AttackCoreGA.cs` 등)과 그 Performer (`DamageSystem`, `CoreSystem`)
- `Docs/convention.md` (코드 규칙)

## 3. 스킬 분해

한 문장을 세 가지로 쪼갠다.

| 예시 문장 | ① 지정 가능한 대상 | ② 실행할 GA | ③ 특수 성질 |
|---|---|---|---|
| 적 용병 또는 우두머리에게 2 피해, 회피 무시 (날카로운 단도) | `EnemyView`, `EnemyFlagView` | 용병 → `DealDamageGA`, 우두머리 → `AttackCoreGA` | 회피 무시 (`DealDamageGA.IgnoreDodge`) |

- 대상 용어 매핑: **적 용병 = `EnemyView`**, **우두머리 = 적 코어 = `EnemyFlagView`**(그리드 밖, `IDamageable` 아님 → 피해는 `AttackCoreGA`). 아군 용병 = `HeroView`, 아군 코어 = `HeroCaptainView`.
- **파괴 가능 장애물(`ObstacleView`)은 토큰**이다(중립지대·발판은 토큰이 아님). "장애물 지정 불가"는 `IsValidTarget`이 `EnemyView`/`EnemyFlagView`**만** 허용하는 허용 목록 방식이어야 충족된다 — `is CombatantView` 같은 넓은 조건으로 바꾸면 장애물이 대상이 된다. 상세는 [08_terrain_obstacle.md](../feature-spec/08_terrain_obstacle.md).
- 문장에 해당하는 GA가 없으면 **새 GA를 만들기 전에 사용자에게 알린다** (GA 추가는 `action_command.md` 갱신까지 이어진다). 같은 의미의 GA가 이미 있으면 재사용한다.
- 원작 규칙을 모르는 항목(회피 스택 소모 여부, 횟수 제한 단위 등)은 단정하지 않고 `(원작 확인 필요)`로 두고, 임시 결정을 사용자에게 확인받는다.

## 4. 스킬 클래스 (`PlayerSkill` 상속)

### 4-1. 작성 규칙
- 위치 `Assets/Game/0_Scripts/PlayerSkills/`, **한 파일에 클래스 1개**, 파일명 = 클래스명.
- `PlayerSkill`(추상, `Models/`)을 상속해 두 메서드를 구현한다.
  - `bool IsValidTarget(Token target)` — 지정 가능한 토큰인지.
  - `GameAction CreateAction(Token target)` — 실행할 GA 하나를 만들어 반환. **AP 차감(`SpendAPGA`)·횟수·턴 플래그는 `PlayerSkillSystem.TryUse`가 붙이므로 여기서 하지 않는다.**
- 수치(피해량 등)는 `[SerializeField] private`으로 노출한다 (`SharpDagger.damage`). 매직넘버 금지. AP 코스트는 필드로 두지 않는다 (공통 상수).
- `PlayerSkill`은 `[System.Serializable]` 추상 클래스이며 `PlayerSkillData`가 `[SerializeReference, SR]`로 구현체를 인스펙터에서 고르게 한다. 구현체에 `[System.Serializable]`을 따로 붙이지 않았다 (`SharpDagger`는 이 상태로 컴파일되고 에셋 YAML이 에러 없이 다시 읽혔다. 인스펙터 드롭다운에 보이는지는 확인하지 못했다 — 안 보이면 구현체에도 `[System.Serializable]`을 붙인다).
- 파일 첫머리에 한 줄 주석으로 스킬 의미를 적는다. 요청한 스킬만 만든다 (인접 코드·UI·시스템은 건드리지 않는다).

골격 예시 (`SharpDagger`):
```csharp
using UnityEngine;

//날카로운 단도 : 적 용병 또는 우두머리를 지정해 피해를 준다 (회피 무시, 장애물 지정 불가)
public class SharpDagger : PlayerSkill
{
    [SerializeField] private int damage = 2;

    public override bool IsValidTarget(Token target)
    {
        return target is EnemyView || target is EnemyFlagView;
    }

    public override GameAction CreateAction(Token target)
    {
        if (target is EnemyFlagView)
            return new AttackCoreGA(null, damage);

        return new DealDamageGA(damage, target as IDamageable, null, false, false, true);
    }
}
```

### 4-2. Caster는 null이다 (가장 중요한 계약)
플레이어 스킬에는 시전 용병이 없으므로 GA의 `Caster`에 **null**을 넘긴다. 이 때문에 다음이 성립한다.
- `DamageCaculator.GetDamage`는 attacker가 null이면 **힘(POWER)·약화를 적용하지 않고**, 대상의 취약만 적용한다. 이 처리가 원작과 같은지는 (원작 확인 필요).
- `Trait.Reaction`의 `FiltersByCaster` 필터(`Caster != owner`)는 null Caster를 통과시키지 않는다 → 소유자 특성은 반응하지 않는다.
- `CoreSystem.AttackCoreGAPerformer`는 `Caster == null`이면 **플레이어 스킬 공격으로 보고** 모션 없이 적 코어에 `Amount`만큼 피해를 준다. `Amount`가 null이면 피해 0이므로 코어 공격은 반드시 `new AttackCoreGA(null, amount)`로 만든다. `IsMissed`가 true면 피해·승패 판정을 건너뛴다 (PRE 리액션이 설정).
- `CoreAttackedCondition`은 `Caster != null` 가드가 있어, 플레이어 스킬의 적 코어 공격에 용병대장 응수 특성이 발동하지 않는다.
- **새 GA를 `Caster == null`로 쓰려면** 그 GA를 소비하는 Performer·조건(`Traits/Conditions/`)이 null Caster를 안전하게 처리하는지 먼저 확인한다. 안전하지 않으면 구현 전에 알리고 승인받는다 (`CoreSystem`/`CoreAttackedCondition`을 고친 것이 그 사례).

### 4-3. 자주 쓰는 패턴
| 하고 싶은 것 | 방법 | 상태 |
|---|---|---|
| 적 용병 1명에게 피해 | `new DealDamageGA(amount, target as IDamageable, null)` | 생성자 존재, 3인자 형태는 이 스킬에서 미사용 |
| 적 용병에게 **회피 무시** 피해 | `new DealDamageGA(amount, target as IDamageable, null, false, false, true)` — 마지막 인자 `ignoreDodge` | `SharpDagger`로 구현 (Play 확인은 사용자) |
| 적 코어(우두머리) 피해 | `new AttackCoreGA(null, amount)` | `SharpDagger`로 구현 |
| 여러 대상 동시 피해 | `new DealDamageGA(amount, List<IDamageable> targets, null)` — `Targets` 오버로드 | 생성자만 확인, 스킬로는 **미검증** |
| 상태이상 부여 | `new AddStatusEffectGA(type, amount, targets, null)` | 시그니처는 `trait_howto.md` §4-3 기준, Caster null 안전성 **미확인** |
| 회복 | `new HealGA(amount, positions, null)` | 시그니처는 `trait_howto.md` §4-3 기준, Caster null 안전성 **미확인** |

- `IgnoreDodge`가 true면 `DamageSystem.DealDamageGAPerformer`가 회피(DODGE) 분기를 건너뛴다 → **DODGE 스택을 소모하지 않고 피해만 적용**한다 (`(원작 확인 필요)`).
- `CreateAction`은 **GA 1개**를 반환한다. 여러 GA가 필요하면 반환 GA의 `PostReactions`에 후속 GA를 붙이거나 Performer 쪽에서 `AddReaction`하는 방식을 쓰게 되는데, 이 경로는 **플레이어 스킬로는 아직 검증하지 않았다** → 필요하면 구현 전에 알린다.

### 4-4. 계약 변경이 필요하면 먼저 알린다
`PlayerSkill`의 시그니처(예: 대상을 `Token` 하나가 아니라 여러 개나 좌표로 받기)나 기존 GA·Performer의 시그니처를 바꿔야 하면 **구현 전에 차이를 짚고 승인받는다.** GA를 바꾸면 `action_command.md`와 `07_player_skill.md` "기존 코드에 가한 변경"도 함께 갱신한다.

## 5. 데이터 에셋 (`PlayerSkillData`)

에셋은 **사용자가 요청할 때만** 만든다. 요청이 없으면 아래 인스펙터 설정 방법만 안내한다.

- 위치 `Assets/Game/1_Datas/10.PlayerSkills/`. 필드: `Id`, `Description`, `Image`(비워 둠 가능), `MaxUses`(전투당 횟수), `Skill`(`[SerializeReference, SR]`).
- **새 스크립트를 만든 직후엔 먼저** `refresh_unity`(`compile: request`, `mode: force`)로 컴파일을 반영하고 `read_console`로 에러 0을 확인한다. 아직 반영되지 않았으면 `manage_scriptable_object`가 `type_not_found`를 낸다.
- 1단계 — 에셋 생성과 일반 필드 설정 (`manage_scriptable_object`, `action: create`, `type_name: PlayerSkillData`, `folder_path: Assets/Game/1_Datas/10.PlayerSkills`). 패치의 `propertyPath`는 자동 프로퍼티 백킹 필드명을 쓴다.
  - `<Id>k__BackingField`, `<Description>k__BackingField`, `<MaxUses>k__BackingField`
- 2단계 — **`Skill` 필드는 MCP 패치로 설정할 수 없다** (`Unsupported SerializedPropertyType: ManagedReference`). `.asset` YAML을 직접 편집한다. 생성 직후 기본값은 `rid: -2` / `type: {class: , ns: , asm: }`이다. 아래처럼 바꾼다 (`SharpDagger` 에셋에서 확인한 형태).
  ```yaml
    <Skill>k__BackingField:
      rid: 1001
    references:
      version: 2
      RefIds:
      - rid: 1001
        type: {class: SharpDagger, ns: , asm: Assembly-CSharp}
        data:
          damage: 2
  ```
  - `class`는 스킬 클래스명, `data:` 아래 키는 그 클래스의 직렬화 필드명(C# 필드명 그대로)이다.
  - 편집 후 `refresh_unity`(`scope: assets`, `mode: force`)로 다시 읽히게 하고, `.asset`에서 `rid`와 `class`가 올바른지 다시 확인한다. 인스펙터에서 `Skill` 드롭다운에 구현체와 수치가 보이는지는 **사용자에게 확인을 부탁**한다 (MCP로는 인스펙터 표시를 볼 수 없다).
  - 에디터에서 직접 고르는 방법: 에셋 선택 → `Skill` 드롭다운에서 구현체 선택 → 수치 입력.

## 6. 시스템·UI 연결 (현재 구조의 한계)

현재 구조는 **스킬 1개**를 전제한다.
- `PlayerSkillSystem`: `[SerializeField] PlayerSkillData skillData` 1개, `RemainingUses` 1개, `usedThisTurn` 1개.
- `UseSkillUI`: 버튼 1개가 `PlayerSkillSystem.TryUse(Token)`을 호출. 스킬 선택 개념이 없다.
- 씬: `--- SYSTEMS ---/PlayerSkillSystem`(컴포넌트의 `Skill Data` 슬롯), `--- UI ---/Bottom Canvas/UseSkillUI`(스크립트 부착).

따라서 경우가 갈린다.

| 상황 | 할 일 |
|---|---|
| **기존 스킬을 교체·수정** (다른 스킬로 바꿈, 수치 변경) | 새 스킬 클래스 + 에셋을 만들고 `PlayerSkillSystem`의 `Skill Data` 슬롯만 새 에셋으로 바꾼다. 코드 변경 없음. |
| **스킬을 하나 더 추가** | **구조 변경이 필요하므로 구현 전에 차이를 짚고 승인받는다.** 결정할 것: 스킬 목록·스킬별 남은 횟수, 버튼을 여러 개 둘지 선택 UI를 둘지, "한 턴에 1번"이 *스킬 전체 공유*인지 *스킬별*인지(현재 규칙은 "모든 플레이어 스킬은 한 턴에 1번"). `PlayerSkillSystem`/`UseSkillUI`를 임의로 확장하지 않는다. |

- 연결 위치(씬)를 바꿔야 하면 `hierarchy.md`의 `--- 카테고리 ---` 관례에 맞춰 안내한다. 씬 편집은 사용자의 일이지만, **사용자가 요청하면** Unity MCP로 대행할 수 있다 (`find_gameobjects` → `manage_components`).
- 씬을 MCP로 수정했다면 **저장되지 않은 상태**다. 사용자에게 Ctrl+S를 안내한다 (직접 저장하지 않는다).

## 7. 문서 갱신
- [07_player_skill.md](../feature-spec/07_player_skill.md): 규칙·구성 표·"기존 코드에 가한 변경"·알려진 제약에 새 스킬을 반영한다.
- [player_skill_catalog.md](../feature-spec/PlayerSkill/player_skill_catalog.md): 요약 표에 행을 추가하고 `SharpDagger` 섹션을 견본으로 스킬 섹션(항목 표·연결 흐름·주의·에디터 설정)을 추가한다. 상단 "현재 수록: N개"도 갱신한다.
- 새 폴더·에셋 폴더가 생겼으면 `Folder.md`, 씬 오브젝트가 생겼으면 `hierarchy.md`.
- 새 GA를 만들었으면 `feature-spec/action/action_command.md`.
- 다른 문서의 낡은 서술은 **고치지 말고 보고만** 한다.

## 8. 검증
1. `refresh_unity` → `read_console`로 에러·경고 0 확인 (컴파일 반영 전에 읽으면 이전 상태일 수 있으니 한 번 더 확인).
2. 기존 `DealDamageGA`/`AttackCoreGA`/`CoreSystem` 호출이 컴파일되는지(시그니처 영향) 함께 확인.
3. Play 확인이 필요한 항목은 **직접 한 척 하지 말고** 사용자에게 안내한다.
   - AP가 부족하면 드래그가 시작되지 않는다
   - 같은 턴에 두 번째 사용이 안 되고 다음 플레이어 턴에는 된다
   - 전투당 횟수를 초과하면 안 되고 다음 전투에서 초기화된다
   - 잘못된 대상(빈 칸·아군·장애물)에 놓으면 AP·횟수를 쓰지 않는다
   - 스킬 고유 효과(예: 회피 보유 적에게 피해가 들어가고 DODGE 스택이 유지된다, 깃발에 놓으면 코어 체력이 줄고 0이 되면 클리어된다)

## 9. 함정·알려진 한계
- **Caster null 파급**: §4-2. 새 GA·조건을 쓸 때 가장 먼저 확인한다.
- 횟수·턴 플래그는 `Perform` 직전에 소모된다. 스킬 GA가 도중에 실패해도 소모된다 (현재 실패 경로는 없음).
- 우두머리(깃발) 드롭 판정은 콜라이더가 아니라 `EnemyFlagView.Model.bounds`(SpriteRenderer) 안에 마우스 월드 좌표가 들어오는지로 한다. 스프라이트가 작거나 카메라가 원근이면 어긋날 수 있다.
- `UseSkillUI`는 `Interactions`의 `IsSetUpHero`/`IsHeroMoveMode`/`IsCardTargetMode`/`IsPileViewOpen` 중 하나라도 켜져 있으면 드래그를 무시한다. 새 선택 모드(`Interactions`의 새 플래그)를 추가하면 이 조건에 넣을지 검토한다.
- 드래그 중 대상 하이라이트, 남은 횟수 텍스트, 사용 불가 시 버튼 비활성 표시는 아직 없다 (요청되지 않아 만들지 않았다).
- 원작 규칙을 모르는 항목은 임의로 단정하지 말고 `(원작 확인 필요)`로 표기하고, 임시값은 SO/`[SerializeField]`로 노출해 둔다.
- 이 프로젝트는 구현을 단계마다 **사용자 허락을 받고** 진행한다. 계획 승인 뒤에도 스크립트 구현·수정은 한 단계씩 확인받는다.

## 10. 완료 보고 형식
- 만든 스킬 클래스와 사용한 GA, 지정 가능한 대상
- 계약·시그니처 변경 여부 (있으면 어떤 문서를 갱신했는지)
- 에셋을 만들었는지 여부와 `Skill Data` 연결 내용 (씬 변경이 있으면 저장 필요 안내)
- Console 결과 (에러·경고 수)
- 남긴 한계, `(원작 확인 필요)` 항목, Play 확인 항목, 문서에서 발견한 낡은 서술(보고만)
