using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public IReadOnlyList<Vector2Int> SetupPoses => setupPoses;   //적 배치 가능 위치 (라인당 1개, 전투 중 불변)
    public EnemyFlagView EnemyFlag { get; set; }                 //적 세력 깃발 (TokenSetup만 설정)

    private List<Vector2Int> setupPoses = new();

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPostReaction, ReactionTiming.POST);
    }

    //자동 전투 턴 종료 시, 몬스터의 기절 1 감소
    private void AutoBattleTurnPostReaction(AutoBattleTurnGA turnGA)
    {
        foreach (var enemy in TokenSystem.Instance.EnemyViews)
        {
            if (enemy != null) enemy.RemoveStatusEffect(StatusEffectType.STUN, 1);
        }
    }

    public void SetUp(List<Vector2Int> poses)
    {
        setupPoses = poses;
    }

    //해당 라인의 배치 위치 x (없으면 false)
    public bool TryGetSetupX(int line, out int x)
    {
        foreach (var pos in setupPoses)
        {
            if (pos.y != line) continue;
            x = pos.x;
            return true;
        }
        x = 0;
        return false;
    }
}
