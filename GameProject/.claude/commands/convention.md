---
description: Docs/convention.md 기준으로 코드 위반 점검 (검토만, 자동 수정 없음)
argument-hint: "[검사할 파일/폴더 경로 — 생략 시 현재 git diff]"
allowed-tools:
  - Read
  - Grep
  - Glob
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
- `Docs/convention.md`를 포함해 어떤 문서도 수정하지 않는다.
- 위반이 아닌 걸 억지로 찾아내지 않는다 — 근거 없는 지적 금지.

**보고 형식**
`Docs/convention.md`의 "위반 사례" 표와 같은 형식으로 정리한다:

| 규약 | 위반 파일:라인 | 설명 |
|---|---|---|
| ... | ... | ... |

위반이 하나도 없으면 표 없이 "위반 없음"이라고 짧게만 답한다.
