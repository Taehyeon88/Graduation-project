using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystem : Singleton<UISystem>
{
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private DemoUI endDemoUI;

    //게임 종료
    public void OnGameOverUI()
    {
        gameOverUI.gameObject.SetActive(true);
    }

    //데모 종료
    public void EndDemoUI()
    {
        endDemoUI.gameObject.SetActive(true);
    }
}
