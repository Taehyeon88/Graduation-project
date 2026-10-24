using IsoTools;
using UnityEngine;

public class StructureView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer model;
    [SerializeField] private IsoObject isoObject;
    [SerializeField] private Color selectableColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    public StructureData Data { get; private set; }
    public int StageNumber { get; private set; }   //스테이지 번호 (캐싱 키)

    public void SetUp(StructureData data, int stageNumber, Vector3 isoPosition)
    {
        Data = data;
        StageNumber = stageNumber;
        model.sprite = data.Sprite;
        isoObject.position = isoPosition;
        SetSelectable(false);
    }

    public void SetSelectable(bool selectable)
    {
        model.color = selectable ? selectableColor : lockedColor;
    }

    public bool IsMouseOver(Vector3 mouseWorld)
    {
        Bounds bounds = model.bounds;
        mouseWorld.z = bounds.center.z;
        return bounds.Contains(mouseWorld);
    }
}
