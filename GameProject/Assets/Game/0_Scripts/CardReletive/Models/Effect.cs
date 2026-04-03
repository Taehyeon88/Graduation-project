using System;
using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    public abstract GameAction GetGameAction(EffectInfo effectInfo);

    //데미지 추가 보정치(데미지 변수 보유시 적용)
    private float DamageRate;
    protected float CalculateDamage(float amount) => (1 + DamageRate) * amount;
    public void AddDamageRate(float rate) => DamageRate = rate;
    protected void InitDamageRate() => DamageRate = 0;
}
