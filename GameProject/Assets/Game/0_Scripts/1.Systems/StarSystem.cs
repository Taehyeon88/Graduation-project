using System.Collections.Generic;

public class StarSystem : Singleton<StarSystem>   //전투 씬: 별 조건 판정용 기록(몬스터 처치·카드 배치)을 모으고, 승리 시 ★1~★3 달성 여부를 판정
{
    private readonly Dictionary<EnemyData, int> killCounts = new();   //몬스터 종류별 처치 수
    private readonly List<Hero> placedHeroes = new();                 //이번 판에 카드로 배치한 영웅

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<KillGA>(KillPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<KillGA>(KillPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardPostReaction, ReactionTiming.POST);
    }

    //Publics
    //별 조건 문구 (★1~★3) — 정보 팝업·승리 UI 공용, 씬과 무관하게 쓰도록 정적
    public static string GetStarDescription(StageData stage, int index) => index switch
    {
        0 => "승리",
        1 => $"용병대장 체력 {stage.CaptainHealthStarThreshold} 이상 남기고 승리",
        _ => stage.SpecialStarCondition != null ? stage.SpecialStarCondition.Description : "(조건 없음)"
    };

    //승리한 판의 별 달성 여부 (GameClearGA Performer가 토큰 정리 전에 호출) — [★1 승리, ★2 용병대장 체력, ★3 특수 조건]
    public bool[] EvaluateStars()
    {
        GameSystem game = GameSystem.Instance;
        StageData stage = game.CurrentStageData;

        StageResult result = new(
            TurnSystem.Instance.CurrentTurn_Number,
            HeroSystem.Instance.HeroCaptain.CurrentHealth,
            game.BaseHeroes,
            game.BattleDeck,
            placedHeroes,
            killCounts);

        return new[]
        {
            true,
            result.CaptainHealth >= stage.CaptainHealthStarThreshold,
            stage.SpecialStarCondition != null && stage.SpecialStarCondition.IsMet(result)
        };
    }

    //Reactions
    //사망 계열 판정 기준과 같이 토큰 제거 전(PRE)에 집계
    private void KillPreReaction(KillGA killGA)
    {
        if (killGA.Token is not EnemyView enemy || enemy.TokenData is not EnemyData data) return;

        killCounts.TryGetValue(data, out int count);
        killCounts[data] = count + 1;
    }

    private void PlayCardPostReaction(PlayCardGA playCardGA)
    {
        placedHeroes.Add(playCardGA.Card.Unit);
    }
}
