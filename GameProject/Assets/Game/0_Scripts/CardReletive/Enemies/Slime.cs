using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    [SerializeField] private int Damage = 5;

    private const int moveDistance = 1;    //이동 거리
    private const int attackDistance = 1;  //공격 사거리

    //슬라임이 알아야 할 것들: (현재 나의 위치, 플레이어의 위치, 현재 지형) |
    public override GameAction JudgeActActions(EnemyView myEnemyView)
    {
        //행동 : 1칸 이내 공격밖에 없음.
        //하나밖에 없기 때문에 이것만 함.
        Vector2Int[] area = TokenSystem.Instance.GetAroundGrids(myEnemyView);
        AttackHeroGA attackHeroGA = new(myEnemyView, Damage, area);
        return attackHeroGA;
    }

    public override PerformMoveGA JudgeMoveAction(EnemyView myEnemyView)
    {
        //이동 : 플레이어와 인접할 때까지 1칸씩 이동

        //영웅 주변으로 이동 가능 타일 찾기
        var canMovePoses = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, attackDistance);

        (int, Vector2Int) minValues = GetMinValues(canMovePoses, myEnemyView);  //최소 경로의 목표 위치 찾기
        int distance = minValues.Item1;
        Vector2Int targetPos = minValues.Item2;

        if (distance == int.MaxValue)  //이동할 경로를 찾을 수 없음. (아무 것도 하지 않음) - 임시
        {
            return null;
        }
        else if (distance == 0)//이미 영웅과 인접한 상황일 경우, 이동X(distance == 0)
        {
            return null;
        }
        else if (distance >= 1)
        {
            //해당 타일로 이동할 최소 경로 추적
            var path = TokenSystem.Instance.GetShortestPath(myEnemyView, targetPos);//현재 위치에서 영웅까지의 경로(그리드)

            return new(myEnemyView, new() { path[0] });
        }
        return null;
    }
    private (int, Vector2Int) GetMinValues(List<Vector2Int> canMovePoses, EnemyView myEnemyView)
    {
        //해당 타일들 중, 가장 거리가 가까운 타일 선정
        int minDistance = int.MaxValue;
        Vector2Int targetPos = Vector2Int.zero;
        foreach (var pos in canMovePoses)
        {
            var path = TokenSystem.Instance.GetShortestPath(myEnemyView, pos);   //최소 경로 찾기
            if (path == null) continue;

            int dis = path.Count;
            if (dis < minDistance)
            {
                minDistance = dis;
                targetPos = pos;
            }
            else if (dis == minDistance)  //같은 거리일 경우, 50퍼센트의 확률로 바뀜
            {
                float randomValue = UnityEngine.Random.value;
                if (randomValue > 0.5f)
                {
                    targetPos = pos;
                }
            }
        }

        return (minDistance, targetPos);
    }
}
