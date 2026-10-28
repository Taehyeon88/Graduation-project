using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaptainDetailsUI : MonoBehaviour
{
    [Header("Token")]
    [SerializeField] private Image portrait;   //DiamondFrame/Masked_Content
    [SerializeField] private TMP_Text title;

    [Header("Stats (Amount)")]
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text speed;

    [Header("Ability")]
    [SerializeField] private AbilitySlot traitSlot;   //용병대장은 특성 1개만 사용

    //Publics
    public void Show(HeroCaptainView view)
    {
        portrait.sprite = view.TokenData.Sprite;
        title.text = RichTextFormatter.Format(view.TokenData.Name);

        attack.text = view.BaseDamage.ToString();
        health.text = $"{view.CurrentHealth}/{view.MaxHealth}";
        speed.text = "-";   //용병대장은 속도가 의미 없음

        if (view.Traits.Count > 0) traitSlot.SetUp(view.Traits[0].Title, view.Traits[0].Description, view.Traits[0].Image);
        else traitSlot.Clear();

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
