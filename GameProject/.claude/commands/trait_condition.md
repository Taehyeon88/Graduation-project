---
description: 조건 문장을 받아 TraitCondition 클래스를 Traits/Conditions/에 작성 (Docs/howto/trait_condition_howto.md 기준)
argument-hint: "[조건 문장 — 예: 본인 사망 시 / 손패 아군이 주변에 배치될 때]"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Write
  - Edit
  - AskUserQuestion
  - mcp__UnityMCP__refresh_unity
  - mcp__UnityMCP__read_console
disable-model-invocation: true
---

`Docs/howto/trait_condition_howto.md`를 처음부터 끝까지 읽고, 그 문서의 절차(3-1 ~ 3-7)를 그대로 따라 특성 조건(`TraitCondition`) 클래스를 만든다. 절차 자체는 이 파일에 복제하지 않는다 — 문서가 기준이다.

**입력**
- 만들 조건: `$ARGUMENTS`
- 비어 있으면 코드를 쓰지 않고 `AskUserQuestion`으로 어떤 조건을 만들지 먼저 묻는다. 추측하지 않는다.

**먼저 읽기**
- `Docs/howto/trait_condition_howto.md` 전체와 그 문서 2절 "사전 읽기" 목록의 파일
- `Docs/convention.md` 중 관련 절 (필드·프로퍼티, 메서드 구성, 인터페이스 계약)
- `Assets/Game/0_Scripts/Traits/Conditions/`의 기존 조건 목록 (Glob) — 같은 의미의 조건이 이미 있으면 새로 만들지 말고 재사용을 제안한다

**코드를 쓰기 전에 멈추고 `AskUserQuestion`으로 묻는 경우**
- 문장에 맞는 `GameAction`이 없어 새 GA가 필요할 때
- `TraitCondition`/`Trait`의 시그니처나 소유자 필터 규칙을 바꿔야 할 때 — 현재 계약과의 차이를 먼저 설명한다
- 문장이 모호할 때 (범위·대상·타이밍 등), 또는 원작 규칙을 모를 때 — 추측하지 말고 선택지를 제시한다. 원작을 모르는 값은 `(원작 확인 필요)`로 표기하고 `[SerializeField]`로 노출하는 임시값을 제안한다
- 기존 조건과 일부만 겹쳐서 확장할지 새로 만들지 판단이 필요할 때

**작성 규칙**
- 저장 위치: `Assets/Game/0_Scripts/Traits/Conditions/<클래스명>.cs` (1파일 1클래스, 파일명 = 클래스명, 이름은 `~Condition`)
- 구독/해제는 같은 GA 타입을 대칭으로 등록·해제한다. 수치는 `[SerializeField] private`으로 노출하고, 소유자·상태를 필드에 저장하지 않는다
- 파일 첫머리에 조건 의미를 한 줄 주석으로 적는다. 권장 `reactionTiming`(PRE/POST)과 이유는 클래스 주석에 적는다
- 요청한 조건만 만든다 — 효과(`TraitEffect`), `TraitData` 에셋, 인접 코드·서식은 건드리지 않는다
- `.meta` 파일은 Unity가 만들므로 직접 만들지 않는다

**하지 말 것**
- `TraitData` 에셋을 만들지 않는다 (인스펙터 설정 방법만 안내)
- Play를 해본 것처럼 보고하지 않는다 — Play 확인이 필요한 항목은 사용자에게 안내한다
- `05_trait_system.md` 표 갱신 외의 문서는 수정하지 않는다

**검증**
1. 파일 작성 후 `refresh_unity`(compile 요청, 완료 대기) → `read_console`로 에러·경고를 확인한다.
2. 에러가 있으면 바로 수정하고 0이 될 때까지 반복한다.

**문서 갱신**
- `Docs/feature-spec/05_trait_system.md`의 핵심 파일 표에서 조건 목록 칸에 새 클래스만 추가한다.
- `Folder.md`, `convention.md` 등 다른 문서에 낡은 서술이 있으면 고치지 말고 보고만 한다.

**완료 보고**
- 만든 클래스, 구독 GA, 권장 `reactionTiming`
- 계약·시그니처 변경 여부 (있었다면 승인받은 내용)
- Console 결과 (에러·경고 수)
- 남긴 한계와 사용자가 Play에서 확인할 항목, 에셋 인스펙터 설정 안내
