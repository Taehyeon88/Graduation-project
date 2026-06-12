using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Map/MapTheme")]
public class MapThemeData : ScriptableObject
{
    [field: SerializeField] public bool IsBossRoom { get; private set; }
    [field: SerializeField] public int MinChunkCount {get; private set;} = 1;      //청크 설치 최소 개수
    [field: SerializeField] public int MaxChunkCount {get; private set;}           //청크 설치 최대 개수
    [field: SerializeField] public int MinObjectCount {get; private set;}          //오브젝트 설치 최소 개수
    [field: SerializeField] public int MaxObjectCount { get; private set; }        //오브젝트 설치 최대 개수
    [field: SerializeField] public int[] ChunkPool {get; private set;}             //설치할 청크ID 풀
    [field: SerializeReference, SR] public ObstacleSpawnInfo[] ObstacleSpawnInfos {get; private set;}  //나머지 설치할 오브젝트 정보들
}

[System.Serializable]
public class ObstacleSpawnInfo
{
    [field : SerializeField] public int ObstacleId {get; private set;}    //오브젝트 ID
    [field : SerializeField] public int spawnWeight {get; private set;}   //오브젝트 가중치
    [field : SerializeField] public int maxSpawnLimit {get; private set;} //오브젝트 최대 생성 개수
}
