using UnityEngine;

//3 숙달된: 전장에 있는 동안 라운드 종료(Enemy 턴 종료)마다 AP를 추가로 얻는다
public class MasteredRumor : Rumor
{
    [SerializeField] private int apAmount = 1;

    public override void OnAdd(HeroView owner)
    {
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    public override void OnRemove()
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    private void TurnGAPostReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Enemy) return;

        ActionSystem.Instance.AddReaction(new RefillAPGA(apAmount));
    }
}
