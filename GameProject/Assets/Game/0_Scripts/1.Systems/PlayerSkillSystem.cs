using UnityEngine;

public class PlayerSkillSystem : Singleton<PlayerSkillSystem>
{
    public const int APCost = 1;   //모든 플레이어 스킬 공통 AP 소모

    [SerializeField] private PlayerSkillData skillData;   //플레이어 스킬은 1개만 존재

    public PlayerSkillData SkillData => skillData;
    public int RemainingUses { get; private set; }
    private bool usedThisTurn;   //플레이어 스킬은 한 턴에 1번만

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }

    public bool CanUse()
    {
        return TurnSystem.Instance.CurrentTurn == TurnType.Player
            && !ActionSystem.Instance.IsPerforming
            && !usedThisTurn
            && RemainingUses > 0
            && APSystem.Instance.HasEnoughAP(APCost);
    }

    //대상이 유효할 때만 AP·횟수를 소모하고 스킬 실행
    public bool TryUse(Token target)
    {
        if (!CanUse() || !skillData.Skill.IsValidTarget(target)) return false;

        GameAction skillAction = skillData.Skill.CreateAction(target);
        skillAction.PreReactions.Add((new SpendAPGA(APCost), null));

        RemainingUses--;
        usedThisTurn = true;
        ActionSystem.Instance.Perform(skillAction);
        return true;
    }

    //Reactions
    private void TurnGAPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type == TurnType.StartBattle)
        {
            RemainingUses = skillData.MaxUses;
            usedThisTurn = false;
        }
        else if (turnGA.Type == TurnType.Player)
        {
            usedThisTurn = false;
        }
    }
}
