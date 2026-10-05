using System.Collections.Generic;
using UnityEngine;

public abstract class CombatantData : TokenData   // 영웅·몬스터 공용 전투 데이터
{
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int Damage { get; private set; }
    [field: SerializeField] public int Speed { get; private set; }
    [field: SerializeField] public List<PerkData> Perks { get; private set; }
}
