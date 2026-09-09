using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class HeroPreview : Token
{
    public enum TokenPreViewState
    {
        Positive, Negative
    }

    public TokenPreViewState State { get; private set; } = TokenPreViewState.Negative;
    public void SetUp(TokenData data)
    {
        IsoObject isoObject = GetComponent<IsoObject>();
        if (isoObject == null)
            isoObject = gameObject.AddComponent<IsoObject>();

        SetUpBaseBase(data, isoObject);
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
        Color color = Model.color;
        color.a = newState == TokenPreViewState.Positive ? 0.8f : 0.0f;
        Model.color = color;
    }
}
