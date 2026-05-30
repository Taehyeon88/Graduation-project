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
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PlayerMoveGA>();
        ActionSystem.DetachPerformer<PerformMoveGA>();
        ActionSystem.DetachPerformer<MoveGA>();
    }

    //Player
    public void PlayPerformMoveToken(PerformMoveGA performMoveGA, System.Action OnPerformFinished)
    {
        ActionSystem.Instance.Perform(performMoveGA, OnPerformFinished);
    }

    //Performer
    private IEnumerator PlayerMoveGAPerformer(PlayerMoveGA playerMoveGA)
    {
        //현재 SPD가 없어서 이동 불가일 경우, 반환처리
        int curSPD = playerMoveGA.Distance;
        if (curSPD <= 0) yield break;

        CombatantView heroView = HeroSystem.Instance.HeroView;
        Vector2Int targetPos = playerMoveGA.TargetPos;
        int distance = TokenSystem.Instance.GetDistance(heroView, targetPos);
        var path = TokenSystem.Instance.GetShortestPath(heroView, targetPos);
        if (path != null)
        {
            PerformMoveGA performMoveGA = new(heroView, path);
            ActionSystem.Instance.AddReaction(performMoveGA);
        }
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
}
