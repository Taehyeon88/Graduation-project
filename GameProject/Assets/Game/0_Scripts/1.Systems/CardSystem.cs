using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    [Header("Card Element")]
    [SerializeField] private HandView handView;
    [SerializeField] private RectTransform handTransform;
    [SerializeField] private RectTransform drawPilePoint;
    [SerializeField] private RectTransform discardPilePoint;

    [Header("Deck (임시 값 - 원작 확인 필요)")]
    [SerializeField] private List<CardData> startingDeck;
    [SerializeField] private int drawAmount;

    [Header("Card Direct Element")]
    [SerializeField] private float discardDuration = 0.15f;

    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();
    private readonly Queue<PlayCardGA> reservedCards = new();
    private CardView currentSelectedCard;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsGAPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardGAPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(PlayerTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(PlayerTurnPostReaction, ReactionTiming.POST);
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

        if (startingDeck == null || startingDeck.Count == 0) return;

        //Shuffle은 입력 리스트를 비우므로 복사본 사용
        foreach (var cardData in new List<CardData>(startingDeck).Shuffle())
            drawPile.Add(new Card(cardData));
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
            currentSelectedCard.SetSelected(false);
            currentSelectedCard = cardView;
            currentSelectedCard.SetSelected(true);
        }
        else                                         //같은 카드 재클릭 = 취소
        {
            ResetSelectMode();
        }
    }

    //Privates
    private IEnumerator CardTargetMode()
    {
        Interactions.Instance.IsCardTargetMode = true;
        currentSelectedCard.SetSelected(true);

        //배치 가능 타일 VG 생성
        List<Vector2Int> placeablePoses = GetPlaceablePoses();
        foreach (var pos in placeablePoses)
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Hero_SetUp_True");

        while (true)
        {
            if (Interactions.Instance.GridSelected)
            {
                Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
                Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

                if (placeablePoses.Contains(pos))
                {
                    PlayCardGA playCardGA = new(currentSelectedCard.Card, pos);
                    reservedCards.Enqueue(playCardGA);
                    break;
                }
                else
                {
                    SoundSystem.Instance.PlaySound(22);      //잘못된 타일 선택 사운드 재생
                }
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
        foreach (var pos in GameSystem.Instance.CurrentStageData.HeroSetupPoses)
        {
            if (TokenSystem.Instance.API.IsGridEmpty(pos))
                result.Add(pos);
        }
        return result;
    }

    private void ResetSelectMode()
    {
        if (currentSelectedCard != null)
            currentSelectedCard.SetSelected(false);

        currentSelectedCard = null;
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
    private void PlayerTurnPostReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Player) return;

        DrawCardsGA drawCardsGA = new(drawAmount);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
}
