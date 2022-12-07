using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//좀비 클래스 사운드 컨트롤 스크립트
// 작성자 :이상준
// 내용 : 지속 효과음, 1회성 효과음 On/Off
// 마지막 수정일 : 2022.11.20
public class ZombieSoundCtrl : MonoBehaviour
{
    public AudioClip[] walk;      // 걷기 효과음 배열
    private int walkIdx;          // 효과음 인덱스 설정용 int형 변수
    public AudioClip[] dash;      // 대쉬 공격 발소리 효과음 배열
    public AudioClip[] dashAttack;// 대쉬 공격 음성 효과음
    public AudioClip[] idle;      // 서있을 때 효과음 배열
    public AudioClip[] attack;    // 할퀴기 공격 효과음 배열
    public AudioClip[] hit;       // 피격 음성 효과음
    public AudioClip[] bite;      // 물기 음성 효과음
    public AudioClip transition;  // 인간에서 좀비 변이 효과음
    public PhotonView pv;         //포톤 뷰 변수
    public AudioSource audio;     // 지속 효과음용 AudioSource
    public AudioSource bodyAudio; // 1회성 효과음용 AudioSource

    public float soundDelay;      // 지속 효과음 딜레이 부여용 float 변수
    public float bodySoundDelay;  // 1회성 효과음 딜레이 부여용 float 변수
    public string nowSound;       // 지금 재생되고 있는 사운드(지속 사운드) 체크용 string형 변수 
    public string nowBodySound;   // 지금 재생되고 있는 사운드(1회성 사운드) 체크용 string형 변수  
    
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        audio = GetComponent<AudioSource>();
        bodyAudio = transform.Find("ZombieBody").GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        walkIdx = 0;
        soundDelay = 0;
        bodySoundDelay = 0; 
        nowBodySound = "";
        nowSound = "";
    }

    //지속 효과음 재생 메서드
    public void PlaySound(string name)
    {//1회성 소리 재생 메서드
        if (soundDelay != 0 && name == "Idle") return;
        if (nowSound != name && nowSound != "") { 
            audio.Stop(); 
            pv.RPC("Net_SoundOff", PhotonTargets.Others, "audio"); 
        }

        switch (name)
        {
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                soundDelay = Random.Range(7.0f, 13.0f);
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Attack":
                nowSound = name;
                audio.clip = attack[Random.Range(0, attack.Length)];
                soundDelay = Random.Range(5.0f, 7.0f);
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "DashAttack":
                nowSound = name;
                audio.clip = dashAttack[Random.Range(0, dashAttack.Length)];
                soundDelay = Random.Range(5.0f, 7.0f);
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Bite"://Bite 는 사운드 딜레이 없음
                nowSound = name;
                audio.clip = bite[Random.Range(0, bite.Length)];
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Transition":
                nowSound = name;
                audio.clip = transition;
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Hit":
                nowSound = name;
                audio.clip = hit[Random.Range(0, hit.Length)];
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
        }
        nowSound = name;
    }

    //1회성 효과음 재생 메서드
    public void PlayBodySound(string name, bool isOff = false)
    { //발소리같은 지속적인 소리 재생 메서드
        if (isOff) { bodyAudio.Stop(); pv.RPC("Net_PlayBodySound", PhotonTargets.Others, "None"); return; }
        if ((bodySoundDelay != 0 || name == "None") && name != "Dash") return;
        if (nowBodySound != name && nowSound != "") { bodyAudio.Stop(); pv.RPC("Net_SoundOff", PhotonTargets.Others, "bodyAudio"); }


        switch (name)
        {
            case "Walk":
                nowBodySound = name;
                bodyAudio.clip = walk[walkIdx++];
                bodySoundDelay = 0.4f;
                bodyAudio.Play();
                pv.RPC("Net_PlayBodySound", PhotonTargets.Others, name);
                if (walkIdx > 1) walkIdx = 0;
                break;
            case "Dash":
                Debug.Log("Dash sound ");
                nowBodySound = name;
                bodyAudio.clip = dash[0];
                bodySoundDelay = 3;
                bodyAudio.Play();
                pv.RPC("Net_PlayBodySound", PhotonTargets.Others, name);

                break;
        }
    }

    //포톤 RPC 지속 효과음 재생 메서드
    [PunRPC]
    public void Net_PlaySound(string name)
    {
        switch (name)
        {
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                audio.Play();
                break;
            case "Attack":
                audio.clip = attack[Random.Range(0, attack.Length)];
                audio.Play();
                break;
            case "DashAttack":
                audio.clip = dashAttack[Random.Range(0, dashAttack.Length)];
                audio.Play();
                break;
            case "Bite":
                audio.clip = bite[Random.Range(0, bite.Length)];
                audio.Play();
                break;
            case "Hit":
                audio.clip = hit[Random.Range(0, hit.Length)];
                audio.Play();
                break;
        }
    }

    //포톤 RPC 1회성 효과음 재생 메서드
    [PunRPC]
    public void Net_PlayBodySound(string name)
    {
        switch (name)
        {
            case "Walk":
                bodyAudio.clip = walk[walkIdx++];
                bodyAudio.Play();
                if (walkIdx > 1) walkIdx = 0;
                break;
            case "Dash":
                bodyAudio.clip = dash[0];
                bodyAudio.Play();
                break;
            case "None":
                bodyAudio.Stop();
                break;
        }
    }

   

    // Update is called once per frame
    void Update()
    {
        //사운드 딜레이값 -=Time.deltaTime
        if (soundDelay >= 0f)
        {
            soundDelay -= Time.deltaTime;
        }
        else
        {// 0 이하일 때 0으로 초기화
            soundDelay = 0;
        }

        if (bodySoundDelay >= 0f)
        {
            bodySoundDelay -= Time.deltaTime;
        }
        else
        {
            bodySoundDelay = 0;
        }
    }
}
