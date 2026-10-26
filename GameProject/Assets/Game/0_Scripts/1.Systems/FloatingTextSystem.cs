using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

//피해 수치·피해 면역 사유를 토큰 위로 띄우는 연출 전용 시스템 (GA 없이 직접 호출)
public class FloatingTextSystem : Singleton<FloatingTextSystem>
{
    public const string ImmuneLabel = "피해면역";   //코어 공격이 빗나가 피해를 받지 않음
    public const string DodgeLabel = "회피";        //회피 상태효과로 피해를 피함

    [SerializeField] private TextMeshPro textPrefab;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color immuneColor = Color.white;
    [SerializeField] private float startHeight = 0.5f;        //토큰 모델 기준 시작 높이
    [SerializeField] private float riseDistance = 0.8f;       //떠오르는 거리
    [SerializeField] private float riseDuration = 0.8f;
    [SerializeField] private float fadeDelay = 0.3f;          //페이드가 시작되기까지 머무는 시간
    [SerializeField] private float randomOffsetX = 0.25f;     //겹침 방지용 좌우 흩뿌림
    [SerializeField] private float punchScale = 0.3f;         //등장 시 스케일 펀치 크기

    private ObjectPool<TextMeshPro> textPool;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        textPool = new ObjectPool<TextMeshPro>(
            createFunc: () => Instantiate(textPrefab, TokenSystem.Instance.IsoWorld.transform),
            actionOnGet: text => text.gameObject.SetActive(true),
            actionOnRelease: text => text.gameObject.SetActive(false),
            actionOnDestroy: text => Destroy(text.gameObject));
    }

    public void ShowDamage(Token target, int amount)
    {
        if (amount <= 0) return;

        Show(target, amount.ToString(), damageColor);
    }

    public void ShowImmune(Token target, string label)
    {
        Show(target, label, immuneColor);
    }

    private void Show(Token target, string content, Color color)
    {
        if (target == null) return;

        TextMeshPro text = textPool.Get();
        text.SetText(content);
        color.a = 1f;
        text.color = color;

        Vector3 start = target.Model.transform.position
            + Vector3.up * startHeight
            + Vector3.right * Random.Range(-randomOffsetX, randomOffsetX);
        text.transform.position = start;
        text.transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence().SetLink(text.gameObject);
        seq.Append(text.transform.DOMoveY(start.y + riseDistance, riseDuration).SetEase(Ease.OutQuad))
           .Join(text.transform.DOPunchScale(Vector3.one * punchScale, 0.2f))
           .Insert(fadeDelay, DOTween.To(() => text.color.a, a => text.color = new Color(color.r, color.g, color.b, a), 0f, riseDuration - fadeDelay))
           .OnComplete(() => textPool.Release(text));
    }
}
