using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : Singleton<MoveSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PlayerMoveGA>(PlayerMoveGAPerformer);
        ActionSystem.AttachPerformer<PerformMoveGA>(PerformMoveGAPerformer);
        ActionSystem.AttachPerformer<MoveGA>(MoveGAPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PlayerMoveGA>();
        ActionSystem.DetachPerformer<PerformMoveGA>();
        ActionSystem.DetachPerformer<MoveGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
    }

    //Player
    public void PlayPlayerMove(PlayerMoveGA playerMoveGA, System.Action OnPerformFinished = null)
    {
        if (!ManaSystem.Instance.HasEnoughMana(1)
            || !TokenSystem.Instance.IsGridEmpty(playerMoveGA.TargetPos)
            || TokenSystem.Instance.CheckContainMovedPath(playerMoveGA.TargetPos))
            return;

        if (ActionSystem.Instance.Perform(playerMoveGA, OnPerformFinished))
        {
            SpendManaGA spendManaGA = new(1);
            playerMoveGA.PerformReactions.Add((spendManaGA, null));
        }
    }

    //Performer
    private IEnumerator PlayerMoveGAPerformer(PlayerMoveGA playerMoveGA)
    {
        MoveGA moveGA = new(HeroSystem.Instance.HeroView, playerMoveGA.TargetPos);
        ActionSystem.Instance.AddReaction(moveGA);

        yield return null;
    }

    private IEnumerator PerformMoveGAPerformer(PerformMoveGA performMoveGA)
    {
        CombatantView mover = performMoveGA.mover;
        List<Vector2Int> path = performMoveGA.path;

        if (mover == null) yield break;   //파괴된 몬스터 예외처리

        //대상 이동 처리
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
        if (mover != null)
        {
            yield return TokenSystem.Instance.MoveToken(mover, position);
        }
    }

    //Subscribers
    private void EnemysTurnPreReaction(EnemysTurnGA enemyTurnGA)   //몬스터 턴 시작 전, 플레이어 이동한 경로 초기화
    {
        TokenSystem.Instance.ResetMovedPath();
        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID()); 
        TokenSystem.Instance.ResetMovedPath();
    }
}
