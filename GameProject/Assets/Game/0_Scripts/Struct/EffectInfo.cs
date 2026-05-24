using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EffectInfo
{
    public List<CombatantView> targets;
    public CombatantView caster;
    public GridTargetMode gridTargetMode;
    public List<Vector2Int> targetPoses;
    public CardType cardType;
    public CardSubType cardSubType;

    /// <summary>
    /// 카드과 관련 없는 것들
    /// 2. AoESystem의 Effect
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="caster"></param>
    public EffectInfo(List<CombatantView> targets, CombatantView caster)
    {
        this.targets = targets;
        this.caster = caster;
        this.gridTargetMode = null;
        this.targetPoses = null;
        this.cardType = CardType.None;
        this.cardSubType = CardSubType.None;
    }

    /// <summary>
    /// 대상 기반 지정용!!
    /// 1. CardSystem의 UseVisualGrid == true일때, AddedEffect
    /// 2. AoESystem의 Effect
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="caster"></param>
    public EffectInfo(List<CombatantView> targets, CombatantView caster, CardType cardType, CardSubType cardSubType)
    {
        this.targets = targets;
        this.caster = caster;
        this.gridTargetMode = null;
        this.targetPoses = null;
        this.cardType = cardType;
        this.cardSubType = cardSubType;
    }

    /// <summary>
    /// CardSystem의 SelfEffects용
    /// </summary>
    /// <param name="target"></param>
    public EffectInfo(CombatantView target, CardType cardType, CardSubType cardSubType)
    {
        this.targets = new() { target };
        this.caster = target;
        this.gridTargetMode = null;
        this.targetPoses = null;
        this.cardType = cardType;
        this.cardSubType = cardSubType;
    }

    /// <summary>
    /// CardSystem의 UseVisualGrid == false인 GridTargetMode용
    /// </summary>
    /// <param name="gridTagetMode"></param>
    public EffectInfo(GridTargetMode gridTagetMode, CardType cardType, CardSubType cardSubType)
    {
        this.targets = null;
        this.caster = null;
        this.gridTargetMode = gridTagetMode;
        this.targetPoses = null;
        this.cardType = cardType;
        this.cardSubType = cardSubType;
    }

    /// <summary>
    /// CardSystem의 UseVisualGrid == true일 때, 및 AddedSECondition == Grid의 AddedStatusEffect용
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="caster"></param>
    public EffectInfo(List<Vector2Int> targetPoses, CombatantView caster, CardType cardType, CardSubType cardSubType)
    {
        this.targets = null;
        this.caster = caster;
        this.gridTargetMode = null;
        this.targetPoses = targetPoses;
        this.cardType = cardType;
        this.cardSubType = cardSubType;
    }

    /// <summary>
    /// CardSystem의 UseVisualGrid == true인 GridTargetMode용
    /// </summary>
    /// <param name="targetPoses"></param>
    public EffectInfo(List<Vector2Int> targetPoses, GridTargetMode gridTagetMode, CombatantView caster, CardType cardType, CardSubType cardSubType)
    {
        this.targets = null;
        this.caster = caster;
        this.gridTargetMode = gridTagetMode;
        this.targetPoses = targetPoses;
        this.cardType = cardType;
        this.cardSubType = cardSubType;
    }
}


