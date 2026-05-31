using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveSystem : Singleton<WaveSystem>
{
    public int CurrentTurn { get; private set; }                     //현재 턴 수
    public bool IsWaveRunning { get; private set; }                  //현재 웨이브 중인지 체크
    public int RemainWaveTurn { get; private set; }                  //다음 웨이브 생성까지 남은 턴수


    private EnemyData[] enemyDatas;                                  //생성될 모든 몬스터(순차적)
    private int[] enemyCountsPerWave;                                //각 웨이브 당 생성할 몬스터 수
    private int currentWave;                                         //현재 웨이브
    private int maxWave;                                             //최대 웨이브 수
    private int genIndex;
    private List<Vector2Int> savedPosition = new List<Vector2Int>(); //다음 웨이브 몬스터 생성될 위치들

    private const int waitCount = 3;                                 //각 웨이브 사이 대기 턴수

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.AttachPerformer<SpawnWaveGA>(SpawnWaveGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.DetachPerformer<SpawnWaveGA>();
    }

    public IEnumerator SetUp(IReadOnlyList<EnemyData> enemys, IReadOnlyList<int> enemyCntPerWave)
    {
        enemyDatas = enemys.ToArray();
        enemyCountsPerWave = enemyCntPerWave.ToArray();
        maxWave = enemyCntPerWave.Count - 1;
        currentWave = genIndex = CurrentTurn = 0;
        IsWaveRunning = true;

        //오류 테스트
        int total = 0;
        foreach (int cnt in enemyCntPerWave)
            total += cnt;

        if (total != enemyDatas.Length)
            Debug.LogError($"총 필요 몬스터 수와 몬스터 데이터 개수가 다름. 적 데이터 수적 데이터 수: { enemyDatas.Length}, 총 웨이브 필요 몬스터 수: { total }");

        yield return ReadyToGenerate(true);
        yield return GenerateEnemy();
    }

    //Performers
    private IEnumerator SpawnWaveGAPerformer(SpawnWaveGA spawnWaveGA)
    {
        if (IsWaveRunning)
        {
            yield return GenerateEnemy();
        }

        yield return null;
    }

    //몬스터 생성
    public IEnumerator GenerateEnemy()
    {
        //Debug.Log("몬스터 생성");

        int enemyCount = enemyCountsPerWave[currentWave];  //생성할 몬스터 수 받기
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData enemy = enemyDatas[genIndex];

            //몬스터 생성
            Vector2Int genPos = savedPosition[0];
            if (TokenSystem.Instance.IsGridEmpty(genPos))
            {
                TokenSystem.Instance.AddToken(enemy, TokenType.Enemy, genPos);
            }
            else Debug.Log($"{genPos} 위치에 다른 대상이 있어서 생성 안됨!");
            savedPosition.RemoveAt(0);

            //이전 생성 위치 시각 효과 종료
            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_SetUp_False");

            //생성 연출(현재 생략)
            genIndex++;
        }
        currentWave++;

        if (currentWave <= maxWave)
        {
            yield return ReadyToGenerate();
        }
        else
        {
            //Debug.Log("웨이브 종료");
            IsWaveRunning = false;
        }
    }

    //몬스터 생성 준비
    private IEnumerator ReadyToGenerate(bool isFirst = false)
    {
        //Debug.Log("몬스터 생성 준비");
        //Debug.Log($"현 웨이브: {currentWave}, 최대 웨이브: {maxWave}");

        //각 몬스터 생성 준비 처리
        int enemyCount = enemyCountsPerWave[currentWave];
        int idx = genIndex;

        RemainWaveTurn = waitCount;
        savedPosition.Clear();

        //각 몬스터의 위치 정보 저장
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData enemy = enemyDatas[idx];
            Vector2Int pos = UtilityRandomGenerator.GetSpawnPosition(
                GetCandidates(), 
                enemy.EnemyGenerateTypes
                );
            savedPosition.Add(pos);

            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Hero_SetUp_False");

            //생성 준비 연출(현재 생략)
            idx++;
        }

        yield return null;
    }

    private List<Vector2Int> GetCandidates()
    {
        bool isTutorial = TutorialSystem.Instance.IsTutorialing;

        if (currentWave == 0)
        {
            if (isTutorial) return GetTutoPos();
            else return GetAllPoses();
        }
        else if (currentWave == 1)
        {
            if (isTutorial) return GetTutoPos();
            else return GetOutLinePoses();
        }
        else
        {
            if (isTutorial) return GetAllPoses();
            else return GetOutLinePoses();
        }
    }

    private List<Vector2Int> GetOutLinePoses()
    {
        var list = new List<Vector2Int>();
        for (int x = 0; x < TokenSystem.Instance.gridWidth; x++)
        {
            for (int y = 0; y < TokenSystem.Instance.gridHeight; y++)
            {
                if (x == 0 || x == TokenSystem.Instance.gridWidth - 1 || y == 0 || y == TokenSystem.Instance.gridHeight - 1)
                {
                    if (TokenSystem.Instance.IsGridEmpty(new(x, y)) && !savedPosition.Contains(new(x, y)))
                        list.Add(new Vector2Int(x, y));
                }
            }
        }
        return list;
    }
    private List<Vector2Int> GetAllPoses()
    {
        var list = new List<Vector2Int>();
        for (int x = 0; x < TokenSystem.Instance.gridWidth; x++)
        {
            for (int y = 0; y < TokenSystem.Instance.gridHeight; y++)
            {
                if (TokenSystem.Instance.IsGridEmpty(new(x, y)) && !savedPosition.Contains(new(x, y)))
                    list.Add(new Vector2Int(x, y));
            }
        }
        return list;
    }
    private List<Vector2Int> GetTutoPos()
    {
        var list = new List<Vector2Int>();

        if (currentWave == 0)
        {
            list.Add(new(2, 3));
        }
        else if (currentWave == 1)
        {
            Vector2Int pos1 = new(4, 0);
            Vector2Int pos2 = new(3, 6);
            if(!savedPosition.Contains(pos1))
                list.Add(pos1);
            else
                list.Add(pos2);
        }
        return list;
    }

    //Subscribers

    private void EnemysTurnPreReaction(EnemysTurnGA enemysTurnGA)
    {
        if (!IsWaveRunning) return;

        CurrentTurn++;

        if (enemysTurnGA.isStartGame) return;
        RemainWaveTurn--;

        if (RemainWaveTurn <= 0)
        {
            SpawnWaveGA spawnWave = new();
            ActionSystem.Instance.AddReaction(spawnWave);
        }
    }

}
