//★3 템플릿: 기본 제공 카드만으로 승리
//진입하려면 카드를 1장 이상 골라야 하므로 "덱"이 아니라 "이번 판에 배치한 카드"가 전부 기본 제공 카드인지로 판정한다
public class BaseHeroesOnlyStarCondition : StarCondition
{
    public override string Description => "기본 제공 카드만 배치해 승리";

    public override bool IsMet(StageResult result)
    {
        foreach (Hero hero in result.PlacedHeroes)
        {
            bool isBase = false;
            foreach (Hero baseHero in result.BaseHeroes)
            {
                if (baseHero != hero) continue;
                isBase = true;
                break;
            }
            if (!isBase) return false;
        }
        return true;
    }
}
