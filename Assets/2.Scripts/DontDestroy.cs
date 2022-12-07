using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    public static DontDestroy obj = null;
    void Awake()
    {
        if (obj == null)
        {
            obj = this;
            
        }
        else if(obj != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}
