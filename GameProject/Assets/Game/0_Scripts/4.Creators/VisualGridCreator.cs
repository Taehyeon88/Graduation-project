using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IsoTools;
using UnityEngine;

public class VisualGridCreator : Singleton<VisualGridCreator>
{
    [SerializeField] TokenGrid grid;
    [SerializeField] Transform gridVisualView;
    [SerializeField] VisualGridData[] visualGridDatas;

    //코어 호버 테두리(VG) 고정 위치 (그리드 밖 Iso 좌표)
    private static readonly Vector3 HeroCaptainVGPosition = new(-1.1f, 1.5f, 0.7f);
    private static readonly Vector3 EnemyFlagVGPosition = new(4.1f, 1.5f, 1.3f);

    //코어는 타일이 없어 그리드 좌표와 겹치지 않는 가상 키로 VG 슬롯을 등록
    public static readonly Vector2Int HeroCaptainSlot = new(-1, -1);
    public static readonly Vector2Int EnemyFlagSlot = new(-2, -2);

    private Dictionary<Vector2Int, IsoObject> vGByPosition = new();
    private Dictionary<(int, string), List<SpriteRenderer>> visualGridsById = new();
    private List<int> hideList = new();
    void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        //새로운 방식
        if (vGByPosition.Count == grid.width * grid.height + 2) return;

        var temp2 = gridVisualView.GetComponentsInChildren<IsoObject>(true);
        int count = 0;
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                if (count < temp2.Length)
                {
                    vGByPosition.TryAdd(new(x, y), temp2[count]);
                    temp2[count].position = new Vector3(x, y, 1);

                    count++;
                }
            }
        }

        //풀의 남는 타일을 코어 슬롯으로 사용
        if (count + 2 > temp2.Length)
        {
            Debug.LogError("VG 타일 풀이 부족해 코어 슬롯을 만들 수 없습니다. gridVisualView 아래 타일을 2개 이상 더 두세요.");
            return;
        }
        vGByPosition.TryAdd(HeroCaptainSlot, temp2[count]);
        temp2[count].position = HeroCaptainVGPosition;
        vGByPosition.TryAdd(EnemyFlagSlot, temp2[count + 1]);
        temp2[count + 1].position = EnemyFlagVGPosition;
    }

    public void ChangeVisualGrid(Vector2Int isoPosition, int tokenId, string beforeName, string afterName)
    {
        RemoveVisualGrid(tokenId, beforeName);
        CreateVisualGrid(tokenId, isoPosition, afterName);
    }

    public void ChangeVisualGridPosition(int tokenId, Vector2Int isoPosition, string vgName)
    {
        RemoveVisualGrid(tokenId, vgName);
        CreateVisualGrid(tokenId, isoPosition, vgName);
    }

    public void CreateVisualGrid(int tokenId, Vector2Int isoPosition, string vgName)
    {
        VisualGridData vgSO = Array.Find(visualGridDatas, v => v.name == vgName);

        if (vgSO == null) return;
        if (vGByPosition.Count <= 0) Initialize();

        foreach (var vgType in vgSO.VisualGridTypes)
        {
            Color color = vgType.GetVGTypeColor();
            Sprite sprite = vgType.GetVGTypeSprite();
            int layerOrder = vgType.sortingOrder;

            if (vGByPosition.TryGetValue(isoPosition, out var vg))
            {
                SpriteRenderer[] spriteRenderers = vg.GetComponentsInChildren<SpriteRenderer>(true);
                if (spriteRenderers != null)
                {
                    foreach (var sr in spriteRenderers)
                    {
                        if (sr.gameObject.CompareTag("VG_None"))
                        {
                            if(!CheckIsHideId(tokenId))
                                sr.gameObject.SetActive(true);
                            sr.sprite = sprite;
                            sr.color = color;
                            sr.gameObject.name = vgName;

                            Vector3 cur = sr.transform.localPosition;
                            sr.gameObject.transform.localPosition = new Vector3(cur.x, cur.y, 0f);
                            sr.sortingOrder = layerOrder;

                            sr.gameObject.tag = "VG_Using";

                            if (visualGridsById.ContainsKey((tokenId, vgName)))
                                visualGridsById[(tokenId, vgName)].Add(sr);
                            else
                                visualGridsById.Add((tokenId, vgName), new() { sr });

                            break;
                        }
                    }
                }
            }
        }
    }

    public bool RemoveVisualGrid(int tokenId, string vgName)
    {
        if (visualGridsById.ContainsKey((tokenId, vgName)))
        {
            var spriteRenderers = visualGridsById[(tokenId, vgName)];
            foreach (var sr in spriteRenderers)
            {
                sr.gameObject.tag = "VG_None";
                sr.sprite = null;
                sr.gameObject.name = "SpriteRenderer_defualt";
                sr.gameObject.SetActive(false);
            }
            visualGridsById[(tokenId, vgName)].Clear();

            return true;
        }
        return false;
    }

    public bool RemoveVisualGridById(int tokenId)
    {
        bool check = false;
        foreach (var vg in visualGridsById.ToList())
        {
            if (vg.Key.Item1 == tokenId)
            {
                var spriteRenderers = visualGridsById[(tokenId, vg.Key.Item2)];
                foreach (var sr in spriteRenderers)
                {
                    sr.gameObject.tag = "VG_None";
                    sr.sprite = null;
                    sr.gameObject.name = "SpriteRenderer_defualt";
                    sr.gameObject.SetActive(false);
                }
                visualGridsById[(tokenId, vg.Key.Item2)].Clear();

                check = true;
            }
        }
        return check;
    }

    public bool ContainVisualGrid(int tokenId, string vgName)
    {
        if (visualGridsById.ContainsKey((tokenId, vgName)))
        {
            if(visualGridsById[(tokenId, vgName)].Count > 0)
                return true;
            else
                return false;
        }
        return false;
    }

    public void SetHideVisualGrid(int tokenId)
    {
        hideList.Add(tokenId);
        foreach (var vg in visualGridsById)
        {
            if (vg.Key.Item1 == tokenId)
            {
                var spriteRenderers = visualGridsById[(tokenId, vg.Key.Item2)];
                foreach (var sr in spriteRenderers)
                {
                    sr.gameObject.SetActive(false);
                }
            }
        }
    }
    public void SetReverseVisualGrid(int tokenId)
    {
        hideList.Remove(tokenId);
        foreach (var vg in visualGridsById)
        {
            if (vg.Key.Item1 == tokenId)
            {
                var spriteRenderers = visualGridsById[(tokenId, vg.Key.Item2)];
                foreach (var sr in spriteRenderers)
                {
                    sr.gameObject.SetActive(true);
                }
            }
        }
    }


    //privates
    private bool CheckIsHideId(int tokenId)
    {
        return hideList.Contains(tokenId);
    }
}
