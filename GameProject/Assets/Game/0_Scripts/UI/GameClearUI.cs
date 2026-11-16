using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;        //승리 결과 패널
    [SerializeField] private Button confirmButton;    //누르면 스테이지 선택(StageScene)으로 이동

    [Header("별 결과")]
    [SerializeField] private TMP_Text gainedStarText;          //이번 판 새로 얻은 별 수
    [SerializeField] private TMP_Text[] starConditionTexts;    //★1~★3 조건 문구
    [SerializeField] private Color metColor = new(1f, 0.82f, 0.1f, 1f);     //충족한 조건
    [SerializeField] private Color unmetColor = new(0.6f, 0.6f, 0.6f, 1f);  //충족하지 못한 조건

    private void OnEnable()
    {
        panel.SetActive(true);
        ShowStarResult();
        confirmButton.onClick.AddListener(Confirm);
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(Confirm);
    }

    //Privates
    private void ShowStarResult()
    {
        GameSystem game = GameSystem.Instance;
        gainedStarText.SetText($"획득한 별 +{game.LastGainedStars}");

        for (int i = 0; i < starConditionTexts.Length && i < game.LastAchievedStars.Count; i++)
        {
            starConditionTexts[i].SetText($"{i + 1}. {StarSystem.GetStarDescription(game.CurrentStageData, i)}");
            starConditionTexts[i].color = game.LastAchievedStars[i] ? metColor : unmetColor;
        }
    }

    private void Confirm()   //스테이지 선택 씬으로 이동 (해금·별·휘장은 승리 시 이미 기록됨)
    {
        GameSystem.Instance.ReturnToStageSelect();
    }
}
