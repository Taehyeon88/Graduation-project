using DG.Tweening;
using IsoTools;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileSystem : Singleton<ProjectileSystem>
{
    private const int ArrowHeight = 1;   //화살이 날아가는 아이소 높이 (토큰 높이와 같음)

    [SerializeField] private IsoObject arrowPrefab;
    [SerializeField] private float arrowSecondsPerTile = 0.08f;   //칸당 화살 비행 시간
    [SerializeField] private float arrowMinDuration = 0.25f;      //화살 최소 비행 시간

    private ObjectPool<IsoObject> arrowPool;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        arrowPool = new ObjectPool<IsoObject>(
            createFunc: () => Instantiate(arrowPrefab, TokenSystem.Instance.IsoWorld.transform),
            actionOnGet: arrow => SetArrowActive(arrow, true),
            actionOnRelease: arrow => SetArrowActive(arrow, false),
            actionOnDestroy: arrow => Destroy(arrow.gameObject));
    }

    public Tween PlayArrow(Vector3 start, Vector3 end, float duration, Ease ease = Ease.Unset, float heightRate = 1)
    {
        IsoObject arrow = arrowPool.Get();
        Tween tween = Utility.GetArrowBezierTween(arrow, arrow.transform.GetChild(0), start, end, duration, ease, heightRate);
        tween.OnComplete(() => arrowPool.Release(arrow));
        return tween;
    }

    //그리드 칸 사이를 날아가는 화살 (거리에 비례한 비행 시간)
    public Tween PlayArrowShot(Vector2Int from, Vector2Int to)
    {
        float duration = Mathf.Max(arrowMinDuration, Vector2Int.Distance(from, to) * arrowSecondsPerTile);
        return PlayArrow(
            Utility.Vector2IntToVector3(from, ArrowHeight),
            Utility.Vector2IntToVector3(to, ArrowHeight),
            duration,
            Ease.Linear);
    }

    private void SetArrowActive(IsoObject arrow, bool active)
    {
        arrow.gameObject.SetActive(active);
        arrow.transform.GetChild(0).gameObject.SetActive(active);
    }
}
