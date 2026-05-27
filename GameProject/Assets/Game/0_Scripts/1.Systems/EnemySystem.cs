using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public IReadOnlyList<EnemyView> Enemise => TokenSystem.Instance.EnemyViews;
    public Action<int> EnemyAddEvent { get; private set; }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemysTurnGA>(EnemysTurnPerformer);
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<EnemyMoveGA>(EnemyMoveGAPerformer);
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
        ActionSystem.DetachPerformer<EnemyMoveGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
    }

   //Performers

    //몬스터 턴 시작
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

    //각 몬스터 턴 실행
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
            var motion = enemy.NextAction.PlayEnemyAction(enemy);
            enemy.NextAction.Directions.Clear();

            yield return motion?.WaitForCompletion();
        }

        EnemyMoveGA enemyMoveGA = new(enemy, isIsolation);
        ActionSystem.Instance.AddReaction(enemyMoveGA);
    }

    private IEnumerator EnemyMoveGAPerformer(EnemyMoveGA enemyMove)
    {
        //이동 판단 및 실행
        if (!enemyMove.IsIsolation)
            enemyMove.EnemyView.Enemy.JudgeAndPlayMove(enemyMove.EnemyView);

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        var attackTargets = new List<CombatantView>();
        foreach (var attackPos in attackHeroGA.AttackArea)
        {
            CombatantView attackTarget = TokenSystem.Instance.GetTokenByPosition(attackPos) as CombatantView;

            //공격범위 안에 공격 가능 대상 존재 여부 체크
            if (attackTarget != null)
            {
                if (attackTarget is HeroView || attackTarget is DestructibleView)
                {
                    attackTargets.Add(attackTarget);
                }
            }
        }

        if (attackTargets.Count > 0)
        {
            DealDamageGA dealDamageGA = new(attackHeroGA.DamageAmount, attackTargets, attackHeroGA.Caster);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }

        yield return new WaitForSeconds(0.1f);

        //미리 보여준 공격 범위 삭제
        VisualGridCreator.Instance.RemoveVisualGrid(attackHeroGA.Caster.GetInstanceID(), "Enemy_Attack");
    }

    //Reactions

    //몬스터들 턴 종료 전
    //모든 적들 다음으로 할 행동 판단 및 보여주기
    private void EnemysTurnPostReaction(EnemysTurnGA enemysTurnGA)
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
            EnemyAction action = enemy.Enemy.PreJudgeActAction(enemy);
            if (action != null)
                enemy.SetNextAction(action);

            //미리 보기 설정
            enemy.Enemy.SetDrawActActionVG(true, enemy, enemy.NextAction);
        }
    }


    //몬스터 들 턴 시작 전
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
        //몬스터 넉백 전, 공격 예고 범위 재판단
        if (moveGA.mover is EnemyView enemyView && moveGA.isKnockBack)
        {
            if (enemyView != null)
                enemyView.Enemy.SetDrawActActionVG(false, enemyView, enemyView.NextAction);
        }
    }

    private void MoveGAPostReaction(MoveGA moveGA)
    {
        //몬스터 넉백 후, 예외처리
        if (moveGA.mover is EnemyView enemyView && moveGA.isKnockBack)
        {
            if (enemyView != null)
                enemyView.Enemy.SetDrawActActionVG(true, enemyView, enemyView.NextAction);
        }

        //영웅이 이동 했을 때, 예고한 행동 재판단
        if (moveGA.mover is HeroView heroView)
        {
            if (heroView != null)
            {
                foreach (var enemy in Enemise)
                {
                    if (enemy.NextAction is WaitEA waitEA)
                    {
                        waitEA.ShouldStopWaiting(enemy);
                    }

                    EnemyAction action = enemy.Enemy.ReCalculate(enemy);
                    if(action != null)
                        enemy.SetNextAction(action);
                }
            }
        }
    }
}
