# 코딩 컨벤션 & 코드 계약 — Draft (convention.test.md)

> 이 문서는 `Docs/convention.md`(구 버전 — 뱀서라이크 토이프로젝트 기준으로 작성됨)를 대체하기 위한 **초안**이다.
> `Assets/Game/0_Scripts/1.Systems`와 이를 지탱하는 `General/`, `Interfaces/`, `2.GameActions/`의 실제 코드를 전수 조사해서, 지금 코드가 **실제로 따르는** 규약을 역추출했다. 희망 규칙이 아니라 관찰 결과이므로, 코드가 바뀌면 이 문서도 다시 검증해야 한다.
> 큰 틀(코드 규약 → 폴더 위치 → 제공 인프라 → 인터페이스 계약)은 기존 `convention.md`와 `Docs/agile-turn-grid-doc-structure-plan.md`를 참고했지만, 내용은 전부 이 프로젝트의 실제 코드 기준으로 새로 썼다. 옛 프로젝트 전용 내용(물리 레이어, 투사체 타깃 규칙, `IDamageable`/`IWeapon`/`IPickup`)은 포함하지 않는다.

---

## 1. 네이밍

- **MonoBehaviour 오케스트레이터**: `~System` 접미사. 예: `EnemySystem`, `HeroSystem`, `MoveSystem`, `SkillSystem`, `DamageSystem`, `TurnSystem`, `APSystem`, `MPSystem`.
- **`GameAction` 서브클래스**: `~GA` 접미사. "GameAction"이라는 단어 자체는 클래스명에 쓰지 않는다. 예: `DealDamageGA`, `TurnGA`, `AttackHeroGA`, `ShieldBashGA`, `KnockBackGA`.
- **인터페이스**: `I` 접두사 + 단일 책임의 작은 캡슐. 예: `IHaveCaster`, `IDamage`, `IHaveDamage`.
- 파일당 public 타입 1개, 파일명 = 타입명.
- **네임스페이스 미사용** — `1.Systems` 트리 전체에서 `namespace` 선언 0건. 새 코드도 전역 네임스페이스에 추가한다.

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
- **(위반 사례)** 필드명 케이싱이 camelCase로 통일돼 있지 않다. 언더스코어 인필스가 여러 파일에 반복된다: `hero_moveRange`/`hero_mover`/`reserved_hero_moves`(`MoveSystem.cs:9-11`), `upSkill_Distance`/`reserved_Skills`/`current_Selected_Skill`/`start_Switch_Skill`(`SkillSystem.cs:15-21`), `bgm_Source_Count`/`sfx_Source_Count`(`SoundSystem.cs:11-12`) — 그런데 같은 `SoundSystem.cs:14`의 `bgmTransform`은 순수 camelCase다. 한 파일 안에서도 혼용된다.
- **(위반 사례)** `GameAction`에서 "누가 시전했는가"를 나타내는 프로퍼티명이 세 갈래로 갈린다:
  - `Caster` + `IHaveCaster` 구현 — `AttackHeroGA.cs:8`, `DealDamageGA.cs:10`
  - `MyView` (PascalCase, 인터페이스 미구현) — `PlaySkillGA.cs:9`
  - `myView` (camelCase 공개 프로퍼티, 인터페이스 미구현) — `ShieldBashGA.cs:9`, `ShoulderBashGA.cs:12`

## 3. 메서드 구성

- 순서: Unity 라이프사이클(`Awake`/`OnEnable`/`OnDisable`/`Update`) → `//Publics` → `//Privates` → `//Performers`/`//Reactions`. `#region`은 쓰지 않고 일반 주석 배너로 섹션을 나눈다. 예: `EnemySystem.cs:28,44,90,126`, `SkillSystem.cs:42,70,272`.
  - 단, 짧은 파일(`APSystem.cs`, `MPSystem.cs`, `HealSystem.cs`, `KnockBackSystem.cs` 등)에는 배너가 생략되는 경우가 많다 — 강제 규칙이라기보다 파일 규모에 따른 관례로 보인다.
- `OnEnable`에서 `ActionSystem.AttachPerformer<T>`/`SubscribeReaction<T>`로 등록한 것은 반드시 `OnDisable`에서 대칭으로 `DetachPerformer<T>`/`UnsubscribeReaction<T>`로 해제한다. 전 파일에서 예외 없이 지켜지는 유일한 규칙이다.
  ```csharp
  void OnEnable()  { ActionSystem.AttachPerformer<XGA>(XPerformer); }
  void OnDisable() { ActionSystem.DetachPerformer<XGA>(); }
  ```
- **(위반 사례)** `OnEnable`/`OnDisable`의 접근제한자가 파일마다 다르다: 무표기(`EnemySystem.cs:13,20`, `DamageSystem.cs:14,19`), `private`(`MoveSystem.cs:12,17`, `HeroSystem.cs:32,36`, `SkillSystem.cs:23,27`), `protected`(`TurnSystem.cs:15`).
- **(위반 사례)** Performer 메서드명에서 "GA" 인필스를 유지할지 뺄지 약 50/50으로 갈린다.
  - GA 유지: `AttackEnemyGAPerformer`(`AttackEnemySystem.cs:21`), `ShieldBashGAPerformer`(`ShieldBashSystem.cs:16`), `SplashGAPerformer`(`SplashSystem.cs:16`), `ShoulderBashGAPerformer`(`ShoulderBashSystem.cs:16`), `KnockBackGAPerformer`(`KnockBackSystem.cs:27`), `MoveGAPerformer`/`PerformMoveGAPerformer`(`MoveSystem.cs:111,133`), `TurnGAPerformer`(`TurnSystem.cs:29`)
  - GA 생략: `DealDamagePerformer`/`KillPerformer`(`DamageSystem.cs:26,71`), `SpendAPPerformer`/`RefillAPPerformer`(`APSystem.cs:24,31`), `SpendMPPerformer`/`RefillMPPerformer`(`MPSystem.cs:24,31`), `PlaySkillPerformer`(`SkillSystem.cs:273`), `EnemyTurnPerformer`/`AttackHeroPerformer`(`EnemySystem.cs:47,62`), `GameClearPerformer`/`GameOverPerformer`(`GameSystem.cs:51,60`)
  - 어느 쪽도 다수라 부를 만큼 우세하지 않으므로, 통일 방향은 8절 참고.
- **(위반 사례)** 같은 GA에 POST 리액션을 두 개 붙일 때 설명적 이름 대신 숫자 접미사로만 구분: `TurnGAPostReaction`/`TurnGAPostReaction2`(`EnemySystem.cs:94,113`).
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

## 4. 동시성 · 주석 스타일

- `async`/`await`는 `1.Systems` 전체에서 0건이다. 모든 비동기/순차 처리는 `IEnumerator` 코루틴 + `ActionSystem.Flow`로 한다. Performer는 즉시 끝나도 관례적으로 `yield return null`을 붙인다(한 프레임에 GA 하나를 처리하는 `Flow` 흐름과 맞추기 위함으로 보인다).
- XML 문서 주석(`/// <summary>`)은 사실상 쓰지 않는 게 표준이다. 전체 폴더에서 정식 XML 문서 주석은 `KnockBackSystem.cs:22-26` 단 한 곳뿐이고, `EnemySystem.cs:111`에는 슬래시 2개짜리 흉내(`// <summary>`, 닫는 태그 없음)만 있다. 새로 작성할 때 XML 문서 주석을 강제하지 않는다.
- 일반 주석은 한글로 "왜/무엇을 하는지"를 짧게 적는 스타일이다. 예: `//공용 종료시, SE 제거`(`EnemySystem.cs:102`), `//데미지 적용 로직`(`DamageSystem.cs:34`).
- 주석 처리된 죽은 코드(`Debug.Log` 블록 등)를 지우지 않고 남겨두는 사례가 있다 — `DamageSystem.cs:78-90`. 새 커밋에서 굳이 따라 하지 않아도 되지만, 기존 코드를 건드릴 때 관련 없는 죽은 코드를 임의로 지우지는 않는다.
- `Debug.Log`/`Debug.LogWarning`/`Debug.LogError`는 실제 로직 경로에 조건부 컴파일 없이 그대로 남겨두는 게 현재 관례다.

## 5. 폴더 위치

```
Assets/Game/0_Scripts/
├─ 1.Systems/              # 게임플레이 오케스트레이터(~System). Skills/, Skills/Player/ 하위 폴더는
│  ├─ Skills/               #   분류용일 뿐 네이밍/구조 규칙은 루트와 동일하게 적용된다.
│  └─ Skills/Player/
├─ 2.GameActions/          # GameAction(~GA) 서브클래스. CardEffectRelative/ 하위 폴더는 분류용.
│  └─ CardEffectRelative/
├─ 3.Views/                # CombatantView/HeroView/EnemyView 등 토큰의 뷰 레이어
├─ 4.Creators/             # TokenCreator 등 생성 담당
├─ 5.Data/                 # ScriptableObject 데이터(HeroData, SkillData, StageData 등)
├─ Interfaces/             # I 접두사 인터페이스 (IHaveCaster, IDamage, IHaveDamage 등)
├─ Enemies/EnemyActions/   # 적 행동 로직(AttackEA 등)
├─ General/
│  ├─ Singleton.cs         # 제공 인프라 — 상속만, 수정 금지
│  └─ ActionSystem/        # 제공 인프라 — ActionSystem.cs, GameAction.cs, ReactionTiming.cs
└─ GameSystem.cs           # 메타 상태(골드/스테이지/영웅 로스터) 전담 Singleton — "중앙 허브"는 아님(9절 참고 대신 6절 설명)
```

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
      IntializeGameData();
  }
  ```
- 대다수 `~System`이 이 베이스를 상속하지만 전부는 아니다 — 예외는 8절 참고.

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
// 1) 형제 리액션으로 큐잉 — ShieldBashSystem.cs:22-23
DealDamageGA dealDamageGA = new(shieldBashGA.Amount, combatants, shieldBashGA.myView);
ActionSystem.Instance.AddReaction(dealDamageGA);

// 2) 다른 GA의 후속 효과로 종속 연결 — ShieldBashSystem.cs:26-27
AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.ARMOR, shieldStack, new() { shieldBashGA.myView });
dealDamageGA.PostReactions.Add((addStatusEffectGA, null));
```

생성자는 target-typed `new(...)`(C# 9)를 쓴다: `TurnGA turnGA = new(TurnType.StartBattle);`(`MatchSetupSystem.cs:45`).

`GameAction` 서브클래스는 순수 데이터 홀더다 — public 오토 프로퍼티 `{ get; private set; }`만 있고, 생성자에서만 값을 채우며 메서드는 두지 않는다(`AttackHeroGA.cs`, `DealDamageGA.cs`, `PlaySkillGA.cs` 등).

`ActionSystem.Instance.Perform(...)`을 직접 호출하는 곳은 코드베이스 전체에서 부트스트랩 역할의 `MatchSetupSystem.cs:46`(전투 시작)와 큐 소비 루프(`SkillSystem.cs:39`, `MoveSystem.cs:33`)뿐이다. 그 외 모든 곳은 `AddReaction`으로 큐잉한다.

## 7. 인터페이스 계약 (시그니처 고정)

`Assets/Game/0_Scripts/Interfaces/`

```csharp
// IHaveCaster.cs
public interface IHaveCaster { Token Caster { get; } }

// IDamage.cs
public interface IDamage { public void Damage(int amount, DealDamageGA dealDamageGA); }

// IHaveDamage.cs
public interface IHaveDamage { float Damage_Amount { get; } }
```

- `IHaveCaster`는 `AttackHeroGA`, `DealDamageGA`처럼 일부 `GameAction`만 구현한다. `TurnGA`, `ShieldBashGA`, `MoveGA` 등 나머지는 구현하지 않고 임시방편 프로퍼티(`myView` 등)를 대신 쓴다 — 2절/8절에서 지적한 명명 분기의 근본 원인이다.
- `IDamage`/`HeroView`/`Token` 등은 다형성 디스패치보다 **런타임 `as`/`is` 패턴 매칭**으로 다뤄지는 경우가 많다:
  ```csharp
  // EnemySystem.cs:70-71
  var target = TokenSystem.Instance.API.GetTokenByPosition(attackPos) as IDamage;
  if (target != null && target is HeroView) targets.Add(target);
  ```
  ```csharp
  // SkillSystem.cs:157
  if (ability.Effects[0] is IHaveDamage idamage) { ... }
  ```

## 8. 현재 위반 사례 (통일 필요 — 후속 작업 근거)

다수 패턴을 규약으로 삼되, 아래는 그 규약에서 벗어난 실제 사례다. 새 코드를 작성할 땐 위 규약을 따르고, 아래 목록은 기존 코드를 정리할 때의 체크리스트로 쓴다.

| # | 규약 | 위반 파일:라인 |
|---|---|---|
| 1 | `~System` 접미사 클래스는 `Singleton<T>`를 상속한다 | `MatchSetupSystem.cs:7`, `EffectSystem.cs:6`, `AttackEnemySystem.cs:7`, `ShieldBashSystem.cs:5`, `SplashSystem.cs:5`, `ShoulderBashSystem.cs:4`, `KnockBackSystem.cs:6` — 전부 일반 `MonoBehaviour` |
| 2 | Performer 메서드명은 GA 인필스 유무를 통일한다 | 약 50/50 혼용 — 3절 목록 참고 |
| 3 | GA의 시전자 프로퍼티명은 `Caster`(+`IHaveCaster`)로 통일한다 | `PlaySkillGA.cs:9`(`MyView`), `ShieldBashGA.cs:9`(`myView`), `ShoulderBashGA.cs:12`(`myView`) |
| 4 | `OnEnable`/`OnDisable`은 접근제한자를 통일한다 | 무표기/`private`/`protected` 혼용 — 3절 목록 참고 |
| 5 | 필드명은 camelCase로 통일한다 | `MoveSystem.cs:9-11`, `SkillSystem.cs:15-21`, `SoundSystem.cs:11-12`(언더스코어 인필스) |
| 6 | 같은 GA에 여러 POST 리액션을 붙일 땐 설명적 이름을 쓴다 | `EnemySystem.cs:94,113`(`TurnGAPostReaction`/`TurnGAPostReaction2`) |
| 7 | (오타) `HasEnoughAP`는 AP 확인용 메서드명이다 | `MPSystem.cs:20-23` — `CurrentMP`를 검사하면서 메서드명은 `HasEnoughAP` 그대로. 삭제된 `ManaSystem.cs`를 `APSystem`/`MPSystem`으로 복제하며 이름을 고치지 않은 흔적 |
| 8 | (오타) `Initialize`가 올바른 철자다 | `GameSystem.cs:32,88,93,100,104` — `IntializeGameData`/`IntializeHero` 등 `Intialize` 오타가 일관되게 반복됨 |
| 9 | (버그성) `Awake`가 실제 Unity 콜백이다 | `SoundSystem.cs:31-34` — `private void OnAwake() { Initialize(); }`는 Unity가 호출하지 않는 메서드라 죽은 코드이며, 실제로는 `PlaySound()` 최초 호출 시점에 `Initialize()`가 지연 호출된다(`SoundSystem.cs:95-96`) |
| 10 | (미구현) `CombatHistories`는 스텁 상태다 | `CombatHistories.cs:3-19` — `Singleton<CombatHistories>` 상속만 되어 있고 `Start`/`Update`가 비어 있음. 전투 로그/리플레이 기능으로 추정되나 미구현 |
| 11 | XML 문서 주석은 쓰지 않는 게 표준이다 | `KnockBackSystem.cs:22-26`(유일한 정식 사용), `EnemySystem.cs:111`(깨진 흉내) |
