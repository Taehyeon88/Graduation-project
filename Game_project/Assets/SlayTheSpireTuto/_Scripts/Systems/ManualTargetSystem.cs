using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTargetSystem : Singleton<ManualTargetSystem>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;

    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.gameObject.SetActive(true);
        arrowView.SetUp(startPosition);
    }
    public EnemyView EndTargeting(Vector3 endPosition)
    {
        arrowView.gameObject.SetActive(false);
        if (Physics.Raycast(endPosition, Vector3.forward, out var hitInfo, 10f, targetLayerMask)
            &&hitInfo.collider != null
            &&hitInfo.collider.TryGetComponent(out EnemyView enemyView))
        {
            return enemyView;
        }
        return null;
    }
}
