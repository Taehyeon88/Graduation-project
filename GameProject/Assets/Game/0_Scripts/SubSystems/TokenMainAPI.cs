using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenMainAPI : MonoBehaviour
{
    [Header("TokenGrid")]
    [field: SerializeField] private TokenGrid grid;

    public List<EnemyView> EnemyViews => TokenSystem.Instance.EnemyViews;

    /// <summary>
    /// 토큰 추가 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="gridPosition"></param>
    public void AddToken(TokenData tokenData, TokenType tokenType, Vector2Int gridPosition)
    {
        Token token = TokenCreator.Instance.CreateToken(
               tokenData,
               tokenType,
               new(gridPosition.x, gridPosition.y, 1)
            );

        //해당 타일에 토큰으로 등록처리 (TokenSystem, Gird)
        grid.SetToken(token, gridPosition);

        if (token is EnemyView enemyView)
        {
            EnemyViews.Add(enemyView);
        }
    }

    /// <summary>
    /// 토큰 삭제 함수
    /// </summary>
    /// <param name="token"></param>
    public IEnumerator RemoveToken(Token token)
    {
        if (token is EnemyView enemyView)
        {
            EnemyViews.Remove(enemyView);
        }
        grid.RemoveToken(token);

        Tween tween = token.Transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(token.gameObject);
    }

    /// <summary>
    /// 임시 토큰 제거 함수(CombatantView가 아닌 것들의 제거)
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public void RemoveToken2(Token token)
    {
        if (token is EnemyView enemyView)
        {
            EnemyViews.Remove(enemyView);
        }
        grid.RemoveToken(token);

        Destroy(token.gameObject);
    }

    /// <summary>
    /// 토큰(영웅, 적, 건물) 이동 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerator MoveToken(Token token, Vector2Int targetPos, bool useAnimation = true, bool useMovedPath = true)
    {
        grid.ChangeTokenPos(token, targetPos);

        if (useAnimation)
        {
            Tween tween = Utility.GetTween(token, targetPos, 0.3f);
            yield return tween.WaitForCompletion();
        }
    }
}
