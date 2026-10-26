using System;

//자신이 생성·배치됐을 때 — 영웅은 손패 카드 배치(PlayCardGA), 몬스터는 웨이브 생성(SpawnWaveGA)
//전투 시작 때 깔리는 초기 몬스터는 GA를 거치지 않아 생성 직후의 전투 시작(TurnGA StartBattle)에서 대신 판정 (웨이브 몬스터는 그 뒤에 만들어져 겹치지 않음)
//권장 reactionTiming: POST — AddToken 이후라 위치 조회 가능
public class SelfDeployedCondition : TraitCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<PlayCardGA>(reaction, reactionTiming);
        ActionSystem.SubscribeReaction<SpawnWaveGA>(reaction, reactionTiming);
        ActionSystem.SubscribeReaction<TurnGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<PlayCardGA>(reaction, reactionTiming);
        ActionSystem.UnsubscribeReaction<SpawnWaveGA>(reaction, reactionTiming);
        ActionSystem.UnsubscribeReaction<TurnGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        if (owner == null) return false;   //파괴된 뷰 (해제 누락된 구독 방어)

        if (action is TurnGA turnGA) return turnGA.Type == TurnType.StartBattle && owner is EnemyView;   //초기 배치 몬스터

        if (action is SpawnWaveGA spawnWaveGA) return owner is EnemyView enemy && spawnWaveGA.SpawnedEnemies.Contains(enemy);

        var playCardGA = action as PlayCardGA;
        return TokenSystem.Instance.API.GetTokenPosition(owner) == playCardGA.TargetPos;
    }
}
