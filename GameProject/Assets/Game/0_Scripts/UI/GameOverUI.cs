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

    private void RestartGame()  //첫 스테이지부터 시작
    {
        GameSystem.Instance.StartFromScratch();
    }

    private void EndGame()  //시작 화면으로 돌아가기
    {
        SceneManager.LoadScene("StartScene");
    }
}
