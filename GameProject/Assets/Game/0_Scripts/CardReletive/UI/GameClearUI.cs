using UnityEngine;
using UnityEngine.UI;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private Transform gameClearUITrans;
    [SerializeField] private Transform rewardTransform;
    [SerializeField] private Button gotoNextLevelButton;

    private CardViewInPile[] currentCards;

    private void OnEnable()
    {
        if(!GameSystem.Instance.IsGameClear)
            gameClearUITrans.gameObject.SetActive(false);
        gotoNextLevelButton.onClick.AddListener(GoToNextLevel);
    }
    private void OnDisable()
    {
        gotoNextLevelButton.onClick.RemoveListener(GoToNextLevel);
    }

    public void UpdateRewardCards(Card[] cards)
    {
        gameClearUITrans.gameObject.SetActive(true);
        currentCards = new CardViewInPile[cards.Length];

        int index = 0;
        foreach (Card card in cards)
        {
            CardViewInPile cardView = CardViewCreator.Instance.CreatCardViewInPile(card, null);
            cardView.transform.SetParent(rewardTransform);
            cardView.transform.localScale = Vector3.one;

            currentCards[index] = cardView;
            index++;
        }
    }

    public void RemoveRewardCards()
    {
        foreach (var card in currentCards)
        {
            Destroy(card.gameObject);
        }
    }

    public void GoToNextLevel()
    {
        GameSystem.Instance.GoToNextLevel();
    }
}
