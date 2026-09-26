using DG.Tweening;
using IsoTools;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileSystem : Singleton<ProjectileSystem>
{
    [SerializeField] private IsoObject arrowPrefab;

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

    private void SetArrowActive(IsoObject arrow, bool active)
    {
        arrow.gameObject.SetActive(active);
        arrow.transform.GetChild(0).gameObject.SetActive(active);
    }
}
