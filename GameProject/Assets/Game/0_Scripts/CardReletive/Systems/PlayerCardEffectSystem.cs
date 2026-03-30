using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardEffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackEnemyGA>(AttackEnemyGAPerformer);
        ActionSystem.AttachPerformer<ShoulderBashGA>(ShoulderBashGAPerformer);
        ActionSystem.AttachPerformer<ShieldBashGA>(ShieldBashGAPerformer);
        ActionSystem.AttachPerformer<SplashGA>(SplashGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackEnemyGA>();
        ActionSystem.DetachPerformer<ShoulderBashGA>();
        ActionSystem.DetachPerformer<ShieldBashGA>();
        ActionSystem.DetachPerformer<SplashGA>();
    }

    //Performers

    /// <summary>
    /// 인접 - 기본 몬스터 공격
    /// </summary>
    /// <param name="attackEnemyGA"></param>
    /// <returns></returns>
    private IEnumerator AttackEnemyGAPerformer(AttackEnemyGA attackEnemyGA)
    {
        List<CombatantView> combatants = new();
        foreach (var targetPos in attackEnemyGA.TargetPoses)
        {
            Token token = TokenSystem.Instance.GetTokenByPosition(targetPos);
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

        yield return null;
    }

    /// <summary>
    /// 인접 - 어깨치기 기술
    /// </summary>
    /// <param name="shoulderBashGA"></param>
    /// <returns></returns>
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

            if (targets != null)
            {
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
            }

            yield return null;
        }
    }

    /// <summary>
    /// 인접 - 방패가격 기술
    /// </summary>
    /// <param name="shieldBashGA"></param>
    /// <returns></returns>
    private IEnumerator ShieldBashGAPerformer(ShieldBashGA shieldBashGA)
    {
        List<CombatantView> combatants = new();
        foreach (var targetPos in shieldBashGA.TargetPoses)
        {
            Token token = TokenSystem.Instance.GetTokenByPosition(targetPos);
            if (token != null)
            {
                combatants.Add(token as CombatantView);
            }
        }
        if (combatants.Count > 0)
        {
            DealDamageGA dealDamageGA = new(shieldBashGA.Amount, combatants, HeroSystem.Instance.HeroView);
            ActionSystem.Instance.AddReaction(dealDamageGA);

            AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.ARMOR, shieldBashGA.Amount, new() { HeroSystem.Instance.HeroView });
            dealDamageGA.PostReactions.Add((addStatusEffectGA, null));
        }
        else
        {
            Debug.Log("해당 범위 안에 대상이 없음");
        }

        yield return null;
    }

    private IEnumerator SplashGAPerformer(SplashGA splashGA)
    {

        foreach (var targetPos in splashGA.TargetPoses)
        {
            CombatantView target = TokenSystem.Instance.GetTokenByPosition(targetPos) as CombatantView;
            var range = splashGA.GridRangeMode.GetGridRanges(targetPos, splashGA.Distance, splashGA.IsPentration);

            if (target != null)
            {
                DealDamageGA dealDamageGA = new(splashGA.Damage, new() { target }, splashGA.Caster);
                ActionSystem.Instance.AddReaction(dealDamageGA);

                List<CombatantView> splashTargets = new();
                foreach (var r in range)
                {
                    CombatantView tg = TokenSystem.Instance.GetTokenByPosition(r) as CombatantView;
                    if (tg != null)
                    {
                        splashTargets.Add(tg);
                    }
                    Debug.Log($"그리드 비주얼: {string.Join(",", range)}");
                    VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), r, "Hero_WillAttack");
                }
                if (splashTargets.Count > 0)
                {
                    DealDamageGA dDGA = new(splashGA.SplashDamage, splashTargets, splashGA.Caster);
                    dealDamageGA.PostReactions.Add((dDGA, null));
                }
            }
            else
            {
                Debug.Log("해당 범위 안에 대상이 없음");
            }
        }
        splashGA.PostReactions.Add((null, EndSplashVisualGrid));

        yield return null;
    }
    private void EndSplashVisualGrid()
    {
        Debug.Log("삭제용");
        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_WillAttack");
    }
}
