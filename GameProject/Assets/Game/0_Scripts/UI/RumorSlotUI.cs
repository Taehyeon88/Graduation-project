using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RumorSlotUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;            //선택 표시용 배경
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.4f, 1f);

    public RumorData Rumor { get; private set; }

    public event Action<RumorSlotUI> Clicked;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void SetUp(RumorData rumor)
    {
        Rumor = rumor;
        nameText.text = rumor.Title;
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? selectedColor : normalColor;
    }

    private void OnClick()
    {
        Clicked?.Invoke(this);
    }
}
