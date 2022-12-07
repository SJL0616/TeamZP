using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public GameObject[] btns;
    bool Active = false;

    //2022-11-18 추가 사운드 전환
    public int sNum = 1;
    private csSoundManager _sMgr;

    void Awake()
    {
        _sMgr = GameObject.Find("SoundManager").GetComponent<csSoundManager>();
    }

    void Start()
    {
        //문제 : 방 들어갔다 로비로 나오면 브금 유지가 안됨 - 해결 완

        if (_sMgr.BgmCheck(sNum))
        {
            return;
        }
        else
        {
            _sMgr.PlayBgm(sNum);
        }
    }

    public void OnPlayClick()
    {
        if (!Active)
        {
            btns[0].SetActive(true);
            Active = true;
        }
        else
        {
            btns[0].SetActive(false);
            Active = false;
        }
    }

    public void OnRankingClick()
    {
        if (!Active)
        {
            btns[1].SetActive(true);
            Active = true;
        }
        else
        {
            btns[1].SetActive(false);
            Active = false;
        }
    }

    //추후 수정 필요 - 주석처리나 별도의 씬으로 옮기기...ㅜㅜ
    public void OnTutorialClick()
    {
        if (!Active)
        {
            btns[2].SetActive(true);
            Active = true;
        }
        else
        {
            btns[2].SetActive(false);
            Active = false;
        }
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
