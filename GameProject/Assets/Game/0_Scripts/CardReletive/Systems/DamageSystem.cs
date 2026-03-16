using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    [SerializeField] private GameObject damageVFX;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach (var target in dealDamageGA.Targets)
        {
            //상태이상 처리
            //혼란 : N% 공격력 감소
            int disarrayStack = dealDamageGA.Caster.GetStatusEffectStacks(StatusEffectType.DISARRAY);
            if (disarrayStack > 0)
            {
                StatusEffectInfo info = dealDamageGA.Caster.GetStatusEffectInfo(StatusEffectType.DISARRAY);
                float percent = info.disarrayPercent;
                dealDamageGA.Amount -= Mathf.CeilToInt(dealDamageGA.Amount * (percent / 100f));
            }

            target.Damage(dealDamageGA.Amount);
            Instantiate(damageVFX, target.transform.position, target.transform.rotation);
            yield return new WaitForSeconds(0.15f);
            if (target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    KillEnemyGA killEnemyGA = new KillEnemyGA(enemyView);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else
                {
                    //Do some Game over logic here
                    //Open Game over Scene
                }
            }
        }
    }
}
