using System.Linq;
using UnityEngine;

public class GridSelector : MonoBehaviour
{
    [SerializeField] private TokenDetailsUI tokenDetailsUI;   //호버한 영웅·몬스터 정보 패널
    [SerializeField] private TerrainDetailsUI terrainDetailsUI;   //호버한 지형지물·장애물 정보 패널
    [SerializeField] private CaptainDetailsUI captainDetailsUI;   //호버한 용병대장 정보 패널
    [SerializeField] private EnemyFlagDetailsUI enemyFlagDetailsUI;   //호버한 적 세력 깃발 정보 패널

    private const string HoverWhiteVG = "Selector_Hover_White";    //몬스터·장애물·지형지물·적 깃발
    private const string HoverYellowVG = "Selector_Hover_Yellow";  //영웅·용병대장

    private Vector2Int hoveredGrid = Vector2Int.down;
    private Token hoveredCore;   //마우스가 올라간 코어 (없으면 null)
    private bool clearHandled;   //승리 후 토큰 정리에 맞춰 호버 갱신을 한 번 처리했는지
    private bool selectingHandled;   //보상 선택 시작·종료에 맞춰 호버 갱신을 한 번 처리했는지

    private void OnEnable()
    {
        Interactions.SetSelectGridEvent(SelectToken, true);
    }
    private void OnDisable()
    {
        Interactions.SetSelectGridEvent(SelectToken, false);
    }
    void Update()
    {
        //승리로 그리드의 토큰이 바뀌면 호버 칸을 다시 판정 (사라진 토큰의 정보 패널 잔존 방지)
        if (GameSystem.Instance.IsGameClear != clearHandled
            || RewardSelectSystem.Instance.IsSelecting != selectingHandled)
        {
            clearHandled = GameSystem.Instance.IsGameClear;
            selectingHandled = RewardSelectSystem.Instance.IsSelecting;
            hoveredGrid = Vector2Int.down;
        }

        //그리드 밖의 코어(용병대장·적 깃발)는 타일이 아니라 스프라이트 영역으로 호버 판정 (호버 중에는 타일 호버 처리 생략)
        Token core = FindHoveredCore();
        if (core != hoveredCore)
        {
            HideCorePanel(hoveredCore);
            hoveredCore = core;
            ShowCorePanel(core);
        }
        if (core != null)
        {
            hoveredGrid = Vector2Int.down;   //코어에서 내려올 때 타일 호버를 다시 판정
            return;
        }

        //현재 마우스 위치에 따라서 호버 칸 갱신
        Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
        Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

        if (!TokenSystem.Instance.API.IsBound(pos))
        {
            if (hoveredGrid != Vector2Int.down)
            {
                hoveredGrid = Vector2Int.down;
                UpdateHoveredToken(null, Vector2Int.down);   //그리드 밖으로 나가면 정보 패널 숨김
            }
            return;
        }

        if (hoveredGrid != pos)
        {
            if (!Interactions.Instance.IsCardTargetMode)
                SoundSystem.Instance.PlaySound(3001);
            hoveredGrid = pos;

            Token token = TokenSystem.Instance.API.GetTokenByPosition(pos);
            UpdateHoveredToken(token, pos);
        }
    }

    //그리드 클릭 실행 함수
    private void SelectToken()
    {
        if (TurnSystem.Instance.CurrentTurn == TurnType.GameSetUp
                    || Interactions.Instance.IsCardTargetMode) return;    //모든 처리 반환

        Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
        Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

        if (!TokenSystem.Instance.API.IsBound(pos)) return;

        Token token = TokenSystem.Instance.API.GetTokenByPosition(pos);

        //승리 후에는 보상 용병 선택만 처리 (영웅 이동 모드 진입 차단)
        if (GameSystem.Instance.IsGameClear)
        {
            RewardSelectSystem.Instance.TrySelect(token);
            return;
        }

        //플레이어 이동모드 및 영웅 선택
        if(token != null && token is HeroView heroView)
        {
            MoveSystem.Instance.PlayPlayerMoveMode(heroView);  //이동 모드 실행/종료 함수

            if (HeroSystem.Instance.CurrentHero == heroView) return;

            SoundSystem.Instance.PlaySound(3002);

            UpdateSelectedToken(token);
        }
    }

    private void UpdateSelectedToken(Token token)
    {
        if (token != null)
        {
            //영웅일 경우, 선택된 영웅 업데이트 및 스킬 페이지
            HeroSystem.Instance.CurrentHero = token as HeroView;
        }
    }

    //마우스가 올라간 코어 (용병대장 > 적 깃발, 카드 타겟 모드면 없음)
    private Token FindHoveredCore()
    {
        if (Interactions.Instance.IsCardTargetMode) return null;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Token captain = captainDetailsUI != null ? HeroSystem.Instance.HeroCaptain : null;
        Token flag = enemyFlagDetailsUI != null ? EnemySystem.Instance.EnemyFlag : null;
        if (IsMouseOver(captain, mouseWorld)) return captain;
        if (IsMouseOver(flag, mouseWorld)) return flag;
        return null;
    }

    private bool IsMouseOver(Token token, Vector3 mouseWorld)
    {
        if (token == null) return false;

        Bounds bounds = token.Model.bounds;
        mouseWorld.z = bounds.center.z;
        return bounds.Contains(mouseWorld);
    }

    private void ShowCorePanel(Token core)
    {
        if (core == null) return;

        if (tokenDetailsUI != null) tokenDetailsUI.Hide();
        if (terrainDetailsUI != null) terrainDetailsUI.Hide();
        if (core is HeroCaptainView captain)
        {
            captainDetailsUI.Show(captain);
            SetHoverVisualGrid(VisualGridCreator.HeroCaptainSlot, HoverYellowVG);
        }
        else if (core is EnemyFlagView)
        {
            enemyFlagDetailsUI.Show(CoreSystem.Instance.EnemyCoreHealth, CoreSystem.Instance.EnemyCoreMaxHealth);
            SetHoverVisualGrid(VisualGridCreator.EnemyFlagSlot, HoverWhiteVG);
        }
    }

    private void HideCorePanel(Token core)
    {
        if (core is HeroCaptainView) captainDetailsUI.Hide();
        else if (core is EnemyFlagView) enemyFlagDetailsUI.Hide();
        if (core != null) SetHoverVisualGrid(Vector2Int.down, null);
    }

    //호버 테두리 VG 교체 (이전 색이 남지 않게 둘 다 지운 뒤 vgName이 있으면 pos에 생성)
    private void SetHoverVisualGrid(Vector2Int pos, string vgName)
    {
        int id = gameObject.GetInstanceID();
        VisualGridCreator.Instance.RemoveVisualGrid(id, HoverWhiteVG);
        VisualGridCreator.Instance.RemoveVisualGrid(id, HoverYellowVG);
        if (vgName != null)
            VisualGridCreator.Instance.CreateVisualGrid(id, pos, vgName);
    }

    //호버한 칸의 정보 패널 갱신 (한 칸에 패널 하나만: 영웅·몬스터 > 장애물 > 지형지물, 카드 타겟 모드·빈 칸은 모두 숨김)
    private void UpdateHoveredToken(Token token, Vector2Int pos)
    {
        bool cardMode = Interactions.Instance.IsCardTargetMode;
        bool showToken = !cardMode && token is HeroView or EnemyView;
        bool showObstacle = !cardMode && token is ObstacleView;
        TerrainView terrain = null;
        bool showTerrain = !cardMode && token == null && TokenSystem.Instance.Setup.TryGetTerrain(pos, out terrain);

        //호버 테두리: 영웅은 노랑(보상 선택 중에는 흰색), 몬스터·장애물·지형지물은 흰색, 빈 칸·카드 타겟 모드는 없음
        string hoverVG = null;
        if (!cardMode)
        {
            if (token is HeroView) hoverVG = RewardSelectSystem.Instance.IsSelecting ? HoverWhiteVG : HoverYellowVG;
            else if (token is EnemyView or ObstacleView || showTerrain) hoverVG = HoverWhiteVG;
        }
        SetHoverVisualGrid(pos, hoverVG);

        if (tokenDetailsUI != null)
        {
            if (showToken) tokenDetailsUI.Show((CombatantView)token);
            else tokenDetailsUI.Hide();
        }

        if (terrainDetailsUI != null)
        {
            if (showObstacle) terrainDetailsUI.ShowObstacle((ObstacleView)token);
            else if (showTerrain) terrainDetailsUI.ShowTerrain(terrain.Data);
            else terrainDetailsUI.Hide();
        }
        //Combat일 경우, 상태효과 페이지
    }
}
