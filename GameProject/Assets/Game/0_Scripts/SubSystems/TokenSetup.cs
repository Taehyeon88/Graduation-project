using IsoTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenSetup : MonoBehaviour
{
    [Header("TokenGrid")]
    [field: SerializeField] private TokenGrid grid;

    [Header("SetUpUI")]
    [field: SerializeField] private SetUpUI setupUI;

    private List<EnemyView> EnemyViews => TokenSystem.Instance.EnemyViews;
    private List<HeroView> HeroViews => TokenSystem.Instance.HeroViews;

    private IsoWorld IsoWorld => TokenSystem.Instance.IsoWorld;
    private List<Vector2Int> heroSetupPositions;

    private HeroPreview preview;
    private int heroCount = 3;

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

            heroCount--;

            if (heroCount <= 0)
            {
                EndSetUpHero();
            }
        }
        else
        {
            Debug.Log("해당 위치에는 영웅을 설치 할 수 없습니다.");
        }
    }

    private void EndSetUpHero()
    {
        Interactions.Instance.IsSetUpHero = false;
        Interactions.SetSelectGridEvent(SetHeroView, false);     //마우스 클릭 인터렉션 바인딩 종료
        Destroy(preview.gameObject);
    }
}
