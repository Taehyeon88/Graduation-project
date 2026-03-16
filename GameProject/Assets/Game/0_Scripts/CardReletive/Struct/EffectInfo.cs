using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public struct EffectInfo
{
    public List<CombatantView> targets;
    public CombatantView caster;
    public GridTargetMode gridTargetMode;
    public List<Vector2Int> targetPoses;

    /// <summary>
    /// CardSystem의 UseVisualGrid == true 및 AddedSECondition == CombatantView의 AddedStatusEffect용
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="caster"></param>
    public EffectInfo(List<CombatantView> targets, CombatantView caster)
    {
        this.targets = targets;
        this.caster = caster;
        this.gridTargetMode = null;
        this.targetPoses = null;
    }

    /// <summary>
    /// CardSystem의 SelfEffects용
    /// </summary>
    /// <param name="target"></param>
    public EffectInfo(CombatantView target)
    {
        this.targets = new() { target };
        this.caster = target;
        this.gridTargetMode = null;
        this.targetPoses = null;
    }

    /// <summary>
    /// CardSystem의 UseVisualGrid == false인 GridTargetMode용
    /// </summary>
    /// <param name="gridTagetMode"></param>
    public EffectInfo(GridTargetMode gridTagetMode)
    {
        this.targets = null;
        this.caster = null;
        this.gridTargetMode = gridTagetMode;
        this.targetPoses = null;
    }

    /// <summary>
    /// CardSystem의 UseVisualGrid == true일 때, 및 AddedSECondition == Grid의 AddedStatusEffect용
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="caster"></param>
    public EffectInfo(List<Vector2Int> targetPoses, CombatantView caster)
    {
        this.targets = null;
        this.caster = caster;
        this.gridTargetMode = null;
        this.targetPoses = targetPoses;
    }

    /// <summary>
    /// CardSystem의 UseVisualGrid == true인 GridTargetMode용
    /// </summary>
    /// <param name="targetPoses"></param>
    public EffectInfo(List<Vector2Int> targetPoses, GridTargetMode gridTagetMode)
    {
        this.targets = null;
        this.caster = null;
        this.gridTargetMode = gridTagetMode;
        this.targetPoses = targetPoses;
    }
}


