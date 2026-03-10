using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool lockInteraction { get; set; } = false;
    public bool PlayerIsDraging { get; set; } = false;
    public bool PlayerCanInteract()
    {
        if(ActionSystem.Instance.IsPerforming
           || lockInteraction) return false;
        else return true;
    }
    public bool PlayerCanHover()
    {
        if (PlayerIsDraging || lockInteraction) return false;
        else return true;
    }
}
