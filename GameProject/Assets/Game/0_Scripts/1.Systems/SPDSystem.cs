using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPDSystem : Singleton<SPDSystem>
{
    public int currentSPD { get; private set; }
    public int currentResourceCount { get; private set; }
    public int maxResourceCount { get; private set; } = 1;
    public bool IsMoveMode { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddSpdGA>(AddSpdGAPerformer);
        ActionSystem.AttachPerformer<ResetSpdGA>(ResetSpdGAPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddSpdGA>();
        ActionSystem.DetachPerformer<ResetSpdGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }

    private IEnumerator AddSpdGAPerformer(AddSpdGA addSpdGA)
    {
        currentResourceCount++;
        maxResourceCount = Mathf.Max(maxResourceCount, 1);
        if (currentResourceCount >= maxResourceCount)
        {
            currentSPD += addSpdGA.Amount;
            maxResourceCount++;
            currentResourceCount = 0;
        }
        yield return null;
    }
    private IEnumerator ResetSpdGAPerformer(ResetSpdGA resetSpdGA)
    {
        currentSPD = 0;
        currentResourceCount = 0;
        yield return null;
    }

    public void SpendSPD(int amount)
    {
        currentSPD -= amount;
    }

    public void ChangeMoveMode()
    {
        //이동 포인트가 0 이거나 몬스터턴일 경우, 변경 불가  --> 이동 모드 상태에서 모든 이동 포인트를 소진시, 이동 모드 자동 종료
        if (currentSPD <= 0 || EnemySystem.Instance.IsEnemyTurn || ActionSystem.Instance.IsPerforming) return;

        IsMoveMode = !IsMoveMode;
        if (IsMoveMode) //실행
        {
            PlayerMoveGA playerMoveGA = new(currentSPD);
            ActionSystem.Instance.Perform(playerMoveGA);
        }
    }

    //ReActions
    private void EnemysTurnPostReaction(EnemysTurnGA enemyTurnGA)
    {
        ResetSpdGA resetSpdGA = new();
        ActionSystem.Instance.AddReaction(resetSpdGA);
    }
}
