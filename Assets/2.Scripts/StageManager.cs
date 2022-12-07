using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//스테이지 매니저 클래스 
// 초안 작성자 : 최원빈
// 추가 수정자 : 이상준, 정유진 (추가 수정한 부분에 이름 주석)
// 내용 : 게임 시작 시 플레이어 오브젝트 생성, 게임 진행, 점수 계산
// 마지막 수정일 : 2022.11.23
public class StageManager : MonoBehaviour
{
    public GameObject inventory;          // 인벤토리 UI 오브젝트
    public GameObject humanUI;            // 인간 UI 오브젝트
    public GameObject zombieUI;           // 좀비 UI 오브젝트
    public GameObject resultUI;           // 결과창 UI 오브젝트
    public GameObject resultItem;         // 결과창 출력용 UI 오브젝트
    public GameObject resultPanel;        // 결과창 출력용 UI 오브젝트
    public GameObject transitionParticle; // 변이 파티클 시스템

    private int spawnNum = 0;             // 스폰 Index로 쓸 int형 변수
    PhotonView pv;                        // 포톤뷰 변수
    bool modeUI;                          // 인간, 좀비에 따른 UI 전환용 bool형 변수
    private int myNum;                    // 인간 생성 지역 배열의 Index에 쓰일 int형 변수.
    private int leftHuman;                // 남은 인간 수를 저장할 int형 변수
    public GameObject[] HspawnPos;        // 인간 스폰 Position 게임오브젝트를 담은 배열
    public GameObject ZspawnPos;          // 좀비 스폰 Position 게임오브젝트

    private UIManager uiManager;          // UI 매니저 스크립트
    csSoundManager soundManager;          // 사운드 매니저 스크립트
    public csRoomPhoton roomPhoton;       // csRoomPhoton(룸 씬 관리 클래스) 스크립트

    //타이머 구현을 위한 변수
    bool startTimer = false;              // 마스터 클라이언트의 시작시간을 담을 float형 변수       
    double startTime;                     // 게임 시간이 시작되면 true  처리를 하기 위한 bool형 변수
    ExitGames.Client.Photon.Hashtable CustomeValue; // 포톤 Hashtable 에 저장하기 위한 변수

    //최원빈 추가 DB 용 변수 (2022-11-18)
    string GetResultURL = "http://teamzombie.dothome.co.kr/Update.php";
    List<Player> ranking = new List<Player>();
    string[] currentArray = null;
    public int score;                    // 점수 저장용 int형 변수
    public string type;                  // 인간, 좀비 타입 입력용 string형 변수
    public bool victory;                 // 승리, 패배 판별, 저장용 bool형 변수
    public double time;                  // 시간 입력용 double형 변수
    public int infection;                // 감염 시킨 인간 수 저장용 int형 변수(구현 못함)

    void Awake()
    {
        //RoomPhoton 스크립트(전 씬)에서 받은 int형 변수를 스폰 위치 index로 사용.
        myNum = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().myNum;

        spawnNum = 0;
        time = 0;
        pv = GetComponent<PhotonView>();
        soundManager = GameObject.Find("S_Canvas").transform.GetComponentInChildren<csSoundManager>();
        modeUI = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().isHuman;
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        pv.RPC("SetUI", PhotonTargets.AllBuffered, null);
        StartCoroutine(this.CreatePlayer());

        //2022-11-22 추가
        roomPhoton = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>();
        soundManager.PlayBgm(11);

        //2022-11-15 이상준 추가
        //마스터 클라이언트에서 Photon.Hashtable()에 시작 시간 입력.
        //이것을 다른 클라이언트에서 받아서 게임 시간 계산하여 표시함.
        if (PhotonNetwork.player.IsMasterClient)
        {
            Debug.Log("MASTER START");
            CustomeValue = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.time;
            startTimer = true;
            CustomeValue.Add("StartTime", startTime);
            PhotonNetwork.room.SetCustomProperties(CustomeValue);
        }
    }

    IEnumerator Start()
    {
        Camera.main.gameObject.SetActive(false);
        //2022-11-18 원빈 추가
        pv.RPC("SetUI", PhotonTargets.AllBuffered, null);
        yield return new WaitForSeconds(1f);
        yield return null;

        uiManager.SetTime(startTime, startTimer);
    }

    //최조 작성자 : 최원빈
    // 수정자 : 이상준 (추가 사항에 주석 추가)
    //내용 : 케릭터 생성 함수
    // 이상준 추가사항: 마스터클라이언트가 남은 인간의 수 기록하는 처리(SetLeftHuman 포톤 RPC 함수 추가)
    //                +  자신을 제외한 네트워크 플레이어 화면에 자신 상태 UI 표시하는 처리( SetPlayerStateAndName 함수 추가)
    //작성일 : 2022.10.29
    IEnumerator CreatePlayer()
    {
        if (modeUI == true)
        {
            type = "Human";
            GameObject st = null;
            if (myNum >= 6)
            {
                st = PhotonNetwork.Instantiate("Student", HspawnPos[0].transform.position, Quaternion.identity, 0) as GameObject;
            }
            else
            {
                st = PhotonNetwork.Instantiate("Student", HspawnPos[myNum -1].transform.position, Quaternion.identity, 0) as GameObject;
            }
            pv.RPC("SetLeftHuman", PhotonTargets.MasterClient, false);
            SetPlayerStateAndName(false, st.GetComponent<StudentCtrl>().pv.owner.NickName, st.GetComponent<StudentCtrl>().pv.viewID);
        }
        else
        {
            type = "Zombie";
            GameObject zo = PhotonNetwork.Instantiate("Zombie", ZspawnPos.transform.position, Quaternion.identity, 0);
            ShowTransitionParticle(ZspawnPos.transform.position);
            SetPlayerStateAndName(true, zo.GetComponent<ZombieCtrl>().pv.owner.NickName, zo.GetComponent<ZombieCtrl>().pv.viewID);
        }
        yield return null;
    }

    // 작성자 : 이상준 
    //내용 : 인간에서 좀비로 변하는 처리 함수
    //작성일 : 2022.11.22
    public void Transition(Transform characterBody, int studentViewID)
    {
        double _startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString()); //CustomProperties에서 시작 시간 가져옴
        double incTimer = 0;

        incTimer = PhotonNetwork.time - _startTime;   //현재 시간 - 시작시간 = 살아남은 시간
        time = (int)incTimer;                         // 살아남은 시간 time 변수에 대입(후에 점수 계산).

        StartChasingBgm(false); 
        victory = false;
        pv.RPC("SetLeftHuman", PhotonTargets.MasterClient, true); //마스터 클라이언트에게 남은 인간 수 -1 처리하게 함
        ChangeStageToZombie(studentViewID);           //다른 플레이어의 화면에 자신의 상태를 좀비로 바꾸는 처리
        StartCoroutine(this.Reborn(characterBody));   // 좀비 오브젝트 생성 처리
    }

    // 작성자 : 이상준 
    //내용 : 마스터클라이언트가 남은 인간의 수 기록, 처리 함수
    //작성일 : 2022.11.22
    [PunRPC]
    public void SetLeftHuman(bool isMinus)
    {// isMinus = true : 인간 수가 줄었는지 파라미터로 입력
        if (isMinus)
        {   // 인간 수가 줄었을 시
            leftHuman -= 1;
            Debug.Log("left human : " + leftHuman);
            if (leftHuman == 0)
            { // 인간 수가 없으면 좀비 승리 메서드 호출
                pv.RPC("ZombieWin", PhotonTargets.All);
            }
        }
        else
        { // 인간 수가 늘었다면 남은 인간 변수에 +1;
            leftHuman += 1;
        }
    }

    // 작성자 : 이상준 
    //내용 : 인간 오브젝트 삭제, 좀비 오브젝트 생성 처리 함수
    //작성일 : 2022.11.23
    IEnumerator Reborn(Transform characterBody)
    {
        yield return new WaitForSeconds(3.0f);
        GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().isHuman = false;
        modeUI = false;
        SetUI();
        characterBody.root.gameObject.SetActive(false);
        GameObject zombie = PhotonNetwork.Instantiate("Zombie", characterBody.position, Quaternion.identity, 0) as GameObject;
        pv.RPC("ShowTransitionParticle", PhotonTargets.All, characterBody.position);

        zombie.GetComponent<ZombieSoundCtrl>().PlaySound("Transition");
        PhotonNetwork.Destroy(characterBody.root.gameObject);
    }
    [PunRPC]
    void ShowTransitionParticle(Vector3 pos)
    { //파티클 생성 함수
        GameObject particle = Instantiate(transitionParticle, pos, Quaternion.Euler(-90,0,0)) as GameObject;

        Destroy(particle, 2.0f);// 2초 후 삭제
    }

    // 작성자 : 이상준 
    //내용 : 자신의 상태를 다른 네트워크 플레이어 상태창(UI)에 입력하는 메서드(아래가 RPC 메서드)
    //작성일 : 2022.11.23
    public void SetPlayerStateAndName(bool isZombie, string name, int viewID)
    {
        pv.RPC("SetStateName", PhotonTargets.Others, isZombie, name, viewID);
    }
    [PunRPC]
    void SetStateName(bool isZombie, string name, int viewID)
    {
        uiManager.SetOhersStateName(isZombie, name, viewID);
    }

    // 작성자 : 이상준 
    //내용 : 다른 플레이어 상태창에 자신의 상태를 변경하는 메서드(아래가 RPC 메서드)
    //작성일 : 2022.11.23
    public void ChangeStageToZombie(int viewID)
    {
        pv.RPC("RPCChangeStageToZombie", PhotonTargets.Others, viewID);
    }
    [PunRPC]
    void RPCChangeStageToZombie(int viewID)
    {
        uiManager.ChangeOthersState(viewID);
    }

    // 작성자 : 이상준 
    //내용 : 감염 시 다른 플레이어 상태창에  남은 감염시간을 이미지로 보여줌. (아래가 RPC 함수)
    //작성일 : 2022.11.23
    public void SetTransition(int viewID, bool isActive)
    {
        pv.RPC("RPCStartTransition", PhotonTargets.Others, viewID, isActive);
    }
    [PunRPC]
    void RPCStartTransition(int viewID, bool isActive)
    {
        uiManager.StartTransitionImg(viewID, isActive);
    }

    // 작성자 : 이상준 
    //내용 : 인간 , 좀비 오브젝트에 따른 UI 관리
    //작성일 : 2022.10.30
    [PunRPC]
    void SetUI()
    {
        if (modeUI)
        {
            humanUI.SetActive(true);
            zombieUI.SetActive(false);
            uiManager.SetMainUi(humanUI);
        }
        else
        {
            zombieUI.SetActive(true);
            humanUI.SetActive(false);
            uiManager.SetMainUi(zombieUI);
        }
    }

    void Update()
    {
        //Tab 키로 큰 미니맵 볼 수 있음
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiManager.ShowBigMap(true, modeUI);
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            uiManager.ShowBigMap(false, modeUI);
        }
        // I 키로 인벤토리 활성화 가능.(인간만 가능)
        if (Input.GetKeyDown(KeyCode.I))
        {
            uiManager.ShowInventory();
        }
    }


    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
    //로비로 돌아가는 메서드
    public void BackLooby()
    {
        roomPhoton.SendMessage("BackLobby", SendMessageOptions.DontRequireReceiver);
    }
    //방으로 돌아가는 메서드
    public void BackRoom()
    {
        roomPhoton.SendMessage("Reactive", SendMessageOptions.DontRequireReceiver);
        SceneManager.UnloadSceneAsync("scNetPlay");
        soundManager.PlayBgm(1);
    }

    // 작성자 : 최원빈
    //내용 : DB - 결과창 출력, 입력
    //작성일 : 2022.11.18
    IEnumerator GetResult(string _myName)
    {
        WWWForm form = new WWWForm();

        form.AddField("Name", _myName);

        Result();
        form.AddField("Result", score);
        form.AddField("Type", type);
        form.AddField("Victory", victory.ToString());

        WWW resultServer = new WWW(GetResultURL, form);

        yield return resultServer;

        Registors(resultServer);
    }
    void Registors(WWW _dataServer)
    {
        currentArray = System.Text.Encoding.UTF8.GetString(_dataServer.bytes).Split(";"[0]);

        for (int i = 0; i <= currentArray.Length - 3; i = i + 2)
        {
            ranking.Add(new Player(currentArray[i], currentArray[i + 1]));
        }
    }
    // 결과창 보여줄 때 사용되는 함수 (개발 시간 부족으로 주석처리)
    //void GameResult()
    //{
    //    for (int i = 0; i < ranking.Count; i++)
    //    {
    //        GameObject obj = Instantiate(resultItem);
    //        Player pl = ranking[i];

    //        if (modeUI.ToString() == "True")
    //        {
    //            obj.GetComponent<ResultData>().DisplayResultData(pl.myName, pl.myScore + "(+" + score + ")");
    //            obj.transform.SetParent(resultPanel.transform);
    //            obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    //        }
    //        else if (modeUI.ToString() == "False")
    //        {
    //            obj.GetComponent<ResultData>().DisplayResultData(pl.myName, pl.myScore + "(-" + score + ")");
    //            obj.transform.SetParent(resultPanel.transform);
    //            obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    //        }
    //    }
    //}

    // 작성자 : 이상준 
    //내용 : 인간 승리 처리 메서드
    //작성일 : 2022.11.23
    public void HumanWin()
    {
        if (type == "Human" && modeUI)
        {//끝까지 살아남은 인간 플레이어
            victory = true;
        }
        else
        { //처음부터 좀비인 플레이어, 중간에 좀비된 플레이어
            Debug.Log("'I Lose");
            victory = false;
        }
        RPCGameOver();
    }
    // 작성자 : 이상준 
    //내용 : 좀비 승리 처리 메서드
    //       마스터 클라이언트가 모든 플레이어에게 호출시키기 위해서 포톤 RPC 함수 사용.
    //작성일 : 2022.11.23
    [PunRPC]
    public void ZombieWin()
    {
        if (type == "Human")
        {//끝까지 살아남은 인간 플레이어
            victory = false;
            
            double _startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());

            double incTimer = 0; // 살아남은 시간 계산
            incTimer = PhotonNetwork.time - _startTime;
            time = incTimer;        }
        else
        { //처음부터 좀비인 플레이어, 중간에 좀비된 플레이어
            victory = true;
        }
        Invoke("RPCGameOver", 3.0f);
    }


    // 작성자 : 이상준 
    //내용 : 게임 종료 처리 함수
    //작성일 : 2022.11.23
    public void RPCGameOver()
    {
        pv.RPC("PlayerSoundOff", PhotonTargets.All);          // 플레이어 오브젝트 삭제
        StartCoroutine(GetResult(PhotonNetwork.playerName));  // DB에 결과 입력

        string resultText = (victory == true ? "Win" : "Lose");
        resultUI.transform.Find("Result").Find("WinTxt").GetComponent<Text>().text = resultText; // UI 에 표시할 텍스트 승리, 패배별로 표시
        resultUI.SetActive(true);
        soundManager.PlayChasingBgm(false);                  // BGM 끄기

        PhotonNetwork.room.IsOpen = true;
        PhotonNetwork.room.IsVisible = true;
    }

    //플레이어 오브젝트 삭제 메서드
    [PunRPC]
    void PlayerSoundOff()
    {
        uiManager.PlayerSoundOff();
    }

    // 작성자 : 이상준 
    //내용 : 쫓기는 음악 재생, 재생 중지 처리 함수
    //작성일 : 2022.11.23
    public void StartChasingBgm(bool isActive)
    {
        soundManager.PlayChasingBgm(isActive);
    }

    // 작성자 : 최원빈
    //내용 : 점수 계산 메서드
    //작성일 : 2022.11.18
    void Result()
    {
        if (type == "Human")
        {
            if (victory)
            {
                score = 10;
            }
            else
            {
                 score = (int)((300 - time) / 300 * 10);
            }
        }
        else if (type == "Zombie")
        {
            if (victory)
            {
                score = 10;
            }
            else
            {
                score = 10 - infection;
            }
        }
    }




}
