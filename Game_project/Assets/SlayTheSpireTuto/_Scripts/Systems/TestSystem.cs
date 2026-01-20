using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSystem : MonoBehaviour
{
    [SerializeField] private HandView handView;
    [SerializeField] private CardData cardData;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Card card = new(cardData);
            CardView cardView = CardViewCreator.Instance.CreatCardView(card, transform.position, Quaternion.identity);
            StartCoroutine(handView.AddCard(cardView));
        }
    }
}
