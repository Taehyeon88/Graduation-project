using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public List<EnemyView> Enemise => TokenSystem.Instance.EnemyViews;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemysTurnGA>(EnemysTurnPerformer);
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemysTurnGA>();
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
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

        int burnStack = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStack > 0)
        {
            ApplyBurnGA applyBurnGA = new(burnStack, enemy);
            ActionSystem.Instance.AddReaction(applyBurnGA);
        }
        //미리 예약한 행동 실행
        if (enemy.actAction != null) 
            ActionSystem.Instance.AddReaction(enemy.actAction);

        //이동 판단 및 실행
        enemy.moveAction = enemy.Enemy.JudgeMoveAction(enemy);
        if (enemy.moveAction != null) 
            ActionSystem.Instance.AddReaction(enemy.moveAction);

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;
        Tween tween = DomoveX(attacker, -1f, 0.15f);
        yield return tween.WaitForCompletion();
        DomoveX(attacker, 1f, 0.25f);

        //공격범위에 플레이어 존재 여부 체크
        var heroPos = TokenSystem.Instance.GetTokenGridPosition(HeroSystem.Instance.HeroView);
        bool isExist = false;
        foreach (var attackPos in attackHeroGA.AttackArea)
        {
            if(attackPos == heroPos) isExist = true;
        }

        //공격범위에 있을 때, 공격 실행
        if (isExist)
        {
            DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { HeroSystem.Instance.HeroView }, attackHeroGA.Caster);
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
            //다음 턴에 할 행동 미리 설정
            enemy.actAction = enemy.Enemy.JudgeActActions(enemy);

            //다음 턴에 할 공격 비주얼 그리드로 미리 보여주기
            if (enemy.actAction is AttackHeroGA attackHeroGA)
            {
                foreach (Vector2Int gridPos in attackHeroGA.AttackArea)
                {
                    VisualGridCreator.Instance.CreateVisualGrid(enemy.GetInstanceID(), gridPos, "Enemy_Attack");
                }
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
