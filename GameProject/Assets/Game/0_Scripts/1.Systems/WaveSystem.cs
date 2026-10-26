using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveSystem : Singleton<WaveSystem>
{
    public int Remain_wave_count { get; private set; } = 0;//남은 웨이브 수
    private EnemyData[] enemyDatas;      //몬스터 리스트
    private int[] wavePerEnemyCount;     //웨이브 당 생성될 몬스터 수
    private int[] waveTurnIntervals;     //다음 웨이브까지 대기할 몬스터 턴 수
    private int[] spawnLines;            //몬스터별 생성 라인 (enemyDatas와 같은 인덱스, 좌표는 EnemySystem의 배치 위치에서 구함)

    private int nextWaveIndex = 1;       //다음에 생성할 웨이브 인덱스 (0번은 SetUpFirstEnemys에서 생성됨)
    private int turnsUntilNextWave;      //다음 웨이브까지 남은 몬스터 턴 수

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpawnWaveGA>(SpawnWaveGAPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpawnWaveGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }

    public void SetUp(EnemyData[] enemyDatas, int[] wavePerEnemyCount, int[] waveTurnIntervals, int[] spawnLines)
    {
        this.enemyDatas = enemyDatas;
        this.wavePerEnemyCount = wavePerEnemyCount;
        this.waveTurnIntervals = waveTurnIntervals;
        this.spawnLines = spawnLines;

        if (spawnLines == null || spawnLines.Length < enemyDatas.Length)
            Debug.LogError("WaveSystem : 몬스터 생성 라인(SpawnLines)이 몬스터 수보다 적습니다");

        Remain_wave_count = wavePerEnemyCount.Length;
        nextWaveIndex = 1;
    }

    public void SetUpFirstEnemys()
    {
        if (enemyDatas == null || wavePerEnemyCount == null)
            Debug.LogError("WaveSystem : SetUp이 되지 않았습니다");

        //첫 웨이브 몬스터 생성
        BuildSpawn(0, wavePerEnemyCount[0], out List<EnemyData> targets, out List<Vector2Int> positions);
        TokenSystem.Instance.Setup.SetUPEnemys(targets.ToArray(), positions.ToArray());   //몬스터 생성

        Remain_wave_count--;
        if (Remain_wave_count > 0)
            turnsUntilNextWave = waveTurnIntervals[nextWaveIndex];
    }

    //Reactions

    //몬스터 턴 종료 후, 플레이어 턴이 시작되기 전에 카운트해서 다음 웨이브 생성 여부 판단
    private void TurnGAPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Player) return;
        if (Remain_wave_count <= 0) return;

        if (turnsUntilNextWave <= 0)
        {
            ActionSystem.Instance.AddReaction(new SpawnWaveGA());
        }

        turnsUntilNextWave--;
    }

    //Performers

    private IEnumerator SpawnWaveGAPerformer(SpawnWaveGA spawnWaveGA)
    {
        int count = wavePerEnemyCount[nextWaveIndex];
        int offset = 0;
        for (int i = 0; i < nextWaveIndex; i++)
            offset += wavePerEnemyCount[i];

        BuildSpawn(offset, count, out List<EnemyData> targets, out List<Vector2Int> positions);
        List<EnemyView> existingEnemies = new(TokenSystem.Instance.EnemyViews);
        TokenSystem.Instance.Main.AddEnemys(targets.ToArray(), positions.ToArray());   //몬스터 생성
        spawnWaveGA.SpawnedEnemies = TokenSystem.Instance.EnemyViews.Where(e => !existingEnemies.Contains(e)).ToList();

        Remain_wave_count--;
        nextWaveIndex++;
        if (Remain_wave_count > 0)
            turnsUntilNextWave = waveTurnIntervals[nextWaveIndex];

        yield return null;
    }

    //Privates

    //offset부터 count마리의 생성 위치를 라인의 적 배치 위치에서 구함 (배치 위치가 없거나 점유/중복된 몬스터는 생성하지 않음)
    private void BuildSpawn(int offset, int count, out List<EnemyData> targets, out List<Vector2Int> positions)
    {
        targets = new();
        positions = new();
        for (int i = 0; i < count; i++)
        {
            int line = spawnLines[offset + i];
            if (!EnemySystem.Instance.TryGetSetupX(line, out int x))
            {
                Debug.LogWarning($"WaveSystem : 라인 {line}에 적 배치 위치가 없어 몬스터를 생성하지 않습니다");
                continue;
            }

            Vector2Int pos = new(x, line);
            if (positions.Contains(pos) || !TokenSystem.Instance.API.IsGridEmpty(pos))
            {
                Debug.LogWarning($"WaveSystem : 생성 위치 {pos}가 점유되어 몬스터를 생성하지 않습니다");
                continue;
            }
            targets.Add(enemyDatas[offset + i]);
            positions.Add(pos);
        }
    }
}
