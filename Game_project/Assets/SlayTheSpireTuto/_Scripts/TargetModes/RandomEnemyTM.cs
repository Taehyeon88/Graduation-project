using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEnemyTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        EnemyView target = EnemySystem.Instance.Enemise[Random.Range(0, EnemySystem.Instance.Enemise.Count)];
        return new() { target };
    }
}
