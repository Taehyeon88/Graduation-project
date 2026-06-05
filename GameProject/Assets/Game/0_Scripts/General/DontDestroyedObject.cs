using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyedObject : Singleton<DontDestroyedObject>
{
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad();
    }
}
