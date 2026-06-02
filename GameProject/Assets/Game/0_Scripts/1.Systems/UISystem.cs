using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystem : Singleton<UISystem>
{
    [SerializeField] private CombatantViewStatusUI combatantViewStatusUI;
    [SerializeField] private PileofCardUI pileofCardUI;
    [SerializeField] private GameClearUI gameClearUI;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private DemoUI endDemoUI;


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

    //카드 보상UI용 (게임 클리어)
    public void UpdateRewardCards(Card[] cards)
    {
        gameClearUI.UpdateRewardCards(cards);
    }
    public void RemoveRewardCards()
    {
        gameClearUI.RemoveRewardCards();
    }

    //게임 종료
    public void OnGameOverUI()
    {
        gameOverUI.gameObject.SetActive(true);
    }

    //데모 종료
    public void EndDemoUI()
    {
        endDemoUI.gameObject.SetActive(true);
    }
}
