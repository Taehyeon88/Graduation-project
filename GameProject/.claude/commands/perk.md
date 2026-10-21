---
description: 유물명 + 조건/효과 설명을 받아 PerkCondition·PerkEffect(없을 때만)와 PerkData 에셋을 만든다 (Docs/howto/perk_howto.md 기준, 재사용 우선, 애매하면 먼저 질문)
argument-hint: "[유물명, 조건+효과 설명 — 예: 검푸른 망토, 아군 깃발에 대한 적 용병의 공격이 빗나갑니다. 전투당 1번 발동됩니다.]"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Write
  - Edit
  - AskUserQuestion
  - mcp__UnityMCP__refresh_unity
  - mcp__UnityMCP__read_console
  - mcp__UnityMCP__manage_scriptable_object
disable-model-invocation: true
---

`Docs/howto/perk_howto.md`를 처음부터 끝까지 읽고 그 절차(조건 §5 → 효과 §6 → 에셋 §7 → 문서 §9 → 검증 §10)를 따라 유물 하나를 만든다. 절차 자체는 이 파일에 복제하지 않는다 — 문서가 기준이다. 아래는 이 커맨드 고유 규칙만 적는다.

**입력**
- `$ARGUMENTS`: 첫 쉼표 앞 = 유물명, 뒤 = 조건+효과 설명 (설명은 `Description`으로도 쓴다).
- 비어 있거나 "언제(조건)"와 "무엇을(효과)"을 구분할 수 없으면 코드를 쓰지 않고 `AskUserQuestion`으로 먼저 묻는다. 추측하지 않는다.

**먼저 읽기 (순서 고정)**
1. `Docs/feature-spec/Perk/perk_catalog.md` — 구조와 이미 만들어진 유물(조건·효과 조합). 설명과 기존 유물이 겹치는지 먼저 본다
2. `Docs/howto/perk_howto.md` 전체와 그 문서 §3 "사전 읽기" 목록 중 필요한 파일
3. `Assets/Game/0_Scripts/Perks/Conditions/`, `Perks/Effects/`를 Glob으로 확인하고 **재사용 후보 클래스는 파일을 직접 읽는다** — 카탈로그와 실제 파일이 어긋나면 어느 쪽이 낡았는지 먼저 알린다 (조용히 맞추지 않는다)
4. `Docs/convention.md` 중 관련 절, 그리고 구독·수정할 GA와 그 GA의 Performer

**분해와 재사용 판단**
- 설명을 "조건 / 효과"로 나누고, 각각 **재사용 / 수치만 다름(기존 클래스 + 인스펙터 값) / 신규** 중 무엇인지 정한다. `Perks/`에 없을 때만 새로 만든다.
- Trait 쪽 부품(`Traits/`)은 소유자 시그니처가 달라 Perk에 그대로 쓸 수 없다. 로직이 같아도 Perk용으로 새로 만들어야 하면 그 사실을 알린다.
- 신규 부품이 하나라도 있으면 한 줄로 요약해 보고한 뒤 진행한다.

**코드를 쓰기 전에 멈추고 `AskUserQuestion`으로 묻는 경우** (시스템·기획상 문제가 있거나 애매하면 바로 묻는다)
- 문장이 모호할 때 — 범위·대상(아군/적, 유닛/코어)·타이밍(PRE/POST)·수치·발동 횟수, 또는 원작 규칙을 모를 때. 추측하지 말고 선택지를 제시한다. 원작을 모르는 값은 `(원작 확인 필요)`로 표기하고 `[SerializeField]`/에셋으로 노출하는 임시값을 제안한다
- **재사용 가능성이 있을 때** — 기존 조건·효과와 완전히 같지는 않지만 쓸 수 있을 것 같으면(부분 겹침, 수치만 다름, 확장 vs 신규) 어느 쪽으로 갈지 묻는다
- 문장에 맞는 `GameAction`이 없어 새 GA가 필요할 때
- **GA에 플래그를 추가하거나 Performer를 수정해야 할 때**(`perk_howto.md` §6-2) — `convention.md` 6절 계약과의 차이를 먼저 설명하고 승인받는다. 기존 플래그(`AttackCoreGA.IsMissed` 등)로 되면 그것을 쓴다
- `PerkCondition`/`PerkEffect`/`Perk`/`PerkSystem` 시그니처를 바꿔야 할 때
- "전투당"·"턴당" 등 횟수 기준이 `TurnType.StartBattle`(현재 스테이지당 1회)과 다를 수 있을 때
- `Caster == null`(플레이어 스킬)·`HeroView`·`EnemyView` 등 판정 대상에 포함할지 불분명할 때
- 무효화 계열인데 모션을 먼저 재생해도 되는지 불분명할 때 (기본은 모션 재생 + 피해 생략)
- 효과가 예약하는 GA를 조건이 그대로 구독하는 조합일 때 (무한 루프)
- 같은 이름의 `PerkData` 에셋이 이미 있을 때 (덮어쓰지 않는다)
- `Id`를 정하기 애매할 때
- 원작에 이 유물이 있는지·원작 효과가 설명과 다른지 알 수 없을 때

**작성 규칙**
- 위치: 조건 `Assets/Game/0_Scripts/Perks/Conditions/`, 효과 `Assets/Game/0_Scripts/Perks/Effects/`. 1파일 1클래스, 파일명 = 클래스명 (`~Condition` / `~Effect`)
- 구독/해제는 같은 GA 타입을 대칭으로 등록·해제한다. 수치는 `[SerializeField] private`으로 노출하고, 조건·효과 인스턴스에 상태를 필드로 저장하지 않는다. 발동 횟수 제한은 효과가 아니라 `PerkData.MaxTriggersPerBattle`로 둔다
- Perk에는 소유자 필터가 없다 — `Caster`/`Target` 판정은 조건에서 직접 하고 `Caster is X` 패턴으로 null을 거른다
- 파일 첫머리에 의미를 한 줄 주석으로 적고, 권장 `reactionTiming`(PRE/POST)과 이유는 클래스 주석에 적는다
- 요청한 것만 만든다. 인접 코드·서식은 건드리지 않는다. `.meta` 파일은 Unity가 만들므로 직접 만들지 않는다

**PerkData 에셋 생성 (클래스 컴파일이 끝난 뒤)** — `perk_howto.md` §7의 검증된 절차를 따른다
1. `refresh_unity`(compile 요청, `mode: force`, 완료 대기) → `read_console`로 에러·경고 0 확인. 컴파일 직후 stale한 에러(`타입을 찾을 수 없음`)는 한 번 더 갱신 후 재확인한다. 에러가 있으면 0이 될 때까지 수정한다.
2. 위치 `Assets/Game/1_Datas/4.Perks/<유물 영문명>.asset`. `manage_scriptable_object create`(`type_name: PerkData`)와 `patches`로 `Id`·`Description`·`MaxTriggersPerBattle`을 지정한다. `Id`는 기존 에셋을 Grep해 최댓값 + 1이고 애매하면 묻는다.
3. `Condition`/`Effect`는 MCP patch가 안 되므로 `.asset`의 `references.RefIds`를 Edit으로 직접 쓴다 (`reactionTiming: 0`=PRE, `1`=POST). `refresh_unity`(`scope: assets`, `mode: force`) 후 `.asset`을 Grep해 `class`와 `reactionTiming`이 실제로 반영됐는지 확인한다. 반영되지 않으면 성공한 척하지 말고 인스펙터에서 설정할 값을 안내한다.
4. `Image`는 비워 두고 사용자에게 안내한다.

**하지 말 것**
- `GameSystem.Perks`에 등록하지 않는다 (씬 직렬화 수정 — 에디터 작업이므로 클릭 순서를 안내하고 멈춘다. 사용자가 명시적으로 요청하면 `perk_howto.md` §7-4로 한다. 이때 기존 목록을 덮어쓰지 않도록 기존 항목을 함께 넣는다)
- `PerkSystem` GameObject는 이미 배치되어 있다. 새로 만들지 않는다
- Play를 해본 것처럼 보고하지 않는다 — Play 확인이 필요한 항목은 사용자에게 안내한다
- 요청 밖의 인접 코드·다른 에셋을 건드리지 않는다. 기존 `PerkData`를 덮어쓰지 않는다
- `Traits/`, `TraitData` 등 Trait 쪽 코드를 수정하지 않는다

**문서 갱신**
- `Docs/feature-spec/Perk/perk_catalog.md`에 요약 표 행과 이 유물 섹션을 추가한다 (검푸른 망토 섹션이 견본). 수록 개수 문구도 갱신한다
- 새 조건/효과/GA 플래그를 만들었다면 카탈로그 섹션의 "코드"와 "주의·한계(계약 변경)"에 적는다
- 그 외 문서의 낡은 서술(옛 `Perk` 표기 등)은 고치지 말고 보고만 한다

**완료 보고**
- 재사용한 부품 / 새로 만든 부품 (구독 GA, 권장 `reactionTiming`)
- 만든 에셋 경로와 `Id`, 코드로 반영된 값과 인스펙터에서 마저 할 값 (`Image`, `GameSystem.Perks` 등록)
- 계약·시그니처·GA 플래그 변경 여부 (있었다면 승인받은 내용)
- Console 결과 (에러·경고 수)
- 남긴 한계, 사용자가 Play에서 확인할 항목, 발견한 낡은 문서 서술
