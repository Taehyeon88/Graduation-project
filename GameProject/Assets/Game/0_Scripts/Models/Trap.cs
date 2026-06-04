using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Trap
{
    public abstract void AddChain(Token myToken);
    public abstract void RemoveChain();
    public abstract Trap Clone();
}

[System.Serializable]
public class SpikeTrap : Trap
{
    [SerializeField] private int damage = 1;

    private Token myToken;

    public override void AddChain(Token myToken)
    {
        this.myToken = myToken;
        ActionSystem.SubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
    }

    public override void RemoveChain()
    {
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
    }

    public override Trap Clone()
    {
        return new SpikeTrap()
        {
            damage = this.damage,
        };
    }

    private void MoveGAPostReaction(MoveGA moveGA)
    {
        if (moveGA.movePosition == TokenSystem.Instance.GetTokenPosition(myToken))
        {
            DealDamageGA dealDamageGA = new(damage, new() { moveGA.mover }, null);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
    }
}

[System.Serializable]
public class Bomb : Trap
{
    [SerializeField] private int turnCount = 3;
    [SerializeField] private int distance = 2;
    [SerializeField] private int damage = 2;

    private Token myToken;
    private List<Vector2Int> range;
    
    public override void AddChain(Token myToken)
    {
        this.myToken = myToken;
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnGAPreReaction, ReactionTiming.PRE);
    }

    public override void RemoveChain()
    {
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnGAPreReaction, ReactionTiming.PRE);
    }

    public override Trap Clone()
    {
        return new Bomb()
        {
            turnCount = this.turnCount,
            distance = this.distance,
            damage = this.damage,
        };
    }

    //Subscribels
    public void EnemysTurnGAPreReaction(EnemysTurnGA enemysTurnGA)
    {
        if (enemysTurnGA.isStartGame)
        {
            Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(myToken);
            range = TokenSystem.Instance.GetAllAroundPlaces(currentPos, distance, true, true, true);

            foreach (var r in range)
            {
                VisualGridCreator.Instance.CreateVisualGrid(myToken.gameObject.GetInstanceID(), r, "Explosion_Damage");
            }

            return;
        }

        turnCount--;
        Debug.Log($"폭발까지 남은 턴 수: {turnCount}");
        if (turnCount == 0)
        {
            var combatants = new List<CombatantView>();
            foreach (var pos in range)
            {
                CombatantView target = TokenSystem.Instance.GetTokenByPosition(pos) as CombatantView;
                if (target != null)
                {
                    combatants.Add(target);
                }
            }
            if (combatants.Count > 0)
            {
                DealDamageGA dealDamageGA = new(damage, combatants, null);
                ActionSystem.Instance.AddReaction(dealDamageGA);
            }

            TokenSystem.Instance.RemoveToken2(myToken);
            VisualGridCreator.Instance.RemoveVisualGrid(myToken.gameObject.GetInstanceID(), "Explosion_Damage");
        }
    }
}