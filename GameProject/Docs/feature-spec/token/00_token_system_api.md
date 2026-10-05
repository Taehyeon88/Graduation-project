# TokenSystem API — TokenServiceAPI 메서드 카탈로그

> 이 문서는 `TokenServiceAPI`(`TokenSystem.Instance.API`)가 제공하는 16개 조회 메서드 각각의 파라미터·반환값·동작을 정리한 카탈로그다.
> - TokenSystem의 전체 구조(파사드 + 4개 하위 컴포넌트의 역할 분담)는 [`00_token_system.md`](./00_token_system.md)를 참고한다 — 여기서는 반복하지 않는다.
> - `TokenMainAPI`(`Main`)/`TokenSetup`(`Setup`)은 이 문서의 범위 밖이다(필요하면 각각 별도 문서로 다룬다).
> - 이 문서는 `TokenServiceAPI.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 즉시 갱신한다.

## 1. 마스터 인덱스

| 메서드 | 카테고리 | 반환값 | 한 줄 설명 |
|---|---|---|---|
| [`GetTokenPosition(Token)`](#gettokenposition) | 위치 조회 | `Vector2Int` | 토큰의 현재 그리드 좌표 |
| [`GetTokenByPosition(Vector2Int)`](#gettokenbyposition) | 위치 조회 | `Token` | 좌표에 있는 토큰 |
| [`GetAllTokens()`](#getalltokens) | 위치 조회 | `List<Token>` | 등록된 모든 토큰 |
| [`IsTokenExist(Token)`](#istokenexist) | 위치 조회 | `bool` | 토큰 등록 여부 |
| [`GetCanMovePlace(Token, int)`](#getcanmoveplace) | 범위 계산 | `List<Vector2Int>` | 이동 가능한 모든 좌표(BFS) |
| [`GetAllAroundPlaces(...)`](#getallaroundplaces) | 범위 계산 | `List<Vector2Int>` | 주변 좌표(예외 옵션 포함) |
| [`GetAroundGrids(Token)`](#getaroundgrids) | 범위 계산 | `Vector2Int[]` | 인접 4방향 좌표 |
| [`GetShortestPath(Token, Vector2Int)`](#getshortestpath) | 경로 / 거리 | `List<Vector2Int>` | 최단 경로 좌표 목록 |
| [`GetMinDistance(Token, Vector2Int)`](#getmindistance) | 경로 / 거리 | `int` | 최단 경로 칸 수 |
| [`GetDistance(...)`](#getdistance) | 경로 / 거리 | `int` | 맨해튼 거리(3 오버로드) |
| [`GetDirection(Token, Token)`](#getdirection) | 방향 계산 | `Vector2Int` | from→to 방향 벡터 |
| [`GetPositionByDirection(Token, Vector2Int)`](#getpositionbydirection) | 방향 계산 | `Vector2Int` | 현재 위치 + 방향 |
| [`GetTargetPosByDirection(Token, Vector2Int, int)`](#gettargetposbydirection) | 방향 계산 | `Vector2Int` | 방향 부호화 후 거리만큼 이동한 좌표 |
| [`IsBound(Vector2Int)`](#isbound) | 유효성 검사 | `bool` | 그리드 범위 내 여부 |
| [`IsGridEmpty(...)`](#isgridempty) | 유효성 검사 | `bool` | 배치 가능 여부(예외 옵션 포함) |
| [`TargetPosesToCombatants(List<Vector2Int>)`](#targetposestocombatants) | 변환 | `List<CombatantView>` | 좌표 목록 → CombatantView 목록 |

## 2. 카테고리별 상세

### 2-1. 위치 조회

#### `GetTokenPosition`
```csharp
public Vector2Int GetTokenPosition(Token token)
```
- **파라미터**: `Token token`
- **반환값**: 토큰의 그리드 좌표. `token`이 `null`이거나 그리드에 등록되지 않았으면 `Vector2Int.down`
- **동작**: `gridPosByToken` 딕셔너리 조회
- **사용 예시**: `1.Systems/Skills/Player/ShoulderBashProcessor.cs:18` — `Vector2Int currentPos = TokenSystem.Instance.API.GetTokenPosition(shoulderBashGA.Caster);`
- **비고**: `Vector2Int.down`은 "없음"을 나타내는 전용 sentinel이 아니라 실제 좌표값(0,-1)이다 — 호출부가 별도 null/범위 체크 없이 그대로 좌표로 쓰면 (0,-1) 위치와 혼동될 수 있다.

#### `GetTokenByPosition`
```csharp
public Token GetTokenByPosition(Vector2Int position)
```
- **파라미터**: `Vector2Int position`
- **반환값**: 해당 좌표의 `Token`, 없으면 `null`
- **동작**: `tokenByGridPos` 딕셔너리 조회
- **사용 예시**: `1.Systems/Skills/Player/SplashProcessor.cs:20` — `IDamageable target = TokenSystem.Instance.API.GetTokenByPosition(targetPos) as IDamageable;`

#### `GetAllTokens`
```csharp
public List<Token> GetAllTokens() => gridPosByToken.Keys.ToList();
```
- **파라미터**: 없음
- **반환값**: 현재 그리드에 등록된 모든 `Token`의 새 리스트(원본 Dictionary 키의 복사본)
- **동작**: `gridPosByToken.Keys.ToList()`
- **사용 예시**: 확인된 외부 호출자 없음

#### `IsTokenExist`
```csharp
public bool IsTokenExist(Token token) => gridPosByToken.ContainsKey(token);
```
- **파라미터**: `Token token`
- **반환값**: 그리드에 등록되어 있으면 `true`
- **동작**: `gridPosByToken.ContainsKey`
- **사용 예시**: 확인된 외부 호출자 없음

### 2-2. 범위 계산

#### `GetCanMovePlace`
```csharp
public List<Vector2Int> GetCanMovePlace(Token token, int maxDistance)
```
- **파라미터**: `Token token`, `int maxDistance`
- **반환값**: 이동 가능한 모든 좌표 목록
- **동작**: 토큰의 현재 좌표에서 `UtilityBFS.FindALLRoots(start, maxDistance)`에 위임
- **사용 예시**: `1.Systems/MoveSystem.cs:101` — `heroMoveRange = TokenSystem.Instance.API.GetCanMovePlace(heroView, MPSystem.Instance.CurrentMP);`
- **비고**: `token`이 그리드에 등록되지 않았으면 `gridPosByToken[token]` 인덱서가 `KeyNotFoundException`을 던진다(널 체크 없음). 또한 `UtilityBFS.FindALLRoots`가 내부적으로 `TokenSystem.Instance.API.IsBound`/`IsGridEmpty` 등을 다시 호출하는 순환 구조다(`00_token_system.md` 4절 참고).

#### `GetAllAroundPlaces`
```csharp
public List<Vector2Int> GetAllAroundPlaces(Vector2Int currentPosition, int maxDistance, bool exceptEnemy = false, bool exceptHero = false, bool exceptDestructable = false)
```
- **파라미터**: 기준 좌표, 최대 거리, 적/영웅/파괴가능물 예외 플래그(기본 전부 `false`)
- **반환값**: 조건에 맞는 주변 좌표 목록
- **동작**: `UtilityBFS.FindAllPlaces(...)`에 그대로 위임
- **사용 예시**: `1.Systems/WaveSystem.cs:45` — `var poses = TokenSystem.Instance.API.GetAllAroundPlaces(corePosition, distance);`
- **비고**: `GetCanMovePlace`와 같은 순환 구조(`UtilityBFS` → `TokenSystem.Instance.API`)를 갖는다.

#### `GetAroundGrids`
```csharp
public Vector2Int[] GetAroundGrids(Token token)
```
- **파라미터**: `Token token`
- **반환값**: 상하좌우 4방향(`UtilityBFS.Dirs`) 중 그리드 범위 안에 있는 좌표 배열
- **동작**: 토큰 위치 기준 4방향 오프셋 계산 후 `grid.IsBound`로 필터링
- **사용 예시**: 확인된 외부 호출자 없음
- **비고**: `token`이 그리드에 없으면 `gridPosByToken[token]`에서 예외가 발생한다.

### 2-3. 경로 / 거리

#### `GetShortestPath`
```csharp
public List<Vector2Int> GetShortestPath(Token token, Vector2Int goal)
```
- **파라미터**: `Token token`, `Vector2Int goal`
- **반환값**: 최단 경로 좌표 리스트(경로가 없을 때의 반환값은 `UtilityBFS.FindShortestPath` 구현에 따름)
- **동작**: `grid.GetSimpleGridCopied()`(원본 보호용 클론) 위에서 `UtilityBFS.FindShortestPath`를 실행
- **사용 예시**: `1.Systems/MoveSystem.cs:68` — `var path = TokenSystem.Instance.API.GetShortestPath(currentHero, pos);`

#### `GetMinDistance`
```csharp
public int GetMinDistance(Token token, Vector2Int endPos)
{
    var path = GetShortestPath(token, endPos);
    if (path == null) return 0;
    return path.Count;
}
```
- **파라미터**: `Token token`, `Vector2Int endPos`
- **반환값**: 최단 경로의 칸 수(경로가 없으면 `0`)
- **동작**: 같은 클래스의 `GetShortestPath`를 내부 호출
- **사용 예시**: 확인된 외부 호출자 없음

#### `GetDistance`
```csharp
public int GetDistance(Token token, Vector2Int endPos)
public int GetDistance(Token token, Token token2)
public int GetDistance(Vector2Int startPos, Vector2Int endPos)
```
- **파라미터**: 3개 오버로드 — (토큰, 좌표) / (토큰, 토큰) / (좌표, 좌표)
- **반환값**: `int` — 세 오버로드 모두 동일하게 맨해튼 거리(`|dx| + |dy|`)를 계산
- **사용 예시**: `General/Util/UtilityBFS.cs:31` — `int dis = TokenSystem.Instance.API.GetDistance(start, current);`
- **비고**: 장애물을 고려하지 않는 직선(맨해튼) 거리다 — 실제 이동 가능 거리가 필요하면 `GetMinDistance`를 쓴다.

### 2-4. 방향 계산

#### `GetDirection`
```csharp
public Vector2Int GetDirection(Token to, Token from)
{
    return gridPosByToken[to] - gridPosByToken[from];
}
```
- **파라미터**: `Token to`, `Token from`
- **반환값**: `from`→`to` 방향 벡터(정규화되지 않은 좌표 차)
- **사용 예시**: 확인된 외부 호출자 없음

#### `GetPositionByDirection`
```csharp
public Vector2Int GetPositionByDirection(Token token, Vector2Int direction)
{
    Vector2Int current = gridPosByToken[token];
    return current + direction;
}
```
- **파라미터**: `Token token`, `Vector2Int direction`
- **반환값**: 현재 위치 + `direction`을 그대로 더한 좌표
- **사용 예시**: 확인된 외부 호출자 없음

#### `GetTargetPosByDirection`
```csharp
public Vector2Int GetTargetPosByDirection(Token token, Vector2Int direction, int distance = 1)
{
    Vector2Int pos = GetTokenPosition(token);
    Vector2 dir = Utility.GetSignVector2(direction);
    return pos + new Vector2Int((int)dir.x, (int)dir.y) * distance;
}
```
- **파라미터**: `Token token`, `Vector2Int direction`, `int distance`(기본 1)
- **반환값**: `token` 위치에서 `direction` 방향으로 `distance`만큼 떨어진 좌표
- **동작**: `Utility.GetSignVector2`로 `direction`을 부호(단위 벡터)만 취한 뒤 `distance`를 곱함 — `GetPositionByDirection`처럼 `direction`을 그대로 더하지 않는다는 점이 다르다.
- **사용 예시**: 확인된 외부 호출자 없음

### 2-5. 유효성 검사

#### `IsBound`
```csharp
public bool IsBound(Vector2Int pos) => grid.IsBound(pos.x, pos.y);
```
- **파라미터**: `Vector2Int pos`
- **반환값**: 그리드 범위 안이면 `true`
- **동작**: `grid.IsBound`에 위임
- **사용 예시**: `SubSystems/GridSelector.cs:40` — `if (!TokenSystem.Instance.API.IsBound(pos)) return;`

#### `IsGridEmpty`
```csharp
public bool IsGridEmpty(Vector2Int isPosition, bool enemyException = false, bool heroException = false, bool destructableExcpt = false)
{
    if (enemyException || heroException)
        return grid.CanSetExceptionToken(isPosition, enemyException, heroException, destructableExcpt);
    else return grid.CanSet(isPosition);
}
```
- **파라미터**: 좌표, 적/영웅/파괴가능물 예외 플래그(기본 전부 `false`)
- **반환값**: 배치 가능하면 `true`
- **동작**: `enemyException`/`heroException` 중 하나라도 `true`면 `grid.CanSetExceptionToken`, 아니면 `grid.CanSet`
- **사용 예시**: `1.Systems/Skills/KnockBackProcessor.cs:47` — `if (!TokenSystem.Instance.API.IsGridEmpty(pushedPos)) chrash = true;`
- **비고**: `grid.CanSetExceptionToken` 내부에서 `TokenSystem.Instance.API.GetTokenByPosition`을 다시 호출하는 순환 참조가 있다(`00_token_system.md` 4절 참고).

### 2-6. 변환

#### `TargetPosesToCombatants`
```csharp
public List<CombatantView> TargetPosesToCombatants(List<Vector2Int> targetPoses)
{
    List<CombatantView> combatants = new();
    foreach (var targetPos in targetPoses)
    {
        Token token = GetTokenByPosition(targetPos);
        if (token != null)
            combatants.Add(token as CombatantView);
    }
    if (combatants.Count > 0) return combatants;
    return null;
}
```
- **파라미터**: `List<Vector2Int> targetPoses`
- **반환값**: 각 좌표의 토큰을 `CombatantView`로 캐스팅한 목록. 대상이 하나도 없으면 빈 리스트가 아니라 `null`
- **동작**: 좌표마다 같은 클래스의 `GetTokenByPosition`을 호출
- **사용 예시**: 확인된 외부 호출자 없음
- **비고**: `token as CombatantView`는 토큰이 `CombatantView`가 아니면(예: `WaveCoreView`) `null`이 되어 결과 리스트에 `null` 항목으로 섞여 들어갈 수 있다 — 호출부에서 개별 null 체크가 필요하다.

## 3. 문서 유지보수 규칙

이 문서는 `TokenServiceAPI.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 1절(마스터 인덱스)과 2절(해당 카테고리 상세 블록)을 즉시 갱신한다. `TokenServiceAPI`의 역할·계약 자체(조회 전용이라는 성격)가 바뀌면 이 문서가 아니라 [`00_token_system.md`](./00_token_system.md)의 2-2절을 갱신한다.
