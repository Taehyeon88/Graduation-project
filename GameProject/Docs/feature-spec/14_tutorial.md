# 14. 튜토리얼 시스템

> 정해진 시나리오로 **기본 전투 → 결과 → 공용 트리 → 영웅 승급 → 스테이지 진입**을 한 번씩 직접 해 보게 하는 안내 시스템. 기획 원본은 Claude Docs 「튜토리얼 시스템 기획서」(`game-design.md` 헤더 참고)이고, 이 문서는 **현재 코드 기준** 구현 명세다.
> 안내는 위쪽 가운데 고정 상자 + **스포트라이트**(대상만 밝게 뚫음)로 한다. 건너뛰기 없음, 완료 후 재실행 없음.
> 이 문서는 구현 후 역추출했다. 기획서가 갱신되어 문구·단계가 일부 달라졌다 — 맨 아래 "기획서와의 차이" 참고.

## 규칙 요약

- 튜토리얼은 **단계 데이터(SO)를 순서대로 읽어 실행하는 매니저** 하나(`TutorialSystem`)가 구동한다. 화면 쪽 코드는 매니저를 부르는 연결 지점만 둔다.
- 안내 구간(`LockInput = true`)에는 **안내 대상이 아닌 UI·월드 입력을 모두 막는다**(설정 아이콘 제외). 행동을 가르치는 단계는 그 행동을 해야 넘어가고, 설명 단계는 "다음" 또는 화면 클릭으로 넘어간다.
- **안 가르친 UI는 숨기고**(`TutorialTarget.hideUntilRevealed`), 소개하는 단계에서 공개한다. 공개 목록은 단계가 지날수록 늘어난다(`RevealIds`, 일부는 `HideIdsOnComplete`로 다시 숨김). 튜토리얼이 끝나면 전부 공개된다.
- 전투는 **시나리오로 고정**한다 — 덱 순서, 시작 손패, 치명타, 같은 속도 주사위 결과, 수동 이동 끔, 1웨이브.
- 튜토리얼 전투가 **스테이지 1**이다. 받은 별·휘장을 성장 파트에서 실제로 쓴다. 클리어한 튜토리얼 스테이지는 **잠겨서 다시 들어갈 수 없다**(별은 그대로 표시).
- 진행 데이터(별·휘장·노드·승급)가 메모리에만 있으므로 튜토리얼 상태도 메모리에만 둔다. 새 게임(`GameSystem.InitializeGameData`) 시 함께 초기화된다.
- **시작 화면 씬(`StartScene`)이 로드되면 튜토리얼을 종료한다**(`TutorialSystem`이 `SceneManager.sceneLoaded` 구독 → `End()`). 설정창의 "시작 화면으로" 등 어떤 경로든 안내 층이 시작 화면 버튼을 막지 않게 하기 위함이며, 이후 `새 게임`이 진행 데이터를 지우므로 튜토리얼은 처음부터 다시 시작한다.

## 구성

### 데이터 (`5.Data/`, 자산은 `1_Datas/16.Tutorial/` 외)

| 파일 | 역할 |
|---|---|
| `TutorialBattleData.cs` | SO. 튜토리얼 전투 고정값: `DrawOrder`(뽑을 카드 더미 순서, 섞기 생략)·`StartDrawAmount`(시작 드로우 장수)·`CriticalRound`/`CriticalHeroes`(그 라운드 자동 전투 끝에 이 영웅만 치명타 100%, 나머지 0%)·`SameSpeedRound`/`SameSpeedWinner`(그 라운드 같은 속도 그룹에서 먼저 행동할 영웅·몬스터, `CombatantData`). 자산 `16.Tutorial/튜토리얼_전투.asset` |
| `StageData.cs` | `TutorialBattle` 필드 추가. **이 필드가 있는 스테이지가 튜토리얼 전투**이고 없으면 일반 스테이지. `GameSystem.TutorialBattle`이 `CurrentStageData.TutorialBattle`을 노출(구조물 미선택이면 null) |
| `TutorialStepData.cs` | 직렬화 클래스 `TutorialStepData`와 enum `TutorialScreen`·`TutorialTrigger`·`TutorialAdvance` |
| `TutorialSequenceData.cs` | SO. `Steps` 배열(전체 단계표). 자산 `16.Tutorial/TutorialSequence.asset` (현재 48단계) |

**`TutorialStepData` 필드**

| 필드 | 내용 |
|---|---|
| `Id`, `Screen`, `Message` | 단계 ID(예: `battle_07`), 화면(`Battle/Result/StageSelect/GrowthTree/HeroManage`), 안내 문구(두 줄 이내) |
| `TargetIds` | 스포트라이트 대상 `TutorialTarget` ID 목록. 특수 ID: `@context`(트리거 시점의 유닛들), `@stageN`(스테이지 선택 씬의 N번 구조물), `battlefield`(판 전체) |
| `CardHero`, `HeroTarget`, `CellPos`, `ShowCell` | 강조·배치할 영웅 카드 / 이미 배치된 영웅 / 목표 칸 / 배치·단도 단계가 아니어도 칸을 비출지 |
| `Trigger` | 단계가 시작되는 시점 (아래 표) |
| `Advance`, `AdvanceId` | 단계가 끝나는 조건 (아래 표)과 그 값(대상 ID·노드 코드·스테이지 번호) |
| `LockInput` | 안내 대상 외 입력 잠금 (자유 플레이 단계만 false) |
| `RevealIds`, `HideIdsOnComplete` | 이 단계부터 보이게 할 UI / 이 단계가 끝나면 다시 숨길 UI |
| `GrantCardOnStart`, `ShowGhost` | 시작 시 뽑을 카드 더미 맨 위 1장을 행동력 없이 지급 / 고스트 시연 재생 |
| `ResumeOnStart`, `ResumeOnComplete` | 자동 전투 안의 시작 시점은 도달하면 전투를 멈추는데, 시작·완료 때 그 멈춤을 푼다 |

**`TutorialTrigger`** (끝에만 추가해 기존 자산의 enum 값을 유지)

| 값 | 시작 시점 | 전투 정지 |
|---|---|---|
| `Immediate` | 앞 단계가 끝나자마자 | – |
| `BattleStart` | 전투 시작(첫 드로우·행동력 지급 전, `TurnGA(StartBattle)` PRE) | ✔ |
| `PlayerTurnStart` | 내 턴 시작(`TurnGA(Player)` POST) | – |
| `AutoBattleStart` | 자동 전투 시작(첫 `AutoBattleGA` PRE) | ✔ |
| `FirstUnitAction` | 첫 유닛 행동 직전(`MoveGA`·기본 공격 `DealDamageGA`·`AttackCoreGA` PRE) | ✔ |
| `SameSpeedTie` | 같은 속도 그룹의 주사위 연출 직전 | ✔ |
| `CriticalAttack` | 치명타를 가진 영웅의 공격 직전 | ✔ |
| `CoreAttack` | 영웅의 코어 공격 직전 | ✔ |
| `EnemyTurnEnd` | 적 턴 종료(`TurnGA(Enemy)` POST) | ✔ |
| `CriticalGained` | 영웅이 치명타를 얻은 직후(`AddStatusEffectGA` POST) | ✔ |
| `SceneEntered` | `Screen` 필드의 화면 씬에 진입(`NotifySceneEntered`) | – |

**`TutorialAdvance`**

| 값 | 완료 조건 |
|---|---|
| `NextButton` | 안내 상자 "다음" 버튼 또는 어두운 곳 클릭(`TutorialSystem.Next()`) |
| `PlaceCard` | `CardHero` 카드를 `CellPos`에 배치(`PlayCardGA` POST) |
| `TurnEnd` | 전투 시작 버튼(턴 종료, `AutoBattleTurnGA` PRE) |
| `DrawCard` | 카드 뽑기 버튼(튜토리얼 지급은 제외, `DrawCardsGA` POST) |
| `UseDagger` | 단도로 `CellPos`의 적 공격(`DealDamageGA` POST, 시전자 null·회피 무시) |
| `BattleVictory` | 전투 승리(`GameClearGA` POST) |
| `UIClick` | `AdvanceId` 대상의 `Button` 클릭 |
| `TargetAppears` | `AdvanceId` 대상이 활성화됨(팝업·확인창 열림) |
| `NodePurchased` | 공용 트리 노드(`AdvanceId` = 코드, 예 `C1`) 구매 |
| `PromotionDone` | 영웅 승급 확정 |
| `StageClick` | 스테이지 선택 씬에서 `AdvanceId` 번호의 스테이지를 클릭해 정보 팝업이 열림 |

### 시스템·UI (`1.Systems/`, `UI/`)

| 파일 | 역할 |
|---|---|
| `1.Systems/TutorialSystem.cs` | `Singleton`(`GAMEMASTER` 하위, 씬을 넘나듦). 시퀀스 진행·조건 감시·공개 목록·입력 가드. 정적 이벤트 `StepStarted/StepCompleted/WrongActionPerformed/TutorialEnded` |
| `UI/TutorialOverlayUI.cs` | `Singleton`. 안내 상자·스포트라이트·고스트 시연 표시, 대상 영역 계산 |
| `UI/TutorialSpotlightGraphic.cs` | `RawImage` + `ICanvasRaycastFilter`. 구멍 밖 입력은 막고 안은 통과 |
| `5_Materials/TutorialSpotlight.shader`·`.mat` | 화면을 어둡게 하고 구멍(최대 8개)을 뚫음. 눌러야 하는 대상 구멍에만 테두리 빛(맥동) |
| `UI/TutorialTarget.cs` | 지목·공개·잠금할 UI/월드 오브젝트에 붙이는 ID 표식 (아래) |
| `UI/ActionOrderBarUI.cs`, `ActionOrderSlotUI.cs` | **튜토리얼 전용** 행동 순서 띠 — `AutoBattleSystem.BattleQueue`의 아직 행동하지 않은 유닛을 순서대로 표시 |
| `UI/TurnOrderUI.cs` | 턴 순서 표시(내 턴 → 자동 전투 → 적 턴 중 현재 칸 강조). 전투 씬 상시 HUD |
| `UI/DiceRollUI.cs` | 같은 속도 주사위 연출(본 게임 공통) |

### `TutorialSystem` 흐름

1. **시작**: `TurnGA(StartBattle)` PRE에서 `GameSystem.TutorialBattle != null`이면 `Begin()`(전투 시작마다 처음부터). 전투 밖 단계는 `NotifySceneEntered`로 시작한다.
2. **단계 시작**(`StartStep`): 정지 트리거면 `ActionSystem.SetPause(true)`, `RevealIds` 공개, `GrantCardOnStart`면 `DrawCardsGA(1)`(AP 없음) 실행, `StepStarted` 발행.
3. **단계 완료**(`CompleteStep`): `HideIdsOnComplete` 처리, `ResumeOnComplete`면 정지 해제, `StepCompleted` 발행 → 다음 단계 준비(`Immediate`면 바로 시작, 아니면 트리거 대기). 단계가 끝나면 `End()` — 숨긴 UI 전부 공개, `TutorialEnded` 발행.
4. **일시정지**: `ActionSystem.SetPause`만 쓴다(`PauseSystem`은 `Time.timeScale = 0`이라 DOTween·연출과 충돌해 쓰지 않음). 설정창이 열리면 튜토리얼과 자동 전투가 멈추고(`SettingUI` → `NotifySettingOpen`) 닫으면 이어진다.
5. **튜토리얼 로직은 GA가 아니다.** `ActionSystem.Perform`은 수행 중이면 조용히 무시되므로, 구독(PRE/POST) + `SetPause`로 동작한다. 구독·해제는 `OnEnable`/`OnDisable`에서 대칭이며, 씬 전환 때 생기는 중복 `GAMEMASTER`에서는 `Instance != this`면 구독하지 않는다.

**정적 가드** (튜토리얼이 없거나 진행 중이 아니면 모두 허용)

| 메서드 | 쓰이는 곳 |
|---|---|
| `IsGuided` | 안내 구간 여부. `StageSelectSystem.Update`, 오버레이 입력 차단 |
| `AllowsCard(card)` / `AllowsPlacement(card, pos)` | `CardSystem.CanPlay`, 배치 확정. 아니면 취소하고 `NotifyWrongAction`(고스트 시연 재생) |
| `AllowsTurnEnd()` / `AllowsDraw()` | `TurnEndUI`, `DrawCardUI` |
| `AllowsDagger(token)` | `PlayerSkillSystem.CanUse`/`TryUse` (null이면 "단도 단계인가"만) |
| `IsStageClickStep` / `AllowsStageInput(번호)` | `StageSelectSystem` — 스테이지 클릭 단계에서만 목표 스테이지의 월드 입력 허용 |
| `IsRevealed(id)` | `TutorialTarget`의 공개 여부 판정 |
| `NotifySettingOpen` / `NotifySameSpeedTie` / `NotifyStagePopupOpened` / `NotifySceneEntered` / `ResetProgress` | 각 시스템에서 튜토리얼로 알림 |

### 스포트라이트와 입력 잠금

- `Tutorial Canvas`(Screen Space Overlay, `sortingOrder` 500)가 어두운 층을 깐다. Tooltip(32767)·Fade(32767)·설정창(1000)·치트(900)보다 아래, 게임 UI(`Bottom` 10·`Middle` 30·`StageInfo` 100·`GrowthNode`/`Promotion` 90)보다 위.
- **구멍 안은 아래 UI로 입력이 통과**하고 밖은 막는다(`IsRaycastLocationValid`). 월드 오브젝트(스테이지 구조물 등)도 구멍 안이면 `IsPointerOverGameObject`가 false가 되어 클릭된다. 안내 중 월드 입력은 별도로 `StageSelectSystem` 가드가 막는다.
- **설정 아이콘**은 구멍이 아니라 **레이어로 예외 처리**한다: `Setting` 버튼에 중첩 Canvas(`overrideSorting`, 순서 600)와 `GraphicRaycaster`를 붙여 튜토리얼 위에 둔다(같은 줄의 별·휘장 표시는 어둡게 남는다).
- **테두리 빛은 눌러야 하는 대상에만**: 대상 자신이 `IPointerClickHandler`/`IBeginDragHandler`(Button, `CardView`, `UseSkillUI` 등)를 구현하면 interactive, 설명용(판·체력·유닛·행동 순서 띠 등)은 구멍만. 안내 상자 영역(+8px)은 셰이더 `_RimMask`로 테두리를 그리지 않고, 상자는 불투명이다.
- 대상 영역 계산: UGUI는 월드 코너, 월드 토큰은 `Token.Model.bounds`, 칸은 `IsoWorld.IsoToScreen`(**화면 픽셀이 아니라 월드 좌표를 돌려주므로** `Camera.WorldToScreenPoint`를 한 번 더 거침, 칸 판정 평면 iso z=1), 손패는 카드들의 합집합, `battlefield`는 `IsoWorld` 아래 보이는 렌더러 전체의 합집합(단계마다 한 번 계산해 캐시).
- **고스트 시연**: 반투명 카드/단도가 원본 UI 이미지와 **같은 크기**로 시작점에서 목표 칸으로 반복 이동. 카드가 날아오는 중이면 0.2초 가만히 있을 때까지 기다린 뒤 시작하고, 반복마다 원본의 현재 위치·크기를 다시 읽는다.

### `TutorialTarget`

- 인스펙터: `id`, `hideUntilRevealed`(기본 true). `OnEnable`에서 정적 목록에 등록, `OnDisable`에서 해제. **ID마다 목록**(씬 전환 때 중복 `GAMEMASTER`가 같은 ID를 잠깐 등록했다 사라져도 원본이 남도록), `TryGet`은 가장 나중에 등록된 것을 반환.
- 숨김: UGUI는 `CanvasGroup`(알파 0·입력 불가, 오브젝트는 활성 유지 — 비활성화하면 소유 스크립트의 구독이 풀린다), 월드는 렌더러 끄기. 공개되면 저장해 둔 값으로 복원.
- `Configure(id, hide)`: **런타임에 생성되는 UI**(영웅 목록 버튼, 스테이지 팝업 카드)에 코드로 ID 부여.

**등록된 ID**

| 씬 | ID |
|---|---|
| 전투 (`GameDemoScene`) | `turn_end`, `ap`, `hand`, `draw_pile`, `discard_pile`, `draw_card`, `dagger`, `battlefield`, `turn_order`, 결과 화면 `result_star_1~3`, `result_insignia`, `result_confirm` |
| `GAMEMASTER` 프리팹 | `action_bar`(행동 순서 띠) |
| 월드 프리팹 | `captain`(`HeroCaptainView`), `enemy_flag`(`EnemyFlagView`) — 둘 다 `hideUntilRevealed = false` |
| 스테이지 선택 (`StageScene`) | 공용 트리 `growth_open`(숨김)·`growth_tree`·`growth_node_c1`·`growth_node_c2`·`growth_stars`·`growth_reset`·`growth_close`·`growth_reset_confirm` / 영웅 승급 `promotion_open`(숨김)·`promotion_insignia`·`promotion_hero_list`·`promotion_promote`·`promotion_options`·`promotion_confirm_panel`·`promotion_confirm_button`·`promotion_close` / 스테이지 팝업 `stage_captain_hp`·`stage_flag_hp`·`stage_monsters`·`stage_star_1~3`·`stage_base_cards`·`stage_owned_cards`·`stage_slots`·`stage_enter` |
| 런타임 생성 | `hero_<HeroData.Id>`(승급 화면 영웅 버튼, 매복자 = `hero_1004`), `stage_hero_<Id>`(스테이지 팝업 보유 카드) |

## 단계표 요약 (`TutorialSequence.asset`, 48단계)

| 구간 | 단계 ID | 내용 |
|---|---|---|
| 전투 1R | `battle_01~15` | 승패(용병대장·깃발) → 뽑을 카드 더미·손패 → 스탯 → 행동력 → 견습기사 배치 → 버린 카드 더미 → 기병 지급·배치 → 턴 순서·전투 시작 → 자동 전투 설명(행동 순서 띠·첫 유닛·같은 속도 주사위) → 적 턴 → 치명타 |
| 전투 2R | `battle_16~25` | 행동력 → 카드 뽑기 → 빠른 발 → 매복자 배치 → 단도 → 사망 처리 → 전투 시작 → 코어 공격 → 치명타 공격 → 3R 시작 전(깃발 체력) |
| 전투 자유 | `battle_26` | 잠금 해제, 승리가 완료 조건 |
| 결과 | `result_01~03` | 별 3줄 → 휘장 +N → 확인 버튼 클릭(스테이지 선택으로) |
| 스테이지 선택 | `stage_01` | `SceneEntered`. 스테이지 1·2 설명 |
| 공용 트리 | `stage_02`, `growth_01~07` | 열기 → 전체 → C1 구매 → C2 해금 → 초기화 → 확인 → C1 재구매 → 닫기 |
| 영웅 승급 | `stage_03`, `hero_01~04`, `end_01` | 열기 → 매복자 선택 → 승급 → 선택지 → 확정 → 닫기 |
| 스테이지 진입 | `stagein_01~04` | 스테이지 2 클릭(`StageClick`) → 팝업 정보 → 매복자 선택 → 슬롯을 모두 채우고 진입 클릭 → **종료** |

**전투 파트 단계 순서가 기획서와 다른 곳** (엔진의 실제 이벤트 순서에 맞춤): 적 턴 설명(`battle_14`)이 치명타 설명(`battle_15`)보다 먼저이고, 치명타는 적 턴이 끝난 *뒤* 처리되므로 `CriticalGained`에서 멈춘다(그 뒤 `battle_16`은 `Immediate`). 2R에서는 기병의 코어 공격이 견습기사의 치명타 공격보다 먼저라 코어 공격 설명이 먼저다.

## 전투 시나리오 (고정 세팅)

| 항목 | 값 |
|---|---|
| 덱 | 견습기사·기병·매복자·짐꾼·대장장이 순서 고정(섞기 생략), 시작 드로우 1장(견습기사). 기병은 `battle_09`에서 지급 |
| 몬스터(1웨이브, 라인 0/1/2) | 순례자1(속도 0, 체력 3) / 순례자2(속도 1, 체력 5) / 개구리교 창병(속도 3, 체력 3) |
| 배치 칸 | 견습기사 (0,1), 기병 (0,2), 매복자 (0,3), 단도 대상 창병 (2,2) |
| 행동력 흐름 | 1R 5 → 배치 −4 → 1 → 2R +3 = 4 → 카드 뽑기 −1 → 매복자 −2 → 단도 −1 → 0 |
| 1R 결과 | 창병이 먼저 전진 → 기병이 창병에 1 피해(체력 2) → 같은 속도 주사위는 순례자2 먼저 → 견습기사가 순례자2에 2 피해(체력 3) |
| 치명타 | 1R 자동 전투 끝에 견습기사만 100% |
| 코어·승리 | 적 깃발 체력 8, 용병대장 체력 20, ★3은 "기본 제공 카드만 배치해 승리"(시나리오대로면 항상 달성) |
| 가르치지 않는 규칙 | 수동 이동(`MoveSystem.PlayPlayerMoveMode` 가드), 웨이브 추가, 발판·장애물·중립지대(데이터 비움) |

자산: `8.Stages/튜토리얼.asset`(StageData), `8.Stages/Waves/튜토리얼_웨이브.asset`, `13.Structures/튜토리얼 전투.asset`(ID 100, 스테이지 선택 1번 구조물). 스테이지 2는 전용 `8.Stages/일반전투_스테이지2.asset`(기본 제공 견습기사, 슬롯 4 — 보유 4명을 모두 채우면 매복자가 반드시 포함).

## 기존 코드에 가한 변경

| 파일 | 변경 |
|---|---|
| `CardSystem` | `SetUp`에서 튜토리얼이면 `DrawOrder`대로 쌓고 섞기 생략, 시작 드로우를 `StartDrawAmount`로 대체, `CanPlay`에 `AllowsCard`, 배치 확정에 `AllowsPlacement`(어긋나면 취소·`NotifyWrongAction`) |
| `AutoBattleSystem` | 큐에 `BattleQueue` 노출. **같은 속도 주사위를 자동 전투 시작 때 그룹마다 한 번만 굴려 순서 확정**하고, 그룹이 처음 행동하는 순간에 `DiceRollUI` 연출(그룹당 1회, 본 게임 공통). 튜토리얼이면 `SameSpeedWinner`에 가장 큰 값, `NotifySameSpeedTie` 후 안내가 끝날 때까지 대기. 이전의 `PickNext()`(선택마다 재굴림)는 제거 |
| `HeroSystem` | 치명타 판정(`RollCritical`)에 튜토리얼 고정 |
| `MoveSystem` | 튜토리얼 전투에서 수동 이동 진입 가드 |
| `PlayerSkillSystem`, `TurnEndUI`, `DrawCardUI` | 위 정적 가드 |
| `SettingUI` | 열고 닫을 때 `NotifySettingOpen` |
| `PileofCardUI` | 단계가 시작될 때 더미 창을 닫음(열린 채 넘어가면 칸 입력이 막혀 진행 불가) |
| `GameClearUI` | **본 게임 공통**: 조건 3줄을 ★1→★2→★3 순서로 확정하는 체크 연출(연출 중 확인 버튼 비활성), 첫 클리어 시 "휘장 +N" 표시 |
| `GameSystem` | `TutorialBattle` 프로퍼티, `InitializeGameData` 끝에서 `TutorialSystem.ResetProgress()` |
| `StageSelectSystem` | 안내 중 월드 입력 차단(스테이지 클릭 단계는 목표 스테이지만 허용), 씬 준비 후 `NotifySceneEntered(StageSelect)`, 팝업 열림 알림, `TryGetStructureView`, **클리어한 튜토리얼 스테이지 재진입 잠금**(`IsReplayLocked`) |
| `StructureView` | `SetState(unlocked, progress, replayLocked)`(별은 보이되 어둡게+자물쇠), `ModelBounds` |
| `StageInfoPopupUI`, `PromotionUI` | 런타임 생성 카드·버튼에 `TutorialTarget.Configure`로 ID 부여 |
| `GrowthNodeSystem`, `PromotionSystem` | 정적 이벤트 `Purchased(node)`, `PromotionCompleted(hero)` (튜토리얼 시스템이 씬을 넘나들어 인스턴스 이벤트로는 구독이 끊기기 때문) |

## 프리팹·씬 구성

- `--- GAMEMASTER ---.prefab`: 하위 `TutorialSystem`(`sequence` 연결), `Tutorial Canvas`(`TutorialOverlayUI`; 자식 `ActionOrderBar` → `Spotlight` → `Ghost` → `MessageBox` 순서로 그려짐), `Top Canvas/DisPlayUI/Setting`에 중첩 Canvas 600.
- `GameDemoScene`: `Middle Canvas/DiceRollUI`, `Bottom Canvas/TurnOrderUI`, 전투 UI·결과 화면(`CheckMark1~3`, `Insignia_Text`)에 `TutorialTarget`.
- `StageScene`: 구조물 배열에 튜토리얼 구조물을 맨 앞에 추가(스테이지 번호 1), 공용 트리·승급·스테이지 팝업에 `TutorialTarget`.

## 개발 도구 — 스테이지 런처 (`Tools > Debug > Stage Launcher`)

- `튜토리얼 전투` 버튼 아래 옵션 2개(진행 상태를 초기화한다):
  - **튜토리얼 클리어 후 스테이지 선택에서 시작** — 클리어 기록(별 3·휘장 1·스테이지 2 해금)을 만들고 결과 이후(`stage_01`)부터.
  - **성장 튜토리얼 종료 후 스테이지 진입 단계에서 시작** — 위 상태에 공용 트리 C1 구매·매복자 승급 1단계(첫 선택지)까지 끝낸 뒤 `stagein_01`부터.
- `GameSystem.RecordClear`를 리플렉션으로 호출하므로 이름이 바뀌면 깨진다. 중간 단계 이어하기용 `TutorialSystem.DebugResumeAfterBattle`/`DebugResumeAtStep`은 `#if UNITY_EDITOR`.

## 기획서와의 차이 / 알려진 제약

- **기획서 갱신분 미반영**: 기획서(rev 39)는 전투 단계를 27단계로 늘리고(2R "빠른 발 발동 확인" 단계 추가, 19·20번 문구 변경) 전 구간 문구를 자연스러운 문장으로 바꿨다. 코드는 구 판(26단계, 요지 문구) 기준이다 — 문구·단계 동기화는 후속 작업.
- **"가지는 넷"**: 기획서는 공용 트리를 4개의 가지로 서술하지만 실제는 5개(CAPTAIN·RESOURCE·HERO·TACTICS·MIXED). 안내 문구는 개수를 말하지 않는다(기획서 수정 필요).
- 기획서에 없는 단계: 결과 화면의 "확인 버튼 클릭"(`result_03`), 승급 화면 닫기(`end_01`).
- 5단계 "카드를 눌러 확대"는 카드 클릭이 곧 배치 대상 선택이라 **호버 확대 + 다음**으로 대체했다.
- 별 환급 연출은 없다(초기화하면 별 텍스트만 갱신). 보상 영웅 선택(`RewardSelectSystem`)은 어디서도 호출되지 않아(죽은 코드) 튜토리얼과 무관하다.
- 안내 상자는 위쪽 가운데 고정이라 카메라 구도상 판 맨 위(적 깃발 높이)와 겹친다. 상자가 불투명이라 그 부분은 가려진다.
- 호버 정보 패널(`GridSelector.Update`)은 마우스 좌표를 직접 읽어 안내 중에도 막히지 않는다.
- 시작 별·휘장이 0이므로 튜토리얼 전투를 이겨야 성장 파트가 성립한다(시작 데이터로 대체 불가).
- 진행·튜토리얼 상태는 메모리 전용이라 앱을 끄면 튜토리얼도 처음부터다(저장 시스템 없음).
