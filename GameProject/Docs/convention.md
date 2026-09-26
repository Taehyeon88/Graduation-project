# 코딩 컨벤션 & 코드 계약 (convention.md)

> `Assets/Game/0_Scripts/1.Systems`와 이를 지탱하는 `General/`, `Interfaces/`, `2.GameActions/`의 실제 코드를 전수 조사해서, 지금 코드가 **실제로 따르는** 규약을 역추출한 문서다. 희망 규칙이 아니라 관찰 결과이므로, 코드가 바뀌면 이 문서도 다시 검증해야 한다.
> 큰 틀(코드 규약 → 폴더 위치 → 제공 인프라 → 인터페이스 계약)은 이전 `convention.md`(뱀서라이크 토이프로젝트 기준 구버전, 이제 이 문서로 대체됨)와 `Docs/agile-turn-grid-doc-structure-plan.md`를 참고했지만, 내용은 전부 이 프로젝트의 실제 코드 기준으로 새로 썼다. 옛 프로젝트 전용 내용(물리 레이어, 투사체 타깃 규칙, `IWeapon`/`IPickup`)은 포함하지 않는다.

---

## 목차

| # | 절 |
|---|---|
| 1 | [네이밍](#sec-1) |
| 2 | [필드 · 프로퍼티 규약](#sec-2) |
| 3 | [메서드 구성](#sec-3) |
| 4 | [동시성 · 주석 스타일](#sec-4) |
| 5 | [폴더 위치](#sec-5) |
| 6 | [제공 인프라 코드](#sec-6) |
| 7 | [인터페이스 계약](#sec-7) |
| 8 | [매직넘버](#sec-8) |
| 9 | [시그니처(열거형)](#sec-9) |
| 10 | [SO 데이터 형태](#sec-10) |
| 11 | [현재 위반 사례](#sec-11) |

---

<a id="sec-1"></a>
## 1. 네이밍

- **MonoBehaviour 오케스트레이터**: `~System` 접미사. 예: `EnemySystem`, `HeroSystem`, `MoveSystem`, `SkillSystem`, `DamageSystem`, `TurnSystem`, `APSystem`, `MPSystem`.
- **`~Processor`**: `~System` 중에서도 `ActionSystem`에 Performer만 등록해 GA를 처리할 뿐, 외부에서 `.Instance`로 조회할 상태가 없는 클래스는 `~System` 대신 `~Processor` 접미사를 쓴다. 예: `EffectProcessor`, `AttackEnemyProcessor`, `ShieldBashProcessor`, `SplashProcessor`, `ShoulderBashProcessor`, `KnockBackProcessor`.
- **`GameAction` 서브클래스**: `~GA` 접미사. "GameAction"이라는 단어 자체는 클래스명에 쓰지 않는다. 예: `DealDamageGA`, `TurnGA`, `AttackHeroGA`, `ShieldBashGA`, `KnockBackGA`.
- **인터페이스**: `I` 접두사 + 단일 책임의 작은 캡슐. 예: `IHaveCaster`, `IDamageable`, `IHaveDamage`.
- 파일당 public 타입 1개, 파일명 = 타입명.
- **네임스페이스 미사용** — `1.Systems` 트리 전체에서 `namespace` 선언 0건. 새 코드도 전역 네임스페이스에 추가한다.

<a id="sec-2"></a>
## 2. 필드 · 프로퍼티 규약

- 인스펙터 노출 필드: `[SerializeField] private`. 예: `MatchSetupSystem.cs:9`, `TurnSystem.cs:8`.
- 인스펙터에 노출하면서 외부엔 읽기 전용으로 공개할 때: `[field: SerializeField] public X Prop { get; private set; }`. 예: `SkillSystem.cs:14`, `GameSystem.cs:16`.
- 읽기 전용 공개 상태의 기본형은 `public X Prop { get; private set; }`. 세터에서 부수효과(다른 System 호출 등)가 필요하면 수동 backing field + 커스텀 세터를 쓴다.
  ```csharp
  // HeroSystem.cs:10-28
  public HeroView CurrentHero
  {
      get { return currentHero; }
      set
      {
          if (value == null || currentHero == value) return;
          currentHero = value;
          ...
          SkillSystem.Instance.UpdateSkillsUI(currentHero); // 세터 안에서 다른 System 호출
      }
  }
  ```
- `[Header("...")]`로 필드를 그룹핑한다. 헤더 텍스트는 한글/영문 혼용. 예: `SoundSystem.cs:10,18`(`"BGM,SFX Setting"`, `"MixerGroup"`), `SkillSystem.cs:9,13`.
- 필드 케이싱은 camelCase로 통일한다 (`[field: SerializeField] public X Prop { get; private set; }` 패턴만 예외로 PascalCase 유지). `hero_moveRange`/`hero_mover`/`reserved_hero_moves`(`MoveSystem.cs`), `reserved_Skills`/`current_Selected_Skill`/`start_Switch_Skill`(`SkillSystem.cs`) 등 순수 private 필드의 언더스코어 위반은 정리됨.
  - **(남은 예외)** `[SerializeField]`/`public` 필드 중 일부는 아직 언더스코어가 섞여 있다: `upSkill_Distance`(`SkillSystem.cs`), `bgm_Source_Count`/`sfx_Source_Count`(`SoundSystem.cs`), `selected_Hero_UI`(`HeroSystem.cs`, public) 등. 유니티가 필드명으로 씬/프리팹/에셋에 값을 직렬화하기 때문에, 이름을 바꾸면 기존 인스펙터 참조가 끊길 위험이 있어 `[FormerlySerializedAs]` 적용 여부를 정한 뒤 별도로 정리한다.
- `GameAction`에서 "누가 시전했는가"를 나타내는 프로퍼티명은 `Caster`로 통일하고 `IHaveCaster`를 구현한다. 캐스터+대상이 모두 있는 전투 계열 GA 9개(`AttackHeroGA`, `DealDamageGA`, `PlaySkillGA`, `PerformEffectGA`, `AttackEnemyGA`, `ShoulderBashGA`, `ShieldBashGA`, `SplashGA`, `KnockBackGA`, `HealGA`, `AddStatusEffectGA`) 전부 적용됐다.
  - `IHaveCaster`는 `Token Caster { get; }`를 요구하지만, 캐스터가 항상 특정 구체 타입(예: 스킬 계열은 `HeroView`, 콤보 계열은 `CombatantView`)인 GA는 공개 프로퍼티를 그 구체 타입으로 유지하고 `IHaveCaster`는 **명시적 인터페이스 구현**(`Token IHaveCaster.Caster => Caster;`)으로 만족시킨다. 실제로 몬스터/영웅 양쪽에서 캐스터가 올 수 있어 `Token` 다형성이 필요한 `AttackHeroGA`/`DealDamageGA`만 공개 프로퍼티 자체가 `Token`이다.

<a id="sec-3"></a>
## 3. 메서드 구성

- 순서: Unity 라이프사이클(`Awake`/`OnEnable`/`OnDisable`/`Update`) → `//Publics` → `//Privates` → `//Performers`/`//Reactions`. `#region`은 쓰지 않고 일반 주석 배너로 섹션을 나눈다. 예: `EnemySystem.cs:28,44,90,126`, `SkillSystem.cs:42,70,272`.
  - 단, 짧은 파일(`APSystem.cs`, `MPSystem.cs`, `HealSystem.cs`, `KnockBackProcessor.cs` 등)에는 배너가 생략되는 경우가 많다 — 강제 규칙이라기보다 파일 규모에 따른 관례로 보인다.
- `OnEnable`에서 `ActionSystem.AttachPerformer<T>`/`SubscribeReaction<T>`로 등록한 것은 반드시 `OnDisable`에서 대칭으로 `DetachPerformer<T>`/`UnsubscribeReaction<T>`로 해제한다. 전 파일에서 예외 없이 지켜지는 유일한 규칙이다.
  ```csharp
  void OnEnable()  { ActionSystem.AttachPerformer<XGA>(XPerformer); }
  void OnDisable() { ActionSystem.DetachPerformer<XGA>(); }
  ```
- `OnEnable`/`OnDisable`은 `private`로 통일한다. 프로젝트 전체에서 압도적 다수(약 40개 메서드)가 이미 `private`이었고, 무표기였던 `EnemySystem.cs`/`DamageSystem.cs`/`StatusEffectSystem.cs`/`EffectProcessor.cs`(당시 `EffectSystem.cs`)와 `protected`였던 `TurnSystem.cs`의 `OnEnable`을 여기에 맞춰 정리했다.
- Performer 메서드명은 `GA클래스명 + Performer` 형태로 "GA" 인필스를 유지하는 쪽으로 통일됐다. 예: `AttackEnemyGAPerformer`, `DealDamageGAPerformer`, `PlaySkillGAPerformer`.
  - `AttachPerformer`로 등록되지 않은 일반 헬퍼(`HeroSystem.PlayHeroTurnPerformer`, `EnemySystem.PlayEnemyTurnPerformer`)와 인프라 디스패처(`ActionSystem.PerformPerformer`)는 GA 서브클래스를 처리하는 Performer가 아니므로 이 규칙 대상이 아니다.
- 같은 GA에 여러 리액션을 등록할 땐 숫자 접미사 대신 조건을 드러내는 이름을 쓴다: `"조건" + "타이밍" + Reaction`(`TurnGA`라는 타입명은 반복하지 않음). 예: `HeroSystem.cs:62`의 `EnemyTurnPreReaction`, `EnemySystem.cs`의 `EnemyTurnPostReaction`/`BattleStartPostReaction`(둘 다 `TurnGA`를 구독하지만 `turnGA.Type`이 각각 `Enemy`/`StartBattle`일 때만 동작).
- **이벤트/델리게이트**: `UnityEvent`/`Button.onClick`은 UI 입력 경계에서만 쓰고, 여기서도 `OnEnable`/`OnDisable` 대칭 등록·해제를 지킨다.
  ```csharp
  // StartSceneSystem.cs:32-45
  private void OnEnable()  { startGameButton.onClick.AddListener(StartGame); ... }
  private void OnDisable() { startGameButton.onClick.RemoveListener(StartGame); ... }
  ```
  그 외 순수 게임 로직 간 통신은 C# `Action`/`Action<T>` 델리게이트가 표준이다(`EnemySystem.cs:11`: `public Action<int> EnemyAddEvent { get; private set; }`). `Interactions.cs:22-23,65-74`처럼 정적 이벤트를 "구독/해제를 bool 파라미터 하나로 토글하는 단일 메서드"로 감싸는 패턴도 쓰인다.
  ```csharp
  // Interactions.cs:65-69
  public static void SetSelectGridEvent(Action action, bool isAdd)
  {
      if (isAdd) SelectGridEvent += action;
      else SelectGridEvent -= action;
  }
  ```

<a id="sec-4"></a>
## 4. 동시성 · 주석 스타일

- `async`/`await`는 `1.Systems` 전체에서 0건이다. 모든 비동기/순차 처리는 `IEnumerator` 코루틴 + `ActionSystem.Flow`로 한다. Performer는 즉시 끝나도 관례적으로 `yield return null`을 붙인다(한 프레임에 GA 하나를 처리하는 `Flow` 흐름과 맞추기 위함으로 보인다).
- XML 문서 주석(`/// <summary>`)은 쓰지 않는다. `1.Systems`/`General`/`2.GameActions`/`Interfaces` 전체에서 0건으로 통일됨(과거 `KnockBackProcessor.cs`(당시 `KnockBackSystem.cs`)·`General/Util/UtilityBFS.cs`의 정식 사용례와 `EnemySystem.cs`의 깨진 흉내 전부 일반 주석으로 정리). 새로 작성할 때도 XML 문서 주석을 쓰지 않는다.
- 일반 주석은 한글로 "왜/무엇을 하는지"를 짧게 적는 스타일이다. 예: `//공용 종료시, SE 제거`(`EnemySystem.cs:102`), `//데미지 적용 로직`(`DamageSystem.cs:34`).
- 주석 처리된 죽은 코드(`Debug.Log` 블록 등)를 지우지 않고 남겨두는 사례가 있다 — `DamageSystem.cs:78-90`. 새 커밋에서 굳이 따라 하지 않아도 되지만, 기존 코드를 건드릴 때 관련 없는 죽은 코드를 임의로 지우지는 않는다.
- `Debug.Log`/`Debug.LogWarning`/`Debug.LogError`는 실제 로직 경로에 조건부 컴파일 없이 그대로 남겨두는 게 현재 관례다.

<a id="sec-5"></a>
## 5. 폴더 위치

> 전체 폴더 구조(`0_Scripts` 포함 `Assets/Game/` 전체)는 [Folder.md](Folder.md) 참고.

<a id="sec-6"></a>
## 6. 제공 인프라 코드 (수정 금지 · 상속·사용만)

### Singleton\<T\>

`Assets/Game/0_Scripts/General/Singleton.cs`

```csharp
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected bool cannotInitialize = false;
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            cannotInitialize = true;
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
    }
    protected virtual void OnDestroy() { ... }
    protected virtual void OnApplicationQuit() { ... }
    protected void DontDestroyOnLoad() { ... }
}
```

- 선언: `public class Foo : Singleton<Foo>`, 사용: `Foo.Instance.Bar()`.
- `Awake()`를 오버라이드할 땐 **`base.Awake()`를 가장 먼저 호출**하고, 그다음 `if (Instance != this) return;`으로 중복 인스턴스의 초기화를 막는다.
  ```csharp
  // GameSystem.cs:27-32
  protected override void Awake()
  {
      base.Awake();
      if (Instance != this) return;
      InitializeGameData();
  }
  ```
- `~System` 접미사 클래스는 전부 `Singleton<T>`를 상속한다 — 단, `MatchSetupSystem`처럼 시작 시 한 번만 실행되고 끝나는 부트스트랩 스크립트는 예외다. `ActionSystem`에 Performer만 등록해서 GA를 처리할 뿐 외부에서 조회할 상태가 없는 클래스는 `~System`이 아니라 `~Processor` 접미사를 쓴다(`EffectProcessor`, `AttackEnemyProcessor`, `ShieldBashProcessor`, `SplashProcessor`, `ShoulderBashProcessor`, `KnockBackProcessor`).

### ActionSystem / GameAction / ReactionTiming

`Assets/Game/0_Scripts/General/ActionSystem/`

```csharp
// GameAction.cs
public abstract class GameAction
{
    public List<(GameAction, System.Action)> PreReactions { get; private set; } = new();
    public List<(GameAction, System.Action)> PerformReactions { get; private set; } = new();
    public List<(GameAction, System.Action)> PostReactions { get; private set; } = new();
}

// ReactionTiming.cs
public enum ReactionTiming { PRE, POST }
```

`ActionSystem`은 `Singleton<ActionSystem>`이며, 타입을 키로 하는 정적 딕셔너리로 Performer/Reaction을 등록받아 `Perform(GameAction, Action)`이 시작하는 코루틴(`Flow`)으로 순서를 관리한다: `PreReactions` 처리 → PRE 구독자 호출 → **Performer 실행** → `PerformReactions` 처리 → POST 구독자 호출 → `PostReactions` 처리.

모든 System에서 반복되는 등록/사용 패턴:

```csharp
void OnEnable()  { ActionSystem.AttachPerformer<XGA>(XPerformer); }
void OnDisable() { ActionSystem.DetachPerformer<XGA>(); }
private IEnumerator XPerformer(XGA gameAction) { ... yield return null; }
```

후속 `GameAction`을 연결하는 방법은 두 가지다:

```csharp
// 1) 형제 리액션으로 큐잉 — ShieldBashProcessor.cs:22-23
DealDamageGA dealDamageGA = new(shieldBashGA.Amount, combatants, shieldBashGA.Caster);
ActionSystem.Instance.AddReaction(dealDamageGA);

// 2) 다른 GA의 후속 효과로 종속 연결 — ShieldBashProcessor.cs:26-27
AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.ARMOR, shieldStack, new() { shieldBashGA.Caster }, shieldBashGA.Caster);
dealDamageGA.PostReactions.Add((addStatusEffectGA, null));
```

생성자는 target-typed `new(...)`(C# 9)를 쓴다: `TurnGA turnGA = new(TurnType.StartBattle);`(`MatchSetupSystem.cs:45`).

`GameAction` 서브클래스는 순수 데이터 홀더다 — public 오토 프로퍼티 `{ get; private set; }`만 있고, 생성자에서만 값을 채우며 메서드는 두지 않는다(`AttackHeroGA.cs`, `DealDamageGA.cs`, `PlaySkillGA.cs` 등).

`ActionSystem.Instance.Perform(...)`을 직접 호출하는 곳은 코드베이스 전체에서 부트스트랩 역할의 `MatchSetupSystem.cs:46`(전투 시작)와 큐 소비 루프(`SkillSystem.cs:39`, `MoveSystem.cs:33`)뿐이다. 그 외 모든 곳은 `AddReaction`으로 큐잉한다.

<a id="sec-7"></a>
## 7. 인터페이스 계약 (시그니처 고정)

`Assets/Game/0_Scripts/Interfaces/`

```csharp
// IHaveCaster.cs
public interface IHaveCaster { Token Caster { get; } }

// IDamageable.cs
public interface IDamageable { public void Damage(int amount, DealDamageGA dealDamageGA); }

// IHaveDamage.cs
public interface IHaveDamage { float Damage_Amount { get; } }
```

- **`IHaveCaster`는 전투 중 캐스터와 대상이 모두 있는 GA에는 무조건 적용한다.** 새 GA를 추가할 때도 이 조건에 해당하면 예외 없이 구현한다(현재 9개: `AttackHeroGA`, `DealDamageGA`, `PlaySkillGA`, `PerformEffectGA`, `AttackEnemyGA`, `ShoulderBashGA`, `ShieldBashGA`, `SplashGA`, `KnockBackGA`, `HealGA`, `AddStatusEffectGA`). `TurnGA`, `MoveGA`, `SpendAPGA` 등 캐스터 개념이 없는 순수 턴/자원/이동 GA는 구현하지 않는다.
- **`IDamageable`은 피격이 있는 모든 `Token` 클래스가 상속받는다.** 현재 `CombatantView`(→`HeroView`/`EnemyView`가 상속으로 자동 충족)와 `WaveCoreView`가 구현한다. `HeroPreview`처럼 전투에서 피격되지 않는 `Token` 서브클래스는 구현하지 않는다.
- `IDamageable`/`HeroView`/`Token` 등은 다형성 디스패치보다 **런타임 `as`/`is` 패턴 매칭**으로 다뤄지는 경우가 많다:
  ```csharp
  // EnemySystem.cs:70-71
  var target = TokenSystem.Instance.API.GetTokenByPosition(attackPos) as IDamageable;
  if (target != null && target is HeroView) targets.Add(target);
  ```
  ```csharp
  // SkillSystem.cs:157
  if (ability.Effects[0] is IHaveDamage idamage) { ... }
  ```

<a id="sec-8"></a>
## 8. 매직넘버

> 다른 절과 달리 이 절은 **관찰이 아니라 새로 도입하는 규칙**이다 — 현재 코드엔 일관된 처리 방식이 없고 위반이 더 많다. 새로 작성하는 코드부터 적용하고, 기존 위반은 고치지 않은 채 11절 표에서 체크리스트로만 관리한다.

- **게임플레이 밸런싱 수치(데미지·코스트·범위·확률·지속시간 등)는 코드에 리터럴로 두지 않고 SO 필드 또는 `[SerializeField]` 필드로 노출한다.**
  ```csharp
  // Effects/AttackEnemyEffect.cs:10-15
  [SerializeField] private float amount;
  ...
  AttackEnemyGA attackEnemyGA = new(targetpoes, amount, count, myView, animationType);
  ```
- 현재 위반 사례(이번 작업에서 수정하지 않음, 11절 참고): `APSystem.MaxAP`/`MPSystem.MaxMP`(자동 프로퍼티 초기값 `= 3`), `KnockBackProcessor.crachDamage`(`const int` `= 1`), `SkillSystem`에서 모든 스킬의 AP 코스트가 `SpendAPGA` 기본 파라미터 `1`로 고정(`SkillData`/`SkillAbility`엔 코스트 필드 자체가 없음), `RewardSystem`의 보상 등급 확률(`0.7f`), `WaveSystem`의 웨이브 코어 인접 스폰 반경(`distance <= 2`).

<a id="sec-9"></a>
## 9. 시그니처(열거형)

`Assets/Game/0_Scripts/Enums/`(13개) + `General/ActionSystem/ReactionTiming.cs` + 개별 파일 내부 선언(`InteractionSystem`, `TokenCreator`, `HeroPreview`) 포함, 총 18개 열거형이 있다.

- **GameAction 필드나 SO 데이터의 구분자로 쓰이는 열거형은 System 분기를 좌우하는 고정 계약이다.** 멤버 이름을 바꾸면 그 값으로 분기하는 모든 System이 깨지므로, 기존 멤버명은 그대로 두고 확장만 한다.
  ```csharp
  // Enums/TurnType.cs — TurnGA.Type의 값, TurnSystem/EnemySystem/HeroSystem의 Pre·PostReaction 분기를 결정
  public enum TurnType { Enemy, Player, StartBattle, GameSetUp }
  ```

| 열거형 | 파일 | 분기 위치 |
|---|---|---|
| `TurnType` | `Enums/TurnType.cs` | `TurnGA.Type` → `TurnSystem.cs`/`EnemySystem.cs`/`HeroSystem.cs`의 Pre·PostReaction |
| `ReactionTiming` | `General/ActionSystem/ReactionTiming.cs` | `ActionSystem.SubscribeReaction<T>`/`UnsubscribeReaction<T>`의 PRE/POST 딕셔너리 선택 |
| `StatusEffectType` | `Enums/StatusEffectType.cs` | `StatusEffectData.EffectType` → `StatusEffectSystem.cs:8` 캐시 키, `AddStatusEffectEffect.cs:8` |
| `SETargetMode` | `Enums/SETargetMode.cs` | `AddStatusEffectEffect.cs:15-34` 대상 선택 switch |
| `TargetType` | `Enums/TargetType.cs` | `SkillSystem.cs` 다수 지점(아군/적군/본인 필터링) |
| `HeroAnimationType` | `Enums/HeroAnimationType.cs` | `AttackEnemyProcessor.cs:29,47` 근접/원거리 애니메이션 분기 |
| `ProjectionType` | `Enums/ProjectionType.cs` | `ProjectionCreator.cs:22-27` 프리팹 매핑 |
| `AudioType` | `Enums/AudioType.cs` | `SoundSystem.cs:96-121` 믹서 그룹 switch |
| `TokenType` | `4.Creators/TokenCreator.cs:5` | 같은 파일 `CreateToken`의 switch — 프리팹/셋업 분기 |

- **선언은 있지만 소비하는 코드가 없는 열거형**(정리 후보, 11절 참고): `CardSubType`, `ETargetModeType`, `CardAbilityType`, `DamageFormulaType`, `AudioEditorType`, `SEMachanicsType`(기본값 `None`만 반환).
- **네이밍 관찰:** 대부분 `~Type`/`~Mode`/`~State` 접미사 + PascalCase 멤버. `ETargetModeType`만 `E` 접두사가 붙고, `StatusEffectType`/`ETargetModeType`/`HeroAnimationType`는 멤버가 ALL_CAPS — 다수 스타일과 다른 예외로 11절에 기록.

<a id="sec-10"></a>
## 10. SO 데이터 형태

`Assets/Game/0_Scripts/5.Data/`(스크립트 정의, `~Data` 접미사 11개) ↔ `Assets/Game/1_Datas/`(실제 `.asset` 인스턴스, `1.Enemies/`·`1.Heroes/`·`2.Skills/`·`3.Stages/`·`4.StatusEffects/`·`6.VisualGrids/` 등 타입별 하위 폴더)로 스크립트 폴더와 자산 폴더가 분리되어 있다. `Resources.LoadAll`/Addressables는 쓰지 않는다 — `GameSystem`이 배열을 `[SerializeField]`로 직접 들고, 하위 System은 `GameSystem.Instance.HeroDatas`/`CurrentStageData`로 참조한다(`GameSystem.cs:16,20,24`). `TokenCreator`는 공통 베이스 `TokenData`를 받아 `as HeroData`/`as EnemyData`로 다운캐스트한다.

- **필드 패턴:** `[CreateAssetMenu(menuName = "Data/...")]` + `[field: SerializeField] public X Prop { get; private set; }`.
  ```csharp
  // 5.Data/EnemyData.cs
  [CreateAssetMenu(menuName = "Data/Token/Enemy")]
  public class EnemyData : TokenData
  {
      [field: SerializeField] public int Health { get; private set; }
      [field: SerializeField] public int MovePoint { get; private set; }
      [field: SerializeReference, SR] public Enemy Enemy { get; private set; }
  }
  ```
- `~Data` 클래스 목록: `TokenData`(공통 베이스), `HeroData`/`EnemyData`/`WaveCoreData`(→ `TokenData` 상속), `SkillData`, `PerkData`, `StageData`, `WaveData`, `StatusEffectData`, `SoundData`, `VisualGridData`(전부 `5.Data/`).
- **SO는 데이터만 들고, 동작은 넣지 않는다.** 예외: `VisualGridData.cs`는 `GetVGLayerOrder()`와 중첩 `abstract class VisualGridType`(`FillType`/`BorderType`/`SymbolType`)에 실제 로직을 갖고 있다 — 이번 작업에서 리팩터링하지 않고 11절에 위반으로만 기록한다.

<a id="sec-11"></a>
## 11. 현재 위반 사례 (통일 필요 — 후속 작업 근거)

다수 패턴을 규약으로 삼되, 아래는 그 규약에서 벗어난 실제 사례다. 새 코드를 작성할 땐 위 규약을 따르고, 아래 목록은 기존 코드를 정리할 때의 체크리스트로 쓴다.

| # | 규약 | 위반 파일:라인 |
|---|---|---|
| 1 | (8절) 밸런싱 수치는 SO/`[SerializeField]`로 노출, 리터럴 금지 | `APSystem.cs:7`(`MaxAP = 3`), `MPSystem.cs:7`(`MaxMP = 3`), `KnockBackProcessor.cs:8`(`crachDamage = 1`), `SkillSystem.cs:275`(스킬 AP 코스트 미데이터화), `RewardSystem.cs:24`(`0.7f`), `WaveSystem.cs:43`(`distance <= 2`) |
| 2 | (9절) 열거형은 접두사 없이 `~Type`/`~Mode`/`~State` 접미사 + PascalCase | `ETargetModeType`만 `E` 접두사 (나머지 17개엔 없음) |
| 3 | (9절) 열거형 멤버는 PascalCase | `StatusEffectType`/`ETargetModeType`/`HeroAnimationType`는 멤버가 ALL_CAPS |
| 4 | (9절) 열거형은 실제로 분기에 쓰여야 함(정리 후보) | `CardSubType`, `ETargetModeType`, `CardAbilityType`, `DamageFormulaType`, `AudioEditorType`, `SEMachanicsType` — 소비하는 코드 없음 |
| 5 | (10절) SO는 데이터만, 동작 금지 | `VisualGridData.cs`(`GetVGLayerOrder()`, 중첩 `VisualGridType`/`FillType`/`BorderType`/`SymbolType`의 `GetVGType*()`) |
