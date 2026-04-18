using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Hermit_QxidizedEA : EnemyAction
{
    //무기 산화 : [혼란 1] 을 부여합니다.
    //[플레이어 지정, 인접 2칸 내 플레이어가 존재할 시]

    public override Sprite Icon
    {
        get { return icon; }
        protected set { }
    }
    public override string Description
    {
        get { return description; }
        protected set { }
    }

    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        var target = new List<CombatantView>();

        var dir = Directions[0];
        var pos = TokenSystem.Instance.GetDirectionPos(enemy, dir);
        var hero = TokenSystem.Instance.GetTokenByPosition(pos) as HeroView;
        if (hero != null)
        {
            target.Add(hero);
        }

        var curPos = TokenSystem.Instance.GetTokenPosition(enemy);
        Tween attackTween = Utility.GetTween(enemy, pos, 0.15f);
        Tween backTween = Utility.GetTween(enemy, curPos, 0.25f);

        Sequence squ = DOTween.Sequence();
        squ.Append(attackTween).Append(backTween);

        //혼란 1 상태 효과 부여
        AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.DISARRAY, 1, target);
        ActionSystem.Instance.AddReaction(addStatusEffectGA);

        return squ;
    }

    public override EnemyAction Clone()
    {
        return new Hermit_QxidizedEA()
        {
            icon = icon,
            description = description,
        };
    }
}
