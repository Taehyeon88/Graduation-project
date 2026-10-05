# 뱀서류 토이프로젝트 — 기능명세서 (목차)

> 시스템별 구체 동작·수치 정의. 게임 기획서의 후속.
> 챕터별 구현 시 Claude에 **해당 섹션 파일**을 input으로 던진다.

> **코드 규약·인터페이스 계약·제공 인프라는 별도 문서:** [convention.md](convention.md) (코드 작성 전 필독)

| # | 문서 | 내용 |
|---|------|------|
| 00 | [00_action_architecture.md](feature-spec/00_action_architecture.md) | ActionSystem 아키텍처 — Performer/Reaction/AddReaction 이벤트 기반 커맨드 패턴 (다른 챕터보다 먼저 읽기) |
| 00 | [action_command.md](feature-spec/action/action_command.md) | GameAction 레지스트리 — 실제 Perform/AddReaction 경로가 있는 모든 GA 카탈로그(소유 System, 트리거 관계) |
| 00 | [00_token_system.md](feature-spec/00_token_system.md) | TokenSystem 아키텍처 — 그리드 좌표·조회·이동·초기배치 파사드 |
| 00 | [00_token_system_main.md](feature-spec/token/00_token_system_main.md) | TokenSystem API — TokenMainAPI(런타임 상태 변경 메서드 4개) 카탈로그 |
| 00 | [00_token_system_setup.md](feature-spec/token/00_token_system_setup.md) | TokenSystem API — TokenSetup(초기 배치 메서드 4개) 카탈로그 |
| 00 | [00_token_system_api.md](feature-spec/token/00_token_system_api.md) | TokenSystem API — TokenServiceAPI(조회 메서드 16개) 카탈로그 |
| 01 | [01_enemy_system.md](feature-spec/01_enemy_system.md) | **(제거됨, 역사 기록)** EnemySystem 아키텍처 — 몬스터 턴 판단·실행 흐름 |
| 01 | [enemy_action_howto.md](feature-spec/enemy_action_howto.md) | 새 EnemyAction 추가 실전 가이드 — FireArrowEA 사례 |
| 01 | [01_hero_system.md](feature-spec/01_hero_system.md) | HeroSystem 아키텍처 — 플레이어 턴 시작 훅 + 선택된 영웅 상태 관리 |
| 02 | [02_skill_system.md](feature-spec/02_skill_system.md) | **(제거됨, 역사 기록)** SkillSystem 아키텍처 — 스킬 카드 클릭→타겟팅→효과 GameAction 위임 흐름. `Effect` 체인 설명은 참고용으로 유효 |
| 03 | [03_wave_system.md](feature-spec/03_wave_system.md) | WaveSystem 아키텍처 — 몬스터 턴마다 카운트다운, n턴마다 웨이브 추가 생성 + 신규 몬스터 행동 자동 연산 |
| 04 | [04_card_system.md](feature-spec/04_card_system.md) | CardSystem 아키텍처 (**최소 범위 구현됨**) — 용병 카드 드로우/손패/버림 사이클 + AP로 타일에 배치 |
| 05 | [05_trait_system.md](feature-spec/05_trait_system.md) | 특성(Trait) 시스템 — 기존 `Perk` 구조 관찰 + 용병당 최대 2개 제약 등 목표 변경 |
