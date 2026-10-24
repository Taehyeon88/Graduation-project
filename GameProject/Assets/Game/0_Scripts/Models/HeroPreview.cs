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

    [SerializeField] private SpriteRenderer bottom;   //발판 (모델과 같은 알파로 함께 표시·숨김)

    public TokenPreViewState State { get; private set; } = TokenPreViewState.Negative;
    public void SetUp(TokenData data)
    {
        SetUpBaseBase(data);
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

        if (bottom != null)
        {
            Color bottomColor = bottom.color;
            bottomColor.a = color.a;
            bottom.color = bottomColor;
        }
    }
}
