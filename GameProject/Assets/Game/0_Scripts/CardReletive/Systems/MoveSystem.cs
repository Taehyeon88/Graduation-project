using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : Singleton<MoveSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformMoveGA>(PerformMoveGAPerformer);
        ActionSystem.AttachPerformer<MoveGA>(MoveGAPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformMoveGA>();
        ActionSystem.DetachPerformer<MoveGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
    }

    //Player
    public void PlayPerformMoveToken(PerformMoveGA performMoveGA, System.Action OnPerformFinished)
    {
        ActionSystem.Instance.Perform(performMoveGA, OnPerformFinished);
    }

    //Performer
    private IEnumerator PerformMoveGAPerformer(PerformMoveGA performMoveGA)
    {
        if (performMoveGA.mover == null) yield break;   //파괴된 몬스터 예외처리

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
    private void EnemysTurnPreReaction(EnemysTurnGA enemyTurnGA)   //몬스터 턴 시작 전, 플레이어 이동한 경로 초기화
    {
        TokenSystem.Instance.ResetMovedPath();
    }
}
