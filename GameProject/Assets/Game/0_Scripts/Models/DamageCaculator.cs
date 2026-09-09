using UnityEngine;

public static class DamageCaculator
{
    public static int GetDamage(float baseDamage, CombatantView attacker, CombatantView target)
    {
        if (attacker == null || target == null)
            return Mathf.FloorToInt(baseDamage);

        float damage = baseDamage + GetPower(attacker); //힘 연산

        damage = CalculateWeak(damage, attacker);       //취약 연산
        damage = CalculateVulnerable(damage, target);   //약화 연산

        Debug.Log($"기본 : {baseDamage}, 최종 : {Mathf.FloorToInt(damage)}, 힘 수치: {GetPower(attacker)}");

        return Mathf.FloorToInt(damage);
    }

    //앞으로 유물, 기타 상태효과 등을 받아와서 데미지 연산 처리
    //(복잡해질 예정)

    //Privates
    private static int GetPower(CombatantView attacker)
    {
        return attacker.GetStatusEffectStacks(StatusEffectType.POWER);
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
