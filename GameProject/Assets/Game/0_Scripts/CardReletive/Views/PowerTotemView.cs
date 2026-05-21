using IsoTools;
using UnityEngine;

public class PowerTotemView : CombatantView
{
    public string Name { get; private set; }
    public Sprite Sprite { get; private set; }
    public int Distance { get; private set; }
    public void SetUp(PowerTotemData powerTotemData)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        Name = powerTotemData.Name;
        Sprite = powerTotemData.Sprite;
        Distance = powerTotemData.Distance;
        SetUpBase(powerTotemData.Health, powerTotemData, isObject);
        TokenModel.Sprite = powerTotemData.Sprite;
    }

    public void OnEnable()
    {
        //조건 + 행동 체인
        //조건 : 적 공격 = AttackEnemyGA
        //행동 : 공격력 1 상승 처리

        ActionSystem.SubscribeReaction<AttackEnemyGA>(AttackEnemyGAPreReaction, ReactionTiming.PRE);
    }

    public void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<AttackEnemyGA>(AttackEnemyGAPreReaction, ReactionTiming.PRE);
    }

    public void AttackEnemyGAPreReaction(AttackEnemyGA attackEnemyGA)
    {
        if (attackEnemyGA.IsTotemAttack) return;

        var distance = TokenSystem.Instance.GetDistance(this, HeroSystem.Instance.HeroView);

        if (distance <= Distance)
        {
            attackEnemyGA.Amount += 1;
        }
    }
}
