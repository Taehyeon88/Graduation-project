using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : Singleton<MoveSystem>
{
    private int currentSPD;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PlayerMoveGA>(PlayerMoveGAPerformer);
        ActionSystem.AttachPerformer<PerformMoveGA>(PerformMoveGAPerformer);
        ActionSystem.AttachPerformer<MoveGA>(MoveGAPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<PlayerMoveGA>(PlayerMovePostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PlayerMoveGA>();
        ActionSystem.DetachPerformer<PerformMoveGA>();
        ActionSystem.DetachPerformer<MoveGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<PlayerMoveGA>(PlayerMovePostReaction, ReactionTiming.POST);
    }

    //Player
    public void PlayPerformMoveToken(PerformMoveGA performMoveGA, System.Action OnPerformFinished)
    {
        ActionSystem.Instance.Perform(performMoveGA, OnPerformFinished);
    }

    //Performer
    private IEnumerator PlayerMoveGAPerformer(PlayerMoveGA playerMoveGA)
    {
        currentSPD = playerMoveGA.Distance;

        //현재 SPD가 없어서 이동 불가일 경우, 반환처리
        if (currentSPD <= 0) yield break;

        //비주얼 그리드 설정
        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());            //이동타일 초기화
        var movedPositions = TokenSystem.Instance.GetMovedPath();                               //이동한 타일 다시 그리기
        foreach (var pos in movedPositions)
        {
            VisualGridCreator.Instance.ChangeVisualGrid(pos, gameObject.GetInstanceID(), "Hero_Move", "Hero_Moved");
            Debug.Log($"이동한 범위:{pos}");
        }

        var positions = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, currentSPD); //이동 가능 타일 미리 보여주기
        foreach (var pos in positions)
        {
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Hero_Move");
        }

        while (true)
        {
            if (InteractionSystem.GridSelected)
            {
                Vector3 mousePosition = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1f);
                Vector2Int isoPosition = new((int)mousePosition.x, (int)mousePosition.y);
                CombatantView heroView = HeroSystem.Instance.HeroView;

                if (TokenSystem.Instance.CheckContainMovedPath(isoPosition)) yield return null;

                int distance = TokenSystem.Instance.GetDistance(heroView, isoPosition);

                if (currentSPD >= distance)
                {
                    var path = TokenSystem.Instance.GetShortestPath(heroView, isoPosition);
                    if (path != null)
                    {
                        currentSPD -= distance;

                        PerformMoveGA performMoveGA = new(heroView, path);
                        ActionSystem.Instance.AddReaction(performMoveGA);

                        yield break;
                    }
                }
                else
                {
                    Debug.Log("거리의 밖 구역으로 이동할 수 없습니다.");
                }
            }

            yield return null;
        }
    }
    private IEnumerator PerformMoveGAPerformer(PerformMoveGA performMoveGA)
    {
        if (performMoveGA.mover == null) yield break;   //파괴된 몬스터 예외처리

        CombatantView mover = performMoveGA.mover;
        List<Vector2Int> path = performMoveGA.path;

        foreach (Vector2Int p in path)
        {
            MoveGA moveGA = new(mover, p);
            ActionSystem.Instance.AddReaction(moveGA);
        }

        yield return null;
    }
    private IEnumerator MoveGAPerformer(MoveGA moveGA)
    {
        Token mover = moveGA.mover;
        Vector2Int position = moveGA.movePosition;

        if (mover is HeroView)
        {
            yield return TokenSystem.Instance.MoveToken(mover, position);
            VisualGridCreator.Instance.ChangeVisualGrid(position, gameObject.GetInstanceID(), "Hero_Move", "Hero_Moved");
        }
        else yield return TokenSystem.Instance.MoveToken(mover, position);
    }

    //Subscribers
    private void EnemysTurnPreReaction(EnemysTurnGA enemyTurnGA)   //몬스터 턴 시작 전, 플레이어 이동한 경로 초기화
    {
        TokenSystem.Instance.ResetMovedPath();

        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
        TokenSystem.Instance.ResetMovedPath();
    }

    private void PlayerMovePostReaction(PlayerMoveGA _playerMoveGA)
    {
        if(currentSPD >= 1)  //새로운 비주얼 그리드 그려서 보여주기
        {
            PlayerMoveGA playerMoveGA = new(null, currentSPD);
            ActionSystem.Instance.AddReaction(playerMoveGA);
        }
        else
        {
            if (_playerMoveGA.IsFirstMove)
            {
                VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
                TokenSystem.Instance.ResetMovedPath();
            }
        }
    }
}
