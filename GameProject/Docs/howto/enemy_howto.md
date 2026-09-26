# 새 Enemy 만들기 — 실전 가이드

> 이 문서는 "왜 이렇게 동작하는가"가 아니라 **"지금 새 몬스터 하나를 어떻게 만드는가"**를 순서대로 정리한 실전 체크리스트다. 아키텍처 자체(판단/실행 시차, ActionSystem 큐잉으로 이동→공격 순서를 보장하는 방식 등)는 [feature-spec/01_enemy_system.md](../feature-spec/01_enemy_system.md)를 먼저 본다. 이 문서는 `Larva`(근접 이동형)와 `ArchorSkeleton`(원거리 이동형)을 실제로 만들면서 밟은 절차를 그대로 옮긴 것이다.
>
> **범위 밖**: 완성된 `EnemyData`를 웨이브(`WaveData`)에 등록해서 실제 스테이지에 스폰시키는 절차는 이 문서에서 다루지 않는다.

## 0. 먼저 정할 것 두 가지

1. **이동하는 몬스터인가, 제자리형인가?**
   - 이동형 → §1의 "이동형" 패턴
   - 제자리형(인접/사거리 내 있으면만 공격) → §1의 "제자리형" 패턴
2. **공격 방식이 기존 `EnemyAction`으로 되는가, 새로 만들어야 하는가?**
   - 근접(사거리 1) → 기존 `AttackEA` 재사용
   - 원거리(투사체) → 기존 `FireArrowEA` 재사용, `Distance` 값만 SO 에셋에서 원하는 값으로 바꾸면 됨(코드 수정 불필요)
   - 둘 다 안 맞으면 §2 "새 EnemyAction 만들기" 참고

## 1. Enemy 서브클래스 작성 (`Assets/Game/0_Scripts/Enemies/`)

### 이동형 — `Enemy.TryMoveIntoRange` 공유 헬퍼 재사용

`Larva`/`ArchorSkeleton` 둘 다 "가장 가까운 대상 방향으로, 사거리 안까지 이동"이라는 동일한 판단 로직을 쓰기 때문에 이 로직은 `Enemy` 베이스 클래스(`Assets/Game/0_Scripts/Models/Enemy.cs`)에 `protected bool TryMoveIntoRange(EnemyView enemy, int distance)`로 이미 뽑혀 있다. 새 이동형 몬스터는 이 헬퍼에 자기 공격의 `Distance`만 넘기면 된다:

```csharp
// ArchorSkeleton.cs 전체
using System;
using UnityEngine;

public class ArchorSkeleton : Enemy
{
    public override EnemyAction JudgeActAction(EnemyView enemy)
    {
        Type type = typeof(FireArrowEA);
        var action = GetEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        return action;
    }

    public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
    {
        if (nextAction is not FireArrowEA fireArrowEA)
            return false;

        return TryMoveIntoRange(enemy, fireArrowEA.Distance);
    }

    public override Enemy Clone()
    {
        return new ArchorSkeleton();
    }
}
```

`TryMoveIntoRange`가 알아서 하는 것: 생존 영웅 중 최근접 대상 탐색(동률이면 랜덤) → 이미 사거리 안이면 이동 없이 `true` → 이동 가능 범위 중 사거리 안으로 들어오는 최단 경로 칸으로 `PerformMoveGA` 큐잉 후 `true` → 그마저 불가능하면 갈 수 있는 만큼만 전진하고 `false`. **`targetPosition`은 여기서 세팅하지 않는다** — 실제 공격 대상은 이동이 끝난 뒤 `EnemyAction.PlayEnemyAction`이 스스로 다시 정한다(§2).

### 제자리형 — `PerformAction`은 위임만

이동이 필요 없으면 재검증/타겟팅 전부 `EnemyAction`이 알아서 하므로 이게 전부다:

```csharp
// Scarecrow.cs
public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
{
    return true;
}
```

### 공통 — 잊기 쉬운 실수: `Clone()`

`Clone()`은 **반드시 자기 자신의 새 인스턴스**를 반환해야 한다. `EnemyView.SetUp`이 스폰마다 `EnemyData.Enemy.Clone()`을 호출해서 실제 AI 인스턴스를 만들기 때문에, 여기서 다른 클래스를 반환하면 SO 에셋에 뭘 넣어놨든 그 다른 클래스의 AI로 동작한다(실제로 `Larva.Clone()`이 `new Scarecrow()`를 반환하던 오탈자 때문에 "Larva가 이동을 안 하는" 버그가 났던 적이 있다).

## 2. 공격 방식(EnemyAction) 정하기

### 기존 것 재사용 (대부분 이 경우)

`AttackEA`(근접)/`FireArrowEA`(원거리) 둘 다 `EnemyAction` 베이스의 공용 헬퍼로 **스스로** 타겟을 찾는다 — `Enemy` 쪽에서 대상을 정해줄 필요가 없다:

```csharp
// EnemyAction.cs — 베이스에 있는 공용 헬퍼
protected HeroView FindRandomHeroInRange(EnemyView enemy, int distance)
{
    Vector2Int myPos = TokenSystem.Instance.API.GetTokenPosition(enemy);

    var heroesInRange = TokenSystem.Instance.HeroViews
        .Where(h => h != null && h.CurrentHealth > 0)
        .Where(h => TokenSystem.Instance.API.GetDistance(myPos, TokenSystem.Instance.API.GetTokenPosition(h)) <= distance)
        .ToList();

    if (heroesInRange.Count == 0) return null;
    return heroesInRange[UnityEngine.Random.Range(0, heroesInRange.Count)];
}
```

```csharp
// AttackEA.cs / FireArrowEA.cs — PlayEnemyAction 맨 앞
public override Sequence PlayEnemyAction(EnemyView enemy)
{
    HeroView target = FindRandomHeroInRange(enemy, Distance);
    if (target == null)
        return null;   // 사거리 내 대상 없음 → 무시

    targetPosition = TokenSystem.Instance.API.GetTokenPosition(target);
    // ... 이하 연출 + AttackHeroGA 큐잉
}
```

이 호출 시점은 **이동이 이미 끝난 뒤의 실제 위치** 기준이다(`PerformMoveGA`를 먼저 큐잉하고 그 다음 공격 GA를 큐잉하는 순서 덕분 — 자세한 건 `01_enemy_system.md` §4-1). 따라서 새 몬스터가 `AttackEA`/`FireArrowEA` 중 하나를 쓴다면 **코드를 한 줄도 안 고치고**, SO 에셋에서 `Distance`/`Damage_Amount` 값만 원하는 대로 설정하면 끝난다(`ArchorSkeleton`은 `FireArrowEA`를 그대로 쓰되 `Distance`만 기본값 3 대신 2로 설정한 사례).

### 새 공격 방식이 필요한 경우

`EnemyAction`을 상속한 새 클래스를 만들고, `PlayEnemyAction` 안에서 위와 같은 패턴(`FindRandomHeroInRange`로 자체 탐색 → 없으면 `null` 반환 → 있으면 연출 + `AttackHeroGA`류 GA 체이닝)을 그대로 따른다. `Distance`가 필요한 공격이면 `IHaveDistance` 인터페이스를 구현한다.

## 3. `EnemyData` SO 에셋 만들기 (에디터 작업)

새 프리팹은 **필요 없다** — `TokenCreator.CreateToken`이 Enemy 타입 전체에 대해 프리팹 하나(`enemyTokenPrefab`)를 공유해서 `Instantiate`하고, `EnemyView.SetUp(EnemyData)` → `Token.SetUpBaseBase`가 `Model.sprite = tokenData.Sprite`로 스프라이트를 자동 반영한다. `EnemyData` SO 에셋 하나만 있으면 된다.

1. `Assets/Game/1_Datas/1.Enemies/`에서 우클릭 → `Create > Data/Token/Enemy`
2. 파일명은 `번호.이름.asset` 패턴 (예: `1.허수아비.asset`, `2.Larva.asset`, `3.ArchorSkeleton.asset` — 다음 번호 이어서 사용)
3. 인스펙터에서 채울 필드:
   - `Name`, `Sprite`, `HRange` — `TokenData`에서 상속
   - `Health`, `MovePoint` — `EnemyData` 고유 필드
   - `Enemy` — 방금 만든 `Enemy` 서브클래스를 SerializeReference 드롭다운에서 선택 (스크립트 컴파일 후에 목록에 뜸)
   - `EnemyActions` — 리스트에 원하는 `EnemyAction` 서브클래스 추가, `Distance`/`Damage_Amount`/`icon` 등 값 입력

## 4. Unity MCP로 이 과정을 자동화할 때 주의점 (이번에 새로 확인한 함정)

Claude가 Unity MCP로 위 절차를 직접 대신 실행해줄 수도 있는데, 그 경우 아래를 알아둬야 시행착오를 줄인다.

- **`manage_scriptable_object`의 `patches`는 `SerializeReference` 필드(`Enemy`, `EnemyActions`)를 지원하지 않는다.** 실제로 시도하면 이런 에러가 난다:
  > `Unsupported SerializedPropertyType: ManagedReference. This type cannot be set via MCP patches. Consider editing the .asset file directly or using Unity's Inspector.`

  `Name`/`Sprite`/`Health`/`MovePoint`처럼 일반 필드(`Integer`/`String`/`ObjectReference`)는 `patches`로 잘 된다 — `Enemy`/`EnemyActions`만 별도 방법이 필요하다.

- **대안: `execute_code`로 `SerializedProperty.managedReferenceValue`를 직접 호출한다.** 실제로 성공한 코드:

  ```csharp
  string path = "Assets/Game/1_Datas/1.Enemies/3.ArchorSkeleton.asset";
  var data = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>(path);
  if (data == null) return "LOAD_FAILED";

  // 주의: 프로젝트 타입(ArchorSkeleton, FireArrowEA 등)을 코드에 직접 쓰면
  // codedom 컴파일러가 어셈블리 참조를 못 찾아 컴파일 에러가 난다.
  // 리플렉션으로 타입을 찾아서 우회한다.
  System.Type enemyType = null;
  System.Type actionType = null;
  var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
  for (int i = 0; i < assemblies.Length; i++)
  {
      if (enemyType == null) enemyType = assemblies[i].GetType("ArchorSkeleton");
      if (actionType == null) actionType = assemblies[i].GetType("FireArrowEA");
  }
  if (enemyType == null) return "ENEMY_TYPE_NOT_FOUND";
  if (actionType == null) return "ACTION_TYPE_NOT_FOUND";

  var so = new UnityEditor.SerializedObject(data);
  so.Update();

  var enemyProp = so.FindProperty("<Enemy>k__BackingField");
  enemyProp.managedReferenceValue = System.Activator.CreateInstance(enemyType);

  var actionsProp = so.FindProperty("<EnemyActions>k__BackingField");
  actionsProp.ClearArray();
  actionsProp.InsertArrayElementAtIndex(0);
  var elem = actionsProp.GetArrayElementAtIndex(0);
  elem.managedReferenceValue = System.Activator.CreateInstance(actionType);

  so.ApplyModifiedPropertiesWithoutUndo();

  // managedReferenceValue를 적용한 뒤 so.Update()를 다시 호출해야
  // 그 인스턴스 내부 필드(FindPropertyRelative)에 접근할 수 있다.
  so.Update();
  var distProp = so.FindProperty("<EnemyActions>k__BackingField")
      .GetArrayElementAtIndex(0)
      .FindPropertyRelative("<Distance>k__BackingField");
  distProp.intValue = 2;
  so.ApplyModifiedPropertiesWithoutUndo();

  UnityEditor.EditorUtility.SetDirty(data);
  UnityEditor.AssetDatabase.SaveAssets();
  UnityEditor.AssetDatabase.Refresh();

  return "OK";
  ```

- **방금 만든 스크립트 파일이 아직 Unity 도메인에 안 실렸을 수 있다.** `Assembly.GetType`으로 새 클래스가 안 찾아지면(스크립트를 만든 직후, 혹은 Unity MCP 연결이 끊겼다 다시 붙은 직후 흔함), 먼저 `refresh_unity(scope: "all", mode: "force", compile: "request", wait_for_ready: true)`를 호출해서 강제로 재컴파일시키고, `mcpforunity://editor/state`에서 `is_compiling: false`/`ready_for_tools: true`가 될 때까지 기다린 뒤 다시 시도한다.
- **`execute_code`의 로컬 함수는 컴파일 에러가 난다.** 기본 컴파일러가 codedom(C# 6)이라 `System.Type Foo(string name) { ... }` 같은 로컬 함수 선언이 안 먹는다 — 로컬 함수 없이 순차적인 코드로 풀어 쓴다.

## 5. 검증

- Console 에러/경고 0건 확인 (`read_console`).
- 플레이 모드에서: 이동형이면 이동 후 공격이 정상 순서로 재생되는지, 제자리형이면 사거리 판정이 맞는지, 여러 후보가 있을 때 랜덤 분산이 되는지 확인.
