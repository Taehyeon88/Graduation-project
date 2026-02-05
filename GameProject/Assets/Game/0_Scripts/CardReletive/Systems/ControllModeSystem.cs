using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class ControllModeSystem : Singleton<ControllModeSystem>
{
    public enum ControllMode
    {
        Move,
        Action
    }

    [SerializeField] private Button moveButton;
    [SerializeField] private Button actionButton;

    private ControllMode currentMode = ControllMode.Action;
    private bool tokenIsMoving = false;


    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);

        moveButton.onClick.AddListener(() => ChangeControllMode(ControllMode.Move));
        actionButton.onClick.AddListener(() => ChangeControllMode(ControllMode.Action));

    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);

        moveButton.onClick.RemoveAllListeners();
        actionButton.onClick.RemoveAllListeners();
    }

    private void ChangeControllMode(ControllMode newMode)
    {
        if (currentMode == newMode) return;

        if (newMode == ControllMode.Move)
        {
            EndActionMode();      //행동 모든 종료
            OnMoveControllMode(); //이동 모드으로 변경
        }
        else if (newMode == ControllMode.Action)
        {
            EndMoveMode();          //이동 모드 종료
            OnActionControllMode(); //행동 모드로 변경
        }
        currentMode = newMode;
    }

    private void OnMoveControllMode()
    {
        //카메라 시전 연출

        //이동한 타일 다시 그리기
        var movedPositions = TokenSystem.Instance.GetMovedPath();
        foreach (var pos in movedPositions)
        {
            VisualGridCreator.Instance.ChangeHeroVisualGrid(pos, Color.gray);
        }

        //현재 SPD가 없어서 이동 불가일 경우, 반환처리
        if (!SPDSystem.Instance.HasEnoughSPD(1)) return;

        //이동 가능 타일 미리 보여주기
        var positions = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, SPDSystem.Instance.RemainSPD());
        foreach (var pos in positions)
        {
            VisualGridCreator.Instance.CreateHeroVisualGrid(pos, new Color32(54, 188, 155, 255));
        }
        InteractionSystem.Instance.SetInteraction(InteractionCase.HeroMove, UpdateHeroMove);   //이동 인터렉션 구독
    }

    private void UpdateHeroMove(bool isSelect)
    {
        if (tokenIsMoving) return;

        Vector3 mousePosition = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1f);
        Vector2Int isoPosition = new((int)mousePosition.x, (int)mousePosition.y);
        if (isSelect)
        {
            CombatantView heroView = HeroSystem.Instance.HeroView;
            if (TokenSystem.Instance.CheckContainMovedPath(isoPosition)) return;

            int distance = TokenSystem.Instance.GetDistance(heroView, isoPosition);
            if (SPDSystem.Instance.HasEnoughSPD(distance))
            {
                var path = TokenSystem.Instance.GetShortestPath(heroView, isoPosition);

                if (path != null)
                {
                    tokenIsMoving = true;
                    PerformMoveGA performMoveGA = new(heroView, path);
                    MoveSystem.Instance.PlayPerformMoveToken(performMoveGA, FinishedPerformer);
                }
            }
            else
            {
                Debug.Log("거리의 밖 구역으로 이동할 수 없습니다.");
            }
        }
    }

    private void FinishedPerformer()
    {
        tokenIsMoving = false;

        if (!SPDSystem.Instance.HasEnoughSPD(1))  //SPD가 0일 경우, 이동 모드 자동 종료
        {
            ChangeControllMode(ControllMode.Action);   //이동 모드 강제 종료
        }
        else  //새로운 비주얼 그리드 그려서 보여주기
        {
            VisualGridCreator.Instance.RemoveHeroVisualGrid();
            var positions = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, SPDSystem.Instance.RemainSPD());
            foreach (var pos in positions)
            {
                VisualGridCreator.Instance.CreateHeroVisualGrid(pos, new Color32(54, 188, 155, 255));
            }
            var movedPositions = TokenSystem.Instance.GetMovedPath();
            foreach (var pos in movedPositions)
            {
                VisualGridCreator.Instance.ChangeHeroVisualGrid(pos, Color.gray);
            }
        }
    }

    private void EndMoveMode()
    {
        VisualGridCreator.Instance.RemoveHeroVisualGrid();
        InteractionSystem.Instance.EndInteraction();
    }
    private void OnActionControllMode()
    {

    }
    private void EndActionMode()
    {

    }

    //Subscribers
    private void EnemysTurnPreReaction(EnemysTurnGA enemyTurnGA)
    {
        //플레이어 턴 종료시, Action모드 혹은 Move모드일 경우, 초기화. 
        VisualGridCreator.Instance.RemoveHeroVisualGrid();
        InteractionSystem.Instance.EndInteraction();

        //+ SPD를 남길 경우, 예외처리
    }
}
