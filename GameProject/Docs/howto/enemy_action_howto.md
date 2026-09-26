# EnemyAction 추가 가이드 — FireArrowEA 사례로 보는 실전 레시피

> `01_enemy_system.md`가 이 시스템의 아키텍처 **관찰 스냅샷**이라면, 이 문서는 "그래서 새 EnemyAction을 실제로 어떻게 추가하나"를 FireArrowEA(원거리 화살 공격) 작업 과정 그대로 보여주는 **how-to**다. 새 몬스터 행동(공격 패턴)을 추가할 때 이 파일 하나만 열어보고 시작하면 된다.
>
> **주의**: `01_enemy_system.md`의 §5(Larva+AttackEA 한 사이클 예시)·§6(사거리 판정 미구현 경고)은 FireArrowEA 작업과 같이 진행된 리팩터링으로 이제 실제 코드와 어긋난 스테일 상태다. 이 문서가 현재 코드 기준 최신 내용이다.

## 0. 핵심 계약 요약

`EnemyAction`(추상 클래스, `Assets/Game/0_Scripts/Models/EnemyAction.cs`) 전체:

```csharp
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class EnemyAction
{
    public List<Vector2Int> targetRange { get; protected set; }
    public Vector2Int targetPosition { get; protected set; }  //PlayEnemyAction 내부에서 스스로 탐색해 채움

    public abstract Sprite Icon { get; protected set; }
    public abstract string Description { get; protected set; }
    public abstract string TextInfo { get; protected set; }

    public abstract Sequence PlayEnemyAction(EnemyView enemy);
    public abstract EnemyAction Clone();  //복사 함수

    //현재 위치 기준, 사거리 내 생존 영웅 중 임의로 하나 탐색. 없으면 null.
    protected HeroView FindRandomHeroInRange(EnemyView enemy, int distance)
    {
        Vector2Int myPos = TokenSystem.Instance.API.GetTokenPosition(enemy);

        var heroesInRange = TokenSystem.Instance.HeroViews
            .Where(h => h != null && h.CurrentHealth > 0)
            .Where(h => TokenSystem.Instance.API.GetDistance(myPos, TokenSystem.Instance.API.GetTokenPosition(h)) <= distance)
            .ToList();

        if (heroesInRange.Count == 0)
            return null;

        return heroesInRange[UnityEngine.Random.Range(0, heroesInRange.Count)];
    }
}
```

**표준 패턴**: `targetPosition`은 더 이상 "외부(`Enemy.PerformAction`)가 미리 채워주는 값"이 아니라, **각 EnemyAction 서브클래스가 `PlayEnemyAction` 안에서 `FindRandomHeroInRange`로 스스로 찾아 채운다.** 대상이 없으면 `PlayEnemyAction`이 `null`을 반환해 이번 행동을 조용히 취소한다 — 호출부(`EnemySystem.PlayEnemyEAGAPerformer`)가 `motion?.WaitForCompletion()`으로 null-safe하게 처리해주므로 별도 방어 코드 불필요.

## 1. 단계별 레시피

### Step 1 — 클래스 스캐폴드

위치: `Assets/Game/0_Scripts/Enemies/EnemyActions/` (예: `AttackEA.cs` 옆에 `FireArrowEA.cs`).
네이밍: `~EA` 접미사. `EnemyAction` 상속 + 필요한 인터페이스(데미지가 있으면 `IHaveDamage`, 사거리 개념이 있으면 `IHaveDistance`).

### Step 2 — Icon / Description / TextInfo

getter 계산 프로퍼티 패턴, setter는 no-op (`AttackEA`/`FireArrowEA` 공통):

```csharp
public override Sprite Icon { get { return icon; } protected set { } }
public override string Description { get { return $"사거리 {Distance}칸 내, 영웅에게 {Damage_Amount} 피해의 원거리 공격"; } protected set { } }
public override string TextInfo { get { return Damage_Amount.ToString(); } protected set { } }
```

### Step 3 — 데이터 필드

밸런스 값(인스펙터 노출용)은 `[field: SerializeField]` 오토 프로퍼티, 내부 전용은 `[SerializeField] private`:

```csharp
[field: SerializeField] public float Damage_Amount { get; protected set; } = 4;
[field: SerializeField] public int Distance { get; protected set; } = 3;
[SerializeField] private Sprite icon;
```

### Step 4 — `PlayEnemyAction(EnemyView enemy)` 본문

순서:
1. `FindRandomHeroInRange(enemy, Distance)`로 대상 탐색. `null`이면 `return null`(행동 취소).
2. `targetPosition`에 대상 좌표 대입.
3. 연출용 `Sequence` 구성(이동형이면 `Utility.GetTween`/`GetBackTween`, 원거리 투사체면 §2 `ProjectileSystem` 참고).
4. 데미지는 **새 GA를 만들지 않고** 기존 `AttackHeroGA`(단일 좌표 또는 영역 리스트 생성자)를 `ActionSystem.Instance.AddReaction`으로 큐잉한다 — 실제 `IDamageable.Damage()` 호출까지는 `EnemySystem.AttackHeroGAPerformer`가 기존 로직 그대로 처리하므로 이 부분은 손댈 필요 없음.
5. `Sequence` 반환.

### Step 5 — `Clone()`

얕은 복사로 새 인스턴스 생성. **`Clone()`이 자기 자신의 새 인스턴스를 반환하는지 반드시 확인** — `Larva.Clone()`이 한동안 `new Scarecrow()`를 반환하던 오탈자가 있었다.

### Step 6 (선택) — 날아가는 투사체가 필요하면 `ProjectileSystem` 재사용

`Assets/Game/0_Scripts/1.Systems/ProjectileSystem.cs`:

```csharp
using DG.Tweening;
using IsoTools;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileSystem : Singleton<ProjectileSystem>
{
    [SerializeField] private IsoObject arrowPrefab;

    private ObjectPool<IsoObject> arrowPool;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        arrowPool = new ObjectPool<IsoObject>(
            createFunc: () => Instantiate(arrowPrefab, TokenSystem.Instance.IsoWorld.transform),
            actionOnGet: arrow => SetArrowActive(arrow, true),
            actionOnRelease: arrow => SetArrowActive(arrow, false),
            actionOnDestroy: arrow => Destroy(arrow.gameObject));
    }

    public Tween PlayArrow(Vector3 start, Vector3 end, float duration, Ease ease = Ease.Unset, float heightRate = 1)
    {
        IsoObject arrow = arrowPool.Get();
        Tween tween = Utility.GetArrowBezierTween(arrow, arrow.transform.GetChild(0), start, end, duration, ease, heightRate);
        tween.OnComplete(() => arrowPool.Release(arrow));
        return tween;
    }

    private void SetArrowActive(IsoObject arrow, bool active)
    {
        arrow.gameObject.SetActive(active);
        arrow.transform.GetChild(0).gameObject.SetActive(active);
    }
}
```

이미 있는 `PlayArrow(start, end, duration, ease)`를 그대로 쓰거나, 새 투사체 종류가 필요하면 이 클래스에 풀을 하나 더 추가한다(현재는 화살 하나뿐이라 범용 키드-풀은 만들지 않았다 — 필요해지면 그때 확장).

**⚠️ 중요 함정 (실제로 겪은 시행착오)**: 투사체 프리팹은 반드시 `TokenSystem.Instance.IsoWorld.transform` 아래에 Instantiate해야 한다. `IsoObject`는 `GetComponentsInParent<IsoWorld>()`로 자기 좌표계를 찾는 구조라(`Assets/IsoTools/Scripts/Internal/IsoBehaviour.cs`의 `FindFirstActiveWorld()`, `IsoObjectBase.cs`의 `OnEnable → Internal_RecacheIsoWorld`), 엉뚱한 부모(예: `ProjectileSystem` 자기 자신의 Transform, `--- SYSTEMS ---` 카테고리 등) 밑에 두면 **논리 좌표(`IsoObject.position`)는 정상적으로 갱신되는데 실제 화면상 Transform은 전혀 움직이지 않는 조용한 버그**가 생긴다. 콘솔 에러도 안 뜨고 값도 맞아 보여서 눈으로 보기 전까지는 알아채기 어렵다. `TokenCreator.CreateToken`이 토큰을 `isoWorld` Transform 밑에 Instantiate하는 것과 같은 이유다.

`FireArrowEA`에서 사용하는 형태:

```csharp
Tween arrowTween = ProjectileSystem.Instance.PlayArrow(
    Utility.Vector2IntToVector3(curPos, 1),
    Utility.Vector2IntToVector3(targetPosition, 1),
    arrowDuration, Ease.Linear);
```

### Step 7 — 이 행동을 실제로 쓸 몬스터 연결

`Enemy` 서브클래스(`Assets/Game/0_Scripts/Enemies/`)에서 두 가지 패턴 중 골라 쓴다:

**(a) 이동이 필요 없는 고정형 몬스터** — `Scarecrow.cs`처럼 `PerformAction`이 그냥 재검증 없이 승인:

```csharp
public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
{
    //대상 탐색은 PlayEnemyAction이 스스로 처리 (이동 없는 몬스터라 재검증할 것도 없음)
    return true;
}
```

**(b) 이동해서 접근해야 하는 몬스터** — `Larva.cs`처럼: 생존 영웅 필터 → 최근접 대상 탐색(`TokenSystem.Instance.API.GetDistance`, 동률이면 랜덤) → 이미 사거리 안이면 즉시 승인(`true`, 대상 재탐색은 `PlayEnemyAction`에 위임) → 아니면 `GetCanMovePlace(enemy, enemy.CurrentMovePoint)`로 이동 가능 칸을 구하고 그중 사거리 안에 들어오는 후보를 `GetShortestPath`로 최단 경로 계산해 `enemy.SpendMovePoint()` + `ActionSystem.Instance.AddReaction(new PerformMoveGA(enemy, bestPath))`로 이동 큐잉 후 승인 → 사거리 진입이 아예 불가능하면 최대한 근접만 이동시키고 공격은 포기(`false`).

`PerformMoveGA`(`Assets/Game/0_Scripts/2.GameActions/PerformMoveGA.cs`)는 원래 플레이어 이동용으로 만들어진 기존 GA를 그대로 재사용한 것 — 새로 만들 필요 없음. `GetCanMovePlace`/`GetShortestPath`/`GetDistance`도 전부 `TokenServiceAPI`의 기존 API.

마지막으로 **에디터 작업**: 새 EnemyAction 인스턴스를 실제 `EnemyData` SO 에셋(`Assets/Game/1_Datas/1.Enemies/*.asset`)의 `EnemyActions` 리스트에 등록해야 그 몬스터가 실전에서 쓸 수 있다.

### Step 8 — 실행 파이프라인 (참고용, 수정 지점 아님)

이미 완성되어 있어 새 EnemyAction을 추가할 때 손댈 필요가 없다. 흐름만 참고:

```
EnemyTurnGA (EnemySystem.EnemyTurnGAPerformer)
  → enemy.Enemy.PerformAction(enemy, enemy.NextAction) 이 true면
  → ActionSystem.Instance.AddReaction(new PlayEnemyEAGA(enemy, enemy.NextAction))
  → EnemySystem.PlayEnemyEAGAPerformer 가 playEnemyEAGA.Action.PlayEnemyAction(enemy) 호출
  → 반환된 Sequence의 WaitForCompletion() 대기 (null이면 즉시 통과)
```

`PlayEnemyEAGA`(`Assets/Game/0_Scripts/2.GameActions/PlayEnemyEAGA.cs`)는 "행동을 판단하는 시점(`EnemyTurnGA`)"과 "실제로 재생하는 시점"을 GameAction으로 분리해 ActionSystem 컨벤션(직접 메서드 호출 대신 AddReaction으로 체이닝)에 맞춘 것.

## 2. 완성 코드 전문

### `FireArrowEA.cs`

```csharp
using DG.Tweening;
using UnityEngine;

public class FireArrowEA : EnemyAction, IHaveDamage, IHaveDistance
{
    public override Sprite Icon
    {
        get { return icon; }
        protected set { }
    }
    public override string Description
    {
        get { return $"사거리 {Distance}칸 내, 영웅에게 {Damage_Amount} 피해의 원거리 공격"; }
        protected set { }
    }
    public override string TextInfo
    {
        get { return Damage_Amount.ToString(); }
        protected set { }
    }

    [field: SerializeField] public float Damage_Amount { get; protected set; } = 4;
    [field: SerializeField] public int Distance { get; protected set; } = 3;

    [SerializeField] private float arrowDuration = 0.35f;
    [SerializeField] private Sprite icon;

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        HeroView target = FindRandomHeroInRange(enemy, Distance);
        if (target == null)
            return null;   //사거리 내 대상 없음 → 무시

        targetPosition = TokenSystem.Instance.API.GetTokenPosition(target);

        var curPos = TokenSystem.Instance.API.GetTokenPosition(enemy);

        Tween arrowTween = ProjectileSystem.Instance.PlayArrow(
            Utility.Vector2IntToVector3(curPos, 1),
            Utility.Vector2IntToVector3(targetPosition, 1),
            arrowDuration, Ease.Linear);

        Sequence squ = DOTween.Sequence();
        squ.Append(arrowTween);

        AttackHeroGA attackHeroGA = new(enemy, Damage_Amount, targetPosition);
        ActionSystem.Instance.AddReaction(attackHeroGA);

        return squ;
    }

    public override EnemyAction Clone()
    {
        return new FireArrowEA()
        {
            icon = icon,
            Damage_Amount = Damage_Amount,
            Distance = Distance,
            arrowDuration = arrowDuration,
        };
    }
}
```

### `ProjectileSystem.cs`

Step 6에 전문 인용됨 — 위 참고.

## 3. 한눈에 체크리스트

- [ ] `Enemies/EnemyActions/`에 `~EA` 클래스 생성, `EnemyAction` + 필요 인터페이스 상속
- [ ] `Icon`/`Description`/`TextInfo` getter 계산 프로퍼티(setter no-op)
- [ ] 밸런스 필드는 `[field: SerializeField]`, 내부 전용은 `[SerializeField] private`
- [ ] `PlayEnemyAction`: `FindRandomHeroInRange` → null 체크 → `targetPosition` 대입 → 연출 `Sequence` → 기존 `AttackHeroGA`(또는 적절한 기존 GA) `AddReaction` → `Sequence` 반환
- [ ] 투사체 필요 시 `ProjectileSystem` 재사용, **반드시 `TokenSystem.Instance.IsoWorld.transform` 아래 Instantiate**
- [ ] `Clone()` 얕은 복사 — 자기 자신의 새 인스턴스인지 재확인
- [ ] 이 행동을 쓸 `Enemy` 서브클래스의 `JudgeActAction`/`PerformAction` 작성 (이동 불필요 vs 이동 필요 패턴 중 선택)
- [ ] `EnemyData` 에셋의 `EnemyActions` 리스트에 등록 (에디터 작업)
- [ ] 실행 파이프라인(`PlayEnemyEAGA`)은 이미 완성 — 손댈 필요 없음

## 4. 알려진 미완 / 별도 처리 사항

- `FireArrowEA`는 이 문서 작성 시점 기준 **아직 어떤 `EnemyData`에도 등록되지 않았다** — 실전에서 이걸 쏘는 몬스터가 없다. 새 원거리 몬스터를 만들 때 §1 Step 7을 따라 연결할 것.
- 플레이어 쪽 원거리 공격(`AttackEnemyProcessor`)은 여전히 `ProjectileSystem`을 쓰지 않고 씬에 미리 배치한 화살 오브젝트를 `SetActive`로 토글하는 기존 방식을 그대로 쓴다. `ProjectileSystem`으로 옮기는 건 별도 마이그레이션 작업.
