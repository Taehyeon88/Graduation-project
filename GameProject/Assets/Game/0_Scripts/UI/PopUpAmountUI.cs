using IsoTools;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpAmountUI : MonoBehaviour
{
    [Header("Element")]
    [SerializeField] private IsoObject[] popup_UIs;

    private TMP_Text[] popup_Texts;

    private void Start()
    {
        popup_Texts = new TMP_Text[popup_UIs.Length];

        for (int i = 0; i < popup_UIs.Length; i++)
        {
            popup_Texts[i] = popup_UIs[i].GetComponentInChildren<TMP_Text>();
            popup_UIs[i].gameObject.SetActive(false);
        }
    }

    public void PopUpAmount(float amount, Token caster, List<Vector2Int> targetPoses)
    {
        int index = 0;
        foreach (var targetPos in targetPoses)
        {
            int damage = DamageCaculator.GetDamage(
                amount, 
                caster as CombatantView, 
                TokenSystem.Instance.API.GetTokenByPosition(targetPos) as CombatantView
                );

            popup_UIs[index].gameObject.SetActive(true);                           //활성화
            popup_UIs[index].position = new Vector3(targetPos.x, targetPos.y, 1);  //위치 설정
            popup_Texts[index].SetText(damage.ToString());                         //수치 갱신

            index++;
        }
    }
    public void EndPopUpAmount()
    {
        for (int i = 0; i < popup_UIs.Length; i++)
        {
            popup_UIs[i].gameObject.SetActive(false);
        }
    }
}
