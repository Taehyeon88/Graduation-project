using Newtonsoft.Json.Bson;
using UnityEngine;

public class TooltipSystem : Singleton<TooltipSystem>
{
    [SerializeField] private TooltipUI tooltip;
    [SerializeField] private TooltipUI skill_Tooltip;
    public void Show(RectTransform rectTrans, string description, string title = "", bool isTracking = false)
    {
        if (tooltip == null) return;   //씬 정리 중 파괴된 UI 접근 방지

        tooltip.SetMode(isTracking, rectTrans.position);
        tooltip.SetText(description, title);
        tooltip.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (tooltip == null) return;

        tooltip.gameObject.SetActive(false);
    }

    public void ShowSkillTooltip(RectTransform rectTrans, string description, string title = "")
    {
        if (skill_Tooltip == null) return;

        skill_Tooltip.SetMode(false, rectTrans.position);
        skill_Tooltip.SetText(description, title);
        skill_Tooltip.gameObject.SetActive(true);
    }

    public void HideSkillTooltip()
    {
        if (skill_Tooltip == null) return;

        skill_Tooltip.gameObject.SetActive(false);
    }
}
