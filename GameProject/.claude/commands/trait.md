---
description: 특성명 + 조건/효과 설명을 받아 TraitCondition·TraitEffect(없을 때만)와 TraitData 에셋을 만든다 (Docs/howto/trait_howto.md 기준, 재사용 우선)
argument-hint: "[특성명, 조건+효과 설명 — 예: 불경한 빛, 사망 시 자신의 속도만큼 모든 적에게 피해]"
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

`Docs/howto/trait_howto.md`를 처음부터 끝까지 읽고 그 절차(조건 §3 → 효과 §4 → 에셋 §5 → 카탈로그 §6 → 검증 §7)를 따라 특성 하나를 만든다. 절차 자체는 이 파일에 복제하지 않는다 — 문서가 기준이다. 아래는 이 커맨드 고유 규칙만 적는다.

**입력**
- `$ARGUMENTS`: 첫 쉼표 앞 = 특성명, 뒤 = 조건+효과 설명 (설명은 `Description`으로도 쓴다).
- 비어 있거나 "언제(조건)"와 "무엇을(효과)"을 구분할 수 없으면 코드를 쓰지 않고 `AskUserQuestion`으로 먼저 묻는다. 추측하지 않는다.

**먼저 읽기 (순서 고정)**
1. `Docs/feature-spec/Trait/trait.md` — 재사용 가능한 조건·효과 레지스트리. **항상 가장 먼저** 읽고 설명을 기존 부품과 대조한다. 재사용을 우선한다 (판단 규칙은 그 문서 §1).
2. `Docs/howto/trait_howto.md` 전체와 그 문서 §2 "사전 읽기" 목록 중 필요한 파일
3. `Docs/convention.md` 중 관련 절
4. `Assets/Game/0_Scripts/Traits/`(`Conditions/` 포함)를 Glob으로 확인 — `trait.md`와 실제 파일이 어긋나면 어느 쪽이 낡았는지 먼저 알린다 (조용히 맞추지 않는다)

**분해와 확인**
- 설명을 "조건 / 효과"로 나누고, 각각 **재사용 / 수치만 다름(기존 클래스 + 인스펙터 값) / 신규** 중 무엇인지 정한다.
- 신규 부품이 하나라도 있으면 한 줄로 요약해 보고한 뒤 진행한다.

**코드를 쓰기 전에 멈추고 `AskUserQuestion`으로 묻는 경우**
- 문장이 모호할 때 (범위·대상·타이밍·수치), 또는 원작 규칙을 모를 때 — 추측하지 말고 선택지를 제시한다. 원작을 모르는 값은 `(원작 확인 필요)`로 표기하고 `[SerializeField]`로 노출하는 임시값을 제안한다
- 문장에 맞는 `GameAction`이 없어 새 GA가 필요할 때
- GA, `TraitCondition`, `TraitEffect`, `Trait`의 시그니처·소유자 필터를 바꿔야 할 때 — 현재 계약과의 차이를 먼저 설명한다
- 기존 부품과 일부만 겹칠 때 (확장 vs 신규 판단)
- 효과가 아직 쓸 수 없는 부품에 의존할 때 (`trait.md` §5, 예: `ModifyStatGA`는 Performer 없음)
- 효과가 예약하는 GA를 조건이 그대로 구독하는 조합일 때 (무한 루프)
- 같은 이름의 `TraitData` 에셋이 이미 있을 때 (덮어쓰지 않는다)
- `Id`를 정하기 애매할 때

**작성 규칙**
- 없는 조건/효과만 새로 만든다. 위치: 조건 `Assets/Game/0_Scripts/Traits/Conditions/`, 효과 `Assets/Game/0_Scripts/Traits/`. 1파일 1클래스, 파일명 = 클래스명 (`~Condition` / `~Effect`)
- 구독/해제는 같은 GA 타입을 대칭으로 등록·해제한다. 수치는 `[SerializeField] private`으로 노출하고, 조건·효과 인스턴스에 소유자·상태를 필드로 저장하지 않는다
- 파일 첫머리에 의미를 한 줄 주석으로 적고, 권장 `reactionTiming`(PRE/POST)과 이유는 클래스 주석에 적는다
- `.meta` 파일은 Unity가 만들므로 직접 만들지 않는다

**TraitData 에셋 생성 (클래스 컴파일이 끝난 뒤)**
1. `refresh_unity`(compile 요청, 완료 대기) → `read_console`로 에러·경고 0 확인. 에러가 있으면 0이 될 때까지 수정한다.
2. 위치 `Assets/Game/1_Datas/3.Traits/<특성명>.asset`. `execute_code`로 `ScriptableObject.CreateInstance<TraitData>()`를 만들고 `SerializedObject`로 채운다:
   - `<Id>k__BackingField`: 기존 에셋 Id를 Grep해 같은 구간(3001~)의 최댓값 + 1. 애매하면 묻는다
   - `<Description>k__BackingField`: 사용자가 준 설명 문장
   - `<Condition>k__BackingField` / `<Effect>k__BackingField`: `managedReferenceValue = new 클래스()`
   - `ConditionName` / `EffectName`: 연결한 클래스명 (작업용 메모)
   - `reactionTiming`·수치 필드: 같은 방식으로 쓰기를 시도하고, 저장 후 `.asset`을 Grep해 실제 반영을 확인한다. 반영되지 않으면 성공한 척하지 말고 **인스펙터에서 설정할 값을 안내**한다
3. `Image`는 비워 두고 사용자에게 안내한다. 파일명은 최근 에셋 관례(이름만)를 따르되 `번호_이름` 규칙과의 불일치는 보고한다.

**하지 말 것**
- 영웅/몬스터 `CombatantData.Traits`에 등록하지 않는다 (에디터 작업 — 클릭 순서를 안내하고 멈춘다)
- Play를 해본 것처럼 보고하지 않는다 — Play 확인이 필요한 항목은 사용자에게 안내한다
- 요청 밖의 인접 코드·서식·다른 에셋을 건드리지 않는다. 기존 `TraitData`를 덮어쓰지 않는다

**문서 갱신**
- 새 조건/효과를 만들었으면 `Docs/feature-spec/Trait/trait.md`의 표에 행을 추가하고, 재사용했으면 해당 행의 "사용 특성" 열에 이 특성을 추가한다
- `Docs/feature-spec/Trait/trait_catalog.md`에 이 특성 섹션을 추가한다 (불경한 빛 섹션이 견본)
- 새 클래스를 만들었으면 `Docs/feature-spec/05_trait_system.md` 핵심 파일 표의 해당 목록에 클래스만 추가한다
- 그 외 문서의 낡은 서술은 고치지 말고 보고만 한다

**완료 보고**
- 재사용한 부품 / 새로 만든 부품 (구독 GA·예약 GA, 권장 `reactionTiming`)
- 만든 에셋 경로와 `Id`, 코드로 반영된 값과 인스펙터에서 마저 할 값 (`Image`, 영웅 등록 등)
- 계약·시그니처 변경 여부 (있었다면 승인받은 내용)
- Console 결과 (에러·경고 수)
- 남긴 한계, 사용자가 Play에서 확인할 항목, 발견한 낡은 문서 서술
