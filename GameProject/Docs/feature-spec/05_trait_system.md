# 특성(Trait) 시스템 아키텍처

> **상태: 현재 코드 관찰(§1~3) + 목표 변경(§4, 최대 2개 제약만 구현됨).** 원작의 "특성(Trait)"은 이 코드베이스에서 이미 `Perk`로 구현되어 있는 구조와 사실상 같다 — 새로 만들지 않고 `Perk`를 재활용하며, 클래스명은 `Perk`를 유지한다(문서에서는 "특성 = Perk"로 대응). 비어 있는 것은 **용병당 최대 2개 제약**과 **카드 배치 흐름과의 연결**이다.
> `ActionSystem`의 Reaction 구독 원리는 [00_action_architecture.md](./00_action_architecture.md), 용병 배치는 [04_card_system.md](./04_card_system.md)를 먼저 본다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.

## 핵심 파일 (현재 코드)

| 경로 (`Assets/Game/0_Scripts/…`) | 역할 |
|---|---|
| `Models/Perk.cs` | 추상 모델 — "어떤 `GameAction`에 반응해 무엇을 하는가" 계약 |
| `Models/PerkItem.cs` | 런타임 래퍼 — `PerkData` 참조 + 소유자 + 구독/해제 |
| `5.Data/PerkData.cs` | SO — `Id`, `Image`, `Description`, `Perk`(`[SerializeReference, SR]`) |
| `Perks/BloodyAxe.cs`, `HeroArmor.cs`, `MuscleTrophy.cs` | 구체 특성(`Perk` 구현체) |
| `3.Views/HeroView.cs` | 소유자 — `SetUp`에서 구독 시작, `OnDisable`에서 해제 |
| `Models/Hero.cs`, `5.Data/HeroData.cs`, `GameSystem.cs` | `HeroData.StartingPerks` → `PerkItem` 리스트 → `Hero.Perks` 조립 |
| `UI/PerkUI.cs`, `AllPerksDisPlayUI.cs`, `DisPlayUI.cs` | 특성 아이콘/툴팁 표시 |

## 1. 한 줄 개요

특성 하나는 "조건(`GameAction` 구독) + 반응(`PerformReaction`)" 쌍이다. `PerkItem`이 `Perk.SubscribeCondition(Reaction)`으로 `ActionSystem`의 PRE/POST 리액션에 구독하고, 해당 `GameAction`이 흐를 때 `SubConditionIsMat`이 소유자 조건을 통과하면 `PerformReaction`이 후속 `GameAction`을 `AddReaction`한다. `GameAction` 파이프라인에 얹히는 순수 리액션 구조라 새 특성을 추가해도 시스템 코드는 바뀌지 않는다.

## 2. 계약 — Perk (추상)

```csharp
public abstract class Perk
{
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMat(GameAction action, HeroView owner);
    public abstract void PerformReaction(GameAction action, HeroView owner);
}
```

- `Subscribe/UnsubscribeCondition`: 어떤 GA 타입을 어느 타이밍(PRE/POST)에 구독할지 정한다. 예: `BloodyAxe`는 `ActionSystem.SubscribeReaction<DealDamageGA>(reaction, ReactionTiming.POST)`.
- `SubConditionIsMat`: 구독한 GA가 "내 소유자 것인지" 거르는 필터. 예: `BloodyAxe`는 `dealDamageGA.Caster == owner`일 때만 통과.
- `PerformReaction`: 조건 통과 시 후속 `GameAction`을 `AddReaction`한다(`BloodyAxe` → `HealGA`).

## 3. 로드 파이프라인과 수명

```
GameSystem.InitializeHero()                       (GameSystem.cs)
 └ HeroData.StartingPerks: List<PerkData> 순회
    └ new PerkItem(perkData) → List<PerkItem>
 └ new Hero(id, hp, skills, perks) → HeroData.SetHero(hero)

HeroView.SetUp(heroData)                           (HeroView.cs)
 └ this.Hero = heroData.Hero
 └ foreach Perks: SetOwner(this) → OnAdd()        ← 구독 시작

HeroView.OnDisable()
 └ foreach Perks: OnRemove()                       ← 구독 해제
```

관찰된 점(수정 대상이라는 뜻이 아니라, 구현 시 알아둘 사실):
- 구독은 `SetUp`, 해제는 `OnDisable`로 **비대칭**이다. `convention.md`의 "`OnEnable`/`OnDisable` 대칭 등록" 규칙과 어긋나므로 위반 목록 성격의 관찰이다(`convention.md` 11절 기준으로 구현 시 판단).
- `PerkItem`은 소유자(`owner`)를 **필드 1개**로 가지고, `PerkItem`은 `Hero`가, `Hero`는 `HeroData`(SO)가 들고 있다. 같은 `HeroData`에서 `HeroView`를 둘 이상 만들면 `owner`가 마지막 값으로 덮어써진다. 현재는 영웅이 데이터당 1명이라 문제가 되지 않는다.
- `PerkItem.Title`은 `PerkData` 에셋 파일명에서 `_` 뒤쪽을 잘라 만든다(`Skill.Title`과 같은 규칙).
- `HeroData.StartingPerks`는 개수 제한이 없는 `List<PerkData>`다.

## 4. 목표 변경

1. **용병당 최대 2개 — 구현됨.** `Hero.MaxPerkCount = 2`(`Models/Hero.cs`)와 `Hero` 생성자 가드: `GameSystem.InitializeHero`가 만든 `PerkItem` 리스트가 2개를 넘으면 `Debug.LogError`로 알리고 앞의 2개만 사용한다(조용히 자르지 않음). 상수는 구조 규칙으로 보고 이름 있는 `const`로 두었다(`convention.md` 8절의 "밸런싱 수치 SO 노출" 대상으로 보지 않음). 이후 특성 획득 경로가 생기면 같은 상한을 거치도록 한다 — 경로가 정해지지 않아 `TryAddPerk` 같은 API는 아직 만들지 않았다. `HeroData.OnValidate` 경고는 SO는 데이터만 든다는 규칙(`convention.md` 10절)에 따라 넣지 않았다.
2. **배치 시점 구독 — 변경 불필요(확인됨).** 카드로 배치되면 `TokenMainAPI.AddToken` → `TokenCreator.CreateToken` → `HeroView.SetUp`이 호출되어 구독이 시작되고, 토큰이 제거되면 오브젝트가 파괴되며 `OnDisable`이 `OnRemove`로 구독을 해제한다.
   - **알려진 한계:** `MuscleTrophy`는 `TurnGA(StartBattle)`의 POST에서만 발동한다. 카드 배치 영웅은 전투 시작 이후에 등장하므로 **이 특성은 발동하지 않는다.** 원작에서 특성이 "전투 시작 시"인지 "배치 시"인지 모르므로 특성 코드는 건드리지 않았다 (원작 확인 필요). `HeroArmor`(적 턴 PRE)는 영향 없음.
3. **같은 용병 중복 배치 대응**: 한 `HeroData`를 둘 이상 배치할 수 있게 된다면(§3의 `owner` 덮어쓰기) 소유자를 `HeroView` 단위로 분리해야 한다. 중복 배치 허용 여부는 (원작 확인 필요).
4. 클래스명은 `Perk` 유지. UI 문구에서만 "특성"을 쓴다.

## 5. 새 특성 추가 절차 (현재 구조 기준)

1. `Perk`를 상속한 클래스를 `Perks/`에 만들고 4개 메서드를 구현한다. 구독할 GA 타입/타이밍은 `Subscribe/UnsubscribeCondition`에서 대칭으로 등록·해제한다.
2. `PerkData` SO를 만들고 `Perk` 필드에 구현체를 연결한다. 파일명은 `번호_이름` 형식(`Title` 파싱 규칙).
3. `HeroData.StartingPerks`에 등록한다(최대 2개 — §4 적용 후).

## 6. 원작 확인 필요 항목

- 특성 종류와 풀, 조건/효과 상세
- 특성 획득 경로(용병 고유 / 모집 시 선택 / 랜덤)와 2개 조합의 의미(시너지·발동 순서)
- 같은 특성 중복 허용 여부, 특성 교체 가능 여부
- 특성 발동 시점의 기준: "전투 시작 시"(현재 `MuscleTrophy`)인지 "용병 배치 시"인지 — 카드 배치 구조에서는 둘이 다르다
