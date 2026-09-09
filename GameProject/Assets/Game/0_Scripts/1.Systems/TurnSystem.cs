using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TurnSystem : Singleton<TurnSystem>
{
    [Header("Element")]
    [SerializeField] private TurnPopUpUI turnPopUpUI;
    public TurnType CurrentTurn => currentTurn;
    public int CurrentTurn_Number => currentTurn_Number;

    private TurnType currentTurn = TurnType.GameSetUp;
    private int currentTurn_Number = 0;

    protected void OnEnable()
    {
        ActionSystem.AttachPerformer<TurnGA>(TurnGAPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<TurnGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
    }

    private IEnumerator TurnGAPerformer(TurnGA turnGA)
    {
        Debug.Log("전투 시작");
        //게임 시작 턴 시작 이후, 플레이어 턴 시작
        if (turnGA.Type == TurnType.StartBattle)
        {
            TurnGA playerTurnGA = new(TurnType.Player);
            ActionSystem.Instance.AddReaction(playerTurnGA);
        }
        else if (turnGA.Type == TurnType.Enemy)
        {
            Tween direct = turnPopUpUI.GetTurnPopUpTween(TurnType.Enemy, currentTurn_Number);   //턴 팝업 연출
            direct?.Restart();
            yield return direct?.WaitForCompletion();

            yield return EnemySystem.Instance.PlayEnemyTurnPerformer();
        }
        else if (turnGA.Type == TurnType.Player)
        {
            yield return new WaitForSeconds(1f);

            currentTurn_Number++;
            Tween direct = turnPopUpUI.GetTurnPopUpTween(TurnType.Player, currentTurn_Number);  //턴 팝업 연출
            direct?.Restart();
            yield return direct?.WaitForCompletion();

            yield return HeroSystem.Instance.PlayHeroTurnPerformer();
        }
    }

    private void TurnGAPostReaction(TurnGA turnGA)
    {
        //몬스터 턴 종료 후, 플레이어 턴 시작
        if(turnGA.Type == TurnType.Enemy)
        {
            TurnGA playerTurnGA = new(TurnType.Player);
            ActionSystem.Instance.AddReaction(playerTurnGA);
        }
    }

    private void TurnGAPreReaction(TurnGA turnGA)
    {
        currentTurn = turnGA.Type;  //턴 타입 변경
    }
}
