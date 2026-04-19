using DG.Tweening;
using IsoTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    private static Camera camera = Camera.main;
    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0f)
    {
        Plane dragPlane = new(camera.transform.forward, new Vector3(0,0, zValue));
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    public static Vector3 GetMousePositionInWorldSpace()
    {
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    public static Vector3 GridToWorldPoint(Vector2Int gridPosition, int zValue)
    {
        IsoWorld isoWorld = TokenSystem.Instance.IsoWorld;
        if (isoWorld == null) return default;

        Vector2 screenPoint = isoWorld.IsoToScreen(new(gridPosition.x, gridPosition.y, 1));
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new(screenPoint.x, screenPoint.y, zValue));
        return worldPoint;
    }

    public static Vector2 GetSignVector2(Vector2 vector)
    {
        return new Vector2
        (
             Mathf.Sign(vector.x) * (vector.x != 0 ? 1 : 0),
             Mathf.Sign(vector.y) * (vector.y != 0 ? 1 : 0)
        );
    }

    public static Tween GetTween(Token token, Vector2 currentPos, Vector2 direction, float distance, float duration, Ease ease = Ease.Unset)
    {
        Tween tween = DOTween.To(() =>
        token.TokenTransform.positionXY,
        v => token.TokenTransform.positionXY = v,
        currentPos + distance * direction,
        duration
        );
        tween.SetEase(ease);
        return tween;
    }

    public static Tween GetTween(Token token, Vector2Int targetPos, float duration, Ease ease = Ease.Unset)
    {
        Tween tween = DOTween.To(() =>
        token.TokenTransform.positionXY,
        v => token.TokenTransform.positionXY = v,
        targetPos,
        duration
        );
        tween.SetEase(ease);
        return tween;
    }

    public static Tween GetBezierTween(Token token, Vector3 start, Vector3 end, float duration, Ease ease = Ease.Unset)
    {
        Vector3 control = (start + end) / 2f + new Vector3(0, 0, 1) * 3f; ;
        float t = 0f;

        Tween tween = DOTween.To(
            () => t,
            x =>
            {
                t = x;

                Vector3 pos =
                     Mathf.Pow(1 - t, 2) * start
                     + 2 * t * (1 - t) * control
                     + Mathf.Pow(t, 2) * end;
                token.TokenTransform.position = pos;
            },
            1f,
            duration
        );
        tween.SetEase(ease);
        return tween;
    }
}
