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
    private Dictionary<Token, List<IsoObject>> visualGridsByToken = new();

    private Dictionary<Vector2Int, IsoObject> vGByPosition = new();
    private Dictionary<(int, string), List<SpriteRenderer>> visualGridsById = new();

    void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        //새로운 방식
        if (vGByPosition.Count == grid.width * grid.height) return;

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
    }

    public void ChangeVisualGrid(Vector2Int isoPosition, int tokenId, string beforeName, string afterName)
    {
        RemoveVisualGrid(tokenId, beforeName);
        CreateVisualGrid(tokenId, isoPosition, afterName);
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
            float vgTransZ = vgSO.GetVGTransZ(vgType.GetVGTypeTransZ());

            if (vGByPosition.TryGetValue(isoPosition, out var vg))
            {
                SpriteRenderer[] spriteRenderers = vg.GetComponentsInChildren<SpriteRenderer>(true);
                if (spriteRenderers != null)
                {
                    foreach (var sr in spriteRenderers)
                    {
                        if (sr.gameObject.CompareTag("VG_None"))
                        {
                            sr.gameObject.SetActive(true);
                            sr.sprite = sprite;
                            sr.color = color;
                            sr.gameObject.name = vgName;

                            Vector3 cur = sr.transform.localPosition;
                            sr.gameObject.transform.localPosition = new Vector3(cur.x, cur.y, vgTransZ);
 

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
}
