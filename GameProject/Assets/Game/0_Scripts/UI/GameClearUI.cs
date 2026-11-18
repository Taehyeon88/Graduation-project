using System.Collections;
using DG.Tweening;
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
    [SerializeField] private GameObject[] starCheckMarks;      //조건 줄마다 충족 시 켜지는 체크 표시 (조건 문구와 같은 순서, 비워도 동작)
    [SerializeField] private Color metColor = new(1f, 0.82f, 0.1f, 1f);     //충족한 조건
    [SerializeField] private Color unmetColor = new(0.6f, 0.6f, 0.6f, 1f);  //충족하지 못한 조건

    [Header("휘장 결과")]
    [SerializeField] private TMP_Text insigniaText;            //이번 판 얻은 휘장 ("휘장 +N", 첫 클리어일 때만 표시, 비워도 동작)

    [Header("Direct Element")]
    [SerializeField] private float lineInterval = 0.5f;        //조건 줄 사이 간격
    [SerializeField] private float punchScale = 0.25f;

    private void OnEnable()
    {
        panel.SetActive(true);
        confirmButton.onClick.AddListener(Confirm);
        StartCoroutine(ShowResultRoutine());
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(Confirm);
    }

    //Privates
    //조건 문구를 먼저 보여 준 뒤 ★1 → ★2 → ★3 순서로 충족 여부를 확정 (연출 중에는 확인 버튼 잠금)
    private IEnumerator ShowResultRoutine()
    {
        GameSystem game = GameSystem.Instance;
        confirmButton.interactable = false;
        gainedStarText.SetText($"획득한 별 +{game.LastGainedStars}");
        if (insigniaText != null) insigniaText.SetText("");   //영역은 유지한 채 비워 둠 (튜토리얼이 이 영역을 비출 수 있게)

        int count = Mathf.Min(starConditionTexts.Length, game.LastAchievedStars.Count);
        for (int i = 0; i < count; i++)
        {
            starConditionTexts[i].SetText($"{i + 1}. {StarSystem.GetStarDescription(game.CurrentStageData, i)}");
            starConditionTexts[i].color = unmetColor;
            if (i < starCheckMarks.Length && starCheckMarks[i] != null) starCheckMarks[i].SetActive(false);
        }

        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(lineInterval);

            bool met = game.LastAchievedStars[i];
            starConditionTexts[i].color = met ? metColor : unmetColor;
            if (!met) continue;

            if (i < starCheckMarks.Length && starCheckMarks[i] != null) starCheckMarks[i].SetActive(true);
            starConditionTexts[i].transform.DOKill(true);
            starConditionTexts[i].transform.DOPunchScale(Vector3.one * punchScale, 0.3f);
        }

        if (insigniaText != null && game.LastGainedInsignias > 0)
        {
            yield return new WaitForSeconds(lineInterval);
            insigniaText.SetText($"휘장 +{game.LastGainedInsignias}");
            insigniaText.transform.DOKill(true);
            insigniaText.transform.DOPunchScale(Vector3.one * punchScale, 0.3f);
        }

        confirmButton.interactable = true;
    }

    private void Confirm()   //스테이지 선택 씬으로 이동 (해금·별·휘장은 승리 시 이미 기록됨)
    {
        GameSystem.Instance.ReturnToStageSelect();
    }
}
