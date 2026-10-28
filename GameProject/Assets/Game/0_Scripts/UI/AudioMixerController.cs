using UnityEngine;
using UnityEngine.UI;

//설정창 슬라이더 표시·저장 전용 (믹서 적용은 SoundSystem)
public class AudioMixerController : MonoBehaviour
{
    [SerializeField] private Slider musicMasterSlider;
    [SerializeField] private Slider musicBGMSlider;
    [SerializeField] private Slider musicSFXSlider;

    //슬라이더 MinValue 0.00001 사운드 볼륨은 Log10 단위로 되어있기 때문에
    private void OnEnable()
    {
        //받아오기 (리스너 등록 전이라 적용·저장은 일어나지 않음)
        musicMasterSlider.value = PlayerPrefs.GetFloat("Master", 1f);
        musicBGMSlider.value = PlayerPrefs.GetFloat("BGM", 1f);
        musicSFXSlider.value = PlayerPrefs.GetFloat("SFX", 1f);

        musicMasterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicBGMSlider.onValueChanged.AddListener(SetBGMVolume);
        musicSFXSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    private void OnDisable()
    {
        musicMasterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        musicBGMSlider.onValueChanged.RemoveListener(SetBGMVolume);
        musicSFXSlider.onValueChanged.RemoveListener(SetSFXVolume);

        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float volume) => ApplyVolume("Master", volume);
    public void SetBGMVolume(float volume) => ApplyVolume("BGM", volume);
    public void SetSFXVolume(float volume) => ApplyVolume("SFX", volume);

    private void ApplyVolume(string key, float volume)
    {
        PlayerPrefs.SetFloat(key, volume);
        SoundSystem.Instance.SetVolume(key, volume);
    }
}
