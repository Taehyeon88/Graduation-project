# Master of Piece 모작 — 게임 기획서

> 졸업 프로젝트 source of truth.
> 작성일: 2026-09-26 / 모작 방향 개정: 2026-10-04 / 카드·특성 명세 반영: 2026-10-04

---

## 1. 개요

- **프로젝트 성격**: **Master of Piece 모작** — 원작의 규칙·시스템을 그대로 따라 만든다. **아트는 모작 범위에서 제외** (임시 에셋 사용, 원작 아트를 베끼지 않음)
- **장르**: 체스에서 영감을 받은 덱빌딩 로그라이트 + 그리드 전술 (카드 대신 용병 말을 모으고 배치)
- **원작**: Master of Piece (아래 "원작 분석" 참고)
- **플랫폼**: PC (Windows). 키보드 입력. **1920×1080 (16:9)** 기준

> 원작에서 확인하지 못한 규칙은 추측하지 않고 `(원작 확인 필요)`로 표기한다. 확인되면 이 문서를 먼저 갱신한다.

## 2. 원작 분석 (Master of Piece)

- **개발**: 인디 스튜디오 I M GAME (GDWC 수상작)
- **출시**: 2026-02-04 Steam 얼리 액세스, $14.99
- **세계관**: 검은 안개에 뒤덮인 세계에서 용병단을 이끌고 탐험

### 핵심 시스템

| 요소 | 내용 |
|---|---|
| 말 = 용병 | 덱빌딩의 "카드"가 체스말 같은 용병 유닛. 덱을 짜는 일이 곧 용병단을 꾸리는 일 |
| AP 배치 | 라운드마다 행동력(AP)을 써서 용병을 그리드에 배치 |
| 자동 전투 | 전투가 시작되면 용병이 **속도(Speed) 순**으로 자동 행동. 플레이어의 핵심 결정은 배치 단계에 집중 |
| 특성(Trait) | 용병마다 전투 중 발동하는 특성이 있고, 용병당 **최대 2개 조합** 가능 → 시너지·콤보 |

### 설계상 강점

1. 보드게임 같은 단순한 규칙 위에 위치·타이밍·시너지 판단이 쌓여 진입장벽은 낮고 깊이는 높다
2. 실행이 자동이라 결정이 배치 단계에 몰리고, 결과를 읽는 퍼즐 같은 재미가 생긴다
3. 특성 2개 조합 구조로 빌드 다양성과 리플레이 가치가 높다
4. 체스말·그리드·배치라는 직관적 은유로 학습 비용이 낮다

### 한계 (추정 포함)

- 자동 전투라 실행 중 개입 여지가 적어 배치 실수를 만회하기 어려울 수 있다
- 얼리 액세스라 콘텐츠량·밸런스가 계속 바뀔 수 있다

### 출처

- [Strategy fans, prepare your decks. Master of Piece Launches New Steam Page and Trailer](https://www.gamedeveloper.com/press-release/strategy-fans-prepare-your-decks-master-of-piece-launches-new-steam-page-and-trailer)
- [Master of Piece Will Make Its Move on Steam on 4th February](https://cogconnected.com/2026/01/master-of-piece-will-make-its-move-on-steam-on-4th-february/)
- [Master of Piece (Steam)](https://store.steampowered.com/app/3009310/Master_of_Piece/)

## 3. 타깃

전략 게이머, 그리드/턴 기반 전략, 덱빌딩·전략 로그라이크 애호가

## 4. 핵심 재미

원작을 따른다:
1. **배치 한 수의 무게** — 속도순 자동 전투라 어디에 무엇을 놓느냐가 승패를 가른다
2. **용병·특성 시너지** — 용병 말과 특성(최대 2개) 조합으로 만드는 빌드
3. **결과를 읽는 쾌감** — 배치 후 전투가 풀리는 과정을 예측하고 검증

## 5. 게임 루프

```
용병단 구성(모집) — 용병 카드로 덱을 구성
 ↓
라운드 시작: 덱에서 카드를 뽑아 손패 구성 (드로우 수: 원작 확인 필요)
 ↓
배치 단계: 손패의 용병 카드를 AP를 써서 그리드 타일에 배치
 ↓
전투 단계: 속도순 자동 전투
 ↓
결과 / 보상 (원작 확인 필요)
 ↓
다음 라운드 반복
```

- 라운드 수·스테이지 구조·보상 종류·AP 수치: (원작 확인 필요)
- **카드 = 용병 1명**: 카드 1장을 사용하면 선택한 타일에 해당 용병이 배치되고, 카드의 AP 비용이 차감된다. 카드 사이클(드로우/버림/손패)의 구조는 이전 프로젝트의 카드 시스템을 따른다 — 상세는 [feature-spec/04_card_system.md](feature-spec/04_card_system.md). 원작에 이 사이클이 그대로 있는지는 (원작 확인 필요).
- **특성**: 용병당 최대 2개. 전투 중 조건(`GameAction`)에 반응해 발동한다 — 상세는 [feature-spec/05_trait_system.md](feature-spec/05_trait_system.md).
- 현재 코드의 AP(`MaxAP = 3`, 턴 시작 시 리필)는 프로토타입 임시 수치이며 원작 수치가 아니다.
- **턴 구조(현재 코드)**: `Player → AutoBattle → Enemy → Player …` 반복. 턴 단위는 `AutoBattleTurnGA`(팝업 → `AutoBattleSystem.CacheCombatants()`로 `Speed` 내림차순 큐 캐싱 → `AutoBattleGA` 시작). `AutoBattleGA` 1회 = 유닛 1명: 큐 맨 앞 속도의 동속 그룹에서 서로 다른 1~6 주사위로 1명을 고르고 `CombatantView.Battle()`로 행동을 예약한 뒤, 마지막에 다음 `AutoBattleGA`를 `AddReaction`해 큐가 빌 때까지(또는 한쪽 전멸) 반복한다. `Battle()` 공용 패턴: **정면 한 줄만** 사용 — 정면 칸이 비어 있으면 1칸 이동을 예약하고(이동은 `Speed`와 무관), 이어서 공격(`DealDamageGA`)을 예약한다. 공격 대상은 예약 시점이 아니라 **`DealDamageGA` 실행 시점의 공격자 정면**(영웅은 깃발 포함 적, 몬스터는 영웅)에서 정하고 없으면 공격하지 않는다. 정면은 영웅 +x, 몬스터 -x. 자동 전투 이동은 `MoveGA` 직접 예약(AP 소모 없음). `Speed` 0 유닛(허수아비)은 행동하지 않는다. 속도순·동속 주사위·1회 행동량·사거리(현재 정면 1칸)·깃발 규칙·전투 종료 조건·몬스터 행동은 (원작 확인 필요). 몬스터 턴은 AI 도입 전까지 빈 턴이다.
- 플레이어 턴 종료(= `AutoBattle` 시작) 시점에 영웅 상태이상 감소(`HeroSystem`)와 방어막 특성(`HeroArmor`)이 걸린다. `MuscleTrophy`(전투 시작 시)는 그대로이며 발동 시점은 (원작 확인 필요).
- **유닛 공용 데이터(영웅·몬스터)**: `CombatantData`(`Health` 체력, `Damage` 공격력, `Speed` 행동 순서(속도), `Perks` 특성 리스트 최대 2개). `Damage`/`Speed`는 자동 전투(`CombatantView.Battle`, `AutoBattleSystem`)가 소비한다 — 피해는 `DealDamageGA` → `DamageCaculator.GetDamage`로 `POWER`/약화/취약과 합성. `Speed`가 순서 키 외에 이동 거리를 겸하는지(현재는 순서 키 전용, 이동은 항상 1칸), `Damage`가 모든 행동의 기준인지는 (원작 확인 필요). 에셋의 `Damage`/`Speed` 값(영웅 4/3/2·1, 몬스터 0/3/3·0/1/1)은 기존 스킬·AI 수치에서 유추한 **임시값**이다. 영웅 런타임 복사본(`Hero`)과 수동 스킬 보유(`StartingSkills`), 골드는 제거했다.

## 6. 승리·패배

- **승리 조건**: (원작 확인 필요)
- **패배 조건**: (원작 확인 필요)
- 이전 기획의 "웨이브 핵 파괴" 조건이 원작에도 있는지: (원작 확인 필요)

## 7. 스코프

- **포함**: 용병 모집·덱빌딩, 용병 카드 시스템(드로우/손패/버림 + AP 배치), 그리드 배치(AP), 속도순 자동 전투, 특성 시스템(용병당 최대 2개), 다중 라운드·로그라이크 진행
- **제외**: 원작 아트·연출 모방 (아트는 모작 대상이 아님)
- **제거됨**: 수동 스킬 시스템(`SkillSystem` — 스킬 버튼 클릭→타겟팅 UI, [feature-spec/02_skill_system.md](feature-spec/02_skill_system.md), 역사 기록). `Effect` → `PerformEffectGA` → 구체 GA 체인, `TargetMode`/`RangeMode`, `Skill`/`SkillData` 데이터 모델은 유닛 행동 실행부로 남겼다(현재 호출 경로 없음).
- **제거됨**: 몬스터 자율 행동 계층(`EnemySystem`, `Enemy`/`EnemyAction` 판단·행동 모델과 구현체, `AttackHeroGA`/`PlayEnemyEAGA`, [feature-spec/01_enemy_system.md](feature-spec/01_enemy_system.md), 역사 기록). 몬스터는 체력·스프라이트만 가진 수동적 토큰이며 적 턴은 빈 턴이다. 적의 유닛 배치·이동 AI는 새로 설계한다(원본은 git 이력).
- **제거 예정**: 플레이어 수동 이동 모드(`MoveSystem`) — 속도순 자동 행동 도입 시 함께 정리.
- **재확인 필요**: 세이브/로드, 인게임 옵션 메뉴, 다국어, 터치 입력 (이전 기획에서는 비스코프였음, 원작 대응 여부 미정)

## 8. 원작 vs 현재 코드

| 항목 | Master of Piece(원작) | 현재 코드 (이전 기획 기반) |
|---|---|---|
| 전투 진행 | 배치 후 속도순 **자동** | 플레이어 턴 → 자동 전투 턴(속도순 1명씩, 정면 이동·공격) → 몬스터 턴(빈 턴) 반복 |
| 플레이어 개입 | 배치 단계 | 매 턴 직접 행동 |
| 말 / 유닛 | 용병 말 + 특성(최대 2개) | `Hero1~3` 등 영웅 SO + `Perk`(용병당 최대 2개 제약 구현됨) |
| 용병 획득·배치 | 카드(용병)를 AP로 배치 | 전투 시작 시 영웅이 `TokenSetup`으로 고정 배치, 카드 시스템 없음 |
| 행동 수단 | 속도순 자동 행동 | 영웅별 스킬 버튼(`SkillSystem`) 수동 사용 + 수동 이동(AP 1칸) |
| 메타 | 용병 모집, 덱빌딩, 로그라이트 | 단일 전투 스테이지 (웨이브 생성까지 구현) |

기존 `ActionSystem`(GameAction + PRE/POST 리액션) 파이프라인 위에 AP 배치·속도순 자동 행동·특성을 얹는 방향이다. 어떤 기존 시스템을 유지·교체할지는 챕터별 명세(`feature-spec/`)를 갱신할 때 결정한다.
