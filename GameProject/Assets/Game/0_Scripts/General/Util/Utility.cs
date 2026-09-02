using DG.Tweening;
using IsoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    // <summary>
    // 방향 정리 함수 EX (-999, 4) -> (-1, 1)
    public static Vector2 GetSignVector2(Vector2 vector)
    {
        return new Vector2
        (
             Mathf.Sign(vector.x) * (vector.x != 0 ? 1 : 0),
             Mathf.Sign(vector.y) * (vector.y != 0 ? 1 : 0)
        );
    }

    // <summary>
    // 방향 정리 함수 EX (-999, 4) -> (-1, 1)
    public static Vector2Int GetSignVector2Int(Vector2Int vector)
    {
        return new Vector2Int
        (
             (int) Mathf.Sign(vector.x) * (vector.x != 0 ? 1 : 0),
             (int) Mathf.Sign(vector.y) * (vector.y != 0 ? 1 : 0)
        );
    }

    // <summary>
    //백터2Int를 아이소매트릭용 위치값으로 변환
    public static Vector3 Vector2IntToIsoVector(Vector2Int vector, int hight = 1)
    {
        return new Vector3(vector.x, vector.y, hight);
    }

    // <summary>
    //아이소매트릭용 위치값을 백터2Int로 변환
    public static Vector2Int IsoVectorToVector2Int(Vector3 vector)
    {
        return new Vector2Int((int)vector.x, (int)vector.y);
    }

    // <summary>
    // Isometric 위치 이동 Tween 반환 (방향 기반으로 거리 조정 가능)
    public static Tween GetTween(Token token, Vector2Int currentPos, Vector2Int targetPos, float duration, float distance = 0.8f, Ease ease = Ease.Unset)
    {
        Vector2 direction = Utility.GetSignVector2Int(targetPos - currentPos);

        Tween tween = DOTween.To(() =>
        token.TokenTransform.positionXY,
        v => token.TokenTransform.positionXY = v,
        currentPos + distance * direction,
        duration
        );
        tween.SetEase(ease);
        return tween;
    }

    // <summary>
    // Isometric 위치 이동 Tween 반환2
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

    // <summary>
    // Isometric 위치 이동 Tween 반환3
    public static Tween GetTween(IsoObject isoObject, Vector2Int targetPos, float duration, Ease ease = Ease.Unset)
    {
        Tween tween = DOTween.To(() =>
        isoObject.positionXY,
        v => isoObject.positionXY = v,
        targetPos,
        duration
        );
        tween.SetEase(ease);
        return tween;
    }

    // <summary>
    // Isometric 위치 이동 BackTween 반환
    public static Tween GetBackTween(Token token, float duration, Ease ease = Ease.Unset)
    {
        Tween tween = DOTween.To(() =>
        token.TokenTransform.positionXY,
        v => token.TokenTransform.positionXY = v,
        TokenSystem.Instance.API.GetTokenPosition(token),
        duration
        );
        tween.SetEase(ease);
        return tween;
    }

    // <summary>
    // Isometric 위치 곡선 이동 Tween 반환
    public static Tween GetBezierTween(IsoObject isoObject, Vector3 start, Vector3 end, float duration, Ease ease = Ease.Unset, float heighRate = 1)
    {
        Vector3 control = (start + end) / 2f + new Vector3(0, 0, 1) * 3f * heighRate;
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
                isoObject.position = pos;
            },
            1f,
            duration
        );
        tween.SetEase(ease);
        return tween;
    }

    // <summary>
    // Isometric 화살 위치 곡선 이동 Tween 반환 (기울기 적용)
    public static Tween GetArrowBezierTween(IsoObject isoObject, Transform arrowTrans, Vector3 start, Vector3 end, float duration, Ease ease = Ease.Unset, float heighRate = 1)
    {
        Vector3 control = (start + end) / 2f + new Vector3(0, 0, 1) * 3f * heighRate;
        float t = 0f;
        Vector2 preScreenPos = start;


        Tween tween = DOTween.To(
            () => t,
            x =>
            {
                t = x;

                Vector3 pos =
                     Mathf.Pow(1 - t, 2) * start
                     + 2 * t * (1 - t) * control
                     + Mathf.Pow(t, 2) * end;
                isoObject.position = pos;


                Vector2 currenPos = TokenSystem.Instance.IsoWorld.IsoToScreen(pos);
                Vector2 direction = currenPos - preScreenPos;

                preScreenPos = currenPos;

                //회전 처리
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrowTrans.rotation = Quaternion.Euler(0, 0, angle - 90f);
            },
            1f,
            duration
        );
        tween.SetEase(ease);
        return tween;
    }

    public static List<CombatantView> PositionsToCombantViews(List<Vector2Int> targetPoses, bool exceptEnemy = false, bool exceptHero = false)
    {
        List<CombatantView> combatants = new(10);
        foreach (var targetPos in targetPoses)
        {
            CombatantView combat = TokenSystem.Instance.API.GetTokenByPosition(targetPos) as CombatantView;
            if (combat != null)
            {
                if (exceptEnemy && combat is EnemyView) continue;
                if (exceptHero && combat is HeroView) continue;

                combatants.Add(combat);
            }
        }
        return combatants;
    }
}
