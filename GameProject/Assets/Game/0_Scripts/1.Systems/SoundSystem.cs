using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

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

    private List<AudioSource> bgmSources = new(20);
    private List<AudioSource> sfxSources = new(20);
    private List<AudioSource> interactionSources = new(20);

    private List<AudioSource> playingSounds;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //씬이 바뀌면 이어지던 브금 종료 (새 씬이 Start에서 자기 브금 재생)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this || playingSounds == null) return;

        foreach (var source in bgmSources)
        {
            source.Stop();
            playingSounds.Remove(source);
        }
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

            bgmSources.Add(source);
        }

        for (int i = 0; i < sfx_Source_Count; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(sfxTransform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sfxSources.Add(source);
        }

        for (int i = 0; i < interaction_Source_Count; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(interactionTransform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            interactionSources.Add(source);
        }
    }

    private static readonly string[] VolumeKeys = { "Master", "BGM", "SFX" };   //PlayerPrefs 키 = 믹서 노출 파라미터명

    //저장된 볼륨을 믹서에 적용 (설정 UI가 없는 씬에서도 유지되도록 영속 시스템이 담당)
    private void Start()
    {
        if (Instance != this) return;

        foreach (string key in VolumeKeys)
        {
            SetVolume(key, PlayerPrefs.GetFloat(key, 1f));
        }
    }

    //볼륨(0~)을 dB로 변환해 믹서 노출 파라미터에 적용
    public void SetVolume(string parameterName, float volume)
    {
        bgmMixerGroup.audioMixer.SetFloat(parameterName, Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
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
                    empty_source = bgmSources.Find(s => !s.isPlaying);
                    break;
                case AudioType.SFX:
                    empty_source = sfxSources.Find(s => !s.isPlaying);
                    break;
                case AudioType.INTERACTION:
                    empty_source = interactionSources.Find(s => !s.isPlaying);
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
