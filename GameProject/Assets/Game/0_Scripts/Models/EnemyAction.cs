using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class EnemyAction
{
    public List<Vector2Int> targetRange { get; protected set; }
    public Vector2Int targetPosition { get; protected set; }  //PlayEnemyAction 내부에서 스스로 탐색해 채움

    public abstract Sprite Icon { get; protected set; }
    public abstract string Description { get; protected set; }
    public abstract string TextInfo { get; protected set; }

    public abstract Sequence PlayEnemyAction(EnemyView enemy);
    public abstract EnemyAction Clone();  //복사 함수

    //현재 위치 기준, 사거리 내 생존 영웅 중 임의로 하나 탐색. 없으면 null.
    protected HeroView FindRandomHeroInRange(EnemyView enemy, int distance)
    {
        Vector2Int myPos = TokenSystem.Instance.API.GetTokenPosition(enemy);

        var heroesInRange = TokenSystem.Instance.HeroViews
            .Where(h => h != null && h.CurrentHealth > 0)
            .Where(h => TokenSystem.Instance.API.GetDistance(myPos, TokenSystem.Instance.API.GetTokenPosition(h)) <= distance)
            .ToList();

        if (heroesInRange.Count == 0)
            return null;

        return heroesInRange[UnityEngine.Random.Range(0, heroesInRange.Count)];
    }
}
