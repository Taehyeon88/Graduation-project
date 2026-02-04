using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class TokenPreview : Token
{
    public enum TokenPreViewState
    {
        Positive, Negative
    }
    [SerializeField] private Material positiveMaterial;
    [SerializeField] private Material negativeMaterial;

    public TokenPreViewState State { get; private set; } = TokenPreViewState.Negative;
    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();

    public void SetUp(TokenData data, Vector3 isoPosition)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        TokenTransform = isObject;
        TokenTransform.position = isoPosition;
        TokenData = data;
        TokenModel = Instantiate(data.TokenModel, transform.position + Vector3.up * 1.7f, Quaternion.identity, transform);
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
