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

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamageGAPerformer);
        ActionSystem.AttachPerformer<KillGA>(KillGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
        ActionSystem.DetachPerformer<KillGA>();
    }

    //Performers
    private IEnumerator DealDamageGAPerformer(DealDamageGA dealDamageGA)
    {
        if (dealDamageGA.Targets != null)   //동시 단체 피격 처리
        {
            foreach (var target in dealDamageGA.Targets)
            {
                if (target == null) continue;

                //데미지 적용 로직
                int amountInt = DamageCaculator.GetDamage(
                        dealDamageGA.Amount,
                        dealDamageGA.Caster as CombatantView,
                        target
                    );

                //피격 이펙트 연출
                PlayDamageVFX(
                    (target as Token).Model.transform.position, 
                    target is HeroView
                    );

                target.Damage(amountInt, dealDamageGA);
            }
        }
        else if (dealDamageGA.Target != null)
        {
            //데미지 적용 로직
            int amountInt = DamageCaculator.GetDamage(
                    dealDamageGA.Amount,
                    dealDamageGA.Caster as CombatantView,
                    dealDamageGA.Target
                 );

            //피격 이펙트 연출
            PlayDamageVFX(
                (dealDamageGA.Target as Token).Model.transform.position, 
                dealDamageGA.Target is HeroView
                );

            dealDamageGA.Target.Damage(amountInt, dealDamageGA);
        }

        yield return null;
    }

    private IEnumerator KillGAPerformer(KillGA killGA)
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
        if (TokenSystem.Instance.EnemyViews.Count <= 0)
        {
            // GameClearGA gameClearGA = new();
            // ActionSystem.Instance.AddReaction(gameClearGA);
        }
        else if (HeroSystem.Instance.HeroViews.Count <= 0)
        {
            GameOverGA gameOverGA = new();
            ActionSystem.Instance.AddReaction(gameOverGA);
        }
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
