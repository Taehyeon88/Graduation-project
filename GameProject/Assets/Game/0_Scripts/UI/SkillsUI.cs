using DG.Tweening;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    public IReadOnlyCollection<SkillView> SkillViews => skillViews;

    [SerializeField] private RectTransform[] skills;

    private SkillView[] skillViews;
    private int active_Count;

    private const float tween_Time = 0.1f;
    private const float tween_delay_Time = 0.08f;

    private void OnEnable()
    {
        Interactions.SetPlaySkillEvent(ListenSkillClick, true);
    }
    private void OnDisable()
    {
        Interactions.SetPlaySkillEvent(ListenSkillClick, false);
    }
    private void Start()
    {
        skillViews = new SkillView[skills.Length];
        active_Count = 0;

        for (int i = 0; i < skills.Length; i++)
        {
            skillViews[i] = skills[i].GetComponent<SkillView>();
            skills[i].localScale = Vector3.zero;
        }
    }

    public void UpdateSkils(HeroView heroView)
    {
        int skill_Count = heroView.Skills.Count;

        //SkilㅣView에 Skill 데이터 셋업
        for (int i = 0; i < skillViews.Length; i++)
        {
            if (i < skill_Count)
            {
                Skill skill = heroView.Skills[i];
                skillViews[i].SetUp(skill);
            }
            else
            {
                skillViews[i].SetUp(null);
            }
        }

        //Skill 재정렬 연출
        if (active_Count < skill_Count)
        {
            Sequence squ = DOTween.Sequence();
            float timing = 0.0f;

            for (int i = active_Count; i < skill_Count; i++)
            {
                int index = i;
                skillViews[index].CancelInteraction = true;  //클릭 방지
                squ.Insert(timing, skills[index].DOScale(Vector3.one, tween_Time)
                                            .OnComplete(() => skillViews[index].CancelInteraction = false)
                                       );
                timing += index == active_Count? tween_delay_Time : tween_Time;

                active_Count++;
            }

        }
        else if (active_Count > heroView.Skills.Count)
        {
            Sequence squ = DOTween.Sequence();
            float timing = 0.0f;

            for (int i = active_Count - 1; i >= skill_Count; i--)
            {
                int index = i;
                skillViews[index].CancelInteraction = true;
                squ.Insert(timing, skills[index].DOScale(Vector3.zero, tween_Time)
                                            .OnComplete(() => skillViews[index].CancelInteraction = false)
                                       );
                timing += index == active_Count ? tween_delay_Time : tween_Time;
                active_Count--;
            }
        }
    }

    private void ListenSkillClick(int number)
    {
        if (active_Count < number) return;

        skillViews[number - 1].OnPointerClick(new PointerEventData(EventSystem.current));
    }
}
