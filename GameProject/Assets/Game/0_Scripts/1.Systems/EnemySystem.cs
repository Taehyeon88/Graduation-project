using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public IReadOnlyList<Vector2Int> SetupPoses => setupPoses;   //적 배치 가능 위치 (라인당 1개, 전투 중 불변)

    private List<Vector2Int> setupPoses = new();

    public void SetUp(List<Vector2Int> poses)
    {
        setupPoses = poses;
    }

    //해당 라인의 배치 위치 x (없으면 false)
    public bool TryGetSetupX(int line, out int x)
    {
        foreach (var pos in setupPoses)
        {
            if (pos.y != line) continue;
            x = pos.x;
            return true;
        }
        x = 0;
        return false;
    }
}
