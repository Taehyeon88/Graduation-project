using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using IsoTools;

[System.Serializable]
public abstract class CustomSquence
{
    public abstract Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses);
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

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Vector2 direction = Utility.GetSignVector2(targetPoses[0] - currentPos);
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, direction, distance, duration, animationEase);
        sequence.Append(startTween);

        return sequence;
    }
}

[System.Serializable]
public class ReturnSQ : CustomSquence
{
    [SerializeField] private float duration;
    [SerializeField] private Ease animationEase;
    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, duration, animationEase);
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

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Vector2 direction = Utility.GetSignVector2(targetPoses[0] - currentPos);
        Sequence sequence = DOTween.Sequence();

        Tween readyTween = Utility.GetTween(token, currentPos, -direction, 0.1f, 0.05f, Ease.Linear);

        Vector2 newCurrentPos = currentPos - (direction * 0.1f);
        Tween startTween = Utility.GetTween(token, newCurrentPos, direction, 1f, 0.15f, Ease.OutExpo);
        sequence.Append(startTween);

        return sequence;
    }
}

public class Adj_DashSQ : CustomSquence
{
    //    2.3. 인접/돌진(넉백)
    //연출 컨셉 : 지정된 칸으로 이동하여 적을 강하게 들이받은 후, 타격의 반동으로 원래 자리로 되돌아오는 묵직한 돌진
    //DOTween 모션:
    //이동 및 충돌 : 타겟 타일로 돌진
    //Ease : Ease.Inback , 0.2초
    //설명 : 살짝 뒤로 움츠렸다가 가속하며 돌진 후 충동 순간 카메라 쉐이크
    //복귀 : 원래 위치로 복귀
    //Ease : Ease.OutQuad , 0.2초
    //설명 : 적 또는 벽에 부딪힌 반동으로 자연스럽게 튕겨져 나오듯 복귀
    //시각 효과 : 충돌 지점에 강한 타격 및 먼지 발생
    //사용 에셋(적 피격) : CFXR3 Hit Misc A
    //사용 에셋(벽 충돌) : CFXR3 Hit Misc F Smoke
    //사운드 효과:
    //돌진하여 부딪히는 마찰음 : 쾅!
    //반동으로 밀려나는 소리 : 스윽-

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Vector2 direction = Utility.GetSignVector2(targetPoses[0] - currentPos);
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, direction, 0.5f, 0.2f, Ease.InBack);
        sequence.Append(startTween);

        return sequence;
    }

}

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

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Vector2 direction = Utility.GetSignVector2(targetPoses[1] - currentPos);
        Sequence sequence = DOTween.Sequence();
        Tween startTween = Utility.GetTween(token, currentPos, direction, 0.3f, 0.2f, Ease.OutBack);
        sequence.Append(startTween);

        return sequence;
    }
}

public class Adj_AllAround : CustomSquence
{
    //연출 컨셉 : 제자리에 순간적으로 몸이 납작하게 웅크렸다가(찌부) 탄력 있게 튕겨져 나오며 주변 8방향에 동시다발적으로 타격
    //캐릭터 DOTween 모션:
    //압축 및 타격 : 캐릭터의 스케일을 순간적으로 납작하게 찌끄러뜨림(Y축 : 0.7, X축 : 1.2) 찌그러짐이 완료되는 즉시 데미지와 이펙트가 발생
    //Ease : Ease.OutExpo , 0.1초
    //설명 : 눈 깜짝할 새 순식간에 찌부됨
    //복귀 : 찌그러졌던 몸이 탄성 있게 원래 스케일로 튕겨 오르며 복귀
    //Ease : Ease.OutElastic , 0.3초
    //설명 : 띠용- 하고 탄력적으로 튕겨져 나오는 느낌
    //시각 효과 : 캐릭터가 가장 납작해진 순간, 캐릭터 중심에서부터 8방향으로 거센 충격파가 터져 나감
    //사용 에셋 : CFXR3 Hit Misc A
    //사운드 효과:
    //순간적으로 눌린듯한 소리 : 읏-
    //직후 튕겨져 나가는 폭발적인 방출음 : 파앗!

    [SerializeField] private bool IsStart = true;
    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Sequence sequence = DOTween.Sequence();

        if (IsStart)
        {
            Tween tween = token.TokenModel.transform.DOScale(new Vector3(1.2f, 0.7f, 1f), 0.1f)
                      .SetEase(Ease.OutExpo);
            sequence.Append(tween);
        }
        else
        {
            Tween tween = token.TokenModel.transform.DOScale(Vector3.one * 2, 0.3f)
                    .SetEase(Ease.OutElastic);
            sequence.Append(tween);
        }
        return sequence;
    }
}

public class Prj_SingleSQ : CustomSquence
{
    //연출 컨셉 : 가벼운 물리 투사체 또는 마법 투사체를 빠르고 정확하게 곡사로 던짐
    //캐릭터 DOTween 모션:
    //반동 : 투사체 발사와 동시에 타겟 반대 방향으로 0.2칸 밀려남
    //Ease : Ease.OutCubic , 0.1초
    //설명 : 빠르게 밀려남
    //복귀 : 원래 위치로 복귀
    //Ease : Ease.InOutSine , 0.2초
    //설명 : 부드럽게 돌아옴
    //투사체 DOTween 모션:
    //비행 : 시전자 위치에서 생성되어 타겟 타일까지 얕은 포물선을 그리며 날아감
    //매개변수:
    //점프력 0.5~1.0
    //점프 횟수 1
    //Ease : Ease.Linear 0.3초
    //설명 : 날아가는 속도는 일정
    //시각 효과 : 발사체가 불꽃이나 잔상을 남기며 포물선으로 날아감, 적중 시 스파크나 작은 폭팔을 일으킴
    //비행 이펙트(마법) : CFXR Fire Breath
    //명중 이펙트(화살/물리) : CFXR3 Hit Misc A
    //명중 이펙트(마법 단일) : CFXR3 Fire Explosion B
    //사운드 효과:
    //가벼운 투척음 : 핑-
    //꽂히는 소리 : 푹!
    //마법 피격 소리 : 펑-

    //[SerializeField] private Sprite projectionSprite;
    [SerializeField] private GameObject flyVFX;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject hitVFX2;


    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Vector2 direction = Utility.GetSignVector2(currentPos - targetPoses[0]);
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, direction, 0.2f, 0.1f, Ease.OutCubic);
        Tween projectionTween = Utility.GetBezierTween(
            HeroVisualEffectSystem.Instance.ProjectionView.IsoObject,
            new(currentPos.x, currentPos.y, 1),
            new(targetPoses[0].x, targetPoses[0].y, 1),
            0.3f,
            Ease.Linear
            );

        projectionTween.OnStart(() =>
        {
            HeroVisualEffectSystem.Instance.ProjectionView.StartFly(null, flyVFX);
            //투척 사운드 실행 SoundSystem.PlaySound();
        });
        projectionTween.OnComplete(() =>
        {
            var endPos = HeroVisualEffectSystem.Instance.ProjectionView.EndFly();
            // 꽂히는 사운드 실행 SoundSystem.PlaySound();
            HeroVisualEffectSystem.Instance.PlayVFX(hitVFX, endPos);
            HeroVisualEffectSystem.Instance.PlayVFX(hitVFX2, endPos);
        });

        sequence.Append(startTween).Join(projectionTween);

        return sequence;
    }
}

public class Prj_PenetrationSQ : CustomSquence
{
    //투사 / 관통
    //연출 컨셉 : 묵직한 투사체를 발사하여 강력한 반동을 받고, 투사체는 직선상의 적들을 뚫어버리는 타격
    //캐릭터 DOTween 모션:
    //반동: 발사와 동시에 타겟 반대 방향으로 0.4칸 크게 밀려남. 이와 동시에 짧은 덜컹거림
    //Ease : Ease.OutExpo , 0.15초
    //설명 : 폭발적인 반동
    //복귀 : 원래 위치로 복귀
    //Ease : Ease.Inquad , 0.3초
    //설명 : 밀려난 후 무게감 있게 돌아옴
    //투사체 DOTween 모션:
    //비행: 포물선 없이 타겍 방향으로 직선으로 사거리 끝까지 빠르게 관통
    //Ease : Ease.Linear , 0.2초
    //설명 : 처음 쏘아질 때 매우 빠름
    //시각 효과 : 거대한 에너지 줄기가 직선을 관통하며 지나가고, 꿰뚫린 적들의 위치에서 연쇄적으로 피격 파티클이 발생
    //비행 이펙트 (마법) : CFXR Fire Breath
    //명중 이펙트 (마법) : CFXR3 Fire Explosion B
    //명중 이펙트 (화살) : CFXR3 Hit Misc A
    //사운드 효과:
    //억눌린 폭발음 : 쿠앙!
    //연쇄 파열음 : 퍼억 -, 파악 -

    [SerializeField] private GameObject flyVFX;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject hitVFX2;

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Vector2 direction = Utility.GetSignVector2(currentPos - targetPoses[0]);
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, direction, 0.4f, 0.15f, Ease.OutExpo);
        Tween projectionTween = Utility.GetLinearTween(
            HeroVisualEffectSystem.Instance.ProjectionView.IsoObject,
            currentPos,
            targetPoses[^1],
            0.2f,
            Ease.Linear
            );

        projectionTween.OnStart(() =>
        {
            HeroVisualEffectSystem.Instance.ProjectionView.StartFly(null, flyVFX);
            //투척 사운드 실행 SoundSystem.PlaySound();
        });
        projectionTween.OnComplete(() =>
        {
            var endPos = HeroVisualEffectSystem.Instance.ProjectionView.EndFly();
            // 억눌린 폭발음 실행 SoundSystem.PlaySound();
            // 연쇄 파열음 SoundSystem.PlaySound();
            HeroVisualEffectSystem.Instance.PlayVFX(hitVFX, endPos);
            HeroVisualEffectSystem.Instance.PlayVFX(hitVFX2, endPos);
        });

        sequence.Append(startTween).Join(projectionTween);

        return sequence;
    }
}


public class Prj_ExplosionSQ : CustomSquence
{
    //연출 컨셉 : 무겁고 큰 투사체를 강하게 하늘 높이 던져 올려서 목표 지점에 떨어뜨리는 타격
    //캐릭터 DOTween 모션:
    //반동 : 온몸에 힘을 실어 던지듯 위아래로 살짝 찌그러지며 뒤로 크게 0.5칸 밀려남
    //Ease : Ease.OutBack , 0.2초
    //복귀 : 원래 위치로 복귀
    //Ease : Ease.InOutSine , 0.3초
    //투사체 DOTween 모션:
    //비행 : 매우 높고 무거운 포물선을 그리며 타겟에 떨어짐
    //매개변수:
    //점프력 : 2.5~3.0
    //점프 횟수 1회
    //Ease : Ease.InExpo , 0.5~0.6초
    //설명 : 처음엔 붕뜨고 떨어질 때 중력에 의해 급가속되는 느낌
    //시각 효과 : 무서운 투사체가 타겟에 떨어지는 순간, 화면을 덮을 정도의 짙은 연기와 거대한 화염이 수직으로 솟구침
    //비행 이펙트 : CFXR Fire Breath(스케일을 크게 키움)
    //명중 폭발 : CFXR Explosion Smoke 2 Solo
    //사운드 효과:
    //묵직한 투척음과 거대한 폭발 굉음 : 쾅아아앙!

    [SerializeField] private GameObject flyVFX;
    [SerializeField] private GameObject hitVFX;

    public override Sequence GetCustomSquence(Token token, Vector2Int currentPos, List<Vector2Int> targetPoses)
    {
        Vector2 direction = Utility.GetSignVector2(currentPos - targetPoses[0]);
        Sequence sequence = DOTween.Sequence();

        Tween startTween = Utility.GetTween(token, currentPos, direction, 0.5f, 0.2f, Ease.OutBack);
        Tween projectionTween = Utility.GetBezierTween(
            HeroVisualEffectSystem.Instance.ProjectionView.IsoObject,
            new(currentPos.x, currentPos.y, 1),
            new(targetPoses[0].x, targetPoses[0].y, 1),
            0.5f,
            Ease.InExpo,
            2
            );

        projectionTween.OnStart(() =>
        {
            HeroVisualEffectSystem.Instance.ProjectionView.StartFly(null, flyVFX, 3);
            //투척 사운드 실행 SoundSystem.PlaySound();
        });
        projectionTween.OnComplete(() =>
        {
            var endPos = HeroVisualEffectSystem.Instance.ProjectionView.EndFly();
            // 묵직한 투척음과 거대한 폭발 굉음 실행 SoundSystem.PlaySound();
            HeroVisualEffectSystem.Instance.PlayVFX(hitVFX, endPos);
        });

        sequence.Append(startTween).Join(projectionTween);

        return sequence;
    }
}