using UnityEngine;

//날카로운 단도 : 적 용병 또는 우두머리를 지정해 피해를 준다 (회피 무시, 장애물 지정 불가)
public class SharpDagger : PlayerSkill
{
    [SerializeField] private int damage = 2;

    public override bool IsValidTarget(Token target)
    {
        return target is EnemyView || target is EnemyFlagView;
    }

    public override GameAction CreateAction(Token target)
    {
        if (target is EnemyFlagView)
            return new AttackCoreGA(null, damage);

        return new DealDamageGA(damage, target as IDamageable, null, false, false, true);
    }
}
