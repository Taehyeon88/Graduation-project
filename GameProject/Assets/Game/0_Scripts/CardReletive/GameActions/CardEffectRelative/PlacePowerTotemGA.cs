using UnityEngine;
using System.Collections.Generic;

public class PlacePowerTotemGA : GameAction
{
    public PowerTotemData PowerTotemData { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }

    public PlacePowerTotemGA(PowerTotemData powerTotemData, List<Vector2Int> targetPoses)
    {
        PowerTotemData = powerTotemData;
        TargetPoses = targetPoses;
    }
}
