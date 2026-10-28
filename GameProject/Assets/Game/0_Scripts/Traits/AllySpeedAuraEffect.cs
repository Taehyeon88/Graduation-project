using System.Collections.Generic;

//전장에 있는 동안 자신의 속도만큼 다른 아군의 속도를 증가 (owner의 특성 중 이 효과를 쓰는 특성을 가진 유닛 제외)
//PlayCardGA(배치): owner가 배치되면 나머지 아군 전원, 다른 아군이 배치되면 그 유닛에게 부여 / KillGA(owner 사망): 부여분 회수
//권장 조건: AuraTriggerCondition (reactionTiming POST)
public class AllySpeedAuraEffect : TraitEffect
{
    private Dictionary<CombatantView, int> speedGrants = new();   //내가 부여한 속도량 (회수용, Trait별 사본이라 유닛별 상태)
    private int ownTraitId;            //owner의 특성 중 이 효과를 쓰는 특성의 Id (첫 부여 때 조회, 이 특성을 가진 유닛은 제외)
    private bool ownTraitResolved;

    public override TraitEffect Clone()
    {
        var clone = (AllySpeedAuraEffect)base.Clone();
        clone.speedGrants = new();   //얕은 복사로 템플릿과 장부가 공유되지 않게 새로 만든다
        clone.ownTraitResolved = false;
        return clone;
    }

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        if (action is KillGA)
            Revoke(owner);
        else
            Grant(action as PlayCardGA, owner);
    }

    private void Grant(PlayCardGA playCardGA, CombatantView owner)
    {
        int amount = owner.Speed;
        if (amount <= 0) return;

        if (!ownTraitResolved)
            ownTraitResolved = owner.TryGetTraitIdByEffect(this, out ownTraitId);

        Token placed = TokenSystem.Instance.API.GetTokenByPosition(playCardGA.TargetPos);

        List<CombatantView> targets = new();
        if (placed == owner)
        {
            if (owner is HeroView)
                targets.AddRange(TokenSystem.Instance.HeroViews);
            else
                targets.AddRange(TokenSystem.Instance.EnemyViews);
            targets.Remove(owner);
        }
        else if (placed is CombatantView ally)
        {
            targets.Add(ally);
        }

        foreach (var target in targets)
        {
            if (target.IsImmobile) continue;   //이동 불가는 속도 증감 대상 아님
            if (ownTraitResolved && target.HasTrait(ownTraitId)) continue;

            speedGrants.TryGetValue(target, out int granted);
            speedGrants[target] = granted + amount;
            ActionSystem.Instance.AddReaction(new ModifyStatGA(StatType.SPEED, amount, new() { target }, owner));
        }
    }

    private void Revoke(CombatantView owner)
    {
        foreach (var grant in speedGrants)
        {
            if (grant.Key == null) continue;   //이미 사망한 대상

            ActionSystem.Instance.AddReaction(new ModifyStatGA(StatType.SPEED, -grant.Value, new() { grant.Key }, owner));
        }
        speedGrants.Clear();
    }
}
