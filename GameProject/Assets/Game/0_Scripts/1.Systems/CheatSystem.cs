using DG.Tweening;
using TMPro;
using UnityEngine;

//C 키로 토글하는 치트 모드 (GAMEMASTER 하위에 두어 씬을 넘나듦)
public class CheatSystem : Singleton<CheatSystem>
{
    [SerializeField] private CanvasGroup popup;        //우측 하단 알림 팝업
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float holdDuration = 2f;  //팝업 유지 시간
    [SerializeField] private float fadeDuration = 0.5f;

    private Sequence popupSequence;

    public bool IsCheatMode { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        popup.alpha = 0f;
    }

    private void OnDisable()
    {
        popupSequence?.Kill();
    }

    private void Update()
    {
        if (Instance != this) return;

        if (Input.GetKeyDown(KeyCode.C)) ToggleCheatMode();
    }

    private void ToggleCheatMode()
    {
        IsCheatMode = !IsCheatMode;
        ShowPopup(IsCheatMode ? "치트가 사용됨" : "치트가 해제됨");
    }

    //즉시 표시 → 유지 → 서서히 사라짐 (연타 시 이전 연출을 끊고 다시 시작)
    private void ShowPopup(string message)
    {
        popupSequence?.Kill();
        popupText.SetText(message);
        popup.alpha = 1f;

        popupSequence = DOTween.Sequence()
            .AppendInterval(holdDuration)
            .Append(popup.DOFade(0f, fadeDuration));
    }
}
