---
description: Docs/convention.md 기준 코드 위반 점검(검토만, 자동 수정 없음) + GA 카탈로그(action_command.md) 갱신 확인
argument-hint: "[검사할 파일/폴더 경로 — 생략 시 현재 git diff]"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Edit
  - Bash(git diff:*)
  - Bash(git status:*)
disable-model-invocation: true
---

`Docs/convention.md`를 읽고, 그 안의 규약(네이밍 / 필드·프로퍼티 / 메서드 구성 / 폴더 위치 / 제공 인프라 / 인터페이스 계약 등 각 절)을 기준으로 아래 대상 코드를 점검한다.

**점검 대상**
- `$ARGUMENTS`가 있으면 그 파일/폴더만 점검한다.
- 없으면 `git diff HEAD`(스테이지 + 비스테이지 변경분) 중 `.cs` 파일만 점검한다. 변경분이 없으면 "점검할 변경 사항 없음"이라고만 답하고 끝낸다.

**절대 하지 말 것**
- 코드를 수정하지 않는다.
- `Docs/convention.md`를 포함해 어떤 문서도 수정하지 않는다 — 단, 아래 "GA 카탈로그 갱신 확인" 단계에서 `Docs/feature-spec/action/action_command.md`만 예외.
- 위반이 아닌 걸 억지로 찾아내지 않는다 — 근거 없는 지적 금지.

**보고 형식**
`Docs/convention.md`의 "위반 사례" 표와 같은 형식으로 정리한다:

| 규약 | 위반 파일:라인 | 설명 |
|---|---|---|
| ... | ... | ... |

위반이 하나도 없으면 표 없이 "위반 없음"이라고 짧게만 답한다.

**추가 작업 — GameAction 카탈로그(action_command.md) 갱신 확인**

이 단계는 위 컨벤션 점검의 대상 범위(`$ARGUMENTS`)와 무관하게 항상 `git diff HEAD` 전체를 스캔한다.

1. `git diff HEAD`에서 아래에 해당하는 변경을 찾는다:
   - `Assets/Game/0_Scripts/2.GameActions/` 아래 신규 `~GA.cs` 파일 추가(신규 untracked 포함)/삭제
   - 기존 `~GA` 클래스의 필드 변경
   - `~System`/`~Processor`의 `ActionSystem.AttachPerformer`/`DetachPerformer`/`SubscribeReaction`/`UnsubscribeReaction` 등록부 변경, `AddReaction`/`PostReactions.Add` 호출부 변경(트리거 지점·파생 액션 변화)
2. 해당하는 변경이 없으면 "GA 카탈로그 변경 사항 없음"이라고만 보고하고 끝낸다 — 문서는 건드리지 않는다.
3. 변경이 있으면 `Docs/feature-spec/action/action_command.md`를 읽고, 그 문서 4절("문서 유지보수 규칙")에 따라 갱신한다:
   - 신규/변경된 GA → 1절 마스터 인덱스 + 해당 카테고리의 2절 상세 블록 갱신
   - 삭제된 GA → 1·2절에서 제거, 3절 체인 다이어그램에 등장했으면 함께 정리
   - `ActionSystem` 자체의 동작 방식(Flow, PRE/POST 의미 등)이 바뀐 경우는 이 문서 소관이 아니라 `feature-spec/00_action_architecture.md` 소관 — 이 문서는 건드리지 않고 그 사실만 알린다
4. 갱신했으면 무엇을 바꿨는지 짧게 보고한다(예: "SpawnWaveGA 신규 추가 — 1절 인덱스 + 2-3절 상세 블록 갱신").
