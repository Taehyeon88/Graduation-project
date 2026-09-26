using System.Linq;
using UnityEngine;

public class WaveSystem : Singleton<WaveSystem>
{
    public Vector2Int corePosition => waveCoreData.CorePosition;  //웨이브 핵 위치

    public int Remain_wave_count { get; private set; } = 0;//남은 웨이브 수
    private EnemyData[] enemyDatas;      //몬스터 리스트
    private int[] wavePerEnemyCount;     //웨이브 당 생성될 몬스터 수
    private WaveCoreData waveCoreData;   //웨이브 핵 데이터
    public void SetUp(EnemyData[] enemyDatas, int[] wavePerEnemyCount, WaveCoreData waveCoreData)
    {
        this.enemyDatas = enemyDatas;
        this.wavePerEnemyCount = wavePerEnemyCount;
        this.waveCoreData = waveCoreData;

        Remain_wave_count = wavePerEnemyCount.Length;
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
    }

    private Vector2Int[] GetRandomPositions(int count)
    {
        int remain = count;
        int distance = 1;
        Vector2Int[] result = new Vector2Int[count];
        while (remain > 0 && distance <= 2)
        {
            var poses = TokenSystem.Instance.API.GetAllAroundPlaces(corePosition, distance);
            if (poses != null)
            {
                foreach (Vector2Int pos in poses)
                {
                    if(remain == 0) break;

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
