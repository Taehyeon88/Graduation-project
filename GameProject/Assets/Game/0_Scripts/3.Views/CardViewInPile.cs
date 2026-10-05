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
    }
}
