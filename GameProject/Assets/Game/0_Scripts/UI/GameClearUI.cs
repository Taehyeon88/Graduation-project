using UnityEngine;
using UnityEngine.UI;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;        //승리 결과 패널 (확인 후 닫힘, 입력 차단막은 유지)
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject exitButton;   //보상 선택 단계에서만 표시 (동작은 추후 구현)

    private void OnEnable()
    {
        panel.SetActive(true);
        exitButton.SetActive(false);
        confirmButton.onClick.AddListener(Confirm);
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(Confirm);
    }

    private void Confirm()   //결과 패널을 닫고 보상 용병 선택 시작
    {
        panel.SetActive(false);
        exitButton.SetActive(true);
        RewardSelectSystem.Instance.BeginSelect();
    }
}
