using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundBtnManager : MonoBehaviour
{
    public GameObject sBar;
    bool Active = false;

    public void OnSoundBtnClick()
    {
        if (!Active)
        {
            sBar.SetActive(true);
            Active = true;
        }
        else
        {
            sBar.SetActive(false);
            Active = false;
        }

    }
    
}
