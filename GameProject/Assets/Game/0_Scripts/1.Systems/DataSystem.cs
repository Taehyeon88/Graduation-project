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

    [Header("Tokens - Walls")]
    [SerializeField] private WallData[] walls;

    [Header("Tokens - Traps")]
    [SerializeField] private TrapData[] traps;

    [Header("Tokens - Destructibles")]
    [SerializeField] private DestructibleData[] destructibles;

    [Header("Status Effects")]
    [SerializeField] private StatusEffectData[] statusEffects;

    [Header("Visual Effects")]
    [SerializeField] private VisualEffectData[] visualEffects;

    [Header("Visual Grids")]
    [SerializeField] private VisualGridData[] visualGrids;

    [Header("AoEs")]
    [SerializeField] private AoEData[] aoEs;

    [Header("Dices")]
    [SerializeField] private DiceData[] dices;

    [Header("Rooms")]
    [SerializeField] private RoomData[] rooms;

    [Header("Sounds")]
    [SerializeField] private SoundData[] sounds;

    public IReadOnlyList<CardData> Cards => cards;
    public IReadOnlyList<HeroData> Heroes => heroes;
    public IReadOnlyList<EnemyData> Enemies => enemies;
    public IReadOnlyList<WallData> Walls => walls;
    public IReadOnlyList<TrapData> Traps => traps;
    public IReadOnlyList<DestructibleData> Destructibles => destructibles;
    public IReadOnlyList<StatusEffectData> StatusEffects => statusEffects;
    public IReadOnlyList<VisualEffectData> VisualEffects => visualEffects;
    public IReadOnlyList<VisualGridData> VisualGrids => visualGrids;
    public IReadOnlyList<AoEData> AoEs => aoEs;
    public IReadOnlyList<DiceData> Dices => dices;
    public IReadOnlyList<RoomData> Rooms => rooms;
    public IReadOnlyList<SoundData> Sounds => sounds;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }
}
