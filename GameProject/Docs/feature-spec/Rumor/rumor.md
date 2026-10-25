# 소문(Rumor) 시스템 — 구조·부품 레지스트리

> 영웅이 **스테이지를 진행하며 얻는** 고유 능력. 영웅당 **최대 1개**이고 시작 소문은 없다. 소문별 이름·설명·연결은 [rumor_catalog.md](./rumor_catalog.md).
> 특성([05_trait_system.md](../05_trait_system.md), [trait.md](../Trait/trait.md))과 달리 "조건+효과" 쌍이 아니라 **`Rumor` 추상 클래스 하나**가 상시 수정과 이벤트 반응을 함께 맡는다.
> **새 소문 클래스를 만들면 이 문서의 §3 표에 행을 추가한다.** 값은 코드를 읽어 확인한 것만 적는다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`.

## 1. 구조

| 경로 (`Assets/Game/0_Scripts/…`) | 역할 |
|---|---|
| `Models/Rumor.cs` | 추상 클래스 — 생명주기 훅(`OnAdd`/`OnRemove`/`Clone`)만 정의. **소문별 수치·기능은 넣지 않는다** |
| `Interfaces/IAPCostModifier.cs` | 카드 배치 AP 보정 계약(`APCostBonus`). 해당 기능이 있는 소문만 구현 |
| `Interfaces/INeutralPlacement.cs` | 기본 배치 열의 중립지대 배치 허용 계약(`CanPlaceOnNeutral`). `Card.CanPlaceOnNeutral` → `CardSystem.GetPlaceablePoses(card)`가 확인 |
| `5.Data/RumorData.cs` | SO — `Id`, `Image`, `Description`, `Rumor`(`[SerializeReference, SR]`), `Title`(파일명 `_` 뒤쪽) |
| `Rumors/*.cs` | 구체 소문. 수치는 각 클래스의 `SerializeField` |
| `Models/Hero.cs` | 덱의 영웅 한 장의 런타임 상태 — `Rumor`(`RumorData`), `CanAddRumor`, `AddRumor()` |
| `3.Views/HeroView.cs` | `SetUp`에서 소문 사본을 만들어 `OnAdd`, `OnDisable`에서 `OnRemove` |
| `Models/Card.cs` | `APCost`에 `IAPCostModifier` 보정 합산, `Description`에 소문 한 줄 |
| `UI/TokenDetailsUI.cs` | 호버한 영웅의 소문 슬롯(`AbilitySlot`, 특성 슬롯과 공용) 표시 |

## 2. 계약

```csharp
[System.Serializable]
public abstract class Rumor
{
    public virtual void OnAdd(HeroView owner) { }      // 배치 직후: 스탯 적용·구독 시작
    public virtual void OnRemove() { }                 // 소유 뷰가 사라질 때: 구독 해제
    public virtual Rumor Clone() => (Rumor)MemberwiseClone();
}
```

- **저장 위치**: 소문은 `HeroData`(SO, 불변 정의)가 아니라 `Hero.Rumor`(런타임)에 있다. 몬스터(`EnemyData`)에는 없다.
- **영웅당 1개**: `Hero.Rumor`가 단일 참조다. 이미 소문이 있는 영웅에게 줄 때의 처리는 호출하는 쪽이 `CanAddRumor`로 거른다(특성의 `CanAddTrait`과 같은 방식). `AddRumor`는 null이 아니면 덮어쓴다. 교체·거부 규칙은 (원작 확인 필요).
- **유닛별 사본**: `HeroView.SetUp`이 `hero.Rumor.Rumor.Clone()`을 만들어 `OnAdd(this)`를 호출한다. `RumorData`의 `Rumor`는 템플릿으로만 쓰고, 소유자 등 상태는 사본의 필드에 둔다. 참조형 상태를 가진 소문은 `Clone()`을 오버라이드한다.
- **구독 대칭**: 구독이 필요한 소문은 `OnAdd`에서 등록하고 `OnRemove`에서 해제한다(`convention.md` 규칙). `OnRemove`는 `HeroView.OnDisable`이 호출한다. 이를 위해 `CombatantView.OnDisable`이 `protected virtual`이 되었다.
- **카드 비용**: 배치 전 카드는 `HeroView`가 없으므로 `Card.APCost`가 `Hero.Rumor.Rumor`(템플릿)에서 `IAPCostModifier`를 읽는다. 그래서 비용 보정 값은 상수여야 한다.
- **스탯 변경**: GA 없이 `CombatantView.ModifyStat`(public)을 직접 호출한다. 같은 방식으로 `StatType.DAMAGE` 증가는 양수 전용이다([trait.md](../Trait/trait.md) §5).

## 3. 구체 소문 (`Rumor` 상속) — 위치 `Assets/Game/0_Scripts/Rumors/`

| 클래스 | 구현 인터페이스 | 노출 필드 | 동작 | 사용 소문 |
|---|---|---|---|---|
| `PowerfulRumor` | `IAPCostModifier` | `attackBonus`(3), `apCostBonus`(1) | `OnAdd`에서 `owner.ModifyStat(DAMAGE, attackBonus)`. `APCostBonus`가 `apCostBonus` | 1 강력한 |
| `FlexibleRumor` | `IAPCostModifier`, `INeutralPlacement` | `apCostReduction`(1) | 훅은 쓰지 않는다. `APCostBonus`가 `-apCostReduction`(`Card.APCost`는 0 미만이 되지 않게 막음), `CanPlaceOnNeutral`이 true | 2 유연한 |
| `MasteredRumor` | 없음 | `apAmount`(1) | `OnAdd`에서 `TurnGA`를 POST로 구독. `TurnType.Enemy`일 때 `RefillAPGA(apAmount)` 예약. `OnRemove`에서 해제 | 3 숙달된 |

## 4. 아직 없는 것
- **소문 획득 경로**: `Hero.AddRumor`를 부르는 코드가 없다(보상·상점·이벤트 등 미정). 지금은 인게임에서 소문이 붙지 않는다.
- `RumorData` 에셋(`1_강력한`, `3_숙달된`)은 만들지 않았다(에디터 작업, 위치는 [Folder.md](../../Folder.md)로 확정).
- 컴파일 에러 0만 확인했고 Play 테스트는 하지 않았다.

## 5. 원작 확인 필요
- 소문 획득 경로(보상·상점 등)와 획득 후보 풀
- 이미 소문이 있는 영웅의 교체·거부 규칙, 같은 소문 중복 가능 여부
- "라운드 종료"의 정의 (현재 숙달된은 `TurnGA(Enemy)` POST로 가정, 클리어로 끝난 라운드에는 발동하지 않음)
- 숙달된의 AP가 최대 AP를 넘을 수 있는지 (현재 `RefillAPGA`가 `MaxAP`로 자름)
- 소문 번호 2번 (목록에서 비어 있음)

## 6. 알려진 한계 (고치지 않고 보고만)
- `HeroHandDebugWindow.cs`가 `HeroData.APCost`를 직접 읽으면 소문 보정이 빠진다(디버그 창이라 손대지 않았다. 현재 코드에서 사용 여부는 재확인 필요).
- 숙달된 영웅이 여럿이면 영웅마다 각각 AP가 추가된다(유닛별 구독).
