using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    public enum BuildingPreViewState
    {
        Positive, Negative
    }
    [SerializeField] private Material positiveMaterial;
    [SerializeField] private Material negativeMaterial;

    public BuildingPreViewState State { get; private set; } = BuildingPreViewState.Negative;
    public BuildingData Data { get; private set; }
    public BuildingModel BuildingModel { get; private set; }
    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();

    public void SetUp(BuildingData data)
    {
        Data = data;
        BuildingModel = Instantiate(data.buildingModel, transform.position, Quaternion.identity, transform);
        renderers.AddRange(BuildingModel.GetComponentsInChildren<Renderer>());
        colliders.AddRange(BuildingModel.GetComponentsInChildren<Collider>());
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        SetPreViewMaterial(State);
    }
    public void ChangeState(BuildingPreViewState newState)
    {
        if (State == newState) return;
        State = newState;
        SetPreViewMaterial(State);
    }
    public void Rotate(float rotationStep)
    {
        BuildingModel.Rotate(rotationStep);
    }
    private void SetPreViewMaterial(BuildingPreViewState newState)
    {
        Material previewMat = newState == BuildingPreViewState.Positive ? positiveMaterial : negativeMaterial;
        foreach (var rend in renderers)
        {
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = previewMat;
            }
            rend.materials = mats;
        }
    }
}
