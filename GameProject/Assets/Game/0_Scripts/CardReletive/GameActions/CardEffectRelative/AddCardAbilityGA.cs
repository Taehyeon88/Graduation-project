
public class AddCardAbilityGA : GameAction
{
    public CardAbilityType CardAbilityType { get; private set; }

    public AddCardAbilityGA(CardAbilityType cardAbilityType)
    {
        CardAbilityType = cardAbilityType;
    }
}
