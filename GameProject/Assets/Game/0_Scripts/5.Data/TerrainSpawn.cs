using System;
using UnityEngine;

[Serializable]
public struct TerrainSpawn   // 스테이지에 배치할 발판 (위치 + 데이터)
{
    public Vector2Int Position;
    public TerrainData Data;
}
