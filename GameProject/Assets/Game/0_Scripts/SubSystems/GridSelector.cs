using System.Linq;
using UnityEngine;

public class GridSelector : MonoBehaviour
{
    private Vector2Int selected_hero_Pos = Vector2Int.down;
    private Vector2Int hoveredGrid = Vector2Int.down;

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
        }

        //현재 선택된 영웅에 위치에 따라서 선택VG 갱신
        Vector2Int s_pos = TokenSystem.Instance.API.GetTokenPosition(HeroSystem.Instance.CurrentHero);
        if (s_pos != Vector2Int.down && s_pos != selected_hero_Pos)
        {
            VisualGridCreator.Instance.ChangeVisualGridPosition(
                    gameObject.GetInstanceID(), 
                    s_pos, 
                    "Selector_Select"
                );
            selected_hero_Pos = s_pos;
        }

        //현재 마우스 위치에 따라서 호버VG 갱신
        Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
        Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

        if (!TokenSystem.Instance.API.IsBound(pos)) return;

        if (hoveredGrid != pos)
        {
            if (!Interactions.Instance.IsSkillTargetMode)
            {
                SoundSystem.Instance.PlaySound(3001);
                VisualGridCreator.Instance.ChangeVisualGridPosition(
                            gameObject.GetInstanceID(),
                            pos,
                            "Selector_Hover"
                        );
            }
            hoveredGrid = pos;

            Token token = TokenSystem.Instance.API.GetTokenByPosition(pos);
            UpdateHoveredToken(token);
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


        //플레이어 이동모드 및 영웅 선택
        if(token != null && token is HeroView heroView)
        {
            MoveSystem.Instance.PlayPlayerMoveMode(heroView);  //이동 모드 실행/종료 함수

            if (HeroSystem.Instance.CurrentHero == heroView) return;

            SoundSystem.Instance.PlaySound(3002);
            VisualGridCreator.Instance.ChangeVisualGridPosition(
                        gameObject.GetInstanceID(),
                        pos,
                        "Selector_Select"
                    );
            selected_hero_Pos = pos;

            UpdateSelectedToken(token);
        }
    }

    private void UpdateSelectedToken(Token token)
    {
        if (token != null)
        {
            //Debug.Log("영웅 선택");
            //영웅일 경우, 선택된 영웅 업데이트 및 스킬 페이지
            HeroSystem.Instance.CurrentHero = token as HeroView;
        }
    }

    private void UpdateHoveredToken(Token token)
    {
        if (token != null)
        {
            //Debug.Log("토큰 정보UI");
            //기물 설명 페이지
            //Combat일 경우, 상태효과 페이지
        }
    }
}
