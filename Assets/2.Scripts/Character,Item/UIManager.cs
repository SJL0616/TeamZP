using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Item;
using System;

// UI 관리 클래스
// 작성자 : 이상준
// 내용 : scNetPlay씬의 모든 UI를 관리 하는 클래스
// 작성일 : 2022.11.05
public class UIManager : MonoBehaviour
{
    public GameObject localPlayer;           // 로컬 플레이어 저장용 변수
    bool modeUI;                             // 인간, 좀비 UI 확인용 bool형 변수
    private Inventory inventory;             // 인벤토리 UI 변수
    private ItemSlot itemSolt;               // 인벤토리 슬롯 UI 변수
    private MovePanel inventoryPanel;        // 인벤토리 판넬 UI 변수
    public List<GameObject> bigMap;          // 큰 맵 UI 변수
    public GameObject mainUi;                // 메인 UI 변수
    public Text time;                        // 시간 표시용 UI Text 변수
    public List<GameObject> stateList;       // 상태창 UI 변수
    private Dictionary<int, IEnumerator> transitionMap;  // 상태창 코루틴 저장용 Dictionary형 변수
    private Image coolTimeImg;               // 이미지 쿨타임 표시용 Image형 변수

    private double startTime;                // 시작 시간 저장용 float형 변수
    private bool gameStarted;                // 게임 시작 확인용 bool형 변수
    private bool gameOver;                   // 게임 종료 확인용 bool형 변수

    private void Awake()
    {
        transitionMap = new Dictionary<int, IEnumerator>();
        bigMap = new List<GameObject>();
        modeUI = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().isHuman;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        if (modeUI)
        {   // 인간 플레이어일 시 인벤토리 관련 UI 활성화.
            inventory = transform.GetComponentInChildren<Inventory>();
            itemSolt = transform.GetComponentInChildren<ItemSlot>();
            inventoryPanel = transform.GetComponentInChildren<MovePanel>();
        }
        if (stateList.Count == 0)
        {  // 네트워크 플레이어의 상태 UI창 SetActive => false
            GameObject statePanel = transform.Find("Room").Find("StatePanel").gameObject;
            for (int i = 0; i < statePanel.transform.childCount; i++)
            {
                stateList.Add(statePanel.transform.GetChild(i).gameObject);
                statePanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        time = transform.Find("Room").transform.Find("Time").GetComponent<Text>();
        time.color = Color.white;
    }

    // 게임 시작시 플레이어 타입(인간, 좀비)에 따라 mainUi변수에 Ui오브젝트 대입.
    public void SetMainUi(GameObject ui)
    {
        mainUi = ui;
        mainUi.transform.Find("MiniMap").gameObject.SetActive(true);
        mainUi.transform.Find("BigMap").gameObject.SetActive(false);
    }

    // 게임 시작시 상태창 참여 인원수에 맞춰서 초기화.
    public void SetOhersStateName(bool isZombie, string name, int viewID)
    {
        foreach (GameObject stateObj in stateList)
        {
            if (!stateObj.gameObject.activeSelf)
            {//  비활성화된 상태창에 참여한 유저의 정보 입력
                stateObj.gameObject.SetActive(true);
                stateObj.gameObject.name = viewID.ToString();                      // 참여 유저의 ID 표시
                string OffState = isZombie != true ? "ZombieState" : "HumanState"; // 참여 유저의 케릭터 타입에 따라 사진 표시
                string OnState = isZombie != true ? "HumanState" : "ZombieState";
                stateObj.gameObject.transform.Find(OffState).gameObject.SetActive(false);
                stateObj.gameObject.transform.Find(OnState).gameObject.SetActive(true);
                stateObj.gameObject.GetComponentInChildren<Text>().text = name;
                return;
            }
        }
    }

    // 네트워크 플레이어의 케릭터 타입이 변할 때 호출 함수
    public void ChangeOthersState(int viewID)
    {
        if (transitionMap.ContainsKey(viewID)) 
        {
            IEnumerator transition = transitionMap[viewID];
            StopCoroutine(transition);
            transition = null;
            transitionMap.Remove(viewID);
            foreach (GameObject stateObj in stateList)
            {
                if (stateObj.gameObject.name == viewID.ToString())
                {
                    stateObj.transform.Find("TransitionImg").GetComponent<Image>().fillAmount = 0;
                }
            }
        }
        foreach (GameObject stateObj in stateList)
        {
            if (stateObj.gameObject.name == viewID.ToString())
            {
                string OffState = "HumanState";
                string OnState = "ZombieState";
                stateObj.gameObject.transform.Find(OffState).gameObject.SetActive(false);
                stateObj.gameObject.transform.Find(OnState).gameObject.SetActive(true);
                return;
            }
        }
    }

    // 네트워크 인간 플레이어가 물렸을 때 차오르는 색 이미지로 표시.
    public void StartTransitionImg(int viewID, bool isActive)
    {
        if (isActive)
        {   // 감염 되었을 때 코루틴 함수로 감염시간 이미지로 표시.
            if (transitionMap.ContainsKey(viewID)) { return; } //이미지 코루틴이 실행중(코루틴 중에 다시 물린 상황)이라면 반환
            foreach (GameObject stateObj in stateList)
            {
                if (stateObj.gameObject.name == viewID.ToString())
                {
                    Image transitionImg = stateObj.transform.Find("TransitionImg").GetComponent<Image>();
                    IEnumerator transition = this.TransitionImgFill(transitionImg, viewID);
                    StartCoroutine(transition);
                    transitionMap.Add(viewID, transition);  // Dictionary<int, IEnumerator> 에 코루틴 변수 추가
                }
            }
        }
        else
        {  // 감염상태가 중지되었을 때 이미지 변화 코루틴 중지 로직
            if (transitionMap.ContainsKey(viewID))
            {
                IEnumerator transition = transitionMap[viewID];
                StopCoroutine(transition);
                transition = null;
                transitionMap.Remove(viewID);
                foreach (GameObject stateObj in stateList)
                {
                    if (stateObj.gameObject.name == viewID.ToString())
                    {
                        stateObj.transform.Find("TransitionImg").GetComponent<Image>().fillAmount = 0;
                    }
                }
            }
        }
    }

    //감염시간 이미지로 표시하는 코루틴 함수
    IEnumerator TransitionImgFill(Image _image, int viewID)
    {
        float cool = 13.0f;
        float leftTime = 0;
        while (cool > leftTime)
        {
            leftTime += Time.deltaTime;
            _image.fillAmount = /*1.0f -*/ (leftTime / cool);
            yield return new WaitForFixedUpdate();
        }
        ChangeOthersState(viewID);
    }

    // 아이템 추가, 사용 함수
    public ItemType SetItemSlot(ItemType type)
    {
        ItemType fristSlotType = ItemType.None;
        if (type == ItemType.None)
        {
            fristSlotType = inventory.UseItem();
        }
        else
        {
            fristSlotType = inventory.AddItem(type);
        }

        return fristSlotType;
    }

    // 같은 아이템 있는지 검사 함수
    public bool CheckHavingSame(ItemType type)
    {
        return inventory.isHavingSame(type);

    }

    // 큰 미니맵 활성 / 비활성 함수
    public void ShowBigMap(bool isActive, bool modeUI)
    {
        mainUi.transform.Find("MiniMap").gameObject.SetActive(!isActive);
        mainUi.transform.Find("BigMap").gameObject.SetActive(isActive);
    }

    // 상호작용 오브젝트 근접시 Ui text 표시 함수
    public void ShowIntrText(bool isActive, string tag)
    {
        if (mainUi != null)
        {
            switch (tag)
            {
                case "ItemStorage":
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "조사 하기";
                    break;
                case "ImdUseItem":
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "사용 하기";
                    break;
                case "Zombie":
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "부수기";
                    break;
                default:
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "";
                    break;
            }

        }
    }

    // 아이템 쿨타임 이미지로 표시 함수
    public void ShowCoolTime(float amount, float cool)
    {
        if (coolTimeImg == null) coolTimeImg = itemSolt.transform.Find("CoolTimeImg").GetComponent<Image>();
        coolTimeImg.fillAmount = 1.0f - (amount / cool);
    }

    // 로컬 플레이어 초기화 함수
    public void SetPlayer(GameObject target)
    {
        this.localPlayer = target;
    }

    // 게임 종료시 플레이어  삭제 함수
    public void PlayerSoundOff()
    {
        PhotonNetwork.Destroy(localPlayer);
    }

    // 인벤토리 활성화, 비활성화 함수
    public void ShowInventory()
    {
        if (inventory)
            inventoryPanel.MoveToPos();
    }


    void Update()
    {   // 남은 시간 표시 함수 Update()에서 지속 실행
        UpdateTime();
    }

    // 시간 세팅 함수
    public void SetTime(double _startTime, bool _gameStarted)
    {
        startTime = _startTime;
        gameStarted = _gameStarted;
    }

    // 시간 업데이트 함수
    void UpdateTime()
    {   if (gameOver) return;
        if (!gameStarted)
        {   // 포톤 CustomProperties에서 시작 시간을 가져옴.
            startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());
            if (startTime != 0)
            {
                gameStarted = true;
            }
            else
            {
                return;
            }
        }
        double incTimer = 0;
        double decTimer = 0;
        incTimer = PhotonNetwork.time - startTime; // 시작시간부터 경과한 시간을 구한다.
        double roundTime = 300.0;                  // 게임 전체 시간 5분(300초)
        decTimer = roundTime - incTimer;           // 전체시간에서 경과 시간을 빼서 남은 시간을 구한다.

        TimeSpan timeSpan = TimeSpan.FromSeconds(decTimer); //TimeSpan형 변수에 남은 시간을 입력.
        time.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds); //화면에 표시하기 위해서 UI Text에 형변환 후 대입

        if (decTimer < 0 )
        {   // 남은 시간이 없을 경우 인간 승리 함수 호출
            gameOver = true;
            GameObject.Find("StageManager").GetComponent<StageManager>().HumanWin();
        }
    }
}
