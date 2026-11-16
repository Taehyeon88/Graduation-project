using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//소유자 주변(8방향 range칸) 같은 진영 아군 중 무작위 targetCount명에게 상태효과 부여 (소유자 제외, 대상이 적으면 있는 만큼만)
public class AddStatusNearbyAlliesEffect : TraitEffect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int amount = 1;        //대상 1명당 스택
    [SerializeField] private int targetCount = 1;   //대상 수
    [SerializeField] private int range = 1;         //주변 범위 (8방향 기준 칸 수)

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        TokenServiceAPI api = TokenSystem.Instance.API;
        if (owner == null || !api.IsTokenExist(owner)) return;

        Vector2Int ownerPos = api.GetTokenPosition(owner);
        IEnumerable<CombatantView> allies = owner is HeroView
            ? TokenSystem.Instance.HeroViews.Cast<CombatantView>()
            : TokenSystem.Instance.EnemyViews.Cast<CombatantView>();

        List<CombatantView> candidates = allies
            .Where(ally => ally != null && ally != owner && api.IsTokenExist(ally))
            .Where(ally => IsInRange(ownerPos, api.GetTokenPosition(ally)))
            .OrderBy(_ => Random.value)
            .Take(targetCount)
            .ToList();
        if (candidates.Count == 0) return;

        ActionSystem.Instance.AddReaction(new AddStatusEffectGA(statusEffectType, amount, candidates, owner));
    }

    private bool IsInRange(Vector2Int from, Vector2Int to)
    {
        int distance = Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
        return distance >= 1 && distance <= range;
    }
}
