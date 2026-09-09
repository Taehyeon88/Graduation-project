using CartoonFX;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    [Header("Direct Element")]
    [SerializeField] private GameObject[] damageVFXs;

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
        if (dealDamageGA.Targets != null)   //동시 단체 피격 처리
        {
            foreach (var target in dealDamageGA.Targets)
            {
                if (target == null) continue;

                //데미지 적용 로직
                int amountInt = CalculateDamage(dealDamageGA.Caster, target, dealDamageGA.Amount);
                Debug.Log($"{dealDamageGA.Caster?.name}가 {target.name}에게 구 - {dealDamageGA.Amount}데미지 | 실 - {amountInt}데미지");

                PlayDamageVFX(target.Model.transform.position, target is HeroView);   //피격 이펙트 연출

                target.Damage(amountInt, dealDamageGA);
            }
        }
        else
        {
            //데미지 적용 로직
            int amountInt = CalculateDamage(dealDamageGA.Caster, dealDamageGA.Target, dealDamageGA.Amount);
            Debug.Log($"{dealDamageGA.Caster?.name}가 {dealDamageGA.Target.name}에게 구 - {dealDamageGA.Amount}데미지 | 실 - {amountInt}데미지");

            //피격 이펙트 연출
            PlayDamageVFX(dealDamageGA.Target.Model.transform.position, dealDamageGA.Target is HeroView);

            dealDamageGA.Target.Damage(amountInt, dealDamageGA);
        }

        yield return null;
    }

    private IEnumerator KillPerformer(KillGA killGA)
    {
        //피격되서 흔들리는 연출
        Tween hit_Tween = killGA.Hit_Tween;
        if (hit_Tween != null)
        {
            hit_Tween.Restart();
            //hit_Tween.OnUpdate(() =>
            //{
            //    Debug.Log("피격 애니 중..");
            //    Debug.Log($"실시간 - 활성화 여부 : {hit_Tween.IsActive()}");
            //});
            yield return hit_Tween.WaitForCompletion();
            //Debug.Log($"완료 여부 : {hit_Tween.IsComplete()}");
            //Debug.Log($"활성화 여부 : {hit_Tween.IsActive()}");
        }
        //else
        //{
        //    Debug.Log("hit_Tween이 존재하지 X");
        //}

        VisualGridCreator.Instance.RemoveVisualGridById(killGA.Token.gameObject.GetInstanceID());
        yield return TokenSystem.Instance.Main.RemoveToken(killGA.Token);

        //게임 클리어 or 오버 판단
        if (EnemySystem.Instance.Enemise.Count <= 0)
        {
            GameClearGA gameClearGA = new();
            ActionSystem.Instance.AddReaction(gameClearGA);
        }
        else if (HeroSystem.Instance.HeroViews.Count <= 0)
        {
            GameOverGA gameOverGA = new();
            ActionSystem.Instance.AddReaction(gameOverGA);
        }
    }

    private int CalculateDamage(CombatantView caster, CombatantView target, float damageAmount)
    {
        //상태이상 처리 로직
        float amount = damageAmount;
        float temp = damageAmount;
        if (caster != null)
        {
            //혼란 : N% 공격력 감소
            int disarrayStack = caster.GetStatusEffectStacks(StatusEffectType.DISARRAY);
            if (disarrayStack > 0)
            {
                amount -= damageAmount * (30 / 100f);
            }

            //집중 : N% 공격력 증가
            int concentrationStatck = caster.GetStatusEffectStacks(StatusEffectType.CONCENTRATION);
            if (concentrationStatck > 0)
            {
                amount += damageAmount * (30 / 100f);
            }
        }

        //방어자 계수
        float defenderAmount = amount;
        //표적 : N% 받는 피해 증가
        int markStack = target.GetStatusEffectStacks(StatusEffectType.MARK);
        if (markStack > 0)
        {
            amount += damageAmount * (30 / 100f);
        }

        return Mathf.CeilToInt(amount);
    }

    private void PlayDamageVFX(Vector3 position, bool isShacking = true)
    {
        var vfx = Array.Find(damageVFXs, vfx => !vfx.activeSelf);
        if (vfx != null)
        {
            var cfxr = vfx.GetComponent<CFXR_Effect>();
            if (cfxr != null)
                cfxr.cameraShake.enabled = isShacking;

            vfx.transform.position = position + Vector3.up * 0.5f;
            vfx.SetActive(true);
        }
    }
}
