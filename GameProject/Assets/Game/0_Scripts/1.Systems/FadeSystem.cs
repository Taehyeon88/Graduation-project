using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

//전체 UI 위를 덮는 검은 화면 Fade (GAMEMASTER 하위에 두어 씬을 넘나듦)
public class FadeSystem : Singleton<FadeSystem>
{
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private float fadeDuration = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //화면이 검게 덮임 (끝날 때까지 입력 차단)
    public Tween FadeIn()
    {
        overlay.blocksRaycasts = true;
        return overlay.DOFade(1f, fadeDuration);
    }

    //검은 화면이 걷힘
    public Tween FadeOut()
    {
        return overlay.DOFade(0f, fadeDuration)
            .OnComplete(() => overlay.blocksRaycasts = false);
    }

    public void LoadSceneWithFade(string sceneName)
    {
        FadeIn().OnComplete(() => SceneManager.LoadScene(sceneName));
    }

    //Events
    //Fade로 넘어온 씬이면 검은 화면을 걷는다
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (cannotInitialize) return;   //Singleton이 파괴할 씬 쪽 복제본

        if (overlay.alpha > 0f) FadeOut();
    }
}
