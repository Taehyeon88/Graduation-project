using UnityEngine;
using UnityEngine.UI;

public class DrawCardUI : MonoBehaviour
{
    private const int DrawAmount = 1;   //버튼 1회당 드로우 장수
    private const int DrawAPCost = 1;   //버튼 1회당 AP 소모

    private Button button;

    private void OnEnable()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(DrawCard);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(DrawCard);
        }
    }

    private void DrawCard()
    {
        //플레이어 턴에 AP가 있고 뽑을 카드가 남아 있을 때만 (카드를 못 뽑으면 AP도 쓰지 않음)
        if (TurnSystem.Instance.CurrentTurn != TurnType.Player) return;
        if (!APSystem.Instance.HasEnoughAP(DrawAPCost)) return;
        if (CardSystem.Instance.DrawPileCount <= 0) return;

        DrawCardsGA drawCardsGA = new(DrawAmount);
        drawCardsGA.PreReactions.Add((new SpendAPGA(DrawAPCost), null));
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
