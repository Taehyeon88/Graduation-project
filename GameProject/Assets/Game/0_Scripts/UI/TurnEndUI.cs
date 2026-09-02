using UnityEngine;
using UnityEngine.UI;

public class TurnEndUI : MonoBehaviour
{
    
    private void OnEnable()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(TurnEnd);
        }
    }

    private void OnDisable()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private void TurnEnd()
    {
        //플레이어 턴 종료 및 몬스터턴 시작
        TurnGA turnGA = new(TurnType.Enemy);
        ActionSystem.Instance.Perform(turnGA);
    }
}
