using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            GameClearGA gameClearGA = new();
            ActionSystem.Instance.Perform(gameClearGA);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            GameOverGA gameOverGA = new();
            ActionSystem.Instance.Perform(gameOverGA);
        }
    }
}
