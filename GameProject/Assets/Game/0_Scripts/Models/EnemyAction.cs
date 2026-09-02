using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EnemyAction
{
    public List<Vector2Int> targetRange { get; set; }  //Enemy에서 PlayEnemyAction 실행 전에 받음 
    public Vector2Int targetPosition { get; set; }  //Enemy에서 PlayEnemyAction 실행 전에 받음 

    public abstract Sprite Icon { get; protected set; }
    public abstract string Description { get; protected set; }
    public abstract string TextInfo { get; protected set; }

    public abstract Sequence PlayEnemyAction(EnemyView enemy);
    public abstract EnemyAction Clone();  //복사 함수
}
