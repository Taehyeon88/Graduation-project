using System.Collections;
using UnityEngine;

public class APSystem : Singleton<APSystem>
{
    private const int StartBattleAP = 5;   //전투 시작 시 획득 AP
    private const int TurnStartAP = 3;     //플레이어 턴 시작마다 획득 AP

    [SerializeField] private APUI apUI;
    public int MaxAP { get; private set; } = 5;
    public int CurrentAP { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendAPGA>(SpendAPGAPerformer);
        ActionSystem.AttachPerformer<RefillAPGA>(RefillAPGAPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendAPGA>();
        ActionSystem.DetachPerformer<RefillAPGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }
    public bool HasEnoughAP(int ap = 1)
    {
        return CurrentAP >= ap;
    }
    private IEnumerator SpendAPGAPerformer(SpendAPGA spendAPGA)
    {
        CurrentAP -= spendAPGA.Amount;

        apUI.SpendAPUI(CurrentAP);
        yield return null;
    }
    private IEnumerator RefillAPGAPerformer(RefillAPGA refillAPGA)
    {
        CurrentAP = Mathf.Min(CurrentAP + refillAPGA.Amount, MaxAP);

        apUI.RefillAPUI(CurrentAP);
        yield return null;
    }

    //Reactions
    private void TurnGAPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type == TurnType.StartBattle)
            ActionSystem.Instance.AddReaction(new RefillAPGA(StartBattleAP));
        else if (turnGA.Type == TurnType.Player)
            ActionSystem.Instance.AddReaction(new RefillAPGA(TurnStartAP));
    }
}
