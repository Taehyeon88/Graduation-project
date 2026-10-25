---
description: 소문 번호+이름+효과 설명을 받아 Rumor 구체 클래스(없을 때만)와 RumorData 에셋을 만든다 (Docs/howto/rumor_howto.md 기준)
argument-hint: "[번호 이름, 효과 설명 — 예: 3 숙달된, 전장에 있는 동안 매 라운드 종료 시 행동력+1개를 추가로 획득합니다.]"
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
  - mcp__UnityMCP__manage_asset
  - mcp__UnityMCP__manage_scriptable_object
disable-model-invocation: true
---

`Docs/howto/rumor_howto.md`를 처음부터 끝까지 읽고 그 절차(분류 §3 → 클래스 §4 → 에셋 §5 → 문서 §6 → 검증 §7)를 따라 소문 하나를 만든다. 절차 자체는 이 파일에 복제하지 않는다 — 문서가 기준이다. 아래는 이 커맨드 고유 규칙만 적는다.

**입력**
- `$ARGUMENTS`: 첫 쉼표 앞 = `번호 이름` (공백으로 구분, 번호는 `Id`와 파일명 `번호_이름`에 같이 쓴다), 뒤 = 효과 설명 (`Description`으로도 쓴다. 문장은 사용자가 준 그대로 쓴다).
- 번호가 없거나, 효과 설명이 비어 있거나, "언제/무엇을"을 구분할 수 없으면 코드를 쓰지 않고 `AskUserQuestion`으로 먼저 묻는다. 추측하지 않는다.
- 같은 번호나 같은 이름의 `RumorData`가 `Assets/Game/1_Datas/14.Rumors/`에 이미 있으면 덮어쓰지 않고 묻는다.

**진행 방식**: 모호하거나 계약을 바꿔야 할 때만 멈추고, 그 외에는 클래스 → 에셋 → 문서 → 검증까지 한 번에 끝낸다.

**먼저 읽기 (순서 고정)**
1. `Docs/feature-spec/Rumor/rumor.md` — 구조·계약과 §3 구체 소문 표. **항상 가장 먼저** 읽고 설명을 기존 소문과 대조한다. 같은 클래스에 수치만 다르면 새 클래스를 만들지 않는다.
2. `Docs/howto/rumor_howto.md` 전체와 그 문서 §2 "사전 읽기" 목록 중 필요한 파일
3. `Docs/convention.md` 중 관련 절
4. `Assets/Game/0_Scripts/Rumors/`를 Glob으로 확인 — `rumor.md`·코드가 어긋나면 어느 쪽이 낡았는지 먼저 알린다 (조용히 맞추지 않는다)

**분해와 확인**
- 설명을 §3 분류(스탯 보정 / 배치 AP 보정 / 이벤트 반응 / 그 외)로 나누고, 각각 **재사용 / 수치만 다름 / 신규**를 정한다. 한 소문이 여러 종류를 겸할 수 있다.
- 신규 부품이 있으면 한 줄로 요약해 보고한 뒤 진행한다.

**코드를 쓰기 전에 멈추고 `AskUserQuestion`으로 묻는 경우**
- 문장이 모호할 때 (범위·대상·타이밍·수치, 예: "라운드 종료"의 정의), 또는 원작 규칙을 모를 때 — 선택지를 제시한다. 원작을 모르는 값은 `(원작 확인 필요)`로 표기하고 `[SerializeField]`로 노출하는 임시값을 제안한다
- 문장에 맞는 `GameAction`이 없어 새 GA가 필요할 때
- `Rumor`·`Hero`·`HeroView`·`Card`·`CombatantView.OnDisable` 등 **기존 코드·계약을 바꿔야 할 때** (예: 새 보정 기능이라 새 인터페이스 `I~Modifier`가 필요할 때) — 현재 계약과의 차이를 먼저 설명하고 승인받는다
- 추상 클래스 `Rumor`에 멤버를 추가하고 싶어질 때 — **추상 클래스에는 소문 개별 데이터·기능을 넣지 않는다**. 인터페이스로 분리할 수 있는지 먼저 본다
- 기존 소문과 일부만 겹칠 때 (확장 vs 신규)
- 구독형 소문이 자기가 예약한 GA를 그대로 구독하는 조합일 때 (무한 루프)

**작성 규칙**
- 위치 `Assets/Game/0_Scripts/Rumors/`, 1파일 1클래스, 파일명 = 클래스명 (`~Rumor`). 파일 첫머리에 `//번호 이름: 의미` 한 줄 주석
- `Rumor` 상속. 수치는 `[SerializeField] private`으로 기본값과 함께 노출한다 (매직넘버 금지)
- 구독형은 `OnAdd`에서 등록, `OnRemove`에서 같은 GA·타이밍으로 대칭 해제한다. 소유자 필터가 없으므로 owner 판정은 소문이 직접 한다
- 요청 밖의 인접 코드·서식·다른 소문을 건드리지 않는다. `.meta`는 직접 만들지 않는다

**RumorData 에셋 생성 (클래스 컴파일이 끝난 뒤)**
1. `refresh_unity` → 컴파일이 끝날 때까지 기다린 뒤 `read_console`로 에러 0 확인. 에러가 있으면 0이 될 때까지 수정한다.
2. 위치 `Assets/Game/1_Datas/14.Rumors/<번호>_<이름>.asset` (폴더가 없으면 `manage_asset` `create_folder`).
3. `manage_scriptable_object`(`create`, `type_name`=`RumorData`)의 `patches`로 `<Id>k__BackingField`(번호)와 `<Description>k__BackingField`(설명)를 `set`한다.
4. `Rumor` 필드는 `[SerializeReference]`라 patches로 안 된다. `execute_code`(`action`=`execute`, 로컬 함수 사용 금지)에서 `new SerializedObject(asset).FindProperty("<Rumor>k__BackingField").managedReferenceValue = new 클래스()` → `ApplyModifiedPropertiesWithoutUndo` → `SetDirty` → `AssetDatabase.SaveAssets` 후 `asset.Rumor.GetType().Name`을 반환해 확인한다. 반영이 안 되면 성공한 척하지 말고 인스펙터 설정값을 안내한다.
5. `Image`는 비워 두고 사용자에게 안내한다.

**하지 말 것**
- `Hero.AddRumor`를 부르는 획득 경로(보상·상점 등)를 임의로 만들지 않는다 — 별도 설계라 안내만 한다. `HeroData`에는 소문 필드를 넣지 않는다
- Play를 해본 것처럼 보고하지 않는다 — 확인 항목은 사용자에게 안내한다
- 에디터 작업(`Image` 연결, Play 테스트)은 클릭 순서를 안내하고 멈춘다

**문서 갱신**
- `Docs/feature-spec/Rumor/rumor_catalog.md`에 이 소문 섹션을 추가하고 상단 "현재 수록" 개수를 고친다 (1 강력한·3 숙달된 섹션이 견본)
- 새 소문 클래스를 만들었으면 `Docs/feature-spec/Rumor/rumor.md` §3 표에 행을 추가하고, 재사용했으면 "사용 소문" 열에 추가한다. 새 인터페이스를 만들었으면 §1 표에도 추가한다
- 그 외 문서(`Folder.md` 등)의 낡은 서술은 고치지 말고 보고만 한다

**완료 보고**
- 재사용한 클래스 / 새로 만든 클래스·인터페이스 (구독 GA·예약 GA, 타이밍)
- 만든 에셋 경로와 `Id`, 코드로 반영된 값과 인스펙터에서 마저 할 값 (`Image`)
- 기존 코드·계약 변경 여부 (있었다면 승인받은 내용과 갱신한 문서)
- Console 결과 (에러·경고 수)
- 남긴 한계, 사용자가 Play에서 확인할 항목 (획득 경로가 없어 임시 `AddRumor` 호출이 필요함), 발견한 낡은 문서 서술
