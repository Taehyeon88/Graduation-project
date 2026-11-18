using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//튜토리얼 화면 층: 안내 상자(위쪽 가운데), 스포트라이트, 고스트 시연
public class TutorialOverlayUI : Singleton<TutorialOverlayUI>   //GAMEMASTER 하위에서 씬을 넘나듦
{
    private const string BattlefieldTargetId = "battlefield";   //판 전체 (칸 전체 + 용병대장 + 적 깃발)
    private const string StageTargetPrefix = "@stage";        //"@stage1", "@stage2" … 스테이지 선택 씬의 구조물
    private const string DaggerTargetId = "dagger";
    private const float GhostWaitLimit = 3f;            //시연 대상(카드 등)이 나타나길 기다리는 최대 시간
    private const float GhostSettleTime = 0.2f;         //시연 대상이 이 시간 동안 움직이지 않으면 자리 잡은 것으로 봄

    [Header("Element")]
    [SerializeField] private TutorialSpotlightGraphic spotlight;
    [SerializeField] private CanvasGroup messageBox;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Image ghost;

    [Header("Direct Element")]
    [SerializeField] private float holePadding = 12f;
    [SerializeField] private float dimFadeSpeed = 4f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float messageFadeDuration = 0.2f;
    [SerializeField] private float ghostMoveDuration = 0.9f;
    [SerializeField] private float ghostAlpha = 0.6f;

    private readonly List<Rect> interactiveRects = new();   //눌러야 하는 대상 (테두리 빛 적용)
    private readonly List<Rect> plainRects = new();         //설명용 대상 (어둡게만 뚫고 테두리 없음)
    private TutorialStepData currentStep;
    private Rect? battlefieldRectCache;   //판 전체 사각형 (단계마다 한 번 계산)
    private float dimAmount;
    private Sequence ghostSequence;
    private Coroutine ghostRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        messageBox.alpha = 0f;
        messageBox.blocksRaycasts = false;
        ghost.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (Instance != this) return;

        TutorialSystem.StepStarted += OnStepStarted;
        TutorialSystem.StepCompleted += OnStepCompleted;
        TutorialSystem.WrongActionPerformed += OnWrongAction;
        TutorialSystem.TutorialEnded += OnTutorialEnded;
        nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        TutorialSystem.StepStarted -= OnStepStarted;
        TutorialSystem.StepCompleted -= OnStepCompleted;
        TutorialSystem.WrongActionPerformed -= OnWrongAction;
        TutorialSystem.TutorialEnded -= OnTutorialEnded;
        nextButton.onClick.RemoveListener(OnNextClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopGhost();
    }

    private void LateUpdate()
    {
        if (Instance != this) return;

        bool active = TutorialSystem.Instance != null && TutorialSystem.Instance.IsActive;
        if (!active && dimAmount <= 0f)
        {
            spotlight.BlockInput = false;
            return;
        }

        bool guided = TutorialSystem.IsGuided;
        spotlight.BlockInput = guided;   //단계 사이에 비는 순간에도 입력 잠금 유지 (어둡게만 하지 않음)

        float targetDim = active && guided && currentStep != null ? 1f : 0f;
        dimAmount = Mathf.MoveTowards(dimAmount, targetDim, dimFadeSpeed * Time.unscaledDeltaTime);

        interactiveRects.Clear();
        plainRects.Clear();
        if (guided && currentStep != null)
            CollectTargetRects(currentStep, interactiveRects, plainRects);

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
        spotlight.Apply(interactiveRects, plainRects, GetMessageBoxRect(), dimAmount, pulse);
    }

    //안내 상자의 화면 사각형 (숨겨진 상태면 빈 사각형) — 이 영역 안에는 테두리 빛을 그리지 않음
    private Rect GetMessageBoxRect()
    {
        if (messageBox.alpha <= 0.01f) return Rect.zero;

        Vector2 size = GetScreenSize((RectTransform)messageBox.transform);
        Vector3 center = messageBox.transform.TransformPoint(((RectTransform)messageBox.transform).rect.center);
        return new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
    }

    //Events
    private void OnStepStarted(TutorialStepData step)
    {
        currentStep = step;
        battlefieldRectCache = null;
        messageText.SetText(step.Message);
        nextButton.gameObject.SetActive(step.Advance == TutorialAdvance.NextButton);
        messageBox.blocksRaycasts = true;
        messageBox.DOKill();
        messageBox.DOFade(1f, messageFadeDuration);

        StartGhost(step);
    }

    private void OnStepCompleted(TutorialStepData step)
    {
        currentStep = null;
        HideMessageBox();
        StopGhost();
    }

    private void OnWrongAction(TutorialStepData step)
    {
        StartGhost(step);   //조건과 다른 행동 뒤에 시연을 처음부터 다시 재생
    }

    private void OnTutorialEnded()
    {
        currentStep = null;
        HideMessageBox();
        StopGhost();
    }

    private void HideMessageBox()
    {
        messageBox.blocksRaycasts = false;
        messageBox.DOKill();
        messageBox.DOFade(0f, messageFadeDuration);
    }

    private void OnNextClicked()
    {
        if (TutorialSystem.Instance != null)
            TutorialSystem.Instance.Next();
    }

    //Target rects (화면 좌표)
    //interactive: 눌러야 하는 대상(버튼·카드·행동 칸 등, 테두리 빛), plain: 설명용 대상(판·체력·유닛 등, 테두리 없음)
    private void CollectTargetRects(TutorialStepData step, List<Rect> interactive, List<Rect> plain)
    {
        if (step.TargetIds != null)
        {
            foreach (string id in step.TargetIds)
            {
                if (id == "@context")
                {
                    foreach (Token token in TutorialSystem.Instance.ContextTokens)
                    {
                        if (token != null && TryGetTokenRect(token, out Rect tokenRect)) plain.Add(Pad(tokenRect));
                    }
                }
                else if (id.StartsWith(StageTargetPrefix))
                {
                    //스테이지 선택 씬의 구조물 ("@stage2" = 스테이지 2)
                    if (int.TryParse(id.Substring(StageTargetPrefix.Length), out int stageNumber)
                        && StageSelectSystem.Instance != null
                        && StageSelectSystem.Instance.TryGetStructureView(stageNumber, out StructureView structure)
                        && TryGetBoundsRect(structure.ModelBounds, out Rect structureRect))
                        (step.Advance == TutorialAdvance.StageClick ? interactive : plain).Add(Pad(structureRect));   //클릭해야 하는 스테이지만 테두리
                }
                else if (id == BattlefieldTargetId)
                {
                    if (TryGetBattlefieldRect(out Rect battlefield)) plain.Add(Pad(battlefield));
                }
                else if (TutorialTarget.TryGet(id, out TutorialTarget target) && TryGetTransformRect(target.transform, out Rect rect))
                {
                    (IsInteractive(target.transform) ? interactive : plain).Add(Pad(rect));
                }
            }
        }

        if (step.CardHero != null)
        {
            CardView cardView = FindCardView(step.CardHero);
            if (cardView != null && TryGetTransformRect(cardView.transform, out Rect cardRect))
                (step.Advance == TutorialAdvance.PlaceCard ? interactive : plain).Add(Pad(cardRect));
        }

        if (step.HeroTarget != null && TokenSystem.Instance != null)
        {
            foreach (HeroView heroView in TokenSystem.Instance.HeroViews)
            {
                if (heroView != null && heroView.Hero.Data == step.HeroTarget && TryGetTokenRect(heroView, out Rect heroRect))
                    plain.Add(Pad(heroRect));
            }
        }

        bool actionCell = step.Advance == TutorialAdvance.PlaceCard || step.Advance == TutorialAdvance.UseDagger;
        if ((actionCell || step.ShowCell) && TryGetCellRect(step.CellPos, out Rect cellRect))
            (actionCell ? interactive : plain).Add(Pad(cellRect));
    }

    //대상 자신에게 클릭·드래그를 받는 컴포넌트(Button, CardView, UseSkillUI 등)가 있으면 눌러야 하는 대상 (자식은 보지 않음: 트리처럼 자식에 버튼이 있는 설명용 영역 제외)
    private static bool IsInteractive(Transform target)
    {
        foreach (MonoBehaviour behaviour in target.GetComponents<MonoBehaviour>())
        {
            if (behaviour is IPointerClickHandler || behaviour is IBeginDragHandler) return true;
        }
        return false;
    }

    private Rect Pad(Rect rect)
    {
        rect.xMin -= holePadding;
        rect.yMin -= holePadding;
        rect.xMax += holePadding;
        rect.yMax += holePadding;
        return rect;
    }

    private bool TryGetTransformRect(Transform target, out Rect rect)
    {
        rect = default;
        if (target is RectTransform rectTransform)
        {
            if (rectTransform.GetComponent<HandView>() != null)   //손패 영역은 크기가 없으므로 카드들의 합집합
                return TryGetHandRect(rectTransform, out rect);

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);   //Overlay 캔버스라 월드 좌표 = 화면 좌표
            rect = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
            return true;
        }

        Token token = target.GetComponent<Token>();
        if (token != null) return TryGetTokenRect(token, out rect);

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return false;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return TryGetBoundsRect(bounds, out rect);
    }

    private bool TryGetHandRect(RectTransform hand, out Rect rect)
    {
        rect = default;
        bool found = false;
        Vector3[] corners = new Vector3[4];
        foreach (RectTransform card in hand)
        {
            if (!card.gameObject.activeInHierarchy) continue;

            card.GetWorldCorners(corners);
            Rect cardRect = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
            if (!found) rect = cardRect;
            else
            {
                rect.xMin = Mathf.Min(rect.xMin, cardRect.xMin);
                rect.yMin = Mathf.Min(rect.yMin, cardRect.yMin);
                rect.xMax = Mathf.Max(rect.xMax, cardRect.xMax);
                rect.yMax = Mathf.Max(rect.yMax, cardRect.yMax);
            }
            found = true;
        }
        return found;
    }

    private bool TryGetTokenRect(Token token, out Rect rect)
    {
        rect = default;
        return token.Model != null && TryGetBoundsRect(token.Model.bounds, out rect);
    }

    private bool TryGetBoundsRect(Bounds bounds, out Rect rect)
    {
        rect = default;
        if (Camera.main == null) return false;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector2 first = Camera.main.WorldToScreenPoint(new Vector3(min.x, min.y, min.z));
        float xMin = first.x, xMax = first.x, yMin = first.y, yMax = first.y;
        for (int i = 1; i < 8; i++)
        {
            Vector3 corner = new((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
            Vector2 screen = Camera.main.WorldToScreenPoint(corner);
            xMin = Mathf.Min(xMin, screen.x);
            xMax = Mathf.Max(xMax, screen.x);
            yMin = Mathf.Min(yMin, screen.y);
            yMax = Mathf.Max(yMax, screen.y);
        }
        rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        return true;
    }

    //판의 한 칸 (등각 투영이라 네 모서리를 화면 좌표로 바꾼 뒤 감싸는 사각형)
    private bool TryGetCellRect(Vector2Int cell, out Rect rect)
    {
        rect = default;
        if (TokenSystem.Instance == null || TokenSystem.Instance.IsoWorld == null) return false;

        if (Camera.main == null) return false;

        //IsoToScreen은 화면 픽셀이 아니라 월드 좌표를 돌려주므로 카메라로 한 번 더 화면 좌표로 바꿈 (칸 판정 평면 iso z = 1)
        IsoTools.IsoWorld world = TokenSystem.Instance.IsoWorld;
        Vector2 a = IsoCornerToScreen(world, cell.x, cell.y);
        Vector2 b = IsoCornerToScreen(world, cell.x + 1, cell.y);
        Vector2 c = IsoCornerToScreen(world, cell.x, cell.y + 1);
        Vector2 d = IsoCornerToScreen(world, cell.x + 1, cell.y + 1);
        rect = Rect.MinMaxRect(
            Mathf.Min(a.x, b.x, c.x, d.x), Mathf.Min(a.y, b.y, c.y, d.y),
            Mathf.Max(a.x, b.x, c.x, d.x), Mathf.Max(a.y, b.y, c.y, d.y));
        return true;
    }

    //판 전체: 판 월드(IsoWorld) 아래에 보이는 모든 렌더러(칸·옆면 발판·용병대장/깃발 자리·유닛)를 감싸는 사각형 — 칸만 계산하면 보이는 판보다 작아짐
    //렌더러가 800여 개라 단계가 시작될 때 한 번만 계산해 둠 (판은 단계 동안 움직이지 않음)
    private bool TryGetBattlefieldRect(out Rect rect)
    {
        rect = default;
        if (battlefieldRectCache.HasValue)
        {
            rect = battlefieldRectCache.Value;
            return true;
        }
        if (TokenSystem.Instance == null || TokenSystem.Instance.IsoWorld == null || Camera.main == null) return false;

        bool found = false;
        Bounds bounds = default;
        foreach (Renderer renderer in TokenSystem.Instance.IsoWorld.GetComponentsInChildren<Renderer>())
        {
            if (!renderer.enabled) continue;

            if (!found) bounds = renderer.bounds;
            else bounds.Encapsulate(renderer.bounds);
            found = true;
        }
        if (!found || !TryGetBoundsRect(bounds, out rect)) return false;

        battlefieldRectCache = rect;
        return true;
    }

    private Vector2 IsoCornerToScreen(IsoTools.IsoWorld world, int x, int y)
    {
        Vector2 worldPos = world.IsoToScreen(new Vector3(x, y, 1f));
        return Camera.main.WorldToScreenPoint(new Vector3(worldPos.x, worldPos.y, 0f));
    }

    private CardView FindCardView(HeroData hero)
    {
        foreach (CardView view in FindObjectsByType<CardView>(FindObjectsSortMode.None))
        {
            if (view.Card != null && view.Card.Unit.Data == hero) return view;
        }
        return null;
    }

    //Ghost (반투명 카드·단도가 시작점에서 목표 칸으로 미끄러지는 시연)
    private void StartGhost(TutorialStepData step)
    {
        StopGhost();
        if (!step.ShowGhost) return;

        ghostRoutine = StartCoroutine(GhostRoutine(step));
    }

    private void StopGhost()
    {
        if (ghostRoutine != null) StopCoroutine(ghostRoutine);
        ghostRoutine = null;
        ghostSequence?.Kill();
        ghostSequence = null;
        if (ghost != null) ghost.gameObject.SetActive(false);
    }

    private IEnumerator GhostRoutine(TutorialStepData step)
    {
        Image source = null;
        float waited = 0f;
        float stableTime = 0f;
        Vector3 lastPosition = default;

        //카드가 뽑을 카드 더미에서 손으로 날아오는 중일 수 있어, 나타나서 자리 잡을 때까지 기다림
        while (true)
        {
            source = FindGhostSource(step);
            if (source != null)
            {
                stableTime = source.transform.position == lastPosition ? stableTime + Time.unscaledDeltaTime : 0f;
                lastPosition = source.transform.position;
                if (stableTime >= GhostSettleTime) break;
            }

            waited += Time.unscaledDeltaTime;
            if (waited > GhostWaitLimit && source != null) break;   //끝까지 움직이면 마지막 위치를 사용
            if (waited > GhostWaitLimit) yield break;
            yield return null;
        }
        if (!TryGetCellRect(step.CellPos, out Rect cell)) yield break;

        Vector2 end = cell.center;
        ghost.sprite = source.sprite;
        ghost.preserveAspect = true;
        ghost.gameObject.SetActive(true);

        Color color = ghost.color;
        ghostSequence = DOTween.Sequence().SetLoops(-1);
        ghostSequence.AppendCallback(() =>
        {
            //반복할 때마다 원본의 현재 위치·크기를 다시 읽음 (원본 UI 이미지와 같은 크기로 표시)
            if (source == null) return;

            ghost.rectTransform.sizeDelta = GetScreenSize(source.rectTransform) / ghost.canvas.rootCanvas.scaleFactor;
            ghost.rectTransform.position = source.rectTransform.TransformPoint(source.rectTransform.rect.center);
            ghost.color = new Color(color.r, color.g, color.b, ghostAlpha);
        });
        ghostSequence.Append(ghost.rectTransform.DOMove(end, ghostMoveDuration).SetEase(Ease.InOutQuad));
        ghostSequence.Append(ghost.DOFade(0f, 0.3f));
        ghostSequence.AppendInterval(0.4f);
        ghostRoutine = null;
    }

    //시연의 시작점이 되는 UI 이미지 (카드는 영웅 그림, 단도는 단도 버튼 그림)
    private Image FindGhostSource(TutorialStepData step)
    {
        if (step.Advance == TutorialAdvance.PlaceCard && step.CardHero != null)
        {
            CardView cardView = FindCardView(step.CardHero);
            if (cardView == null) return null;

            foreach (Image image in cardView.GetComponentsInChildren<Image>())
            {
                if (image.sprite != null && image.sprite == cardView.Card.Image) return image;
            }
            return cardView.GetComponent<Image>();
        }

        if (step.Advance == TutorialAdvance.UseDagger && TutorialTarget.TryGet(DaggerTargetId, out TutorialTarget dagger))
            return dagger.GetComponent<Image>();
        return null;
    }

    private Vector2 GetScreenSize(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);   //Overlay 캔버스라 월드 좌표 = 화면 좌표
        return new Vector2(corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }
}
