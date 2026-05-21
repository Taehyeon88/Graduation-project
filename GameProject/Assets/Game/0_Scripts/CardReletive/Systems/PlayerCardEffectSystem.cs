using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardEffectSystem : Singleton<PlayerCardEffectSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackEnemyGA>(AttackEnemyGAPerformer);
        ActionSystem.AttachPerformer<PlacePowerTotemGA>(PlacePowerTotemPerformer);
        ActionSystem.AttachPerformer<PowerTotemEmissionGA>(PowerTotemEmissionPerformer);
        ActionSystem.AttachPerformer<ShoulderBashGA>(ShoulderBashGAPerformer);
        ActionSystem.AttachPerformer<ShieldBashGA>(ShieldBashGAPerformer);
        ActionSystem.AttachPerformer<SplashGA>(SplashGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackEnemyGA>();
        ActionSystem.DetachPerformer<PlacePowerTotemGA>();
        ActionSystem.DetachPerformer<PowerTotemEmissionGA>();
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
            if (attackEnemyGA.IsRandomTargetMode)
            {
                List<CombatantView> randomTargets = new();
                foreach (var combatant in combatants)
                {
                    int randomIndex = UnityEngine.Random.Range(0, combatants.Count - 1);
                    randomTargets.Add(combatants[randomIndex]);
                }
                combatants = randomTargets;
            }

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
    /// 파워 토큰 설치
    /// </summary>
    /// <param name="placePowerTotemGA"></param>
    /// <returns></returns>
    private IEnumerator PlacePowerTotemPerformer(PlacePowerTotemGA placePowerTotemGA)
    {
        //받은 범위안에 토큰 배치 처리
        foreach (var pos in placePowerTotemGA.TargetPoses)
            TokenSystem.Instance.AddToken(placePowerTotemGA.PowerTotemData, pos);

        yield return null;
    }

    private IEnumerator PowerTotemEmissionPerformer(PowerTotemEmissionGA powerTotemEmissionGA)
    {
        //모든 파워 토템을 찾아서 인접 범위를 받아서 순차적으로 폭발 및 데미지 처리
        //순서 : 토템 1 -> 폭발 -> 토템 2 -> 폭발

        var tokens = TokenSystem.Instance.GetAllTokens();
        foreach (var token in tokens)
        {
            if (token is PowerTotemView powerTotemView)
            {
                Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(token);
                List<Vector2Int> poses = TokenSystem.Instance.GetAllAroundPlaces(currentPos, 1, true);

                AttackEnemyGA attackEnemyGA = new(poses, powerTotemEmissionGA.Damage, false, true);
                ActionSystem.Instance.AddReaction(attackEnemyGA);
            }
        }

        return null;
    }

    /// <summary>
    /// 인접 - 어깨치기 범위VG 생성 함수
    /// </summary>
    public void ShoulderBashRVG(int ownerID, List<Vector2Int> range)
    {
        Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        foreach (var r in range)
        {
            VisualGridCreator.Instance.CreateVisualGrid(ownerID, r, "Hero_Move");
            Vector2Int attackPos = r + (r - currentPos);
            VisualGridCreator.Instance.CreateVisualGrid(ownerID, attackPos, "Hero_WillAttack");
        }
    }

    /// <summary>
    /// 인접 - 어깨치기 기술
    /// </summary>
    /// <param name="shoulderBashGA"></param>
    /// <returns></returns>
    private IEnumerator ShoulderBashGAPerformer(ShoulderBashGA shoulderBashGA)
    {
        Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        Vector2Int targetPos = shoulderBashGA.TargetPoses[0];
        CombatantView heroView = HeroSystem.Instance.HeroView;

        var path = TokenSystem.Instance.GetShortestPath(heroView, targetPos);
        if (path != null)
        {
            PerformMoveGA performMoveGA = new(heroView, path);
            ActionSystem.Instance.AddReaction(performMoveGA);

            Vector2Int attackPos = path[0] + (path[0] - currentPos);
            CombatantView target = TokenSystem.Instance.GetTokenByPosition(attackPos) as CombatantView;

            if (target != null)
            {
                //연출
                HeroVisualEffectSystem.Instance.PlayVisualEffectPreGameAction(
                         CardType.Attack_Adjacent,
                         CardSubType.Dash,
                         new() { attackPos },
                         true
                         );

                //이동 이후, 공격 체인
                DealDamageGA dealDamageGA = new(shoulderBashGA.Damage, new() { target }, heroView);
                ActionSystem.Instance.AddReaction(dealDamageGA);

                //연출
                HeroVisualEffectSystem.Instance.PlayVisualEffectPostGameAction(
                         CardType.Attack_Adjacent,
                         CardSubType.Dash,
                         new() { attackPos },
                         true
                         );

                //공격 이후, 대상 넉백 체인
                KnockBackGA knockBackGA = new(heroView, shoulderBashGA.Distance, attackPos, (path[0] - currentPos));
                ActionSystem.Instance.AddReaction(knockBackGA);
            }
            else
            {
                Debug.Log("대상이 존재하지 않음");
            }
        }
        yield return null;
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

            int shieldStack = Mathf.CeilToInt(shieldBashGA.Amount);
            AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.ARMOR, shieldStack, new() { HeroSystem.Instance.HeroView });
            dealDamageGA.PostReactions.Add((addStatusEffectGA, null));
        }
        else
        {
            Debug.Log("해당 범위 안에 대상이 없음");
        }

        yield return null;
    }

    public void SplashTVG(bool active, int ownerID, List<Vector2Int> range, Card card)
    {
        if (active)
        {
            Vector2Int targetPos = range[0];
            if (card.GridTargetMode.Effect is SplashEffect splashEffect)
            {
                var rg = splashEffect.GridRangeMode.GetGridRanges(
                    targetPos,
                    splashEffect.Distance,
                    splashEffect.IsPentration
                    );

                VisualGridCreator.Instance.CreateVisualGrid(ownerID, targetPos, "Explosion_Damage");
                foreach (var r in rg)
                    VisualGridCreator.Instance.CreateVisualGrid(ownerID, r, "Explosion_Splash");
            }
        }
        else
        {
            VisualGridCreator.Instance.RemoveVisualGrid(ownerID, "Explosion_Damage");
            VisualGridCreator.Instance.RemoveVisualGrid(ownerID, "Explosion_Splash");
        }
    }

    /// <summary>
    /// 스플래시 효과
    /// </summary>
    /// <param name="splashGA"></param>
    /// <returns></returns>
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
                        if(tg is not HeroView)
                            splashTargets.Add(tg);
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

        yield return null;
    }
}
