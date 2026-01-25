using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenPreview : MonoBehaviour
{
    public enum TokenPreViewState
    {
        Positive, Negative
    }
    [SerializeField] private Material positiveMaterial;
    [SerializeField] private Material negativeMaterial;

    public TokenPreViewState State { get; private set; } = TokenPreViewState.Negative;
    public TokenData Data { get; private set; }
    public TokenModel TokenModel { get; private set; }
    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();

    public void SetUp(TokenData data)
    {
        Data = data;
        TokenModel = Instantiate(data.TokenModel, transform.position, Quaternion.identity, transform);
        renderers.AddRange(TokenModel.GetComponentsInChildren<Renderer>());
        colliders.AddRange(TokenModel.GetComponentsInChildren<Collider>());
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        SetPreViewMaterial(State);
    }
    public void ChangeState(TokenPreViewState newState)
    {
        if (State == newState) return;
        State = newState;
        SetPreViewMaterial(State);
    }
    public void Rotate(float rotationStep)
    {
        TokenModel.Rotate(rotationStep);
    }
    private void SetPreViewMaterial(TokenPreViewState newState)
    {
        Material previewMat = newState == TokenPreViewState.Positive ? positiveMaterial : negativeMaterial;
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
