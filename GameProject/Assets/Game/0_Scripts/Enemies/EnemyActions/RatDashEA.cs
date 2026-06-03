using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class RatDashEA : EnemyAction
{
    public override Sprite Icon
    {
        get { return icon; }
        protected set { }
    }
    public override string Description
    {
        get { return description; }
        protected set { }
    }

    [SerializeField] private int damage = 6;
    [SerializeField] private int distance = 2;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    public override int? AttackDamage => damage;

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(enemy);
        Vector2Int direction = Directions[0];

        var simpleGrid = new Token[distance + 2];
        for (int i = 0; i <= distance + 1; i++)
        {
            Vector2Int targetPos = currentPos + direction * i;
            Token token = TokenSystem.Instance.GetTokenByPosition(targetPos);
            simpleGrid[i] = token;
        }

        for (int i = 1; i <= distance; i++)
        {
            Vector2Int targetPos = currentPos + direction * i;

            //플레이어 및 파괴가능 오브젝트 제외한 이동 가능여부 판단
            bool canMove = TokenSystem.Instance.IsGridEmpty(targetPos, false, true, true);
            if (canMove)
            {
                //피해 가능 오브젝트 (플레이어 제외)+ 파괴 불가인가?
                Token token = simpleGrid[i];
                if (token != null)
                {
                    if (token is DestructibleView dv && token.TokenData.Health > damage)
                    {
                        DealDamageGA dealDamageGA = new(damage, new() { dv }, enemy);
                        ActionSystem.Instance.AddReaction(dealDamageGA);
                        Debug.Log("시퀀스 2");
                        break;
                    }

                    //플레이어 충돌
                    if (token is HeroView heroView)
                    {
                        DealDamageGA dealDamageGA = new(damage, new() { heroView }, enemy);
                        ActionSystem.Instance.AddReaction(dealDamageGA);

                        KnockBackGA knockBackGA = new(enemy, 1, targetPos, direction);
                        ActionSystem.Instance.AddReaction(knockBackGA);

                        Vector2Int checkPos2 = currentPos + direction * (i + 1);

                        bool isEmpty = TokenSystem.Instance.IsGridEmpty(checkPos2, false, true, true) 
                                       && simpleGrid[i + 1] is not DestructibleView;
                        if (!isEmpty)
                        {
                            Debug.Log("시퀀스 3-1");
                            break;
                        }
                        else
                        {
                            Token hero = simpleGrid[i];
                            simpleGrid[i + 1] = hero;
                            simpleGrid[i] = null;
                            Debug.Log("시퀀스 3-2");
                        }
                    }

                    //피해 가능 오브젝트(플레이어 제외) + 파괴 가능
                    if (token is DestructibleView dv2 && token.TokenData.Health <= damage)
                    {
                        DealDamageGA dealDamageGA = new(damage, new() { dv2 }, enemy);
                        ActionSystem.Instance.AddReaction(dealDamageGA);

                        simpleGrid[i] = null;

                        Debug.Log("시퀀스 4");
                    }
                }

                Debug.Log("시퀀스 5");
                MoveGA moveGA = new(enemy, targetPos);
                ActionSystem.Instance.AddReaction(moveGA);
            }
            else
            {
                Debug.Log("시퀀스 1");
                break;
            }
        }

        return null;
    }

    public override EnemyAction Clone()
    {
        return new RatDashEA()
        {
            icon = icon,
            description = description,
            damage = damage,
            distance = distance,
        };
    }
}
