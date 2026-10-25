# 소문(Rumor) 만들기 — 분류 → 클래스 → 에셋 → 문서

> 소문 하나를 처음부터 끝까지 만드는 레시피. 구조·계약은 [rumor.md](../feature-spec/Rumor/rumor.md), 완성된 소문 목록은 [rumor_catalog.md](../feature-spec/Rumor/rumor_catalog.md)가 갖고 있으므로 여기서는 **만드는 순서와 판단 기준**만 다룬다. 커스텀 슬래시 커맨드 `/rumor 소문명, 효과 설명`(`.claude/commands/rumor.md`, 아직 없음)이 이 문서의 절차를 따라 클래스·에셋·문서를 만든다. 첫 구현 사례는 **1 강력한**(상시 수정자)과 **3 숙달된**(이벤트 반응형)이다.

## 1. 언제 쓰나
영웅이 진행 중 얻는 **영웅당 1개** 능력(소문)을 새로 만들 때. 예: "공격력 +3을 얻습니다. 배치 시 소모되는 행동력이 1증가합니다."
- 소문은 특성(Trait)과 다르다. **조건+효과로 나누지 않고 `Rumor` 추상 클래스 구현체 하나**가 맡는다.
- 이미 있는 소문의 수치만 다르면 새 클래스 없이 §5(에셋)만 수행한다 (수치는 구체 클래스의 `SerializeField`).

## 2. 사전 읽기 (관련 부분만)
- `Docs/feature-spec/Rumor/rumor.md` — **가장 먼저.** §3 구체 소문 표에서 재사용할 클래스가 있는지 확인
- `Assets/Game/0_Scripts/Models/Rumor.cs` — 계약(훅 3개)
- `Assets/Game/0_Scripts/Models/Hero.cs`, `3.Views/HeroView.cs` — 소문 저장·적용 위치
- `Assets/Game/0_Scripts/Rumors/PowerfulRumor.cs`(상시 수정자 예), `MasteredRumor.cs`(구독형 예)
- 소문이 구독·예약할 GA 파일(`2.GameActions/`)과 그 GA를 수행하는 System
- `Docs/convention.md` (코드 규칙)
- **지금 코드를 먼저 확인한다.** 이 문서와 코드가 어긋날 수 있다(예: 초기 조사 때는 덱이 `List<HeroData>`였지만 실제로는 `Hero` 런타임 클래스가 생겨 있었다). 어긋나면 조용히 맞추지 말고 어느 쪽이 낡았는지 먼저 알린다.

## 3. 소문 분류 (먼저 결정)

문장을 아래 중 어디에 속하는지 가른다. **하나의 소문이 여러 종류를 겸할 수 있다**(강력한 = 스탯 + 비용).

| 종류 | 예 | 구현 방법 |
|---|---|---|
| 스탯 보정 | 공격력 +3 | `OnAdd`에서 `owner.ModifyStat(StatType.X, 수치)` (GA 없이 직접 호출, 양수 전용) |
| 배치 AP 보정 | 비용 +1 | `IAPCostModifier` 구현 (`Card.APCost`가 합산). 상수여야 한다 |
| 이벤트 반응 | 라운드 종료 시 AP +1 | `OnAdd`에서 `ActionSystem.SubscribeReaction<T>`, `OnRemove`에서 해제. 반응에서 후속 GA를 `AddReaction` |
| 그 외 새 보정(예: 카드 드로우 수) | — | 새 인터페이스(`I~Modifier`)가 필요한지 사용자에게 먼저 묻는다 |

문장에 해당하는 GA가 없으면 **새 GA를 만들기 전에 사용자에게 알린다** (`action_command.md` 갱신까지 이어진다).

## 4. 클래스 작성 (`Rumor`)

### 4-1. 원칙 (사용자 지정)
- **추상 클래스 `Rumor`에는 소문 개별 데이터·기능을 넣지 않는다.** 수치와 개별 기능은 **구체 소문 클래스**가 가진다. 추상 클래스는 생명주기 훅(`OnAdd`/`OnRemove`/`Clone`)만 둔다. 새 소문 때문에 `Rumor`에 멤버를 추가하고 싶어지면 멈추고 인터페이스로 분리할 수 있는지 먼저 본다.
- 개별 기능은 **단일 책임 인터페이스**로 노출한다 (`IAPCostModifier`처럼). 이 소문만 구현하고, 사용하는 쪽은 `is`로 검사한다.

### 4-2. 작성 규칙
- 위치 `Assets/Game/0_Scripts/Rumors/`, **한 파일에 클래스 1개**, 파일명 = 클래스명 (`~Rumor`). 파일 첫머리에 `//번호 이름: 의미` 한 줄 주석.
- `Rumor` 상속(+필요한 인터페이스). 수치는 `[SerializeField] private`으로 노출하고 기본값을 둔다 (매직넘버 금지, 예: `attackBonus = 3`).
- 구독형은 `OnAdd`에서 등록하고 `OnRemove`에서 **같은 GA·같은 타이밍으로 대칭 해제**한다. `OnRemove`는 `HeroView.OnDisable`이 호출한다.
- 소유자(`owner`)가 필요하면 필드에 저장한다. `HeroView`가 `Clone()`한 **유닛별 사본**이라 안전하다. 단 Dictionary 같은 참조형 상태가 있으면 `Clone()`을 오버라이드해 새로 만든다.
- 요청한 소문만 만든다. 인접 코드·서식은 건드리지 않는다.

골격 예시 (`MasteredRumor`):
```csharp
using UnityEngine;

//3 숙달된: 전장에 있는 동안 라운드 종료(Enemy 턴 종료)마다 AP를 추가로 얻는다
public class MasteredRumor : Rumor
{
    [SerializeField] private int apAmount = 1;

    public override void OnAdd(HeroView owner)
    {
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    public override void OnRemove()
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    private void TurnGAPostReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Enemy) return;

        ActionSystem.Instance.AddReaction(new RefillAPGA(apAmount));
    }
}
```

### 4-3. 타이밍·소유자 주의
- 구독형은 `Trait`과 달리 **소유자 필터가 없다.** 구독한 GA는 모든 영웅에게 오므로 소문이 owner와 관련된 판정을 직접 해야 한다 (`IHaveCaster`면 `Caster == owner` 비교, 사망은 `KillGA.Token == owner`).
- 사망 계열은 PRE로 구독한다. 토큰이 파괴되면 `OnDisable` → `OnRemove`로 구독이 해제되어 POST에서는 놓칠 수 있다 (`trait_howto.md` §3-3과 같은 이유).
- "전장에 있는 동안"은 `OnAdd`(배치 직후)부터 `OnRemove`(뷰 비활성)까지다.
- `RefillAPGA` 같은 AP 획득은 `MaxAP`로 잘린다. "추가로"가 상한을 넘을 수 있는지는 (원작 확인 필요).

### 4-4. 계약 변경이 필요하면 먼저 알린다
`Rumor`·`Hero`·`HeroView`·`Card`·기존 `OnDisable`의 시그니처나 흐름을 바꿔야 하면 **구현 전에 차이를 짚고 승인받는다** (예: `CombatantView.OnDisable`을 `protected virtual`로 바꾼 것). 변경하면 `rumor.md`도 함께 갱신한다.

## 5. 에셋 단계 (`RumorData`)

`RumorData` 에셋은 **사용자가 요청할 때만** 만든다. 요청이 없으면 아래 인스펙터 설정만 안내한다.

- 위치 `Assets/Game/1_Datas/14.Rumors/`. 파일명은 `번호_이름` (`RumorData.Title`이 `_` 뒤쪽을 잘라 만든다). `Id`는 번호와 같게, `Description`은 사용자가 준 문장 그대로 채운다.
- `Image`는 임시 스프라이트로 사용자가 직접 연결한다 (비워 둔다).
- **Unity MCP로 만드는 방법** (강력한·숙달된에서 확인):
  1. `manage_asset`(`create_folder`)로 폴더 확인/생성.
  2. `manage_scriptable_object`(`create`, `type_name`=`RumorData`)에 `patches`로 `Id`와 `Description`을 넣는다. `propertyPath`는 자동 프로퍼티 백킹 필드명을 쓴다 (`<Id>k__BackingField`, `<Description>k__BackingField`, `op`=`set`).
  3. `Rumor` 필드는 `[SerializeReference]`라 patches로 채울 수 없었다. `execute_code`(`action`=`execute`)에서 `new SerializedObject(asset).FindProperty("<Rumor>k__BackingField").managedReferenceValue = new X()` → `ApplyModifiedPropertiesWithoutUndo` → `SetDirty` → `AssetDatabase.SaveAssets`로 지정한다. 반환값으로 `asset.Rumor.GetType().Name`을 확인한다.
  - `execute_code`는 로컬 함수를 지원하지 않는다 (`void F(){}` → 컴파일 실패). 직접 풀어서 쓴다.
- 클래스 수치는 기본값으로 들어간다. 다른 값이면 인스펙터에서 바꾼다.
- 한글 파일명은 에셋 경로에서 그대로 쓸 수 있다.

## 6. 문서 갱신
- [rumor_catalog.md](../feature-spec/Rumor/rumor_catalog.md)에 소문 섹션을 추가한다 (Id / 설명 / 소문 클래스 / 수치 / 동작 / 파생 GA / 코드 + 연결 흐름 + 주의·한계 + 에디터 설정). 견본은 1 강력한·3 숙달된 섹션이다. 상단 "현재 수록" 개수도 고친다.
- 새 소문 클래스를 만들었으면 [rumor.md](../feature-spec/Rumor/rumor.md) §3 표에 행을 추가한다 (새 인터페이스를 만들었으면 §1 표에도).
- 에셋을 만들었으면 카탈로그의 "에셋(예정)" 표기를 실제 경로로 바꾼다.
- `Docs/Folder.md`의 `1_Datas`/`0_Scripts` 표에 새 폴더·파일이 필요한지는 **보고만** 한다 (낡은 서술은 고치지 않는다).

## 7. 검증
1. `refresh_unity` → `read_console`로 에러 0 확인. 새로고침 직후 컴파일 중일 수 있으니 상태가 idle인지 본 뒤 읽는다.
2. 기존 `Rumor`/`Hero`/`Card` 코드가 컴파일되는지(시그니처 영향) 함께 확인.
3. Play 확인은 직접 한 척 하지 말고 사용자에게 안내한다. 확인 항목:
   - 보정형: 카드 비용 표시·실제 AP 차감이 일치하는지, 배치된 영웅의 스탯, 호버 패널의 소문 슬롯
   - 구독형: 발동 타이밍에 효과가 나는지, 영웅이 죽은 뒤 더 이상 발동하지 않는지
   - 소문이 없는 영웅은 변화가 없고 소문 슬롯이 `Null_Text`인지

## 8. 함정·알려진 한계
- **획득 경로가 없다.** `Hero.AddRumor`를 부르는 코드가 없어 소문은 인게임에서 붙지 않는다. 테스트하려면 임시로 호출해야 하며, 정식 경로(보상·상점 등)는 별도 설계가 필요하다.
- 소문은 `HeroData`(SO)가 아니라 **`Hero.Rumor`(런타임)** 에 있다. `HeroData`에는 시작 소문 필드를 두지 않는다.
- 이미 소문이 있는 영웅에게 줄 때의 교체·거부는 호출하는 쪽이 `CanAddRumor`로 거른다. `AddRumor`는 null이 아니면 덮어쓴다 (규칙은 원작 확인 필요).
- 스탯 보정은 배치된 개체(`HeroView`)에만 적용되고 `Hero`의 보너스 값은 바뀌지 않는다. 다시 배치될 때마다 `OnAdd`가 적용한다.
- 보상 후보 표시용 임시 개체(`new Hero(data)`)에는 소문이 없다.
- `HeroHandDebugWindow`처럼 `HeroData.APCost`를 직접 읽는 곳은 소문 비용 보정이 빠진다.
- 같은 구독 방식의 소문이 여럿이면 발동 순서는 보장하지 않는다. 같은 소문을 가진 영웅이 여럿이면 각자 따로 발동해 중첩된다.
- 원작 규칙을 모르는 항목은 단정하지 말고 `(원작 확인 필요)`로 표기하고 임시값을 `SerializeField`로 노출해 둔다.

## 9. 진행 방식 (이 프로젝트 합의)
- 구현은 **단계마다 사용자 허락을 받고** 진행한다: ① 추상/인터페이스/Data → ② 구체 소문 → ③ 기존 코드 연결(기존 코드 수정이 있으면 이 단계에서 따로 확인) → ④ 문서 → ⑤ 에셋.
- 각 단계 전에 의도를 한 줄로 말하고, 명세와 다르게 가야 하면 차이를 먼저 알린다.
- 에디터에서만 할 수 있는 일(Image 연결, Play 테스트)은 클릭 순서를 안내하고 멈춘다.

## 10. 완료 보고 형식
- 만든 소문 클래스, 구현한 인터페이스, 구독/예약 GA, 타이밍
- 계약·기존 코드 변경 여부 (있으면 어떤 문서를 갱신했는지)
- 에셋을 만들었는지 여부와 연결 내용
- Console 결과 (에러 수)
- 남긴 한계와 Play 확인 항목
