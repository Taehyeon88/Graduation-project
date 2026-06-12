using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DataSystem : Singleton<DataSystem>
{
    [Header("Cards")]
    [SerializeField] private CardData[] cards;

    [Header("Tokens - Heroes")]
    [SerializeField] private HeroData[] heroes;

    [Header("Tokens - Enemies")]
    [SerializeField] private EnemyData[] enemies;

    [Header("Tokens - Objects")]
    [SerializeField] private ObjectData[] objects;

    [Header("Status Effects")]
    [SerializeField] private StatusEffectData[] statusEffects;

    [Header("Visual Effects")]
    [SerializeField] private VisualEffectData[] visualEffects;

    [Header("Visual Grids")]
    [SerializeField] private VisualGridData[] visualGrids;

    [Header("AoEs")]
    [SerializeField] private AoEData[] aoEs;

    [Header("Chunks")]
    [SerializeField] private ChunkData[] chunks;

    [Header("Rooms")]
    [SerializeField] private RoomData[] rooms;

    [Header("Sounds")]
    [SerializeField] private SoundData[] sounds;

    public IReadOnlyList<CardData> Cards => cards;
    public IReadOnlyList<HeroData> Heroes => heroes;
    public IReadOnlyList<EnemyData> Enemies => enemies;
    public IReadOnlyList<ObjectData> Objects => objects;
    public IReadOnlyList<StatusEffectData> StatusEffects => statusEffects;
    public IReadOnlyList<VisualEffectData> VisualEffects => visualEffects;
    public IReadOnlyList<VisualGridData> VisualGrids => visualGrids;
    public IReadOnlyList<AoEData> AoEs => aoEs;
    public IReadOnlyList<RoomData> Rooms => rooms;
    public IReadOnlyList<SoundData> Sounds => sounds;

    private Dictionary<int, ObjectData> ObjectDatas;
    private Dictionary<int, ChunkData> ChunkDatas;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }

    //오브젝트 데이터 캐싱 함수
    private void ResetDataByType(Type type)
    {
        if (type == typeof(ObjectData))               //오브젝트 데이터
        {
            ObjectDatas = new Dictionary<int, ObjectData>();

            foreach (var obj in objects)
                ObjectDatas.TryAdd(obj.ObjectId, obj);
        }
        else if (type == typeof(ChunkData))           //청크 데이터
        {
            ChunkDatas = new Dictionary<int, ChunkData>();

            foreach (var chunk in chunks)
                ChunkDatas.TryAdd(chunk.Chunk_id, chunk);
        }
    }

    /// <summary>
    /// 오브젝트 데이터 찾기 함수
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ObjectData GetObjectById(int id)
    {
        if (ObjectDatas == null)
            ResetDataByType(typeof(ObjectData));

        if (ObjectDatas.ContainsKey(id))
        {
            return ObjectDatas[id];
        }
        Debug.LogError($"{id} 아이디의 ObjectData를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 청크 데이터 찾기 함수
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ChunkData GetChunkById(int id)
    {
        if (ChunkDatas == null)
            ResetDataByType(typeof(ChunkData));

        if (ChunkDatas.ContainsKey(id))
        {
            return ChunkDatas[id];
        }
        Debug.LogError($"{id} 아이디의 ChunkData를 찾을 수 없습니다.");
        return null;
    }
}
