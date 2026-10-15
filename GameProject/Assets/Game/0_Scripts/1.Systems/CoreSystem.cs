using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CoreSystem : Singleton<CoreSystem>
{
    public int PlayerCoreHealth { get; private set; }
    public int EnemyCoreHealth { get; private set; }
    public int PlayerCoreMaxHealth { get; private set; }
    public int EnemyCoreMaxHealth { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackCoreGA>(AttackCoreGAPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackCoreGA>();
    }

    public void SetUp(StageData stageData)
    {
        PlayerCoreMaxHealth = PlayerCoreHealth = stageData.PlayerCoreHealth;
        EnemyCoreMaxHealth = EnemyCoreHealth = stageData.EnemyCoreHealth;
    }

    //Performers
    private IEnumerator AttackCoreGAPerformer(AttackCoreGA attackCoreGA)
    {
        CombatantView attacker = attackCoreGA.Caster as CombatantView;
        if (attacker == null) yield break;

        bool isHeroAttack = attacker is HeroView;   //영웅은 적 코어, 적은 플레이어 코어를 공격
        TokenServiceAPI api = TokenSystem.Instance.API;
        Vector2Int attackerPos = api.GetTokenPosition(attacker);
        Vector2 direction = isHeroAttack ? Vector2.right : Vector2.left;

        //기본 공격 모션, 타격 시점에 피해 적용
        bool hit = false;
        Sequence attackSeq = Utility.GetBasicAttackTween(attacker, attackerPos, direction, () => hit = true);
        yield return new WaitUntil(() => hit);

        if (isHeroAttack)
            EnemyCoreHealth = Mathf.Max(0, EnemyCoreHealth - attacker.BaseDamage);
        else
            PlayerCoreHealth = Mathf.Max(0, PlayerCoreHealth - attacker.BaseDamage);

        //코어 체력 0: 적 코어면 승리, 플레이어 코어면 패배
        if (isHeroAttack && EnemyCoreHealth <= 0)
            ActionSystem.Instance.AddReaction(new GameClearGA());
        else if (!isHeroAttack && PlayerCoreHealth <= 0)
            ActionSystem.Instance.AddReaction(new GameOverGA());

        if (attackSeq != null && attackSeq.IsActive())
            yield return attackSeq.WaitForCompletion();   //복귀까지 대기
    }
}
