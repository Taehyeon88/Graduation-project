using DG.Tweening;
using IsoTools;
using UnityEngine;

//스테이지 선택 씬 전용 영웅 연출 (전투 이동과 동일하게 한 칸씩 이동). 전투 로직 의존 없음
public class StageHeroView : MonoBehaviour
{
    private const int MoveSoundId = 2;   //전투 이동(MoveSystem)과 같은 칸 이동 효과음

    [SerializeField] private IsoObject isoObject;
    [SerializeField] private float stepDuration = 0.3f;   //한 칸 이동 시간 (전투 이동과 동일)

    public void SetPosition(Vector3 isoPosition)
    {
        isoObject.position = isoPosition;
    }

    //목표까지 직선으로 한 칸씩 이동하고, 칸마다 이동 효과음을 재생
    public Sequence PlayMove(Vector3 isoPosition)
    {
        Vector2 start = isoObject.positionXY;
        Vector2 target = isoPosition;
        int stepCount = Mathf.CeilToInt(Vector2.Distance(start, target));

        Sequence sequence = DOTween.Sequence();
        for (int i = 1; i <= stepCount; i++)
        {
            Vector2 stepTarget = Vector2.Lerp(start, target, (float)i / stepCount);
            sequence.Append(DOTween.To(() => isoObject.positionXY, value => isoObject.positionXY = value, stepTarget, stepDuration));
            sequence.AppendCallback(() => SoundSystem.Instance.PlaySound(MoveSoundId));
        }
        return sequence;
    }
}
