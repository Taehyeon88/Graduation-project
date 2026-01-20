using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSystem : MonoBehaviour
{
    [SerializeField] private HandView handView;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CardView cardView = CardViewCreator.Instance.CreatCardView(transform.position, Quaternion.identity);
            StartCoroutine(handView.AddCard(cardView));
        }
    }
}
