using UnityEngine;
using UnityEngine.UI;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;        //승리 결과 패널 (확인 후 닫힘, 입력 차단막은 유지)
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject exitButton;   //보상 선택 단계에서만 표시, 누르면 다음 스테이지 선택(StageScene)으로 이동

    private void OnEnable()
    {
        panel.SetActive(true);
        exitButton.SetActive(false);
        confirmButton.onClick.AddListener(Confirm);
        exitButton.GetComponent<Button>().onClick.AddListener(Exit);
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(Confirm);
        exitButton.GetComponent<Button>().onClick.RemoveListener(Exit);
    }

    private void Exit()   //다음 스테이지로 진행하고 스테이지 선택 씬으로 이동
    {
        GameSystem.Instance.GoToNextLevel();
    }

    private void Confirm()   //결과 패널을 닫고 보상 용병 선택 시작
    {
        panel.SetActive(false);
        exitButton.SetActive(true);
        RewardSelectSystem.Instance.BeginSelect();
    }
}
