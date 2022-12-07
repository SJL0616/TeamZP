using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundUiLoad : MonoBehaviour
{
    public static SoundUiLoad instance
    {
        get;
        private set;
    }

    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
            return;
        }
        DontDestroyOnLoad(instance);
    }

    //디버깅 용
    void Start()
    {
        Debug.Assert(this);
    }

    private void OnApplicationQuit()
    {
        instance = null;
    }

}
