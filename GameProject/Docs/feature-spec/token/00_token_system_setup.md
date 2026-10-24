# TokenSystem API — TokenSetup 메서드 카탈로그

> 이 문서는 `TokenSetup`(`TokenSystem.Instance.Setup`)이 제공하는 초기 배치 메서드 각각의 파라미터·반환값·동작을 정리한 카탈로그다.
> - TokenSystem의 전체 구조(파사드 + 4개 하위 컴포넌트의 역할 분담)는 [`00_token_system.md`](./00_token_system.md)를 참고한다 — 여기서는 반복하지 않는다.
> - `TokenServiceAPI`(`API`)는 [`00_token_system_api.md`](./00_token_system_api.md), `TokenMainAPI`(`Main`)는 [`00_token_system_main.md`](./00_token_system_main.md)에서 다룬다.
> - 이 문서는 `TokenSetup.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 즉시 갱신한다.

## 1. 마스터 인덱스

| 메서드 | 카테고리 | 반환값 | 한 줄 설명 |
|---|---|---|---|
| [`SetUpStageMap()`](#setupstagemap) | 맵 초기화 | `void` | 스테이지 바닥 타일 생성 및 그리드 배열 초기화 |
| [`SetUpNeutralZones(Vector2Int[])`](#setupneutralzones) | 지형 | `void` | 중립지대 칸을 막고 바닥 타일을 숨김 |
| [`SetUpObstacles(ObstacleSpawn[])`](#setupobstacles) | 지형 | `void` | 파괴 가능 장애물 토큰 생성 + 그리드 등록 |
| [`SetUPEnemys(EnemyData[], Vector2Int[])`](#setupenemys) | 몬스터 / 코어 배치 | `void` | 몬스터 토큰 일괄 생성 + 그리드 등록 |
| [`SetUpWaveCore(WaveCoreData)`](#setupwavecore) | 몬스터 / 코어 배치 | `void` | 웨이브 코어 토큰 생성 + 그리드 등록 |

## 2. 카테고리별 상세

### 2-1. 맵 초기화

#### `SetUpStageMap`
```csharp
public void SetUpStageMap()
{
    grid.GenerateStage();
}
```
- **파라미터**: 없음
- **반환값**: `void`
- **동작**: `grid.GenerateStage()`에 위임 — 스테이지 바닥 타일 풀을 활성화/배치하고, `simpleGrid`/`remainCells`를 새로 할당해 초기화한다.
- **사용 예시**: `1.Systems/MatchSetupSystem.cs:25` — `TokenSystem.Instance.Setup.SetUpStageMap();`
- **비고**: `simpleGrid`/`remainCells`를 새로 할당하므로, 나머지 배치 메서드(`SetUpNeutralZones`/`SetUpObstacles`/`SetUPEnemys`/`SetUpWaveCore`)보다 반드시 먼저 호출해야 한다 — 이 순서는 `TokenSetup`/`TokenGrid` 자신의 내부 로직 때문에 생기는 제약이며, 호출자가 임의로 정한 관례가 아니다.

### 2-1b. 지형 (상세: [`08_terrain_obstacle.md`](../08_terrain_obstacle.md))

#### `SetUpNeutralZones`
```csharp
public void SetUpNeutralZones(Vector2Int[] neutrals)
{
    if (neutrals == null) return;
    foreach (Vector2Int pos in neutrals)
        grid.SetBlockedCell(pos);
}
```
- **파라미터**: `Vector2Int[] neutrals` (`StageData.NeutralPositions`, null 허용)
- **동작**: 각 칸을 `TokenGrid.SetBlockedCell`로 막는다 — `simpleGrid`=1, `remainCells`에서 제거, 바닥 타일 비활성화. 토큰은 등록하지 않는다.
- **사용 예시**: `1.Systems/MatchSetupSystem.cs` — `SetUpStageMap()` 직후
- **비고**: `CalcSetupPoses`(배치 위치 계산) 전에 호출한다.

#### `SetUpObstacles`
```csharp
public void SetUpObstacles(ObstacleSpawn[] spawns)
```
- **파라미터**: `ObstacleSpawn[] spawns` (`StageData.ObstacleSpawns`, null 허용)
- **동작**: 각 항목에 대해 데이터가 없거나 `grid.CanSet(Position)`이 false면 `LogError` 후 건너뛰고, 아니면 `TokenMainAPI.AddToken(Data, TokenType.Obstacle, Position)`로 생성·등록한다.
- **사용 예시**: `1.Systems/MatchSetupSystem.cs` — `SetUpNeutralZones` 직후, 몬스터 배치 전
- **비고**: 발판(풀숲)은 토큰이 아니라 아래 `SetUpTerrains`가 따로 생성한다.

#### `SetUpTerrains`
```csharp
public void SetUpTerrains(TerrainSpawn[] spawns)
public bool TryGetTerrain(Vector2Int pos, out TerrainView terrain)
public IEnumerable<TerrainView> Terrains { get; }
```
- **파라미터**: `TerrainSpawn[] spawns` (`StageData.TerrainSpawns`, null 허용)
- **동작**: 각 항목에 대해 데이터가 없거나 이미 발판이 있거나 `grid.CanSet(Position)`이 false면 `LogError` 후 건너뛰고, 아니면 `TokenCreator.CreateTerrain`으로 `TerrainView`를 만들어 위치별 딕셔너리에 보관한다. 토큰이 아니라서 `grid.SetToken`을 쓰지 않는다.
- **사용 예시**: `1.Systems/MatchSetupSystem.cs` — `SetUpObstacles` 직후, 배치 위치 계산 전. 조회(`TryGetTerrain`, `Terrains`)는 `1.Systems/TerrainSystem.cs`가 이동·소환 리액션에서 쓴다.
- **비고**: 조회 메서드가 `TokenServiceAPI`가 아니라 `TokenSetup`에 있다.

### 2-2. 몬스터 / 코어 배치

#### `SetUPEnemys`
```csharp
public void SetUPEnemys(EnemyData[] enemyDatas, Vector2Int[] setupPositions)
{
    EnemyViews.Clear();
    int index = 0;
    foreach (var enemyData in enemyDatas)
    {
        Token token = TokenCreator.Instance.CreateToken(enemyData, TokenType.Enemy, transform.position);
        token.TokenTransform.position = Utility.Vector2IntToIsoVector(setupPositions[index]);
        EnemyViews.Add(token as EnemyView);
        grid.SetToken(token, setupPositions[index]);
        index++;
    }
}
```
- **파라미터**: `EnemyData[] enemyDatas`, `Vector2Int[] setupPositions`(인덱스로 1:1 매칭)
- **반환값**: `void`
- **동작**: `EnemyViews`를 초기화(`Clear`)한 뒤, `enemyDatas`마다 `TokenCreator`로 토큰을 생성 → 대응하는 `setupPositions`로 시각적 배치 → `EnemyViews`에 추가 → `grid.SetToken`으로 그리드 등록
- **사용 예시**: `1.Systems/WaveSystem.cs:34` — `TokenSystem.Instance.Setup.SetUPEnemys(targets, positions);`
- **비고**: `enemyDatas.Length`와 `setupPositions.Length`가 다르면 인덱스 범위 예외가 발생할 수 있다(길이 검증 코드 없음). `SetUpStageMap()` 이후 호출한다는 전제가 있다(그래야 `grid.SetToken`이 유효한 그리드에 등록된다).

#### `SetUpWaveCore`
```csharp
public void SetUpWaveCore(WaveCoreData coreData)
{
    Token token = TokenCreator.Instance.CreateToken(coreData, TokenType.WaveCore, transform.position);
    token.TokenTransform.position = Utility.Vector2IntToIsoVector(coreData.CorePosition);
    grid.SetToken(token, coreData.CorePosition);
}
```
- **파라미터**: `WaveCoreData coreData`
- **반환값**: `void`
- **동작**: 웨이브 코어 토큰을 생성해 `coreData.CorePosition`으로 배치하고 `grid.SetToken`으로 그리드 등록
- **사용 예시**: `1.Systems/WaveSystem.cs:35` — `TokenSystem.Instance.Setup.SetUpWaveCore(waveCoreData);`
- **비고**: `SetUpStageMap()` 이후 호출한다는 전제가 있다.

## 3. 문서 유지보수 규칙

이 문서는 `TokenSetup.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 1절(마스터 인덱스)과 2절(해당 카테고리 상세 블록)을 즉시 갱신한다. `TokenSetup`의 역할·계약 자체(초기 배치 전용이라는 성격)가 바뀌면 이 문서가 아니라 [`00_token_system.md`](./00_token_system.md)의 2-4절을 갱신한다.
