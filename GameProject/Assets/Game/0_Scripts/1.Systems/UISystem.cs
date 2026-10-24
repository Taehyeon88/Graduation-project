using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystem : Singleton<UISystem>
{
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private DemoUI endDemoUI;
    [SerializeField] private GameObject gameClearUI;

    //게임 클리어
    public void OnGameClearUI()
    {
        gameClearUI.SetActive(true);
    }

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
