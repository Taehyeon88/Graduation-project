using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemySystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackEnemyGA>(AttackEnemyGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackEnemyGA>();
    }

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
                VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());

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

                break;
            }

            yield return null;
        }
    }
}
