using UnityEngine;
using UnityEngine.UI;

//턴 순서 표시: 내 턴 → 자동 전투 → 적 턴 중 지금 진행 중인 칸을 강조
public class TurnOrderUI : MonoBehaviour
{
    [SerializeField] private Image playerSlot;
    [SerializeField] private Image autoBattleSlot;
    [SerializeField] private Image enemySlot;
    [SerializeField] private Color idleColor = new(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color activeColor = new(1f, 0.85f, 0.3f, 1f);

    private void LateUpdate()
    {
        TurnType turn = TurnSystem.Instance.CurrentTurn;
        playerSlot.color = turn == TurnType.Player ? activeColor : idleColor;
        autoBattleSlot.color = turn == TurnType.AutoBattle ? activeColor : idleColor;
        enemySlot.color = turn == TurnType.Enemy ? activeColor : idleColor;
    }
}
