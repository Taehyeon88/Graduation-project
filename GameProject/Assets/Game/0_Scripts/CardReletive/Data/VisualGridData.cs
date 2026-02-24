using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/VisualGrid")]
public class VisualGridData : ScriptableObject
{
    [field: SerializeReference, SR] public List<VisualGridType> VisualGridTypes {  get; private set; }
    [Range(0, 2)]
    [SerializeField] private int layerDepth;            //같은 VG끼리의 깊이 설정(수치UP -> 우선도UP)
    public int LayerDepth { get { return layerDepth; } }

    public float GetVGTransZ(float targetZ) => targetZ - layerDepth * 0.3f;
}

[System.Serializable]
public abstract class VisualGridType
{
    public abstract Color GetVGTypeColor();
    public abstract Sprite GetVGTypeSprite();
    public abstract float GetVGTypeTransZ();
}

public class FillType : VisualGridType
{
    [SerializeField] public Color Color;
    [SerializeField] public Sprite sprite;
    private const float transZ = -1f;

    public override Color GetVGTypeColor() => Color;
    public override Sprite GetVGTypeSprite() => sprite;
    public override float GetVGTypeTransZ() => transZ;
}

public class BorderType : VisualGridType
{
    [SerializeField] public Color Color;
    [SerializeField] public Sprite sprite;
    private const float transZ = -2f;
    public override Color GetVGTypeColor() => Color;
    public override Sprite GetVGTypeSprite() => sprite;
    public override float GetVGTypeTransZ() => transZ;
}

public class SymbolType : VisualGridType
{
    [HideInInspector] public Color Color = Color.white;
    [SerializeField] public Sprite sprite;
    private const float transZ = -3f;
    public override Color GetVGTypeColor() => Color;
    public override Sprite GetVGTypeSprite() => sprite;
    public override float GetVGTypeTransZ() => transZ;
}