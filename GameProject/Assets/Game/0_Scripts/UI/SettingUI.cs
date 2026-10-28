using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUI : Singleton<SettingUI>   //GAMEMASTER 하위에서 씬을 넘나듦
{
    private const string StartSceneName = "StartScene";

    [SerializeField] private Button exitButton;
    [SerializeField] private Button mainMenuButton;   //HUD Setting 아이콘으로 열었을 때만 표시
    [SerializeField] private Transform panel;

    public bool IsActive => panel.gameObject.activeSelf;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }

    private void OnEnable()
    {
        exitButton.onClick.AddListener(ExitSettingUI);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(ExitSettingUI);
        mainMenuButton.onClick.RemoveListener(GoToMainMenu);
    }

    private void Update()
    {
        if (Instance != this) return;

        if (IsActive && Input.GetKeyDown(KeyCode.Escape)) ExitSettingUI();   //X 버튼과 동일 동작
    }

    //Publics
    public void SetActiveSettingUI(bool active, bool showMainMenu = false)
    {
        panel.gameObject.SetActive(active);
        mainMenuButton.gameObject.SetActive(active && showMainMenu);
        if (active) mainMenuButton.interactable = true;
    }

    //HUD Setting 아이콘의 onClick에 연결. StartScene에서는 메인 화면 이동 버튼을 숨김
    public void OpenFromHUD()
    {
        bool isStartScene = SceneManager.GetActiveScene().name == StartSceneName;
        SetActiveSettingUI(true, !isStartScene);
    }

    //Privates
    private void ExitSettingUI()
    {
        SetActiveSettingUI(false);
        PlayerPrefs.Save();   //루트는 활성 유지라 OnDisable이 없으므로 닫을 때 볼륨 설정을 디스크에 반영
    }

    private void GoToMainMenu()
    {
        mainMenuButton.interactable = false;   //연타 방지
        ExitSettingUI();
        FadeSystem.Instance.LoadSceneWithFade(StartSceneName);
    }
}
