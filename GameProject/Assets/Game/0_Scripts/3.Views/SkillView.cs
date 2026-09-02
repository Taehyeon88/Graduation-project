using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Skill Skill { get; private set; }
    public Image[] Images { get; private set; }
    public bool CancelInteraction { get; set; }

    [Header("Skill Element")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text limit_Text;

    private RectTransform skill_Rect;
    private bool is_Cannot_Use = true;
    public void SetUp(Skill skill)
    {
        if (skill == null) return;

        Skill = skill;
        icon.sprite = skill.Image;
        limit_Text.SetText(skill.Limit.ToString());

        skill_Rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Skill == null || Interactions.Instance.IsSkillTargetMode)
            return;

        //스킬 사용 불가 여부 연출
        if (ManaSystem.Instance.HasEnoughMana() && Skill.HasEnoughLimit())
        {
            if (is_Cannot_Use)
            {
                icon.color = Color.HSVToRGB(0.0f, 0.0f, 100.0f);   //스킬 사용 불가 처리 취소
                is_Cannot_Use = false;
            }
        }
        else
        {
            if (!is_Cannot_Use)
            {
                icon.color = Color.HSVToRGB(0.0f, 0.0f, 50.0f);   //스킬 사용 불가 처리
                is_Cannot_Use = true;
            }
        }

        limit_Text.SetText(Skill.Limit.ToString());   //사용 횟수 실시간 카운트는 자동
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CancelInteraction || Skill == null) return;

        if (ManaSystem.Instance.HasEnoughMana() && Skill.HasEnoughLimit())
        {
            OnPointerExit(eventData);  //호버 종료 예외처리

            SoundSystem.Instance.PlaySound(18);    //스킬 선택 사운드
            SkillSystem.Instance.PlaySkillTargetMode(this);  //선택 모드 시작 / 스킬 전환 / 종료
        }
        else
        {
            //코스트 부족 사운드 재생
            SoundSystem.Instance.PlaySound(21);

            //코스트 부족으로 사용불가 연출
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactions.Instance.CanSkillHovering() || CancelInteraction) return;

        Interactions.Instance.IsSkillHovering = true;

        SoundSystem.Instance.PlaySound(17);        //스킬 호버 사운드 재생
        TooltipSystem.Instance.Show(skill_Rect, Skill.Description, Skill.Title); //스킬 툴팁 팝업
        SkillSystem.Instance.HighlightUI.Show(skill_Rect.anchoredPosition);    //하이라이트 활성화
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactions.Instance.CanCancelSkilHovering()) return;

        Interactions.Instance.IsSkillHovering = false;

        TooltipSystem.Instance.Hide(); //스킬 툴팁 팝업
        SkillSystem.Instance.HighlightUI.Hide();     //하이라이트 비 활성화
    }
}
