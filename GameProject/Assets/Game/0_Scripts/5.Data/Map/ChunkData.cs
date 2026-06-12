using UnityEngine;

[CreateAssetMenu(menuName = "Data/Map/Chunk")]
public class ChunkData : ScriptableObject
{
   [field: SerializeField] public int Chunk_id {get; private set;}     //청크 고유 식별자
   [field: SerializeField] public int Width {get; private set;}         //가로
   [field: SerializeField] public int Height {get; private set;}        //세로
   [field: SerializeField] public int[] Objects {get; private set;}   //장애물 리스트
}
