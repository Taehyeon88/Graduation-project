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
        Sequence attackSeq = null;
        bool damaged = false;   //피해가 실제로 적용됐는지 (치명타 소모 판정용)

        //자동 전투 기본 공격: 대상은 Battle()이 예약 시점에 확정, 여기서는 모션만 재생
        if (dealDamageGA.IsBasicAttack && dealDamageGA.Target != null && dealDamageGA.Caster is HeroCaptainView captain)
        {
            //그리드 밖 용병대장: Iso 오른쪽으로 근접 모션, 타격 시점에 피해 적용
            bool hit = false;
            attackSeq = Utility.GetBasicAttackTween(captain, captain.TokenTransform.positionXY, Vector2.right, () => hit = true);
            yield return new WaitUntil(() => hit);
        }
        else if (dealDamageGA.IsBasicAttack && dealDamageGA.Target != null && dealDamageGA.Caster is CombatantView attacker)
        {
            //기본 공격 모션 1 재생, 타격 시점에 피해 적용
            Vector2Int attackerPos = TokenSystem.Instance.API.GetTokenPosition(attacker);
            Vector2Int targetPos = TokenSystem.Instance.API.GetTokenPosition(dealDamageGA.Target as Token);
            Vector2 direction = Utility.GetSignVector2Int(targetPos - attackerPos);

            if (dealDamageGA.IsRangedAttack)
            {
                //원거리: 제자리에서 화살 발사, 도착 시점에 피해 적용
                Tween arrowTween = ProjectileSystem.Instance.PlayArrowShot(attackerPos, targetPos);
                yield return arrowTween.WaitForCompletion();
            }
            else
            {
                bool hit = false;
                attackSeq = Utility.GetBasicAttackTween(attacker, attackerPos, direction, () => hit = true);
                yield return new WaitUntil(() => hit);
            }
        }

        if (dealDamageGA.Targets != null)   //동시 단체 피격 처리
        {
            foreach (var target in dealDamageGA.Targets)
            {
                if (target == null || (target as Token) == null) continue;   //인터페이스 null 비교는 파괴된 오브젝트를 못 걸러서 구체 타입으로 검사

                //회피 : 스택 1 소모하고 피해·피격 연출 생략 (회피 무시 시 건너뜀)
                if (!dealDamageGA.IgnoreDodge && target is CombatantView dodger && dodger.CheckStatusEffectExist(StatusEffectType.DODGE))
                {
                    dodger.RemoveStatusEffect(StatusEffectType.DODGE, 1);
                    continue;
                }

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
                dealDamageGA.DamagedTargets.Add(target);
                damaged = true;
            }
        }
        else if (dealDamageGA.Target != null && (dealDamageGA.Target as Token) == null)
        {
            //실행 전에 이미 파괴된 대상: 피해·연출 생략
        }
        else if (!dealDamageGA.IgnoreDodge && dealDamageGA.Target is CombatantView singleDodger && singleDodger.CheckStatusEffectExist(StatusEffectType.DODGE))
        {
            //회피 : 스택 1 소모하고 피해·피격 연출 생략
            singleDodger.RemoveStatusEffect(StatusEffectType.DODGE, 1);
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
            dealDamageGA.DamagedTargets.Add(dealDamageGA.Target);
            damaged = true;
        }

        //치명타 : 피해가 적용된 공격 1회마다 스택 1 소모 (다중 대상은 전원 보너스를 받은 뒤 1회만)
        if (damaged && dealDamageGA.Caster is CombatantView critCaster && critCaster.CheckStatusEffectExist(StatusEffectType.CRITICAL))
            critCaster.RemoveStatusEffect(StatusEffectType.CRITICAL, 1);

        if (attackSeq != null && attackSeq.IsActive())
            yield return attackSeq.WaitForCompletion();   //복귀까지 대기
        else
            yield return null;
    }

    private IEnumerator KillGAPerformer(KillGA killGA)
    {
        //피격되서 흔들리는 연출
        Tween hit_Tween = killGA.Hit_Tween;
        if (hit_Tween != null)
        {
            hit_Tween.Restart();
            yield return hit_Tween.WaitForCompletion();
        }

        VisualGridCreator.Instance.RemoveVisualGridById(killGA.Token.gameObject.GetInstanceID());
        yield return TokenSystem.Instance.Main.RemoveToken(killGA.Token);
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
