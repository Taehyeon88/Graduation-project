# 소문(Rumor) 카탈로그

> 소문 하나가 어떤 `Rumor` 구현체로 연결되는지 **소문 단위**로 정리한 문서. 구조·계약·부품 표는 [rumor.md](./rumor.md).
> 새 소문(`RumorData` 에셋)을 만들면 이 문서에 섹션을 추가한다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> **현재 수록: 3개** — 1 강력한, 2 유연한, 3 숙달된. 에셋은 `Assets/Game/1_Datas/14.Rumors/`에 있고 `Image`는 비어 있다. 획득 경로가 없어 Play 확인은 하지 않았다.

## 1 · 강력한

에셋: `Assets/Game/1_Datas/14.Rumors/1_강력한.asset`

| 항목 | 값 |
|---|---|
| Id | 1 |
| 설명 (`Description`) | 공격력 +3을 얻습니다. 배치 시 소모되는 행동력이 1증가합니다. |
| 소문 클래스 | `PowerfulRumor` (`IAPCostModifier` 구현) |
| 수치 | `attackBonus` = 3, `apCostBonus` = 1 |
| 동작 | 배치 직후 `OnAdd`가 `ModifyStat(DAMAGE, 3)`을 호출한다. `Card.APCost`가 기본 AP에 `APCostBonus`를 더한다 |
| 파생 GA | 없음 (`ModifyStat` 직접 호출) |
| 코드 | `Rumors/PowerfulRumor.cs`, `Interfaces/IAPCostModifier.cs`, `Models/Card.cs` |

**연결 흐름**

```
카드 표시/사용: Card.APCost = Hero.Data.APCost + (Hero.Rumor.Rumor is IAPCostModifier → APCostBonus)
                 └ CardView(표시·사용 가능 판정) / CardSystem.PlayCardGAPerformer(SpendAPGA)

배치: HeroView.SetUp(hero)
       └ hero.Rumor.Rumor.Clone().OnAdd(HeroView) → ModifyStat(DAMAGE, attackBonus)
```

**주의·한계**
- 공격력은 영구가 아니라 **배치된 개체**에만 적용된다. 영웅이 죽거나 제거되면 다음 배치 때 `OnAdd`가 다시 적용된다(`Hero.DamageBonus`는 바뀌지 않는다).
- 보상 후보 표시용 임시 개체(`new Hero(data)`)에는 소문이 없다.
- 비용 +1은 카드가 손패에 있을 때부터 표시에 반영된다. 원작에서 비용 표기 시점은 (원작 확인 필요).

**에디터 설정**: `Rumor`=`PowerfulRumor`(`attackBonus`=3, `apCostBonus`=1).

## 2 · 유연한

에셋: `Assets/Game/1_Datas/14.Rumors/2_유연한.asset`

| 항목 | 값 |
|---|---|
| Id | 2 |
| 설명 (`Description`) | 중립 타일에 배치할 수 있습니다. 배치 시 소모되는 행동력이 1 감소합니다. |
| 소문 클래스 | `FlexibleRumor` (`IAPCostModifier`, `INeutralPlacement` 구현, 훅 미사용) |
| 수치 | `apCostReduction` = 1 |
| 동작 | `Card.APCost`가 기본 AP에 `-1`을 더하고 0 미만이면 0. `Card.CanPlaceOnNeutral`이 true면 `CardSystem.GetPlaceablePoses`가 기본 배치 열(x=0)의 **토큰 없는** 중립 칸을 후보에 추가한다 |
| 파생 GA | 없음 |
| 코드 | `Rumors/FlexibleRumor.cs`, `Interfaces/INeutralPlacement.cs`, `Models/Card.cs`, `1.Systems/CardSystem.cs`, `1.Systems/HeroSystem.cs`(`NeutralSetupPoses`), `SubSystems/TokenSetup.cs`(`CalcNeutralSetupPoses`), `1.Systems/MatchSetupSystem.cs`, `Models/TokenGrid.cs`(`neutralCells`) |

**연결 흐름**

```
MatchSetupSystem → HeroSystem.SetUp(SetupPoses, NeutralSetupPoses = NeutralPositions 중 x==0)
카드 선택/변경: CardSystem.GetPlaceablePoses(card)
  └ SetupPoses 중 IsGridEmpty  +  (card.CanPlaceOnNeutral이면) NeutralSetupPoses 중 GetTokenByPosition == null
배치: PlayCardGAPerformer → AddHero → TokenGrid.SetToken (중립 칸도 simpleGrid 이미 1이라 그대로 등록)
이동·사망: TokenGrid.RemoveToken / ChangeTokenPos → FreeCell → neutralCells면 칸을 막은 채 유지
```

**주의·한계**
- 중립지대는 변하지 않는다. 이 소문의 영웅만 올라갈 수 있고, 떠나거나 죽어도 다른 유닛은 그 칸에 들어갈 수 없다.
- 후보는 **기본 배치 열(x=0)** 의 중립지대뿐이다(범위는 사용자 결정, 원작 확인 필요). 같은 라인에서 일반 배치 칸과 중립 칸이 모두 후보라 한 라인에 영웅을 앞뒤로 배치할 수 있다.
- 배치 구역 표시(`Zone_Hero`)에는 중립 칸이 포함되지 않고, 카드 선택 후 마우스를 올렸을 때만 하이라이트된다.
- 비용 하한 0은 `Card.APCost` 전체에 적용된다(다른 소문의 음수 보정도 같은 하한).
- `Hero.AddRumor` 호출 경로가 없어 Play 확인은 임시 호출이 필요하다.

**에디터 설정**: `Rumor`=`FlexibleRumor`(`apCostReduction`=1).

## 3 · 숙달된

에셋: `Assets/Game/1_Datas/14.Rumors/3_숙달된.asset`

| 항목 | 값 |
|---|---|
| Id | 3 |
| 설명 (`Description`) | 전장에 있는 동안 매 라운드 종료 시 행동력+1개를 추가로 획득합니다. |
| 소문 클래스 | `MasteredRumor` |
| 수치 | `apAmount` = 1 |
| 구독 GA / 타이밍 | `TurnGA` / POST, `TurnType.Enemy`일 때만 (라운드 종료 = 몬스터 턴 종료로 가정) |
| 동작 | `RefillAPGA(apAmount)`를 `AddReaction`. 이후 `TurnGA(Player)` PRE에서 `APSystem`이 턴 시작 AP를 채운다 |
| 파생 GA | `RefillAPGA` ([action_command.md](../action/action_command.md)) |
| 코드 | `Rumors/MasteredRumor.cs`, `1.Systems/APSystem.cs`(변경 없음) |

**연결 흐름**

```
배치: HeroView.SetUp → Rumor.Clone().OnAdd → SubscribeReaction<TurnGA>(POST)
몬스터 턴 종료: TurnGA(Enemy) POST → MasteredRumor.TurnGAPostReaction → AddReaction(RefillAPGA(1))
퇴장: HeroView.OnDisable → Rumor.OnRemove → UnsubscribeReaction<TurnGA>(POST)
```

**주의·한계**
- `RefillAPGA`는 `MaxAP`(5)로 잘린다. 추가 AP가 직후 턴 시작 +3과 합쳐져 상한을 넘으면 사라진다. 상한을 넘길 수 있는지는 (원작 확인 필요).
- 승리 시 `TurnSystem`이 몬스터 턴을 만들지 않아 클리어로 끝난 라운드에는 발동하지 않는다.
- 숙달된 영웅이 2명이면 각각 구독하므로 +1씩 중첩된다.
- 소문 구독은 `TurnGA` 전체에 걸리므로, 같은 구독 방식의 다른 소문과 발동 순서는 보장하지 않는다.

**에디터 설정**: `Rumor`=`MasteredRumor`(`apAmount`=1).
