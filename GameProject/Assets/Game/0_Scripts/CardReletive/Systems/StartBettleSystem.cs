using System.Collections;
using UnityEngine;

public class StartBettleSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<StartBattleGA>(StartBattleGAPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<StartBattleGA>();
    }


    //Performers
    private IEnumerator StartBattleGAPerformer(StartBattleGA startBattleGA)
    {
        Debug.Log("게임 시작");
        EnemysTurnGA enemysTurnGA = new(true);
        ActionSystem.Instance.AddReaction(enemysTurnGA);
        yield return null;
    }
}
