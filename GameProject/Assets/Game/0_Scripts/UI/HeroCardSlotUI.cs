using System;
using UnityEngine;
using UnityEngine.UI;

public class HeroCardSlotUI : MonoBehaviour   //정보 팝업의 영웅 카드 한 장 — 카드 표시(CardViewInPile) + 선택 토글
{
    [SerializeField] private CardViewInPile cardView;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedMark;   //선택됨 표시
    [SerializeField] private GameObject lockedMark;     //기본 제공(고정) 또는 선택 불가 표시

    public Hero Hero { get; private set; }

    private Action<HeroCardSlotUI> clicked;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
    }

    //Publics
    public void SetUp(Hero hero, bool isLocked, Action<HeroCardSlotUI> onClick)
    {
        Hero = hero;
        clicked = onClick;
        cardView.SetUp(new Card(hero));
        lockedMark.SetActive(isLocked);
        button.interactable = !isLocked;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        selectedMark.SetActive(selected);
    }

    //Privates
    private void OnClick()
    {
        clicked?.Invoke(this);
    }
}
