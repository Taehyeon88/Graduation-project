using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrowthNodeSystem : Singleton<GrowthNodeSystem>   //스테이지 선택 씬: 공용 트리 규칙 판단·실행 (노드 레벨 저장은 GameSystem)
{
    public const int StarsPerLevel = 1;   //노드 1레벨 = 별 1 (성장 시스템 기획서 확정 규칙)

    [SerializeField] private GrowthNodeData[] nodes;   //트리 전체 노드 (본부 제외)

    public event System.Action Changed;   //구매·초기화 후 (UI 갱신용)

    public IReadOnlyList<GrowthNodeData> Nodes => nodes;

    //남은 별 = 별 기록 합계 − 구매 레벨 합계 (저장하지 않고 매번 계산)
    public int AvailableStars => Game.Stars - SpentStars;
    public int SpentStars => Game.TotalNodeLevels * StarsPerLevel;

    private GameSystem Game => GameSystem.Instance;

    //Publics
    public int GetLevel(GrowthNodeData node) => Game.GetNodeLevel(node);

    public bool IsMaxLevel(GrowthNodeData node) => GetLevel(node) >= node.MaxLevel;

    //선행 조건 모두 충족 (비어 있으면 본부에서 바로 열림)
    public bool IsUnlocked(GrowthNodeData node)
        => node.Prerequisites.All(IsSatisfied);

    public bool IsSatisfied(GrowthNodePrerequisite prerequisite)
        => GetLevel(prerequisite.Node) >= prerequisite.Level;

    public GrowthNodeState GetState(GrowthNodeData node)
    {
        if (node.Type == GrowthNodeType.NONE) return GrowthNodeState.NOT_IMPLEMENTED;
        if (!IsUnlocked(node)) return GrowthNodeState.LOCKED;
        if (IsMaxLevel(node)) return GrowthNodeState.MAX;
        if (AvailableStars < StarsPerLevel) return GrowthNodeState.NOT_ENOUGH_STARS;
        return GrowthNodeState.PURCHASABLE;
    }

    public bool CanPurchase(GrowthNodeData node)
        => node != null && nodes.Contains(node) && GetState(node) == GrowthNodeState.PURCHASABLE;

    //1레벨 구매 (별 1)
    public bool Purchase(GrowthNodeData node)
    {
        if (!CanPurchase(node)) return false;

        Game.AddNodeLevel(node);
        Changed?.Invoke();
        return true;
    }

    //전체 초기화 — 모든 노드 레벨 0, 쓴 별 전부 환급 (일부 되돌리기는 선행 조건이 깨져 허용하지 않음)
    public void ResetAll()
    {
        if (SpentStars == 0) return;

        Game.ClearNodeLevels();
        Changed?.Invoke();
    }

    //해당 레벨의 누적 효과 설명 (레벨 0이면 null)
    public static string GetEffectText(GrowthNodeData node, int level)
        => level > 0 ? string.Format(node.EffectFormat, level * node.AmountPerLevel) : null;
}
