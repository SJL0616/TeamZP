using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class csSoundManager : MonoBehaviour
{
    public AudioClip[] soundFile;

    public float soundVolume = 1.0f;
    public bool isSoundMute = false;

    public Slider bgmSl;
    public Toggle bgmTg;
    //필요 없어서 주석처리~
    //public GameObject bgmSound;
    //2022-11-17 유진 수정 : SCanvas에서 싱글톤 사용하기 때문에 주석처리 했는데...좀 애매;
    //static csSoundManager my;

    //오디오 소스 받아옴
    public AudioSource audio;

    //SoundBtnManager에 있는 내용 싹 옮겨옴
    public GameObject sBar;
    bool Active = false;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        LoadSoundData();

        //currentBgm = GetComponent<AudioSource>().clip.ToString();

        //if (my == null)
        //{
        //    my = this;

        //}
        //else if (my != this)
        //{
        //    Destroy(gameObject);
        //}
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        soundVolume = bgmSl.value;
        isSoundMute = bgmTg.isOn;
        AudioSet();
    }

    public void OnSoundBtnClick()
    {
        if (!Active)
        {
            sBar.SetActive(true);
            Active = true;
        }
        else
        {
            SaveSoundData();
            sBar.SetActive(false);
            Active = false;
        }

    }

    public void SetSound()
    {
        soundVolume = bgmSl.value;
        isSoundMute = bgmTg.isOn;
        AudioSet();
    }
    void AudioSet()
    {
        audio.volume = soundVolume;
        audio.mute = isSoundMute;
    }

    public void PlayBgm(int sNum)
    {
        GetComponent<AudioSource>().clip = soundFile[sNum - 1];
        AudioSet();
        GetComponent<AudioSource>().Play();
    }

    //2022-11-19 추가(브금 스위치 용)
    public bool BgmCheck(int sNum)
    {
        if (GetComponent<AudioSource>().clip == soundFile[sNum - 1])
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void PlayChasingBgm(bool isActive)
    {
        if (isActive)
        {
            GetComponent<AudioSource>().clip = GetSfx("ChasingBgm");
            AudioSet();
            GetComponent<AudioSource>().Play();
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }

    }
    #region 사운드 저장 코드
    public void SaveSoundData()
    {
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
        PlayerPrefs.SetInt("IsSoundMute", System.Convert.ToInt32(isSoundMute));
    }

    public void LoadSoundData()
    {
        bgmSl.value = PlayerPrefs.GetFloat("SoundVolume");
        bgmTg.isOn = System.Convert.ToBoolean(PlayerPrefs.GetInt("IsSoundMute"));

        int isSoundSave = PlayerPrefs.GetInt("IsSoundSave");

        if (isSoundSave == 0)
        {
            bgmSl.value = 1.0f;
            bgmTg.isOn = false;
            SaveSoundData();
            PlayerPrefs.SetInt("IsSoundSave", 1);
        }
    }


    #endregion
    //효과음 조절 여기서 하면 될 것 같은데 잘 안됨...ㅠ
    public void PlayEffect(Vector3 pos, string name)
    {
        //if (isSoundMute)
        //{
        //    return;
        //}
        Debug.Log("vector :" + pos + " , " + name);
        AudioClip sfx = null;
        sfx = GetSfx(name);
        //soundObj는 많이 쓰이기 때문에 편의를 위해 앞에 _를 붙임(자동완성 굿)
        GameObject _soundObj = new GameObject("sfx");
        //_soundObj.transform.position = pos;

        AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();
        _audioSource.clip = sfx;
        _audioSource.volume = 1;
        _audioSource.minDistance = 3.0f;
        _audioSource.spatialBlend = 1;
        _audioSource.maxDistance = 8.0f;

        _audioSource.playOnAwake = true;
        Instantiate(_soundObj, pos, Quaternion.identity);

        Destroy(_soundObj, sfx.length + 0.2f);
        // 사운드 길이에 딱 맞추면 잘리는 경우 있으므로 0.2초정도 추가
    }

    AudioClip GetSfx(string name)
    {
        AudioClip sfx = null;
        for (int i = 0; i < soundFile.Length; i++)
        {
            if (soundFile[i].name.Contains(name))
            {
                sfx = soundFile[i];

            }
        }
        return sfx;
    }
}
