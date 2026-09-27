# WaveSystem 아키텍처

> 이 문서는 전투 중 몬스터 웨이브가 "언제, 무엇을, 어떻게" 추가로 생성되는지, 그리고 그 결과가 `EnemySystem`의 턴 판단 흐름과 어떻게 맞물리는지를 정리한 참고 문서다. `ActionSystem` 자체의 동작 원리는 [00_action_architecture.md](./00_action_architecture.md), 몬스터 개별 턴 AI(`JudgeActAction`/`EnemyAction`)는 [01_enemy_system.md](./01_enemy_system.md), 토큰 생성/그리드 등록 계약은 [00_token_system.md](./00_token_system.md)를 먼저 본다. 웨이브 밸런싱 수치(웨이브 수, 마리 수, 턴 간격의 실제 값)와 `WaveCoreData`의 체력/피격 로직은 이 문서 범위 밖이다.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/1.Systems/WaveSystem.cs` | 파사드(Singleton) — 웨이브 카운트다운, 생성 대상 슬라이싱, `SpawnWaveGA` Performer |
| `Assets/Game/0_Scripts/5.Data/WaveData.cs` | ScriptableObject — 웨이브별 몬스터/수량/대기 턴 정의 |
| `Assets/Game/0_Scripts/2.GameActions/SpawnWaveGA.cs` | 필드 없는 시그널 GA — "웨이브 하나를 생성하라" |
| `Assets/Game/0_Scripts/SubSystems/TokenMainAPI.cs` | (외부 소유) `AddEnemys`/`AddToken` — 런타임 중 몬스터 그리드 등록 |
| `Assets/Game/0_Scripts/1.Systems/EnemySystem.cs` | (외부 소유) `SpawnWavePostReaction` — 새로 생성된 몬스터의 다음 행동 계산 |
| `Assets/Game/0_Scripts/1.Systems/TurnSystem.cs` | (외부 소유) `TurnGA` 흐름의 소유자 — `WaveSystem`이 끼어드는 지점(PRE, `TurnType.Player`) |
| `Assets/Game/0_Scripts/1.Systems/MatchSetupSystem.cs` | (외부) 전투 시작 시 `WaveSystem.SetUp` → `SetUpFirstEnemys` 호출부 |

## 1. 한 줄 개요

`WaveSystem`은 원래 "전투 시작 시 첫 웨이브만 즉석 생성하고 끝"이던 컴포넌트였다(`SetUpFirstEnemys`, `MatchSetupSystem.cs:29-35`에서 직접 호출). 여기에 `ActionSystem` 연동(`OnEnable`/`OnDisable`)을 추가해, **몬스터 턴이 끝날 때마다 카운트다운하다가 0이 되면 `SpawnWaveGA`를 큐잉해 다음 웨이브를 추가 생성**하도록 확장했다. 생성 방식(코어 인접 랜덤 위치 선정)은 첫 웨이브와 완전히 동일한 `GetRandomPositions()`를 그대로 재사용하고, 몬스터를 그리드에 등록하는 실제 창구만 초기배치용(`TokenSetup`) 대신 런타임용(`TokenMainAPI`)으로 나뉜다.

## 2. 데이터 구조

### 2-1. WaveData — 오프셋 기반 배열 레이아웃

```csharp
// WaveData.cs
[field: SerializeField] public EnemyData[] EnemyDatas { get; private set; }
[field: SerializeField] public int[] WavePerEnemyCount { get; private set; }   //웨이브 당 생성될 몬스터 수
[field: SerializeField] public int[] WaveTurnIntervals { get; private set; }   //다음 웨이브까지 대기할 몬스터 턴 수 (index 0 미사용)
[field: SerializeField] public WaveCoreData WaveCoreData { get; private set; }
```

- `EnemyDatas`는 "웨이브 인덱스 오름차순 오프셋" 레이아웃이다 — 웨이브 `i`의 몬스터는 `EnemyDatas[offset..offset+WavePerEnemyCount[i]-1]` 구간이고, `offset`은 `WavePerEnemyCount[0..i-1]`의 누적 합이다. 실측 자산(`Simple_Wave.asset`)에서 `WavePerEnemyCount = [2, 3, 1, 1]`(합계 7)이고 `EnemyDatas`도 정확히 7개로, 이 레이아웃 전제를 그대로 따른다. **새 웨이브 자산을 만들 때 `EnemyDatas` 총 길이가 `WavePerEnemyCount`의 합과 어긋나면 별도 검증 없이 엉뚱한 `EnemyData`가 슬라이스된다.**
- `WaveTurnIntervals`는 `WavePerEnemyCount`와 **동일 길이**로 맞춘다. `index 0`은 사용되지 않는다(첫 웨이브는 전투 시작 시 즉시 생성되므로 대기 개념이 없음). `index i(i>=1)`는 "웨이브 `(i-1)` 생성 후 웨이브 `i`가 생성되기까지 대기할 몬스터 턴 수"다. 길이가 `WavePerEnemyCount`보다 짧으면 `SpawnWaveGAPerformer`/`SetUpFirstEnemys`에서 `IndexOutOfRangeException`이 난다.

### 2-2. SpawnWaveGA — 시그널 전용 GA

```csharp
// SpawnWaveGA.cs
public class SpawnWaveGA : GameAction
{
}
```

캐스터/대상 개념이 없는 순수 턴/자원 계열이라 `IHaveCaster`를 구현하지 않는다(convention.md §7). `GameOverGA`/`GameClearGA`와 동일하게 "지금 웨이브 하나를 생성하라"는 트리거 그 자체가 데이터다 — 몇 번째 웨이브를 생성할지는 `WaveSystem` 내부 상태(`nextWaveIndex`)가 들고 있다.

## 3. 카운트다운 → 생성 흐름 — 왜 `TurnGA(Enemy)`의 POST가 아니라 `TurnGA(Player)`의 PRE인가

```csharp
// WaveSystem.cs
private void OnEnable()
{
    ActionSystem.AttachPerformer<SpawnWaveGA>(SpawnWaveGAPerformer);
    ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
}

private void TurnGAPreReaction(TurnGA turnGA)
{
    if (turnGA.Type != TurnType.Player) return;
    if (Remain_wave_count <= 0) return;

    turnsUntilNextWave--;
    if (turnsUntilNextWave <= 0)
        ActionSystem.Instance.AddReaction(new SpawnWaveGA());
}
```
(`WaveSystem.cs:18-21`, `64-74`)

`TurnGA(Enemy)`의 POST에는 이미 `TurnSystem.TurnGAPostReaction`(→ `TurnGA(Player)`를 `AddReaction`, `TurnSystem.cs:58-66`)과 `EnemySystem.EnemyTurnPostReaction`(`EnemySystem.cs:101-116`)이 같이 물려 있다. 만약 웨이브 스폰도 여기 POST에 구독을 걸어 `AddReaction(spawnWaveGA)`를 호출하면, `ActionSystem.PerformSubscribers`가 같은 GA 타입의 POST 구독자 전원을 **등록 순서대로 동기 호출**하기 때문에(`ActionSystem.cs:78-97`), "스폰 GA가 `TurnGA(Player)`보다 먼저 큐에 들어가는지"가 `WaveSystem`과 `TurnSystem`의 `OnEnable` 등록 순서(= 스크립트 실행 순서)에 의존하게 된다. 순서가 꼬이면 플레이어 턴 전체가 먼저 실행된 뒤에야 몬스터가 스폰되는 버그가 생긴다.

대신 `TurnGA(Player)` 자신의 **PRE 리액션**에서 카운트다운·판단을 하도록 걸었다. `ActionSystem.Flow()`는 한 GA당 `PreReactions 처리 → Perform → PostReactions 처리` 순서를 구조적으로 보장하므로(`ActionSystem.cs:41-65`), `TurnGAPreReaction`에서 큐잉한 `SpawnWaveGA`는 `TurnGA(Player)`의 Perform 단계(= 실제 플레이어 턴이 시작되는 지점, `TurnSystem.cs:46-55`의 `HeroSystem.PlayHeroTurnPerformer` 호출부)보다 **항상** 먼저 완전히 끝난다. 등록 순서와 무관하게 "몬스터 턴 종료 → 카운트/생성 → 플레이어 턴 시작" 순서가 지켜지고, 카운트 자체는 여전히 게임 1턴 사이클당 정확히 1회(몬스터 턴이 끝나고 다음 `TurnGA(Player)`가 생성될 때) 일어난다.

## 4. 생성 실행 — SpawnWaveGAPerformer → TokenMainAPI.AddEnemys

```csharp
// WaveSystem.cs
private IEnumerator SpawnWaveGAPerformer(SpawnWaveGA spawnWaveGA)
{
    int count = wavePerEnemyCount[nextWaveIndex];
    int offset = 0;
    for (int i = 0; i < nextWaveIndex; i++)
        offset += wavePerEnemyCount[i];

    EnemyData[] targets = new EnemyData[count];
    for (int i = 0; i < count; i++)
        targets[i] = enemyDatas[offset + i];

    Vector2Int[] positions = GetRandomPositions(count);
    TokenSystem.Instance.Main.AddEnemys(targets, positions);   //몬스터 생성

    Remain_wave_count--;
    nextWaveIndex++;
    if (Remain_wave_count > 0)
        turnsUntilNextWave = waveTurnIntervals[nextWaveIndex];

    yield return null;
}
```
(`WaveSystem.cs:78-98`)

- 위치 선정은 첫 웨이브와 동일한 `GetRandomPositions()`(`WaveSystem.cs:100-130`, 웨이브 핵 인접 1~2칸 랜덤)를 그대로 재사용한다 — 수정 없음.
- 몬스터를 실제로 씬/그리드에 등록하는 창구만 다르다. 첫 웨이브(`SetUpFirstEnemys`)는 [00_token_system.md](./00_token_system.md)가 "전투 시작 전 초기 배치 전용, 게임 진행 중 호출 안 됨"이라고 규정한 `TokenSetup.SetUPEnemys()`(내부에서 `EnemyViews.Clear()` 실행)를 쓰지만, 후속 웨이브는 전투 진행 **중**의 런타임 상태 변경이므로 "게임 진행 중 그리드/토큰 상태를 바꾸는 유일한 창구"인 `TokenMainAPI`(`Main`)에 새로 만든 `AddEnemys()`를 쓴다:

```csharp
// TokenMainAPI.cs
public void AddEnemys(EnemyData[] enemyDatas, Vector2Int[] setupPositions)
{
    for (int i = 0; i < enemyDatas.Length; i++)
        AddToken(enemyDatas[i], TokenType.Enemy, setupPositions[i]);
}
```
(`TokenMainAPI.cs:40-46`)

`AddEnemys`는 기존에 있었지만 아무도 호출하지 않던 `AddToken(TokenData, TokenType, Vector2Int)`(`TokenMainAPI.cs:18-33`, 00_token_system.md §4가 "현재 외부 호출자가 없다"고 기록했던 바로 그 메서드)를 배열만큼 반복 호출하는 얇은 래퍼다. `AddToken` 내부는 `TokenCreator.CreateToken`으로 토큰을 만들고, `grid.SetToken`으로 그리드에 등록하고, `EnemyView`면 `EnemyViews.Add`까지 처리한다 — `TokenSetup.SetUPEnemys()`와 최종 결과(아이소메트릭 위치 포함)는 동일하되 `Clear()`가 없어 기존 생존 몬스터를 지우지 않는다. 웨이브 코어(`WaveCoreView`)는 전투당 1개뿐이라 `SetUpWaveCore()`는 `SetUpFirstEnemys()`에서만 호출되고 후속 웨이브에서는 다시 생성하지 않는다.

## 5. 새로 생성된 몬스터의 행동 자동 연산 — EnemySystem.SpawnWavePostReaction

```csharp
// EnemySystem.cs
private void SpawnWavePostReaction(SpawnWaveGA spawnWaveGA)
{
    foreach (EnemyView enemy in Enemise)
    {
        if (enemy.NextAction != null) continue;

        EnemyAction action = enemy.Enemy.JudgeActAction(enemy);
        if (action != null)
            enemy.SetNextAction(action);
    }
}
```
(`EnemySystem.cs:132-143`, 구독: `EnemySystem.cs:20`/`29`)

`BattleStartPostReaction`(`EnemySystem.cs:119-130`, 전투 시작 시 전원 계산)·`EnemyTurnPostReaction`(`EnemySystem.cs:101-116`, 매 몬스터 턴 종료 시 전원 재계산)과 같은 `JudgeActAction`/`SetNextAction` 로직이지만, **`NextAction == null`인 몬스터만** 골라 계산한다는 점이 다르다. `EnemyView.NextAction`은 생성 직후 기본값이 `null`이므로(`EnemyView.cs:29`), 이 필터는 정확히 "이번에 새로 생성된 몬스터"만 걸러낸다 — 이미 다음 턴 행동이 정해진 기존 생존 몬스터를 웨이브 생성 시점에 덮어쓰지 않는다.

`SpawnWaveGA`는 §3에서 설명한 대로 `TurnGA(Player)`의 PreReactions 안에서 완전히 끝난 뒤(PRE→Perform→**POST**까지) 비로소 `TurnGA(Player)`의 Perform 단계(실제 플레이어 턴 시작)로 넘어가므로, "플레이어 턴이 시작되기 전에 새 몬스터의 다음 행동이 모두 연산되어 예고 UI까지 표시되어 있다"가 구조적으로 보장된다.

## 6. 전체 사이클 시퀀스 (예: WaveTurnIntervals = [_, 2, 2, 2])

```
[전투 시작] MatchSetupSystem.StartSetting()
 └ WaveSystem.SetUp(enemyDatas, wavePerEnemyCount, waveTurnIntervals, waveCoreData)
    Remain_wave_count = 4, nextWaveIndex = 1
 └ WaveSystem.SetUpFirstEnemys()
    1. 웨이브 0 몬스터 생성(TokenSetup.SetUPEnemys) + 웨이브 코어 생성(SetUpWaveCore)
    2. Remain_wave_count-- (4 → 3)
    3. turnsUntilNextWave = waveTurnIntervals[1] (=2)

[TurnGA(Enemy)의 POST] 몬스터 턴 종료
 └ EnemySystem.EnemyTurnPostReaction: 생존 몬스터 전원 NextAction 재계산
 └ TurnSystem.TurnGAPostReaction: TurnGA(Player)를 AddReaction

[TurnGA(Player)의 PRE 단계] — 실제 플레이어 턴이 시작되기 전
 └ WaveSystem.TurnGAPreReaction (Type==Player 필터 통과)
    turnsUntilNextWave-- (2 → 1, 아직 0 아님 → 이번 턴엔 생성 안 함)
    ... (다음 몬스터 턴 종료 후 반복) turnsUntilNextWave-- (1 → 0)
    └ SpawnWaveGA를 AddReaction (TurnGA(Player)의 PreReactions에 쌓임)
       PRE: 구독자 없음
       Perform: WaveSystem.SpawnWaveGAPerformer
         - offset = wavePerEnemyCount[0] (웨이브 1 몬스터 구간 계산)
         - EnemyDatas[offset..] 슬라이스 + GetRandomPositions(count)
         - TokenSystem.Instance.Main.AddEnemys(targets, positions)
         - Remain_wave_count--(3→2), nextWaveIndex++(1→2), turnsUntilNextWave = waveTurnIntervals[2]
       POST: EnemySystem.SpawnWavePostReaction
         - Enemise 순회, NextAction == null인(=방금 생성된) 몬스터만 JudgeActAction/SetNextAction
 └ (SpawnWaveGA 처리가 끝나야) TurnGA(Player)의 Perform 단계 진입 — 턴 번호 증가, 팝업, HeroSystem.PlayHeroTurnPerformer

[Remain_wave_count가 0이 되면] TurnGAPreReaction이 즉시 return — 더 이상 웨이브 생성 안 함
```

## 7. 주의사항 / 함정

- **`Remain_wave_count`는 원래 죽은 필드였다.** 이 작업 전에는 `SetUp()`에서 초기화만 되고 어디서도 감소하지 않았다(0으로 내려가지 않는 카운터). 지금은 `SetUpFirstEnemys()`와 `SpawnWaveGAPerformer()` 양쪽에서 웨이브를 생성할 때마다 감소시키도록 고쳤다 — "마지막 웨이브 이후 더 이상 생성하지 않는다"는 판단이 이 필드에 의존한다.
- **`AddToken`에 첫 호출자가 생겼다.** [00_token_system.md](./00_token_system.md) §4는 `TokenMainAPI.AddToken`을 "현재 외부 호출자가 없다 — 예비된 메서드"로 기록하고 있는데, 이제 `WaveSystem`(경유: `TokenMainAPI.AddEnemys`)이 실질적인 첫 호출자다. 해당 문서 자체는 이번 작업에서 갱신하지 않았다 — 다음에 `00_token_system.md`를 다시 조사/작성할 때 이 사실이 반영되어야 한다.
- **`GetRandomPositions()`의 `distance <= 2` 매직넘버**(`WaveSystem.cs:105`)는 convention.md §11이 이미 "기존 위반 사례"로 기록한 지점이다. 웨이브가 반복 생성될 때마다 코어 인접 1~2칸 범위 안에서만 빈 자리를 찾으므로, 웨이브들이 누적되어 그 범위가 다 채워지면 `Debug.LogError`만 찍고 남는 자리는 `Vector2Int(0,0)` 기본값으로 조용히 채워진다(그리드 밖/겹침 위치일 수 있음) — 값이 큰 `WavePerEnemyCount`나 짧은 `WaveTurnIntervals`를 넣을 때 특히 주의.
- **`nextWaveIndex`는 순증가만 한다.** 웨이브를 건너뛰거나 재시도하는 로직이 없다 — `SpawnWaveGAPerformer`가 한 번 실행되면 무조건 `nextWaveIndex`가 1 증가한다.
- **`WaveCoreData`는 전투당 1회만 생성된다.** 후속 웨이브 스폰 경로(`TokenMainAPI.AddEnemys`)는 `TokenType.Enemy`만 다루고 `WaveCoreView`를 다시 만들지 않는다.

## 8. 새 웨이브 관련 기능 추가 시 체크리스트

1. 트리거 조건을 "턴 수" 대신 다른 것(예: 코어 HP 임계치, 특정 몬스터 전멸)으로 바꾸고 싶다면 `WaveSystem.TurnGAPreReaction`의 판단 로직만 교체하면 된다 — `SpawnWaveGAPerformer` 이하 생성·계산 파이프라인은 트리거 종류와 무관하게 그대로 재사용 가능하다.
2. 새 웨이브 데이터(`WaveData` 자산)를 만들 때는 `EnemyDatas` 총 길이가 `WavePerEnemyCount`의 합과 정확히 일치해야 하고, `WaveTurnIntervals`는 `WavePerEnemyCount`와 동일 길이여야 한다(index 0은 값 무관) — 어긋나면 조용히 잘못된 `EnemyData`를 스폰하거나 `IndexOutOfRangeException`이 난다. 검증 코드는 없다.
3. 새로 생성된 몬스터에게 행동 계산 외의 부가 처리(예: 등장 연출, 특수 상태이상 부여)가 필요하면 `EnemySystem.SpawnWavePostReaction`의 `NextAction == null` 필터를 참고해 같은 지점(`SpawnWaveGA`의 POST)에 건다 — 기존 생존 몬스터와 섞이지 않도록 반드시 이 필터(또는 동등한 구분 조건)를 거쳐야 한다.
