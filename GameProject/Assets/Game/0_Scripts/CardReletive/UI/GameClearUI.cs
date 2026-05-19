using UnityEngine;
using UnityEngine.UI;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private Transform gameClearUITrans;
    [SerializeField] private Transform rewardTransform;
    [SerializeField] private Button gotoNextLevelButton;

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

    }

    public void RemoveRewardCards()
    {

    }

    public void GoToNextLevel()
    {
        GameSystem.Instance.GoToNextLevel();
    }
}
