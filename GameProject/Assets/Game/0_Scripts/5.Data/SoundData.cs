using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Data/SoundData")]
public class SoundData : ScriptableObject
{
    public int SoundId => int.Parse(name.Substring(0, name.IndexOf(".")));
    [field: SerializeField] public AudioType AudioType { get; private set; }
    [field: SerializeField] public AudioClip Clip { get; private set; }

    [field: SerializeField] public float Volume { get; private set; } = 1f;
    [field: SerializeField] public bool Loop { get; private set; } = false;
}
