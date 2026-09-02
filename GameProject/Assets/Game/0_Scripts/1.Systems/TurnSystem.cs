using System.Collections;
using UnityEngine;

public class TurnSystem : Singleton<TurnSystem>
{
    public TurnType CurrentTurn => currentTurn;

    private TurnType currentTurn = TurnType.GameSetUp;

    protected void OnEnable()
    {
        ActionSystem.AttachPerformer<TurnGA>(TurnGAPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<TurnGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    private IEnumerator TurnGAPerformer(TurnGA turnGA)
    {
        Debug.Log("전투 시작");
        //게임 시작 턴 시작 이후, 플레이어 턴 시작
        if (turnGA.Type == TurnType.StartBattle)
        {
            TurnGA playerTurnGA = new(TurnType.Player);
            ActionSystem.Instance.AddReaction(playerTurnGA);
        }
        else if (turnGA.Type == TurnType.Enemy)
        {
            yield return EnemySystem.Instance.PlayEnemyTurnPerformer();
        }
        yield return null;
    }

    private void TurnGAPostReaction(TurnGA turnGA)
    {
        //몬스터 턴 종료 후, 플레이어 턴 시작
        if(turnGA.Type == TurnType.Enemy)
        {
            TurnGA playerTurnGA = new(TurnType.Player);
            ActionSystem.Instance.AddReaction(playerTurnGA);
        }
    }

    private void TurnGAPreReaction(TurnGA turnGA)
    {
        currentTurn = turnGA.Type;  //턴 타입 변경
    }
}
