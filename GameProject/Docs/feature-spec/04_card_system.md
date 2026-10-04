# CardSystem 아키텍처 (용병 카드 — 목표 설계)

> **상태: 최소 범위 구현됨 (2단계, 씬/프리팹 연결은 에디터 작업 대기).** "용병 카드를 AP로 그리드에 배치"하는 카드 시스템이다. 이전 프로젝트의 카드 시스템(git `3ba83bb^`)의 draw/discard/hand 3더미 구조를 이식하되, 카드가 "스킬"이 아니라 "용병 1명"을 뜻하도록 의미를 바꿨다. 구현된 범위와 이식하지 않은 범위는 §4-1을 본다. 구현 코드는 `1.Systems/CardSystem.cs` 외 `2.GameActions/{DrawCardsGA,PlayCardGA}.cs`, `3.Views/{CardView,HandView}.cs`, `4.Creators/CardViewCreator.cs`, `5.Data/CardData.cs`, `Models/Card.cs`.
> `ActionSystem` 동작 원리는 [00_action_architecture.md](./00_action_architecture.md), AP 자원은 [01_hero_system.md](./01_hero_system.md), 토큰 배치 API는 [00_token_system.md](./00_token_system.md)를 먼저 본다. 원작(Master of Piece)에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기하며, 확인되면 [game-design.md](../game-design.md)를 먼저 갱신한다.

## 1. 한 줄 개요

덱에서 카드를 뽑아 손패에 쌓고(드로우), 손패의 카드 1장을 골라 타일을 선택하면 AP가 차감되고 그 타일에 용병이 배치되며 카드는 버린 더미로 간다. 카드 흐름은 `drawPile` ↔ `hand` ↔ `discardPile` 3더미 사이클이고, 각 단계는 `GameAction`(`DrawCardsGA`/`PlayCardGA` 등)으로 흐른다.

## 2. 이식 원본 (git `3ba83bb^`, 현재 워킹트리에는 없음)

| 이전 경로 (`Assets/Game/0_Scripts/…`) | 역할 |
|---|---|
| `1.Systems/CardSystem.cs` | 오케스트레이터(Singleton) — `drawPile`/`discardPile`/`hand` 3더미 보유, 아래 GA 6종의 Performer 소유 |
| `1.Systems/CardViewHoverSystem.cs` | 손패 카드 호버 연출 |
| `2.GameActions/DrawCardsGA.cs` | "N장 드로우" (`Amount`, `IsFirstDraw`) |
| `2.GameActions/DiscardCardGA.cs` | "카드 1장 버림" (`CardView`) |
| `2.GameActions/DiscardAllCardsGA.cs` | "손패 전부 버림" |
| `2.GameActions/DrawCardFromDiscardPileGA.cs` | "버린 더미에서 카드 회수" |
| `2.GameActions/PlayCardTargetingGA.cs` | "카드 사용 전 타일 선택 시작" (`Card`, `EndSelectAction`) |
| `2.GameActions/PlayCardGA.cs` | "카드 사용 확정" (`Card`, `TargetPoses`, `IsPart1`) |
| `3.Views/CardView.cs`, `CardViewInPile.cs`, `HandView.cs` | 카드/더미/손패 뷰 |
| `4.Creators/CardViewCreator.cs` | 카드 뷰 생성 |
| `5.Data/CardData.cs`, `Models/Card.cs` | 카드 SO / 런타임 래퍼 |
| `UI/PileofCardUI.cs`, `UI/CheckDeckUI.cs` | 더미 개수·덱 확인 UI |

이전 구조의 동작(요약):
- `SetUp(deckData)`: 덱을 셔플해 `drawPile` 구성.
- `DrawCardsPerformer`: 뽑을 장수만큼 `drawPile`에서 손패로. `drawPile`이 모자라면 `discardPile`을 셔플해 `drawPile`로 채운다(`RefillDect`).
- `PlayCardPerformer`: 손패에서 카드를 빼 `discardPile`로 보내고, `SpendManaGA(card.Mana)`를 `AddReaction`한 뒤, 카드 효과를 `PerformEffectGA`로 위임한다.
- 사용 진입은 `PlayCardTargetingGA` → (타일 선택 코루틴, 그리드 클릭) → `PlayCardGA`를 `AddReaction`하는 순서.

## 3. 새 매핑 (이식 시 바꾸는 부분)

| 이전 | 목표 | 비고 |
|---|---|---|
| `CardData.Mana` | AP 비용 (`CardData`의 `int` 필드, SO로 노출) | 자원은 `APSystem` / `SpendAPGA` 재사용. 현재 `SpendAPGA`는 amount 인자(기본 1)를 지원 |
| `CardData`의 `SelfEffects`/`GridTargetMode`/`CardType`/`CardSubType` | `HeroData`(영웅 SO) 참조 1개 | 카드가 "용병 1명"을 가리킨다. 효과/타겟 모드 필드는 쓰지 않음 |
| `PlayCardGA` 효과(`PerformEffectGA`로 카드 효과 실행) | "선택한 타일에 해당 용병 토큰 배치" | 배치 창구 후보: `TokenMainAPI`의 토큰 추가 계열(웨이브 몬스터 런타임 생성에 쓰이는 경로, 구현 시 확인). 초기 일괄 배치용 `TokenSetup.StartSetUpHero`는 전투 시작용이라 그대로 재사용 불가 |
| `PlayCardTargetingGA`의 타일 선택(공격 범위 VG) | `CardSystem` 코루틴(`CardTargetMode`)이 "배치 가능한 빈 타일" 선택 — **GA 없음** | `SkillSystem`과 같은 방식: 확정 시 `PlayCardGA`를 `reservedCards` 큐에 넣고 `Update`에서 `IsPerforming == false`일 때 `Perform`. 이전엔 `CardView`가 `ActionSystem.Perform`을 직접 호출했으나 `convention.md` 6절 위배라 바꿈. 배치 가능 타일 = `StageData.HeroSetupPoses` 중 `TokenServiceAPI.IsGridEmpty`인 곳(임시, 구역 제한은 (원작 확인 필요)) |
| `InteractionSystem.*` 입력 플래그 | 현재 코드의 `Interactions`(`GridSelected`, `CancelUse` 등) | 이전과 이름이 다름 — 구현 시 현재 클래스로 맞춘다 |

`IsPart1`(카드 1회 사용을 두 단계 `PlayCardGA`로 나누는 이전 플래그)은 용병 배치에는 필요 없으므로 이식하지 않는다.

## 4. 사용 흐름 (목표)

```
[손패의 카드 클릭]  (CardView.OnPointerClick)
 └ ActionSystem 수행 중 / 스킬 타겟·이동 모드 중이면 무시
 └ APSystem.HasEnoughAP(card.APCost) 체크 — 부족하면 사운드만 재생하고 종료
 └ CardSystem.PlayCardTargetMode → CardTargetMode 코루틴
     ├ Interactions.IsCardTargetMode = true, 배치 가능 타일에 VG(Hero_SetUp_True) 표시
     ├ 다른 카드 클릭 = 선택 카드 변경, 같은 카드 재클릭/CancelUse = 취소
     └ 유효 타일 클릭 → PlayCardGA(card, tile)를 reservedCards 큐에 Enqueue

[CardSystem.Update]  reservedCards가 있고 IsPerforming == false → ActionSystem.Perform(PlayCardGA)

[PlayCardGAPerformer]
 └ 카드를 hand → discardPile로 이동(연출 포함)
 └ SpendAPGA(card.APCost)를 AddReaction
 └ TokenMainAPI.AddToken으로 해당 타일에 용병 토큰 배치 (HeroView.SetUp → 특성 구독 시작, 05_trait_system.md)
 └ HeroSystem.CurrentHero가 비어 있으면 방금 배치한 영웅으로 지정

[TurnGA 리액션]
 └ Player 턴 POST: DrawCardsGA(drawAmount)  (AP 리필 이후)
 (턴 종료 시 손패 버림 없음 — 손패는 유지되고 카드는 사용 시에만 버린 더미로 간다. 매 Player 턴 드로우가 기존 손패 위에 누적됨)
 (정확한 시점/드로우 수: 원작 확인 필요 — 현재 drawAmount는 인스펙터 임시값)
```

## 4-1. 구현 범위 (2단계)

- **구현됨**: `drawPile`/`discardPile`/`hand` 3더미 + 덱 소진 시 버린 더미 셔플 보충, `DrawCardsGA`/`PlayCardGA`, 클릭 선택 → 타일 선택 → 배치, AP 비용 체크·차감, 시작 영웅 일괄 배치(`SetUpUI`/`TokenSetup.StartSetUpHero`)를 대체(`MatchSetupSystem`이 `CardSystem.SetUp()` 호출).
- **이식하지 않음**: 호버 확대(`CardViewHoverSystem`), 드래그 사용, 더미/덱 확인 UI(`PileofCardUI`/`CheckDeckUI`/`CardViewInPile`), `DiscardCardGA`, `DrawCardFromDiscardPileGA`, 첫 드로우 잠금(`IsFirstDraw`), `LockDiscarding`.
- **임시 값**: 덱(`CardSystem.startingDeck`), 드로우 수(`drawAmount`), 카드 AP 비용(`CardData.APCost`)은 인스펙터/SO 값이며 원작 수치가 아니다.
- `CardData.Unit`은 `GameSystem.HeroDatas`에 등록된 `HeroData`여야 한다(`HeroView.SetUp`이 `heroData.Hero`를 쓰는데 `Hero`는 `GameSystem.InitializeHero`가 그 배열에 대해서만 만든다).
- 같은 `HeroData` 카드를 두 장 이상 배치하면 특성 `owner` 덮어쓰기 문제가 있다(05_trait_system.md §3).

## 5. 기존 코드에 미치는 영향

- **영웅이 "전투 시작 시 고정 배치"에서 "런타임 배치"로 바뀐다.** 현재 `HeroSystem.HeroViews`와 `MatchSetupSystem`/`TokenSetup.StartSetUpHero` 흐름은 영웅이 처음부터 존재한다는 전제다. 런타임에 영웅이 늘어나므로 `HeroViews`를 쓰는 곳(턴 시작 `ReduceSEWhenMyTurnStart` 등)과 `HeroSystem.CurrentHero` 선택 흐름을 점검해야 한다.
- 이전 카드 시스템은 **영웅 1명(`HeroSystem.Instance.HeroView`)** 을 전제로 작성되었다. 다영웅 구조에 맞게 시전자/기준 위치 참조를 쓰지 않는다(배치 카드는 시전자가 필요 없다).
- `SkillSystem`과 입력 플래그(`Interactions.IsSkillTargetMode` 등)를 공유하므로, 카드 배치 동작을 확인한 뒤 `SkillSystem`을 제거한다(제거 순서: 카드 → 특성 → Skill 제거).
- `CardViewCreator`/`CardViewHoverSystem`/`HandView`는 UGUI 프리팹과 씬 배치가 필요하다 — 에디터 작업은 사용자 몫이며 구현 단계에서 클릭 순서를 안내한다.

## 6. 원작 확인 필요 항목

- 원작에도 드로우/버림/손패 사이클이 있는가, 아니면 덱 전체가 곧 선택지인가
- 라운드당 드로우 장수, 손패 상한(턴 종료 버림이 없어 손패가 누적되므로 상한 필요 여부 포함), 덱 소진 시 처리
- 배치된 용병 카드가 전투 후 버린 더미로 돌아오는 시점(바로 vs 라운드 종료)과 사망 시 처리
- AP 최대치·카드별 AP 비용 수치, AP가 라운드마다 리필되는지
- 배치 가능 타일 구역 제한
- 카드 획득·덱 구성(모집) 경로 — 이 문서 범위 밖
