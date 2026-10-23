using IsoTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenSetup : MonoBehaviour
{
    [Header("TokenGrid")]
    [field: SerializeField] private TokenGrid grid;

    [Header("SetUpUI")]
    [field: SerializeField] private SetUpUI setupUI;

    [Header("CoreViewPositions (그리드 밖 Iso 좌표)")]
    [SerializeField] private Vector3 heroCaptainIsoPosition;
    [SerializeField] private Vector3 enemyFlagIsoPosition;

    private List<EnemyView> EnemyViews => TokenSystem.Instance.EnemyViews;
    private List<HeroView> HeroViews => TokenSystem.Instance.HeroViews;

    private IsoWorld IsoWorld => TokenSystem.Instance.IsoWorld;
    private List<Vector2Int> heroSetupPositions;

    private readonly Dictionary<Vector2Int, TerrainView> terrains = new();   //발판은 토큰이 아니라 그리드에 등록하지 않고 따로 보관
    public IEnumerable<TerrainView> Terrains => terrains.Values;

    private HeroPreview preview;

    private void Update()
    {
        if (!Interactions.Instance.IsSetUpHero) return;

        Vector3 isoPosition = IsoWorld.MouseIsoTilePosition(1f);

        if (preview != null)
        {
            if(setupUI.SelectedData == null)
            {
                preview.ChangeState(HeroPreview.TokenPreViewState.Negative);
                return;
            }

            if (preview.TokenData.Id != setupUI.SelectedData.Id)
                preview.SetUp(setupUI.SelectedData);

            Vector2Int pos = Utility.IsoVectorToVector2Int(isoPosition);

            if (heroSetupPositions.Contains(pos) && grid.CanSet(pos))
            {
                preview.ChangeState(HeroPreview.TokenPreViewState.Positive);
                preview.TokenTransform.position = isoPosition;
            }
            else
            {
                preview.ChangeState(HeroPreview.TokenPreViewState.Negative);
            }
        }
        else
        {
            if(setupUI.SelectedData != null)
                preview = TokenCreator.Instance.CreateTokenPreview(setupUI.SelectedData);
        }
    }

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

    /// <summary>
    /// 전투 시작시, 영웅 배치를 위해서 실행되는 함수
    /// </summary>
    /// <param name="heroDatas"></param>
    public void StartSetUpHero(HeroData[] heroDatas, List<Vector2Int> heroSetupPositions)
    {
        Interactions.Instance.IsSetUpHero = true;

        HeroViews.Clear();

        //영웅 셋업 VG 생성
        this.heroSetupPositions = heroSetupPositions;
        foreach (var gridPos in heroSetupPositions)
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), gridPos, "Hero_SetUp_True");

        setupUI.SetUp(heroDatas);        //SetupUI 활성화

        Interactions.SetSelectGridEvent(SetHeroView, true);     //마우스 클릭 인터렉션 바인딩
    }

    private void SetHeroView()
    {
        if (setupUI.SelectedData == null) return;

        Vector3 isoPosition = IsoWorld.MouseIsoTilePosition(1f);
        Vector2Int pos = Utility.IsoVectorToVector2Int(isoPosition);

        if (heroSetupPositions.Contains(pos) && grid.CanSet(pos))
        {
            //해당 위치 데이터 및 VG 제거
            heroSetupPositions.Remove(pos);

            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_SetUp_True");
            foreach (var p in heroSetupPositions)
                VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), p, "Hero_SetUp_True");

            //위치에 토큰 생성
            Token token = TokenCreator.Instance.CreateToken(
                setupUI.SelectedData,
                TokenType.Hero,
                isoPosition
                );

            grid.SetToken(token, pos);
            HeroViews.Add(token as HeroView);

            //SetUpUI에 Slot 삭제
            setupUI.RemoveSlot();
        }
    }

    private void EndSetUpHero()
    {
        Interactions.Instance.IsSetUpHero = false;
        Interactions.SetSelectGridEvent(SetHeroView, false);     //마우스 클릭 인터렉션 바인딩 종료
        Destroy(preview.gameObject);
    }
}
