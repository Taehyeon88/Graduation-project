using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameClearGA gameClearGA = new GameClearGA();
            ActionSystem.Instance.Perform(gameClearGA);
        }
    }
}
