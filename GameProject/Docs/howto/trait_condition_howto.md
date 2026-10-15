# 특성 조건(TraitCondition) 추가 절차

> 새 `TraitCondition`을 만들 때 따르는 레시피. 클래스 목록·구조 설명은 [05_trait_system.md](../feature-spec/05_trait_system.md)가 갖고 있으므로 여기서는 **만드는 순서와 판단 기준**만 다룬다. 이 문서를 읽는 커스텀 슬래시 커맨드는 입력(`$ARGUMENTS`)으로 조건 문장을 받는다고 가정한다.

## 1. 언제 쓰나
"~할 때 발동하는 특성"처럼 **발동 조건**이 새로 필요할 때. 효과(`TraitEffect`)만 새로 필요한 경우는 이 문서 대상이 아니다(`05_trait_system.md` §5).

## 2. 사전 읽기 (관련 부분만)
- `Assets/Game/0_Scripts/Models/TraitCondition.cs` — 계약
- `Assets/Game/0_Scripts/Models/Trait.cs` — `Reaction`의 소유자 필터
- `Assets/Game/0_Scripts/Traits/Conditions/TurnCondition.cs` — 가장 단순한 기존 조건 예
- `Assets/Game/0_Scripts/General/ActionSystem/ActionSystem.cs` — PRE/POST 흐름, 구독 해제 지연
- 구독할 GA 파일 (`2.GameActions/`)과 그 GA를 `AddReaction`/`Perform` 하는 곳
- `Docs/convention.md` (코드 규칙)

## 3. 절차

### 3-1. 문장을 "구독 GA / 타이밍 / 판정"으로 분해
| 예시 문장 | 구독 GA | 타이밍 | 판정 |
|---|---|---|---|
| 본인 사망 시 | `KillGA` | PRE | `Token == owner` |
| 손패 카드로 자신이 배치될 때 | `PlayCardGA` | POST | owner 위치 == `TargetPos` |
| 주변에 아군이 배치될 때마다 | `PlayCardGA` | POST | owner가 영웅, `TargetPos`가 owner와 1~`range`칸 |
| 상대 유닛/코어를 공격한 뒤 | `DealDamageGA` + `AttackCoreGA` | POST | 대상이 상대 진영, 코어 공격은 무조건 통과 |

문장에 해당하는 GA가 없으면 **새 GA를 만들기 전에 사용자에게 알린다** (GA 추가는 `action_command.md` 갱신까지 이어진다).

### 3-2. 소유자 판정 방식
- GA가 `IHaveCaster`면 `Trait.Reaction`이 `Caster != owner`를 자동으로 걸러낸다 → 조건에서는 소유자를 따로 비교하지 않는다 (예: `DealDamageGA`, `AttackCoreGA`).
- GA가 `IHaveCaster`가 아니면(`KillGA`, `PlayCardGA`, `TurnGA`) 모든 소유자에게 통과한다 → 소유자가 관련된 조건이면 `SubConditionIsMat(action, owner)` 안에서 직접 비교한다.
- **owner는 판정용 인자로만 쓴다.** 조건 인스턴스는 `TraitData`(SO)에 든 `SerializeReference`라 같은 `TraitData`를 쓰는 모든 유닛이 공유한다 → 소유자·상태를 필드에 저장하면 안 된다.

### 3-3. 타이밍 결정
- **사망 계열은 PRE.** `KillGA` performer가 토큰을 파괴하면 `CombatantView.OnDisable`이 구독을 해제해서 POST에서는 놓칠 수 있다.
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

### 3-6. 검증
1. `refresh_unity` → `read_console`로 에러·경고 0 확인.
2. 기존 조건 클래스가 컴파일되는지(시그니처 영향) 함께 확인.
3. Play 확인이 필요한 항목은 직접 한 척 하지 말고 사용자에게 안내한다.

### 3-7. 에셋·문서
- `TraitData` 에셋(조건 연결, `reactionTiming` 설정)은 **사용자가 요청할 때만** 만든다. 만들지 않으면 인스펙터 설정 방법(조건 선택, 타이밍)을 안내한다.
- `05_trait_system.md` 핵심 파일 표의 조건 목록에 새 클래스를 추가한다. 다른 문서의 낡은 서술은 고치지 말고 보고만 한다.

## 4. 함정·알려진 한계
- 배치 계열은 **손패 카드 배치(`PlayCardGA`)만** 잡는다. 전투 전 초기 배치(`TokenSetup`)와 웨이브 몬스터 생성(`AddToken` 직접 호출)은 GA를 거치지 않아 발동하지 않는다. 포함하려면 `AddToken` 경로를 GA로 감싸야 해서 큰 변경이다.
- 카드 배치 조건은 몬스터가 owner면 발동하지 않는다 (몬스터는 카드 배치 없음).
- `DealDamageGA`는 `KillGA`를 자기 `PostReactions`에 먼저 넣는다 → 공격 후 효과는 대상이 이미 죽은 뒤 실행될 수 있으므로 효과가 `Target`을 참조하면 안 된다.
- 소유자 공통 필터는 `IHaveCaster.Caster != owner`만 본다. 타인의 GA에 반응하는 특성은 조건에서 직접 처리해야 한다.
- 원작 규칙을 모르는 항목(예: "주변"의 범위, 발동 순서)은 임의로 단정하지 말고 `(원작 확인 필요)`로 표기하고 임시값을 SO로 노출해 둔다.

## 5. 완료 보고 형식
- 만든 클래스와 구독 GA/권장 타이밍
- 계약·시그니처 변경 여부 (있으면 어떤 문서를 갱신했는지)
- Console 결과 (에러·경고 수)
- 남긴 한계와 Play 확인 항목
