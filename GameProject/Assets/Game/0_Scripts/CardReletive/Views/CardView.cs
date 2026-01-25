using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private Image image;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;

    private RectTransform rectTransform;
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    public Card card { get; private set; }
    public void SetUp(Card card)
    {
        this.card = card;
        title.text = card.Title;
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        image.sprite = card.Image;
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        wrapper.SetActive(false);
        Vector2 pos = rectTransform.anchoredPosition + Vector2.up * 120f;
        CardViewHoverSystem.Instance.Show(card, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (card.ManualTargetEffects != null && card.ManualTargetEffects.Count > 0)
        {
            ManualTargetSystem.Instance.StartTargeting(transform.position);
        }
        else
        {
            Interactions.Instance.PlayerIsDraging = true;
            wrapper.SetActive(true);
            CardViewHoverSystem.Instance.Hide();
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.position = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (card.ManualTargetEffects != null && card.ManualTargetEffects.Count > 0) return;
        transform.position = eventData.position;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (card.ManualTargetEffects != null && card.ManualTargetEffects.Count > 0)
        {
            EnemyView target = ManualTargetSystem.Instance.EndTargeting(eventData.position);
            if (target != null && ManaSystem.Instance.HasEnoughMana(card.Mana))
            {
                PlayCardGA playCardGA = new(card, target);
                ActionSystem.Instance.Perform(playCardGA);
            }
        }
        else
        {
            if (ManaSystem.Instance.HasEnoughMana(card.Mana)
            && Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hitInfo, 10f, dropLayer))
            {
                PlayCardGA playCardGA = new(card);
                ActionSystem.Instance.Perform(playCardGA);
            }
            else
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }
            Interactions.Instance.PlayerIsDraging = false;
        }
    }
}
