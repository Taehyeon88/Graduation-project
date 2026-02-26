using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

    //Performers
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
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
        hand.Remove(playCardGA.Card);
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        yield return DiscardCard(cardView);

        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);

        if (playCardGA.Card.ManualTargetEffects != null && playCardGA.Card.ManualTargetEffects.Count > 0)
        {
            foreach (var effect in playCardGA.Card.ManualTargetEffects)
            {
                PerformEffectGA performEffectGA = new(effect, playCardGA.ManualTarget , HeroSystem.Instance.HeroView);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
        }

        if (playCardGA.Card.GridTargetMode != null && playCardGA.Card.GridTargetMode.Count > 0)
        {
            foreach (var targetMode in playCardGA.Card.GridTargetMode)
            {
                Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
                bool penetration = targetMode.TargetMode is LineTM;
                var range = targetMode.GridRangeMode.GetGridRanges(currentPos, targetMode.Distance, penetration);

                List<Vector2Int> currentTargets = new();

                //비주얼 공격 예상 범위 그리드 업데이트
                foreach (var r in range)
                    VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), r, "Hero_WillAttack");

                Debug.Log("공격 가능 범위: " + string.Join(",", range));

                while (true)
                {
                    Vector3 temp = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(0);
                    Vector2Int gridPosition = new((int)temp.x, (int)temp.y);

                    var targets = targetMode.TargetMode.GetTargets(range, gridPosition, currentPos, targetMode.Distance);
                    if (targets != null)
                    {
                        if (currentTargets != targets)
                        {
                            //비주얼 공격 범위 그리드 업데이트
                            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Attack");
                            foreach (var target in targets)
                                VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), target, "Hero_Attack");

                            currentTargets = targets;
                        }
                    }

                    if (InteractionSystem.GridSelected)  //그리드 선택 인터렉션 감지
                    {
                        Debug.Log("그리드 선택됨");
                        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());

                        List<CombatantView> combatants = new();
                        foreach (var target in currentTargets)
                        {
                            Token token = TokenSystem.Instance.GetTokenByPosition(target);
                            if (token != null)
                            {
                                combatants.Add(token as CombatantView);
                            }
                        }
                        if (combatants.Count > 0)
                        {
                            DealDamageGA dealDamageGA = new(5, combatants, HeroSystem.Instance.HeroView);
                            ActionSystem.Instance.AddReaction(dealDamageGA);
                        }
                        else
                        {
                            Debug.Log("해당 범위 안에 대상이 없음");
                        }

                        break;
                    }

                    yield return null;
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
