using System;
using UnityEngine;

[Serializable]
public struct ObstacleSpawn   // 스테이지에 배치할 장애물 (위치 + 데이터)
{
    public Vector2Int Position;
    public ObstacleData Data;
}
