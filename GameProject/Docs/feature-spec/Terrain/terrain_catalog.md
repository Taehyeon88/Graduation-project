# 지형 카탈로그 — 장애물 · 발판

> 스테이지에 놓이는 **파괴 가능 장애물**과 **발판**을 에셋 단위로 정리한 문서. 아키텍처·생성 순서·중립지대는 [08_terrain_obstacle.md](../08_terrain_obstacle.md)를 본다. 새 장애물·발판을 만들면 이 문서에 섹션을 추가한다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> **현재 수록: 장애물 1개** (돌격 화살통), **발판 1개** (풀숲). 중립지대는 위치만 가지는 데이터 없는 지형이라 이 카탈로그의 항목이 아니다 (08 §3).

## 구조

| 구성 | 장애물 | 발판 |
|---|---|---|
| SO | `5.Data/ObstacleData.cs` (`CombatantData` 상속: `Id`, `Sprite`, `Health`, `Damage`, `Speed`, `Traits` + `Description`) | `5.Data/TerrainData.cs` (`Id`, `Description`, `Sprite`, `Effect`(`TerrainEffect`, `[SerializeReference, SR]`), 이름은 SO 에셋 이름) |
| 기능 | 특성 부품 (`TraitCondition`/`TraitEffect`) | `Models/TerrainEffect.cs` (추상: `OnEnter`/`OnExit`/`Clone`) → `Terrains/` 구현체 |
| 배치 데이터 | `5.Data/ObstacleSpawn.cs` (`Position`, `Data`) → `StageData.ObstacleSpawns` | `5.Data/TerrainSpawn.cs` (`Position`, `Data`) → `StageData.TerrainSpawns` |
| 뷰 | `3.Views/ObstacleView.cs` (`CombatantView` 상속) | `3.Views/TerrainView.cs` |
| 생성 | `TokenSetup.SetUpObstacles` → `TokenMainAPI.AddToken(TokenType.Obstacle)` → `TokenCreator` | `TokenSetup.SetUpTerrains` → `TokenCreator.CreateTerrain` (`Instantiate`, 딕셔너리 보관) |
| 토큰 여부 | 예 (칸을 점유, 공격 대상) | 아니오 (`simpleGrid` 미점유, 이동 가능, 공격 대상 아님) |
| 에셋 위치 | `Assets/Game/1_Datas/11.Obstacles/` | `Assets/Game/1_Datas/12.Terrains/` |
| 프리팹 | `3_Prefabs/Views/ObstacleView.prefab` (`TokenCreator.obstaclePrefab`) | `3_Prefabs/Views/TerrainView.prefab` (`TokenCreator.terrainPrefab`) |

- 생성 순서(`MatchSetupSystem.StartSetting`): 맵 타일 → 중립지대 → **장애물** → **발판** → 배치 위치 계산(`CalcSetupPoses`) → 몬스터/웨이브.
- 장애물/발판 모두 점유·범위 밖·데이터 없음이면 `LogError` 후 건너뛴다.
- 씬에는 `TerrainSystem` GameObject(리액션 전용, 인스펙터 연결 없음)가 `--- SYSTEMS ---` 아래에 1개 있어야 하고, `TokenCreator.terrainPrefab`에 `TerrainView.prefab`이 연결돼야 한다.

## 요약

### 장애물

| Id | 이름 | 설명 | 체력/공격력/속도 | 특성 | 에셋 |
|---|---|---|---|---|---|
| 3001 | 돌격 화살통 | 이 장애물을 파괴한 용병이 모든 적 용병을 추가로 공격합니다. | 4 / 0 / 0 | 9001 돌격 화살통 | `돌격 화살통.asset` |

### 발판

| Id | 이름 | 설명 | 효과 | 에셋 |
|---|---|---|---|---|
| 1 | 풀숲 | 이 타일 위의 용병은 회피를 얻습니다. | 회피 +1 (`StatusEffectType.DODGE`) | `풀숲.asset` |

> 설명은 `ObstacleData.Description` / `TerrainData.Description`(`[TextArea]`)에 저장한다. 돌격 화살통은 특성 9001의 `Description`과 같은 문구를 쓴다.

---

# A. 장애물

## 3001 · 돌격 화살통

에셋: `Assets/Game/1_Datas/11.Obstacles/돌격 화살통.asset`

| 항목 | 값 |
|---|---|
| Id | 3001 |
| 설명 | 이 장애물을 파괴한 용병이 모든 적 용병을 추가로 공격합니다. (`ObstacleData.Description`, 특성 9001과 동일 문구) |
| 체력 / 공격력 / 속도 | 4 / 0 / 0 |
| 특성 | `Traits[0]` = `3.Traits/돌격 화살통.asset` (Id 9001) |
| 조건 | `SelfDeathCondition` — 소유자 사망 시 통과 (`Traits/Conditions/SelfDeathCondition.cs`) |
| 타이밍 | PRE (`reactionTiming: 0`) — `KillGA` Performer가 토큰을 파괴하면 `OnDisable`이 구독을 해제해 POST에서는 놓칠 수 있다 |
| 효과 | `KillerAttackAllOpponentsEffect` — 처치자 진영의 상대 전원에 `DealDamageGA(killer.BaseDamage, targets, killer)` 예약 |
| 스프라이트 | `Sprite` 지정됨 (임시 에셋) |
| 배치 예시 | `TestStage.ObstacleSpawns` — (1, 1) |
| 공격 대상 | 영웅·적 모두 (`HeroView.IsTarget`/`EnemyView.IsTarget`에 `ObstacleView` 포함) |
| 행동 | 스스로 행동하지 않음 (`ObstacleView`의 `Forward`가 zero라 `Battle()`이 즉시 끝남) |
| 플레이어 스킬 | 지정 불가 — `SharpDagger.IsValidTarget`이 `EnemyView`/`EnemyFlagView`만 허용하는 허용 목록 방식 |
| 코드 | `3.Views/ObstacleView.cs`, `5.Data/ObstacleData.cs`, `5.Data/ObstacleSpawn.cs`, `Traits/KillerAttackAllOpponentsEffect.cs`, `2.GameActions/KillGA.cs`, `3.Views/CombatantView.cs`, `SubSystems/TokenSetup.cs`, `4.Creators/TokenCreator.cs` |

**연결 흐름**

```
영웅/적이 정면 칸에 장애물이 있을 때 CombatantView.ReserveAttack
 └ DealDamageGA(Caster = 영웅 또는 적, 대상 = ObstacleView)
     └ CombatantView.Damage: 체력 ≤ 0
         └ KillGA(token = 장애물, Killer = dealDamageGA.Caster) 를 PostReactions에 예약
             ├ PRE 구독자 → Trait.Reaction (장애물의 특성 9001)
             │   ├ SelfDeathCondition: 죽는 토큰이 소유자(장애물)인가
             │   └ KillerAttackAllOpponentsEffect
             │       ├ killer가 HeroView → EnemyViews 전체
             │       ├ killer가 EnemyView → HeroViews 전체
             │       └ DealDamageGA(killer.BaseDamage, targets, killer) 예약 (대상이 없거나 Killer가 null/비 Combatant면 아무 일도 안 함)
             └ DamageSystem.KillGAPerformer → TokenMainAPI.RemoveToken
```

**주의·한계**
- **계약 변경**: `KillGA`에 `Killer`(`Token`, 기본 null)를 추가했다. 생성자 `KillGA(token, hit_Tween, killer = null)`의 세 번째 인자는 선택이라 기존 호출부는 그대로 유효하다.
- 처치자가 없는 경우(플레이어 스킬 등)에는 효과가 아무 일도 하지 않으며, 코어는 공격하지 않는다.
- 추가 피해량은 처치자의 `BaseDamage`(공격력, 최소 1 보정)로 가정했다 **(원작 확인 필요)**.
- 적이 파괴했을 때도 같은 효과(영웅 전원 추가 공격)가 나가도록 구현했다 **(원작 확인 필요)**.
- 추가 공격에 코어(깃발)는 포함하지 않았다 **(원작 확인 필요)**.
- 장애물의 공격력이 0이어도 `CombatantView.BaseDamage`가 최소 1로 보정되지만, 공격하지 않으므로 영향 없다.
- `AddToken`의 `EnemyView`/`HeroView` 분기에 걸리지 않아 `EnemyViews`/`HeroViews` 목록에는 들어가지 않는다 — 추가 공격 대상에도 포함되지 않는다.
- 에셋 특성의 `Reaction Timing`이 POST면 효과가 나가지 않을 수 있다. 반드시 PRE(0)로 둔다.
- **Play 테스트는 하지 않았다.** 확인 항목: 영웅/적이 정면 장애물을 공격하는지, 파괴 시 처치자가 상대 전원을 추가 공격하는지, 날카로운 단도로 지정되지 않는지, 장애물이 스스로 움직이거나 공격하지 않는지.

**에디터 설정**: `ObstacleData` 에셋 `Id`=3001, `Health`=4, `Damage`=0, `Speed`=0, `Traits`에 특성 9001 등록. `TokenCreator.obstaclePrefab`=`ObstacleView.prefab` 연결 완료. `StageData.ObstacleSpawns`에 (위치, 데이터) 입력 (`TestStage`: (1, 1)).

---

# B. 발판

## 1 · 풀숲

에셋: `Assets/Game/1_Datas/12.Terrains/풀숲.asset`

| 항목 | 값 |
|---|---|
| Id | 1 |
| 이름 | 풀숲 (SO 에셋 이름, `TerrainData.Name`) |
| 설명 | 이 타일 위의 용병은 회피를 얻습니다. (`TerrainData.Description`) |
| 기능 | `Effect` = `BushEffect` — 올라서면 `AddStatusEffectGA(DODGE, dodgeStack)`, 떠나면 회수 |
| 수치 | `dodgeStack` = 1 (`[SerializeField]`) |
| 스프라이트 | `Sprite` 지정됨 (임시 에셋) |
| 배치 예시 | `TestStage.TerrainSpawns` — (2, 2) |
| 보관 | `TokenSetup`의 `Dictionary<Vector2Int, TerrainView>` (토큰이 아니므로 `grid.SetToken`을 쓰지 않음) |
| 타일 뷰 | 바닥 타일 복사 프리팹, iso z 0.01, sortingOrder -9 (높이는 프리팹 값 유지) |
| 회피 추적 | `BushEffect.granted` (`HashSet<CombatantView>`) — 발판별 사본이 자기가 회피를 준 유닛을 보유 (`Clone()`에서 새 집합 생성) |
| 코드 | `Models/TerrainEffect.cs`, `Terrains/BushEffect.cs`, `3.Views/TerrainView.cs`, `1.Systems/TerrainSystem.cs`, `SubSystems/TokenSetup.cs`, `4.Creators/TokenCreator.cs`, `5.Data/TerrainData.cs`, `5.Data/TerrainSpawn.cs`, `UI/StatusEffectUI.cs` (아이콘 표시) |

**연결 흐름**

```
MatchSetupSystem → TokenSetup.SetUpTerrains → TokenCreator.CreateTerrain → TerrainView.SetUp(data) : data.Effect.Clone() 보관

MoveGA (POST) → TerrainSystem.MoveGAPostReaction
 ├ 모든 TerrainView.Exit(mover) → BushEffect.OnExit : 자기 granted에 있으면 RemoveStatusEffect(DODGE) 후 제거
 └ movePosition이 발판 칸 → TerrainView.Enter(mover) → BushEffect.OnEnter

PlayCardGA (POST) → TerrainSystem.PlayCardGAPostReaction
 └ TargetPos가 발판 칸이고 그 칸의 토큰이 CombatantView → TerrainView.Enter(unit) → BushEffect.OnEnter

BushEffect.OnEnter(unit)
 ├ granted에서 파괴된 유닛 정리 → unit 추가
 └ ActionSystem.AddReaction(AddStatusEffectGA(DODGE, dodgeStack, [unit], unit))
```

**주의·한계**
- **미구현**: "타일 위의 용병이 공격을 받으면 이 지형지물이 제거되며, 2라운드 후 다시 생성된다" (`TurnGA(Player)` PRE 카운트로 구현 예정, 08 §6).
- 웨이브로 스폰되는 적이 풀숲 칸에 나오면 회피를 받지 않는다 (`PlayCardGA`/`MoveGA` 경로가 아님).
- 회피 스택에 출처 구분이 없다. 풀숲 회피를 이미 소모한 뒤 떠날 때 다른 출처의 회피가 있으면 그것이 1 회수된다 (회수 여부 자체는 발판별 `granted`로 판단).
- `TerrainData.Effect`가 비어 있으면 발판은 표시만 되고 아무 효과도 없다 (`TerrainView.Enter/Exit`가 null 무시).
- 토큰이 아니라서 공격 대상이 되지 않고, `simpleGrid`를 건드리지 않아 이동은 가능하다.
- 발판은 중립지대·장애물·이미 있는 발판 위에는 놓을 수 없다 (`grid.CanSet` + 중복 검사, `TokenSetup.SetUpTerrains`).
- `TerrainSystem`은 `OnEnable`/`OnDisable`에서 `MoveGA`·`PlayCardGA` POST 구독을 대칭으로 등록·해제한다.
- **수정된 오류 (아이콘)**: 풀숲에 들어간 영웅에게 회피 대신 힘 아이콘이 표시됐다. 부여 자체는 정상(`DODGE`, 스프라이트 `회피`)이었고, 영웅이 쓰는 `StatusEffectUIAtWorld.prefab`의 `Image`에 힘 아이콘이 기본 스프라이트로 박혀 있는데 `StatusEffectUI.Set`이 전달받은 스프라이트보다 기존 값을 우선해서 생긴 문제였다. `Set`을 `if (sprite != null) image.sprite = sprite;`로 바꿔 전달된 스프라이트를 우선하도록 고쳤다 (스택 감소 시 `sprite`가 null로 오면 기존 아이콘 유지). 이 수정은 풀숲뿐 아니라 모든 상태효과 아이콘에 적용된다. 프리팹의 기본 스프라이트를 비우는 것은 선택 사항이다.
- **Play 테스트는 일부만 했다.** 소환 시 `DODGE`·`회피` 스프라이트 부여는 로그로 확인했다. 아이콘 수정 후 확인 항목: 풀숲에 소환하거나 이동해 들어가면 회피 아이콘 +1, 나가면 -1, 풀숲 타일이 바닥 위에 올바르게 정렬되어 보이는지.

**에디터 설정**: `TerrainData` 에셋 `Id`=1, `Sprite` 지정, `Effect`=`BushEffect`(`dodgeStack`=1). `TokenCreator.terrainPrefab`=`TerrainView.prefab` 연결 필요(미확인 — 사용자 작업). `StageData.TerrainSpawns`에 (위치, 데이터) 입력 (`TestStage`: (2, 2)).
