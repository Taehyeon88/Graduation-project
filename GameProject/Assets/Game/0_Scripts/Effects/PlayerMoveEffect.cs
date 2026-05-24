using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMoveEffect : Effect
{
    [SerializeField]private int distance;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        PlayerMoveGA playerMoveGA = new(distance, effectInfo.targetPoses);
        return playerMoveGA;
    }
}
