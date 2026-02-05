using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IsoTools;
using UnityEngine;

public class VisualGridCreator : Singleton<VisualGridCreator>
{
    [SerializeField] TokenGrid grid;
    [SerializeField] Transform gridVisualView;
    private Dictionary<Token, List<IsoObject>> visualGridsByToken = new();
    private List<IsoObject> heroSetupVisuals = new();
    private Queue<IsoObject> visualGrids = new();

    void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        var temp = gridVisualView.GetComponentsInChildren<IsoObject>(true);
        foreach (var t in temp)
        {
            if (t == gridVisualView) continue;
            visualGrids.Enqueue(t);
        }
    }

    public void CreateVisualGrid(Token token, Vector2Int isoPosition, Color color)
    {
        if (visualGrids.Count <= 0) Initialize();

        if (visualGrids.TryDequeue(out var vg))
        {
            vg.gameObject.SetActive(true);
            vg.GetComponentInChildren<SpriteRenderer>().color = color;
            vg.position = new Vector3(isoPosition.x, isoPosition.y, 1);
            if (visualGridsByToken.ContainsKey(token))
                visualGridsByToken[token].Add(vg);
            else
                visualGridsByToken.Add(token, new() { vg });
        }
    }

    public void RemoveVisualGrid(Token token)
    {
        if (visualGridsByToken.ContainsKey(token))
        {
            var visuals = visualGridsByToken[token];
            foreach (var v in visuals)
            {
                visualGrids.Enqueue(v);
                v.gameObject.SetActive(false);
            }
            visualGridsByToken[token].Clear();
        }
    }

    public void CreateHeroVisualGrid(Vector2Int isoPosition, Color color)
    {
        if (visualGrids.Count <= 0) Initialize();

        if (visualGrids.TryDequeue(out var vg))
        {
            vg.gameObject.SetActive(true);
            vg.GetComponentInChildren<SpriteRenderer>().color = color;
            vg.position = new Vector3(isoPosition.x, isoPosition.y, 1);
            heroSetupVisuals.Add(vg);
        }
    }

    public void RemoveHeroVisualGrid()
    {
        foreach (var v in heroSetupVisuals)
        {
            visualGrids.Enqueue(v);
            v.gameObject.SetActive(false);
        }
        heroSetupVisuals.Clear();
    }

    public void ChangeHeroVisualGrid(Vector2Int isoPosition, Color color)
    {
        var target = heroSetupVisuals.Find(t => t.positionXY == isoPosition);
        if (target != null)
        {
            target.GetComponentInChildren<SpriteRenderer>().color = color;
        }
        else
        {
            CreateHeroVisualGrid(isoPosition, color);
        }
    }
}
