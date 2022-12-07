using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

// 아이템 저장소 클래스
// 작성자 : 이상준
// 내용 : 아이템 상호작용(획득), 아이템 보여주기 기능
// 작성일 : 2022.11.05
public class ItemStorage : MonoBehaviour, IItemStorage
{
    [field: SerializeField]
    public ItemType Type { get; set; }         // 저장소가 가지고 있는아이템 타입
    public GameObject fixPos;                  // 상호작용시 사용자의 위치를 고정할 오브젝트
    public int durability;                     // 아이템의 내구도

    public Transform[] itemSamples;            // 아이템 보여줄 때 사용될 하위 아이템 오브젝트들
    public ParticleSystem shineParticle;       // 빛나는 파티클 
    public IItem container;                    // 컨테이너 스크립트
    public int index;                          // 아이템 매니저 클래스가 관리용으로 부여한 인덱스값
    private ItemManager itemManager;           // 아이템 매니저 클래스 스크립트 변수
    public bool isTaken;                       // 현재 아이템을 플레이어가 획득시 true 반환 변수(확인용)

    //트리거 범위 내에 들어온 인간 오브젝트가 있으면 파티클 활성화
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Student" && !isTaken && shineParticle.isStopped && !(transform.root.name == "DropItemPoints"))
        {
            shineParticle.Play(); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Student" && !isTaken && shineParticle.isPlaying && !(transform.root.name == "DropItemPoints"))
        {
            shineParticle.Stop();
        }
    }

    private void Awake()
    {
        container = null;
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        itemSamples = new Transform[transform.childCount];
        fixPos = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            itemSamples[i] = transform.GetChild(i);
        }
    }
    // Start is called before the first frame update
    void Start()
    { // 하위 아이템 오브젝트 초기화 (안 보이게 함)
        for (int i = 0; i < itemSamples.Length; i++) itemSamples[i].gameObject.SetActive(false);
    }

    // 아이템 타입 설정
    public void SetType(ItemType type, int _durability = 0)
    { // 먼저 게임 시작 시 ItemManager 에서 해당 메서드를 호출하여 아이템 타입 부여.
        this.Type = type;
        if (type == ItemType.None)
        { // 아이템 타입이 None일 경우 콜라이더 비활성화.
            GetComponent<Collider>().enabled = false;
            shineParticle.Stop();
            isTaken = false;
        }
        this.durability = _durability;
    }

    // 자신의 아이템 타입에 따라 아이템 보이기 함수.
    public bool SetItemActive(bool isActive, GameObject target)
    {

        if (target.gameObject.name != "ItemManager") itemManager.pv.RPC("ShowItem", PhotonTargets.Others, this.index, isActive);
        if (!isActive) SetType(ItemType.None);
        bool itemActivated = false; // 현재 스폰된 아이템의 SetActive확인용 변수

        for (int i = 0; i < itemSamples.Length; i++)
        {
            if (isActive && itemSamples[i].gameObject.name == this.Type.ToString())
            {
                itemActivated = itemSamples[i].gameObject.activeSelf;
                itemSamples[i].gameObject.SetActive(true);
            }
            else
            {
                itemSamples[i].gameObject.SetActive(false);
            }
        }
        if (container != null)
        {
            container.Use(this.gameObject); // 컨테이너 여는 애니메이션 실행.
        }
        return itemActivated;
    }

    // 아이템 저장소의 컨테이너 설정 함수
    public void SetContainer(GameObject _container, int _index)
    {
        this.index = _index;
        if (_container == null)
        { // 컨테이너가 없을 경우(떨어진 아이템) : 파티클 활성화
            shineParticle = transform.GetComponentInChildren<ParticleSystem>();
            shineParticle.Stop();
        }
        else
        { // 컨테이너가 있을 경우 연결
            container = _container.GetComponent<IItem>();
            shineParticle = _container.transform.Find("ShineParticle").GetComponent<ParticleSystem>();
            shineParticle.Stop();
        }
    }

    public void SetFixPos(GameObject obj)
    {
        fixPos = obj;
    }

    private void Update()
    {   // 플레이어가 아이템 획득시 아이템 비활성화.
        if (isTaken && this.Type != ItemType.None)
        {
            SetItemActive(false, this.gameObject);
        }
    }

    void UseContainer()
    {
        container.Use(this.gameObject);
    }

}
