# SkillSystem 아키텍처

> **⚠ 폐기 예정**: 모작 방향 개정으로 수동 스킬 시스템은 제거된다. 용병 카드 배치([04_card_system.md](./04_card_system.md))가 동작한 뒤 제거하며, 그때까지 이 문서는 현재 코드 기준으로 유지한다. `Effect` → `PerformEffectGA` 체인과 `TargetMode`/`RangeMode`는 유닛 행동 실행부로 남는다.

> 이 문서는 스킬(카드) 한 장이 클릭되고 나서 타겟팅 UI를 거쳐 실제 효과 `GameAction`으로 위임되기까지의 흐름을 정리한 참고 문서다. `ActionSystem` 자체의 동작 원리는 [00_action_architecture.md](./00_action_architecture.md), AP 자원 자체는 [01_hero_system.md](./01_hero_system.md)를 먼저 본다. **개별 효과가 실제로 어떻게 처리되는지**(`AttackEnemySystem`/`ShieldBashSystem`/`DamageSystem`/`StatusEffectSystem` 등 `1.Systems/Skills/` 하위 시스템들의 내부 로직)는 이 문서 범위 밖이다 — 여기서는 "SkillSystem이 어디로, 어떤 순서로 위임하는가"만 다룬다.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/1.Systems/SkillSystem.cs` | 오케스트레이터(Singleton) — 타겟팅 UI 코루틴 + `PlaySkillGA` Performer |
| `Assets/Game/0_Scripts/2.GameActions/PlaySkillGA.cs` | GA — "스킬 1장 발동" (payload: `Skill`, 타겟 좌표들, 시전자) |
| `Assets/Game/0_Scripts/2.GameActions/PerformEffectGA.cs` | GA — "효과 1개 실행 위임" (payload: `Effect`, 타겟 좌표들, 시전자) |
| `Assets/Game/0_Scripts/1.Systems/EffectProcessor.cs` | (외부 소유) `PerformEffectGA`의 Performer — `Effect`를 구체 `GameAction`으로 변환 |
| `Assets/Game/0_Scripts/Models/Effect.cs` | 추상 모델 — "이 효과는 어떤 `GameAction`으로 실행되는가" 계약 (`GetGameAction`) |
| `Assets/Game/0_Scripts/5.Data/SkillData.cs` | 스킬 원본 데이터(SO) — 기획자가 인스펙터에서 채우는 디자인타임 소스 |
| `Assets/Game/0_Scripts/Models/Skill.cs` | 런타임 래퍼 — `SkillData` 참조 + **에셋 파일명 기반 `Title` 파싱** |
| `Assets/Game/0_Scripts/Models/SkillAbility.cs` | `SkillData`가 소유하는 타겟 규칙 묶음(`TargetTypes`/`TargetMode`/`RangeMode`/`Effects`) |
| `Assets/Game/0_Scripts/Models/TargetMode.cs` | 추상 — "범위 안에서 마우스 위치로 실제 타겟 칸을 어떻게 뽑는가" (`Single`/`Line`/`Cone`/`GlobalTM`) |
| `Assets/Game/0_Scripts/Models/RangeMode.cs` | 추상 — "시전자 기준 조준 가능 범위가 어디까지인가" (`AllAround`/`Plus`/`Cross`/`SnowRM` 등) |
| `Assets/Game/0_Scripts/3.Views/SkillView.cs` | 스킬 버튼 — **실제 발동 진입점**(AP 체크 후 `SkillSystem.PlaySkillTargetMode` 호출) |
| `Assets/Game/0_Scripts/UI/SkillsUI.cs` | 스킬 슬롯 목록 UI — 보유 스킬 갱신 + 활성 슬롯 재정렬 연출 |
| `Assets/Game/0_Scripts/Models/Hero.cs`, `GameSystem.cs` | (외부) `SkillData → Skill` 변환 및 `Hero.Skills` 조립 지점 |
| `Assets/Game/0_Scripts/1.Systems/Interactions.cs` | (외부) 입력/모드 플래그 허브 — `IsSkillTargetMode`, `GridSelected`, `CancelUse` 등 |
| `Assets/Game/0_Scripts/4.Creators/VisualGridCreator.cs` | (외부) 그리드 하이라이트 생성/제거 유틸리티 — 내부 로직은 범위 밖 |
| `Assets/Game/0_Scripts/UI/SkillHighlightUI.cs`, `TooltipSystem.cs`, `PopUpAmountUI.cs` | (외부) 스킬 호버/선택 연출 보조 — 내부 로직은 범위 밖 |
| `Assets/Game/0_Scripts/Interfaces/IHaveCaster.cs`, `IHaveDamage.cs` | 여러 GA/Effect가 구현하는 공용 계약 (`Caster`, `Damage_Amount`) |

## 1. 한 줄 개요

`SkillSystem`은 "스킬 한 장을 실제로 발동시키는" 유일한 창구이자, 타겟팅 UI(그리드 하이라이트 미리보기)까지 직접 코루틴으로 그리는 오케스트레이터다. `EnemySystem`/`HeroSystem`과 달리 실행 로직이 **3단계 위임 체인**(`PlaySkillGA` → `PerformEffectGA` → 구체 효과 GA)으로 나뉘어 있고, 최종 처리는 항상 `SkillSystem` 바깥(`EffectProcessor`, 그리고 효과별 전용 시스템)에 있다.

## 2. 3단계 위임 체인 (핵심 구조)

```
Skill (SkillData 래퍼, 카드 1장)
 └ SkillAbility.Effects: List<Effect>   ← 카드 한 장이 여러 효과를 가질 수 있음

[1단계] PlaySkillGA  ─Performer→ SkillSystem.PlaySkillGAPerformer
          │ SpendAPGA를 AddReaction (AP 소비)
          │ Effects 각각에 대해 PerformEffectGA를 AddReaction
          ▼
[2단계] PerformEffectGA  ─Performer→ EffectProcessor.PerformEffectGAPerformer
          │ effect.GetGameAction(targetPoses, caster)로 구체 GA 생성
          │ 그 GA를 AddReaction
          ▼
[3단계] 구체 효과 GA (AttackEnemyGA / ShieldBashGA / SplashGA / AddStatusEffectGA / HealGA ...)
          └ 각자 전용 시스템이 Performer로 처리 (이 문서 범위 밖)
```

| Effect 구현체 | 반환하는 GA | 처리 시스템(범위 밖, 참고용) |
|---|---|---|
| `AttackEnemyEffect` | `AttackEnemyGA` | `AttackEnemySystem` |
| `ShieldBashEffect` | `ShieldBashGA` | `ShieldBashSystem` |
| `ShoulderBashEffect` | `ShoulderBashGA` | `ShoulderBashSystem` |
| `SplashEffect` | `SplashGA` | `SplashSystem` |
| `AddStatusEffectEffect` | `AddStatusEffectGA` | `StatusEffectSystem`(추정) |
| `HealEffect` | `HealGA` | (미확인, 범위 밖) |

### 2-1. PlaySkillGA — "스킬 1장 발동"

```csharp
public class PlaySkillGA : GameAction, IHaveCaster
{
    public Skill Skill { get; set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView Caster { get; private set; }
}
```

`SkillSystem`이 유일하게 `AttachPerformer`로 등록하는 GA다(`SkillSystem.cs:25`). Performer(`PlaySkillGAPerformer`, `SkillSystem.cs:273-284`)는 실제 효과 로직을 전혀 갖지 않고, **AP 소비 1건 + 효과 개수만큼의 위임**만 큐잉한다.

### 2-2. PerformEffectGA — "효과 1개 실행 위임"

```csharp
public class PerformEffectGA : GameAction, IHaveCaster
{
    public Effect Effect { get; set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView Caster { get; private set; }
}
```

Performer는 `SkillSystem`이 아니라 `EffectProcessor`(별도 `MonoBehaviour`)가 소유한다(`EffectProcessor.cs:10`). 이 Performer가 하는 일은 정확히 한 줄이다: `effect.GetGameAction(...)`으로 구체 GA를 만들어 `AddReaction`하는 것.

### 2-3. Effect — "이 효과는 어떤 GameAction인가" (추상)

```csharp
public abstract class Effect
{
    public abstract GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView);
}
```

`Effect` 자신은 실행 로직이 전혀 없다 — 오직 "나는 실행되면 어떤 `GameAction`이 되는가"만 안다. 실제 로직(피해 계산, 넉백, 상태이상 부여 등)은 그 `GameAction`을 처리하는 전용 시스템에 있다.

## 3. 데이터 모델 & UI 뷰 계층 — SkillData → Skill → SkillView/SkillsUI

앞 절이 "발동된 이후"의 흐름이라면, 이 절은 **2절의 `PlaySkillGA.Skill`이 애초에 어디서 오고 화면에 어떻게 뜨는지**를 다룬다.

### 3-1. SkillData — 스킬 원본 데이터 (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public SkillType SkillType { get; private set; }
    [field: SerializeField] public SkillAbility SkillAbility { get; private set; }
}
```

기획자가 인스펙터에서 직접 채우는 디자인타임 원본이다. `SkillType`(`Enums/CardType.cs`: `Attack_Adjacent`/`Skill_Buff`/`Skill_Area` 등)은 분류용 태그일 뿐 — 실제 동작(타겟팅/효과)은 전부 `SkillAbility`가 가진 `TargetMode`/`RangeMode`/`Effects`가 결정한다(2절, 5절). `SkillData` 자신에는 **이름(`Title`) 필드가 없다** — 3-2절 참고.

### 3-2. Skill — 런타임 래퍼

```csharp
public class Skill
{
    public int Id => data.Id;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public SkillType CardType => data.SkillType;
    public SkillAbility SkillAbility => data.SkillAbility;
    public string Title { get; private set; }

    public readonly SkillData data;

    public Skill(SkillData data)
    {
        this.data = data;
        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0 ? name.Substring(index + 1) : name;
    }
}
```

`SkillData`를 그대로 쓰지 않고 `Skill`로 한 번 감싸는 이유는 이 `Title` 계산 때문이다 — **`SkillData` 에셋의 파일명에서 `_` 뒤쪽을 잘라 제목으로 쓰는 숨은 네이밍 규칙**이 여기 있다(예: 에셋 이름이 `1_파이어볼`이면 `Title == "파이어볼"`). `_`가 없으면 파일명 전체가 그대로 제목이 된다(6절 참고).

### 3-3. 로드 파이프라인 — SkillData가 화면에 뜨기까지

```
GameSystem.InitializeHero()                          (GameSystem.cs:104-124)
 └ HeroData.StartingSkills: List<SkillData> 순회
    └ new Skill(skillData) → List<Skill> 조립
 └ new Hero(id, hp, skills, perks)                    (Models/Hero.cs)

HeroView.SetUp(heroData)                              (HeroView.cs:15-31)
 └ this.Hero = heroData.Hero                           ← 이 Hero 인스턴스를 그대로 참조
 └ HeroView.Skills => Hero.Skills                       (읽기 전용 위임)

HeroSystem.CurrentHero = heroView                      (01_hero_system.md §2-2)
 └ SkillSystem.Instance.UpdateSkillsUI(heroView)        (SkillSystem.cs:65-68)
    └ skillsUI.UpdateSkils(heroView)                    (SkillsUI.cs:40-93)
       └ heroView.Skills를 skillViews[] 슬롯에 순서대로 SetUp
```

`SkillData`(SO) → `Skill`(런타임 래퍼) → `Hero.Skills` → `HeroView.Skills` → `SkillsUI` → `SkillView` 순으로 흘러가며, `SkillSystem`은 이 파이프라인의 마지막 트리거(`UpdateSkillsUI`)만 담당한다 — 실제 로딩·조립은 `GameSystem`/`Hero`/`HeroView` 몫이다.

### 3-4. SkillView — 스킬 버튼 (실제 발동 진입점)

```csharp
public class SkillView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Skill Skill { get; private set; }
    public bool CancelInteraction { get; set; }
}
```

- `SetUp(skill)`(`SkillView.cs:20-28`): `skill`이 `null`이면 **그냥 반환한다** — 즉 이전에 표시하던 `Skill`/아이콘 참조가 초기화되지 않고 그대로 남는다(6절 참고).
- `Update()`(`SkillView.cs:30-52`): 매 프레임 `APSystem.Instance.HasEnoughAP()`를 폴링해 아이콘을 흑백/컬러로 토글한다 — "사용 가능 여부"를 GA나 이벤트가 아니라 폴링으로 표현하는 유일한 지점.
- `OnPointerClick`(`SkillView.cs:54-69`): AP가 충분하면 `SkillSystem.Instance.PlaySkillTargetMode(this)`를 호출한다 — **이 프로젝트에서 스킬 발동이 시작되는 유일한 진입점**이다. 부족하면 사운드만 재생하고 끝난다.
- `OnPointerEnter`/`OnPointerExit`: `Interactions.CanSkillHovering()`/`CanCancelSkilHovering()` 가드를 거쳐 툴팁·하이라이트를 표시/해제한다 — 스킬 타겟 모드 중에는 호버 연출이 억제된다.

### 3-5. SkillsUI — 슬롯 목록 관리

- 인스펙터에 고정 배열 `skills: RectTransform[]`로 슬롯 개수가 정해진다(`SkillsUI.cs:12`) — `Start()`가 각 슬롯의 `SkillView` 컴포넌트를 캐싱하고 초기 스케일을 0으로 접어둔다.
- `UpdateSkils(heroView)`(`SkillsUI.cs:40-93`): `heroView.Skills.Count`만큼 슬롯에 `SetUp`을 채우고, 이전 대비 늘어나거나 줄어든 슬롯 수만큼 `DOScale` 트윈으로 순차 등장/퇴장 연출을 재생한다(`activeCount`로 이전 상태와 비교).
- `ListenSkillClick(number)`(`SkillsUI.cs:95-100`): `Interactions.SetPlaySkillEvent`로 구독한 키보드 단축키(1~5) 이벤트를 받아 `skillViews[number-1].OnPointerClick(...)`를 합성 호출한다 — 마우스 클릭과 키보드 단축키가 완전히 동일한 `SkillView.OnPointerClick` 경로를 공유한다.

## 4. 발동 흐름 (버튼 클릭 → 효과 실행까지)

```
[진입점] SkillView.OnPointerClick  (SkillSystem 밖, 3.Views/SkillView.cs:54-69)
 └ APSystem.Instance.HasEnoughAP() 체크
    └ 통과 시: SkillSystem.Instance.PlaySkillTargetMode(this)

[SkillSystem.PlaySkillTargetMode] (SkillSystem.cs:43-63)
 └ 타겟 모드 아님 → SkillTargetMode(skillView) 코루틴 시작
 └ 이미 타겟 모드 → 같은 스킬 재클릭 시 취소, 다른 스킬 클릭 시 전환 플래그(startSwitchSkill)만 세팅

[SkillTargetMode 코루틴] (SkillSystem.cs:71-217)
 1. Interactions.IsSkillTargetMode = true
 2. 툴팁 표시 + 하이라이트 + 스킬 아이콘 위로 살짝 이동하는 연출
 3. myPos(시전자 좌표) 계산, TargetTypes로 Self/Enemy/Friendly 분기
    - Self 포함 → 자기 위치에 VG(비주얼 그리드) 표시
    - Enemy/Friendly 포함 → SkillAbility.RangeMode.GetGridRanges(myPos, Distance, ...)로 "조준 가능 범위" 계산 후 VG 표시
 4. while(true) 매 프레임:
    a. 마우스 위치 → TargetMode.GetTargets(range, mousePos, myPos, Distance)로 "지금 마우스가 가리키는 실제 타겟 칸들" 계산
    b. GetTargetPoses(targetPoints, Enemy|Friendly)로 그 칸들 중 실제 대상이 있는 칸만 필터링 → VG 갱신
    c. Effects[0]이 IHaveDamage면 PopUpAmountUI로 데미지 수치 미리보기 (6절 참고)
    d. 그리드 클릭(Interactions.GridSelected) → PlaySkillGA를 reservedSkills 큐에 Enqueue → ResetSelectMode() → 코루틴 종료
    e. 취소 입력/스킬 변경 입력 → 각각 리셋 또는 재귀적으로 PlaySkillTargetMode 재시작

[SkillSystem.Update()] (매 프레임, SkillSystem.cs:32-40)
 └ reservedSkills에 항목이 있고 ActionSystem.IsPerforming == false면
    Dequeue 후 ActionSystem.Instance.Perform(playCardGA)   ← 최상위 진입점(00_action_architecture.md §2-5)

[PlaySkillGAPerformer] (SkillSystem.cs:273-284)
 └ SpendAPGA를 AddReaction
 └ Skill.SkillAbility.Effects를 순서대로 순회하며 각각 PerformEffectGA를 AddReaction

[PerformEffectGAPerformer] (EffectProcessor.cs:16-22)
 └ effect.GetGameAction(targetPoses, caster) → 구체 GA 생성 → AddReaction
    (이후 처리는 그 GA를 처리하는 전용 시스템 몫, 이 문서 범위 밖)
```

**실행 순서는 완전 순차다.** `SpendAPGA`와 각 `PerformEffectGA`는 모두 `PlaySkillGA`의 `PerformReactions` 리스트에 순서대로 쌓이고, `PerformReactions()`가 그 순서 그대로 재귀 `Flow()`를 하나씩 끝까지 완료해가며 처리한다(00_action_architecture.md §3). 즉 **AP 소비가 항상 가장 먼저 끝나고**, `Effects` 리스트 순서대로 한 이펙트의 전체 GA 체인(연출 포함)이 끝나야 다음 이펙트로 넘어간다 — 카드 하나에 여러 효과를 넣어도 동시에 발생하지 않는다.

## 5. 타겟팅 좌표 계산 — RangeMode vs TargetMode (두 축 분리)

- **`RangeMode`**(`Models/RangeMode.cs`): "시전자 기준 조준이 닿는 칸 집합"을 계산한다. `AllAroundRM`/`AllAround_ExpceptEnemyRM`/`PlusRM`/`CrossRM`/`SnowRM` 등 — 대부분 `TokenSystem.API`(`GetAllAroundPlaces`/`IsGridEmpty`)에 위임하고, `penetration`(관통) 옵션에 따라 장애물을 만나면 그 방향 탐색을 멈춘다.
- **`TargetMode`**(`Models/TargetMode.cs`): 그 범위 안에서 "지금 마우스가 가리키는 위치 기준으로 실제 타겟이 되는 칸들"을 계산한다. `SingleTM`(1칸), `LineTM`(직선 관통), `ConeTM`(대각선 부채꼴, `distance == 1`에서만 동작), `GlobalTM`(범위 전체).
- **`SkillSystem.GetTargetPoses`**(`SkillSystem.cs:222-250`): `TargetMode`가 뽑은 칸들 중 `TargetType`(Enemy/Friendly)에 맞는 **실제 토큰이 있는 칸만** 최종 필터링한다 — 이 결과가 `PlaySkillGA.TargetPoses`로 넘어간다.

세 단계는 역할이 분명히 나뉜다: RangeMode(조준 가능 칸) → TargetMode(마우스 기준 타겟 칸 후보) → GetTargetPoses(실제 대상이 있는 칸만).

## 6. 주의사항 / 함정

- **`Skill.Title`은 `SkillData` 에셋의 파일명에서 `_` 뒤쪽을 잘라 만든다**(`Skill.cs:21-23`). 새 `SkillData` 에셋을 만들 때 `번호_이름` 네이밍 규칙을 지키지 않으면(예: `_`가 아예 없는 파일명) 파일명 전체가 스킬 제목으로 그대로 노출된다.
- **`SkillView.SetUp(null)`은 이전 `Skill`/아이콘 상태를 초기화하지 않는다**(`SkillView.cs:20-28`, `if (skill == null) return;`). `SkillsUI`가 슬롯을 비울 때(스킬 개수가 줄어들 때) 스케일만 0으로 접힐 뿐, 그 슬롯의 `SkillView.Skill`은 이전 값을 계속 들고 있다.
- **`SkillsUI`가 채우는 슬롯 개수는 인스펙터 `skills` 배열 길이로 고정된다**(`SkillsUI.cs:12`, `for (i < skillViews.Length)`) — `heroView.Skills.Count`가 배열 길이보다 많아지면 초과분은 에러 없이 조용히 무시된다.
- **"데미지 미리보기는 `Effects[0]`이 `IHaveDamage`여야 뜬다"는 숨은 규칙이 있다**(`SkillSystem.cs:156-161`, 주석: "데미지 수치가 있는 Effect는 무조건 0번째 인덱스 배치!!"). 새 스킬 SO를 만들 때 `Effects` 리스트 순서를 지키지 않으면 **실제 효과 실행에는 영향이 없지만 데미지 팝업 미리보기가 뜨지 않는다.**
- **`myPos` 계산에 원작성자가 남긴 TODO가 있다**: `Vector2Int myPos = TokenSystem.Instance.API.GetTokenPosition(myhero);  //나중에 바꿔야 함`(`SkillSystem.cs:89`). 무엇을 어떻게 바꿔야 하는지는 주석만으로 알 수 없다 — 확인이 필요하다.
- **`reservedSkills`는 `Queue<PlaySkillGA>`지만 실질적으로 한 번에 하나만 쌓인다.** `Interactions.IsSkillTargetMode` 가드 때문에 새 타겟팅 세션은 이전 것이 끝나야 시작되므로, 여러 스킬을 동시에 예약해서 순서대로 터뜨리는 흐름은 현재 코드상 만들어지지 않는다.
- **스킬 전환(`startSwitchSkill`)은 재귀 호출로 처리된다.** 코루틴이 `break`로 끝난 뒤 곧바로 `PlaySkillTargetMode(selected_Skill)`을 다시 호출해 새 코루틴을 시작하는 구조라(`SkillSystem.cs:206-213`), 매번 실제 유저 입력이 있어야만 다음 전환이 걸리긴 하지만 "코루틴이 자신을 재시작"하는 패턴이라는 점은 디버깅 시 주의가 필요하다.
- **`EffectProcessor`는 `Singleton<T>`이 아니라 평범한 `MonoBehaviour`다**(`EffectProcessor.cs:6`). 이 프로젝트의 다른 `~System` 클래스 대부분이 `Singleton<T>`을 상속하는 것과 다른 패턴 — 씬에 정확히 1개만 존재해야 한다는 보장이 코드로 강제되지 않는다.

## 7. 새 스킬/효과 추가 시 체크리스트

1. 새 `SkillData` 에셋을 만들 때 파일명을 `번호_이름` 형식으로 짓는다 — `Skill.Title`이 여기서 파싱된다(6절 참고).
2. `Effect`를 상속한 새 클래스에서 `GetGameAction(targetPoses, myView)`를 구현해, 이 효과가 실행되면 어떤 `GameAction`이 될지 정한다 (예: `AttackEnemyEffect → AttackEnemyGA`, `HealEffect → HealGA`).
3. 그 `GameAction`을 실제로 처리할 Performer(전용 시스템, 예: `AttackEnemySystem`)를 새로 만들거나 기존 걸 재사용한다 — `EffectProcessor`는 GA를 만들어 `AddReaction`만 할 뿐, 실행은 항상 다른 시스템 몫이다(2절 표 참고).
4. 새 `SkillData`(SO)의 `SkillAbility.Effects` 리스트에 등록한다. 데미지 미리보기가 필요하면 `IHaveDamage`를 구현하고 **반드시 0번 인덱스**에 놓는다(6절 참고).
5. 새로운 조준 방식이 필요한 경우가 아니면, 기존 `TargetMode`(`Single`/`Line`/`Cone`/`GlobalTM`) · `RangeMode`(`AllAround`/`Plus`/`Cross`/`SnowRM` 등)로 표현 가능한지 먼저 확인하고 없을 때만 새로 추가한다.
6. 새 영웅에게 스킬을 쥐어주려면 `HeroData.StartingSkills`에 `SkillData`를 등록한다 — `GameSystem.InitializeHero()`가 이 목록을 읽어 `Skill`로 감싼다(3-3절).
