using UnityEngine;
using UnityEngine.UI;

public class DrawCardUI : MonoBehaviour
{
    [SerializeField] private int drawAmount = 1;   //버튼 1회당 드로우 장수
    [SerializeField] private int drawAPCost = 1;   //버튼 1회당 AP 소모

    private Button button;
    private int freeDrawsLeft;   //공용 트리 보급 상자: 이번 전투에 남은 무료 뽑기 (씬이 전투마다 새로 로드돼 전투당 초기화)

    private void OnEnable()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(DrawCard);
        }
    }

    private void Start()
    {
        freeDrawsLeft = GameSystem.Instance.GetGrowthBonus(GrowthNodeType.FREE_DRAW);
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
        if (!TutorialSystem.AllowsDraw()) return;
        bool isFree = freeDrawsLeft > 0;
        if (!isFree && !APSystem.Instance.HasEnoughAP(drawAPCost)) return;
        if (CardSystem.Instance.DrawPileCount <= 0) return;

        DrawCardsGA drawCardsGA = new(drawAmount);
        if (isFree) freeDrawsLeft--;
        else drawCardsGA.PreReactions.Add((new SpendAPGA(drawAPCost), null));
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
