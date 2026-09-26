---
description: Docs/howto/enemy_howto.md 가이드를 따라 새 Enemy(~Enemy 서브클래스) 스캐폴드
argument-hint: "[새 Enemy 이름]"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Write
  - Edit
  - AskUserQuestion
  - Bash(git status:*)
disable-model-invocation: true
---

`Docs/howto/enemy_howto.md`(새 Enemy 추가 실전 가이드)를 따라 새 `Enemy` 서브클래스를 스캐폴드한다.

**Step 0 — 이름 확인 (필수, 건너뛰지 말 것)**
`$ARGUMENTS`가 비어 있으면 코드를 하나도 쓰지 말고 "새 Enemy 이름이 뭔가요?"라고 되물은 뒤 답을 받을 때까지 멈춘다. 이름이 있으면 그대로 클래스명으로 쓴다(강제로 붙이는 접미사 없음 — `Larva`/`Scarecrow`/`ArchorSkeleton` 모두 별도 접미사 규칙이 없다).

**Step 1 — 가이드·참고 코드 읽기**
`Docs/howto/enemy_howto.md` 전체를 읽는다. 참고 예시로 `Assets/Game/0_Scripts/Enemies/Larva.cs`(이동형), `Scarecrow.cs`(제자리형), `ArchorSkeleton.cs`(이동형+원거리 조합)를 읽는다. `Assets/Game/0_Scripts/Enemies/<클래스명>.cs`가 이미 있으면 그 사실만 알리고 멈춘다(덮어쓰지 않음).

**Step 2 — 설계 확인 (코드 작성 전, 한 번에 묶어서 질문)**
AskUserQuestion으로 아래를 확인한다(이미 사용자가 먼저 답을 줬으면 다시 묻지 않는다):
1. 이동형인지 제자리형인지 — 가이드 §0/§1 분기.
2. 공격 방식 — 기존 `AttackEA`(근접)/`FireArrowEA`(원거리) 중 재사용할지, 새 `EnemyAction`이 필요한지. 새로 필요하면 이 커맨드로 만들지 않고 `/add-EnemyAction`으로 그 클래스부터 먼저 만들라고 안내한 뒤 멈춘다 — 이 커맨드는 `Enemy` 서브클래스 스캐폴드 전용이다.

**Step 3 — `Enemy` 서브클래스 작성**
가이드 §1을 그대로 따른다:
- 이동형 → `PerformAction`에서 `TryMoveIntoRange(enemy, 선택된EnemyAction.Distance)`로 위임(`Larva`/`ArchorSkeleton` 패턴).
- 제자리형 → `PerformAction`은 `return true;`만(`Scarecrow` 패턴).
- `JudgeActAction`은 `GetEnemyAction(enemy, typeof(선택된EnemyAction))` 패턴.
- `Clone()`은 **반드시 자기 자신의 새 인스턴스**를 반환하는지 직접 확인한다 — 가이드에 나온 흔한 함정(과거 `Larva.Clone()`이 `new Scarecrow()`를 반환했던 사례)을 반복하지 않는다.

**Step 4 — 에디터 작업 안내 후 멈춤**
`EnemyData` SO 에셋 생성(가이드 §3)은 에디터 작업이다. 클릭 순서만 안내하고 직접 만들지 않는다(CLAUDE.md 작업분담 규칙). 웨이브(`WaveData`) 등록은 가이드 범위 밖이므로 다루지 않는다.

**Step 5 — 검증**
Unity MCP가 연결돼 있으면 콘솔 에러 0을 확인한다. 안 되면 사용자에게 직접 확인해달라고 안내한다.

**절대 하지 말 것**
- Step 0의 이름 확인을 건너뛰지 않는다.
- Step 2의 설계 확인 없이 이동형/제자리형·공격 방식을 임의로 추측해서 코드부터 쓰지 않는다.
- `EnemyData` 에셋(.asset)이나 웨이브 파일을 직접 수정하지 않는다 — 에디터 작업으로 안내만 한다.
