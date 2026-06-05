using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private Button exitButton;

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
        gameObject.SetActive(false);
    }
}
