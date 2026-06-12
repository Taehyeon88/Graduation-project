using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Object/Trap")]
public class TrapData : ObjectData
{
    [field: SerializeReference, SR] public Trap Trap { get; private set; }
}
