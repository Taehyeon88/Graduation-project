using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Audio;

public class SoundSystem : Singleton<SoundSystem>     //씬을 넘나들면서 사용할 시스템
{
    [SerializeField] private SoundData[] soundDatas;

    [Header("BGM,SFX Setting")]
    [SerializeField] private int bgm_Source_Count = 5;
    [SerializeField] private int sfx_Source_Count = 10;
    [SerializeField] private int interaction_Source_Count = 8;
    [SerializeField] private Transform bgmTransform;
    [SerializeField] private Transform sfxTransform;
    [SerializeField] private Transform interactionTransform;

    [Header("MixerGroup")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup interactionMixerGroup;

    private Dictionary<int, SoundData> soundDataById = new();

    private List<AudioSource> bgm_Sources = new(20);
    private List<AudioSource> sfx_Sources = new(20);
    private List<AudioSource> interaction_Sources = new(20);

    private List<AudioSource> playingSounds;

    private void OnAwake()
    {
        Initialize();
    }

    private void Initialize()
    {
        //soundDataById 데이터 생성
        foreach (var data in soundDatas)
        {
            if (data != null && !soundDataById.ContainsKey(data.SoundId))
                soundDataById.Add(data.SoundId, data);
        }

        //bgm, sfx AudioSource 생성
        playingSounds = new(20);

        for (int i = 0; i < bgm_Source_Count; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(bgmTransform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            bgm_Sources.Add(source);
        }

        for (int i = 0; i < sfx_Source_Count; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(sfxTransform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sfx_Sources.Add(source);
        }

        for (int i = 0; i < interaction_Source_Count; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(interactionTransform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            interaction_Sources.Add(source);
        }
    }

    private void Update()
    {
        if (playingSounds == null || playingSounds.Count == 0) return;

        var source = playingSounds.Find(s => s != null && !s.isPlaying && !s.loop);
        if (source != null)
        {
            playingSounds.Remove(source);
        }
    }

    //씬 이동할 경우의 예외처리
    //현재 재생 중인 모든 사운드 종료 및 playingSounds 데이터 초기화

    public void PlaySound(int soundId)
    {
        if (playingSounds == null)
            Initialize();

        if (soundDataById.TryGetValue(soundId, out var data))
        {
            AudioSource empty_source = null;
            switch (data.AudioType)
            {
                case AudioType.BGM:
                    empty_source = bgm_Sources.Find(s => !s.isPlaying);
                    break;
                case AudioType.SFX:
                    empty_source = sfx_Sources.Find(s => !s.isPlaying);
                    break;
                case AudioType.INTERACTION:
                    empty_source = interaction_Sources.Find(s => !s.isPlaying);
                    break;
            }
            if (empty_source == null)
            {
                Debug.LogError($"{data.AudioType} 타입에 비어있는 audioSource 가 없다");
                return;
            }

            empty_source.clip = data.Clip;
            empty_source.volume = data.Volume;
            empty_source.loop = data.Loop;
            empty_source.outputAudioMixerGroup = data.AudioType switch
            {
                AudioType.BGM => bgmMixerGroup,
                AudioType.SFX => sfxMixerGroup,
                AudioType.INTERACTION => interactionMixerGroup,
                _ => null
            };
            playingSounds.Add(empty_source);

            empty_source.Play();
        }
    }

    public void StopSound(int soundId)
    {
        if (soundDataById.TryGetValue(soundId, out var data))
        {
            var source = playingSounds.Find(s => s.clip == data.Clip && s.isPlaying);
            if (source != null)
            {
                source.Stop();
                playingSounds.Remove(source);
            }
        }
    }
}
