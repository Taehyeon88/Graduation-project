using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardViewInPile : MonoBehaviour
{
    public Card Card { get; private set; }

    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text apCost;
    [SerializeField] private Image image;

    public void SetUp(Card card)
    {
        Card = card;
        title.text = card.Title;
        apCost.text = card.APCost.ToString();
        image.sprite = card.Image;
        image.preserveAspect = true;   //틀은 1:1 고정, 스프라이트 비율 유지
    }
}
