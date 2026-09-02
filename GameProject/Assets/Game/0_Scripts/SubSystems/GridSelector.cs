using System.Linq;
using UnityEngine;

public class GridSelector : MonoBehaviour
{
    private Token selectedToken;
    private Vector2Int selectedGrid = Vector2Int.down;
    private Vector2Int hoveredGrid;


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
        if (Interactions.Instance.IsSkillTargetMode)    //선택 모드시, 모든 처리 반환
        {
            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Selector_Hover");
            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Selector_Select");
            return;
        }

        //선택된 그리드에 변경이 있는지 체크 및 갱신
        if (selectedGrid != Vector2Int.down)
        {
            Token token = TokenSystem.Instance.API.GetTokenByPosition(selectedGrid);
            if (token != selectedToken)
            {
                UpdateSelectedToken(token);
            }
        }


        //현재 마우스 위치에 따라서 호버VG 갱신
        Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
        Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

        if (!TokenSystem.Instance.API.IsBound(pos)) return;

        if (hoveredGrid != pos)
        {
            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Selector_Hover");
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Selector_Hover");
            hoveredGrid = pos;
        }
    }

    //그리드 클릭 실행 함수
    private void SelectToken()
    {
        if (TurnSystem.Instance.CurrentTurn == TurnType.GameSetUp
                    || Interactions.Instance.IsSkillTargetMode) return;    //모든 처리 반환

        Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
        Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

        if (!TokenSystem.Instance.API.IsBound(pos)) return;

        Token token = TokenSystem.Instance.API.GetTokenByPosition(pos);


        //플레이어 이동모드 관리
        if(token != null && token is HeroView heroView)
        {
            MoveSystem.Instance.PlayPlayerMoveMode(heroView);  //이동 모드 실행/종료 함수
        }

        if (MoveSystem.Instance.Hero_MoveRange != null &&
            MoveSystem.Instance.Hero_MoveRange.Contains(pos)) return;  //이동 모드 중, 이동 그리드 클릭시, 반환

        if (selectedGrid == pos) return;

        //Debug.Log("그리드 선택");

        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Selector_Select");
        VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Selector_Select");
        selectedGrid = pos;

        UpdateSelectedToken(token);

    }

    private void UpdateSelectedToken(Token token)
    {
        if (token != null)
        {
            Debug.Log("토큰 업데이트");
            selectedToken = token;

            //기물 설명 페이지
            //Combat일 경우, 상태효과 페이지

            //영웅일 경우, 선택된 영웅 업데이트 및 스킬 페이지
            HeroSystem.Instance.CurrentHero = token as HeroView;
        }
        else
        {
            selectedToken = null;
        }
    }
}
