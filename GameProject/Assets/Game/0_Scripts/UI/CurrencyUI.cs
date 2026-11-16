using TMPro;
using UnityEngine;

//상단 표시줄에 별·휘장 보유량 표시 (값이 바뀌면 바로 갱신, 연출 없음)
public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text starText;
    [SerializeField] private TMP_Text insigniaText;

    private int shownStars = -1;       //마지막으로 표시한 값 (바뀔 때만 텍스트 갱신)
    private int shownInsignias = -1;

    private void Update()
    {
        if (GameSystem.Instance == null) return;

        int stars = GameSystem.Instance.Stars;
        if (stars != shownStars)
        {
            shownStars = stars;
            starText.SetText(stars.ToString());
        }

        int insignias = GameSystem.Instance.Insignias;
        if (insignias != shownInsignias)
        {
            shownInsignias = insignias;
            insigniaText.SetText(insignias.ToString());
        }
    }
}
