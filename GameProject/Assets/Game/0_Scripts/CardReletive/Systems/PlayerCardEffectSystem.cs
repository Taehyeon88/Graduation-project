using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardEffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackEnemyGA>(AttackEnemyGAPerformer);
        ActionSystem.AttachPerformer<ShoulderBashGA>(ShoulderBashGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackEnemyGA>();
        ActionSystem.DetachPerformer<ShoulderBashGA>();
    }

    //Performers
    private IEnumerator AttackEnemyGAPerformer(AttackEnemyGA attackEnemyGA)
    {
        GridTargetMode targetMode = attackEnemyGA.GridTargetMode;

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
            Vector3 temp = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
            Vector2Int gridPosition = new((int)temp.x, (int)temp.y);

            var targets = targetMode.TargetMode.GetTargets(range, gridPosition, currentPos, targetMode.Distance);

            if (targets != null)
            {
                string targetStr = string.Join("", targets);
                string curStr = string.Join("", currentTargets);

                if (curStr != targetStr)
                {
                    Debug.Log("이전 대상: " + string.Join(",", currentTargets) + "변경 대상: " + string.Join(",", targets));
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
                Debug.Log("선택 대상: " + string.Join(",", currentTargets));

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
                    DealDamageGA dealDamageGA = new(attackEnemyGA.Amount, combatants, HeroSystem.Instance.HeroView);
                    ActionSystem.Instance.AddReaction(dealDamageGA);
                }
                else
                {
                    Debug.Log("해당 범위 안에 대상이 없음");
                }

                VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
                break;
            }

            yield return null;
        }
    }

    private IEnumerator ShoulderBashGAPerformer(ShoulderBashGA shoulderBashGA)
    {
        GridTargetMode targetMode = shoulderBashGA.GridTargetMode;

        SPDSystem.Instance.AddSPD(targetMode.Distance);

        //현재 SPD가 없어서 이동 불가일 경우, 반환처리
        int curSPD = SPDSystem.Instance.RemainSPD();
        if (curSPD <= 0) yield break;

        Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        var range = targetMode.GridRangeMode.GetGridRanges(currentPos, targetMode.Distance, false);
        List<Vector2Int> attackRange = new();

        List<Vector2Int> currentTargets = new();

        //비주얼 공격 예상 범위 그리드 업데이트
        foreach (var r in range)
        {
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), r, "Hero_Move");
            Vector2Int attackPos = r + (r - currentPos);
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), attackPos, "Hero_WillAttack");
            attackRange.Add(attackPos);
        }

        while (true)
        {
            Vector3 temp = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
            Vector2Int gridPosition = new((int)temp.x, (int)temp.y);

            var targets = targetMode.TargetMode.GetTargets(range, gridPosition, currentPos, targetMode.Distance);

            if (InteractionSystem.GridSelected)  //그리드 선택 인터렉션 감지
            {
                Vector3 mousePosition = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1f);
                Vector2Int isoPosition = new((int)mousePosition.x, (int)mousePosition.y);
                CombatantView heroView = HeroSystem.Instance.HeroView;

                int distance = TokenSystem.Instance.GetDistance(heroView, isoPosition);

                if (curSPD >= distance)
                {
                    var path = TokenSystem.Instance.GetShortestPath(heroView, isoPosition);
                    if (path != null)
                    {
                        SPDSystem.Instance.SpendSPD(distance);

                        PerformMoveGA performMoveGA = new(heroView, path);
                        ActionSystem.Instance.AddReaction(performMoveGA);

                        Vector2Int attackPos = path[0] + (path[0] - currentPos);
                        CombatantView target = TokenSystem.Instance.GetTokenByPosition(attackPos) as CombatantView;

                        if (target != null)
                        {
                            //이동 이후, 공격 체인
                            DealDamageGA dealDamageGA = new(shoulderBashGA.Damage, new() { target }, heroView);
                            performMoveGA.PostReactions.Add((dealDamageGA, null));

                            //공격 이후, 대상 넉백 체인
                            KnockBackGA knockBackGA = new(heroView, shoulderBashGA.Distance, attackPos, (path[0] - currentPos));
                            performMoveGA.PostReactions.Add((knockBackGA, null));
                        }
                        else
                        {
                            Debug.Log("대상이 존재하지 않음");
                        }
                    }
                }
                else
                {
                    Debug.Log("거리의 밖 구역에서 사용할 수 없습니다.");
                }

                Debug.Log("초기화.");
                VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
                yield break;
            }

            yield return null;
        }
    }
}
