using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveSystem : Singleton<WaveSystem>
{
    public Vector2Int corePosition => waveCoreData.CorePosition;  //웨이브 핵 위치

    public int Remain_wave_count { get; private set; } = 0;//남은 웨이브 수
    private EnemyData[] enemyDatas;      //몬스터 리스트
    private int[] wavePerEnemyCount;     //웨이브 당 생성될 몬스터 수
    private int[] waveTurnIntervals;     //다음 웨이브까지 대기할 몬스터 턴 수
    private WaveCoreData waveCoreData;   //웨이브 핵 데이터

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

    public void SetUp(EnemyData[] enemyDatas, int[] wavePerEnemyCount, int[] waveTurnIntervals, WaveCoreData waveCoreData)
    {
        this.enemyDatas = enemyDatas;
        this.wavePerEnemyCount = wavePerEnemyCount;
        this.waveTurnIntervals = waveTurnIntervals;
        this.waveCoreData = waveCoreData;

        Remain_wave_count = wavePerEnemyCount.Length;
        nextWaveIndex = 1;
    }

    public void SetUpFirstEnemys()
    {
        if (enemyDatas == null || wavePerEnemyCount == null)
            Debug.LogError("WaveSystem : SetUp이 되지 않았습니다");

        //첫 웨이브 몬스터 생성
        EnemyData[] targets = new EnemyData[wavePerEnemyCount[0]];
        Vector2Int[] positions = GetRandomPositions(wavePerEnemyCount[0]);

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = enemyDatas[i];
        }
        TokenSystem.Instance.Setup.SetUPEnemys(targets, positions);   //몬스터 생성
        TokenSystem.Instance.Setup.SetUpWaveCore(waveCoreData);       //웨이브 코어 생성

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

    private Vector2Int[] GetRandomPositions(int count)
    {
        int remain = count;
        int distance = 1;
        Vector2Int[] result = new Vector2Int[count];
        HashSet<Vector2Int> used = new();
        while (remain > 0 && distance <= 2)
        {
            var poses = TokenSystem.Instance.API.GetAllAroundPlaces(corePosition, distance);
            if (poses != null)
            {
                foreach (Vector2Int pos in poses)
                {
                    if(remain == 0) break;
                    if (!used.Add(pos)) continue;   //이미 선택된 좌표는 건너뜀

                    result[remain - 1] = pos;
                    remain--;
                }

                distance++;
            }
            else
            {
                break;
            }
        }

        if (remain > 0)
            Debug.LogError($"WaveSystem : WaveCore 인접 2칸 내로 랜덤 생성할 위치 {count}개를 찾을 수 없음");

        return result.Shuffle();
    }
}
