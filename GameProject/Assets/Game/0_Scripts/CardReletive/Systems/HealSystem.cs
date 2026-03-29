using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSystem : Singleton<HealSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<HealGA>(HealGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<HealGA>();
    }

    private IEnumerator HealGAPerformer(HealGA healGA)
    {
        if (healGA == null)
        {
            Debug.LogError($"현재 사용한 healGA가 존재하지 않습니다.");
            yield break;
        }
        //대상 위치가 존재할 경우, 해당 위치의 대상을 받아서 추가
        if (healGA.TargetPoses != null)
        {
            foreach (var targetPos in healGA.TargetPoses)
            {
                var target = TokenSystem.Instance.GetTokenByPosition(targetPos) as CombatantView;
                if (target != null)
                    healGA.Targets.Add(target);
            }
        }

        //모든 대상에게 회복
        foreach (var target in healGA.Targets)
        {
            if (target != null)
            {
                target.Heal(healGA.Amount);
                //회복 연출
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}
