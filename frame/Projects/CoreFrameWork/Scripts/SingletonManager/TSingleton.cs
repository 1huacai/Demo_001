using UnityEngine;
using System;
using System.Collections;

public class TSingleton<T> : MonoBehaviour where T : TSingleton<T>
{
    public static bool IsSingletonCreated
    {
        get
        {
            return (TSingleton<T>.ms_Singleton != null);
        }
    }

    private static T ms_Singleton;
    public static T Inst
    {
        get
        {
            if (ms_Singleton != null)
                return ms_Singleton;

            ms_Singleton = FindObjectOfType<T>();

            if (ms_Singleton != null)
                return ms_Singleton;

            Init();

            return ms_Singleton;
        }
    }

    //--------------------------------------------------------------------------------------------

    protected virtual void Awake()
    {
        if (ms_Singleton != null && ms_Singleton != this as T)
        {
            Destroy(base.gameObject);
            return;
        }

        ms_Singleton = this as T;

        DontDestroyOnLoad(this);
    }

    protected virtual void Update()
    {

    }

    protected virtual void OnDestroy()
    {

    }

    //--------------------------------------------------------------------------------------------

    public static T Init()
    {
        if (ms_Singleton != null)
        {
            return ms_Singleton;
        }
        GameObject single = new GameObject(typeof(T).Name);
        ms_Singleton = single.AddComponent<T>();
        return ms_Singleton;
    }
}
