using DG.Tweening;
using UnityEngine;

public abstract class CustomSquence
{
    public abstract Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2Int dirrection);
}

[System.Serializable]
public class Adj_SingleSQ : CustomSquence
{
    //연출 컨셉 : 타겟을 향해 순간적으로 튀어나가 부딪히는 간결하고 묵직한 박치기
    //DOTween 모션:
    //돌진 : 타겟 방향으로 0.5칸 이동
    //Ease : Ease.OutBack , 0.15초
    //설명 : 순간적으로 튀어나가며 타격 지점에서 멈칫하는 묵직함 표현
    //복귀 : 원래 위치로 복귀
    //Ease : Ease.InOutSine , 0.2초
    //시각 효과 : 충돌 지점에 물리 피격 이펙트 발생.역경직 0.1초
    //사용 에셋 : CFXR3 Hit Misc A
    //사운드 효과:
    //돌직 시 짧은 옷깃 스치는 소리 : 슉!
    //충돌 시 둔탁한 타격음 : 퍽!

    [Header("ReadyInfo")]
    [SerializeField] private float duration;
    [SerializeField] private float distance;
    [SerializeField] private Ease animationEase;

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2Int dirrection)
    {
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, dirrection, distance, duration, animationEase);
        sequence.Append(startTween);

        return sequence;
    }
}