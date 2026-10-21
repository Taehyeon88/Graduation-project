---
description: 상태효과명 + 설명을 받아 기존 상태효과와 중복인지 먼저 확인한 뒤, 없을 때만 StatusEffectType·판정 로직·StatusEffectData 에셋을 만든다 (Docs/howto/statuseffect_howto.md 기준, 재사용 우선)
argument-hint: "[상태효과명, 효과 설명 — 예: 회피, 이 용병을 대상으로 하는 다음 1회의 모든 공격과 피해가 빗나갑니다]"
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
  - mcp__UnityMCP__manage_scene
disable-model-invocation: true
---

`Docs/howto/statuseffect_howto.md`를 처음부터 끝까지 읽고 그 절차(설계 §3 → 코드 §4 → 에셋 §5 → 문서 §6 → 검증 §7)를 따라 상태효과 하나를 만든다. 절차 자체는 이 파일에 복제하지 않는다 — 문서가 기준이다. 아래는 이 커맨드 고유 규칙만 적는다.

**입력**
- `$ARGUMENTS`: 첫 쉼표 앞 = 상태효과명, 뒤 = 효과 설명 (설명은 에셋 `Discription`으로도 쓴다).
- 비어 있거나 "스택의 의미 / 지속·감소 / 무엇에 작용하는지"를 구분할 수 없으면 코드를 쓰지 않고 `AskUserQuestion`으로 먼저 묻는다. 추측하지 않는다.

**1. 필수 중복 체크 (가장 먼저, 코드·에셋을 만들기 전에 반드시 수행 — 생략 금지)**

읽기 순서 고정:
1. `Docs/feature-spec/StatusEffect/status_effect_catalog.md` — 기존 상태효과의 이름·타입·설명·판정 위치
2. `Docs/howto/statuseffect_howto.md` 전체와 그 문서 §2 "사전 읽기" 중 필요한 파일
3. `Docs/convention.md` 중 관련 절

실제 코드와 대조 (문서만 믿지 않는다):
- `Assets/Game/0_Scripts/Enums/StatusEffectType.cs`
- `Assets/Game/1_Datas/6.StatusEffects/*.asset` (Glob 후 `EffectType`/`Discription` Grep)
- `General/Util/DamageCaculator.cs`, `1.Systems/Skills/DamageSystem.cs`, `3.Views/CombatantView.cs`의 상태효과 사용처
- 문서와 코드가 어긋나면 어느 쪽이 낡았는지 먼저 알린다 (조용히 맞추지 않는다)

요청을 아래 넷 중 하나로 분류하고, 근거(타입·파일)와 함께 사용자에게 보고한다.

| 분류 | 처리 |
|---|---|
| **동일** (같은 효과가 이미 있음) | 새로 만들지 않는다. 기존 타입과 부여 방법(카드 `AddStatusEffectEffect` / 특성 `AddStatusTraitEffect`)만 안내하고 종료한다. 같은 이름·`EffectType`의 에셋은 덮어쓰지 않는다 |
| **수치·부여만 다름** | 코드 추가 없이 기존 타입 + 스택 수/부여 수단으로 해결 가능한지 설명하고, 사용자 확인을 받은 뒤에만 진행한다 |
| **부분 겹침 (걸침)** | 반드시 `AskUserQuestion` — 선택지: 기존 재사용 / 기존 확장 / 신규 타입. 임의로 정하지 않는다 |
| **신규** | 한 줄로 요약 보고하고 설계(§3)로 진행한다 |

특히 다음은 임의 판단하지 말고 묻는다:
- `WEAK`/`VULNERABLE`처럼 **타입은 있으나 에셋이 없는** 효과와 겹치는 요청
- 취약·약화의 이름과 효과 방향이 뒤바뀐 듯한 불일치(원작 확인 필요)에 걸리는 요청

**2. 코드를 쓰기 전에 멈추고 `AskUserQuestion`으로 반드시 다시 묻는 경우**

애매하거나 문제가 있으면 진행하지 말고 선택지를 제시한다. 원작 규칙을 모르는 값은 `(원작 확인 필요)`로 표기하고 에셋/`[SerializeField]`로 노출하는 임시값을 제안한다.

- 문장이 모호할 때: 스택 의미(수치/횟수/유무), 지속·감소 규칙(무기한/N턴/발동 시 소모), 대상 범위, 수치
- 기존 상태효과와 일부만 겹칠 때 (위 "부분 겹침")
- 판정 지점이 불분명하거나 howto §3-2 표의 어느 행에도 맞지 않을 때
- **구현에 문제가 있을 때**:
  - 지속형인데 용병에는 턴 종료 감소 훅이 없을 때 (영웅만 `HeroSystem`이 호출)
  - 피해를 막는 효과에서 `DealDamageGA`를 구독한 특성이 막힌 뒤에도 발동하는 문제 (반응 유지 vs 취소 — 취소는 `ActionSystem` 계약 변경)
  - 새 `GameAction`·시스템·UI·훅이 필요할 때
  - `Singleton`/`ActionSystem`/GA 시그니처/SO 필드 형태 등 계약을 바꿔야 할 때 — 현재 계약과의 차이를 먼저 설명한다
  - 효과가 `DamageCaculator`를 거치지 않는 경로(예: `AttackCoreGA`)에 걸릴 때
  - 열거형·메서드·파일을 지워야 할 때 — 참조 Grep 결과를 먼저 보여준다
- 같은 이름·`EffectType`의 `StatusEffectData` 에셋이 이미 있을 때 (덮어쓰지 않는다)
- 씬 `StatusEffectSystem.effectDatas`에 등록할지 (에디터 작업이므로 진행 전 확인)

**작성 규칙**
- `StatusEffectType`은 `StatusEffectType.cs`의 **끝에** 추가하고 한 줄 주석을 단다 (에셋에 정수로 직렬화되므로 기존 값 유지)
- 판정 위치는 howto §3-2 표를 따른다. 피해·피격 연출을 막는 효과는 `DamageSystem.DealDamageGAPerformer`에서 `GetDamage`/`PlayDamageVFX` **이전**에, `Targets` 루프와 `Target` 단일 분기 **두 곳 모두**에 둔다
- 사용자가 원치 않으면 새 메서드를 만들지 않고 호출 지점에 인라인한다
- 요청한 효과만 만든다. 안 쓸 유연성·설정·미래 대비 코드를 넣지 않는다. 멀쩡한 인접 코드·서식은 건드리지 않는다
- 비UTF-8 파일은 Edit/Write가 거부한다 — 삭제만 필요하면 shell로 `.cs`와 `.meta`를 함께 지운다
- `.meta` 파일은 Unity가 만들므로 직접 만들지 않는다

**에셋 생성 (코드 컴파일이 끝난 뒤)**
1. `refresh_unity`(compile 요청, 완료 대기) → `read_console`로 에러·경고 0 확인. 에러가 있으면 0이 될 때까지 수정한다.
2. 위치 `Assets/Game/1_Datas/6.StatusEffects/<상태효과명>.asset`. `manage_scriptable_object`의 `create`(`StatusEffectData`) 후 `modify`로 채운다:
   - `<EffectType>k__BackingField`: 새 열거형의 정수 값
   - `<Discription>k__BackingField`: 사용자가 준 설명 문장
   - `<spriteImage>k__BackingField`: `Assets/Game/4_Medias/StatusEffects/`에 같은 이름의 이미지가 있으면 `ref.path`로 연결, 없으면 비워 두고 안내
3. 저장 후 `.asset`을 Grep해 실제 반영을 확인한다. 반영되지 않으면 성공한 척하지 말고 인스펙터에서 설정할 값을 안내한다.

**씬 등록 (사용자 확인을 받은 경우에만)**
1. `find_gameobjects`(`by_component`, `StatusEffectSystem`)로 객체를 찾는다.
2. 현재 `effectDatas` 배열을 **먼저 읽고**, 기존 항목 + 신규 에셋을 모두 포함해 `manage_components set_property`로 쓴다 (배열을 통째로 덮어쓴다).
3. `manage_scene save` 후 `git diff`로 추가분만 바뀌었는지 확인한다.
- 확인을 받지 못했거나 거절되면 하지 않고, 클릭 순서를 안내하고 멈춘다.

**하지 말 것**
- 부여 수단(카드·특성)을 만들거나 연결하지 않는다 — 방법만 안내한다
- Play를 해본 것처럼 보고하지 않는다 — Play 확인이 필요한 항목은 사용자에게 안내한다
- 기존 에셋·인접 코드·서식·다른 문서의 낡은 서술을 조용히 고치지 않는다 (보고만)
- 아트(연출·사운드·"MISS" 표시)를 추가하지 않는다 (프로젝트 범위 제외)

**문서 갱신**
- `Docs/feature-spec/StatusEffect/status_effect_catalog.md` — 요약 표에 행 추가 + 상태효과 섹션 추가 (회피 섹션이 견본), 상단의 "현재 수록" 개수 문구 갱신
- `Docs/feature-spec/StatusEffect.md` — 요약 표에 행 + 상세 섹션
- 열거형·메서드를 지웠다면 `Docs/convention.md`의 관련 언급
- 새 문서를 만들었다면 `Docs/feature-spec.md` 목차
- 그 외 문서의 낡은 서술은 고치지 말고 보고만 한다

**완료 보고**
- 중복 체크 분류(동일/수치·부여만 다름/걸침/신규)와 근거
- 추가한 `StatusEffectType` 값과 판정 위치(파일·메서드)
- 계약·시그니처 변경 여부 (있었다면 승인받은 내용)
- 에셋 경로, 씬 등록 여부, 인스펙터에서 마저 할 값 (스프라이트, 부여 수단 연결)
- Console 결과 (에러·경고 수)
- 남긴 한계, 사용자가 Play에서 확인할 항목, 발견한 낡은 문서 서술
