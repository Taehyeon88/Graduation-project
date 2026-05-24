using SerializeReferenceEditor;
using UnityEngine;

[System.Serializable]
public class CustomSquenceAndSound
{
    [field : SerializeReference, SR] public CustomSquence CustomSquence {  get; private set; }


    [SerializeField] private bool useSound;
    [ShowIf("useSound")]
    [SerializeField] private int soundId;

    [HideInInspector] public int SoundId => soundId;
    [HideInInspector] public bool UseSound => useSound;
}
