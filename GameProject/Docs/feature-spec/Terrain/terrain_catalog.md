# 지형 카탈로그 — 장애물 · 발판

> 스테이지에 놓이는 **파괴 가능 장애물**과 **발판**을 에셋 단위로 정리한 문서. 아키텍처·생성 순서·중립지대는 [08_terrain_obstacle.md](../08_terrain_obstacle.md)를 본다. 새 장애물·발판을 만들면 이 문서에 섹션을 추가한다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> **현재 수록: 장애물 2개** (돌격 화살통, 야전 공구함), **발판 2개** (풀숲, 언덕). 중립지대는 위치만 가지는 데이터 없는 지형이라 이 카탈로그의 항목이 아니다 (08 §3).

## 구조

| 구성 | 장애물 | 발판 |
|---|---|---|
| SO | `5.Data/ObstacleData.cs` (`CombatantData` 상속: `Id`, `Sprite`, `Health`, `Damage`, `Speed`, `Traits` + `Description`) | `5.Data/TerrainData.cs` (`Id`, `Description`, `Sprite`, `Effect`(`TerrainEffect`, `[SerializeReference, SR]`), 이름은 SO 에셋 이름) |
| 기능 | 특성 부품 (`TraitCondition`/`TraitEffect`) | `Models/TerrainEffect.cs` (추상: `OnEnter`/`OnExit`/`Clone`) → `Terrains/` 구현체 |
| 배치 데이터 | `5.Data/ObstacleSpawn.cs` (`Position`, `Data`) → `StageData.ObstacleSpawns` | `5.Data/TerrainSpawn.cs` (`Position`, `Data`) → `StageData.TerrainSpawns` |
| 뷰 | `3.Views/ObstacleView.cs` (`CombatantView` 상속) | `3.Views/TerrainView.cs` |
| 생성 | `TokenSetup.SetUpObstacles` → `TokenMainAPI.AddToken(TokenType.Obstacle)` → `TokenCreator` | `TokenSetup.SetUpTerrains` → `TokenCreator.CreateTerrain` (`Instantiate`, 딕셔너리 보관) |
| 토큰 여부 | 예 (칸을 점유, 공격 대상) | 아니오 (`simpleGrid` 미점유, 이동 가능, 공격 대상 아님) |
| 에셋 위치 | `Assets/Game/1_Datas/11.Obstacles/` | `Assets/Game/1_Datas/12.Terrains/` |
| 프리팹 | `3_Prefabs/Views/ObstacleView.prefab` (`TokenCreator.obstaclePrefab`) | `3_Prefabs/Views/TerrainView.prefab` (`TokenCreator.terrainPrefab`) |

- 생성 순서(`MatchSetupSystem.StartSetting`): 맵 타일 → 중립지대 → **장애물** → **발판** → 배치 위치 계산(`CalcSetupPoses`) → 몬스터/웨이브.
- 장애물/발판 모두 점유·범위 밖·데이터 없음이면 `LogError` 후 건너뛴다.
- 씬에는 `TerrainSystem` GameObject(리액션 전용, 인스펙터 연결 없음)가 `--- SYSTEMS ---` 아래에 1개 있어야 하고, `TokenCreator.terrainPrefab`에 `TerrainView.prefab`이 연결돼야 한다.

## 요약

### 장애물

| Id | 이름 | 설명 | 체력/공격력/속도 | 특성 | 에셋 |
|---|---|---|---|---|---|
| 3001 | 돌격 화살통 | 이 장애물을 파괴한 용병이 모든 적 용병을 추가로 공격합니다. | 4 / 0 / 0 | 9001 돌격 화살통 | `돌격 화살통.asset` |
| 3002 | 야전 공구함 | 피해를 받을 때마다 공격한 영웅에게 공격력 +1, 체력 +1을 부여합니다. | 4 / 0 / 0 | 9002 야전 공구함 | `야전 공구함.asset` |

### 발판

| Id | 이름 | 설명 | 효과 | 에셋 |
|---|---|---|---|---|
| 1 | 풀숲 | 이 타일 위의 용병은 회피를 얻습니다. | 회피 +1 (`StatusEffectType.DODGE`) | `풀숲.asset` |
| 2 | 언덕 | 매 라운드 종료 시 이 타일 위의 영웅을 바로 뒤를 제외한 다른 모든 타일로 이동할 수 있습니다. 이 효과로 이동한 영웅은 공격력 +1, 이동력 +1을 얻습니다. | 이동 범위 확장 + 공격력 +1·속도 +1 | `언덕.asset` |

> 설명은 `ObstacleData.Description` / `TerrainData.Description`(`[TextArea]`)에 저장한다. 돌격 화살통은 특성 9001의 `Description`과 같은 문구를 쓴다.

---

# A. 장애물

## 3001 · 돌격 화살통

에셋: `Assets/Game/1_Datas/11.Obstacles/돌격 화살통.asset`

| 항목 | 값 |
|---|---|
| Id | 3001 |
| 설명 | 이 장애물을 파괴한 용병이 모든 적 용병을 추가로 공격합니다. (`ObstacleData.Description`, 특성 9001과 동일 문구) |
| 체력 / 공격력 / 속도 | 4 / 0 / 0 |
| 특성 | `Traits[0]` = `3.Traits/돌격 화살통.asset` (Id 9001) |
| 조건 | `SelfDeathCondition` — 소유자 사망 시 통과 (`Traits/Conditions/SelfDeathCondition.cs`) |
| 타이밍 | PRE (`reactionTiming: 0`) — `KillGA` Performer가 토큰을 파괴하면 `OnDisable`이 구독을 해제해 POST에서는 놓칠 수 있다 |
| 효과 | `KillerAttackAllOpponentsEffect` — 처치자 진영의 상대 전원에 `DealDamageGA(owner.BaseDamage, targets, owner)` 예약 (피해량·Caster = 장애물) |
| 스프라이트 | `Sprite` 지정됨 (임시 에셋) |
| 배치 예시 | `TestStage.ObstacleSpawns` — (1, 1) |
| 공격 대상 | 영웅·적 모두 (`HeroView.IsTarget`/`EnemyView.IsTarget`에 `ObstacleView` 포함) |
| 행동 | 스스로 행동하지 않음 (`ObstacleView`의 `Forward`가 zero라 `Battle()`이 즉시 끝남) |
| 플레이어 스킬 | 지정 불가 — `SharpDagger.IsValidTarget`이 `EnemyView`/`EnemyFlagView`만 허용하는 허용 목록 방식 |
| 코드 | `3.Views/ObstacleView.cs`, `5.Data/ObstacleData.cs`, `5.Data/ObstacleSpawn.cs`, `Traits/KillerAttackAllOpponentsEffect.cs`, `2.GameActions/KillGA.cs`, `3.Views/CombatantView.cs`, `SubSystems/TokenSetup.cs`, `4.Creators/TokenCreator.cs` |

**연결 흐름**

```
영웅/적이 정면 칸에 장애물이 있을 때 CombatantView.ReserveAttack
 └ DealDamageGA(Caster = 영웅 또는 적, 대상 = ObstacleView)
     └ CombatantView.Damage: 체력 ≤ 0
         └ KillGA(token = 장애물, Killer = dealDamageGA.Caster) 를 PostReactions에 예약
             ├ PRE 구독자 → Trait.Reaction (장애물의 특성 9001)
             │   ├ SelfDeathCondition: 죽는 토큰이 소유자(장애물)인가
             │   └ KillerAttackAllOpponentsEffect
             │       ├ killer가 HeroView → EnemyViews 전체
             │       ├ killer가 EnemyView → HeroViews 전체
             │       └ DealDamageGA(owner.BaseDamage, targets, owner) 예약 (대상이 없거나 Killer가 null/비 Combatant면 아무 일도 안 함)
             └ DamageSystem.KillGAPerformer → TokenMainAPI.RemoveToken
```

**주의·한계**
- **계약 변경**: `KillGA`에 `Killer`(`Token`, 기본 null)를 추가했다. 생성자 `KillGA(token, hit_Tween, killer = null)`의 세 번째 인자는 선택이라 기존 호출부는 그대로 유효하다.
- 처치자가 없는 경우(플레이어 스킬 등)에는 효과가 아무 일도 하지 않으며, 코어는 공격하지 않는다.
- 추가 피해량은 **장애물 자신의 `BaseDamage`**(`ObstacleData.Damage`, 최소 1 보정이라 현재 1)이고 `Caster`도 장애물이라 처치자의 힘·치명타·취약 보정이 섞이지 않는다 (사용자 결정). 처음에는 처치자의 `BaseDamage`와 `Caster = 처치자`로 구현했으나, 처치자 공격력이 2 이상이면 체력 2 몬스터가 죽는 문제가 확인되어 바꿨다 **(원작 확인 필요)**. 처치자는 어느 진영을 칠지 정하는 데만 쓴다.
- 부작용: 추가 공격의 `Caster`가 장애물이라 처치자의 공격 관련 특성(`Caster == owner` 조건)은 추가 공격에 발동하지 않고, 추가 공격으로 죽은 몬스터의 `KillGA.Killer`도 장애물이다. 피해자의 취약(×1.5, 올림)과 회피는 그대로 적용된다.
- 적이 파괴했을 때도 같은 효과(영웅 전원 추가 공격)가 나가도록 구현했다 **(원작 확인 필요)**.
- 추가 공격에 코어(깃발)는 포함하지 않았다 **(원작 확인 필요)**.
- 장애물의 공격력이 0이어도 `CombatantView.BaseDamage`가 최소 1로 보정되지만, 공격하지 않으므로 영향 없다.
- `AddToken`의 `EnemyView`/`HeroView` 분기에 걸리지 않아 `EnemyViews`/`HeroViews` 목록에는 들어가지 않는다 — 추가 공격 대상에도 포함되지 않는다.
- 에셋 특성의 `Reaction Timing`이 POST면 효과가 나가지 않을 수 있다. 반드시 PRE(0)로 둔다.
- **Play 테스트는 하지 않았다.** 확인 항목: 영웅/적이 정면 장애물을 공격하는지, 파괴 시 처치자가 상대 전원을 추가 공격하는지, 날카로운 단도로 지정되지 않는지, 장애물이 스스로 움직이거나 공격하지 않는지.

**에디터 설정**: `ObstacleData` 에셋 `Id`=3001, `Health`=4, `Damage`=0, `Speed`=0, `Traits`에 특성 9001 등록. `TokenCreator.obstaclePrefab`=`ObstacleView.prefab` 연결 완료. `StageData.ObstacleSpawns`에 (위치, 데이터) 입력 (`TestStage`: (1, 1)).

## 3002 · 야전 공구함

에셋: `Assets/Game/1_Datas/11.Obstacles/야전 공구함.asset`

| 항목 | 값 |
|---|---|
| Id | 3002 |
| 설명 | 피해를 받을 때마다 공격한 영웅에게 공격력 +1, 체력 +1을 부여합니다. (`ObstacleData.Description`, 특성 9002와 동일 문구) |
| 체력 / 공격력 / 속도 | 4 / 0 / 0 |
| 특성 | `Traits[0]` = `3.Traits/야전 공구함.asset` (Id 9002, `OwnerType` 2) |
| 조건 | `SelfDamagedCondition` — 구독 `DealDamageGA`, 판정: `DamagedTargets.Contains(owner)` (생존 여부 무관, 파괴 타격 포함). `FiltersByCaster` = false |
| 타이밍 | POST (`reactionTiming: 1`) — `DamagedTargets`는 피해 적용 뒤에 채워진다. `KillGA`는 `DealDamageGA`의 PostReactions에 들어가므로 조건 판정이 구독 해제보다 먼저 실행된다 |
| 효과 | `ModifyStatAttackerEffect` — `modifiers = [DAMAGE +1, HEALTH +1]` |
| 효과 동작 | `DealDamageGA.Caster`가 `CombatantView`면 그 유닛에게 `ModifyStatGA(DAMAGE, 1)`, `ModifyStatGA(HEALTH, 1)` 예약 (caster = 장애물) |
| 스프라이트 | `Sprite` 지정됨 |
| 공격 대상 / 행동 / 스킬 | 돌격 화살통과 동일 (영웅·적 모두 공격, 스스로 행동 안 함, 플레이어 스킬 지정 불가) |
| 코드 | `Traits/Conditions/SelfDamagedCondition.cs`, `Traits/ModifyStatAttackerEffect.cs`, `2.GameActions/DealDamageGA.cs`, `1.Systems/StatSystem.cs` |

**연결 흐름**

```
영웅/적이 정면 칸의 장애물을 공격: DealDamageGA(Caster = 공격자, 대상 = 장애물)
 └ DamageSystem: target.Damage() 후 DamagedTargets.Add(장애물)
 └ DealDamageGA POST
     └ Trait.Reaction (FiltersByCaster == false → Caster != owner 필터 건너뜀)
         └ SelfDamagedCondition.SubConditionIsMat (장애물이 DamagedTargets에 있는가)
             └ ModifyStatAttackerEffect.PerformReaction
                 └ AddReaction(ModifyStatGA, DAMAGE +1 / HEALTH +1, 대상 = 공격자)
 └ (체력 ≤ 0이면) KillGA가 PostReactions에서 이후 수행 → 장애물 파괴
```

**주의·한계**
- **설명과 다른 점**: 설명은 "공격한 영웅"이지만 공격한 **적에게도** 부여한다 (사용자 결정, 원작 확인 필요).
- 파괴되는 마지막 타격에서도 부여한다 (사용자 결정). 위 흐름의 순서 가정은 Play에서 확인해야 한다.
- 공격자가 없거나 `CombatantView`가 아닌 피해(코어, 스킬 등)는 무동작이다.
- 회피로 건너뛴 피격은 `DamagedTargets`에 없어 발동하지 않는다 (장애물은 회피가 없어 사실상 해당 없음).
- 효과가 예약하는 `ModifyStatGA`는 조건의 구독 대상(`DealDamageGA`)이 아니므로 무한 루프는 없다.
- 발동 횟수 상한이 없다 (설명에 상한 언급 없음).
- `StatSystem`이 씬에 배치되어 있어야 동작한다 (`StageScene`에는 없고 `GameDemoScene` 등에 있음).
- **Play 테스트는 하지 않았다.** 확인 항목: 정면 장애물을 공격할 때마다 공격자 공격력·체력 +1, 파괴 타격에서도 부여, 날카로운 단도로 지정되지 않는지.

**에디터 설정**: `ObstacleData` 에셋 `Id`=3002, `Health`=4, `Damage`=0, `Speed`=0, `Traits`에 특성 9002 등록 완료. `StageData.ObstacleSpawns`에 (위치, 데이터) 입력은 아직 하지 않았다.

---

# B. 발판

## 1 · 풀숲

에셋: `Assets/Game/1_Datas/12.Terrains/풀숲.asset`

| 항목 | 값 |
|---|---|
| Id | 1 |
| 이름 | 풀숲 (SO 에셋 이름, `TerrainData.Name`) |
| 설명 | 이 타일 위의 용병은 회피를 얻습니다. (`TerrainData.Description`) |
| 기능 | `Effect` = `BushEffect` — 올라서면 `AddStatusEffectGA(DODGE, dodgeStack)`, 떠나면 회수 |
| 수치 | `dodgeStack` = 1 (`[SerializeField]`) |
| 스프라이트 | `Sprite` 지정됨 (임시 에셋) |
| 배치 예시 | `TestStage.TerrainSpawns` — (2, 2) |
| 보관 | `TokenSetup`의 `Dictionary<Vector2Int, TerrainView>` (토큰이 아니므로 `grid.SetToken`을 쓰지 않음) |
| 타일 뷰 | 바닥 타일 복사 프리팹, iso z 0.01, sortingOrder -9 (높이는 프리팹 값 유지) |
| 회피 추적 | `BushEffect.granted` (`HashSet<CombatantView>`) — 발판별 사본이 자기가 회피를 준 유닛을 보유 (`Clone()`에서 새 집합 생성) |
| 코드 | `Models/TerrainEffect.cs`, `Terrains/BushEffect.cs`, `3.Views/TerrainView.cs`, `1.Systems/TerrainSystem.cs`, `SubSystems/TokenSetup.cs`, `4.Creators/TokenCreator.cs`, `5.Data/TerrainData.cs`, `5.Data/TerrainSpawn.cs`, `UI/StatusEffectUI.cs` (아이콘 표시) |

**연결 흐름**

```
MatchSetupSystem → TokenSetup.SetUpTerrains → TokenCreator.CreateTerrain → TerrainView.SetUp(data) : data.Effect.Clone() 보관

MoveGA (POST) → TerrainSystem.MoveGAPostReaction
 ├ 모든 TerrainView.Exit(mover) → BushEffect.OnExit : 자기 granted에 있으면 RemoveStatusEffect(DODGE) 후 제거
 └ movePosition이 발판 칸 → TerrainView.Enter(mover) → BushEffect.OnEnter

PlayCardGA (POST) → TerrainSystem.PlayCardGAPostReaction
 └ TargetPos가 발판 칸이고 그 칸의 토큰이 CombatantView → TerrainView.Enter(unit) → BushEffect.OnEnter

BushEffect.OnEnter(unit)
 ├ granted에서 파괴된 유닛 정리 → unit 추가
 └ ActionSystem.AddReaction(AddStatusEffectGA(DODGE, dodgeStack, [unit], unit))
```

**주의·한계**
- **미구현**: "타일 위의 용병이 공격을 받으면 이 지형지물이 제거되며, 2라운드 후 다시 생성된다" (`TurnGA(Player)` PRE 카운트로 구현 예정, 08 §6).
- 웨이브로 스폰되는 적이 풀숲 칸에 나오면 회피를 받지 않는다 (`PlayCardGA`/`MoveGA` 경로가 아님).
- 회피 스택에 출처 구분이 없다. 풀숲 회피를 이미 소모한 뒤 떠날 때 다른 출처의 회피가 있으면 그것이 1 회수된다 (회수 여부 자체는 발판별 `granted`로 판단).
- `TerrainData.Effect`가 비어 있으면 발판은 표시만 되고 아무 효과도 없다 (`TerrainView.Enter/Exit`가 null 무시).
- 토큰이 아니라서 공격 대상이 되지 않고, `simpleGrid`를 건드리지 않아 이동은 가능하다.
- 발판은 중립지대·장애물·이미 있는 발판 위에는 놓을 수 없다 (`grid.CanSet` + 중복 검사, `TokenSetup.SetUpTerrains`).
- `TerrainSystem`은 `OnEnable`/`OnDisable`에서 `MoveGA`·`PlayCardGA` POST 구독을 대칭으로 등록·해제한다.
- **수정된 오류 (아이콘)**: 풀숲에 들어간 영웅에게 회피 대신 힘 아이콘이 표시됐다. 부여 자체는 정상(`DODGE`, 스프라이트 `회피`)이었고, 영웅이 쓰는 `StatusEffectUIAtWorld.prefab`의 `Image`에 힘 아이콘이 기본 스프라이트로 박혀 있는데 `StatusEffectUI.Set`이 전달받은 스프라이트보다 기존 값을 우선해서 생긴 문제였다. `Set`을 `if (sprite != null) image.sprite = sprite;`로 바꿔 전달된 스프라이트를 우선하도록 고쳤다 (스택 감소 시 `sprite`가 null로 오면 기존 아이콘 유지). 이 수정은 풀숲뿐 아니라 모든 상태효과 아이콘에 적용된다. 프리팹의 기본 스프라이트를 비우는 것은 선택 사항이다.
- **Play 테스트는 일부만 했다.** 소환 시 `DODGE`·`회피` 스프라이트 부여는 로그로 확인했다. 아이콘 수정 후 확인 항목: 풀숲에 소환하거나 이동해 들어가면 회피 아이콘 +1, 나가면 -1, 풀숲 타일이 바닥 위에 올바르게 정렬되어 보이는지.

**에디터 설정**: `TerrainData` 에셋 `Id`=1, `Sprite` 지정, `Effect`=`BushEffect`(`dodgeStack`=1). `TokenCreator.terrainPrefab`=`TerrainView.prefab` 연결 필요(미확인 — 사용자 작업). `StageData.TerrainSpawns`에 (위치, 데이터) 입력 (`TestStage`: (2, 2)).

## 2 · 언덕

에셋: `Assets/Game/1_Datas/12.Terrains/언덕.asset`

| 항목 | 값 |
|---|---|
| Id | 2 |
| 이름 | 언덕 (SO 에셋 이름) |
| 설명 | 매 라운드 종료 시 이 타일 위의 영웅을 바로 뒤를 제외한 다른 모든 타일로 이동할 수 있습니다. 이 효과로 이동한 영웅은 공격력 +1, 이동력 +1을 얻습니다. |
| 기능 | `Effect` = `HillEffect` (`TerrainEffect` + `IExtendedMoveTerrain`). `OnEnter`/`OnExit`는 비어 있다 |
| 수치 | `damageBonus` = 1, `speedBonus` = 1 (`[SerializeField]`) |
| 이동력 해석 | 속도(`StatType.SPEED`) — 원작 확인 필요 |
| 스프라이트 | `Sprite` 지정됨 |
| 코드 | `Terrains/HillEffect.cs`, `Interfaces/IExtendedMoveTerrain.cs`, `1.Systems/MoveSystem.cs`(`CalcMovePaths`, `Update`, `TryGetExtendedMoveTerrain`) |

**연결 흐름**

```
MoveSystem.CalcMovePaths(hero)  (플레이어 이동 모드 진입·영웅 변경 시)
 ├ 기본: 라운드 시작 위치 기준 3x3 BFS 경로
 └ hero가 아직 라운드 시작 칸에 있고 그 칸 발판의 Data.Effect가 IExtendedMoveTerrain이면
    └ 그리드 전체 칸 중 빈 칸 · 적 배치 구간 아님 · 기본 범위에 없음 · AllowsExtendedMove(바로 뒤 제외)인 칸을
       paths[칸] = [칸] (직접 점프)로 추가, extendedDests에 기록

PlayerMoveMode: 클릭한 칸이 extendedDests에 있으면 extendedMoves[PerformMoveGA] = 발판 효과

MoveSystem.Update: PerformMoveGA를 꺼낸 뒤 AP 확인 통과 후
 └ 확장 이동이고 extendedBonusUsed.Add(hero) 성공(영웅당 라운드 1회)이면
    PerformMoveGA.PostReactions에 GetExtendedMoveReactions(hero) 추가
    └ ModifyStatGA(DAMAGE +1), ModifyStatGA(SPEED +1)  (이동이 끝난 뒤 실행)

TerrainSystem: 점프의 MoveGA POST에서 기존대로 Exit/Enter 처리 (코드 변경 없음)
```

**주의·한계**
- "매 라운드 종료 시"는 **라운드 시작 위치가 언덕인 영웅**(직전 라운드 종료 때 언덕 위에 있던 영웅)으로 해석했다 (사용자 확정). 강제 이동이 아니라 이동 범위 확장이며, `TerrainSystem`에는 `TurnGA` 구독을 추가하지 않았다.
- 몬스터는 직접 이동 기능이 없어 적용되지 않는다 (사용자 확정).
- 보너스는 **기본 3x3 밖으로 점프했을 때만**, 영웅당 라운드 1회 부여한다 (영구, `ModifyStatGA`). 범위 안으로 이동하면 보너스가 없다.
- 영웅이 이미 시작 칸을 떠났다면(`current != start`) 확장이 적용되지 않는다. 시작 칸으로 돌아오면 다시 적용되지만 보너스는 라운드당 1회다.
- 카드로 배치된 라운드에는 영웅이 이동할 수 없어(`placedThisRound`) 다음 라운드부터 적용된다.
- 점프는 경로가 목적지 1칸이라 중간 칸의 발판 Exit/Enter는 건너뛴다. 목적지가 발판이면 정상 Enter.
- 효과 템플릿(`TerrainData.Effect`)을 직접 조회하므로 `HillEffect`는 상태를 가지면 안 된다 (현재 상태 없음).
- **설계 결정**: `TerrainEffect` 추상 클래스에는 이동 관련 멤버를 넣지 않고 인터페이스(`IExtendedMoveTerrain`)로 분리했다 (사용자 요청).
- **Play 테스트는 하지 않았다.** 확인 항목: 언덕 위 영웅의 이동 VG가 3x3 밖 빈 칸(바로 뒤 제외)까지 표시되는지, 점프 후 공격력·속도 +1과 같은 라운드 재점프 시 보너스 없음, 언덕이 아닌 영웅의 기존 3x3 이동 유지.

**에디터 설정**: `TerrainData` 에셋 `Id`=2, `Sprite` 지정, `Effect`=`HillEffect`(`damageBonus`=1, `speedBonus`=1) 연결 완료. `StageData.TerrainSpawns`에 (위치, 데이터) 입력은 아직 하지 않았다.
