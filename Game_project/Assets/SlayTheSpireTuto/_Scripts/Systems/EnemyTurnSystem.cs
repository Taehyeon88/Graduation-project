using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurn)
    {
        Debug.Log("Enemy Turn");
        yield return new WaitForSeconds(2f);
        Debug.Log("End Enemy Turn");
    }
}
