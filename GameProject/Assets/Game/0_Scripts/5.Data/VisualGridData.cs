using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/VisualGrid")]
public class VisualGridData : ScriptableObject
{
    [field: SerializeReference, SR] public List<VisualGridType> VisualGridTypes {  get; private set; }
}

[System.Serializable]
public abstract class VisualGridType
{
    [SerializeField] public int sortingOrder;            //수치UP -> 우선도UP (타입 무관, 순수 Order in Layer)

    public abstract Color GetVGTypeColor();
    public abstract Sprite GetVGTypeSprite();
}

[System.Serializable]
public class FillType : VisualGridType
{
    [SerializeField] public Color Color;
    [SerializeField] public Sprite sprite;

    public override Color GetVGTypeColor() => Color;
    public override Sprite GetVGTypeSprite() => sprite;
}

[System.Serializable]
public class BorderType : VisualGridType
{
    [SerializeField] public Color Color;
    [SerializeField] public Sprite sprite;

    public override Color GetVGTypeColor() => Color;
    public override Sprite GetVGTypeSprite() => sprite;
}

[System.Serializable]
public class SymbolType : VisualGridType
{
    [HideInInspector] public Color Color = Color.white;
    [SerializeField] public Sprite sprite;

    public override Color GetVGTypeColor() => Color;
    public override Sprite GetVGTypeSprite() => sprite;
}
