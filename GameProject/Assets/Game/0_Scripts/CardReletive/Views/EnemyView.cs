using System.Collections;
using System.Collections.Generic;
using IsoTools;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    public string EnemyName { get; private set; }     //적 이름
    public Sprite EnemySprite { get; private set; }   //적 이미지
    public float AttackPower { get; set; }            //적 공격력 !추후수정
    public Enemy Enemy { get; private set; }          //적 모델

    public EnemyActionInfo ActionInfo { get; set; }   //몬스터 행동/이동 정보


    private bool isEnemysTurn = false;
    private Dictionary<StatusEffectType, (int, Sprite, float[])> newStatusEffectUIs = new();
    public void SetUp(EnemyData enemyData)
    {
        //Isometric 설정
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        //Enemy 데이터 설정
        EnemyName = enemyData.name;
        EnemySprite = enemyData.TokenModel.Sprite;
        AttackPower = enemyData.AttackPower;
        Enemy = enemyData.Enemy;
        SetUpBase(enemyData.Health, enemyData, isObject);
    }

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }

    //Subscribers
    private void EnemysTurnPreReaction(EnemysTurnGA enemysTurnGA) => isEnemysTurn = true;
    private void EnemysTurnPostReaction(EnemysTurnGA enemysTurnGA)
    {
        isEnemysTurn = false;
        foreach (var seUI in newStatusEffectUIs)
            AddStatusEffect(seUI.Key, seUI.Value.Item1, seUI.Value.Item2, seUI.Value.Item3);
        newStatusEffectUIs.Clear();
    }

    //overrides
    public override void AddStatusEffect(StatusEffectType type, int stackCount, Sprite sprite, float[] infoes = null)
    {
        if (isEnemysTurn)
        {
            if (!newStatusEffectUIs.ContainsKey(type) && !statusEffectUIs.ContainsKey(type))
                newStatusEffectUIs.Add(type, (1, sprite, infoes));
        }
        base.AddStatusEffect(type, stackCount, sprite, infoes);
    }

    public override void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        base.RemoveStatusEffect(type, stackCount);
    }
}
