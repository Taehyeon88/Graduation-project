using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class SoundSystem : Singleton<SoundSystem>
{
    [SerializeField] private SoundData[] soundDatas;
    [SerializeField] private Transform bgmTransform;
    [SerializeField] private Transform sfxTransform;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    
    private Dictionary<int, AudioSource> soundDataById = new();

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (var data in soundDatas)
        {
            AudioSource audioSource = data.AudioType switch
            {
                AudioType.BGM => bgmTransform?.gameObject.AddComponent<AudioSource>(),
                AudioType.SFX => sfxTransform?.gameObject.AddComponent<AudioSource>(),
                _=> null
            };

            audioSource.clip = data.Clip;
            audioSource.volume = data.Volume;
            audioSource.pitch = 1;
            audioSource.loop = false;

            audioSource.outputAudioMixerGroup = data.AudioType switch
            {
                AudioType.BGM => bgmMixerGroup,
                AudioType.SFX => sfxMixerGroup,
                _ => null
            };

            //오디오 타입에 따른 오디오 믹서 설정

            if (!soundDataById.TryAdd(data.SoundId, audioSource))
                Debug.LogError($"{data.name}데이터의 효과ID가 {soundDataById[data.SoundId].name}과 {data.SoundId}으로 충돌함");
        }
    }

    public void PlaySound(int soundId)
    {
        if (soundDataById.ContainsKey(soundId))
        {
            var sound = soundDataById[soundId];
            if (sound != null)
            {
                sound.Play();
            }
        }
    }

    public void PauseSound(int soundId)
    {
        if (soundDataById.ContainsKey(soundId))
        {
            var sound = soundDataById[soundId];
            if (sound != null)
            {
                sound.Pause();
            }
        }
    }
}
