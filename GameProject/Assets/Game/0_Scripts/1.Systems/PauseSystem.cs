using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseSystem : Singleton<PauseSystem>
{
    private bool pauseState = false;

    public bool IsPausing() => pauseState;

    public void SetPause(bool pause)
    {
        if (pauseState == pause)
        {
            Debug.LogWarning($"이미 일시정지가 {pauseState}인 상태입니다.");
            return;
        }

        //일시정지/취소 처리
        Time.timeScale = pause ? 0 : 1;
        ActionSystem.Instance.SetPause(pause);

        pauseState = pause;
    }

}
