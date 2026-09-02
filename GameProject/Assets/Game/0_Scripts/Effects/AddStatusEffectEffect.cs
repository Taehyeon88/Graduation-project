using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AddStatusEffectEffect : Effect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int stackCount;
    [SerializeField] public SETargetMode setargetMode = SETargetMode.MySelf;    //(이펙트 효과 받는 대상 = 나) 여부 체크
    public override GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView)
    {
        List<CombatantView> targets = new(10);

        switch (setargetMode)
        {
            case SETargetMode.MySelf: targets.Add(myView); break;
            case SETargetMode.Targets: 
                foreach (var pos in targetpoes)
                {
                    Token token = TokenSystem.Instance.API.GetTokenByPosition(pos);
                    if(token != null)
                        targets.Add(token as CombatantView);
                }
                break;
            case SETargetMode.Entire:
                targets.Add(myView);
                foreach (var pos in targetpoes)
                {
                    Token token = TokenSystem.Instance.API.GetTokenByPosition(pos);
                    if (token != null)
                        targets.Add(token as CombatantView);
                }
                break;
        }
        return new AddStatusEffectGA(statusEffectType, stackCount, targets);
    }
}