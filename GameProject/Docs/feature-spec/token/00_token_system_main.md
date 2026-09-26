# TokenSystem API — TokenMainAPI 메서드 카탈로그

> 이 문서는 `TokenMainAPI`(`TokenSystem.Instance.Main`)가 제공하는 4개 런타임 상태 변경 메서드 각각의 파라미터·반환값·동작을 정리한 카탈로그다.
> - TokenSystem의 전체 구조(파사드 + 4개 하위 컴포넌트의 역할 분담)는 [`00_token_system.md`](./00_token_system.md)를 참고한다 — 여기서는 반복하지 않는다.
> - `TokenServiceAPI`(`API`)는 [`00_token_system_api.md`](./00_token_system_api.md)에서 다룬다. `TokenSetup`(`Setup`)은 이 문서의 범위 밖이다(필요하면 별도 문서로 다룬다).
> - 이 문서는 `TokenMainAPI.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 즉시 갱신한다.

## 1. 마스터 인덱스

| 메서드 | 카테고리 | 코루틴 | 반환값 | 한 줄 설명 |
|---|---|---|---|---|
| [`AddToken(TokenData, TokenType, Vector2Int)`](#addtoken) | 추가 | 아니오 | `void` | 토큰 생성 + 그리드 등록 |
| [`RemoveToken(Token)`](#removetoken) | 제거 | 예 | `IEnumerator` | 그리드 즉시 반영 + 축소 연출 대기 후 파괴 |
| [`RemoveToken2(Token)`](#removetoken2) | 제거 | 아니오 | `void` | 연출 없이 즉시 그리드 제거 + 파괴 |
| [`MoveToken(Token, Vector2Int, bool, bool)`](#movetoken) | 이동 | 예 | `IEnumerator` | 그리드 좌표 즉시 반영 + 이동 연출 대기 |

## 2. 카테고리별 상세

### 2-1. 추가

#### `AddToken`
```csharp
public void AddToken(TokenData tokenData, TokenType tokenType, Vector2Int gridPosition)
{
    Token token = TokenCreator.Instance.CreateToken(tokenData, tokenType, new(gridPosition.x, gridPosition.y, 1));
    grid.SetToken(token, gridPosition);
    if (token is EnemyView enemyView) EnemyViews.Add(enemyView);
}
```
- **파라미터**: `TokenData tokenData`, `TokenType tokenType`, `Vector2Int gridPosition`
- **반환값**: `void`
- **코루틴 여부**: 아니오
- **동작**: `TokenCreator.Instance.CreateToken(...)`으로 토큰을 생성 → `grid.SetToken(token, gridPosition)`으로 그리드에 등록 → 생성된 토큰이 `EnemyView`면 `EnemyViews`에도 추가
- **사용 예시**: 확인된 외부 호출자 없음
- **비고**: 게임 진행 중 토큰을 추가하는 용도로 만들어졌지만 현재 어떤 시스템도 호출하지 않는 예비 API다.

### 2-2. 제거

#### `RemoveToken`
```csharp
public IEnumerator RemoveToken(Token token)
{
    if (token is EnemyView enemyView) EnemyViews.Remove(enemyView);
    grid.RemoveToken(token);

    Tween tween = token.Transform.DOScale(Vector3.zero, 0.25f);
    yield return tween.WaitForCompletion();
    Destroy(token.gameObject);
}
```
- **파라미터**: `Token token`
- **반환값**: `IEnumerator`(코루틴)
- **코루틴 여부**: 예
- **동작**: `token`이 `EnemyView`면 `EnemyViews`에서 먼저 제거 → `grid.RemoveToken(token)`으로 그리드 데이터를 즉시 반영 → 스케일 축소 트윈(`0.25초`) 재생 → 완료 대기 → `Destroy(token.gameObject)`
- **사용 예시**: `1.Systems/Skills/DamageSystem.cs:93` — `yield return TokenSystem.Instance.Main.RemoveToken(killGA.Token);`
- **비고**: 그리드 데이터(`grid.RemoveToken`)는 연출 시작 전에 즉시 갱신되고, 코루틴은 시각적 파괴만 지연시킨다.

#### `RemoveToken2`
```csharp
public void RemoveToken2(Token token)
{
    if (token is EnemyView enemyView) EnemyViews.Remove(enemyView);
    grid.RemoveToken(token);
    Destroy(token.gameObject);
}
```
- **파라미터**: `Token token`
- **반환값**: `void`
- **코루틴 여부**: 아니오
- **동작**: `RemoveToken`과 동일하지만 연출 트윈 없이 `grid.RemoveToken` 직후 즉시 `Destroy`
- **사용 예시**: 확인된 외부 호출자 없음
- **비고**: 주석상 "임시 토큰 제거 함수(`CombatantView`가 아닌 것들의 제거)" 용도다. 그런데 실제로 연출 없이 즉시 제거되어야 할 대표 사례인 `HeroPreview`(배치 미리보기)조차 `TokenSetup.EndSetUpHero()`에서 `Destroy(preview.gameObject)`로 직접 파괴되고 `RemoveToken2`를 거치지 않는다 — 의도된 용도가 있음에도 현재는 아무도 호출하지 않는 상태다.

### 2-3. 이동

#### `MoveToken`
```csharp
public IEnumerator MoveToken(Token token, Vector2Int targetPos, bool useAnimation = true, bool useMovedPath = true)
{
    grid.ChangeTokenPos(token, targetPos);
    if (useAnimation)
    {
        Tween tween = Utility.GetTween(token, targetPos, 0.3f);
        yield return tween.WaitForCompletion();
    }
}
```
- **파라미터**: `Token token`, `Vector2Int targetPos`, `bool useAnimation`(기본 `true`), `bool useMovedPath`(기본 `true`)
- **반환값**: `IEnumerator`(코루틴)
- **코루틴 여부**: 예
- **동작**: `grid.ChangeTokenPos(token, targetPos)`로 그리드 좌표를 즉시 반영 → `useAnimation`이 `true`면 `Utility.GetTween(token, targetPos, 0.3f)` 이동 트윈 재생 후 완료 대기
- **사용 예시**: `1.Systems/MoveSystem.cs:139` — `yield return TokenSystem.Instance.Main.MoveToken(mover, position);`
- **비고**: `useMovedPath` 파라미터는 시그니처에만 있고 메서드 본문에서 전혀 사용되지 않는다.

## 3. 문서 유지보수 규칙

이 문서는 `TokenMainAPI.cs`의 스냅샷이다. 메서드가 추가/변경/삭제되면 1절(마스터 인덱스)과 2절(해당 카테고리 상세 블록)을 즉시 갱신한다. `TokenMainAPI`의 역할·계약 자체(런타임 상태 변경 전용이라는 성격)가 바뀌면 이 문서가 아니라 [`00_token_system.md`](./00_token_system.md)의 2-3절을 갱신한다.
