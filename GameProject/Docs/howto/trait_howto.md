# 특성(Trait) 만들기 — 조건 → 효과 → 에셋 → 카탈로그

> 특성 하나를 처음부터 끝까지 만드는 레시피. 구조·계약·로드 파이프라인은 [05_trait_system.md](../feature-spec/05_trait_system.md), 완성된 특성 목록은 [trait_catalog.md](../feature-spec/Trait/trait_catalog.md)가 갖고 있으므로 여기서는 **만드는 순서와 판단 기준**만 다룬다. 커스텀 슬래시 커맨드 `/trait 특성명, 조건+효과 설명`(`.claude/commands/trait.md`)이 이 문서의 절차를 따라 조건·효과·에셋을 만든다. 재사용 가능한 기존 조건·효과는 [trait.md](../feature-spec/Trait/trait.md)에 정리되어 있다.

## 1. 언제 쓰나
"~할 때 ~한다" 형태의 특성을 새로 만들 때. 예: "사망 시 자신의 속도만큼 모든 적 용병 및 우두머리에게 피해를 줍니다."
- 조건만 새로 필요하면 §3만, 효과만 새로 필요하면 §4만 수행한다.
- 이미 있는 조건·효과 조합만 바꾸는 경우는 §5(에셋)만 보면 된다.

## 2. 사전 읽기 (관련 부분만)
- `Docs/feature-spec/Trait/trait.md` — **가장 먼저.** 이미 있는 조건·효과 목록. 재사용할 수 있으면 새로 만들지 않는다
- `Assets/Game/0_Scripts/Models/TraitCondition.cs`, `TraitEffect.cs` — 계약
- `Assets/Game/0_Scripts/Models/Trait.cs` — `Reaction`의 소유자 필터
- `Assets/Game/0_Scripts/Traits/Conditions/TurnCondition.cs` — 가장 단순한 기존 조건 예
- `Assets/Game/0_Scripts/Traits/AddStatusTraitEffect.cs` — 가장 단순한 기존 효과 예 (`HealOnDamageEffect`도 참고)
- `Assets/Game/0_Scripts/General/ActionSystem/ActionSystem.cs` — PRE/POST 흐름, 구독 해제 지연
- 구독·예약할 GA 파일 (`2.GameActions/`)과 그 GA를 `AddReaction`/`Perform` 하는 곳
- `Docs/convention.md` (코드 규칙)

## 3. 조건 단계 (`TraitCondition`)

### 3-1. 문장을 "조건 + 효과"로 분해
먼저 한 문장을 조건(구독 GA / 타이밍 / 판정)과 효과(예약할 GA)로 쪼갠다.

| 예시 문장 | 구독 GA | 타이밍 | 판정 | 효과가 예약할 GA |
|---|---|---|---|---|
| 본인 사망 시 ~ | `KillGA` | PRE | `Token == owner` | (효과에 따라) |
| 손패 카드로 자신이 배치될 때 ~ | `PlayCardGA` | POST | owner 위치 == `TargetPos` | (효과에 따라) |
| 주변에 아군이 배치될 때마다 ~ | `PlayCardGA` | POST | owner가 영웅, `TargetPos`가 owner와 1~`range`칸 | (효과에 따라) |
| 상대 유닛/코어를 공격한 뒤 ~ | `DealDamageGA` + `AttackCoreGA` | POST | 대상이 상대 진영, 코어 공격은 무조건 통과 | (효과에 따라) |

문장에 해당하는 GA가 없으면 **새 GA를 만들기 전에 사용자에게 알린다** (GA 추가는 `action_command.md` 갱신까지 이어진다). 같은 의미의 조건이 `Traits/Conditions/`에 이미 있으면 새로 만들지 말고 재사용한다.

### 3-2. 소유자 판정 방식
- GA가 `IHaveCaster`면 `Trait.Reaction`이 `Caster != owner`를 자동으로 걸러낸다 → 조건에서는 소유자를 따로 비교하지 않는다 (예: `DealDamageGA`, `AttackCoreGA`).
- GA가 `IHaveCaster`가 아니면(`KillGA`, `PlayCardGA`, `TurnGA`) 모든 소유자에게 통과한다 → 소유자가 관련된 조건이면 `SubConditionIsMat(action, owner)` 안에서 직접 비교한다.
- **owner는 판정용 인자로만 쓴다.** `Trait`이 생성 시 조건·효과를 `Clone()`(얕은 복사)해 유닛별 사본을 쓰므로 유닛별 상태를 필드로 가질 수 있다. 단 owner 자체는 필드에 저장하지 않고, Dictionary 같은 참조형 상태를 두면 `Clone()`을 오버라이드해 새로 만든다 (`AllySpeedAuraEffect` 참고).

### 3-3. 타이밍 결정
- **사망 계열은 PRE.** `KillGA` performer가 토큰을 파괴하면 `CombatantView.OnDisable`이 구독을 해제해서 POST에서는 놓칠 수 있다. PRE 시점에는 owner가 아직 그리드에 있어 위치 조회도 가능하다.
- 공격 후·배치 후는 POST. 배치는 `PlayCardGA` performer가 `AddToken`을 끝낸 뒤라 owner 위치 조회가 가능하다.
- 타이밍은 클래스에 박지 않고 `reactionTiming`(SO 인스펙터 값)을 그대로 `SubscribeReaction`에 넘긴다. 권장 타이밍은 클래스 주석에 적는다.

### 3-4. 클래스 작성
- 위치 `Assets/Game/0_Scripts/Traits/Conditions/`, **한 파일에 클래스 1개**, 파일명 = 클래스명 (`~Condition`).
- `TraitCondition` 상속. `SubscribeCondition`/`UnsubscribeCondition`은 **같은 GA 타입을 대칭**으로 등록·해제한다 (GA를 두 개 구독하면 두 개 모두 해제).
- 판정 수치(범위 등)는 `[SerializeField] private`으로 노출한다 (예: `range`). 매직넘버 금지.
- 파일 첫머리에 한 줄 주석으로 조건 의미를 적는다.
- 요청한 조건만 만든다. 효과·에셋·인접 코드는 건드리지 않는다.

골격 예시 (`SelfDeathCondition`):
```csharp
using System;

//본인 사망 시 (reactionTiming은 PRE로 설정)
public class SelfDeathCondition : TraitCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<KillGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<KillGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        var killGA = action as KillGA;
        return killGA.Token == owner;
    }
}
```

### 3-5. 계약 변경이 필요하면 먼저 알린다
`TraitCondition`/`Trait`의 시그니처나 필터 규칙을 바꿔야 하는 상황이면 **구현 전에 차이를 짚고 승인받는다** (`SubConditionIsMat`에 `owner`를 추가한 것이 그 사례). 변경하면 `05_trait_system.md` §2도 같이 갱신한다.

## 4. 효과 단계 (`TraitEffect`)

### 4-1. 원칙
- 효과는 **후속 `GameAction`을 `ActionSystem.Instance.AddReaction`으로 예약만** 한다. 실제 수행은 해당 GA의 Performer가 한다 (효과 안에서 직접 체력·상태를 바꾸지 않는다).
- 조건·필터를 통과한 뒤에 호출되므로 효과 안에서 조건을 다시 검사하지 않는다.

### 4-2. 클래스 작성
- 위치 `Assets/Game/0_Scripts/Traits/`, **한 파일에 클래스 1개**, 파일명 = 클래스명 (`~Effect`).
- `TraitEffect` 상속, `PerformReaction(GameAction action, CombatantView owner)`를 구현한다.
- 수치(피해량 보정, 스택 수 등)는 `[SerializeField] private`으로 노출한다 (예: `AddStatusTraitEffect.amount`). 매직넘버 금지.
- 조건과 마찬가지로 효과도 `Trait`이 `Clone()`한 유닛별 사본이라 유닛별 상태를 필드로 가질 수 있다. owner는 인자로만 쓰고, 참조형 상태(Dictionary 등)가 있으면 `Clone()`을 오버라이드해 새로 만든다 (안 하면 같은 `TraitData`를 쓰는 유닛끼리 공유됨).
- 발동 횟수 제한("N번 발동")은 효과 클래스에 `[SerializeField] maxTriggers`(0 = 무제한) 데이터를 두고 `ILimitedTrigger`를 구현한다(`ModifyStatTraitEffect` 참고). 카운터를 효과에 두지 않는다 — 유닛별 카운트는 `Trait`이 센다. `TraitEffect` 베이스에는 넣지 않는다.
- 요청한 효과만 만든다.

### 4-3. 자주 쓰는 패턴
| 하고 싶은 것 | 방법 |
|---|---|
| 상대 진영 판별 | `owner is HeroView ? 적(EnemyView) : 영웅(HeroView)` (`AttackedOpponentCondition.IsOpponent`와 같은 규칙) |
| 상대 진영 전체 목록 | `TokenSystem.Instance.EnemyViews` / `HeroViews` |
| 여러 유닛에 동시 피해 | `new DealDamageGA(amount, List<IDamageable> targets, owner)` — `Targets` 오버로드, 공격 모션 없음 |
| 상대 코어 피해 | `new AttackCoreGA(owner, amount)` — `owner`가 `HeroView`면 적 코어, 아니면 플레이어 코어 |
| 상태이상 부여 | `new AddStatusEffectGA(type, amount, targets, owner)` |
| 회복 | `new HealGA(amount, positions, owner)` |

골격 예시 (`DamageAllOpponentsBySpeedEffect`):
```csharp
using System.Collections.Generic;
using System.Linq;

public class DamageAllOpponentsBySpeedEffect : TraitEffect
{
    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        int amount = owner.Speed;

        List<IDamageable> targets = owner is HeroView
            ? TokenSystem.Instance.EnemyViews.Cast<IDamageable>().ToList()
            : TokenSystem.Instance.HeroViews.Cast<IDamageable>().ToList();

        if (targets.Count > 0)
            ActionSystem.Instance.AddReaction(new DealDamageGA(amount, targets, owner));
        ActionSystem.Instance.AddReaction(new AttackCoreGA(owner, amount));
    }
}
```

### 4-4. 계약 변경이 필요하면 먼저 알린다
기존 GA의 시그니처를 바꿔야 하는 경우(예: 모션 없는 코어 피해를 위해 `AttackCoreGA`를 수정)는 **구현 전에 차이를 짚고 승인받는다.** GA를 바꾸면 `action_command.md`도 함께 갱신한다.

## 5. 에셋 단계 (`TraitData`)

`TraitData` 에셋은 **사용자가 요청할 때만** 만든다. 요청이 없으면 아래 인스펙터 설정 방법만 안내한다.

- 위치 `Assets/Game/1_Datas/3.Traits/`. `Id`, `Image`, `Description`을 채운다. 파일명은 `번호_이름` 형식이 규칙이지만(`Trait.Title`이 `_` 뒤쪽을 잘라 만든다) 현재 일부 에셋은 이름만 쓰고 있다 — 관찰된 불일치로 보고만 한다.
- `Condition`/`Effect`는 `[SerializeReference, SR]` 필드라 인스펙터에서 구현체를 드롭다운으로 고른다. 조건을 고르면 그 안의 `reactionTiming`(PRE/POST)도 이 에셋에서 정한다 (§3-3 기준).
- `ConditionName`/`timing`/`EffectName`은 "작업용 변수" 메모 필드이고 동작에는 쓰이지 않는다. 연결한 클래스명을 함께 적어 둔다.
- Unity MCP로 연결할 때 `Effect`는 `execute_code`에서 `SerializedObject.FindProperty("<Effect>k__BackingField").managedReferenceValue = new X()` 후 `ApplyModifiedProperties` + `SaveAssets`로 지정할 수 있다 (불경한 빛에서 확인). `reactionTiming` 값을 코드로 쓰는 방법은 검증하지 않았다 — 인스펙터에서 설정한다.
- 연결 후 `.asset` 파일에서 `<Condition>`/`<Effect>`의 `rid`가 올바른 클래스를 가리키는지 확인한다.

## 6. 카탈로그 갱신
[trait_catalog.md](../feature-spec/Trait/trait_catalog.md)에 특성 섹션을 추가한다. 형식은 불경한 빛 섹션이 견본이다.
- 표: Id / 설명 / 조건 / 타이밍 / 효과 / 효과 동작 / 파생 GA / 코드
- 연결 흐름 도식(GA → `Trait.Reaction` → 조건 → 효과 → 예약 GA)
- 주의·한계, 에디터 설정
- 조건/효과 클래스를 새로 만들었다면 [trait.md](../feature-spec/Trait/trait.md)의 표에 행을 추가하고(재사용했다면 "사용 특성" 열에 이 특성 추가), [05_trait_system.md](../feature-spec/05_trait_system.md) 핵심 파일 표의 목록에도 추가한다. 다른 문서의 낡은 서술은 고치지 말고 보고만 한다.

## 7. 검증
1. `refresh_unity` → `read_console`로 에러·경고 0 확인.
2. 기존 조건·효과 클래스가 컴파일되는지(시그니처 영향) 함께 확인.
3. Play 확인이 필요한 항목은 직접 한 척 하지 말고 사용자에게 안내한다.

## 8. 함정·알려진 한계
- 배치 계열은 **손패 카드 배치(`PlayCardGA`)만** 잡는다. 전투 전 초기 배치(`TokenSetup`)와 웨이브 몬스터 생성(`AddToken` 직접 호출)은 GA를 거치지 않아 발동하지 않는다. 포함하려면 `AddToken` 경로를 GA로 감싸야 해서 큰 변경이다.
- 카드 배치 조건은 몬스터가 owner면 발동하지 않는다 (몬스터는 카드 배치 없음).
- `DealDamageGA`는 `KillGA`를 자기 `PostReactions`에 먼저 넣는다 → 공격 후 효과는 대상이 이미 죽은 뒤 실행될 수 있으므로 효과가 `Target`을 참조하면 안 된다.
- 소유자 공통 필터는 `IHaveCaster.Caster != owner`만 본다. 타인의 GA에 반응하는 특성은 조건에서 직접 처리해야 한다.
- **무한 루프**: 효과가 예약하는 GA를 그대로 구독하는 조건과 조합하면 효과가 자기 자신을 다시 트리거한다 (예: `DealDamageGA`를 예약하는 효과 + `AttackedOpponentCondition`).
- `AttackCoreGA`는 owner의 공격 모션을 재생하고 `DamageCaculator` 보정을 적용하지 않는다 (`DealDamageGA`는 적용) → 같은 특성 안에서 유닛 피해와 코어 피해가 다를 수 있다 (원작 확인 필요).
- 원작 규칙을 모르는 항목(예: "주변"의 범위, 발동 순서)은 임의로 단정하지 말고 `(원작 확인 필요)`로 표기하고 임시값을 SO로 노출해 둔다.

## 9. 완료 보고 형식
- 만든 조건·효과 클래스와 구독/예약 GA, 권장 타이밍
- 계약·시그니처 변경 여부 (있으면 어떤 문서를 갱신했는지)
- 에셋을 만들었는지 여부와 연결 내용
- Console 결과 (에러·경고 수)
- 남긴 한계와 Play 확인 항목
