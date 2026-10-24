using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    private const int PercentMax = 100;

    public IReadOnlyList<HeroView> HeroViews => TokenSystem.Instance.HeroViews;
    public HeroCaptainView HeroCaptain { get; set; }   //용병대장 (TokenSetup만 설정)
    public HeroView CurrentHero
    {
        get { return currentHero; }
        set
        {
            if (value == null 
                || currentHero == value) return;

            currentHero = value;
        }
    }

    public IReadOnlyList<Vector2Int> SetupPoses => setupPoses;   //영웅 배치 가능 위치 (라인당 1개, 전투 중 불변)

    private HeroView currentHero;
    private List<Vector2Int> setupPoses = new();

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnPostReaction, ReactionTiming.POST);
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
        yield return null;
    }

    private void AutoBattleTurnPreReaction(AutoBattleTurnGA turnGA)
    {
        foreach (var hero in HeroViews)
        {
            hero.ReduceSEWhenMyTurnEnd();  //공용 종료시, SE 제거
        }
    }

    //자동 전투 종료 시, 영웅마다 치명타 확률(%)로 치명타 상태효과 획득
    private void AutoBattleTurnPostReaction(AutoBattleTurnGA turnGA)
    {
        foreach (var hero in HeroViews)
        {
            if (hero == null || hero.TokenData is not HeroData data) continue;
            if (Random.Range(0, PercentMax) >= data.Critical) continue;

            ActionSystem.Instance.AddReaction(new AddStatusEffectGA(StatusEffectType.CRITICAL, 1, new() { hero }, hero));
        }
    }
}
