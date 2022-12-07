using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class csRoomPhoton : MonoBehaviour
{
    public GameObject myPlayer;
    public GameObject RoomUI;
    public GameObject RoomCanvas;
    public Text start;
    public Text txtConnet;
    public Text[] readyTxt;
    public Text[] playerName;
    public Text txtChatField;
    public InputField chatFiled;
    PhotonView pv;
    bool ItemActive = false;
    public bool isReady = false;
    public int readyCount = 0;
    public bool isHuman = false;
    public int humanCount = 0;
    public int zombieCount = 0;

    //2022-11-10 유진 추가
    //좀비 선택 유저를 표시하기 위한 이미지 배열 추가
    public Image[] ZombieMarks;
    //방 번호를 표시하기 위한 텍스트 변수 추가
    public Text txtRoomName;

    //2022-11-20 유진 추가
    //게임 시작 시 배경으로 쓰인 이미지 비활성화
    public GameObject roomBg;

    //2022-11-21 유진 추가
    //자신이 선택한 캐릭터의 이미지를 표시하기 위한 배열 추가
    public Image[] Z_SelectedImgs;
    public Image[] H_SelectedImgs;

    //2022-11-22 추가 (Restart 시 카메라 다시 활성화)
    public GameObject roomCam;
    //내 캐릭터 선택 시 등장하는 rawImage
    public GameObject studentModel;
    public GameObject zombieModel;
    public GameObject rendering;
    public int myNum;

    private void Awake()
    {
        
        pv = GetComponent<PhotonView>();

        PhotonNetwork.isMessageQueueRunning = true;
        GetConnectPlayerCount();

        //2022-11-10 유진 추가 내가 참여한 방 이름 표시 
        MarkRoomName();

        //입장시 기본적으로 방장은 좀비, 다른 플레이어는 인간으로 설정함(디폴트)
        if (PhotonNetwork.isMasterClient)
        {
            start.text = "게임시작";
            pv.RPC("MasterSetting", PhotonTargets.AllBuffered, null);
            isHuman = false;
        }
        else
        {
            start.text = "준   비";
            pv.RPC("PlayerSetting", PhotonTargets.AllBuffered, null);
            isHuman = true;
        }

    }

    IEnumerator Start()
    {
        //채팅 창에 노란색으로 입장 문구 띄워줌
        string msg =
            "<color=#ffff00>" +
            "\n" + PhotonNetwork.player.NickName + "님이 방에 입장했습니다."
            +"</color>";
        myNum = PhotonNetwork.countOfPlayers;
        Debug.Log("나의 번호 : " + myNum);
        pv.RPC("LogMsg", PhotonTargets.All, msg);

        //내 칸에 나의 닉네임 표시 + 이름을 저장
        playerName[0].text = PhotonNetwork.player.NickName;
        //원격 상의 내 칸에도 나의 닉네임 표시
        pv.RPC("SetText", PhotonTargets.OthersBuffered, playerName[0].text);

        //내가 방장이라면
        if (PhotonNetwork.isMasterClient)
        {
            //내 칸에 방장이라고 표시, 원격에도 방장으로 표시
            readyTxt[0].text = "방   장";
            pv.RPC("MasterSet", PhotonTargets.OthersBuffered, playerName[0].text);
        }
        //2022-11-16 추가 / 준비여부 텍스트 색깔로 보여주게 할 것임...
        else
        {
            //방장이 아니면 준비라는 텍스트를 어두운 회색으로 표시, 원격에도 표시
            readyTxt[0].text = "<color=#585858>"+"준   비"+"</color>";
            pv.RPC("NotReadyTxtSet", PhotonTargets.OthersBuffered, playerName[0].text);
        }

        //내가 만약 좀비라면
        if (!isHuman)
        {
            //2022-11-22 추가
            //내 칸에 좀비 모델 활성화
            zombieModel.SetActive(true);
            //내 칸에 좀비 마크 표시
            ZombieMarks[0].gameObject.SetActive(true);

            //다른 플레이어 화면에서도 내 칸에 좀비마크를 표시하기 위해
            //네크워크로 나의 이름을 전송함
            pv.RPC("ZombieMark", PhotonTargets.OthersBuffered, playerName[0].text);
        }
        //아니면
        else
        {
            //2022-11-22 추가
            //내 칸에 인간 모델 활성화
            studentModel.SetActive(true);
            //내 칸에서 좀비 마크 삭제
            ZombieMarks[0].gameObject.SetActive(false);
            pv.RPC("DeleteZombieMark", PhotonTargets.OthersBuffered, playerName[0].text);
        }

        yield return null;
    }

    [PunRPC]
    void MasterSet(string _master)
    {

        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == _master)
            {
                readyTxt[i].text = "방   장";

                break;
            }
        }
    }

    void FixedUpdate()
    {
        
    }

    void Update()
    {
        Chat();
    }

    //2022-11-10 유진 추가
    //좀비 마크 표시
    [PunRPC]
    void ZombieMark(string _zombieName)
    {
        //for 문을 돌면서 내 이름(좀비의 이름)이 있다면
        //그 칸에 있는 좀비 마크 활성화~
        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == _zombieName)
            {
                ZombieMarks[i].gameObject.SetActive(true);
                //2022-11-21 좀비 이미지도 추가해줌
                Z_SelectedImgs[i - 1].gameObject.SetActive(true);
                H_SelectedImgs[i - 1].gameObject.SetActive(false);

                break;
            }
        }
    }

    //2022-11-10 유진 추가
    //좀비 마크 지우기 
    [PunRPC]
    void DeleteZombieMark(string _zombieName)
    {
        //for 문을 돌면서 내 이름(좀비의 이름)이 있다면
        //그 칸에 있는 좀비 마크 활성화 해제~
        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == _zombieName)
            {
                ZombieMarks[i].gameObject.SetActive(false);
                //2022-11-21 좀비 이미지 삭제 + 인간 이미지 활성화
                Z_SelectedImgs[i - 1].gameObject.SetActive(false);
                H_SelectedImgs[i - 1].gameObject.SetActive(true);

                break;
            }
        }
    }

    //최초로 방을 개설한 마스터 클라이언트에 한함
    //방장을 이어받은 경우에는 해당 X
    [PunRPC]
    void MasterSetting()
    {
        //2022-11-15 유진 수정 (추후 수정 필요)
        //방장 혼자 있을 때 게임 시작하면 안되므로 readycount 주석처리 필요함
        readyCount++;
        zombieCount++;
    }

    [PunRPC]
    void PlayerSetting()
    {
        humanCount++;
    }

    //2022-11-22 추가(방 꽉차면 닫는 메서드)
    public void MaxRoomClose()
    { 
            //방이 꽉 차면 방 닫음+안보이도록 처리
            if (PhotonNetwork.room.PlayerCount == PhotonNetwork.room.MaxPlayers)
            {
                //방 닫기
                PhotonNetwork.room.IsOpen = false;
                //방 안보이게 함
                PhotonNetwork.room.IsVisible = false;
            }
    }

    public void OnJoinedRoom()
    {
        MaxRoomClose();
    }

    public void GameStart()
    {
        string myName = PhotonNetwork.player.NickName;

        if (PhotonNetwork.isMasterClient)
        {
            //수정 필요 - 마스터 클라이언트일 경우 readycount++가 되도록 수정하고
            //추후~!! count 최소 두 명 이상일 때 ㄱ ㄱ 
            //수정 필요 - 방에 좀비, 플레이어가 최소 한 명씩은 있어야함
            //수정 필요 - 준비가 되지 않아도 게임 시작이 되는 오류 : 위에 거 때문에 생기는 오류같음
            //수정 필요 - 이미 게임을 플레이하고 있는 방인데 입장이 되는 오류

            //준비된 사람이 두 명 이상이고, 각각 좀비와 사람이 한 명씩 있을 때 시작 가능해야 함
            //테스트 완료 후 주석 해제!!!!(수정 필요)
            if (/*readyCount>=2 && zombieCount==1 &&*/ readyCount == PhotonNetwork.room.PlayerCount)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;

                //버튼 눌렀을 때 readycount +1 
                pv.RPC("AllReadyStart", PhotonTargets.AllBufferedViaServer, null);
            }
        }
        else
        {
            if (!isReady)
            {
                isReady = true;
                pv.RPC("Ready", PhotonTargets.MasterClient, null);
                readyTxt[0].text = "<color=#ff0000>" + "준   비" + "</color>";
                pv.RPC("ReadyTxtSet", PhotonTargets.OthersBuffered, myName);

            }
            else
            {
                isReady = false;
                pv.RPC("NotReady", PhotonTargets.MasterClient, null);
                readyTxt[0].text = "<color=#585858>" + "준   비" + "</color>";
                pv.RPC("NotReadyTxtSet", PhotonTargets.OthersBuffered, myName);
            }
        }
    }

    [PunRPC]
    void AllReadyStart()
    {
        rendering.SetActive(false);
        RoomCanvas.SetActive(false);
        roomBg.SetActive(false);
        SceneManager.LoadScene("scNetPlay", LoadSceneMode.Additive);
    }

    [PunRPC]
    void Ready()
    {
        readyCount++;
    }

    [PunRPC]
    void NotReady()
    {
        readyCount--;
    }

    [PunRPC]
    void ReadyTxtSet(string _myName)
    {
        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == _myName)
            {
                readyTxt[i].text = "<color=#ff0000>" + "준   비" + "</color>";

                break;
            }
        }
    }

    [PunRPC]
    void NotReadyTxtSet(string _myName)
    {
        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == _myName)
            {
                readyTxt[i].text = "<color=#585858>" + "준   비" + "</color>";

                break;
            }
        }
    }
    void GetConnectPlayerCount()
    {
        Room currRoom = PhotonNetwork.room;

        txtConnet.text = currRoom.PlayerCount.ToString() + " / " + currRoom.MaxPlayers.ToString();

    }

    //2022-11-10 유진 추가
    void MarkRoomName()
    {
        txtRoomName.text = PhotonNetwork.room.Name;
    }

    //플레이어가 방에 들어왔을 때
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    { 

        GetConnectPlayerCount();
    }

    //플레이어가 방을 나갔을 때
    void OnPhotonPlayerDisconnected(PhotonPlayer outPlayer)
    {

        GetConnectPlayerCount();
    }

    [PunRPC]
    void SetText(string _playerName)
    {
        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == "")
            {
                playerName[i].text = _playerName;

                break;
            }
        }
    }

    //사람 유저가 방을 나갔을 때 humanCount 하나를 빼주고
    //이름, 준비 문구 초기화시킴
    [PunRPC]
    void HumanOut(string _myName)
    {
        readyCount--;
        humanCount--;

        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == _myName)
            {
                playerName[i].text = "";
                readyTxt[i].text = "";
                Z_SelectedImgs[i - 1].gameObject.SetActive(false);
                H_SelectedImgs[i - 1].gameObject.SetActive(false);

                break;
            }
        }
    }

    //좀비 유저가 방을 나갔을 때 zombieCount 하나를 빼주고
    //이름, 준비 문구,좀비 마크를 초기화시킴
    [PunRPC]
    void ZombieOut(string _myName)
    {
        readyCount--;
        zombieCount--; 

        for (int i = 1; i < 5; i++)
        {
            if (playerName[i].text == _myName)
            {
                playerName[i].text = "";
                readyTxt[i].text = "";
                ZombieMarks[i].gameObject.SetActive(false);
                Z_SelectedImgs[i - 1].gameObject.SetActive(false);
                H_SelectedImgs[i - 1].gameObject.SetActive(false);

                break;
            }
        }
    }

    //뒤로가기 버튼을 눌렀을 때
    public void BackLobby()
    {
        //노란색으로 퇴장 문구 표시
        string msg = 
            "<color=#ffff00>" +
            "\n" + PhotonNetwork.player.NickName + "님이 방을 나가셨습니다."
            +"</color>";

        pv.RPC("LogMsg", PhotonTargets.All, msg);

        //내 이름 저장
        string myName = PhotonNetwork.player.NickName;

        //원격에서 내 이름, readyTxt, 좀비마크 등 다 지우고 나갈 것임 
        if (isHuman) //인간일 때 
        {
            pv.RPC("HumanOut", PhotonTargets.AllBuffered, myName);
        }
        else //좀비일 때 
        {
            pv.RPC("ZombieOut", PhotonTargets.AllBuffered, myName);
        }

        //방을 나감
        PhotonNetwork.LeaveRoom();
    }

    //방을 나가면
    void OnLeftRoom()
    {
        //로비로 돌아옴
        SceneManager.LoadScene("scLobby");
    }

    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }

    //2022-11-10 유진 수정
    //사람 버튼을 눌렀을 때
    public void OnSelectHuman()
    {
        //내가 이미 사람이면
        if (isHuman)
        {
            //사람 선택 유지
            isHuman = true;
        }
        //내가 좀비이고, 사람 수가 4명 이하일 때 버튼을 누르면
        else if (!isHuman && humanCount <= 4)
        {
            //사람 선택(변경) 완료
            isHuman = true;

            //2022-11-22 추가
            zombieModel.SetActive(false);
            studentModel.SetActive(true);

            string myName = PhotonNetwork.player.NickName;
            pv.RPC("OnHuman", PhotonTargets.AllBuffered, null);

            //내 칸에서 좀비 마크 삭제
            ZombieMarks[0].gameObject.SetActive(false);
            //원격에서 내 칸의 좀비마크도 삭제해줌
            pv.RPC("DeleteZombieMark", PhotonTargets.OthersBuffered, myName);

        }

    }

    //2022-11-10 유진 수정
    //좀비 버튼을 눌렀을 때
    public void OnSelectZombie()
    {

        //내가 이미 좀비라면
        if (!isHuman)
        {
            //좀비 선택 유지
            isHuman = false;
        }
        //내가 사람이면서 좀비 수가 0일 때 
        else if (isHuman && zombieCount == 0)
        {
            isHuman = false;
            
            //2022-11-22 추가
            studentModel.SetActive(false);
            zombieModel.SetActive(true);
      
            string myName = PhotonNetwork.player.NickName;
            //좀비 선택(변경) 완료
            
            pv.RPC("OnZombie", PhotonTargets.AllBuffered, null);

            //내 칸에 좀비 마크 표시
            ZombieMarks[0].gameObject.SetActive(true);

            //다른 플레이어 화면에서도 내 칸에 좀비마크를 표시하기 위해
            //네크워크로 나의 이름을 전송함
            pv.RPC("ZombieMark", PhotonTargets.OthersBuffered, myName);

            //채팅창에 좀비 선택 메시지를 띄워줌 
            string msg = 
                "<color=#ff00ff>" 
                + "\n" + "[" + myName + "]" + "님이 좀비를 선택하셨습니다."
                + "</color>";

            pv.RPC("LogMsg", PhotonTargets.All, msg);

        }
        //내가 사람이면서 좀비 수가 1일 때
        else if (isHuman && zombieCount == 1)
        { 
            //이미 좀비가 존재하므로 선택 불가, 인간 선택 유지
            isHuman = true;
            //채팅 창에 빨간색으로 텍스트 띄워줌
            txtChatField.text +=
                "<color=#ff0000>" +
                "\n[선택 불가] 이미 다른 유저가 좀비를 선택했습니다."
                +"</color>";
        }
    }

    [PunRPC]
    void OnHuman()
    {
        humanCount++;
        zombieCount--;
        if (zombieCount <= 0)
        {
            zombieCount = 0;
        }

    }

    [PunRPC]
    void OnZombie()
    {
        if (zombieCount == 0)
        {
            zombieCount++;
            humanCount--;
        }
        if (humanCount <= 0)
        {
            humanCount = 0;
        }

    }

    [PunRPC]
    void LogMsg(string msg)
    {
        txtChatField.text = txtChatField.text + msg;
    }

    void Chat()
    {
        string msg = "\n" + PhotonNetwork.player.NickName + " : " + chatFiled.text;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            pv.RPC("LogMsg", PhotonTargets.All, msg);
            chatFiled.text = "";
            //2022-11-15 유진 추가 : 엔터 후에도 커서가 유지되도록 함
            chatFiled.ActivateInputField();
        }
    }

    //2022-11-15 유진 추가
    //방장이 나갔을 때 문제...
    //좀비를 선택해도 좀비 마크가 나타나지 않음 - 해결 완 
    //좀비가 나갔는데도 좀비를 선택할 수 없음... - 해결 완 
    void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {   
        
        //방장이 나가고 내가 방장이라면...

        if (PhotonNetwork.isMasterClient)
        {
            //내 화면 방장 화면으로 세팅해줌
            start.text = "게임시작";
            readyTxt[0].text = "방   장";

            //내 이름 string으로 저장하고 포톤으로 쏴줌
            string myName = PhotonNetwork.player.NickName;
            //다른 사람들 컴퓨터에서도 내가 방장으로 보이도록 함!
            pv.RPC("MasterSet", PhotonTargets.AllBuffered, myName);
            pv.RPC("Ready", PhotonTargets.MasterClient, null);
        }
        
    }

    public void OnDebug()
    {
        Debug.Log("사람:" + readyCount);
        //Debug.Log("좀비:" + zombieCount);
    }

    //2022-11-22 추가
    public void Reactive()
    {
        roomCam.SetActive(true);
        RoomCanvas.SetActive(true);
        roomBg.SetActive(true);
        rendering.SetActive(true);
    }
}
