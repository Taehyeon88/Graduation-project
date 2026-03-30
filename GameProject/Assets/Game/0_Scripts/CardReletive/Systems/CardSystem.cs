using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private RectTransform handTransform;
    [SerializeField] private RectTransform drawPilePoint;
    [SerializeField] private RectTransform discardPilePoint;

    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();

    public int drawPileCA => drawPile.Count;    public int discardPileCA => discardPile.Count;
    private bool isIsolation;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardTargetingGA>(PlayCardTargetingGAPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardTargetingGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
    }
    private void Update()
    {
        //상태이상 - 고립 처리
        CombatantView hero = HeroSystem.Instance.HeroView;
        if (hero != null)
        {
            int isolationStack = hero.GetStatusEffectStacks(StatusEffectType.ISOLATION);
            if (isolationStack > 0)
            {
                //손패의 있는 모든 이동 타입의 카드 비활성화 처리
                foreach (var card in hand)
                {
                    if (card.CardSubType == CardSubType.Move 
                        || card.CardSubType == CardSubType.Dash)
                        handView.SetCardLockView(true, card);
                }
                isIsolation = true;
            }
            else if (isIsolation)
            {
                foreach (var card in hand)
                {
                    if (card.CardSubType == CardSubType.Move
                        || card.CardSubType == CardSubType.Dash)
                        handView.SetCardLockView(true, card);
                }
                isIsolation = false;
            }
        }
    }

    //Public
    public void SetUp(List<CardData> deckData)
    {
        foreach (var cardData in deckData.Shuffle())
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }
    public void EndLockState()
    {
        handView.SetCardsLockView(false);
        Interactions.Instance.lockInteraction = false;
    }

    //Performers
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        Interactions.Instance.lockInteraction = true;  //카드 드로우시, 카드 인터렉션 잠금

        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardsGA.Amount - actualAmount;
        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }
        if (notDrawnAmount > 0)
        {
            RefillDect();
            for (int i = 0; i < notDrawnAmount; i++)
                yield return DrawCard();
        }

        if (drawCardsGA.IsFirstDraw) handView.SetCardsLockView(true); //첫턴 카드 드로우시, 바로 카드사용 불가 처리
        else Interactions.Instance.lockInteraction = false;           //드로우 종료후, 잠금 해제
    }


    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach (var card in hand)
        {
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
    }


    /// <summary>
    /// 카드 사용 전, 그리드 타겟팅 시스템
    /// !!주의: 선택 모드는 철저히 TargetMode, GridRangeMode 유무로 판단. 
    /// 즉, GridTargetMode가 존재할 경우, TargetMode, GridRangeMode를 무조건 가져야함.
    /// </summary>
    /// <param name="playCardTargetingGA"></param>
    /// <returns></returns>
    private IEnumerator PlayCardTargetingGAPerformer(PlayCardTargetingGA playCardTargetingGA)
    {
        //선택 가능_그리드 미리보기
        Card card = playCardTargetingGA.Card;

        //자기자신에 사용 - 비주얼 그리드
        if (card.SelfEffects != null && card.SelfEffects.Count > 0)
        {
            Vector2Int heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), heroPos, "Hero_UseSelf");
        }

        //선택 가능 타일 - 비주얼 그리드
        if (card.GridTargetMode != null && card.GridTargetMode.GridRangeMode != null)
        {
            Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
            var targetMode = card.GridTargetMode;
            bool penetration = targetMode.TargetMode is LineTM;
            var range = targetMode.GridRangeMode.GetGridRanges(currentPos, targetMode.Distance, penetration);

            //!!예외처리: 이동효과
            if (targetMode.Effect is PlayerMoveEffect)
            {
                range = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, targetMode.Distance);
            }

            //비주얼 공격 예상 범위 그리드 업데이트
            if (card.GridTargetMode.UseVisualGrid)
            {
                foreach (var r in range)
                    VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), r, targetMode.WillSelectVGName);
            }
            else
            {
                //커스텀 예상 범위 비주얼
            }

            Debug.Log("공격 가능 범위: " + string.Join(",", range));


            //선택 - 그리드 미리보기
            string curStr = "";
            while (true)
            {
                Vector3 temp = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
                Vector2Int gridPosition = new((int)temp.x, (int)temp.y);

                var targets = targetMode.TargetMode.GetTargets(range, gridPosition, currentPos, targetMode.Distance);
                if (targets != null)
                {
                    if (targetMode.UseSelectVG)
                    {
                        string targetStr = string.Join("", targets);
                        if (curStr != targetStr)
                        {
                            //비주얼 공격 범위 그리드 업데이트
                            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), targetMode.SelectVGName);
                            foreach (var target in targets)
                                VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), target, targetMode.SelectVGName);

                            curStr = targetStr;
                        }
                    }
                    else
                    {
                        //커스텀 선택 VG 사용
                    }

                    //그리드 선택 인터렉션 감지
                    if (InteractionSystem.GridSelected)
                    {
                        playCardTargetingGA.EndSelectAction?.Invoke();

                        PlayCardGA PlayCardGA = new(playCardTargetingGA.Card, targets);
                        ActionSystem.Instance.AddReaction(PlayCardGA);
                        break;
                    }
                }

                //카드 사용 준비 취소 인터렉션 감지
                if (InteractionSystem.CancelReadyUseCard)
                {
                    break;
                }

                yield return null;
            }
        }

        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
    }

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        if (playCardGA.IsPart1)
        {
            hand.Remove(playCardGA.Card);
            CardView cardView = handView.RemoveCard(playCardGA.Card);
            yield return DiscardCard(cardView);

            SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
            ActionSystem.Instance.AddReaction(spendManaGA);

            if (playCardGA.Card.SelfEffects != null)
            {
                foreach (var effect in playCardGA.Card.SelfEffects)
                {
                    //effect가 AddStatusEffectEffect일 경우, 항상 자신자신에게 사용을 설정
                    if (effect is AddStatusEffectEffect addSEE)
                    {
                        addSEE.setargetMode = SETargetMode.MySelf;
                    }

                    PerformEffectGA performEffectGA = new(effect, 
                        new(HeroSystem.Instance.HeroView, 
                        playCardGA.Card.CardType, 
                        playCardGA.Card.CardSubType
                        ));
                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            }

            if (playCardGA.Card.GridTargetMode != null && playCardGA.Card.GridTargetMode.GridRangeMode != null)
            {
                PlayCardGA playCard2GA = new(playCardGA.Card, playCardGA.TargetPoses, false);
                playCardGA.PostReactions.Add((playCard2GA, null));
            }
        }
        else
        {
            GridTargetMode targetMode = playCardGA.Card.GridTargetMode;
            var targetPoses = playCardGA.TargetPoses;

            //메인 실행 GameAction - 그리드좌표 기반
            PerformEffectGA performEffectGA = new(targetMode.Effect,
                new(targetPoses,
                targetMode,
                HeroSystem.Instance.HeroView,
                playCardGA.Card.CardType,
                playCardGA.Card.CardSubType
                ));
            ActionSystem.Instance.AddReaction(performEffectGA);

            //후속 실행 GameAction들 - 대상 선택 가능(그리드좌표/대상)
            if (targetMode._AddedSECondition == GridTargetMode.AddedSECondition.Grid)
            {
                //그리드 좌표 기반
                foreach (var effect in targetMode.AddedEffects)
                {
                    PerformEffectGA performStatusEffectGA = new(effect,
                                   new(targetPoses,
                                   HeroSystem.Instance.HeroView,
                                   playCardGA.Card.CardType,
                                   playCardGA.Card.CardSubType
                               ));
                    performEffectGA.PostReactions.Add((performStatusEffectGA, null));
                }
            }
            else if (targetMode._AddedSECondition == GridTargetMode.AddedSECondition.CombatantView)
            {
                //대상 기반
                List<CombatantView> combatants = new();
                foreach (var targetPos in targetPoses)
                {
                    Token token = TokenSystem.Instance.GetTokenByPosition(targetPos);
                    if (token != null)
                        combatants.Add(token as CombatantView);
                }
                if (combatants.Count > 0)
                {
                    foreach (var effect in targetMode.AddedEffects)
                    {
                        PerformEffectGA performStatusEffectGA = new(effect, new(combatants, HeroSystem.Instance.HeroView));
                        performEffectGA.PostReactions.Add((performStatusEffectGA, null));
                    }
                }
            }
        }
    }


    //Helpers
    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreatCardView(card, drawPilePoint, handTransform);
        yield return handView.AddCard(cardView);
    }
    private void RefillDect() => drawPile.AddRange(discardPile.Shuffle());

    private IEnumerator DiscardCard(CardView cardView)
    {
        discardPile.Add(cardView.card);
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }
}
