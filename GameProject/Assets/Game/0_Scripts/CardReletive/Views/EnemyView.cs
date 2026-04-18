using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IsoTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CombatantView
{
    [SerializeField] private Transform EnemyInfoUITrans;
    [SerializeField] private StatusEffectsUI statusEffectsUI;
    [SerializeField] private Image nextActUIImage;

    public string EnemyName { get; private set; }         //적 이름
    public Sprite EnemySprite { get; private set; }       //적 이미지
    public Enemy Enemy { get; private set; }              //적 모델
    public List<EnemyAction> Actions { get; private set; }//적 행동들 
    public List<Vector2Int> NextMovePath { get; set; }    //다음 이동할 경로
    public EnemyAction NextAction                         //다음에 할 행동
    {
        get {  return nextAction; }
        private set
        {
            nextAction = value;
            UpdateNextActionUI();
        }
    }
    private EnemyAction nextAction;

    //상태효과 - NEW개볌 변수
    private bool isEnemysTurn = false;
    private Dictionary<StatusEffectType, (int, Sprite, float[])> newStatusEffectUIs = new();


    public void SetUp(EnemyData enemyData)
    {
        //Isometric 설정
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        //Enemy 데이터 설정
        EnemyName = enemyData.Name;
        EnemySprite = enemyData.Sprite;
        Enemy = enemyData.Enemy.Clone();
        Actions = enemyData.EnemyActions
                 .Select(e => e.Clone())
                 .ToList();
        SetUpBase(enemyData.Health, enemyData, isObject);
        TokenModel.Sprite = enemyData.Sprite;
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

    //Publics
    public void UpdateNextActionUI()
    {
        if (nextActUIImage != null)
        {
            nextActUIImage.sprite = NextAction.Icon;
        }
    }
    public void SetNextAction(EnemyAction action)
    {
        NextAction = action;
    }
    public void SetEnemyInfoUIActive(bool active)
    {
        EnemyInfoUITrans.gameObject.SetActive(active);
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
        statusEffectsUI.UpdateStatusEffect(type, GetStatusEffectStacks(type), sprite);
    }

    public override void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        base.RemoveStatusEffect(type, stackCount);
        statusEffectsUI.UpdateStatusEffect(type, GetStatusEffectStacks(type));
    }
}
