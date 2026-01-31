using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : Singleton<MoveSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformMoveGA>(PerformMoveGAPerformer);
        ActionSystem.AttachPerformer<MoveGA>(MoveGAPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformMoveGA>();
        ActionSystem.DetachPerformer<MoveGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    }

    //Perform
    public void PerformMoveToken(PerformMoveGA performMoveGA, System.Action OnPerformFinished)
    {
        if (performMoveGA.mover is EnemyView)
        {
            OnPerformFinished += TokenSystem.Instance.ResetMovedPath;   //필수 초기화 (플레이어는 밑에 EnemyTurn_Pre때, 초기화되게 구독.)
        }

        ActionSystem.Instance.Perform(performMoveGA, OnPerformFinished);
    }

    //Performer
    private IEnumerator PerformMoveGAPerformer(PerformMoveGA performMoveGA)
    {
        CombatantView mover = performMoveGA.mover;
        List<Vector2Int> path = performMoveGA.path;

        if (mover is HeroView)
        {
            SpendSPDGA spendSPDGA = new(path.Count);
            ActionSystem.Instance.AddReaction(spendSPDGA);
        }

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
            Vector3 currentPos = TokenSystem.Instance.GetTokenWorldPosition(mover);
            yield return TokenSystem.Instance.MoveToken(mover, position);
            VisualGridCreator.Instance.ChangeHeroVisualGrid(currentPos, Color.gray);
        }
        else yield return TokenSystem.Instance.MoveToken(mover, position);
    }

    //Subscribers
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        TokenSystem.Instance.ResetMovedPath();
    }
}
