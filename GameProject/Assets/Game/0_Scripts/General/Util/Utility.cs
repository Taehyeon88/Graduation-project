using DG.Tweening;
using IsoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static Vector3 IsoPositionToWorldPoint(Token target, Vector3 position)
    {
        Vector3 world = TokenSystem.Instance.IsoWorld.IsoToScreen(position);
        world += target.Defualt_Direction;
        return world;
    }

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
    //백터2를 아이소매트릭용 위치값으로 변환
    public static Vector3 Vector2ToIsoVector(Vector2 vector, int hight = 1)
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
    //Vector2Int를 Vector3로 변환
    public static Vector3 Vector2IntToVector3(Vector2Int vector, float z = 0)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    // <summary>
    // Isometric 위치 이동 Tween 반환
    public static Tween GetTween(Token token, Vector2 targetPos, float distance, float duration, Ease ease = Ease.Unset)
    {
        Vector2 currentPos = TokenSystem.Instance.API.GetTokenPosition(token);
        Vector2 direction = Utility.GetSignVector2(targetPos - currentPos);

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
    // Isometric 위치 이동 Tween 반환
    public static Tween GetTweenByDirection(Token token, Vector2 currentPos, Vector2 direction, float distance, float duration, Ease ease = Ease.Unset)
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
    // Isometric 위치 이동 BackTween 반환 (자동 일시정지)
    public static Tween GetBackTween(Token token, float duration, Ease ease = Ease.Unset)
    {
        Tween tween = DOTween.To(() =>
        token.TokenTransform.positionXY,
        v => token.TokenTransform.positionXY = v,
        TokenSystem.Instance.API.GetTokenPosition(token),
        duration
        );
        tween.SetEase(ease).Pause();
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

    // <summary>
    // Isometric 오브젝트 진동 Tween
    public static Tween ShakePosition(IsoObject isoObject, float duration, float power, int vibrato = 10)
    {
        Vector2 originalPosition = isoObject.positionXY;

        return DOTween.To(
            () => 0f,
            value =>
            {
                float progress = value;

                // 진동 횟수
                float wave = Mathf.Sin(progress * vibrato * Mathf.PI * 2f);

                // 진폭 감소
                float damping = 1f - progress;

                //운동 방향
                Vector2 strength = new Vector2(
                        UnityEngine.Random.value > 0.5f ? 1f : -1f,
                        UnityEngine.Random.value > 0.5f ? 1f : -1f);

                Vector2 offset = strength * wave * damping * UnityEngine.Random.Range(0f, power);

                isoObject.positionXY = originalPosition + offset;
            },
            1f,
            duration
        ).SetEase(Ease.Linear);
    }

    // <summary>
    // 특정 위치 리스트의 모든 CombatantView 찾기
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

    public static Tween GetModelTween(Token token, Vector2Int current_Pos, Vector2 direction, float distance, float duration, Ease ease)
    {
        Vector3 endPos = Utility.IsoPositionToWorldPoint(
            token,
            Utility.Vector2ToIsoVector(current_Pos + direction * distance)
        );
        return token.Model.transform.DOMove(endPos, duration).SetEase(ease);
    }
    public static Tween GetModelBackTween(Token token, float duration, Ease ease)
    {
        Vector3 endPos = token.Defualt_Position;
        return token.Model.transform.DOLocalMove(endPos, duration).SetEase(ease);
    }
    public static Tween GetModelShakeTween(Token token, float duration, float strangth, int vibrato, Ease ease)
    {
        return token.Model.transform.DOShakePosition(duration, strangth, vibrato)
            .SetEase(ease);
    }
    public static Tween GetModelShakeTween(Token token, float duration, Vector3 strangth, int vibrato, float randomness)
    {
        return token.Model.transform.DOShakePosition(duration, strangth, vibrato, randomness);
    }
}
