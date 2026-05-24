using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected bool cannotInitialize = false;
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            cannotInitialize = true;
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
    }
    protected virtual void OnDestroy()
    {
        if (cannotInitialize) return;

        Instance = null;
    }
    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }

    protected void DontDestroyOnLoad()
    {
        if (Instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
