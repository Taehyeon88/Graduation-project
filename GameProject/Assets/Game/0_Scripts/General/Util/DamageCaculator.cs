using UnityEngine;

public static class DamageCaculator
{
    private const float CriticalRatio = 0.5f;   //치명타 : 공격자 공격력 대비 추가 피해 비율

    public static int GetDamage(float baseDamage, CombatantView attacker, IDamageable target)
    {
        float damage = baseDamage;

        if (attacker != null)
        {
            damage += GetPower(attacker);              //힘 연산
            damage += GetCritical(attacker);           //치명타 연산
            damage = CalculateWeak(damage, attacker);  //취약 연산
        }

        if (target != null && target is CombatantView combat)
        {
            damage = CalculateVulnerable(damage, combat);   //약화 연산
        }

        return Mathf.CeilToInt(damage);
    }

    //앞으로 유물, 기타 상태효과 등을 받아와서 데미지 연산 처리
    //(복잡해질 예정)

    //Privates
    private static int GetPower(CombatantView attacker)
    {
        return attacker.GetStatusEffectStacks(StatusEffectType.POWER);
    }

    private static float GetCritical(CombatantView attacker)
    {
        return attacker.CheckStatusEffectExist(StatusEffectType.CRITICAL) ? attacker.BaseDamage * CriticalRatio : 0f;
    }

    private static float CalculateWeak(float damage, CombatantView attacker)
    {
        int weak = attacker.GetStatusEffectStacks(StatusEffectType.WEAK);
        if (weak > 0)
        {
            return damage *= 0.75f;
        }
        return damage;
    }
    private static float CalculateVulnerable(float damage, CombatantView target)
    {
        int vn = target.GetStatusEffectStacks(StatusEffectType.VULNERABLE);
        if (vn > 0)
        {
            return damage *= 1.5f;
        }
        return damage;
    }
}
