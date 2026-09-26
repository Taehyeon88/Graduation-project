---
description: Docs/howto/enemy_action_howto.md 가이드를 따라 새 EnemyAction(~EA) 클래스를 스캐폴드
argument-hint: "[새 EnemyAction 이름]"
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

`Docs/howto/enemy_action_howto.md`(새 EnemyAction 추가 실전 가이드)를 따라 새 `~EA` 클래스를 스캐폴드한다.

**Step 0 — 이름 확인 (필수, 건너뛰지 말 것)**
`$ARGUMENTS`가 비어 있으면 코드를 하나도 쓰지 말고 "새 EnemyAction 이름이 뭔가요?"라고 되물은 뒤 답을 받을 때까지 멈춘다. 이름이 있으면 그대로 진행한다. 이름 끝에 `EA`가 없으면 붙여서 클래스명을 확정한다(예: `PoisonCloud` → `PoisonCloudEA`).

**Step 1 — 가이드·참고 코드 읽기**
`Docs/howto/enemy_action_howto.md` 전체를 읽는다. 참고 예시로 `Assets/Game/0_Scripts/Enemies/EnemyActions/AttackEA.cs`(근접, 이동형과 조합)와 `FireArrowEA.cs`(원거리, 투사체)를 읽는다. `Assets/Game/0_Scripts/Enemies/EnemyActions/<클래스명>.cs`가 이미 있으면 그 사실만 알리고 멈춘다(덮어쓰지 않음).

**Step 2 — 설계 확인 (코드 작성 전, 한 번에 묶어서 질문)**
AskUserQuestion으로 아래를 확인한다(이미 사용자가 먼저 답을 줬으면 다시 묻지 않는다):
1. 근접형인지 원거리(투사체)형인지 — 가이드 §1 Step 4/6 분기.
2. 단일 타겟인지 범위(영역)인지.
3. 이 행동을 쓸 몬스터 — 기존 `Enemy` 서브클래스에 추가로 등록할지, 새 `Enemy` 서브클래스가 필요한지. 새로 필요하면 이동형(`Larva` 패턴)인지 고정형(`Scarecrow` 패턴)인지도 확인 — 가이드 §1 Step 7.

**Step 3 — `~EA` 클래스 작성**
가이드 §1 Step 1~5를 그대로 따른다: 위치 `Assets/Game/0_Scripts/Enemies/EnemyActions/`, `EnemyAction` + 필요 인터페이스(`IHaveDamage`/`IHaveDistance` 등) 상속, `Icon`/`Description`/`TextInfo` getter 계산 프로퍼티, 밸런스 필드는 `[field: SerializeField]`, `PlayEnemyAction`은 `FindRandomHeroInRange`로 자체 타겟팅 후 null 체크 → 연출 `Sequence` → 기존 `AttackHeroGA`(또는 적절한 기존 GA) `AddReaction` → `Sequence` 반환, `Clone()`은 얕은 복사로 자기 자신의 새 인스턴스 반환.

원거리로 확인됐으면 가이드 §1 Step 6의 `ProjectileSystem.Instance.PlayArrow`(화살)을 재사용한다. 화살이 아닌 다른 투사체가 필요하면, 새 풀을 그 클래스에 추가하는 게 맞을지 먼저 사용자에게 짚고 진행한다(과설계 방지).

**Step 4 — 몬스터 연결**
Step 2에서 확인한 대로:
- 기존 `Enemy` 서브클래스에 추가하는 경우: 그 클래스의 `JudgeActAction`/`PerformAction`을 수정하기 전에 기존 로직과 어떻게 공존시킬지(예: 조건 분기 추가) 먼저 한 줄로 설명하고 진행한다 — 기존 몬스터 동작을 조용히 바꾸지 않는다.
- 새 `Enemy` 서브클래스가 필요한 경우: 가이드 §1 Step 7의 두 패턴(`Scarecrow`형 고정 vs `Larva`형 이동) 중 확인된 쪽으로 작성한다.

**Step 5 — 에디터 작업 안내 후 멈춤**
새 `~EA` 인스턴스를 실제 `EnemyData` 에셋(`Assets/Game/1_Datas/1.Enemies/*.asset`)의 `EnemyActions` 리스트에 등록하는 건 에디터 작업이다. 클릭 순서만 안내하고 직접 하지 않는다(CLAUDE.md 작업 분담 규칙).

**Step 6 — 검증**
Unity MCP 연결돼 있으면 콘솔 에러 0을 확인한다. 안 되면 사용자에게 직접 확인해달라고 안내한다.

**절대 하지 말 것**
- Step 0의 이름 확인을 건너뛰지 않는다.
- Step 2의 설계 확인 없이 임의로 근접/원거리·단일/범위를 추측해서 코드부터 쓰지 않는다.
- `EnemyData` 에셋(.asset)이나 씬 파일을 직접 수정하지 않는다 — 에디터 작업으로 안내만 한다.
