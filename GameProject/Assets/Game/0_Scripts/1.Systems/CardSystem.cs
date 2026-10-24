using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CardSystem : Singleton<CardSystem>
{
    [Header("Card Element")]
    [SerializeField] private HandView handView;
    [SerializeField] private RectTransform handTransform;
    [SerializeField] private RectTransform drawPilePoint;
    [SerializeField] private RectTransform discardPilePoint;

    [Header("Target Element")]
    [SerializeField] private CardArrowView arrowView;   //카드 → 마우스 화살 (비활성 상태로 씬에 배치)

    [Header("Draw (임시 값 - 원작 확인 필요)")]
    [FormerlySerializedAs("drawAmount")]
    [SerializeField] private int startDrawAmount;   //전투 시작 시 드로우 장수

    [Header("Card Direct Element")]
    [SerializeField] private float discardDuration = 0.15f;

    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();
    private readonly Queue<PlayCardGA> reservedCards = new();
    private CardView currentSelectedCard;
    private HeroPreview heroPreview;                          //배치 미리보기 (TokenGrid 미등록)
    private Vector2Int hoveredPos = Vector2Int.down;          //마지막으로 판정한 마우스 타일
    private bool isDragMode;                                  //드래그로 배치 중 (놓을 때 확정)
    private bool dropRequested;                               //드래그 종료(버튼 놓음) 감지

    public static event System.Action PilesReset;     //SetUp으로 더미가 재구성될 때 (더미 UI 갱신용)

    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public List<Card> DrawPileCards => new(drawPile);
    public List<Card> DiscardPileCards => new(discardPile);

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsGAPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardGAPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(StartBattlePreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(StartBattlePreReaction, ReactionTiming.PRE);
    }

    private void Update()
    {
        //예약된 카드 사용이 있고 액션이 종료 되면 다음 예약 실행
        if (reservedCards.Count <= 0 || ActionSystem.Instance.IsPerforming)
            return;

        var playCardGA = reservedCards.Dequeue();
        ActionSystem.Instance.Perform(playCardGA);
    }

    //Publics
    public void SetUp()
    {
        drawPile.Clear();
        discardPile.Clear();
        hand.Clear();

        List<HeroData> deck = GameSystem.Instance.Deck;
        if (deck != null && deck.Count > 0)
        {
            //Shuffle은 입력 리스트를 비우므로 복사본 사용 (GameSystem.Deck은 보존)
            foreach (var heroData in new List<HeroData>(deck).Shuffle())
                drawPile.Add(new Card(heroData));
        }

        PilesReset?.Invoke();
    }

    //테스트용: 덱과 무관하게 지정한 영웅 카드를 손패에 추가 (에디터 윈도우에서 호출)
    public void AddHeroToHand(HeroData heroData)
    {
        StartCoroutine(AddHeroToHandRoutine(heroData));
    }

    public void PlayCardTargetMode(CardView cardView)
    {
        if (Interactions.Instance.IsHeroMoveMode)
            return;

        if (!Interactions.Instance.IsCardTargetMode)
        {
            currentSelectedCard = cardView;
            StartCoroutine(CardTargetMode());
        }
        else if (cardView != currentSelectedCard)   //타겟 모드 중, 카드 변경
        {
            ChangeSelectedCard(cardView);
        }
        else                                         //같은 카드 재클릭 = 취소
        {
            ResetSelectMode();
        }
    }

    //카드 드래그 시작: 타겟 모드 진입(이미 진행 중이면 드래그 방식으로 전환), 놓는 위치에 배치
    public void BeginCardDrag(CardView cardView)
    {
        if (Interactions.Instance.IsHeroMoveMode)
            return;

        isDragMode = true;
        dropRequested = false;

        if (!Interactions.Instance.IsCardTargetMode)
        {
            currentSelectedCard = cardView;
            StartCoroutine(CardTargetMode());
        }
        else if (cardView != currentSelectedCard)
        {
            ChangeSelectedCard(cardView);
        }
    }

    //카드 드래그 종료: 타겟 모드가 이미 끝났거나(우클릭 취소 등) 다른 카드면 무시
    public void EndCardDrag(CardView cardView)
    {
        if (!isDragMode || cardView != currentSelectedCard)
            return;

        dropRequested = true;
    }

    //Privates
    private IEnumerator CardTargetMode()
    {
        Interactions.Instance.IsCardTargetMode = true;
        currentSelectedCard.SetSelected(true);

        List<Vector2Int> placeablePoses = GetPlaceablePoses();
        ShowTargetVisuals();

        while (true)
        {
            Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
            Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

            //마우스가 올라간 타일이 배치 가능할 때만 하이라이트 VG·영웅 미리보기 표시
            if (pos != hoveredPos)
            {
                bool placeable = placeablePoses.Contains(pos);
                if (placeable)
                {
                    VisualGridCreator.Instance.ChangeVisualGridPosition(gameObject.GetInstanceID(), pos, "Hero_SetUp_Highlight");
                    heroPreview.TokenTransform.position = new Vector3(pos.x, pos.y, 1);
                }
                else
                {
                    VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_SetUp_Highlight");
                }
                heroPreview.ChangeState(placeable ? HeroPreview.TokenPreViewState.Positive : HeroPreview.TokenPreViewState.Negative);
                hoveredPos = pos;
            }

            //확정 입력: 클릭 방식은 그리드 클릭, 드래그 방식은 버튼 놓기
            bool confirmed = isDragMode ? dropRequested : Interactions.Instance.GridSelected;
            if (confirmed)
            {
                if (placeablePoses.Contains(pos) && (!isDragMode || Interactions.Instance.IsMouseInGridRange))
                {
                    PlayCardGA playCardGA = new(currentSelectedCard.Card, pos);
                    reservedCards.Enqueue(playCardGA);
                    break;
                }

                SoundSystem.Instance.PlaySound(22);          //잘못된 타일 선택 사운드 재생
                if (isDragMode) break;                       //드래그는 잘못된 위치에 놓으면 취소
            }

            //카드 사용 준비 취소 인터렉션 감지 (카드 뷰가 파괴된 경우 포함)
            if (Interactions.Instance.CancelUse || currentSelectedCard == null)
                break;

            yield return null;
        }

        ResetSelectMode();
    }

    //배치 가능 타일 = 영웅 배치 구역 중 비어있는 타일
    private List<Vector2Int> GetPlaceablePoses()
    {
        List<Vector2Int> result = new(10);
        foreach (var pos in HeroSystem.Instance.SetupPoses)
        {
            if (TokenSystem.Instance.API.IsGridEmpty(pos))
                result.Add(pos);
        }
        return result;
    }

    //선택한 카드 기준으로 화살과 영웅 미리보기를 (재)생성 (카드 변경 시에도 호출)
    private void ShowTargetVisuals()
    {
        if (heroPreview != null)
            Destroy(heroPreview.gameObject);
        heroPreview = TokenCreator.Instance.CreateTokenPreview(currentSelectedCard.Card.Unit);

        arrowView.gameObject.SetActive(true);
        arrowView.SetUp(GetCardWorldPosition(currentSelectedCard));

        hoveredPos = Vector2Int.down;               //다음 프레임에 마우스 타일을 다시 판정
    }

    private void ChangeSelectedCard(CardView cardView)
    {
        currentSelectedCard.SetSelected(false);
        currentSelectedCard = cardView;
        currentSelectedCard.SetSelected(true);
        ShowTargetVisuals();                         //화살 시작점·미리보기를 새 카드 기준으로 교체
    }

    private Vector3 GetCardWorldPosition(CardView cardView)
    {
        Vector3 screenPos = cardView.GetComponent<RectTransform>().position;   //Overlay 캔버스라 화면 좌표
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);             //월드 z=0 평면
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    private void ResetSelectMode()
    {
        if (currentSelectedCard != null)
            currentSelectedCard.SetSelected(false);

        if (heroPreview != null)
            Destroy(heroPreview.gameObject);
        heroPreview = null;
        arrowView.gameObject.SetActive(false);

        currentSelectedCard = null;
        isDragMode = false;
        dropRequested = false;
        Interactions.Instance.IsCardTargetMode = false;
        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
    }

    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint, handTransform);
        yield return handView.AddCard(cardView);
    }

    private IEnumerator AddHeroToHandRoutine(HeroData heroData)
    {
        Card card = new(heroData);
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint, handTransform);
        yield return handView.AddCard(cardView);
    }

    private IEnumerator DiscardCard(Card card, CardView cardView)
    {
        discardPile.Add(card);
        if (cardView == null) yield break;

        cardView.transform.DOScale(Vector3.zero, discardDuration);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, discardDuration);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }

    //Performers
    private IEnumerator DrawCardsGAPerformer(DrawCardsGA drawCardsGA)
    {
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);

        for (int i = 0; i < actualAmount; i++)
            yield return DrawCard();
    }

    private IEnumerator PlayCardGAPerformer(PlayCardGA playCardGA)
    {
        Card card = playCardGA.Card;
        hand.Remove(card);
        CardView cardView = handView.RemoveCard(card);
        yield return DiscardCard(card, cardView);

        SpendAPGA spendAPGA = new(card.APCost);
        ActionSystem.Instance.AddReaction(spendAPGA);

        //선택 타일에 용병 배치
        TokenSystem.Instance.Main.AddToken(card.Unit, TokenType.Hero, playCardGA.TargetPos);

        if (HeroSystem.Instance.CurrentHero == null)
            HeroSystem.Instance.CurrentHero = TokenSystem.Instance.API.GetTokenByPosition(playCardGA.TargetPos) as HeroView;
    }

    //Reactions
    //전투 시작 시에만 드로우 (턴 시작 드로우 없음, 이후 드로우는 카드 뽑기 버튼)
    private void StartBattlePreReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.StartBattle) return;

        DrawCardsGA drawCardsGA = new(startDrawAmount);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
}
