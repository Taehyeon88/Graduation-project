using UnityEngine;

public class PlacePowerTotemEffect : Effect
{
    [SerializeField] private PowerTotemData powerTotemData;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new PlacePowerTotemGA(powerTotemData, effectInfo.targetPoses);
    }
}
