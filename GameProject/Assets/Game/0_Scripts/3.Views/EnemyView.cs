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
    [SerializeField] private TMP_Text movePointText;

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

    public int MovePoint { get; private set; }

    public int CurrentMovePoint
    {
        get { return currentMovePoint; }
        private set
        {
            currentMovePoint = value;
            UpdateMovePointUI();
        }
    }
    private int currentMovePoint;

    public void SetUp(EnemyData enemyData)
    {
        //Enemy 데이터 설정
        Enemy = enemyData.Enemy.Clone();
        Actions = enemyData.EnemyActions
                 .Select(e => e.Clone())
                 .ToList();

        CurrentMovePoint = MovePoint = enemyData.MovePoint;

        SetUpBase(enemyData.Health, enemyData.Health, enemyData);
    }

    //Privates
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

    private void UpdateMovePointUI()
    {
        if (movePointText != null)
        {
            movePointText.SetText(CurrentMovePoint.ToString());
        }
    }

    //Publics

    //MP API는 몬스터만 사용
    public void ResetMovePoint()
    {
        CurrentMovePoint = MovePoint;
    }

    public bool HasEnoughMovePoint(int movePoint)
    {
        return CurrentMovePoint >= movePoint;
    }

    public void SpendMovePoint(int movePoint)
    {
        CurrentMovePoint -= movePoint;
    }
    public void SetNextAction(EnemyAction action)
    {
        NextAction = action;
    }
}
