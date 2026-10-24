using IsoTools;
using UnityEngine;

public class TerrainView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer model;
    [SerializeField] private IsoObject isoObject;

    private TerrainEffect effect;   //SO 공유를 피하려고 발판별 사본을 보유

    public TerrainData Data { get; private set; }   //정보 패널 표시용

    public void SetUp(TerrainData data, Vector2Int gridPosition)
    {
        Data = data;
        model.sprite = data.Sprite;
        isoObject.position = new Vector3(gridPosition.x, gridPosition.y, isoObject.position.z);   //높이는 프리팹 값 유지
        effect = data.Effect?.Clone();
    }

    public void Enter(CombatantView unit) => effect?.OnEnter(unit);

    public void Exit(CombatantView unit) => effect?.OnExit(unit);
}
