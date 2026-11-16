using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button endGameButton;
    private void OnEnable()
    {
        restartButton.onClick.AddListener(RestartGame);
        endGameButton.onClick.AddListener(EndGame);
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(RestartGame);
        endGameButton.onClick.RemoveListener(EndGame);
    }

    private void RestartGame()  //같은 스테이지·같은 선택으로 재도전
    {
        GameSystem.Instance.RetryStage();
    }

    private void EndGame()  //스테이지 선택 화면으로 돌아가기
    {
        GameSystem.Instance.ReturnToStageSelect();
    }
}
