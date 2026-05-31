using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityRandomGenerator
{
    public static Vector2Int GetFirstGenPos(List<Vector2Int> candidates)
    {
        int r_value = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[r_value];
    }

    /// <summary>
    /// 특정 몬스터 랜덤 생성 위치 반환 함수
    /// </summary>
    /// <param name="candidates"></param>
    /// <param name="enemyGenerateTypes"></param>
    /// <returns></returns>
    public static Vector2Int GetSpawnPosition(List<Vector2Int> candidates, EnemyGenerateType[] enemyGenerateTypes)
    {
        if (HeroSystem.Instance.HeroView == null)
        {
            return GetFirstGenPos(candidates);
        }

        if (enemyGenerateTypes == null)
        {
            Debug.LogError($"몬스터 생성 대상 타입이 존재하지 않습니다.");
            return default;
        }

        if (enemyGenerateTypes.Length == 0)
        {
            Debug.LogError($"몬스터 생성 대상 타입의 개수가 0 입니다.");
            return default;
        }

        List<float> weights = new();

        float total = 0f;

        foreach (var pos in candidates)
        {
            float weight = ApplyWeight(pos, enemyGenerateTypes);

            weights.Add(weight);

            total += weight;
        }

        float rand = UnityEngine.Random.value * total;

        float current = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            current += weights[i];

            if (rand <= current)
                return candidates[i];
        }

        return candidates[^1];
    }

    private static float ApplyWeight(Vector2Int pos, EnemyGenerateType[] enemyGenerateTypes)
    {
        float weight = 1f;
        foreach (var type in enemyGenerateTypes)
        {
            switch (type)
            {
                case EnemyGenerateType.CloseDistance:
                    weight *= GetDistanceWeight(pos); break;

                case EnemyGenerateType.MiddleDistance:
                    weight *= GetDistanceWeight(pos, 5); break;
            }
        }
        return weight;
    }

    private static float GetDistanceWeight(Vector2Int pos, float center = 3)
    {
        Vector2Int heroPos = HeroSystem.Instance.HeroPosition;
        if (heroPos != null)
        {
            int distance = TokenSystem.Instance.GetDistance(pos, heroPos);
            float sigma = 1.5f;
            float x = distance - center;
            return Mathf.Exp(-(x * x) / (2 * sigma * sigma));
        }
        else
        {
            return 10;
        }
    }
}
