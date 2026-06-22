using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Transform panel;

    public bool IsActive => panel.gameObject.activeSelf;

    private void OnEnable()
    {
        exitButton.onClick.AddListener(ExitSettingUI);
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(ExitSettingUI);
    }

    private void ExitSettingUI()
    {
        panel.gameObject.SetActive(false);
    }

    public void SetActiveSettingUI(bool active)
    {
        panel.gameObject.SetActive(active);
    }
}
