using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Mana { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public CardType CardType { get; private set; }
    [field: SerializeField] public CardSubType CardSubType { get; private set; }
    [field: SerializeReference, SR] public List<Effect> SelfEffects { get; private set; } = null;
    [field: SerializeField] public GridTargetMode GridTargetMode { get; private set; }

    //기타 옵션들
    public bool OtherOptions = false;
    [ShowIf("OtherOptions")]
    public bool LockDiscarding;    //손패에 그래로 남음! 사용시에만 버려짐
}
