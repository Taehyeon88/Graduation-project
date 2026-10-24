using System;
using UnityEngine;

public class RewardSystem : Singleton<RewardSystem>
{
    [Header("1스테이지 보상 (임시)")]
    [SerializeField] private HeroData blessing;       //축복자
    [SerializeField] private HeroData shieldBearer;   //방패병

    //스테이지 번호에 맞는 미리 정해 둔 보상 (임시, 확률 없음)
    public HeroData[] GetRewards(int stageLevel)
    {
        switch (stageLevel)
        {
            case 1: return GetStage1Rewards();
            default: return Array.Empty<HeroData>();   //2스테이지 이후는 미정
        }
    }

    private HeroData[] GetStage1Rewards()
    {
        return new[] { blessing, shieldBearer };
    }
}
