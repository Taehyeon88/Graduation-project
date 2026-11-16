# 11. 스테이지 진행 · 스테이지 선택 시스템 (B안)

> 로그라이크 대신 스테이지 클리어 방식. 스테이지 선택 → 정보 팝업·영웅 카드 선택 → 배치 전투 → 클리어 판정·별·휘장 지급 → 선택 씬 별 표시.
> 원본: Claude Docs [「스테이지 선택 시스템 기획서」](https://claude.ai/artifact/Au9KNHFbg3zE38iDk7v4JP)(rev 15), 상위 [「졸업작품 기획 결정 정리 — 스테이지형 전환」](https://claude.ai/artifact/Jze3unwnJ26XAPB113egh5)(rev 16).
> 구현 항목: [task-list.md](../task-list.md) T-02·T-03·T-04·T-05(선택 씬 미리보기) 통합. **상태: 구현 완료, 플레이 검증 전** (2026-11-16).

## 1. 규칙

### 1-1. 해금 · 클리어 · 재도전
- 해금: 이전 스테이지를 클리어하면 다음 스테이지가 해금된다. 첫 스테이지는 처음부터 해금.
- 클리어: **승리 = ★1 = 클리어**. ★2·★3은 해금에 필요 없다.
- 패배: 별 0개, 해금 없음. 같은 스테이지에 재도전하거나 선택 씬으로 돌아간다.
- 재도전: 같은 몬스터 구성·순서. 적 행동에 랜덤이 없고 AI가 현재 판세에 반응하므로 시드 고정은 하지 않는다. 전투 내 동속 주사위·치명타 랜덤은 그대로.
- 클리어한 스테이지는 언제든 다시 도전해 남은 별을 모을 수 있다.
- 비전투 구조물(주점): 나가면 클리어 처리(다음 해금), 별 없음.

### 1-2. 별

| 별 | 조건 | 스테이지마다 다른 것 |
|---|---|---|
| ★1 | 승리 | 없음 |
| ★2 | 용병대장 체력 N 이상 남기고 승리 | 수치 N |
| ★3 | 스테이지 특수 조건(템플릿 1개) | 조건 종류와 수치 |

- ★3 템플릿(구현 4종): N라운드 안에 승리 / 특정 영웅 없이 승리 / 기본 제공 카드만으로 승리 / 특정 몬스터 N마리 이상 처치. "특정 키워드 연계 N회"는 키워드 도입(T-15) 때 추가.
- 조건은 하나씩 따로 영구 기록한다. 한 판에 모두 달성하지 않아도 되고, 한 번 달성한 조건은 사라지지 않는다. 모든 조건은 **승리한 판에서만** 인정.
- 별 조건 3개와 달성 여부는 정보 팝업에 미리 표시한다(덱 선택에 영향).

### 1-3. 보상

| 보상 | 지급 기준 | 쓰는 곳 |
|---|---|---|
| 별 | 이번 판에서 **새로** 달성한 조건 수 (스테이지당 최대 3) | 공용 트리(T-12) |
| 휘장 | 스테이지별로 정한 수량, **첫 클리어 1회만** | 영웅 승급(T-11) |
| ~~보상 용병~~ | **제거** (2026-11-16) — 승리 후 용병 선택 없음. `RewardSystem`·`RewardSelectSystem` 코드는 호출만 끊고 남김(정리 여부 결정 필요). 영웅 획득 경로는 성장 시스템(T-11~)에서 다시 정함 | — |

별·휘장은 승리 처리 시 바로 가산되고, 상단 표시줄(`CurrencyUI`)에 즉시 반영된다 (획득 연출은 나중).

### 1-4. 영웅 카드 선택 (덱)
- 시작 덱 = 스테이지 **기본 제공 영웅 카드**(고정, 빼거나 바꿀 수 없음) + 플레이어가 고른 카드.
- 선택 슬롯 = 스테이지 덱 크기(5~8) − 기본 카드 수. 공용 트리 "덱 슬롯 +1"은 T-12에서 반영.
- 최소 1장 골라야 진입 가능. 빈 슬롯이 있으면 경고만 표시.
- 기본 카드와 같은 영웅은 중복 선택 불가.
- 재진입·재도전 시 지난 선택을 미리 채운다.

### 1-5. 용병대장 체력 ("플레이어 체력")
- 스테이지별 고정값(`StageData`). 매 전투 그 값으로 시작한다. 스테이지 간 이월 없음.
- 상단 표시줄의 체력 칸(`PlayerHealthUI`)은 제거 (2026-11-16) — 전투 밖에서 지난 값이 남아 보였음. 전투 중엔 용병대장 토큰 체력·호버 정보 패널, 진입 전엔 정보 팝업으로 확인.

## 2. 화면

### 2-1. 흐름
1. 선택 씬에서 해금된 스테이지를 클릭 → 정보 팝업.
2. 팝업에서 영웅 카드 선택 → 진입(1장 이상 선택 시 활성) → 영웅 이동 연출 → 전투 씬.
3. 승리 → 별·휘장 지급, 다음 해금 → 승리 UI(획득한 별 +N, 별 조건 3줄 — 이번 판 충족한 조건만 노란색) → 확인 → 선택 씬 복귀.
4. 패배 → "재도전"(같은 스테이지·같은 선택) / "스테이지 선택으로". 패배 화면 디자인은 기획서 미정 — 기존 `GameOverUI` 버튼만 바꾼다.
- 팝업 닫기 → 선택 씬 그대로.

### 2-2. 정보 팝업

| 영역 | 표시 | 데이터 |
|---|---|---|
| 스테이지 정보 | 용병대장 체력 | `StageData` |
| 스테이지 정보 | 적 깃발 체력 | `StageData.EnemyCoreHealth` |
| 스테이지 정보 | 등장 몬스터 종류 | `StageData.waveData.EnemyDatas` 중복 제거 |
| 스테이지 정보 | 별 조건 3개 + 달성 여부 | `StageData` + 진행 기록 |
| 스테이지 정보 | 맵 미리보기(4×4, 중립지대·장애물·발판) | `StageData` |
| 영웅 카드 선택 | 기본 제공 카드(고정) | `StageData` |
| 영웅 카드 선택 | 보유 카드 목록과 선택 슬롯 | `GameSystem.Deck` |
| 버튼 | 진입(선택 시 활성), 닫기 | — |

- 전투 준비 바로가기(소문 교체·공용 트리)는 해당 기능이 생길 때(T-12·T-13) 추가.

### 2-3. 선택 씬 아이콘

| 상태 | 조건 | 표시 |
|---|---|---|
| 잠김 | 이전 스테이지 미클리어 | 자물쇠, 클릭 불가 |
| 선택 가능 | 해금됨, 클리어 전 | 빈 별 3개 |
| 클리어 | 별 1개 이상 | 채운 별 / 빈 별 |

비전투 구조물은 별을 표시하지 않는다. 툴팁(설명)은 기존대로 잠김 상태에서도 보인다.

## 3. 구현 구조

| 구분 | 위치 | 내용 |
|---|---|---|
| 데이터 | `5.Data/StageData` | + `HeroCaptainHealth`, `DeckSize`, `BaseHeroes`, `InsigniaReward`, `CaptainHealthStarThreshold`(★2 N), `SpecialStarCondition`(★3, `[SerializeReference, SR]`) |
| 모델 | `Models/StarCondition`(추상) + `StarConditions/` 4종 | `Description`, `IsMet(StageResult)` — `RoundLimit`·`WithoutHero`·`BaseHeroesOnly`·`KillEnemy` |
| 모델 | `Models/StageResult` | 승리 라운드 수, 용병대장 남은 체력, 기본 제공 카드, 시작 덱, 배치한 영웅, 몬스터별 처치 수 |
| 모델 | `Models/StageProgress` | 클리어 여부, 별 3칸 달성, 휘장 지급 여부, 지난 선택. `RecordStars`는 새로 달성한 수 반환 |
| 시스템 | `GameSystem` | 구조물별 `StageProgress`, 총 `Stars`·`Insignias`, 보유 `Deck` + 시작 `BattleDeck`, `EnterStage`/`RetryStage`/`ReturnToStageSelect`/`MarkCurrentCleared`/`GetSelectSlotCount`/`CanSelect`/`GetAutoSelection`(디버그 진입용). 클리어 기록은 `GameClearGA` Performer 안에서(첫 클리어 판정 → 별·휘장 지급 → 보상 용병은 첫 클리어만) |
| 시스템 | `StarSystem`(GameDemoScene `--- SYSTEMS ---`) | `KillGA` **PRE**로 몬스터별 처치 집계(토큰 제거 전), `PlayCardGA` POST로 배치 영웅 기록, `EvaluateStars()`를 `GameClearGA` Performer가 토큰 정리 전에 호출 |
| 시스템 | `StageSelectSystem` | `IsUnlocked`(이전 스테이지 클리어) 기준 선택, 전투 구조물 클릭 → 팝업 / 비전투 구조물 → 바로 진입, 팝업 열림 중 월드 입력 잠금 |
| 뷰 | `StructureView` | `SetState(unlocked, progress)` — 잠김(어둡게+자물쇠) / 선택 가능(빈 별 3) / 클리어(획득 별 채움), 비전투는 별 숨김 |
| UI | `UI/StageInfoPopupUI` + `UI/HeroCardSlotUI` | 2-2 표 전체. 카드 표시는 `CardViewInPile` 프리팹 재사용. 별 조건 문구는 `StarSystem.GetStarDescription`(정적, 승리 UI와 공용) |
| UI | `UI/GameClearUI` | `GameSystem.LastAchievedStars`·`LastGainedStars`로 별 결과 표시, 확인 → `ReturnToStageSelect` |
| UI | `UI/CurrencyUI`(`--- GAMEMASTER ---` `Top Canvas/DisPlayUI`) | 별·휘장 보유량 표시 (골드 칸을 별로 교체 + 휘장 칸 추가) |
| 정리 | `CardSystem` | 드로우 더미를 `BattleDeck`으로 |
| 정리 | `MatchSetupSystem` | 용병대장 체력을 `StageData.HeroCaptainHealth`에서 |
| 정리 | `GameClearUI`/`GameOverUI`/`TavernSystem`/`StageLauncherWindow` | 새 흐름 메서드로 (`GoToNextLevel`·`SelectStructure`·체력 이월 제거) |

### 3-1. 씬·에셋 설정 (2026-11-16)
- GameDemoScene `--- SYSTEMS ---/StarSystem` (인스펙터 연결 없음)
- StageScene `--- UI ---/StageInfo Canvas` (sortingOrder 100, 1920×1080) — `StageInfoPopupUI` 필드 전부 연결, `StageSelectSystem.stageInfoPopup` 연결. 배치·색은 임시
- `StructureView.prefab`: `Lock`, `Stars/Star1~3` 자식 + 임시 스프라이트 `4_Medias/2.HUDUI/Temp_StarFilled·Temp_StarEmpty·Temp_Lock.png`
- 스테이지 구성(`StageSelectSystem.stageStructures`): `일반 전투 1~4` → `정예 전투` (총 5). 주점은 목록에서 제외(에셋·Tavern 씬은 유지). 일반 전투 1~4는 진행 기록이 `StructureData` 키라 에셋을 4개로 분리했고, 같은 `StageData`(일반전투)를 공유한다
- 스테이지 에셋 값: 일반전투·정예전투 모두 용병대장 체력 20, 덱 크기 5, 기본 제공 카드 없음, 휘장 1, ★2 체력 10 이상. ★3 = 일반전투 5라운드 / 정예전투 7라운드 안에 승리

## 4. 구현 전 코드 상태 (기록, 2026-11-16)

| 위치 | 현재 동작 |
|---|---|
| `GameSystem.cs` | `CurrentStageLevel` 1씩 증가하는 1자 진행. `Deck`이 `startingDeck`에서 시작해 보상으로 누적. `StartFromScratch`로 처음부터. 용병대장 체력(`HeroCaptainHealth`) 승리 시 저장·이월 |
| `StageSelectSystem.cs` | `stageStructures` 임시 1자형 리스트, `CurrentStageLevel` 번호만 선택 가능, 클릭 즉시 이동 |
| `StructureView.cs` | 선택 가능/잠김 2색(`SetSelectable`), 호버 툴팁 |
| `RewardSystem.cs` | 1스테이지만 임시 보상(용병 2명 후보), 클리어할 때마다 |
| `GameOverUI.cs` | 재시작 = `StartFromScratch`(처음부터), 종료 = StartScene |
| `CardSystem.cs:72` | `GameSystem.Deck` 전체로 드로우 더미 구성 |
| `MatchSetupSystem.cs:59` | 이월된 용병대장 체력으로 `RestoreHealth` |

## 5. 결정 필요 · 보류
- 세이브/로드: 진행 기록은 `GameSystem`(DontDestroy) 메모리에만 둔다. 저장 범위 포함 여부 (결정 필요)
- 패배 화면 디자인 (기획서 미정)
- 영웅 보유 수 대비 덱 슬롯 기준 (보류 — 참고: 초반 보유 수 ≈ 슬롯, 중반 이후 보유 수 ≥ 선택 슬롯 × 2)
- 기존 주점·엘리트 스테이지의 장기 존속 (결정 필요 — 이번엔 유지)
