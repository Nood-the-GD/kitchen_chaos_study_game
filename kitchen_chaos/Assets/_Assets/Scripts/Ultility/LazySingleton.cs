using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _s;
    private static object m_Lock = new object();
    public static T s
    {
        get
        {

            lock (m_Lock)
            {
                if (_s == null || _s.gameObject == null)
                    TryInit();
                return _s;
            }
        }
    }

    public static T ForceGetInstance()
    {
        if (_s != null)
            return _s;

        _s = (T)FindObjectOfType(typeof(T));
        if (_s == null)
        {
            GameObject singleton = new GameObject();
            _s = singleton.AddComponent<T>();
            singleton.name = typeof(T).ToString();
            Debug.Log("added " + typeof(T).ToString());
        }
        return _s;
    }

    static void TryInit()
    {
        _s = (T)FindObjectOfType(typeof(T));
        if (_s == null)
        {

            // GameObject singleton = new GameObject();
            // _s = singleton.AddComponent<T>();
            // singleton.name = typeof(T).ToString();
            // DontDestroyOnLoad(_s);
            // Debug.Log("added " + typeof(T).ToString());
        }
    }

    protected virtual bool dontDestroy => false;
    public void DontDestroy()
    {
        DontDestroyOnLoad(gameObject);
    }
    protected virtual void Awake()
    {
        if (_s == null)
            TryInit();

        if (dontDestroy)
            DontDestroy();

        if (s != null)
        {
            var list = FindObjectsOfType<T>();
            foreach (var i in list)
            {
                if (s != i)
                {
                    Destroy(i.gameObject);
                }
            }
        }
    }
    protected virtual void Start()
    {


    }

    public static void DestroyImmediate()
    {
        DestroyImmediate(s.gameObject);
    }
}
