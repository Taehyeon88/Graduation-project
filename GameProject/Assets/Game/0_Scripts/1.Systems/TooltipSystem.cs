using Newtonsoft.Json.Bson;
using UnityEngine;

public class TooltipSystem : Singleton<TooltipSystem>
{
    [SerializeField] private TooltipUI tooltip;
    [SerializeField] private TooltipUI skill_Tooltip;
    public void Show(RectTransform rectTrans, string description, string title = "", bool isTracking = false)
    {
        tooltip.SetMode(isTracking, rectTrans.position);
        tooltip.SetText(description, title);
        tooltip.gameObject.SetActive(true);
    }

    public void Hide()
    {
        tooltip.gameObject.SetActive(false);
    }

    public void ShowSkillTooltip(RectTransform rectTrans, string description, string title = "")
    {
        skill_Tooltip.SetMode(false, rectTrans.position);
        skill_Tooltip.SetText(description, title);
        skill_Tooltip.gameObject.SetActive(true);
    }

    public void HideSkillTooltip()
    {
        skill_Tooltip.gameObject.SetActive(false);
    }
}
