using System;
using UnityEngine;

//손패 카드로 자신이 배치됐을 때 (전투 전 초기 배치·웨이브 생성은 PlayCardGA를 거치지 않아 제외)
//requireNoOpponentInFront가 켜져 있으면 정면 1칸에 상대 진영 유닛이 없을 때만 충족 (권장 reactionTiming: POST)
public class SelfPlacedCondition : TraitCondition
{
    [SerializeField] private bool requireNoOpponentInFront;   //정면 1칸에 상대 진영 유닛이 없어야 하는지

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<PlayCardGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<PlayCardGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        var playCardGA = action as PlayCardGA;
        TokenServiceAPI api = TokenSystem.Instance.API;
        Vector2Int ownerPos = api.GetTokenPosition(owner);
        if (ownerPos != playCardGA.TargetPos) return false;

        if (!requireNoOpponentInFront) return true;

        Token frontToken = api.GetTokenByPosition(ownerPos + owner.FrontDirection);
        return owner is HeroView ? frontToken is not EnemyView : frontToken is not HeroView;
    }
}
