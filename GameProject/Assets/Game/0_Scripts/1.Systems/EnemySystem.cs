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
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction2, ReactionTiming.POST);
    }

    //Publics
    public IEnumerator PlayEnemyTurnPerformer()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("몬스터s턴 시작");

        EnemysTurnGAPreReaction();        //몬스터 시작 처리

        //몬스터 턴 로직 실행
        foreach (EnemyView enemy in Enemise)
        {
            EnemyTurnGA enemyTurnGA = new(enemy);
            ActionSystem.Instance.AddReaction(enemyTurnGA);
        }
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
        yield return null;
    }

    //Reactions

    //몬스터들 턴 종료 전
    //모든 적들 다음으로 할 행동 판단 및 보여주기
    private void TurnGAPostReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Enemy) return;

        Debug.Log("몬스터 턴 종료");

        foreach (EnemyView enemy in Enemise)
        {
            enemy.ReduceSEWhenMyTurnEnd();   //공용 종료시, SE 제거

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

    //Privates

    //몬스터s 턴 시작
    private void EnemysTurnGAPreReaction()
    {
        //적들의 턴 시작시, 방어막 스택 제거
        foreach (EnemyView enemy in Enemise)
        {
            enemy.ResetMovePoint();            //이동 포인트 초기화
            enemy.ReduceSEWhenMyTurnStart();   //공용 시작시, SE 제거
        }
    }
}
