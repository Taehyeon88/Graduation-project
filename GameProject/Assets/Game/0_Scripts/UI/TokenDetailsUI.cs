using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenDetailsUI : MonoBehaviour
{
    [Header("Token")]
    [SerializeField] private Image portrait;   //DiamondFrame/Masked_Content
    [SerializeField] private TMP_Text title;

    [Header("Stats (Amount)")]
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text critical;

    [Header("Abilities")]
    [SerializeField] private AbilitySlot[] traitSlots;   //위에서부터 채움
    [SerializeField] private AbilitySlot rumorSlot;

    //Publics
    public void Show(CombatantView view)
    {
        portrait.sprite = view.TokenData.Sprite;
        title.text = view.TokenData.Name;

        attack.text = view.BaseDamage.ToString();
        health.text = $"{view.CurrentHealth}/{view.MaxHealth}";
        speed.text = view.Speed.ToString();
        Hero hero = (view as HeroView)?.Hero;
        critical.transform.parent.gameObject.SetActive(hero != null);   //몬스터는 치명타 데이터가 없어 슬롯 숨김
        if (hero != null) critical.text = $"{hero.Critical}%";

        IReadOnlyList<Trait> traits = view.Traits;
        for (int i = 0; i < traitSlots.Length; i++)
        {
            if (i < traits.Count) traitSlots[i].SetUp(traits[i].Title, traits[i].Description, traits[i].Image);
            else traitSlots[i].Clear();
        }
        if (hero?.Rumor != null) rumorSlot.SetUp(hero.Rumor.Title, hero.Rumor.Description, hero.Rumor.Image);
        else rumorSlot.Clear();

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
