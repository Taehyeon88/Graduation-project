using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSystem : Singleton<HeroSystem>
{
    public Image selected_Hero_UI;
    public IReadOnlyList<HeroView> HeroViews => TokenSystem.Instance.HeroViews;
    public HeroView CurrentHero
    {
        get { return currentHero; }
        set
        {
            if (value == null 
                || currentHero == value) return;

            currentHero = value;

            if (selected_Hero_UI != null)
            {
                if (!selected_Hero_UI.gameObject.activeSelf)
                    selected_Hero_UI.gameObject.SetActive(true);
                selected_Hero_UI.sprite = (currentHero.TokenData as HeroData).simbolIcon; //선택된 영웅 이미지 변경
            }
        }
    }

    private HeroView currentHero;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPreReaction, ReactionTiming.PRE);
    }

    //Publics
    public IEnumerator PlayHeroTurnPerformer()
    {
        Debug.Log("플레이어 턴 시작");

        foreach (var hero in HeroViews)
        {
            hero.ReduceSEWhenMyTurnStart(); //공용 시작시, SE 제거
        }

        //행동 포인트(AP) 회복
        RefillAPGA refillAPGA = new();
        ActionSystem.Instance.AddReaction(refillAPGA);

        yield return null;
    }

    private void AutoBattleTurnPreReaction(AutoBattleTurnGA turnGA)
    {

        Debug.Log("플레이어 턴 종료");

        foreach (var hero in HeroViews)
        {
            hero.ReduceSEWhenMyTurnEnd();  //공용 종료시, SE 제거
        }
    }
}
