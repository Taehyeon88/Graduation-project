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

    public IReadOnlyList<Vector2Int> NeutralSetupPoses => neutralSetupPoses;   //중립 배치 소문을 가진 영웅만 쓸 수 있는 배치 위치 (기본 배치 열의 중립지대)

    private HeroView currentHero;
    private List<Vector2Int> setupPoses = new();
    private List<Vector2Int> neutralSetupPoses = new();

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
    public void SetUp(List<Vector2Int> poses, List<Vector2Int> neutralPoses = null)
    {
        setupPoses = poses;
        neutralSetupPoses = neutralPoses ?? new();
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

    //자동 전투 종료 시, 영웅의 기절 1 감소 후 영웅마다 치명타 확률(%)로 치명타 상태효과 획득
    private void AutoBattleTurnPostReaction(AutoBattleTurnGA turnGA)
    {
        foreach (var hero in HeroViews)
        {
            hero.RemoveStatusEffect(StatusEffectType.STUN, 1);
        }

        int criticalBonus = GameSystem.Instance.GetGrowthBonus(GrowthNodeType.HERO_CRITICAL);   //공용 트리 치명타 훈련 (덱 영웅에 저장하지 않고 판정에만 더함)
        foreach (var hero in HeroViews)
        {
            if (hero == null || hero.Hero == null) continue;
            if (!RollCritical(hero, criticalBonus)) continue;

            ActionSystem.Instance.AddReaction(new AddStatusEffectGA(StatusEffectType.CRITICAL, 1, new() { hero }, hero));
        }
    }

    //치명타 판정. 튜토리얼의 지정 라운드에는 지정 영웅만 100%, 나머지는 0%로 고정
    private bool RollCritical(HeroView hero, int criticalBonus)
    {
        TutorialBattleData tutorial = GameSystem.Instance.TutorialBattle;
        if (tutorial != null && TurnSystem.Instance.CurrentTurn_Number == tutorial.CriticalRound)
            return System.Array.IndexOf(tutorial.CriticalHeroes, hero.Hero.Data) >= 0;

        return Random.Range(0, PercentMax) < hero.Hero.Critical + criticalBonus;
    }
}
