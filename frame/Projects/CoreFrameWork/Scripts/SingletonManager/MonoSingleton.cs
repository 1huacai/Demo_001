using UnityEngine;
using System;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>,IDisposable
{
	private static T m_instance = null;

	public static T Inst
    {
        get
        {
			if (m_instance == null)
            {
            	m_instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (m_instance == null)
                {
                    ManagerGO = new GameObject(typeof(T).Name);
                    m_instance = ManagerGO.AddComponent<T>();
                    //GameObject parent = GameObject.Find("Boot");
                    //if (parent != null)
                    //{
                    //    go.transform.parent = parent.transform;
                    //}
                }
            }

            return m_instance;
        }
    }

    public static GameObject ManagerGO;

    public static T CreateInstance()
    {
        return Inst;
    }

    /*
     * 没有任何实现的函数，用于保证MonoSingleton在使用前已创建
     */
    public virtual void Startup()
    {

    }

    protected virtual void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
        }

        DontDestroyOnLoad(gameObject);
        Init();
    }

    protected virtual void Init()
    {

    }

    public void DestroySelf()
    {
        Dispose();
        MonoSingleton<T>.m_instance = null;
        UnityEngine.Object.Destroy(gameObject);
    }

    public virtual void Dispose()
    {

    }

}