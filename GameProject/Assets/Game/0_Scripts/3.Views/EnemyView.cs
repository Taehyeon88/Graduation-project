using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IsoTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CombatantView
{
    [SerializeField] private Image nextActUIImage;
    [SerializeField] private TMP_Text nextActText;

    public int Id => TokenData.Id;
    public string EnemyName => TokenData.Name;            //적 이름
    public Sprite EnemySprite => TokenData.Sprite;        //적 이미지     
    public Enemy Enemy { get; private set; }              //적 모델
    public List<EnemyAction> Actions { get; private set; }//적 행동들 
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
    private Dictionary<StatusEffectType, (int, Sprite)> newStatusEffectUIs = new();


    public void SetUp(EnemyData enemyData)
    {
        //Isometric 설정
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        //Enemy 데이터 설정
        Enemy = enemyData.Enemy.Clone();
        Actions = enemyData.EnemyActions
                 .Select(e => e.Clone())
                 .ToList();
        SetUpBase(enemyData.Health, enemyData.Health, enemyData.MovePoint, enemyData, isObject);
    }

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<TurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }

    //Publics
    private void UpdateNextActionUI()
    {
        if (nextActUIImage != null)
            nextActUIImage.sprite = NextAction.Icon;

        if (nextActText != null)
        {
            if (!string.IsNullOrEmpty(NextAction.TextInfo))
                nextActText.text = NextAction.TextInfo;
        }
    }
    public void SetNextAction(EnemyAction action)
    {
        NextAction = action;
    }

    //Subscribers
    private void EnemysTurnPostReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Enemy) return;

        foreach (var seUI in newStatusEffectUIs)
            AddStatusEffect(seUI.Key, seUI.Value.Item1, seUI.Value.Item2);
        newStatusEffectUIs.Clear();
    }

    //overrides
    public override void AddStatusEffect(StatusEffectType type, int stackCount, Sprite sprite)
    {
        if (TurnSystem.Instance.CurrentTurn == TurnType.Enemy)
        {
            if (!newStatusEffectUIs.ContainsKey(type) && !statusEffectUIs.ContainsKey(type))
                newStatusEffectUIs.Add(type, (1, sprite));
        }
        base.AddStatusEffect(type, stackCount, sprite);
    }

    public override void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        base.RemoveStatusEffect(type, stackCount);
    }
}
