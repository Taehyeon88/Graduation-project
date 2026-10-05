using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    public float Amount { get; set; }
    public List<IDamageable> Targets { get; private set; }
    public IDamageable Target { get; set; }   //공격자 정면 공격은 Performer가 실행 시점에 기록
    public Token Caster { get; private set; }

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

    //대상 미지정 — Performer가 실행 시점에 공격자 정면에서 대상을 결정
    public DealDamageGA(float amount, CombatantView attacker)
    {
        Amount = amount;
        Caster = attacker;
    }
}
