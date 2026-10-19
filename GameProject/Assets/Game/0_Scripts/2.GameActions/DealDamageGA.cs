using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    public float Amount { get; set; }
    public List<IDamageable> Targets { get; private set; }
    public IDamageable Target { get; set; }
    public Token Caster { get; private set; }
    public bool IsBasicAttack { get; private set; }   //자동 전투 기본 공격 (공격 모션 재생)
    public bool IsRangedAttack { get; private set; }  //기본 공격 모션을 근접 돌진 대신 화살 발사로 재생

    public DealDamageGA(float amount, List<IDamageable> targets, Token caster)
    {
        Amount = amount;
        Targets = new(targets);
        Caster = caster;
    }

    public DealDamageGA(float amount, IDamageable target, Token caster)
    {
        Amount = amount;
        Target = target;
        Caster = caster;
    }

    public DealDamageGA(float amount, IDamageable target, Token caster, bool isBasicAttack)
    {
        Amount = amount;
        Target = target;
        Caster = caster;
        IsBasicAttack = isBasicAttack;
    }

    public DealDamageGA(float amount, IDamageable target, Token caster, bool isBasicAttack, bool isRangedAttack)
        : this(amount, target, caster, isBasicAttack)
    {
        IsRangedAttack = isRangedAttack;
    }
}
