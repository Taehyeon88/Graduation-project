using System;

[Serializable]
public struct GrowthNodePrerequisite   //공용 트리 선행 조건 ("A n+" = Node를 Level 이상 구매)
{
    public GrowthNodeData Node;
    public int Level;
}
