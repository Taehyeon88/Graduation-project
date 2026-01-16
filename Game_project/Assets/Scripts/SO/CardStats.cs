using UnityEngine;
using System.Collections.Generic;
using TableForge.DataStructures;
using TableForge.Attributes;

public enum cardType
{
    Attack, Skill, Power
}
public enum cardEffect
{
    None, Exhaust, Ethereal, Critical, Copy, Congenital  //소멸, 휘발성, (치명타, 복사), 선천성
}

namespace GameData.SO
{
    [CreateAssetMenu(fileName = "CardStats", menuName = "ScriptableObject/Card Stats")]
    public class CardStats : ScriptableObject
    {
        public string cardName;
        public string desForDev;
        public int cost;
        public cardType cardType;
        public cardEffect cardEffect;
        public List<Ability> abilities;
        public Sprite cardImage;
        public string description;

        [System.Serializable]
        public class Ability
        {
            public int cardAblityId;
            public cardAblityInfo abilityInfo;
        }
    }
}

[System.Serializable]
public struct cardAblityInfo
{
    public float one, two, three;
}