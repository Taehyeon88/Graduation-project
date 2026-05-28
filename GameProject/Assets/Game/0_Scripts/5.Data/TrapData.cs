using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Trap")]
public class TrapData : TokenData
{
    [field: SerializeReference, SR] public Trap Trap { get; private set; }
}
