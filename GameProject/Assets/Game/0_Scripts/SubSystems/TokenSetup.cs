using IsoTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenSetup : MonoBehaviour
{
    [Header("TokenGrid")]
    [field: SerializeField] private TokenGrid grid;

    [Header("CoreViewPositions (그리드 밖 Iso 좌표)")]
    [SerializeField] private Vector3 heroCaptainIsoPosition;
    [SerializeField] private Vector3 enemyFlagIsoPosition;

    private List<EnemyView> EnemyViews => TokenSystem.Instance.EnemyViews;
    private List<HeroView> HeroViews => TokenSystem.Instance.HeroViews;

    private IsoWorld IsoWorld => TokenSystem.Instance.IsoWorld;

    private readonly Dictionary<Vector2Int, TerrainView> terrains = new();   //발판은 토큰이 아니라 그리드에 등록하지 않고 따로 보관
    public IEnumerable<TerrainView> Terrains => terrains.Values;

    //스테이지 맵 타일 셋업
    public void SetUpStageMap()
    {
        grid.GenerateStage();
    }

    //중립지대 칸 막기 (스테이지 맵 생성 후, 배치 위치 계산 전에 호출)
    public void SetUpNeutralZones(Vector2Int[] neutrals)
    {
        if (neutrals == null) return;
        foreach (Vector2Int pos in neutrals)
            grid.SetBlockedCell(pos);
    }

    //파괴 가능 장애물 배치 (중립지대 설정 후, 몬스터 배치 전에 호출)
    public void SetUpObstacles(ObstacleSpawn[] spawns)
    {
        if (spawns == null) return;
        foreach (ObstacleSpawn spawn in spawns)
        {
            if (spawn.Data == null || !grid.CanSet(spawn.Position))
            {
                Debug.LogError($"장애물 배치 불가: {spawn.Position} (데이터 없음, 범위 밖 또는 이미 점유된 칸)");
                continue;
            }
            TokenSystem.Instance.Main.AddToken(spawn.Data, TokenType.Obstacle, spawn.Position);
        }
    }

    //발판 배치 (중립지대·장애물 설정 후, 배치 위치 계산 전에 호출)
    public void SetUpTerrains(TerrainSpawn[] spawns)
    {
        if (spawns == null) return;
        foreach (TerrainSpawn spawn in spawns)
        {
            if (spawn.Data == null || terrains.ContainsKey(spawn.Position) || !grid.CanSet(spawn.Position))
            {
                Debug.LogError($"발판 배치 불가: {spawn.Position} (데이터 없음, 범위 밖, 중복 또는 이미 점유된 칸)");
                continue;
            }
            terrains.Add(spawn.Position, TokenCreator.Instance.CreateTerrain(spawn.Data, spawn.Position));
        }
    }

    public bool TryGetTerrain(Vector2Int pos, out TerrainView terrain) => terrains.TryGetValue(pos, out terrain);

    //발판 전체 제거 (승리 후 정리용)
    public void ClearTerrains()
    {
        foreach (TerrainView terrain in terrains.Values)
        {
            if (terrain != null)
                Destroy(terrain.gameObject);
        }
        terrains.Clear();
    }

    //라인마다 기본 배치 칸(baseX)에서 장애물이 없는 첫 칸까지 step 방향으로 이동한 배치 위치 목록 (그리드 밖이면 해당 라인 제외)
    public List<Vector2Int> CalcSetupPoses(Vector2Int[] obstacles, int baseX, int step)
    {
        obstacles ??= Array.Empty<Vector2Int>();
        List<Vector2Int> poses = new(grid.height);

        for (int y = 0; y < grid.height; y++)
        {
            Vector2Int pos = new(baseX, y);
            while (Array.IndexOf(obstacles, pos) >= 0)
                pos.x += step;

            if (pos.x < 0 || pos.x >= grid.width) continue;
            poses.Add(pos);
        }
        return poses;
    }

    //용병대장 생성 (그리드 밖, TokenGrid 미등록)
    public void SetUpHeroCaptain(HeroCaptainData heroCaptainData)
    {
        Token token = TokenCreator.Instance.CreateToken(heroCaptainData, TokenType.HeroCaptain, heroCaptainIsoPosition);
        HeroSystem.Instance.HeroCaptain = token as HeroCaptainView;
    }

    //적 세력 깃발 생성 (그리드 밖, TokenGrid 미등록)
    public void SetUpEnemyFlag(EnemyFlagData enemyFlagData, int health)
    {
        Token token = TokenCreator.Instance.CreateToken(enemyFlagData, TokenType.EnemyFlag, enemyFlagIsoPosition);
        EnemyFlagView enemyFlagView = token as EnemyFlagView;
        enemyFlagView.UpdateHealth(health);
        EnemySystem.Instance.EnemyFlag = enemyFlagView;
    }

    /// <summary>
    /// 전투 시작시, 모든 몬스터들 비어있는 그리드에 랜덤 배치 함수
    /// </summary>
    /// <param name="enemyDatas"></param>
    /// <param name="setupPositions"></param>
    public void SetUPEnemys(EnemyData[] enemyDatas, Vector2Int[] setupPositions)
    {
        EnemyViews.Clear();

        int index = 0;
        foreach (var enemyData in enemyDatas)
        {
            Token token = TokenCreator.Instance.CreateToken(
                    enemyData,
                    TokenType.Enemy,
                    transform.position
                );

            token.TokenTransform.position = Utility.Vector2IntToIsoVector(setupPositions[index]);
            EnemyViews.Add(token as EnemyView);
            grid.SetToken(token, setupPositions[index]);

            index++;
        }
    }
}
