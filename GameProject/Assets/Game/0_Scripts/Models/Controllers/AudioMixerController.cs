using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioMixerController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicMasterSlider;
    [SerializeField] private Slider musicBGMSlider;
    [SerializeField] private Slider musicSFXSlider;

    //슬라이더 MinValue 0.001 사운드 볼륨은 Log10 단위로 되어있기 때문에

    private void OnEnable()
    {
        if (!PlayerPrefs.HasKey("Master"))
            PlayerPrefs.SetFloat("Master", 1f);
        if (!PlayerPrefs.HasKey("BGM"))
            PlayerPrefs.SetFloat("BGM", 1f);
        if (!PlayerPrefs.HasKey("SFX"))
            PlayerPrefs.SetFloat("SFX", 1f);

        //받아오기
        musicMasterSlider.value = PlayerPrefs.GetFloat("Master");
        musicBGMSlider.value = PlayerPrefs.GetFloat("BGM");
        musicSFXSlider.value = PlayerPrefs.GetFloat("SFX");
        //적용
        SetMasterVolume(musicMasterSlider.value);
        SetBGMVolume(musicBGMSlider.value);
        SetSFXVolume(musicSFXSlider.value);

        musicMasterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicBGMSlider.onValueChanged.AddListener(SetBGMVolume);
        musicSFXSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    private void OnDisable()
    {
        //저장
        PlayerPrefs.SetFloat("Master", musicMasterSlider.value);
        PlayerPrefs.SetFloat("BGM", musicBGMSlider.value);
        PlayerPrefs.SetFloat("SFX", musicSFXSlider.value);

        musicMasterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        musicBGMSlider.onValueChanged.RemoveListener(SetBGMVolume);
        musicSFXSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
        Debug.Log($"볼륨: {volume}, 진짜 볼륨: {Mathf.Log10(volume) * 20}");
    }
    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
    }
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }
}
