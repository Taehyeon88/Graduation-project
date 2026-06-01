using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneSystem : Singleton<StartSceneSystem>
{
    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button startTutorialButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button endGameButton;

    [Header("UIPanels")]
    [SerializeField] private GameObject settingUI;

    protected override void Awake()
    {
        base.Awake();

        if(!PlayerPrefs.HasKey("Tuto"))
            PlayerPrefs.SetInt("Tuto", 1);   //첫 튜토리얼 존재 여부(1 = true, 2 = false)

        if (PlayerPrefs.GetInt("Tuto") == 1)
        {
            startGameButton.gameObject.SetActive(false);
        }
        else
        {
            startGameButton.gameObject.SetActive(true);
        }
    }
    private void OnEnable()
    {
        startGameButton.onClick.AddListener(StartGame);
        startTutorialButton.onClick.AddListener(StartTuto);
        settingButton.onClick.AddListener(OnSettingUI);
        endGameButton.onClick.AddListener(EndGame);
    }
    private void OnDisable()
    {
        startGameButton.onClick.RemoveListener(StartGame);
        startTutorialButton.onClick.RemoveListener(StartTuto);
        settingButton.onClick.RemoveListener(OnSettingUI);
        endGameButton.onClick.RemoveListener(EndGame);
    }

    private void StartGame()
    {
        GameSystem.Instance.StartFromScratch();
    }

    private void StartTuto()
    {
        if (PlayerPrefs.GetInt("Tuto") == 1)   //첫 튜토리얼 종료 처리
        {
            PlayerPrefs.SetInt("Tuto", 0);
        }

        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.IsTutorial = true;
            GameSystem.Instance.StartFromScratch();
        }
    }

    private void OnSettingUI()
    {
        if (settingUI != null)
        {
            settingUI.SetActive(true);
        }
    }

    private void EndGame()
    {
        Application.Quit();
    }

    //Publics

    public void OffSettingUI()
    {
        if (settingUI != null)
        {
            settingUI.SetActive(false);
        }
    }
}
