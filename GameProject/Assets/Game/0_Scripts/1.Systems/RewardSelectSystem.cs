using System.Collections.Generic;
using UnityEngine;

//승리 후 보상 용병 선택: 확인 후 보상 용병을 그리드에 생성하고, 하나를 클릭하면 덱에 추가
public class RewardSelectSystem : Singleton<RewardSelectSystem>
{
    [SerializeField] private Vector2Int[] rewardPositions = { new(1, 1), new(1, 2) };   //보상 용병 생성 위치

    private readonly Dictionary<HeroView, HeroData> rewardHeroes = new();   //생성한 보상 용병
    private HeroData[] pendingRewards = System.Array.Empty<HeroData>();     //수령했지만 아직 생성하지 않은 보상

    public bool IsSelecting { get; private set; }

    //보상 보관 (생성은 BeginSelect에서)
    public void SetRewards(HeroData[] rewards)
    {
        pendingRewards = rewards;
        rewardHeroes.Clear();
        IsSelecting = false;
    }

    //보상 용병 생성 후 선택 시작 (승리 UI 확인 이후)
    public void BeginSelect()
    {
        int count = Mathf.Min(pendingRewards.Length, rewardPositions.Length);
        for (int i = 0; i < count; i++)
        {
            TokenSystem.Instance.Main.AddToken(pendingRewards[i], TokenType.Hero, rewardPositions[i]);

            HeroView hero = TokenSystem.Instance.API.GetTokenByPosition(rewardPositions[i]) as HeroView;
            if (hero != null)
                rewardHeroes.Add(hero, pendingRewards[i]);
        }
        pendingRewards = System.Array.Empty<HeroData>();
        IsSelecting = rewardHeroes.Count > 0;
    }

    //클릭한 토큰이 보상 용병이면 획득(즉시 제거)하고, 선택하지 않은 용병은 연출 후 제거
    public void TrySelect(Token token)
    {
        if (!IsSelecting || token is not HeroView selected || !rewardHeroes.TryGetValue(selected, out HeroData selectedData))
            return;

        IsSelecting = false;
        GameSystem.Instance.AddHeroToDeck(selectedData);

        foreach (HeroView hero in rewardHeroes.Keys)
        {
            TokenSystem.Instance.HeroViews.Remove(hero);
            if (hero == selected)
                TokenSystem.Instance.Main.RemoveToken2(hero);
            else
                StartCoroutine(TokenSystem.Instance.Main.RemoveToken(hero));
        }
        rewardHeroes.Clear();
    }
}
