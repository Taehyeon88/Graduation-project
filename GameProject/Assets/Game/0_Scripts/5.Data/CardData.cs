using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public HeroData Unit { get; private set; }   //카드가 가리키는 용병
    [field: SerializeField] public int APCost { get; private set; }      //배치에 드는 AP
}
