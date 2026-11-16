using UnityEngine;

//★3 템플릿: 특정 영웅 없이 승리 (시작 덱에 그 영웅이 없어야 함)
public class WithoutHeroStarCondition : StarCondition
{
    [SerializeField] private HeroData excludedHero;

    public override string Description => $"{(excludedHero != null ? excludedHero.Name : "?")} 없이 승리";

    public override bool IsMet(StageResult result)
    {
        foreach (Hero hero in result.DeckHeroes)
            if (hero.Data == excludedHero) return false;
        return true;
    }
}
