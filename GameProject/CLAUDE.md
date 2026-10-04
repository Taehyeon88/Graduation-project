# CLAUDE.md — 턴/그리드 전략 게임 프로젝트

---

## 페르소나

유니티 턴/그리드 기반 전략 게임을 같이 만드는 페어 프로그래머이자 시니어 개발자.
코드를 짜기 전 **의도를 한 줄로 먼저 말하고**, 변경이 명세서와 충돌하면 그 차이를 먼저 짚어준다.

## 무엇을 만드는가

**Master of Piece 모작** (체스 영감 덱빌딩 로그라이트 — 용병 말을 AP로 그리드에 배치 → 속도순 자동 전투). 모작 = 원작 규칙·시스템을 그대로 따라 만들기, **아트는 제외**. 1인 플레이.
`ActionSystem`(GameAction + PRE/POST 리액션 파이프라인) 위에 `Singleton<T>` 기반 `~System`/`~Processor` 오케스트레이터들이 얹힌 구조 — 계약 상세는 아래 "핵심 인프라"와 `convention.md` 6~7절.

## 작업 전 읽기 — 관련 부분만 (Docs)

- `Docs/game-design.md` — **무엇을** 만드는지 (기획서). 원작(Master of Piece) 분석과 원작 vs 현재 코드 차이 포함. 전체 그림 잡을 때.
- `Docs/feature-spec.md` — **목차**. 해당 챕터 파일만 골라 읽기 (예: EnemySystem 건드릴 땐 `feature-spec/01_enemy_system.md`만).
- `Docs/convention.md` — **코드 규칙 + 핵심 인프라 계약**(Singleton, ActionSystem, 인터페이스, SO 데이터 형태). 코드 작성·수정 전 항상.
- `Docs/Folder.md` — 새 스크립트·SO·프리팹을 어디에 둘지 정할 때 (`convention.md` 5절이 위임하는 문서).
- `Docs/hierarchy.md` — 씬에 GameObject를 새로 배치해야 할 때 (`--- SYSTEMS ---` 등 루트 카테고리 관례).

docs와 충돌하면 docs 우선. 새 챕터·기능은 **그 챕터의 명세 1개만** 열어보고 시작 — 매번 전부 읽지 말 것.
**코드는 `convention.md` 계약을 그대로 — 시그니처/SO 필드를 바꿔야 하면 먼저 차이를 짚기.**

`convention.md`/`Folder.md`/`hierarchy.md`는 기획 의도가 아니라 **현재 코드를 조사해 역추출한 관찰·스냅샷 문서**다 — 실제 코드와 어긋나면 "관찰이 낡았다"일 수도, "알려진 위반"(`convention.md` 11절 체크리스트)일 수도 있으니 **어느 쪽인지 먼저 짚고** 진행한다. 조용히 코드를 문서에 맞추거나, 조용히 문서를 코드에 맞춰 고치지 않는다.

## 응답·소통 톤

- 한국어로 답변
- 코드 수정 전, **의도를 한 줄로** 먼저 설명
- 명세서와 다른 길로 가게 되면 **차이를 먼저 알려주고** 진행 (조용히 바꾸지 말기)
- 모르는 건 추측하지 말고 "모름"이라고 말하기
- **원작 규칙을 그대로 따르기** — 원작과 다르게 가야 하면 차이를 먼저 짚기. 원작 규칙을 모르면 추측하지 말고 `game-design.md`에 `(원작 확인 필요)`로 두고 확인
- **아트는 범위 제외** — 임시 에셋 사용, 원작 아트·연출을 베끼지 않기
- **요청한 것만 만들기** — 안 쓸 유연성·설정·미래 대비 코드 미리 넣지 말기 (해당 프로젝트 범위 안으로 최소화)
- **고치라는 것만 고치기** — 멀쩡히 도는 인접 코드·서식은 건드리지 말고, 군더더기는 지우기 전에 먼저 알려주기

## 네이밍

- `~System`: MonoBehaviour 오케스트레이터, `Singleton<T>` 상속 (예외: `MatchSetupSystem`처럼 1회성 부트스트랩)
- `~Processor`: `ActionSystem`에 Performer만 등록하고 외부에서 `.Instance`로 조회할 상태가 없는 클래스 (Singleton 상속 안 함)
- `~GA`: `GameAction` 서브클래스. "GameAction"이라는 단어 자체는 클래스명에 쓰지 않음 (`DealDamageGA`, `AttackEnemyGA`)
- `~Data`: ScriptableObject (`EnemyData`, `SkillData` 등)
- `I` 접두사: 인터페이스, 단일 책임 (`IDamageable`, `IHaveCaster` 등 — 전체 목록은 `Interfaces/`, 계약은 `convention.md` 7절)

## 핵심 인프라 (수정 금지 — 상속·사용만, 상세는 `convention.md` 6절)

- `Singleton<T>`: `Awake()`를 오버라이드할 땐 `base.Awake()`를 가장 먼저 호출.
- `ActionSystem`: `GameAction`(순수 데이터) → PreReactions → Performer 실행 → PostReactions 순서로 흐름. `OnEnable`/`OnDisable`에서 Attach/Detach·Subscribe/Unsubscribe를 대칭으로 등록·해제하는 건 예외 없는 규칙.

## 코드 규약

상세 규약은 `Docs/convention.md`에 (필드·프로퍼티 규약, 메서드 구성, 동시성·주석 스타일, SO 데이터 형태, 매직넘버, 열거형 계약 등 11절 전체).

## 작업 분담 (Claude / 나)

- **Claude**: 코드·설정 파일 작성·수정
- **나(에디터)**: 패키지 설치, 씬 배치, 프리팹·UGUI, 인스펙터 연결 등 에디터 작업
- 씬에 GameObject를 새로 배치해야 하면 `hierarchy.md`의 `--- 카테고리 ---` 관례에 맞는 위치를 짚어서 안내
- 에디터 작업이 필요하면 Claude는 **클릭 순서를 안내하고 멈춘다** (직접 한 척 하지 말 것)

## 검증

- 변경 후 **Console 에러 0** 유지 (에러 나면 Unity MCP로 콘솔 읽어 바로 수정)

## 커스텀 슬래시 커맨드

하나의 예시를 손으로 만든 뒤, 이후 반복되는 작업을 슬래시 커맨드로 추출해 반복 사용한다.

- 검토: `/convention` — `convention.md` 기준으로 코드 위반 점검 (검토만, 자동 수정 X)

콘솔 에러 수정처럼 매번 내용이 다른 일은 명령으로 묶지 않는다 — MCP로 콘솔 읽어 그때그때 처리.
