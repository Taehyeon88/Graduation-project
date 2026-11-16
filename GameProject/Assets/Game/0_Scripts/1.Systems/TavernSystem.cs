using UnityEngine;

public class TavernSystem : Singleton<TavernSystem>   //주점 흐름: 영웅 배치 → (취소 | 소문 얻기 → 선택·확인) → 나가기
{
    [SerializeField] private TavernUI tavernUI;

    private Card placedCard;   //타일에 배치한 영웅 카드

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardPostReaction, ReactionTiming.POST);
    }

    //Publics
    public void Cancel()   //배치한 영웅을 제거하고 카드를 손패로 복귀
    {
        TokenSystem.Instance.Main.ClearAllTokens();
        HeroSystem.Instance.CurrentHero = null;

        tavernUI.ShowHand();
        CardSystem.Instance.ReturnCardToHand(placedCard);
        placedCard = null;
    }

    public void ConfirmRumor(RumorData rumor)   //배치한 영웅에게 소문 부여 후 나가기만 가능
    {
        placedCard.Unit.AddRumor(rumor);
        tavernUI.ShowExit();
    }

    public void Exit()   //주점을 마치면 클리어 처리(다음 해금) 후 스테이지 선택으로
    {
        GameSystem.Instance.MarkCurrentCleared();
        GameSystem.Instance.ReturnToStageSelect();
    }

    //Reactions
    private void PlayCardPostReaction(PlayCardGA playCardGA)
    {
        placedCard = playCardGA.Card;
        tavernUI.ShowActions();
    }
}
