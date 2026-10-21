---
description: 스킬명 + 설명을 받아 PlayerSkill 구현체와 PlayerSkillData 에셋을 만든다. 시스템·기획상 문제나 애매한 점이 있으면 코드를 쓰기 전에 바로 질문한다 (Docs/howto/playerskill_howto.md 기준)
argument-hint: "[스킬명, 설명 — 예: 날카로운 단도, 적 용병 또는 우두머리를 지정해 2의 피해를 줍니다. 회피를 무시하며, 장애물은 지정할 수 없습니다. 횟수 제한 2]"
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
  - mcp__UnityMCP__find_gameobjects
  - mcp__UnityMCP__manage_components
disable-model-invocation: true
---

`Docs/howto/playerskill_howto.md`를 처음부터 끝까지 읽고 그 절차(분해 §3 → 스킬 클래스 §4 → 에셋 §5 → 연결 §6 → 문서 §7 → 검증 §8)를 따라 플레이어 스킬 하나를 만든다. 절차 자체는 이 파일에 복제하지 않는다 — 문서가 기준이다. 아래는 이 커맨드 고유 규칙만 적는다.

**입력**
- `$ARGUMENTS`: 첫 쉼표 앞 = 스킬명, 뒤 = 스킬 설명 (설명은 에셋 `Description`으로도 쓴다).
- 비어 있거나 "누구를 지정하는가(대상)"와 "무엇을 하는가(효과)"를 구분할 수 없으면 코드를 쓰지 않고 `AskUserQuestion`으로 먼저 묻는다. 추측하지 않는다.

**1. 먼저 읽기 (순서 고정, 코드·에셋을 만들기 전에 반드시 수행)**
1. `Docs/feature-spec/PlayerSkill/player_skill_catalog.md` — 기존 플레이어 스킬의 이름·대상·실행 GA·공통 규칙. 설명을 기존 스킬과 대조한다
2. `Docs/howto/playerskill_howto.md` 전체와 그 문서 §2 "사전 읽기" 중 필요한 파일
3. `Docs/feature-spec/07_player_skill.md`, `Docs/convention.md` 중 관련 절
4. 실제 코드와 대조 (문서만 믿지 않는다): `Assets/Game/0_Scripts/Models/PlayerSkill.cs`, `PlayerSkills/*.cs`(Glob), `1.Systems/PlayerSkillSystem.cs`, `UI/UseSkillUI.cs`, `1_Datas/10.PlayerSkills/*.asset`(Glob)
5. 문서와 코드가 어긋나면 어느 쪽이 낡았는지 먼저 알린다 (조용히 맞추지 않는다)

**2. 분류와 분해**

요청을 아래 넷 중 하나로 분류하고 근거(클래스·에셋·파일)와 함께 보고한다.

| 분류 | 처리 |
|---|---|
| **동일** (같은 스킬이 이미 있음) | 새로 만들지 않는다. 기존 스킬과 사용 방법만 안내하고 종료한다. 같은 이름의 클래스·에셋은 덮어쓰지 않는다 |
| **수치만 다름** | 기존 구현체 + 에셋 값 조정으로 되는지 설명하고, 사용자 확인을 받은 뒤에만 진행한다 |
| **부분 겹침** | 반드시 `AskUserQuestion` — 선택지: 기존 수정 / 신규 구현체. 임의로 정하지 않는다 |
| **신규** | 한 줄로 요약 보고하고 진행한다 |

설명을 **① 지정 가능한 대상 ② 실행할 GA ③ 특수 성질 ④ 수치(피해량·횟수 등)** 로 분해하고, 각각 기존 GA 재사용 / 신규 필요를 정한다 (howto §3).

**3. 코드를 쓰기 전에 멈추고 `AskUserQuestion`으로 반드시 묻는 경우**

시스템·기획상 문제가 있거나 애매하면 진행하지 말고 선택지를 제시한다. 질문은 한 번에 모아서 하되, 각 질문에 권장안을 첫 번째 선택지로 둔다. 원작 규칙을 모르는 값은 `(원작 확인 필요)`로 표기하고 `[SerializeField]`/SO로 노출하는 임시값을 제안한다.

*설명이 모호할 때*
- 대상이 불분명: 적 용병 / 아군 용병 / 적·아군 코어 / 빈 칸 / 자기 자신 / 범위 중 무엇인지
- "우두머리"·"코어"·"깃발"처럼 용어가 코드의 어떤 타입인지 애매할 때 (howto §3 매핑을 근거로 제시)
- 수치(피해량·횟수·범위·지속)가 빠졌거나 모호할 때
- 효과 문장이 여러 해석을 가질 때 (예: "회피를 무시" = 스택 소모 여부, "피해를 준다" = 힘·취약 보정 적용 여부)

*공통 규칙과 충돌할 때* (howto §1 — 공통 규칙은 `PlayerSkillSystem`이 처리한다)
- 설명이 AP 코스트가 1이 아니라고 하거나, 턴당 사용 제한이 "한 턴에 1번"과 다를 때 → 공통 상수/규칙 변경이므로 영향 범위를 설명하고 승인받는다
- 횟수 제한의 단위가 전투당이 아닐 때 (런당 등) → 초기화 위치와 저장 위치가 바뀐다
- 횟수 제한이 설명에 없을 때 → `MaxUses`를 어떻게 할지

*현재 구조가 받쳐 주지 못할 때* (모두 계약·구조 변경이므로 현재 계약과의 차이를 먼저 설명한다)
- **스킬을 하나 더 추가**하는 경우 (현재 `PlayerSkillSystem`·`UseSkillUI`는 스킬 1개 전제) — 선택지: 기존 스킬을 이 스킬로 교체 / 구조를 확장해 추가(스킬 목록·스킬별 횟수·버튼/선택 UI·"턴당 1회"가 전체 공유인지 스킬별인지). 임의로 확장하지 않는다
- 대상이 `Token`이 아닐 때 (빈 칸·좌표·범위 지정) — `PlayerSkill.IsValidTarget/CreateAction(Token)` 시그니처와 `UseSkillUI`의 대상 판정을 바꿔야 한다
- 대상이 그리드 밖 토큰일 때 (예: 용병대장 `HeroCaptainView`) — `UseSkillUI`의 판정이 `EnemyFlagView`만 별도 처리하므로 추가 판정이 필요하다
- 한 스킬이 GA를 **여러 개** 실행해야 할 때 (`CreateAction`은 GA 1개를 반환하고, 체이닝 경로는 검증되지 않았다)
- 문장에 맞는 `GameAction`이 없어 **새 GA**가 필요할 때 (`action_command.md` 갱신까지 이어진다)
- GA·Performer·`PlayerSkill`·`PlayerSkillSystem`의 시그니처나 SO 필드 형태를 바꿔야 할 때
- 플레이어 스킬은 **Caster가 null**이다 (howto §4-2). 사용할 GA·Performer·소비하는 조건이 null Caster를 안전하게 처리하지 않을 때, 또는 스킬이 시전자에 의존할 때(힘·방어·시전자 위치 기준 범위 등)
- 코어(`AttackCoreGA`) 피해가 필요한데 `DamageCaculator` 보정이 적용되지 않는 점이 설명과 어긋날 때
- 설명에 **지속·턴 경과·상태이상 부여**가 있어 현재 파이프라인(턴 종료 감소 훅 등)에 걸릴 때
- 플레이어 턴 외·액션 수행 중 사용 같은 사용 시점이 공통 `CanUse()`와 다를 때
- 열거형·메서드·파일을 지워야 할 때 — 참조 Grep 결과를 먼저 보여준다

*에셋·연결*
- 같은 이름·같은 `Id`의 `PlayerSkillData` 에셋이 이미 있을 때 (덮어쓰지 않는다)
- `Id`를 정하기 애매할 때
- 씬 `PlayerSkillSystem.Skill Data` 슬롯을 새 에셋으로 바꿀지 (기존 스킬이 교체되므로 에디터 작업 진행 전 확인)

**4. 작성 규칙**
- 위치 `Assets/Game/0_Scripts/PlayerSkills/`, 1파일 1클래스, 파일명 = 클래스명. `PlayerSkill`(`Models/`)을 상속해 `IsValidTarget`, `CreateAction`만 구현한다
- AP 코스트·턴당 1회·횟수 제한 로직은 스킬 클래스에 넣지 않는다 (`PlayerSkillSystem`/`MaxUses`가 처리)
- 수치는 `[SerializeField] private`으로 노출한다. 매직넘버 금지
- 코어 피해는 `new AttackCoreGA(null, amount)`, 유닛 피해는 `DealDamageGA`의 `Caster`에 null (howto §4-3 표를 따른다). 표에서 **미검증**인 패턴을 쓰게 되면 그 사실을 먼저 알린다
- 파일 첫머리에 스킬 의미를 한 줄 주석으로 적는다 (`SharpDagger` 견본)
- 요청한 스킬만 만든다. 안 쓸 유연성·설정·미래 대비 코드를 넣지 않는다. 멀쩡한 인접 코드·서식은 건드리지 않는다
- `.meta` 파일은 Unity가 만들므로 직접 만들지 않는다
- 구현은 단계마다 사용자에게 알리며 진행한다 (분류·설계 보고 → 코드 → 컴파일 → 에셋 → 문서)

**5. 에셋 생성 (코드 컴파일이 끝난 뒤)**
1. `refresh_unity`(`compile: request`, `mode: force`, 완료 대기) → `read_console`로 에러·경고 0 확인. 에러가 있으면 0이 될 때까지 수정한다. 컴파일 반영 전에는 `manage_scriptable_object`가 `type_not_found`를 내므로 한 번 더 읽고 확인한다
2. 위치 `Assets/Game/1_Datas/10.PlayerSkills/<스킬명>.asset`. `manage_scriptable_object create`(`type_name: PlayerSkillData`) 후 채운다:
   - `<Id>k__BackingField`: 기존 에셋 Id를 Grep해 최댓값 + 1. 애매하면 묻는다
   - `<Description>k__BackingField`: 사용자가 준 설명 문장
   - `<MaxUses>k__BackingField`: 설명의 횟수 제한 (없으면 3절에서 확인한 값)
   - `<Image>`는 비워 두고 안내한다
3. **`Skill` 필드는 MCP 패치로 설정할 수 없다** (ManagedReference). howto §5의 YAML 템플릿대로 `.asset`을 `Edit`로 직접 고친다 (`rid`, `type: {class: 구현체명, ns: , asm: Assembly-CSharp}`, `data:` 아래에 `[SerializeField]` 필드명과 값). 편집 뒤 `refresh_unity`(`scope: assets`, `mode: force`)로 다시 읽히게 하고 `.asset`을 Grep해 `rid`·`class`·수치가 실제 반영됐는지 확인한다. 반영되지 않으면 성공한 척하지 말고 인스펙터에서 설정할 값을 안내한다
4. 인스펙터 드롭다운에 구현체가 보이는지는 MCP로 확인할 수 없다 → 사용자에게 확인을 부탁한다 (안 보이면 구현체에 `[System.Serializable]`을 붙인다)

**6. 씬 연결 (사용자 확인을 받은 경우에만)**
1. 3절에서 "교체"로 확인된 경우에 한해 `find_gameobjects`(`by_component`, `PlayerSkillSystem`)로 객체를 찾고, 현재 `skillData` 값을 먼저 읽은 뒤 `manage_components set_property`로 새 에셋을 연결한다
2. 씬을 저장하지 않는다. 변경이 저장되지 않은 상태임을 알리고 사용자에게 Ctrl+S를 안내한다
- 확인을 받지 못했거나 거절되면 하지 않고, 클릭 순서(`--- SYSTEMS ---/PlayerSkillSystem` → `Skill Data` 슬롯)를 안내하고 멈춘다

**하지 말 것**
- 스킬을 두 개 이상 동시에 쓰도록 `PlayerSkillSystem`/`UseSkillUI`를 임의로 확장하지 않는다 (3절 질문 후 승인받은 경우만)
- 하이라이트·남은 횟수 텍스트·사운드·연출 같은 요청 밖 UI/아트를 추가하지 않는다 (프로젝트 범위 제외)
- Play를 해본 것처럼 보고하지 않는다 — 드래그 판정·피해·코어 처리는 사용자가 Play에서 확인할 항목으로 안내한다
- 기존 에셋·인접 코드·서식·다른 문서의 낡은 서술을 조용히 고치지 않는다 (보고만)

**문서 갱신**
- `Docs/feature-spec/PlayerSkill/player_skill_catalog.md` — 요약 표에 행 추가 + 스킬 섹션 추가 (날카로운 단도 섹션이 견본), 상단의 "현재 수록" 개수 문구 갱신
- `Docs/feature-spec/07_player_skill.md` — 규칙·구성 표·"기존 코드에 가한 변경"·알려진 제약에 반영
- 새 폴더·에셋 폴더가 생겼으면 `Docs/Folder.md`, 새 GA를 만들었으면 `Docs/feature-spec/action/action_command.md`, 새 문서를 만들었으면 `Docs/feature-spec.md` 목차
- 그 외 문서의 낡은 서술은 고치지 말고 보고만 한다

**완료 보고**
- 분류(동일/수치만 다름/겹침/신규)와 근거, 3절에서 사용자가 정한 사항
- 만든 구현체 클래스·사용한 GA·지정 가능한 대상
- 계약·시그니처 변경 여부 (있었다면 승인받은 내용과 갱신한 문서)
- 에셋 경로와 `Id`, 코드로 반영된 값과 인스펙터에서 마저 할 값 (`Image`, 드롭다운 확인), 씬 연결 여부 (저장 필요 안내)
- Console 결과 (에러·경고 수)
- 남긴 한계, `(원작 확인 필요)` 항목, 사용자가 Play에서 확인할 항목, 발견한 낡은 문서 서술
