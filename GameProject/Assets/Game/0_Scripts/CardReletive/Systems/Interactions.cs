using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool lockInteraction { get; set; } = false;
    public bool PlayerIsDraging { get; set; } = false;
    public bool PlayerIsTargeting { get; set; } = false;
    public bool PlayerCanDraging()           //드래그 가능 <- 수행하는 액션 없음, 타겟 모드X
    {
        if(ActionSystem.Instance.IsPerforming 
            || PlayerIsTargeting
            || lockInteraction) return false;
        else return true;
    }
    public bool PlayerCanHover()           //호버 가능 <- 타겟 모드X, 드래그X
    {
        if (PlayerIsTargeting 
            || PlayerIsDraging 
            || lockInteraction) return false;
        else return true;
    }
    public bool PlayerCanTargeting()       //타겟 모드 가능 <- 드래그X, 수행하는 액션 없음
    {
        if (ActionSystem.Instance.IsPerforming
            || PlayerIsDraging 
            || lockInteraction) return false;
        else return true;
    }
}
