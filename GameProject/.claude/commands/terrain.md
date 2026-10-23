---
description: 종류(o=장애물, t=발판) + 이름 + 설명을 받아 ObstacleData·TerrainData 에셋(과 필요한 효과 코드)을 만든다 (Docs/howto/terrain_howto.md 기준, 설명이 모호하거나 설정이 이상하면 먼저 되묻는다)
argument-hint: "[o|t, 이름, 설명 — 예: o, 돌격 화살통, 체력 4. 파괴한 용병이 모든 적 용병을 추가로 공격 / t, 풀숲, 올라선 용병은 회피를 얻음]"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Write
  - Edit
  - AskUserQuestion
  - mcp__UnityMCP__refresh_unity
  - mcp__UnityMCP__read_console
  - mcp__UnityMCP__execute_code
disable-model-invocation: true
---

`Docs/howto/terrain_howto.md`를 처음부터 끝까지 읽고 그 절차를 따라 장애물(§3) 또는 발판(§4) 하나를 만든다. 절차 자체는 이 파일에 복제하지 않는다 — 문서가 기준이다. 아래는 이 커맨드 고유 규칙만 적는다.

**입력**
- `$ARGUMENTS`: `종류, 이름, 설명` — 첫 쉼표 앞 = 종류(`o` 장애물 / `t` 발판), 둘째 = 이름, 나머지 = 설명.
- 종류가 `o`/`t`가 아니거나(예: 중립지대를 말하는 경우 포함), 이름·설명이 비어 있으면 코드를 쓰지 않고 `AskUserQuestion`으로 먼저 묻는다. 추측하지 않는다.

**먼저 읽기 (순서 고정)**
1. `Docs/howto/terrain_howto.md` 전체
2. `Docs/feature-spec/Terrain/terrain_catalog.md` — 기존 항목·Id·한계와 대조 (이름·효과 중복 확인)
3. `Docs/feature-spec/08_terrain_obstacle.md` 중 관련 절, `Docs/convention.md` 중 관련 절
4. 종류별: 장애물이면 `Docs/feature-spec/Trait/trait.md`(재사용 가능한 조건·효과), 발판이면 `Assets/Game/0_Scripts/Models/TerrainEffect.cs`, `Terrains/BushEffect.cs`, `1.Systems/TerrainSystem.cs`
5. 문서와 실제 코드·에셋이 어긋나면 어느 쪽이 낡았는지 먼저 알린다 (조용히 맞추지 않는다)

**설명 해석과 검증 — 아래에 하나라도 걸리면 코드·에셋을 만들기 전에 `AskUserQuestion`으로 되묻는다**

공통
- 설명에서 수치·대상·범위·타이밍이 모호하거나, 원작 규칙을 모를 때 — 선택지를 제시한다. 원작을 모르는 값은 `(원작 확인 필요)`로 두고 임시값을 제안한다
- 같은 이름의 에셋이 이미 있을 때 (덮어쓰지 않는다), `Id`를 정하기 애매할 때
- 설명 내용이 입력한 종류와 맞지 않을 때 (종류 설정이 틀린 것 같을 때):
  - `o`인데 "올라서면/위에 있으면 ~", "지나갈 수 있다", "이동 가능" → 발판(`t`)이 아닌지
  - `o`인데 "파괴할 수 없다/이동·배치 불가" → 파괴 불가 칸은 장애물이 아니라 중립지대(데이터 없음, `StageData.NeutralPositions`에 좌표만)가 아닌지
  - `t`인데 "체력/파괴/공격받으면 사라짐(체력 기준)/공격 대상" → 장애물(`o`)이 아닌지
  - `t`인데 "이동 불가/막는다" → 중립지대 또는 장애물이 아닌지

장애물(`o`)
- 체력이 없거나 0 이하, 또는 설명 안에 체력이 없을 때 — 체력을 묻는다
- 설명에 "스스로 공격/이동" 같은 행동이 있을 때 — `ObstacleView`는 `Forward`가 zero라 스스로 행동하지 않는다 (시스템적으로 불가)
- 설명이 공격력/속도를 요구할 때 (`Damage`/`Speed`는 0으로 둔다) — 의도 확인
- 효과가 있는 경우 `terrain_howto.md` §3-2에 따라 **특성 부품**으로 만든다: `trait.md`와 대조해 재사용 우선. 신규 조건/효과·새 GA·계약 변경이 필요하면 `/trait` 커맨드의 질문 기준(`.claude/commands/trait.md` "멈추고 묻는 경우")을 그대로 적용한다
- 처치자 정보가 필요한 효과인데 처치자가 없는 경로(스킬로 파괴)의 동작이 설명에 없을 때 — 기본은 무동작임을 알리고 확인
- 사망 계열 특성의 타이밍이 PRE가 아닐 때 (POST면 구독 해제로 효과가 안 나간다) — 시스템 문제로 알린다
- 효과가 예약하는 GA를 조건이 그대로 구독하는 조합일 때 (무한 루프)
- 스킬(플레이어 스킬)로 지정 가능해야 한다는 설명 — 현재 `SharpDagger` 등은 허용 목록이라 장애물이 지정되지 않는다. 지정 가능하게 하려면 스킬 코드 수정이 필요하므로 먼저 알리고 승인받는다

발판(`t`)
- 효과가 기존 `TerrainEffect`(`Terrains/` 폴더, 예: `BushEffect` = 올라서면 회피)와 같으면 → 에셋만 만들고 `Effect`에 그 기능을 지정한다 (수치만 다르면 인스펙터 값)
- 효과가 그 외(다른 상태효과, 피해, 이동 제한 등)면 `TerrainEffect` 서브클래스를 새로 만든다 (`OnEnter`/`OnExit`, `Terrains/`). 신규 기능 클래스가 필요하다는 점을 한 줄로 보고한 뒤 진행한다
- 효과가 `MoveGA`/`PlayCardGA` 외의 시점(웨이브 스폰, 턴 시작·종료 등)에 발동해야 한다면 `TerrainSystem` 리액션 추가가 필요하다 — 계약 변경이므로 차이를 먼저 설명하고 승인받는다
- "공격받으면 제거·N라운드 후 재생성", "웨이브 스폰 시 효과" 등 미구현 경로가 설명에 있을 때 — 미구현 후속 과제임(`08` §6)을 알리고 이번에 구현할지 묻는다
- 발판이 토큰처럼 칸을 점유해야 한다는 설명 — 발판은 토큰이 아니다 (점유하려면 장애물/중립지대)

**작성 규칙**
- 장애물: 새 `ObstacleView`/프리팹 코드는 만들지 않는다 (기존 `ObstacleView.prefab`과 `TokenCreator.obstaclePrefab`을 그대로 쓴다). 새 코드는 특성 조건/효과뿐이며 위치·규칙은 `trait_howto.md`를 따른다
- 발판: 기존 기능이면 코드 없음. 새 기능은 `TerrainEffect` 서브클래스만 만든다 (유닛별 참조형 상태가 있으면 `Clone()` 오버라이드). `TerrainSystem` 확장 시 `OnEnable`/`OnDisable`에서 Subscribe/Unsubscribe를 대칭으로 두고, 승인받은 범위만 고친다
- 구독/해제 대칭, 매직넘버 노출 `[SerializeField] private`, 파일 첫머리 한 줄 주석, 1파일 1클래스 — `convention.md` 기준
- `.meta` 파일은 Unity가 만들므로 직접 만들지 않는다

**에셋 생성 (클래스 컴파일이 끝난 뒤)**
1. 코드를 추가했으면 `refresh_unity`(compile 요청, 완료 대기) → `read_console`로 에러·경고 0 확인. 에러가 있으면 0이 될 때까지 수정한다.
2. 장애물 특성이 필요하면 `/trait` 커맨드 절차로 `1_Datas/3.Traits/<특성명>.asset`을 만든다 (Id 9001번대 등 기존 구간 규칙은 trait 커맨드가 따른다).
3. `execute_code`로 `ScriptableObject.CreateInstance`를 만들고 `SerializedObject`로 채운다:
   - 장애물: 위치 `Assets/Game/1_Datas/11.Obstacles/<이름>.asset`, 타입 `ObstacleData`. `<Id>k__BackingField`는 기존 에셋 Id를 Grep해 3001번대 최댓값 + 1(애매하면 묻는다), `<Description>`=사용자가 준 설명 문장, `<Health>`=설명의 체력, `<Damage>`=0, `<Speed>`=0, `<Traits>`에 특성 에셋 연결(없으면 비움). `Sprite`는 비워 두고 안내
   - 발판: 위치 `Assets/Game/1_Datas/12.Terrains/<이름>.asset`, 타입 `TerrainData`. `<Id>k__BackingField`는 기존 Id 최댓값 + 1. `<Description>`=사용자가 준 설명 문장. `<Effect>k__BackingField`는 `managedReferenceValue = new 기능클래스()`로 지정(수치 필드는 `FindPropertyRelative`로 쓰기 시도). `Sprite`는 비워 두고 안내
4. 저장 후 `.asset`을 Grep해 실제 반영을 확인한다. 반영되지 않았으면 성공한 척하지 말고 **인스펙터에서 설정할 값을 안내**한다.

**하지 말 것**
- 스테이지(`StageData`)에 배치하거나 씬·프리팹·인스펙터를 직접 고치지 않는다 — 에디터 작업이므로 클릭 순서(어느 `StageData`의 `ObstacleSpawns`/`TerrainSpawns`에 위치+데이터 추가, 중립지대·기본 배치 칸과 겹치지 않게)를 안내하고 멈춘다. 사용자가 `TestStage`에 넣어 달라고 명시한 경우에만 수정한다
- Play를 해본 것처럼 보고하지 않는다 — Play 확인 항목은 사용자에게 안내한다
- 요청 밖의 인접 코드·서식·다른 에셋을 건드리지 않는다. 기존 에셋을 덮어쓰지 않는다

**문서 갱신**
- `Docs/feature-spec/Terrain/terrain_catalog.md`에 이 항목 섹션을 추가한다 (구조 표 변경 없음, 요약 표 행 + 속성 표 + 연결 흐름 + 주의·한계 + 에디터 설정; 기존 돌격 화살통/풀숲 섹션이 견본). "현재 수록: N개" 문구도 갱신한다
- `trait.md` / `trait_catalog.md` 갱신은 `/trait` 절차를 따른다 (장애물 특성을 새로 만든 경우)
- `TerrainSystem`·`TerrainData`·`KillGA` 등 계약을 바꿨다면 `08_terrain_obstacle.md`도 갱신한다
- 그 외 문서의 낡은 서술은 고치지 말고 보고만 한다

**완료 보고**
- 종류·이름·`Id`·핵심 수치 (장애물: 체력/공격력/속도/특성, 발판: 효과)
- 재사용한 부품 / 새로 만든 코드 (있다면 구독 GA·타이밍)
- 만든 에셋 경로, 코드로 반영된 값과 인스펙터에서 마저 할 값 (`Sprite`, 스테이지 배치)
- 계약·시그니처 변경 여부 (있었다면 승인받은 내용)
- Console 결과 (에러·경고 수)
- 남긴 한계, 사용자가 Play에서 확인할 항목, 발견한 낡은 문서 서술
