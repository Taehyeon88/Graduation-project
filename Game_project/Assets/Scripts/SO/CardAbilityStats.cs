using UnityEngine;
using System.Collections.Generic;
using TableForge.DataStructures;
using TableForge.Attributes;

public enum CardAbilityType
{
    Effect, InGameLogic, Attack
}

namespace GameData.SO
{
    [CreateAssetMenu(fileName = "CardAbilityStats", menuName = "ScriptableObject/CardAbility Stats")]
    public class CardAbilityStats : ScriptableObject
    {
        public int cardAbilityId;
        public string descriptionForDeveloper;
    }
}