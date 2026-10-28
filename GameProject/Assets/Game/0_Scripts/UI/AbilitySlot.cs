using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AbilitySlot   //정보 패널의 특성·소문 공용 슬롯 (비면 Null_Text만 켠다)
{
    [SerializeField] private Image icon;   //소문 슬롯은 비워 둔다
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text nullText;

    public void SetUp(string name, string desc, Sprite sprite)
    {
        if (icon != null)
        {
            icon.gameObject.SetActive(true);
            icon.sprite = sprite;
            icon.enabled = sprite != null;   //이미지가 없는 항목은 흰 사각형을 숨김
        }
        nameText.gameObject.SetActive(true);
        description.gameObject.SetActive(true);
        nullText.gameObject.SetActive(false);
        nameText.text = RichTextFormatter.Format(name);
        description.text = RichTextFormatter.Format(desc);
    }

    public void Clear()
    {
        if (icon != null) icon.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);
        description.gameObject.SetActive(false);
        nullText.gameObject.SetActive(true);
    }
}
