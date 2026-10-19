# 특성(Trait) 시스템 아키텍처

> **상태: 현재 코드 관찰(§1~3) + 목표 변경(§4: 최대 2개 제약, 영웅·몬스터 공용화 구현됨).** 원작의 "특성(Trait)"은 이 코드베이스에서 이미 `Perk`라는 이름으로 구현되어 있던 구조와 사실상 같다 — 새로 만들지 않고 그 구조를 재활용하며, 코드 식별자(클래스·변수·함수·폴더)는 모두 `Trait~`로 통일했다(`Perk` → `Trait`, `PerkItem` → `Trait`, `PerkData` → `TraitData`). 계약은 **조건(`TraitCondition`)과 효과(`TraitEffect`)로 분리**했다(§2). 비어 있는 것은 **용병당 최대 2개 제약**과 **카드 배치 흐름과의 연결**이다.
> `ActionSystem`의 Reaction 구독 원리는 [00_action_architecture.md](./00_action_architecture.md), 용병 배치는 [04_card_system.md](./04_card_system.md)를 먼저 본다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.

## 핵심 파일 (현재 코드)

| 경로 (`Assets/Game/0_Scripts/…`) | 역할 |
|---|---|
| `Models/TraitCondition.cs` | 추상 모델 — "어떤 `GameAction`을 어느 타이밍에 구독하고, 반응할 조건인가" 계약 |
| `Models/TraitEffect.cs` | 추상 모델 — 조건 통과 시 "무엇을 하는가"(후속 `GameAction` 생성) 계약 |
| `Models/Trait.cs` | 런타임 래퍼 — `TraitData` 참조 + 소유자 + 구독/해제 + 소유자 필터 |
| `5.Data/TraitData.cs` | SO — `Id`, `Image`, `Description`, `Condition`·`Effect`(각각 `[SerializeReference, SR]`) |
| `Traits/DealDamageCondition.cs`, `TurnCondition.cs`, `AutoBattleTurnCondition.cs`, `SelfDeathCondition.cs`, `SelfPlacedCondition.cs`, `AllyPlacedNearbyCondition.cs`, `AttackedOpponentCondition.cs`, `AuraTriggerCondition.cs` | 구체 조건 (배치 계열은 손패 카드 배치 `PlayCardGA` 기준 — 초기 배치·웨이브 생성은 제외) |
| `Traits/HealOnDamageEffect.cs`, `AddStatusTraitEffect.cs`, `ModifyStatTraitEffect.cs`, `ExtraAttackEffect.cs`, `AllySpeedAuraEffect.cs`, `SniperEffect.cs` | 구체 효과 |
| `3.Views/CombatantView.cs` | 소유자(영웅·몬스터 공용) — `SetUpBase`에서 구독 시작, `OnDisable`에서 해제, 최대 `MaxTraitCount`(2) 가드 |
| `5.Data/CombatantData.cs` | 영웅·몬스터 공용 SO 베이스 — `Health`/`Damage`/`Speed`/`Traits`(`List<TraitData>`). `HeroData`/`EnemyData`가 상속 |
| `UI/TraitUI.cs`, `DisPlayUI.cs` | 특성 아이콘/툴팁 표시 |

## 1. 한 줄 개요

특성 하나는 "조건(`TraitCondition`) + 효과(`TraitEffect`)" 쌍이다. `Trait`이 `TraitCondition.SubscribeCondition(Reaction)`으로 `ActionSystem`의 PRE/POST 리액션에 구독하고, 해당 `GameAction`이 흐를 때 소유자 필터와 `SubConditionIsMat`을 통과하면 `TraitEffect.PerformReaction`이 후속 `GameAction`을 `AddReaction`한다. `GameAction` 파이프라인에 얹히는 순수 리액션 구조라 새 특성을 추가해도 시스템 코드는 바뀌지 않는다.

## 2. 계약 — TraitCondition / TraitEffect (추상)

```csharp
public abstract class TraitCondition
{
    [SerializeField] protected ReactionTiming reactionTiming;
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMat(GameAction action, CombatantView owner);
}

public abstract class TraitEffect
{
    public abstract void PerformReaction(GameAction action, CombatantView owner);
}
```

- `reactionTiming`: PRE/POST를 SO(인스펙터)에서 정한다. 구체 조건이 `ActionSystem.SubscribeReaction<T>(reaction, reactionTiming)`에 그대로 넘긴다. 어떤 GA 타입을 구독할지는 구체 조건 클래스가 정한다(예: `DealDamageCondition` → `DealDamageGA`).
- `Subscribe/UnsubscribeCondition`: `OnAdd`/`OnRemove`에서 대칭으로 호출되는 구독·해제.
- `SubConditionIsMat(action, owner)`: 구독한 GA가 조건에 맞는지 거르는 필터. `owner`는 판정용 **인자**로만 받는다 — `TraitData`(SO)의 조건·효과는 `Trait` 생성 시 `Clone()`(얕은 복사)되어 **`Trait`(= 유닛 1개)마다 사본**을 쓴다. 그래서 효과가 유닛별 상태(예: `AllySpeedAuraEffect.speedGrants`)를 필드로 가질 수 있지만, 참조형 상태는 얕은 복사로 템플릿과 공유되므로 `Clone()`을 오버라이드해 새로 만들어야 한다. 예: `TurnCondition`은 `turnType`이 일치할 때만 통과, `SelfDeathCondition`은 `KillGA.Token == owner`일 때만 통과.
- **사망 조건 주의**: `SelfDeathCondition`은 `reactionTiming`을 PRE로 둔다. `KillGA` performer가 토큰을 파괴하면 `CombatantView.OnDisable`이 구독을 해제하므로 POST에서는 놓칠 수 있다.
- **소유자 필터(`Trait.Reaction`)**: `action`이 `IHaveCaster`이고 `Caster != owner`이면 무시한다. `Caster`가 없는 GA(`TurnGA`, `AutoBattleTurnGA`)는 모든 소유자에게 통과한다. 한계: 타인의 GA에 반응하는 특성(예: 아군 공격 시 발동)은 이 공통 필터로는 만들 수 없다 — 필요해지면 조건에 필터 옵션을 추가한다.
- **발동 횟수 상한(`ILimitedTrigger`)**: 상한 값(`MaxTriggers`, 0 = 무제한)은 `TraitEffect` 베이스가 아니라 **구체 효과 클래스의 데이터**로 두고(`ModifyStatTraitEffect.maxTriggers`), 효과가 `Interfaces/ILimitedTrigger`를 구현해 `Trait`이 읽게 한다. 유닛별 카운트(`triggerCount`)는 `Trait` 런타임 객체가 가진다(효과·조건은 SO 공유라 상태를 저장할 수 없다). `Trait.Reaction`은 소유자 필터·`SubConditionIsMat` 통과 뒤 `effect is ILimitedTrigger`이고 `MaxTriggers > 0 && triggerCount >= MaxTriggers`이면 무시하고, 아니면 카운트를 올리고 효과를 실행한다. 리셋 없이 해당 `Trait`(= 배치된 유닛)이 살아 있는 동안 누적된다. 예: `맹공`(공격력+1, `maxTriggers` 4).
- **자동 전투 행동 대체(`IBattleOverride`)**: 효과가 `Interfaces/IBattleOverride`(`bool TryBattle(CombatantView owner)`)를 구현하면 `CombatantView.Battle()`이 기본 이동·정면 공격 전에 `Trait.TryOverrideBattle()`로 각 특성에 먼저 묻고, true를 반환한 특성이 있으면 기본 행동을 건너뛴다(먼저 true를 반환한 쪽만 실행). 예: `SniperEffect`.
- `TraitEffect.PerformReaction(action, owner)`: 조건·필터 통과 시 후속 `GameAction`을 `AddReaction`한다(`HealOnDamageEffect` → `HealGA`, `AddStatusTraitEffect` → `AddStatusEffectGA`).

## 3. 로드 파이프라인과 수명

```
HeroView.SetUp(heroData) / EnemyView.SetUp(enemyData)
 └ CombatantView.SetUpBase(CombatantData)           (CombatantView.cs)
    └ CombatantData.Traits: List<TraitData> 순회 (최대 MaxTraitCount개)
       └ new Trait(traitData) → SetOwner(this) → OnAdd()   ← 구독 시작

CombatantView.OnDisable()
 └ foreach Traits: OnRemove()                                 ← 구독 해제
```

관찰된 점(수정 대상이라는 뜻이 아니라, 구현 시 알아둘 사실):
- 구독은 `SetUpBase`, 해제는 `OnDisable`로 **비대칭**이다. `convention.md`의 "`OnEnable`/`OnDisable` 대칭 등록" 규칙과 어긋나므로 위반 목록 성격의 관찰이다(`convention.md` 11절 기준으로 구현 시 판단).
- `Trait`은 소유자(`owner`)를 **필드 1개**로 갖고 `Trait`은 `CombatantView` 인스턴스마다 새로 만들어진다(이전엔 `Hero` 런타임 객체가 `Trait`을 들고 `HeroData`(SO)가 `Hero`를 들어, 같은 `HeroData`로 `HeroView`를 둘 이상 만들면 `owner`가 덮어써졌다 — **`Hero` 제거로 이 문제는 해소됨**).
- `Trait.Title`은 `TraitData` 에셋 파일명에서 `_` 뒤쪽을 잘라 만든다(`Skill.Title`과 같은 규칙).

## 4. 목표 변경

1. **유닛당 최대 2개 — 구현됨.** `CombatantView.MaxTraitCount = 2`와 `SetUpTraits` 가드(이전엔 `Hero.MaxTraitCount`/`Hero` 생성자): `CombatantData.Traits`가 2개를 넘으면 `Debug.LogError`로 알리고 앞의 2개만 사용한다(조용히 자르지 않음). **영웅·몬스터 공용**이다 — `Trait`의 `owner` 타입을 `HeroView`에서 `CombatantView`로, 특성이 만드는 GA(`HealGA`, `AddStatusEffectGA`)의 `Caster`도 `CombatantView`로 넓혔다. 몬스터에 실제 특성을 부여하는 콘텐츠는 아직 없다. 상수는 구조 규칙으로 보고 이름 있는 `const`로 두었다(`convention.md` 8절의 "밸런싱 수치 SO 노출" 대상으로 보지 않음). 이후 특성 획득 경로가 생기면 같은 상한을 거치도록 한다 — 경로가 정해지지 않아 `TryAddTrait` 같은 API는 아직 만들지 않았다. `HeroData.OnValidate` 경고는 SO는 데이터만 든다는 규칙(`convention.md` 10절)에 따라 넣지 않았다.
2. **배치 시점 구독 — 변경 불필요(확인됨).** 카드로 배치되면 `TokenMainAPI.AddToken` → `TokenCreator.CreateToken` → `HeroView.SetUp`이 호출되어 구독이 시작되고, 토큰이 제거되면 오브젝트가 파괴되며 `OnDisable`이 `OnRemove`로 구독을 해제한다.
   - **알려진 한계:** 근육 트로피(`TurnCondition`: `TurnGA` StartBattle, POST)는 전투 시작 때만 발동한다. 카드 배치 영웅은 전투 시작 이후에 등장하므로 **이 특성은 발동하지 않는다.** 원작에서 특성이 "전투 시작 시"인지 "배치 시"인지 모르므로 특성 코드는 건드리지 않았다 (원작 확인 필요). 용사의 갑옷(`AutoBattleTurnCondition`, `AutoBattleTurnGA` PRE)은 영향 없음.
3. **같은 용병 중복 배치**: `Trait`이 뷰 인스턴스마다 생성되므로 `owner` 덮어쓰기 문제는 사라졌다. 중복 배치 허용 여부는 (원작 확인 필요).
4. 코드 식별자는 `Trait~`로 통일한다(이전 `Perk~`에서 변경). UI 문구에서만 "특성"을 쓴다.

## 5. 새 특성 추가 절차 (현재 구조 기준)

1. 필요한 조건이 없으면 `TraitCondition`을 상속한 클래스를 `Traits/`에 만든다. 구독할 GA 타입은 `Subscribe/UnsubscribeCondition`에서 대칭으로 등록·해제하고 타이밍은 `reactionTiming`을 쓴다. 필요한 효과가 없으면 `TraitEffect`를 상속해 `PerformReaction`을 구현한다.
2. `TraitData` SO를 만들고 `Condition`/`Effect` 필드에 구현체를 각각 연결하고 `reactionTiming`을 정한다. 파일명은 `번호_이름` 형식(`Title` 파싱 규칙).
3. 영웅/몬스터 데이터(`HeroData`/`EnemyData`)의 `Traits`에 등록한다(최대 2개 — §4).

## 6. 원작 확인 필요 항목

- 특성 종류와 풀, 조건/효과 상세
- 특성 획득 경로(용병 고유 / 모집 시 선택 / 랜덤)와 2개 조합의 의미(시너지·발동 순서)
- 같은 특성 중복 허용 여부, 특성 교체 가능 여부
- 특성 발동 시점의 기준: "전투 시작 시"(현재 근육 트로피)인지 "용병 배치 시"인지 — 카드 배치 구조에서는 둘이 다르다
