using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class Test : MonoBehaviour
{
    private void Update()
    {
        //튜토리얼 넘기기
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (TutorialSystem.Instance.IsTutorialing)
                GameSystem.Instance.StartFromScratch();
        }

        //모든 몬스터 처치
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (EnemySystem.Instance.Enemise.Count <= 0) return;

            var targets = new List<CombatantView>();
            foreach(var enemy in EnemySystem.Instance.Enemise)
                targets.Add(enemy);
            DealDamageGA dealDamageGA = new(100, targets, HeroSystem.Instance.HeroView);
            ActionSystem.Instance.Perform(dealDamageGA);
        }
    }
}
