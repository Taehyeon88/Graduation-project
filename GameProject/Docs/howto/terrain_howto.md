# 지형(장애물 · 발판) 만들기 — 데이터 → 뷰/시스템 → 에셋 → 스테이지 배치 → 카탈로그

> 장애물 또는 발판 하나를 처음부터 끝까지 만드는 레시피. 구조·생성 순서·중립지대는 [08_terrain_obstacle.md](../feature-spec/08_terrain_obstacle.md), 완성된 목록은 [terrain_catalog.md](../feature-spec/Terrain/terrain_catalog.md)가 갖고 있으므로 여기서는 **만드는 순서와 판단 기준**만 다룬다. 이 문서는 돌격 화살통(장애물)·풀숲(발판)을 만든 실제 과정에서 추출했다.

## 1. 먼저 종류를 고른다

| 질문 | 장애물 (파괴 가능) | 발판 (타일 위 효과) | 중립지대 |
|---|---|---|---|
| 칸을 점유하나 | 예 (토큰) | 아니오 (이동 가능) | 예 (토큰 없이 `simpleGrid`만 막음) |
| 공격 대상인가 | 예 (`IsTarget`에 포함) | 아니오 | 아니오 |
| 데이터(SO) | `ObstacleData` | `TerrainData` | 없음 — 위치만 |
| 이 문서의 대상 | **§3** | **§4** | 대상 아님 — `StageData.NeutralPositions`에 좌표만 추가 |

- 새 장애물이 **기존 `ObstacleView` 그대로** 쓸 수 있으면(체력·특성만 다른 경우) 코드 작성 없이 §3-3(에셋)부터 시작한다.
- 새 발판이 **기존 `TerrainEffect`(예: `BushEffect`)와 다른 효과**를 가지면 `TerrainEffect` 서브클래스를 새로 만든다 → §4-2.

## 2. 사전 읽기 (관련 부분만)
- `Docs/feature-spec/08_terrain_obstacle.md` — 구조와 생성 순서
- `Docs/feature-spec/Terrain/terrain_catalog.md` — 기존 항목과 한계 (중복 방지)
- `Docs/convention.md` — 코드 규칙 (`~System`/`~Data` 네이밍, `OnEnable`/`OnDisable` 대칭, SO 데이터 형태)
- 장애물: `3.Views/ObstacleView.cs`, `3.Views/CombatantView.cs`(`IsTarget`, `Damage`), `SubSystems/TokenSetup.cs`(`SetUpObstacles`), `4.Creators/TokenCreator.cs`
- 발판: `Models/TerrainEffect.cs`, `Terrains/BushEffect.cs`(가장 단순한 기존 기능 예), `5.Data/TerrainData.cs`, `3.Views/TerrainView.cs`, `1.Systems/TerrainSystem.cs`, `SubSystems/TokenSetup.cs`(`SetUpTerrains`)
- 장애물에 처치 효과를 달 때: [trait_howto.md](trait_howto.md), `Docs/feature-spec/Trait/trait.md`(재사용 가능한 조건·효과 목록)

---

## 3. 장애물 만들기

### 3-1. 문장을 분해한다
"이 장애물을 파괴한 용병이 ~한다" 같은 문장이면 **체력(`Health`) + 특성(`Traits`)** 으로 쪼갠다. 장애물은 `CombatantData`를 상속하므로 `Health`/`Damage`/`Speed`/`Traits`를 그대로 쓴다.
- 공격력은 0, 속도는 -1(이동 불가)로 둔다. 속도 -1은 자동 전투 큐에서 제외되고 UI에 `-`로 표시된다. `ObstacleView`는 `Forward`가 zero라 스스로 행동하지 않는다.
- 특성이 필요 없으면 `Traits`를 비운다 (단순 벽 역할).

### 3-2. 새 코드가 필요한 경우 (처치 효과)
처치자 정보가 필요한 효과는 이미 `KillGA.Killer`(`Token`, 처치자 없으면 null)로 전달된다. 새 효과 코드는 **특성 부품**으로 만든다 (장애물 전용 시스템을 만들지 않는다).
1. 조건은 대부분 `SelfDeathCondition`(소유자 사망, **PRE**)을 재사용한다. 사망 계열이 PRE인 이유: `KillGA` performer가 토큰을 파괴하면 `OnDisable`이 구독을 해제해 POST에서는 놓칠 수 있다.
2. 효과는 `Traits/` 아래 `~Effect` 클래스로 만든다 (`TraitEffect.PerformReaction(GameAction, CombatantView owner)`). 예: `KillerAttackAllOpponentsEffect`.
   - `killGA.Killer is not CombatantView killer`면 즉시 반환 — 스킬 등으로 처치자가 없는 경우 무동작이 계약이다.
   - 영웅/적 목록을 순회할 때 장애물은 `EnemyViews`/`HeroViews`에 들어가지 않는다는 점을 전제로 한다.
3. 세부 절차(조건·효과·에셋 순서)는 `trait_howto.md`를 따른다. 새 GA가 필요하면 만들기 전에 알리고 `action_command.md`를 갱신한다.

### 3-3. 에셋 (`ObstacleData`)
1. 생성: Project 우클릭 → `Create > Data > Token > Obstacle`.
2. 위치: `Assets/Game/1_Datas/11.Obstacles/<이름>.asset`.
3. 필드: `Id`(3001번대에서 이어서), `Description`(사용자가 준 설명 문장), `Sprite`(임시 에셋 OK — 아트 제외), `Health`, `Damage`=0, `Speed`=-1, `Traits`(특성 에셋 연결).
4. 특성 에셋은 `1_Datas/3.Traits/`에 둔다. 조건 `Reaction Timing`=**PRE(0)**.

### 3-4. 뷰·프리팹
- 기존 `3_Prefabs/Views/ObstacleView.prefab`과 `TokenCreator.obstaclePrefab`을 그대로 쓴다. 장애물마다 프리팹을 새로 만들 필요 없다 — `ObstacleView.SetUp(ObstacleData)`가 스프라이트·체력을 데이터에서 채운다.
- 크기 조정은 프리팹 scale이 아니라 **스프라이트 `.meta`의 `spritePixelsToUnits`** 로 한다 (프로젝트 규칙).

### 3-5. 공격 대상·스킬 지정 규칙 (이미 반영됨 — 바꾸지 말 것)
- `HeroView.IsTarget` / `EnemyView.IsTarget`은 `ObstacleView`를 포함한다. 그래서 장애물에 막힌 영웅·적은 정면 장애물을 공격한다.
- 플레이어 스킬(`SharpDagger.IsValidTarget`)은 `EnemyView`/`EnemyFlagView`만 허용하는 **허용 목록** 방식이다. 새 스킬도 허용 목록으로 만들면 장애물이 자동으로 제외된다. 장애물을 지정 가능하게 하려면 의도적으로 추가해야 한다.

### 3-6. 스테이지 배치
`StageData.ObstacleSpawns`에 `(Position, Data)` 추가 (`MatchSetupSystem` → `TokenSetup.SetUpObstacles` → `AddToken(TokenType.Obstacle)`).
- 중립지대·다른 토큰이 있는 칸, 그리드 밖, 데이터 없음이면 `LogError`("장애물 배치 불가") 후 건너뛴다.
- 영웅/적 기본 배치 칸(`CalcSetupPoses`)과 겹치지 않게 둔다. 배치 위치 계산은 `NeutralPositions`만 본다.

---

## 4. 발판 만들기

### 4-1. 에셋 (`TerrainData`)
1. 생성: `Create > Data > Terrain`.
2. 위치: `Assets/Game/1_Datas/12.Terrains/<이름>.asset`.
3. 필드: `Id`(1부터 이어서), `Description`(사용자가 준 설명 문장), `Sprite`, `Effect`(`TerrainEffect` 서브클래스 — 인스펙터의 SR 드롭다운에서 선택). 이름은 SO 에셋 이름(`TerrainData.Name`)이다.
4. **효과는 `Effect` 필드의 `TerrainEffect` 서브클래스가 가진다.** 기존 기능을 그대로 쓰면(예: `BushEffect` = 올라서면 회피) 코드 없이 에셋만 만든다. 수치(`dodgeStack` 등)는 인스펙터에서 바꾼다.

### 4-2. 새 효과가 필요한 발판 (`TerrainEffect` 서브클래스)
발판은 **토큰이 아니다** — `grid.SetToken`을 쓰면 칸을 점유해 이동이 막힌다. `TokenSetup`의 `Dictionary<Vector2Int, TerrainView>`에만 보관한다.
- 위치 `Assets/Game/0_Scripts/Terrains/`, 1파일 1클래스, 파일명 = 클래스명 (`~Effect`). `TerrainEffect`를 상속해 `OnEnter(CombatantView unit)`/`OnExit(CombatantView unit)`을 구현한다. 파일 첫머리에 의미 한 줄 주석.
- 발동 시점은 `TerrainSystem`이 `MoveGA`/`PlayCardGA` POST에서 `TerrainView.Enter/Exit`로 호출하므로 효과 클래스는 GA를 구독하지 않는다. 새 호출 경로(예: 웨이브 스폰)가 필요하면 `TerrainSystem` 리액션을 추가하고 `OnEnable`/`OnDisable`에서 **대칭**으로 등록·해제한다 — 계약 변경이므로 구현 전에 알린다.
- 수치는 `[SerializeField] private`으로 노출한다 (`BushEffect.dodgeStack` 참고). 매직넘버 금지.
- **상태**: `TerrainView`가 `Clone()`한 발판별 사본을 쓰므로 필드에 유닛별 상태(예: `BushEffect.granted`)를 둘 수 있다. 참조형 상태(List/HashSet)는 `Clone()`을 오버라이드해 새로 만든다 (`TraitEffect`와 같은 규칙).
- 부여한 효과를 떠날 때 회수해야 하면 `BushEffect.granted`처럼 추적 집합을 둔다. 한계: 상태효과 스택에는 출처 구분이 없어 다른 출처 회피가 회수될 수 있다 (카탈로그 참고).
- 아직 없는 경로: 웨이브 스폰, "공격받으면 제거·2라운드 후 재생성". 필요하면 후속 과제(08 §6)로 따로 만든다.

### 4-3. 타일 뷰·프리팹
- 기존 `3_Prefabs/Views/TerrainView.prefab`(바닥 타일 복사: iso z 0.01, sortingOrder -9)을 `TokenCreator.terrainPrefab`으로 쓴다. 발판마다 프리팹을 새로 만들 필요 없다 — `TerrainView.SetUp(data, position)`이 스프라이트·위치를 채우고 `Effect`를 복제하며, 높이(z)는 프리팹 값을 유지한다.
- 타일 부모는 `TokenCreator.isoWorld`(`IsoGridWorldView`).

### 4-4. 스테이지 배치
`StageData.TerrainSpawns`에 `(Position, Data)` 추가 (`MatchSetupSystem` → `TokenSetup.SetUpTerrains` → `TokenCreator.CreateTerrain`).
- 중립지대·장애물·이미 있는 발판 위, 범위 밖, 데이터 없음이면 `LogError`("발판 배치 불가") 후 건너뛴다.
- **생성 순서 의존**: 발판은 중립지대·장애물 **뒤**에 놓인다 (점유 검사가 `grid.CanSet`). 순서를 바꾸지 않는다.

---

## 5. 처음 쓰는 씬 설정 (한 번만)
이미 `GameDemoScene`에 완료되어 있다. 새 씬을 만들 때만 필요하다.
- `--- SYSTEMS ---` 아래 `TerrainSystem` GameObject 1개 (리액션 전용, 인스펙터 연결 없음).
- `TokenCreator`: `obstaclePrefab`=`ObstacleView.prefab`, `terrainPrefab`=`TerrainView.prefab`.
- 에디터 작업(씬 배치·프리팹·인스펙터 연결)은 사용자가 한다 — Claude는 클릭 순서를 안내하고 멈춘다.

## 6. 체크리스트
- [ ] 종류 결정 (§1) — 장애물 / 발판 / 중립지대
- [ ] (필요 시) 효과 코드: 특성 부품(§3-2) 또는 `TerrainEffect` 서브클래스(§4-2), 구독·해제 대칭
- [ ] 에셋 생성: `11.Obstacles/` 또는 `12.Terrains/`, `Id` 중복 없음
- [ ] 스테이지 배치: `StageData`의 `ObstacleSpawns` / `TerrainSpawns` / `NeutralPositions`
- [ ] Console 에러 0 (Unity MCP `read_console`)
- [ ] Play 확인 — 장애물: 정면 공격·파괴·처치 효과·스킬 지정 불가 / 발판: 올라서면 효과, 떠나면 회수
- [ ] 문서 갱신: `terrain_catalog.md`에 섹션 추가(구조 표·요약 표·연결 흐름·한계), 시그니처/SO 필드를 바꿨다면 `08_terrain_obstacle.md`와 `convention.md`도 확인

## 7. 흔한 실수
- 장애물 특성의 `Reaction Timing`을 POST로 둠 → 파괴 후 구독 해제되어 효과가 안 나간다. PRE(0).
- 발판을 `AddToken`/`SetToken`으로 만듦 → 칸이 점유되어 이동 불가, 공격 대상이 된다.
- 장애물을 `EnemyViews`/`HeroViews`에 기대어 순회 → 목록에 없다. 추가 공격 대상에도 안 들어간다.
- 중립지대 칸에 장애물·발판을 놓음 → 배치 불가 `LogError`.
- 크기 조정을 프리팹 scale로 함 → 스프라이트 `.meta`의 `spritePixelsToUnits`로 조정한다.
- `StageData.ObstaclePositions`라는 옛 이름 — 현재는 `NeutralPositions`(중립지대)다. 직렬화 값은 `FormerlySerializedAs`로 유지된다.
