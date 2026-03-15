using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;

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

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
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
                        addSEE.isMySelf = true;
                    }

                    PerformEffectGA performEffectGA = new(effect, new(HeroSystem.Instance.HeroView));
                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            }

            PlayCardGA playCard2GA = new(playCardGA.Card, false);
            playCardGA.PostReactions.Add((playCard2GA, null));
        }
        else if (playCardGA.Card.GridTargetMode != null)
        {
            foreach (var targetMode in playCardGA.Card.GridTargetMode)
            {
                //그리드 선택 기능을 사용할 경우
                if (targetMode.UseVisualGrid)
                {
                    Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
                    bool penetration = targetMode.TargetMode is LineTM;
                    var range = targetMode.GridRangeMode.GetGridRanges(currentPos, targetMode.Distance, penetration);

                    //이동 카드일 경우, 자체적인 이동 범위 사용
                    if (targetMode.Effect is PlayerMoveEffect)
                    {
                        range = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, targetMode.Distance);
                    }

                    //비주얼 공격 예상 범위 그리드 업데이트
                    foreach (var r in range)
                        VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), r, targetMode.WillSelectVGName);

                    Debug.Log("공격 가능 범위: " + string.Join(",", range));

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

                            if (InteractionSystem.GridSelected)  //그리드 선택 인터렉션 감지
                            {
                                //전달할 데이터 :
                                //1. 타겟으로 지정된 타일 좌표들
                                PerformEffectGA performEffectGA = new(targetMode.Effect, new(targets, targetMode));
                                ActionSystem.Instance.AddReaction(performEffectGA);

                                //StatusEffect(버프/디버프)같이 적용 기능
                                foreach (var effect in targetMode.AddedStatusEffects)
                                {
                                    if (effect is AddStatusEffectEffect)
                                    {
                                        PerformEffectGA performStatusEffectGA = new(effect, new(targets, HeroSystem.Instance.HeroView));
                                        performEffectGA.PostReactions.Add((performStatusEffectGA, null));
                                    }
                                }

                                VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
                                break;
                            }
                        }

                        yield return null;
                    }
                }
                else
                {
                    PerformEffectGA performEffectGA = new(targetMode.Effect, new(targetMode));  //그리드를 선택할 수 있는 기능의 GAEffect만 사용 가능
                    ActionSystem.Instance.AddReaction(performEffectGA);
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
