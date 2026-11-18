using UnityEngine;

//튜토리얼 전체 단계표 (전투 → 성장 순서대로)
[CreateAssetMenu(menuName = "Data/TutorialSequence")]
public class TutorialSequenceData : ScriptableObject
{
    [field: SerializeField] public TutorialStepData[] Steps { get; private set; }
}
