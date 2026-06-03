using System.Collections.Generic;
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
    private List<int> bgmIds = new();

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
                _ => null
            };

            audioSource.clip = data.Clip;
            audioSource.volume = data.Volume;
            audioSource.pitch = 1;
            audioSource.loop = data.Loop;

            audioSource.outputAudioMixerGroup = data.AudioType switch
            {
                AudioType.BGM => bgmMixerGroup,
                AudioType.SFX => sfxMixerGroup,
                _ => null
            };

            if (!soundDataById.TryAdd(data.SoundId, audioSource))
                Debug.LogError($"{data.name}데이터의 효과ID가 {soundDataById[data.SoundId].name}과 {data.SoundId}으로 충돌함");

            if (data.AudioType == AudioType.BGM)
                bgmIds.Add(data.SoundId);
        }
    }

    public void PlayBGM(int soundId)
    {
        foreach (var id in bgmIds)
        {
            if (soundDataById.TryGetValue(id, out var bgm) && bgm != null)
                bgm.Stop();
        }
        PlaySound(soundId);
    }

    public void PlaySound(int soundId)
    {
        if (soundDataById.TryGetValue(soundId, out var sound) && sound != null)
            sound.Play();
    }

    public void StopSound(int soundId)
    {
        if (soundDataById.TryGetValue(soundId, out var sound) && sound != null)
            sound.Stop();
    }

    public void PauseSound(int soundId)
    {
        if (soundDataById.TryGetValue(soundId, out var sound) && sound != null)
            sound.Pause();
    }
}
