using UnityEngine;
using UnityEngine.UI;

public class StartSceneSystem : Singleton<StartSceneSystem>
{
    private const int BgmSoundId = 5001;

    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button endGameButton;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }
    private void Start()
    {
        if (Instance != this) return;
        SoundSystem.Instance.PlaySound(BgmSoundId);
    }
    private void OnEnable()
    {
        startGameButton.onClick.AddListener(StartGame);
        settingButton.onClick.AddListener(OnSettingUI);
        endGameButton.onClick.AddListener(EndGame);
    }
    private void OnDisable()
    {
        startGameButton.onClick.RemoveListener(StartGame);
        settingButton.onClick.RemoveListener(OnSettingUI);
        endGameButton.onClick.RemoveListener(EndGame);
    }

    private void StartGame()
    {
        GameSystem.Instance.StartFromScratch();
    }

    private void OnSettingUI()
    {
        SettingUI.Instance.SetActiveSettingUI(true);
    }

    private void EndGame()
    {
        Application.Quit();
    }

    //Publics

    public void OffSettingUI()
    {
        SettingUI.Instance.SetActiveSettingUI(false);
    }
}
