using DG.Tweening;
using IsoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static Vector2 GetSignVector2(Vector2 vector)
    {
        return new Vector2
        (
             Mathf.Sign(vector.x) * (vector.x != 0 ? 1 : 0),
             Mathf.Sign(vector.y) * (vector.y != 0 ? 1 : 0)
        );
    }

    public static Vector2Int GetSignVector2Int(Vector2Int vector)
    {
        return new Vector2Int
        (
             (int) Mathf.Sign(vector.x) * (vector.x != 0 ? 1 : 0),
             (int) Mathf.Sign(vector.y) * (vector.y != 0 ? 1 : 0)
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

    public static Tween GetLinearTween(IsoObject isoObject, Vector2Int startPos, Vector2Int targetPos, float duration, Ease ease = Ease.Unset)
    {
        isoObject.positionXY = startPos;
        Tween tween = DOTween.To(() =>
        isoObject.positionXY,
        v => isoObject.positionXY = v,
        targetPos,
        duration
        );
        tween.SetEase(ease);
        return tween;
    }

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

    public static List<Vector2Int> FindChunkSetPosition(int[,] grid, ChunkData chunkData)
    {
        List<Vector2Int> result = new();

        //블럭 정보를 byte로 변환
        int[] chunkArray = new int[chunkData.Objects.Length];
        for (int i = 0; i < chunkData.Objects.Length; i++)
            chunkArray[i] = chunkData.Objects[i] > 0 ? 1 : 0;

        string blockString = string.Join("", chunkArray);
        byte blockByte = Convert.ToByte(blockString, 2);

        //1. 가로 세로의 맵 규격으로 추리기
        //2. 특정 위치에서 청크와 겹치는 오브젝트 체크로 추리기
        for(int x = 0; x < grid.GetLength(0); x++)
        {
            for(int y = 0; y < grid.GetLength(1); y++)
            {
                int maxX = x + chunkData.Width;
                int maxY = y + chunkData.Height;
                if(maxX >= grid.GetLength(0) || maxY >= grid.GetLength(1))
                        continue;

                //환경 정보를 byte로 변환
                int cnt = 0;
                int[] temp = new int[chunkData.Width * chunkData.Height];
                for (int ty = 0; ty < chunkData.Height; ty++)
                {
                    for (int tx = 0; tx < chunkData.Width; tx++)
                    {
                        temp[cnt] = grid[x + tx, y + ty];
                        cnt++;
                    }
                }
                string gridString = string.Join("", temp);
                byte gridByte = Convert.ToByte(gridString, 2);

                var te = blockByte & gridByte;

                if (te == 0)
                {

                }
            }
        }
        return null;
    }
}
