using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Object")]
public class ObjectData : TokenData
{
    [field: SerializeField] public int ObjectId {get; private set;}
}
