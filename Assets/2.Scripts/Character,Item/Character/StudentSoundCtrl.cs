using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//학생(인간)케릭터 클래스 사운드 컨트롤 스크립트
// 작성자 :이상준
// 내용 : 지속 효과음, 1회성 효과음 On/Off
// 마지막 수정일 : 2022.11.20
public class StudentSoundCtrl : MonoBehaviour
{
    public AudioClip[] idle;   // 서있을 때 효과음 배열
    public AudioClip[] walk;   // 걷기 효과음 배열
    private int walkIdx;       // 효과음 인덱스 설정용 int형 변수
    public AudioClip[] run;    // 뛰기 효과음 배열
    public AudioClip[] attack; // 공격 효과음 배열
    public AudioClip[] hit;    // 피격 효과음 배열
    public AudioClip[] scary;  // 무서울 때 내는 효과음 배열
    public AudioClip[] health; // 치료 시 나는 효과음 배열
    public AudioClip[] drink;  // 마실 떄 나는 효과음 배열
    public AudioClip[] death;  // 죽을 때 나는 효과음 배열
    public PhotonView pv;      // 포톤뷰 변수
    private AudioSource audio; // 지속 효과음용 AudioSource
    private AudioSource audioBody;// 1회성 효과음용 AudioSource

    public float soundDelay;   // 지속 효과음 딜레이 부여용 float 변수
    public float bodySoundDelay;// 1회성 효과음 딜레이 부여용 float 변수
    public string nowSound;    // 지금 재생되고 있는 사운드(지속 사운드) 체크용 string형 변수 
    public string nowBodySound; // 지금 재생되고 있는 사운드(1회성 사운드) 체크용 string형 변수  
   
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        audio = GetComponent<AudioSource>();
        audioBody = transform.Find("StudentBody").GetComponent<AudioSource>();
    }

    private void Start()
    {
        walkIdx = 0;
        soundDelay = 0;
        bodySoundDelay = 0;
        nowBodySound = "";
        nowSound = "";
    }

    //지속 효과음 재생 메서드
    public void PlaySound(string name)
    {

        if (soundDelay > 0) return;  
        if (nowSound != name && nowSound != "")
        {
            audio.Stop();
            pv.RPC("Net_SoundOff", PhotonTargets.Others, "audio");
        }
        switch (name)
        {
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                soundDelay = Random.Range(7.0f, 13.0f);
                audio.Play();
                break;
            case "Walk":
                Debug.Log("walk");
                audio.clip = walk[walkIdx];
                walkIdx++;
                soundDelay = 0.48f;
                audio.volume = 0.2f;
                if (walkIdx > 1)
                {
                    walkIdx = 0;
                }
                if (!audio.isPlaying)
                {
                    audio.Play();
                    pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                }
                break;
            case "Run":
                audio.volume = 1f;
                if (!audio.isPlaying)
                {
                    audio.clip = run[0];
                    audio.Play();
                    pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                }
                break;
            case "Attack":
                audio.clip = attack[Random.Range(0, attack.Length)];
                soundDelay = Random.Range(5.0f, 7.0f);
                audio.Play();
                break;
        }
        nowSound = name;
    }

    //1회성 효과음 재생 메서드
    public void PlayBodySound(string name)
    {
        if (bodySoundDelay > 0 && name == "Scary") { return; }
        if (nowBodySound != name && nowBodySound != "")
        {
            audioBody.Stop();
            pv.RPC("Net_SoundOff", PhotonTargets.Others, "audioBody");

        }
        switch (name)
        {

            case "Hit": //Hit 는 사운드 딜레이 없음
                Debug.Log("is Hitted");
                int num = Random.Range(0, hit.Length);
                Debug.Log(num);
                if (audio.isPlaying)
                {
                    soundDelay = 1.0f;
                    audio.Stop();
                }

                nowBodySound = name;
                audioBody.clip = hit[num];
                audioBody.volume = 1f;
                audioBody.Play();
                break;
            case "Scary":
                audioBody.clip = scary[Random.Range(0, scary.Length)];
                audioBody.Play();
                bodySoundDelay = 3.5f;
                break;
            case "Health":
                audioBody.clip = health[Random.Range(0, health.Length)];
                audioBody.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Drink":
                audioBody.clip = drink[Random.Range(0, drink.Length)];
                audioBody.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Death":
                audioBody.clip = death[Random.Range(0, death.Length)];
                audioBody.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
        }
        nowBodySound = name;
    }

    //포톤 RPC 효과음 재생 메서드
    [PunRPC]
    void Net_PlaySound(string name)
    {

        switch (name)
        {
            case "None":
                if (audio.isPlaying)
                {
                    audio.Stop();
                }
                break;
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                audio.Play();
                break;
            case "Walk":
                audio.clip = walk[walkIdx];
                walkIdx++;
                audio.volume = 0.8f;
                if (walkIdx > 1)
                {
                    walkIdx = 0;
                }
                if (!audio.isPlaying)
                {
                    audio.Play();
                }
                break;
            case "Run":
                audio.clip = run[0];
                audio.Play();

                break;
            case "Attack":
                audio.clip = attack[Random.Range(0, attack.Length)];
                audio.Play();
                break;
            case "Scary":
                audio.clip = scary[Random.Range(0, scary.Length)];
                audio.Play();
                break;
            case "Health":
                audioBody.clip = health[Random.Range(0, scary.Length)];
                audioBody.Play();
                break;
            case "Drink":
                audioBody.clip = drink[Random.Range(0, scary.Length)];
                audioBody.Play();
                break;
            case "Death":
                audioBody.clip = death[Random.Range(0, scary.Length)];
                audioBody.Play();
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
        { // 0 이하일 때 0으로 초기화
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
