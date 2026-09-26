# TokenSystem 아키텍처

> 이 문서는 그리드 위의 모든 토큰(영웅/몬스터/웨이브 코어 등)의 좌표·조회·이동·초기배치를 관리하는 `TokenSystem`의 구조와 사용 계약을 정리한 참고 문서다. `Token`/`CombatantView`/`HeroView`/`EnemyView` 등 각 토큰 서브클래스의 전투 도메인 상세(HP 계산식, 스킬, 상태이상 종류 등)는 다루지 않는다 — 여기서는 "TokenSystem이 이들을 어떻게 다루는가"만 다룬다.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/1.Systems/TokenSystem.cs` | 파사드(Singleton) — 하위 컴포넌트/공유 상태 노출 |
| `Assets/Game/0_Scripts/Models/TokenGrid.cs` | 데이터 계층 — 좌표 ↔ 토큰 매핑, 점유 판정 |
| `Assets/Game/0_Scripts/SubSystems/TokenSetup.cs` | 초기 배치 전용 (맵/몬스터/영웅 셋업) |
| `Assets/Game/0_Scripts/SubSystems/TokenServiceAPI.cs` | 조회 전용 API (side-effect 없음) |
| `Assets/Game/0_Scripts/SubSystems/TokenMainAPI.cs` | 런타임 상태 변경 API (이동/추가/삭제) |
| `Assets/Game/0_Scripts/Models/Token.cs` | 모든 토큰의 최상위 베이스 클래스 |
| `Assets/Game/0_Scripts/4.Creators/TokenCreator.cs` | 토큰 프리팹 생성 팩토리 |

## 1. 한 줄 개요

`TokenSystem` 자신은 로직이 없는 얇은 파사드다. 실제 책임은 4개로 분리되어 있다 — `TokenGrid`가 "좌표 ↔ Token" 매핑을 소유하는 유일한 데이터 계층이고, `TokenServiceAPI`(조회 전용, side-effect 없음)와 `TokenMainAPI`(런타임 상태 변경, 연출 코루틴 포함)가 이 데이터에 접근하는 두 개의 분리된 계약을 제공하며, `TokenSetup`은 전투 시작 전 초기 배치만 전담한다.

## 2. 파사드와 4개 하위 컴포넌트

```csharp
// TokenSystem.cs
public class TokenSystem : Singleton<TokenSystem>
{
    [field: SerializeField] private TokenGrid grid;
    [field: SerializeField] public IsoWorld IsoWorld { get; private set; }
    [field: SerializeField] public TokenSetup Setup { get; private set; }
    [field: SerializeField] public TokenServiceAPI API { get; private set; }
    [field: SerializeField] public TokenMainAPI Main { get; private set; }

    public Token SelectedToken { get; private set; }
    public List<HeroView> HeroViews { get; set; } = new();
    public List<EnemyView> EnemyViews { get; private set; } = new();
    public int gridWidth => grid.width; public int gridHeight => grid.height;
}
```

`Setup`/`API`/`Main` 3개 컴포넌트는 인스펙터에서 각자 동일한 씬의 `TokenGrid`를 참조하도록 배선되어 있다(코드로 강제되는 싱글턴 참조는 아님). `HeroViews`/`EnemyViews`/`IsoWorld`는 이 4개 컴포넌트가 공유하는 상태 저장소다.

### 2-1. TokenGrid — 데이터 계층

```csharp
// TokenGrid.cs
public Dictionary<Token, Vector2Int> gridPosByToken = new();
public Dictionary<Vector2Int, Token> tokenByGridPos = new();
public int[,] simpleGrid { get; private set; }   //0 - 토큰 없음 | 1 - 토큰 있음
private List<Vector2Int> remainCells = new();     //비어있는 셀 목록
```

- 좌표 ↔ 토큰 관계를 소유하는 **유일한 데이터 계층**. 양방향 Dictionary 2개 + 빠른 점유 판정용 `int[,]` + "빈 칸" 목록을 함께 유지한다(용도별로 조회 비용을 나눈 3중 구조).
- `SetToken`/`RemoveToken`/`ChangeTokenPos`는 이 세 저장소를 항상 함께 갱신한다 — 하나만 갱신하면 즉시 불일치가 생긴다.
- `GetSimpleGridCopied()`는 `int[,]`의 클론을 반환한다 — 경로탐색(`UtilityBFS.FindShortestPath`)이 탐색 중 배열을 마킹해도 원본이 훼손되지 않게 하기 위함.
- `CanSetExceptionToken(...)`은 내부에서 `TokenSystem.Instance.API.GetTokenByPosition(pos)`를 호출한다 — 데이터 계층이 상위 계층(`API`)을 역참조하는 유일한 지점(4절 참고).
- 두 Dictionary는 `Token`(베이스 클래스) 타입으로만 키를 잡는다 — 구체 서브클래스가 몇 개든, 어떤 필드를 가지든 `TokenGrid`/`TokenServiceAPI`/`TokenMainAPI`는 알 필요가 없다. `Token` 자신도 자신의 그리드 좌표를 필드로 갖지 않는다 — "이 토큰이 어디 있는가"는 항상 이 Dictionary(→`TokenServiceAPI`)에게 물어야 한다.

### 2-2. TokenServiceAPI (`API`) — 조회 전용 계약

`grid`만 참조하며 상태를 바꾸는 코드가 단 한 줄도 없는 완전한 읽기 전용(query) 계약이다. 코드베이스 전역(이동/스킬/AI/UI/경로탐색)에서 가장 많이 호출되는 창구이기도 하다. 16개 메서드 각각의 파라미터·반환값·동작은 [`00_token_system_api.md`](./00_token_system_api.md)에서 다룬다.

### 2-3. TokenMainAPI (`Main`) — 런타임 상태 변경 계약

게임 진행 중 그리드/토큰 상태를 실제로 바꾸는 유일한 창구다. 그리드 데이터는 연출 시작 전에 즉시 갱신되고, 코루틴은 시각적 결과(오브젝트 파괴/실제 이동)만 지연시킨다. 4개 메서드 각각의 파라미터·반환값·동작은 [`00_token_system_main.md`](./00_token_system_main.md)에서 다룬다.

### 2-4. TokenSetup (`Setup`) — 초기 배치 전용 컴포넌트

전투 시작 전 스테이지 맵과 몬스터/영웅 토큰을 그리드에 배치하는 초기화 전용 컴포넌트다. 게임 진행 중에는 호출되지 않는다. 4개 메서드 각각의 파라미터·반환값·동작은 [`00_token_system_setup.md`](./00_token_system_setup.md)에서 다룬다.

## 3. 사용 계약 요약 — API vs Main vs Setup

| 판단 질문 | API (조회) | Main (커맨드) | Setup (초기화) |
|---|---|---|---|
| 그리드/토큰 상태를 실제로 바꾸는가? | ❌ | ✅ | ✅ (게임 시작 전만) |
| 게임 진행 중(전투 중) 호출되는가? | ✅ | ✅ | ❌ |
| 연출(코루틴) 완료를 기다려야 하는가? | ❌ | ✅ (`MoveToken`/`RemoveToken`) | ❌ (배치 자체는 즉시, 이후 입력 대기는 이벤트 기반) |
| 여러 시스템이 공통으로 호출하는 범용 질의인가? | ✅ (전역에서 호출) | ❌ (호출처 극소수) | ❌ |

## 4. 주의사항 / 함정

- **순환 참조 2곳**: `TokenGrid.CanSetExceptionToken()` → `TokenSystem.Instance.API.GetTokenByPosition()` 역참조. `TokenServiceAPI`가 위임하는 `UtilityBFS`(static)도 `TokenSystem.Instance.API`/`gridWidth`/`gridHeight`를 다시 호출한다 — 계층이 완전히 한 방향으로만 흐르지 않는다.
- **`TokenSystem.SelectedToken`은 죽은 프로퍼티다.** `private set`인데 프로젝트 전체에서 이 값을 설정하는 코드가 없다. 실제 "선택된 영웅" 상태는 `HeroSystem.CurrentHero`(설정: `GridSelector`의 그리드 클릭 처리)가 전담한다.
- **`WaveCoreView`는 `CombatantView`를 상속하지 않고 `IDamageable`을 독립적으로 구현한다.** HP 차감/피격 연출 로직이 `CombatantView.Damage()`와 사실상 중복되어 있고, 코어 체력이 0 이하가 되어도 `Debug.Log("게임 승리")`만 호출할 뿐 실제 게임 종료 처리(`GameClearGA` 등)로 이어지지 않는 미완성 상태다.
- **`TokenMainAPI.MoveToken`의 `useMovedPath` 파라미터는 시그니처에만 있고 메서드 본문에서 전혀 쓰이지 않는다.**
- **`TokenMainAPI.AddToken`은 현재 외부 호출자가 없다** — "게임 진행 중 토큰 추가" 용도로 예비되어 있으나 아직 어떤 시스템도 사용하지 않는다.
- **`RemoveToken`(코루틴, 연출 O) vs `RemoveToken2`(즉시, 연출 X)** 구분에 주의한다 — 후자는 주석상 "CombatantView가 아닌 것들의 제거" 용도로 만들어졌다.

## 5. 새 토큰 타입/기능 추가 시 체크리스트

1. 새 `Token` 서브클래스는 `Token`을 상속하고, `SetUp(...)` 안에서 반드시 `SetUpBaseBase(tokenData)`(전투 유닛이면 `CombatantView.SetUpBase`)를 호출해 공통 초기화를 마친다.
2. 전투 가능한 유닛이면 `CombatantView`를 상속해 HP/상태이상/`Damage()`를 재사용한다 — `WaveCoreView`처럼 `IDamageable`을 처음부터 다시 구현하지 않는다.
3. `TokenCreator.CreateToken()`에 새 `TokenType` 분기와 프리팹 필드를 추가한다. `TokenCreator`는 **시각적 배치**(`TokenTransform.position`)만 책임진다.
4. 논리적 그리드 등록(`grid.SetToken(token, pos)`)은 호출자(생성 로직) 책임이다 — `TokenSetup`/`TokenMainAPI.AddToken` 패턴을 따른다.
5. 좌표 조회가 필요하면 항상 `TokenServiceAPI`(`API`)를 거친다 — 새 클래스에 좌표 필드를 직접 추가하지 않는다.
6. 게임 진행 중 상태를 바꾸는 로직은 `TokenMainAPI`(`Main`)에 추가한다. 연출이 필요하면 `MoveToken`/`RemoveToken`처럼 "데이터 즉시 반영 → 코루틴으로 연출 대기" 패턴을 따른다.
