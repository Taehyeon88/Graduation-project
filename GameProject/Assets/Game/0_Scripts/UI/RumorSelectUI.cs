using UnityEngine;
using UnityEngine.UI;

public class RumorSelectUI : MonoBehaviour   //소문 3슬롯 중 하나를 고르고 하단 중앙 확인 버튼으로 확정
{
    [SerializeField] private GameObject panel;
    [SerializeField] private RumorSlotUI[] slots;   //인스펙터 고정 3개
    [SerializeField] private RumorData[] rumors;      //slots와 같은 순서, 비어 있는 칸은 슬롯 비활성
    [SerializeField] private Button confirmButton;

    private RumorData selected;

    private void OnEnable()
    {
        confirmButton.onClick.AddListener(Confirm);
        foreach (RumorSlotUI slot in slots)
            slot.Clicked += Select;
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(Confirm);
        foreach (RumorSlotUI slot in slots)
            slot.Clicked -= Select;
    }

    //Publics
    public void Open()
    {
        selected = null;
        confirmButton.interactable = false;

        for (int i = 0; i < slots.Length; i++)
        {
            RumorData rumor = i < rumors.Length ? rumors[i] : null;
            slots[i].gameObject.SetActive(rumor != null);
            if (rumor != null)
                slots[i].SetUp(rumor);
            slots[i].SetSelected(false);
        }
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    //Privates
    private void Select(RumorSlotUI clicked)
    {
        selected = clicked.Rumor;
        foreach (RumorSlotUI slot in slots)
            slot.SetSelected(slot == clicked);
        confirmButton.interactable = true;
    }

    private void Confirm()
    {
        TavernSystem.Instance.ConfirmRumor(selected);
    }
}
