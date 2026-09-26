# TokenSystem API — TokenSetup 메서드 카탈로그

> 이 문서는 `TokenSetup`(`TokenSystem.Instance.Setup`)이 제공하는 4개 초기 배치 메서드 각각의 파라미터·반환값·동작을 정리한 카탈로그다.
> - TokenSystem의 전체 구조(파사드 + 4개 하위 컴포넌트의 역할 분담)는 [`00_token_system.md`](./00_token_system.md)를 참고한다 — 여기서는 반복하지 않는다.
> - `TokenServiceAPI`(`API`)는 [`00_token_system_api.md`](./00_token_system_api.md), `TokenMainAPI`(`Main`)는 [`00_token_system_main.md`](./00_token_system_main.md)에서 다룬다.
> - 이 문서는 `TokenSetup.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 즉시 갱신한다.

## 1. 마스터 인덱스

| 메서드 | 카테고리 | 반환값 | 한 줄 설명 |
|---|---|---|---|
| [`SetUpStageMap()`](#setupstagemap) | 맵 초기화 | `void` | 스테이지 바닥 타일 생성 및 그리드 배열 초기화 |
| [`SetUPEnemys(EnemyData[], Vector2Int[])`](#setupenemys) | 몬스터 / 코어 배치 | `void` | 몬스터 토큰 일괄 생성 + 그리드 등록 |
| [`SetUpWaveCore(WaveCoreData)`](#setupwavecore) | 몬스터 / 코어 배치 | `void` | 웨이브 코어 토큰 생성 + 그리드 등록 |
| [`StartSetUpHero(HeroData[], List<Vector2Int>)`](#startsetuphero) | 영웅 배치 | `void` | 클릭 기반 영웅 배치 시작(이벤트 기반, 비동기 완료) |

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
- **비고**: `simpleGrid`/`remainCells`를 새로 할당하므로, 나머지 3개 메서드(`SetUPEnemys`/`SetUpWaveCore`/`StartSetUpHero`)보다 반드시 먼저 호출해야 한다 — 이 순서는 `TokenSetup`/`TokenGrid` 자신의 내부 로직 때문에 생기는 제약이며, 호출자가 임의로 정한 관례가 아니다.

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

### 2-3. 영웅 배치

#### `StartSetUpHero`
```csharp
public void StartSetUpHero(HeroData[] heroDatas, List<Vector2Int> heroSetupPositions)
{
    Interactions.Instance.IsSetUpHero = true;
    HeroViews.Clear();
    this.heroSetupPositions = heroSetupPositions;
    foreach (var gridPos in heroSetupPositions)
        VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), gridPos, "Hero_SetUp_True");
    setupUI.SetUp(heroDatas);
    Interactions.SetSelectGridEvent(SetHeroView, true);
}
```
- **파라미터**: `HeroData[] heroDatas`, `List<Vector2Int> heroSetupPositions`(배치 가능 좌표 목록 — 내부에서 배치될 때마다 이 리스트 자체가 줄어든다)
- **반환값**: `void` — 호출 즉시 배치가 끝나지 않는다(아래 비고 참고)
- **동작**: `HeroViews` 초기화 → 배치 가능 좌표에 시각 그리드(`"Hero_SetUp_True"`) 표시 → 배치 UI(`setupUI`) 활성화 → 그리드 클릭 이벤트(`SetHeroView`)를 등록. 클릭할 때마다 내부 `heroCount`가 줄고 0이 되면 `EndSetUpHero()`가 호출되어 이벤트를 해제하고 배치 미리보기(`HeroPreview`)를 파괴한다.
- **사용 예시**: `1.Systems/MatchSetupSystem.cs:40` — `TokenSystem.Instance.Setup.StartSetUpHero(heroDatas.ToArray(), stageData.HeroSetupPoses.ToList());`
- **비고**: 이 메서드는 완료를 동기적으로 보장하지 않는 **이벤트 기반 메서드**다 — 호출자는 `Interactions.Instance.IsSetUpHero`가 `false`가 될 때까지 배치가 진행 중인 것으로 취급해야 한다. `SetUpStageMap()` 이후 호출한다는 전제가 있다.

## 3. 문서 유지보수 규칙

이 문서는 `TokenSetup.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 1절(마스터 인덱스)과 2절(해당 카테고리 상세 블록)을 즉시 갱신한다. `TokenSetup`의 역할·계약 자체(초기 배치 전용이라는 성격)가 바뀌면 이 문서가 아니라 [`00_token_system.md`](./00_token_system.md)의 2-4절을 갱신한다.
