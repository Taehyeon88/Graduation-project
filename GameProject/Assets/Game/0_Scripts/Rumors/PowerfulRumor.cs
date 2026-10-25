using UnityEngine;

//1 강력한: 공격력을 얻고, 배치 시 소모되는 AP가 늘어난다
public class PowerfulRumor : Rumor, IAPCostModifier
{
    [SerializeField] private int attackBonus = 3;
    [SerializeField] private int apCostBonus = 1;

    public int APCostBonus => apCostBonus;

    public override void OnAdd(HeroView owner)
    {
        owner.ModifyStat(StatType.DAMAGE, attackBonus);
    }
}
