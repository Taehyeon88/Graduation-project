using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualGridCreator : Singleton<VisualGridCreator>
{
    [SerializeField] TokenGrid grid;
    [SerializeField] Transform gridVisualView;

    private Dictionary<Token, List<Transform>> visualGridsByToken = new();
    private List<Transform> heroSetupVisuals = new();
    private Queue<Transform> visualGrids = new();

    void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        var temp = gridVisualView.GetComponentsInChildren<Transform>(true);
        foreach (var t in temp)
        {
            if (t == gridVisualView) continue;
            visualGrids.Enqueue(t);
        }
    }

    public void CreateVisualGrid(Token token, Vector3 position, Color color)
    {
        if (visualGrids.Count <= 0) Initialize();

        if (visualGrids.TryDequeue(out var vg))
        {
            vg.GetComponent<SpriteRenderer>().color = color;
            vg.position = new Vector3(position.x, 0.01f, position.z);
            vg.gameObject.SetActive(true);
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

    public void CreateHeroVisualGrid(Vector3 position, Color color)
    {
        if (visualGrids.Count <= 0) Initialize();

        if (visualGrids.TryDequeue(out var vg))
        {
            vg.GetComponent<SpriteRenderer>().color = color;
            vg.position = new Vector3(position.x, 0.01f, position.z);
            vg.gameObject.SetActive(true);
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

    public void ChangeHeroVisualGrid(Vector3 position, Color color)
    {
        var target = heroSetupVisuals.Find(t => t.position == position);
        if (target != null)
        {
            target.GetComponent<SpriteRenderer>().color = color;
        }
        else
        {
            CreateHeroVisualGrid(position, color);
        }
    }
}
