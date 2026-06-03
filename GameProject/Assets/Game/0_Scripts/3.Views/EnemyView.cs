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
    [SerializeField] private TMP_Text nextActDamageText;

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
    private bool isEnemysTurn => EnemySystem.Instance.IsEnemyTurn;
    private Dictionary<StatusEffectType, (int, Sprite, float[])> newStatusEffectUIs = new();


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
        SetUpBase(enemyData.Health, enemyData.Health, enemyData, isObject);
    }

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<KillGA>(KillGAPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<KillGA>(KillGAPreReaction, ReactionTiming.PRE);
    }

    //Publics
    public void UpdateNextActionUI()
    {
        if (nextActUIImage != null)
            nextActUIImage.sprite = NextAction.Icon;

        if (nextActDamageText != null)
        {
            int? dmg = NextAction.AttackDamage;
            nextActDamageText.gameObject.SetActive(dmg.HasValue);
            if (dmg.HasValue)
                nextActDamageText.text = dmg.Value.ToString();
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
    private void EnemysTurnPostReaction(EnemysTurnGA enemysTurnGA)
    {
        foreach (var seUI in newStatusEffectUIs)
            AddStatusEffect(seUI.Key, seUI.Value.Item1, seUI.Value.Item2, seUI.Value.Item3);
        newStatusEffectUIs.Clear();
    }
    private void MoveGAPostReaction(MoveGA moveGA)
    {
        if (!VisualGridCreator.Instance.ContainVisualGrid(gameObject.GetInstanceID(), "UI_SelectedEnemy")) return;

        Debug.Log($"{EnemyName}, {VisualGridCreator.Instance.ContainVisualGrid(gameObject.GetInstanceID(), "UI_SelectedEnemy")}");

        if (moveGA.mover == this)
        {
            VisualGridCreator.Instance.ChangeVisualGrid(
                moveGA.movePosition,
                gameObject.GetInstanceID(),
                "UI_SelectedEnemy",
                "UI_SelectedEnemy"
                );
        }
    }
    private void KillGAPreReaction(KillGA killGA)
    {
        if (killGA.Token != this) return;

        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "UI_SelectedEnemy");  //선택 그리드 비활성화
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
