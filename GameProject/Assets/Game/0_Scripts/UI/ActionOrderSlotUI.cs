using TMPro;
using UnityEngine;
using UnityEngine.UI;

//행동 순서 띠의 유닛 한 칸
public class ActionOrderSlotUI : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private GameObject nextMark;   //다음에 행동하는 유닛 표시

    public void SetUp(CombatantView unit, bool isNext)
    {
        portrait.sprite = unit.TokenData.Sprite;
        speedText.SetText(unit.Speed.ToString());
        nextMark.SetActive(isNext);
    }
}
