using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    [field : SerializeField] public GameObject DamageVFX { get; set; }
    [field: SerializeField] public int DamageSoundId { get; set; } = 1;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
        ActionSystem.AttachPerformer<KillGA>(KillPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
        ActionSystem.DetachPerformer<KillGA>();
    }

    //Performers

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach (var target in dealDamageGA.Targets)
        {
            if (target == null)
                continue;

            float amount = dealDamageGA.Amount;
            float temp = amount;

            //상태이상 처리
            if (dealDamageGA.FormulaType == DamageFormulaType.Main) 
            {
                //공격자 계수
                if (dealDamageGA.Caster != null)
                {
                    //혼란 : N% 공격력 감소
                    int disarrayStack = dealDamageGA.Caster.GetStatusEffectStacks(StatusEffectType.DISARRAY);
                    if (disarrayStack > 0)
                    {
                        StatusEffectStorage disarrayInfo = dealDamageGA.Caster.GetStatusEffectInfo(StatusEffectType.DISARRAY);
                        amount -= dealDamageGA.Amount * (disarrayInfo.Disarray_Percent / 100f);
                    }

                    //집중 : N% 공격력 증가
                    int concentrationStatck = dealDamageGA.Caster.GetStatusEffectStacks(StatusEffectType.CONCENTRATION);
                    if (concentrationStatck > 0)
                    {
                        StatusEffectStorage concentrationInfo = dealDamageGA.Caster.GetStatusEffectInfo(StatusEffectType.CONCENTRATION);
                        amount += dealDamageGA.Amount * (concentrationInfo.Concentration_Percent / 100f);
                    }

                    //카드 효과 : 다음 인접 카드 50% 증가
                    var attackerEvent = CardAbilitySystem.Instance.AddNextAdjDamageEvent;
                    if (attackerEvent != null)
                    {
                        amount += dealDamageGA.Amount * attackerEvent.Invoke();
                    }
                }

                //방어자 계수
                float defenderAmount = amount;
                //표적 : N% 받는 피해 증가
                int markStack = target.GetStatusEffectStacks(StatusEffectType.MARK);
                if (markStack > 0)
                {
                    StatusEffectStorage markInfo = target.GetStatusEffectInfo(StatusEffectType.MARK);
                    amount += dealDamageGA.Amount * (markInfo.Mark_Percent / 100f);
                }
            }

            int amountInt = Mathf.CeilToInt(amount);
            Debug.Log($"{dealDamageGA.Caster?.name}가 {target.name}에게 구 - {temp}데미지 | 실 - {amountInt}데미지");

            target.Damage(amountInt);
            Instantiate(DamageVFX, target.transform.position, target.transform.rotation);
            SoundSystem.Instance.PlaySound(DamageSoundId);

            yield return new WaitForSeconds(0.15f);
            if (target.CurrentHealth <= 0)
            {
                if (target is not HeroView)
                {
                    KillGA killGA = new KillGA(target);
                    dealDamageGA.PerformReactions.Add((killGA, () =>
                    {
                        if (EnemySystem.Instance.Enemise.Count <= 0)
                        {
                            if (WaveSystem.Instance.IsWaveRunning)
                            {
                                SpawnWaveGA spawnWave = new();
                                dealDamageGA.PostReactions.Add((spawnWave, null));
                            }
                            else
                            {
                                GameClearGA gameClearGA = new();
                                dealDamageGA.PostReactions.Add((gameClearGA, null));
                            }
                        }
                    }
                    ));
                }
                else
                {
                    GameOverGA gameOverGA = new();
                    ActionSystem.Instance.AddReaction(gameOverGA);
                }
            }
        }
    }

    private IEnumerator KillPerformer(KillGA killGA)
    {
        if (killGA.Token is EnemyView)
        {
            VisualGridCreator.Instance.RemoveVisualGridById(killGA.Token.gameObject.GetInstanceID());
        }

        yield return TokenSystem.Instance.RemoveToken(killGA.Token);
    }
}
