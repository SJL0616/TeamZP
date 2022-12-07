using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class csMainCheck : MonoBehaviour
{
    //2022-11-18 추가 (사운드 매니저 메인 씬으로 이동)
    public int sNum = 2;
    private csSoundManager _sMgr;

    //2022-11-21 추가
    //튜토리얼씬 삭제 후 씬 병합 위한 변수 추가
    public GameObject startBtn;
    public GameObject startVideo;
    public GameObject firstUser;
    public GameObject firstUserSet;

    private void Awake()
    {
        //태그로 바꿔도 ㄱㅊ 
        _sMgr = GameObject.Find("SoundManager").GetComponent<csSoundManager>();
    }

    private void Start()
    {
        _sMgr.PlayBgm(sNum);
    }

    public void OnCheck()
    {
        if (PlayerPrefs.GetInt("ISSAVE") == 0)
        {
            //2022-11-21 주석처리(튜토리얼 씬 삭제)
            //SceneManager.LoadScene("Tutorial");
            startBtn.SetActive(false);
            startVideo.SetActive(false);
            firstUser.SetActive(true);
            firstUserSet.SetActive(true);

            LoadData();
        }
        else if (PlayerPrefs.GetInt("ISSAVE") == 1)
        {

            SceneManager.LoadScene("scLobby");
        }
    }

    public void LoadData()
    {
        int isSave = PlayerPrefs.GetInt("ISSAVE");
        if (isSave == 0)
        {
            PlayerPrefs.SetInt("ISSAVE", 1);
        }
    }
}
