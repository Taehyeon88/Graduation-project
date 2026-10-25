using UnityEngine;
using UnityEngine.UI;

public class TavernUI : MonoBehaviour
{
    [SerializeField] private GameObject handRoot;          //손패 UI (HandView)
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button getRumorButton;
    [SerializeField] private RumorSelectUI rumorSelectUI;
    [SerializeField] private Button exitButton;

    private void OnEnable()
    {
        cancelButton.onClick.AddListener(Cancel);
        getRumorButton.onClick.AddListener(OpenRumorSelect);
        exitButton.onClick.AddListener(Exit);

        ShowHand();
    }

    private void OnDisable()
    {
        cancelButton.onClick.RemoveListener(Cancel);
        getRumorButton.onClick.RemoveListener(OpenRumorSelect);
        exitButton.onClick.RemoveListener(Exit);
    }

    //Publics
    public void ShowHand()   //배치 전: 손패만
    {
        handRoot.SetActive(true);
        SetActionButtons(false);
        rumorSelectUI.Close();
        exitButton.gameObject.SetActive(false);
    }

    public void ShowActions()   //배치 후: 손패 대신 취소·소문 얻기
    {
        handRoot.SetActive(false);
        SetActionButtons(true);
    }

    public void ShowExit()   //소문 부여 후: 나가기만
    {
        SetActionButtons(false);
        rumorSelectUI.Close();
        exitButton.gameObject.SetActive(true);
    }

    //Privates
    private void SetActionButtons(bool active)
    {
        cancelButton.gameObject.SetActive(active);
        getRumorButton.gameObject.SetActive(active);
    }

    private void Cancel()
    {
        TavernSystem.Instance.Cancel();
    }

    private void OpenRumorSelect()
    {
        SetActionButtons(false);
        rumorSelectUI.Open();
    }

    private void Exit()
    {
        TavernSystem.Instance.Exit();
    }
}
