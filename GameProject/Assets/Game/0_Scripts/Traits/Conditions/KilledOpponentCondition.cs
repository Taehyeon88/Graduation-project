using System;

//자신이 상대 진영 용병을 처치했을 때 (장애물·코어 제외. KillGA는 Caster가 없어 처치자 판정을 여기서 직접 함)
//권장 reactionTiming: PRE — 토큰 제거 전이라 처치된 유닛의 진영을 확인할 수 있음 (사망 계열 기준과 같음)
public class KilledOpponentCondition : TraitCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<KillGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<KillGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        if (owner == null) return false;   //파괴된 뷰 (해제 누락된 구독 방어)

        var killGA = action as KillGA;
        if (killGA.Killer != owner) return false;
        return owner is HeroView ? killGA.Token is EnemyView : killGA.Token is HeroView;
    }
}
