using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystem : Singleton<UISystem>
{
    [SerializeField] private CombatantViewStatusUI combatantViewStatusUI;
    [SerializeField] private PileofCardUI pileofCardUI;


    /// <summary>
    /// 영웅 혹은 몬스터들 상태효과UI 갱신 함수
    /// </summary>
    /// <param name="combatantView"></param>
    /// <param name="statusEffectType"></param>
    /// <param name="stackCount"></param>
    /// <param name="sprite"></param>
    public void UpdateStatusEffectUI(CombatantView combatantView, StatusEffectType statusEffectType, int stackCount, Sprite sprite = null)
    {
        combatantViewStatusUI.UpdateStatusEffect(combatantView, statusEffectType, stackCount, sprite);
    }

    public void UpdatePerkUI(Perk perk, bool isAdd)
    {
        if (isAdd) combatantViewStatusUI.AddPerkUI(perk);
        else combatantViewStatusUI.RemovePerkUI(perk);
    }

    public void SetPileofCardUI(bool isDraw, bool active, bool isAbility = false)
    {
        if (isDraw) pileofCardUI.SetDrawPileUI(active, isAbility);
        else pileofCardUI.SetDiscardPileUI(active, isAbility);
    }
    public void OffPileofCardUI()
    {
        pileofCardUI.OffPileofCardUI();
    }
}
