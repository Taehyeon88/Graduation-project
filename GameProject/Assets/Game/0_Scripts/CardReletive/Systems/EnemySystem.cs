using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public IReadOnlyList<EnemyView> Enemise => TokenSystem.Instance.EnemyViews;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemysTurnGA>(EnemysTurnPerformer);
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<MoveGA>(MoveGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemysTurnGA>();
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
    }

   //Performers
    private IEnumerator EnemysTurnPerformer(EnemysTurnGA enemysTurn)
    {
        if (enemysTurn.isStartGame) yield break;   //게임 시작시, 반환처리

        //적 행동 순서 판단(확장)

        foreach (EnemyView enemy in Enemise)
        {
            EnemyTurnGA enemyTurnGA = new(enemy);
            ActionSystem.Instance.AddReaction(enemyTurnGA);
        }
        yield return null;
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurn)
    {
        EnemyView enemy = enemyTurn.EnemyView;

//---------------------------------------------몬스터 상태효과-----------------------------------------------

        int burnStack = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStack > 0)
        {
            ApplyBurnGA applyBurnGA = new(burnStack, enemy);
            ActionSystem.Instance.AddReaction(applyBurnGA);
        }

        //상태 이상 - 고립 처리
        bool isIsolation = false;
        int isolationStack = enemy.GetStatusEffectStacks(StatusEffectType.ISOLATION);
        if (isolationStack > 0)
        {
            isIsolation = true;
        }

        //-------------------------------------------------------------------------------------------------------

        //!! - 고립 조건에 걸리는 행동은 예외처리!!
        //미리 예약한 행동 실행
        if (enemy.NextAction != null)
        {
            enemy.NextAction.PlayEnemyAction(enemy);
        }

        //이동 판단 및 실행
        if (enemy.NextMovePath != null && !isIsolation)
        {
            enemy.Enemy.PlayMoveAction(enemy, enemy.NextMovePath);
        }

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;
        Tween tween = DomoveX(attacker, -1f, 0.15f);
        yield return tween.WaitForCompletion();
        DomoveX(attacker, 1f, 0.25f);

        //공격범위에 플레이어 존재 여부 체크
        var heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        bool isExist = false;
        foreach (var attackPos in attackHeroGA.AttackArea)
        {
            if(attackPos == heroPos) isExist = true;
        }

        //공격범위에 있을 때, 공격 실행
        if (isExist)
        {
            DealDamageGA dealDamageGA = new(attackHeroGA.DamageAmount, new() { HeroSystem.Instance.HeroView }, attackHeroGA.Caster);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }

        yield return new WaitForSeconds(0.5f);

        //미리 보여준 공격 범위 삭제
        VisualGridCreator.Instance.RemoveVisualGrid(attacker.GetInstanceID(), "Enemy_Attack");
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return TokenSystem.Instance.RemoveEnemy(killEnemyGA.EnemyView);
    }

    //Reactions
    private void EnemysTurnPostReaction(EnemysTurnGA enemysTurnGA)   //모든 적들 다음으로 할 행동 판단 및 보여주기
    {
        foreach (EnemyView enemy in Enemise)
        {
//---------------------------------------------몬스터 상태효과-----------------------------------------------
            //몬스터 상태효과 N감소
            foreach (var statusEffectType in enemy.GetStatusEffects())
            {
                //기간제 및 조건제만 실행
                var mcType = StatusEffectSystem.Instance.GetMachanicsType(statusEffectType);
                if (mcType == SEMachanicsType.FixedTerm || mcType == SEMachanicsType.ConditionTerm)
                {
                    enemy.RemoveStatusEffect(statusEffectType, 1);
                }
            }

            //상태효과 - 악화 삭제
            bool tdSEExist = enemy.CheckStatusEffectExist(StatusEffectType.POISIONING)
                           || enemy.CheckStatusEffectExist(StatusEffectType.BLEEDING);
            if (!tdSEExist)
                enemy.RemoveStatusEffect(StatusEffectType.DETERIORATE, 0);
            //-------------------------------------------------------------------------------------------------------

            //다음 턴에 할 행동 미리 설정
            enemy.SetNextAction(enemy.Enemy.PreJudgeActAction(enemy));
            enemy.NextMovePath = enemy.Enemy.PreJudgeMoveAction(enemy);

            //미리 보기 설정
            enemy.Enemy.SetDrawActActionVG(true, enemy, enemy.NextAction);
            enemy.Enemy.SetDrawMoveActionVG(true, enemy, enemy.NextMovePath);
        }
    }

    private void EnemysTurnPreReaction(EnemysTurnGA enemysTurnGA)
    {
        //적들의 턴 시작시, 방어막 스택 제거
        foreach (EnemyView enemy in Enemise)
        {
            int armorStack = enemy.GetStatusEffectStacks(StatusEffectType.ARMOR);
            if(armorStack > 0) enemy.RemoveStatusEffect(StatusEffectType.ARMOR, armorStack);

            //상태 효과
            //악화
            float specialRate = 1;
            bool deteriaorateExist = enemy.CheckStatusEffectExist(StatusEffectType.DETERIORATE);
            if (deteriaorateExist)
            {
                float rate = enemy.GetStatusEffectInfo(StatusEffectType.DETERIORATE).Deteriorate_Rate;
                specialRate *= rate;
            }

            //독물
            int poisionStatcks = enemy.GetStatusEffectStacks(StatusEffectType.POISIONING);
            if (poisionStatcks > 0)
            {
                float percent = enemy.GetStatusEffectInfo(StatusEffectType.POISIONING).Poision_Percent;
                float amount = enemy.MaxHealth * (percent / 100f) * specialRate;
                DealDamageGA dealDamageGA = new(amount, new() { enemy }, enemy, DamageFormulaType.Special);
                ActionSystem.Instance.AddReaction(dealDamageGA);
            }

            //출혈
            int bleedingStatcks = enemy.GetStatusEffectStacks(StatusEffectType.BLEEDING);
            if (bleedingStatcks > 0)
            {
                float percent = enemy.GetStatusEffectInfo(StatusEffectType.BLEEDING).Bleeding_Percent;
                float amount = enemy.MaxHealth * (percent / 100f) * specialRate;
                DealDamageGA dealDamageGA = new(amount, new() { enemy }, enemy, DamageFormulaType.Special);
                ActionSystem.Instance.AddReaction(dealDamageGA);
            }
        }
    }

    private void MoveGAPreReaction(MoveGA moveGA)
    {
        if (moveGA.mover is EnemyView enemyView && moveGA.isKnockBack)
        {
            if (enemyView != null)
            {
                enemyView.Enemy.SetDrawActActionVG(false, enemyView, enemyView.NextAction);
                enemyView.Enemy.SetDrawMoveActionVG(false, enemyView, null);
            }
        }
    }
    private void MoveGAPostReaction(MoveGA moveGA)
    {
        if (moveGA.mover is EnemyView enemyView && moveGA.isKnockBack)
        {
            if (enemyView != null)
            {
                enemyView.Enemy.SetDrawActActionVG(true, enemyView, enemyView.NextAction);
                enemyView.NextMovePath = enemyView.Enemy.PreJudgeMoveAction(enemyView);
                enemyView.Enemy.SetDrawMoveActionVG(true, enemyView, enemyView.NextMovePath);
            }
        }
    }

    //privates
    private Tween DomoveX(Token token, float dis, float duration)
    {
        float startX = token.TokenTransform.positionX;
        return DOTween.To(() =>
                 token.TokenTransform.positionX,
                 x => token.TokenTransform.positionX = x,
                 startX + dis,
                 duration
               );
    }
}
