using DG.Tweening;
using IsoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Vector2Int position;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {

            Vector3 world = TokenSystem.Instance.IsoWorld.IsoToScreen(
                            new Vector3(position.x, position.y, 1f));
            Debug.Log($"{position} 위의 토큰의 world 위치는 {world} 이다");

            //Vector3 pos = TokenSystem.Instance.IsoWorld.IsoToScreen(new Vector3(position.x, position.y, 1));
            //Debug.Log($"{position} 위의 토큰의 화면상 위치는 {pos} 이다");

            //Vector3 mousePos = Input.mousePosition;
            //Vector3 pos = TokenSystem.Instance.IsoWorld.ScreenToIso(mousePos);
            //Debug.Log($"마우스 위치 {mousePos}에 iso 위치는 {pos} 이다");
        }
    }
}
