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
                selected_Hero_UI.sprite = currentHero.TokenData.Sprite; //선택된 영웅 이미지 변경
            }
        }
    }

    public IReadOnlyList<Vector2Int> SetupPoses => setupPoses;   //영웅 배치 가능 위치 (라인당 1개, 전투 중 불변)

    private HeroView currentHero;
    private List<Vector2Int> setupPoses = new();

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPreReaction, ReactionTiming.PRE);
    }

    //Publics
    public void SetUp(List<Vector2Int> poses)
    {
        setupPoses = poses;
    }

    //해당 라인의 배치 위치 x (없으면 false)
    public bool TryGetSetupX(int line, out int x)
    {
        foreach (var pos in setupPoses)
        {
            if (pos.y != line) continue;
            x = pos.x;
            return true;
        }
        x = 0;
        return false;
    }

    public IEnumerator PlayHeroTurnPerformer()
    {
        foreach (var hero in HeroViews)
        {
            hero.ReduceSEWhenMyTurnStart(); //공용 시작시, SE 제거
        }

        yield return null;
    }

    private void AutoBattleTurnPreReaction(AutoBattleTurnGA turnGA)
    {
        foreach (var hero in HeroViews)
        {
            hero.ReduceSEWhenMyTurnEnd();  //공용 종료시, SE 제거
        }
    }
}
