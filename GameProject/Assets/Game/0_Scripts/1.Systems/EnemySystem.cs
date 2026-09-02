using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public bool IsEnemyTurn { get; private set; }
    public IReadOnlyList<EnemyView> Enemise => TokenSystem.Instance.EnemyViews;
    public Action<int> EnemyAddEvent { get; private set; }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction2, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);

    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction2, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }

    //Publics
    public IEnumerator PlayEnemyTurnPerformer()
    {
        //몬스터 턴 시작
        foreach (EnemyView enemy in Enemise)
        {
            EnemyTurnGA enemyTurnGA = new(enemy);
            ActionSystem.Instance.AddReaction(enemyTurnGA);
        }

        yield return null;
    }

    //Performers

    //각 몬스터 턴 실행
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurn)
    {
        EnemyView enemy = enemyTurn.EnemyView;

        //미리 예약한 행동 실행
        if (enemy.NextAction != null)
        {
            if (enemy.Enemy.PerformAction(enemy, enemy.NextAction))
            {
                var motion = enemy.NextAction.PlayEnemyAction(enemy);
                yield return motion?.WaitForCompletion();
            }
        }
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        var targets = new List<CombatantView>(10);

        if (attackHeroGA.AttackArea != null)
        {
            foreach (var attackPos in attackHeroGA.AttackArea)
            {
                CombatantView target = TokenSystem.Instance.API.GetTokenByPosition(attackPos) as CombatantView;
                if (target != null)
                    if (target is HeroView)
                        targets.Add(target);
            }
        }
        else
        {
            CombatantView target = TokenSystem.Instance.API.GetTokenByPosition(attackHeroGA.AttackPosition) as CombatantView;
            targets.Add(target);
        }

        if (targets.Count > 0)
        {
            DealDamageGA dealDamageGA = new(attackHeroGA.DamageAmount, targets, attackHeroGA.Caster);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }

        Debug.Log("공격 처리");

        yield return new WaitForSeconds(0.1f);  //hit stop
    }

    //Reactions

    //몬스터s 턴 시작 전
    private void TurnGAPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Enemy) return;

        //적들의 턴 시작시, 방어막 스택 제거
        foreach (EnemyView enemy in Enemise)
        {
            //이동 포인트 초기화
            enemy.ResetMovePoint();

            int armorStack = enemy.GetStatusEffectStacks(StatusEffectType.ARMOR);
            if (armorStack > 0) enemy.RemoveStatusEffect(StatusEffectType.ARMOR, armorStack);

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

    //몬스터들 턴 종료 전
    //모든 적들 다음으로 할 행동 판단 및 보여주기
    private void TurnGAPostReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Enemy) return;

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
            EnemyAction action = enemy.Enemy.JudgeActAction(enemy);
            if (action != null)
                enemy.SetNextAction(action);
        }
    }

    // <summary>
    // 게임 시작시, 다음 할 행동 설정
    private void TurnGAPostReaction2(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.StartBattle) return;

        foreach (EnemyView enemy in Enemise)
        {
            //다음 턴에 할 행동 미리 설정
            EnemyAction action = enemy.Enemy.JudgeActAction(enemy);
            if (action != null)
                enemy.SetNextAction(action);
        }
    }
}
