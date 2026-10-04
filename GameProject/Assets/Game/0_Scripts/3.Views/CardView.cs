using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerClickHandler
{
    public Card Card { get; private set; }

    [SerializeField] private TMP_Text title;
    [FormerlySerializedAs("mana")]
    [SerializeField] private TMP_Text apCost;
    [SerializeField] private Image image;

    [Header("Select Direct Element")]
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private float selectDuration = 0.1f;

    private void OnDestroy()
    {
        transform.DOKill();
    }

    //Publics
    public void SetUp(Card card)
    {
        Card = card;
        title.text = card.Title;
        apCost.text = card.APCost.ToString();
        image.sprite = card.Image;
    }

    public void SetSelected(bool selected)
    {
        transform.DOScale(selected ? selectedScale : 1f, selectDuration);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ActionSystem.Instance.IsPerforming
            || Interactions.Instance.IsSkillTargetMode
            || Interactions.Instance.IsHeroMoveMode)
            return;

        if (APSystem.Instance.HasEnoughAP(Card.APCost))
        {
            SoundSystem.Instance.PlaySound(3004);        //선택 사운드
            CardSystem.Instance.PlayCardTargetMode(this);
        }
        else
        {
            SoundSystem.Instance.PlaySound(3006);        //코스트 부족 사운드
        }
    }
}
