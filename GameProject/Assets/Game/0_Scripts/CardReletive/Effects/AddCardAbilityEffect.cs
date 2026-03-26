using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCardAbilityEffect : Effect
{
    [SerializeField] private CardAbilityType cardAbilityType;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new AddCardAbilityGA(cardAbilityType);
    }
}
