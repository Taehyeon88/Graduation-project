using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Data/SoundData")]
public class SoundData : ScriptableObject
{
    [field: SerializeField] public int SoundId { get; private set; }
    [field: SerializeField] public AudioType AudioType { get; private set; } = AudioType.SFX;
    [field: SerializeField] public AudioClip Clip { get; private set; }
    [field: SerializeField] public float Volume { get; private set; } = 1f;
    [field: SerializeField] public bool Loop { get; private set; } = false;
    [SerializeField] private AudioEditorType editorType;
}
