using UnityEngine;
using System;
using Unity.IO.LowLevel.Unsafe;

public abstract class Singleton<T>:MonoBehaviour where T : MonoBehaviour
{
    static T _instance;

    public static T instance
    {
        get
        {
            if(_instance == null)
            {
                Type t = typeof(T);

                _instance = (T)FindFirstObjectByType(t);
            }

            return _instance;
        }
    }

    virtual protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            return;
        }
        else if (instance == this)
        {
            return;
        }

        Destroy(this);
    }
}

