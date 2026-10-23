using UnityEngine;

public class TerrainSystem : Singleton<TerrainSystem>
{
    private TokenSetup Setup => TokenSystem.Instance.Setup;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardGAPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardGAPostReaction, ReactionTiming.POST);
    }

    //Reactions
    private void MoveGAPostReaction(MoveGA moveGA)
    {
        //떠난 발판은 기능 회수(각 발판이 자기가 준 것만 회수), 도착한 칸이 발판이면 기능 적용
        foreach (TerrainView terrain in Setup.Terrains)
            terrain.Exit(moveGA.mover);

        if (Setup.TryGetTerrain(moveGA.movePosition, out TerrainView arrived))
            arrived.Enter(moveGA.mover);
    }

    private void PlayCardGAPostReaction(PlayCardGA playCardGA)
    {
        if (!Setup.TryGetTerrain(playCardGA.TargetPos, out TerrainView terrain)) return;
        if (TokenSystem.Instance.API.GetTokenByPosition(playCardGA.TargetPos) is CombatantView unit)
            terrain.Enter(unit);
    }
}
