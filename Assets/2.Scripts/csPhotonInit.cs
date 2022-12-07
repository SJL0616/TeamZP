using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class csPhotonInit : MonoBehaviour
{
    public string version = "Ver 0.1.0";

    public PhotonLogLevel LogLevel = PhotonLogLevel.Full;

    public Text userId;
    public Text roomName;
    public GameObject roomscrollContents;
    public GameObject roomItem;

    //2022-11-18 원빈 추가(DB)
    public GameObject rankScrollContents;
    public GameObject rankItem;

    // DB변수
    string GetHumanURL = "http://teamzombie.dothome.co.kr/HumanRank.php";
    string GetZombieURL = "http://teamzombie.dothome.co.kr/ZombieRank.php";
    List<Player> ranking = new List<Player>();
    string[] currentArray = null;
    public bool isHumanRank = true;

    public Sprite[] rankImgs;

    private void Awake()
    {
        //이거 주석 처리해야되나?
        Screen.SetResolution(1280, 680, false);
        
        if(!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings(version);

            PhotonNetwork.logLevel = LogLevel;

            //2022-11-18 원빈 수정 (DB)
            //PhotonNetwork.playerName = "GUEST_" + Random.Range(1, 99999);
            PhotonNetwork.playerName = PlayerPrefs.GetString("Player");

            //2022-11-20 주석처리
            //roomName.text = "Room_" + Random.Range(0, 999).ToString("000");

            roomscrollContents.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f);
            
        }
 
    }

    void OnJoinedLobby()
    {
        userId.text = GetUserId();
        
        //2022-11-20 주석처리 
        //roomName.text = "Room_" + Random.Range(0, 999).ToString("000");

        //2022-11-18 원빈 추가
        StartCoroutine(GetName());
    }

    string GetUserId()
    {
        //2022-11-18 원빈 수정
        //string userId = PlayerPrefs.GetString("USER_ID");

        //if(string.IsNullOrEmpty(userId))
        //{
        //    userId = "GUEST_" + Random.Range(0, 99999).ToString("000");
        //}
        //return userId;

        string userId = PlayerPrefs.GetString("Player");

        return userId;
    }

    //2022-11-20 유진 추가
    //방 입장 실패 시 방 생성 - 문제 : 오류 뜸
    void OnPhotonJoinRoomFailed()
    {
        Debug.Log("입장 실패");
        string _roomName = "Room_" + Random.Range(0, 999).ToString("000");

        PhotonNetwork.player.NickName = userId.text;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }

    //방 랜덤 입장 실패 시 방 생성 - 얘는 됨 
    void OnPhotonRandomJoinFailed()
    {
        string _roomName = "Room_" + Random.Range(0, 999).ToString("000");
   
        PhotonNetwork.player.NickName = userId.text;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }

    void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {

    }

    void OnJoinedRoom()
    {
        StartCoroutine(this.LoadRoom());
    }

    IEnumerator LoadRoom()
    {
        PhotonNetwork.isMessageQueueRunning = false;
        AsyncOperation ao = SceneManager.LoadSceneAsync("scRoom");
        
        yield return ao;
    }

    //2022-11-14 유진 추가
    //방 랜덤 입장 메서드 추가
    public void OnClickJoinRandomRoom()
    {
        PhotonNetwork.player.NickName = userId.text;

        PhotonNetwork.JoinRandomRoom();
    }

    public void OnClickCreateRoom()
    {
        string _roomName = "Room_" + Random.Range(0, 999).ToString("000");
        
        PhotonNetwork.player.NickName = userId.text;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);   
    }

    void OnReceivedRoomListUpdate()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Room_Item"))
        {
            Destroy(obj);
        }

        //안써서 걍 주석처리 함~
        //int rowCount = 0;

        roomscrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        foreach(RoomInfo _room in PhotonNetwork.GetRoomList())
        {

            GameObject room = (GameObject)Instantiate(roomItem);
            room.transform.SetParent(roomscrollContents.transform, false);

            RoomData roomdata = room.GetComponent<RoomData>();
            roomdata.roomName = _room.Name;
            roomdata.connectPlayer = _room.PlayerCount;
            roomdata.maxPlayer = _room.MaxPlayers;

            roomdata.DisplayRoomData();

            roomdata.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { OnClickRoomItem(roomdata.roomName); } );

            ////2022-11-19 방 비활성화 메서드 
            ////방이 꽉 찼을 때
            ////이미 플레이 중일 때 입장 불가하게 하는건 아직 해결 못함
            ////아마 메서드 자체를 따로 빼서 roomphoton에서 호출하면 되지 않을까...?
            //if(_room.PlayerCount==_room.MaxPlayers)
            //{
            //    //방에 입장 불가
            //    roomOps.IsOpen = false;
            //    //방이 보이지 않도록 처리 
            //    roomOps.IsVisible = false;
            //}

            //방이 계속 추가되는 오류가 있어 주석 처리
            //scrollContents.GetComponent<GridLayoutGroup>().constraintCount = ++rowCount;
            roomscrollContents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 0);
        }
    }

    void OnClickRoomItem(string roomName)
    {
        PhotonNetwork.player.NickName = userId.text;
        
        PhotonNetwork.JoinRoom(roomName);
    }

    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }

    //원빈 추가
    //데이터 베이스 값 불러
    IEnumerator GetName()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Rank_Item"))
        {
            Destroy(obj);
            ranking.Clear();
        }

        if (isHumanRank)
        {

            WWW humanServer = new WWW(GetHumanURL);

            yield return humanServer;

            if (humanServer.error != null)
            {
                Debug.LogWarning("Error!!!!!!!!");
            }
            else
            {
                Registors(humanServer);
                PanelRegistors();
            }
        }
        else
        {

            WWW zombieServer = new WWW(GetZombieURL);

            yield return zombieServer;

            if (zombieServer.error != null)
            {
                Debug.LogWarning("Error!!!!!!!!");
            }
            else
            {
                Registors(zombieServer);
                PanelRegistors();
            }
        }
    }
    void Registors(WWW _dataServer)
    {
        currentArray = System.Text.Encoding.UTF8.GetString(_dataServer.bytes).Split(";"[0]);

        for (int i = 0; i <= currentArray.Length - 3; i = i + 2)
        {
            ranking.Add(new Player(currentArray[i], currentArray[i + 1]));
        }
    }
    void PanelRegistors()
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            GameObject obj = Instantiate(rankItem);
            Player pl = ranking[i];

            if (isHumanRank)
            {
                obj.GetComponent<RankData>().DisplayRankData((i+1).ToString(), rankImgs[0], pl.myName, pl.myScore);
                obj.transform.SetParent(rankScrollContents.transform);
                obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            }
            else
            {
                obj.GetComponent<RankData>().DisplayRankData((i+1).ToString(), rankImgs[1], pl.myName, pl.myScore);
                obj.transform.SetParent(rankScrollContents.transform);
                obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            }
        }
    }

    public void OnHumanRank()
    {
        if (!isHumanRank)
        {
            isHumanRank = true;
            StartCoroutine(GetName());
        }

    }
    public void OnZombieRank()
    {
        if (isHumanRank)
        {
            isHumanRank = false;
            StartCoroutine(GetName());
        }
    }
}
