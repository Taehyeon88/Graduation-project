# 08. 지형 — 중립지대 · 파괴 가능 장애물 · 발판

> 4×4 그리드 위에는 영웅·몬스터 외에 **중립지대**(이동·배치 불가), **파괴 가능 장애물**, **발판**이 놓인다(원작 Master of Piece 기준). 세 종류 모두 `StageData`에 위치(+데이터)를 저장하고 `MatchSetupSystem`이 전투 시작 때 읽어 생성한다.
> 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> 에셋 단위 정리(Id·수치·연결 흐름·한계)는 [terrain_catalog.md](Terrain/terrain_catalog.md)를 본다.

## 1. 한눈에 보기

| 종류 | 토큰 여부 | 이동 | 공격 대상 | 데이터 | 생성 경로 |
|---|---|---|---|---|---|
| 중립지대 | 아니오 (`simpleGrid`만 막음) | 불가 (배치도 불가) | 아니오 | 없음 — `StageData.NeutralPositions`(위치만) | `TokenSetup.SetUpNeutralZones` → `TokenGrid.SetBlockedCell` |
| 파괴 가능 장애물 | **예** (`ObstacleView : CombatantView`) | 불가 (1셀 1토큰) | 영웅·적 모두(`IsTarget`). 플레이어 스킬(`SharpDagger`)은 불가 | `ObstacleData : CombatantData` | `TokenSetup.SetUpObstacles` → `TokenMainAPI.AddToken(TokenType.Obstacle)` |
| 발판 | 아니오 (별도 레이어) | 가능 | 아니오 | `TerrainData` (+ 기능 `TerrainEffect`) | `TokenSetup.SetUpTerrains` → `TokenCreator.CreateTerrain` |

생성 순서(`MatchSetupSystem.StartSetting`): 맵 타일 → 중립지대 → 장애물 → 발판 → 배치 위치 계산(`CalcSetupPoses`) → 몬스터/웨이브.

## 2. 데이터 (`StageData`)

| 필드 | 형태 | 비고 |
|---|---|---|
| `NeutralPositions` | `Vector2Int[]` | 구 `ObstaclePositions`(`FormerlySerializedAs`로 직렬화 값 유지). 기본 배치 칸에 있으면 그 라인의 배치 위치가 한 칸 앞으로 이동(`CalcSetupPoses`) |
| `ObstacleSpawns` | `ObstacleSpawn[]` (`Position`, `ObstacleData Data`) | 점유 칸·범위 밖·데이터 없음이면 `LogError` 후 건너뜀 |
| `TerrainSpawns` | `TerrainSpawn[]` (`Position`, `TerrainData Data`) | 중립지대·장애물·이미 있는 발판 위는 `LogError` 후 건너뜀 |

## 3. 중립지대

- `TokenGrid.SetBlockedCell(pos)`: `simpleGrid[pos]=1`, `remainCells`에서 제거, 바닥 타일(`floorTiles`)을 비활성화. 토큰은 등록하지 않는다.
- 막힘 판정은 기존 `simpleGrid` 기반 호출부(`CanSet`/`IsGridEmpty`/BFS/넉백/웨이브 스폰/카드 배치)가 그대로 처리한다.
- **주의**: `CanSetExceptionToken`은 `simpleGrid==1`인데 토큰이 없으면 false를 반환한다(토큰 없이 막힌 칸). 이 분기가 없으면 "적 예외" 옵션 호출부(`RangeMode` 등)가 중립 칸을 빈 칸으로 오판한다.

## 4. 파괴 가능 장애물 — 돌격 화살통(0/4)

> 이 장애물을 파괴한 용병이 모든 적 용병을 추가로 공격합니다.

- **구성**: `ObstacleView`(`Forward`가 zero라 `Battle()`이 즉시 끝남 → 스스로 행동하지 않음) + `ObstacleData`(체력·공격력·속도·특성). 에셋 `1_Datas/11.Obstacles/돌격 화살통.asset`(체력 4, 공격력 0, 속도 0). 프리팹 `3_Prefabs/Views/ObstacleView.prefab`. `CombatantView.BaseDamage`는 최소 1로 보정되지만 공격하지 않으므로 영향 없음.
- **공격 대상화**: `HeroView.IsTarget`/`EnemyView.IsTarget`에 `ObstacleView`를 포함. 기존 `ReserveAttack`이 정면 토큰이 장애물이면 `DealDamageGA`를 예약한다. 즉 장애물에 막힌 영웅·적은 정면 장애물을 공격한다.
- **플레이어 스킬 제외**: `SharpDagger.IsValidTarget`은 `EnemyView`/`EnemyFlagView`만 허용하는 **허용 목록** 방식이라 변경 없이 장애물을 지정할 수 없다. 이 방식을 유지할 것.
- **파괴**: 기존 `CombatantView.Damage → KillGA → DamageSystem.KillGAPerformer → TokenMainAPI.RemoveToken`. 영웅·적 목록에는 들어가지 않는다(`AddToken`의 `EnemyView`/`HeroView` 분기에 걸리지 않음).
- **처치 효과**:
  - `KillGA.Killer`(`Token`, 기본 null) — `CombatantView.Damage`가 `dealDamageGA.Caster`를 넘긴다.
  - 특성 `9001 돌격 화살통` = `SelfDeathCondition`(PRE) + `KillerAttackAllOpponentsEffect`. 처치자가 영웅이면 `EnemyViews` 전체, 적이면 `HeroViews` 전체에 `DealDamageGA(killer.BaseDamage, targets, killer)`를 예약한다. 처치자가 없으면(스킬 등) 아무 일도 하지 않고 코어는 공격하지 않는다.
  - PRE인 이유: `KillGA` performer가 토큰을 파괴하면 `OnDisable`이 구독을 해제해 POST에서는 놓칠 수 있다.
- **(원작 확인 필요)**: 추가 공격 피해량(처치자 공격력으로 가정), 적이 파괴했을 때도 같은 효과인지(현재는 "영웅·적 모두"로 구현), 추가 공격이 코어도 포함하는지(현재 미포함).

## 5. 발판 — 풀숲

> 이 타일 위의 용병은 회피를 얻습니다. (타일 위의 용병이 공격을 받으면 이 지형지물이 제거되며, 2라운드 후 다시 생성됩니다 — **미구현**, 후속)

- **구성**: `TerrainData`(`Id`, `Name`, `Sprite`, `Effect`), `TerrainEffect`(추상 기능, `Models/`)와 구현체 `BushEffect`(`Terrains/`), `TerrainView`(바닥 타일 복사 프리팹 `3_Prefabs/Views/TerrainView.prefab`, iso z 0.01, sortingOrder -9), `TerrainSystem`(`Singleton<T>`, `--- SYSTEMS ---` 아래, 리액션 전용). 에셋 `1_Datas/12.Terrains/풀숲.asset`(`Effect`=`BushEffect`, 임시 스프라이트).
- **생성**: `MatchSetupSystem` → `TokenSetup.SetUpTerrains(stageData.TerrainSpawns)` → `TokenCreator.CreateTerrain`(`TokenCreator.terrainPrefab`, 부모는 `isoWorld`). 데이터가 없거나 점유(`grid.CanSet` false)·중복이면 `LogError` 후 건너뛴다.
- **토큰이 아니다**: `grid.SetToken`을 쓰면 칸을 점유해 이동이 막히므로, `TokenSetup`이 `Dictionary<Vector2Int, TerrainView>`로 따로 보관한다(`Terrains`, `TryGetTerrain`). `simpleGrid`는 건드리지 않아 이동은 가능하고, 토큰이 아니라서 공격 대상이 되지 않는다.
- **기능 구조**: `TerrainData.Effect`(`[SerializeReference, SR]`)를 `TerrainView.SetUp`이 `Clone()`해 발판별 사본으로 보유한다. `TerrainView.Enter(unit)`/`Exit(unit)`이 `TerrainEffect.OnEnter`/`OnExit`에 위임한다. 새 발판 기능은 `TerrainEffect` 서브클래스를 만들어 SO에 지정한다.
- **발동 경로** (`TerrainSystem`, 모두 POST):
  - `MoveGA`: 모든 발판에 `Exit(mover)`(각 발판이 자기가 준 것만 회수), 도착 칸이 발판이면 `Enter(mover)`.
  - `PlayCardGA`: 소환된 칸이 발판이고 그 칸 토큰이 `CombatantView`이면 `Enter(unit)`.
- **`BushEffect`**: `OnEnter` → 발판별 `granted` 목록에 기록 + `AddStatusEffectGA(DODGE, dodgeStack=1)`, `OnExit` → 목록에 있으면 `RemoveStatusEffect(DODGE, dodgeStack)`.
- **한계**: 웨이브로 스폰되는 적이 풀숲 칸에 나오면 회피를 받지 않는다(`PlayCardGA`/`MoveGA` 경로가 아님). 회피는 상태효과 스택이라 풀숲 회피를 이미 소모한 뒤 떠날 때 다른 출처 회피가 있으면 그것이 1 회수된다(스택에 출처 구분 없음).

## 6. 후속 과제

- 풀숲: 위 유닛이 공격받으면 제거 + 2라운드 후 재생성(`TurnGA(Player)` PRE 카운트).
- 웨이브 스폰 시 풀숲 회피 부여.
- 원작 확인 필요 항목(§4) 확정.
