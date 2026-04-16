using DG.Tweening;
using UnityEngine;

[System.Serializable]
public abstract class CustomSquence
{
    public abstract Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2 dirrection);
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

    [SerializeField] private float duration;
    [SerializeField] private float distance;
    [SerializeField] private Ease animationEase;

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2 dirrection)
    {
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, dirrection, distance, duration, animationEase);
        sequence.Append(startTween);

        return sequence;
    }
}

[System.Serializable]
public class ReturnSQ : CustomSquence
{
    [SerializeField] private float duration;
    [SerializeField] private Ease animationEase;
    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2 dirrection)
    {
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, dirrection, 0, duration, animationEase);
        sequence.Append(startTween);

        return sequence;
    }
}

[System.Serializable]
public class Adj_PenetrationSQ : CustomSquence
{
    //연출 컨셉 : 몸 전체를 창처럼 일직선으로 깊게 찔러 넣는 박치기
    //DOTween 모션:
    //준비: 공격 반대 방향으로 아주 살짝 (0.1칸) 뒤로 이동
    //돌진 : 타겟 방향으로 1칸 가까이 깊게 전진
    //Ease : Ease.OutExpo , 0.15초
    //복귀 : 원래 위치로 복귀 
    //Ease : Ease.Inquad , 0.25초
    //시각 효과 : 캐릭터 몸체나 무기 쪽에 긴 트레일 적용 및 관통되는 타일마다 피격 이펙트
    //사용 에셋 : CFXR3 Hit Misc A
    //사운드 효과:
    //몸을 당길 때 : 스으읍
    //연쇄 타격음 : 챙 -

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2 dirrection)
    {
        Sequence sequence = DOTween.Sequence();

        Tween readyTween = Utility.GetTween(token, currentPos, - dirrection, 0.1f, 0.05f, Ease.Unset);

        Vector2 newCurrentPos = currentPos - (dirrection * 0.1f);
        Tween startTween = Utility.GetTween(token, newCurrentPos, dirrection, 1f, 0.15f, Ease.OutExpo);
        sequence.Append(startTween);

        return sequence;
    }
}

//public class Adj_DashSQ : CustomSquence
//{
//    //    2.3. 인접/돌진(넉백)
//    //연출 컨셉 : 지정된 칸으로 이동하여 적을 강하게 들이받은 후, 타격의 반동으로 원래 자리로 되돌아오는 묵직한 돌진
//    //DOTween 모션:
//    //이동 및 충돌 : 타겟 타일로 돌진
//    //Ease : Ease.Inback , 0.2초
//    //설명 : 살짝 뒤로 움츠렸다가 가속하며 돌진 후 충동 순간 카메라 쉐이크
//    //복귀 : 원래 위치로 복귀
//    //Ease : Ease.OutQuad , 0.2초
//    //설명 : 적 또는 벽에 부딪힌 반동으로 자연스럽게 튕겨져 나오듯 복귀
//    //시각 효과 : 충돌 지점에 강한 타격 및 먼지 발생
//    //사용 에셋(적 피격) : CFXR3 Hit Misc A
//    //사용 에셋(벽 충돌) : CFXR3 Hit Misc F Smoke
//    //사운드 효과:
//    //돌진하여 부딪히는 마찰음 : 쾅!
//    //반동으로 밀려나는 소리 : 스윽-

//    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2 dirrection)
//    {
//        Sequence sequence = DOTween.Sequence();

//        Vector2 newCurrentPos = currentPos - (dirrection * 0.1f);
//        Tween startTween = Utility.GetTween(token, newCurrentPos, dirrection, 1f, 0.15f, Ease.InBack);
//        sequence.Append(startTween);

//        return sequence;
//    }

//}

[System.Serializable]
public class Adj_Cleave : CustomSquence
{
    //2.4.인접 / 횡베기
    //연출 컨셉 : 선택한 한 방향으로 파고들며, 어깨로 넓은 범위를 거칠게 밀어내는 타격
    //DOTween 모션:
    //휩쓸기: 선택한 방향으로 살짝(0.3칸) 전진함과 동시에, 공격 방향의 수직 (직각) 축으로 짧고 강하게 왕복 진동을 주어 넓은 범위를 몸통으로 부딪히는 느낌을 표현
    //Ease : Ease.OutBack 0.2초
    //설명 : 타격 순간 힘이 실려 튕기는 느낌
    //복귀 : 진동이 끝나면 윈래 위치로 복귀
    //시각 효과 : 선택한 방향에 맞춰 가로로 넓게 긁고 지나가는 듯한 이펙트 연출
    //사용 에셋 : CFXR3 Hit Misc A
    //해당 범위 내 3칸 내 적중한  적마다 동시 발생
    //사운드 효과:
    //전방을 가로로 넓게 휩쓰는 예리하고 묵직한 마찰음 : 샤악 -, 쾅!

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, Vector2 dirrection)
    {
        Sequence sequence = DOTween.Sequence();
        Tween startTween = Utility.GetTween(token, currentPos, dirrection, 0.3f, 0.2f, Ease.OutBack);
        sequence.Append(startTween);

        return sequence;
    }
}