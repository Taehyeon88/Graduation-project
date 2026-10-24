using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CoreSystem : Singleton<CoreSystem>
{
    private HeroCaptainView HeroCaptain => HeroSystem.Instance.HeroCaptain;
    private EnemyFlagView EnemyFlag => EnemySystem.Instance.EnemyFlag;

    public int PlayerCoreHealth => HeroCaptain != null ? HeroCaptain.CurrentHealth : 0;   //용병대장 체력
    public int EnemyCoreHealth { get; private set; }
    public int EnemyCoreMaxHealth { get; private set; }   //정보 패널 표시용

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackCoreGA>(AttackCoreGAPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackCoreGA>();
    }

    public void SetUp(StageData stageData)
    {
        EnemyCoreHealth = stageData.EnemyCoreHealth;
        EnemyCoreMaxHealth = stageData.EnemyCoreHealth;   //플레이어 코어 체력은 HeroCaptainData에서 View가 보유
    }

    //Performers
    private IEnumerator AttackCoreGAPerformer(AttackCoreGA attackCoreGA)
    {
        CombatantView attacker = attackCoreGA.Caster as CombatantView;
        bool isSkillAttack = attackCoreGA.Caster == null;   //플레이어 스킬: 시전자·모션 없이 적 코어에 Amount만큼 직접 피해
        if (attacker == null && !isSkillAttack) yield break;

        bool isHeroAttack = isSkillAttack || attacker is HeroView;   //영웅은 적 코어, 적은 플레이어 코어를 공격
        TokenServiceAPI api = TokenSystem.Instance.API;
        Vector2Int attackerPos = isSkillAttack ? Vector2Int.zero : api.GetTokenPosition(attacker);
        Vector2 direction = isHeroAttack ? Vector2.right : Vector2.left;

        Sequence attackSeq = null;
        if (isSkillAttack)
        {
            //모션 생략
        }
        else if (attackCoreGA.IsRangedAttack)
        {
            //원거리: 제자리에서 같은 행의 상대 쪽 끝까지 화살 발사, 도착 시점에 피해 적용
            int edgeX = isHeroAttack ? TokenSystem.Instance.gridWidth - 1 : 0;
            Tween arrowTween = ProjectileSystem.Instance.PlayArrowShot(attackerPos, new Vector2Int(edgeX, attackerPos.y));
            yield return arrowTween.WaitForCompletion();
        }
        else
        {
            //기본 공격 모션, 타격 시점에 피해 적용
            bool hit = false;
            attackSeq = Utility.GetBasicAttackTween(attacker, attackerPos, direction, () => hit = true);
            yield return new WaitUntil(() => hit);
        }

        if (!attackCoreGA.IsMissed)   //빗나감이면 모션만 재생하고 피해·승패 판정은 건너뜀
        {
            int damage = DamageCaculator.GetDamage(
                    attackCoreGA.Amount ?? (isSkillAttack ? 0 : attacker.BaseDamage),
                    attacker,
                    null   //코어는 IDamageable이 아니라 대상 쪽 보정은 없음
                );
            if (isHeroAttack)
            {
                EnemyCoreHealth = Mathf.Max(0, EnemyCoreHealth - damage);
                EnemyFlag.UpdateHealth(EnemyCoreHealth);
                EnemyFlag.PlayHitShake();
            }
            else
            {
                HeroCaptain.ReceiveCoreDamage(damage);
                HeroCaptain.PlayHitShake();
            }

            //치명타 : 피해가 적용된 공격 1회마다 스택 1 소모
            if (attacker != null && attacker.CheckStatusEffectExist(StatusEffectType.CRITICAL))
                attacker.RemoveStatusEffect(StatusEffectType.CRITICAL, 1);

            //코어 체력 0: 적 코어면 승리, 플레이어 코어면 패배
            if (isHeroAttack && EnemyCoreHealth <= 0)
                ActionSystem.Instance.AddReaction(new GameClearGA());
            else if (!isHeroAttack && PlayerCoreHealth <= 0)
                ActionSystem.Instance.AddReaction(new GameOverGA());
        }

        if (attackSeq != null && attackSeq.IsActive())
            yield return attackSeq.WaitForCompletion();   //복귀까지 대기
    }
}
