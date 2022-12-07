using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

// 아이템 매너저 클래스
// 작성자 : 이상준
// 내용 : scNetPlay씬의 모든 아이템 저장소 관리 클래스
// 작성일 : 2022.11.05
public class ItemManager : Photon.MonoBehaviour
{
    public PhotonView pv;                   // 포톤뷰 변수

    public GameObject itemStorage;          // 아이템 저장소 프리펍
    public ParticleSystem shineParticle;    // 아이템 저장소용 파티클
    public ItemType[] itemSets;             // 아이템 타입 설정용 배열
    private Queue<ItemType> itemQueue;      // 아이템 타입 저장용 큐
    public GameObject[] rootContainer;      // 컨테이너 오브젝트가 생성될 루트 오브젝트
    public GameObject[] containers;         // 컨테이너 프리펍 배열
    public List<GameObject> itemStorages;   // 맵에 생성된 아이템 저장소 (관리용 배열)
    GameObject fixPos;                      // 아이템 저장소 상호작용시 위치 고정용 오브젝트

    public GameObject[] rootdoors;          // 문 오브젝트가 생성될 루트 오브젝트
    public GameObject FDoor;                // 앞 문 프리펍
    public GameObject BDoor;                // 뒷 문 프리펍
    public List<GameObject> OpenableDoors;  // 맵에 생성된 문 (관리용 배열)

    private GameObject itemSpawnPoints;     // 아이템 스폰 포인트 배열
    private GameObject dropItemPoints;      // 떨어진 아이템 생성 포인트 배열
    private csSoundManager soundManager;    // 사운드 매니저 클래스 배열
    int DropStorageNum = 999;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;
        itemSpawnPoints = GameObject.Find("ItemSpawnPoints");
        dropItemPoints = GameObject.Find("DropItemPoints");
        soundManager = GameObject.Find("SoundManager").GetComponent<csSoundManager>();
        fixPos = new GameObject("FixPos");
        OpenableDoors = new List<GameObject>();
        itemStorages = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {   // 게임 시작하면 아이템 저장소, 문을 맵에 생성
        SetItemTypeQueue();
        CreateItemStorage();
        CreateDoors();
    }

    //퍼블릭 설정한 List의 ItemType값 큐에 넣는 함수
    void SetItemTypeQueue()
    {
        itemQueue = new Queue<ItemType>();
        for(int i = 0; i < itemSets.Length; i++)
        {
            itemQueue.Enqueue(itemSets[i]);
        }
    }

    //맵에 문을 생성하는 함수
    void CreateDoors()
    {
        for (int i = 0; i < rootdoors.Length; i++)
        {
            GameObject door = rootdoors[i].gameObject.name == "F_Door" ? FDoor : BDoor;
            GameObject openableDoor = Instantiate(door, rootdoors[i].transform.position, rootdoors[i].transform.localRotation, rootdoors[i].transform.parent) as GameObject;
            Destroy(rootdoors[i]);

            if (openableDoor != null)
            {
                openableDoor.GetComponentInChildren<Door>().SetIndex(i); //만든 문에 개별적으로 index값 부여
                OpenableDoors.Add(openableDoor); // 만든 문을 배열에 추가.
            }
        }
    }
    //문을 사용하는 RPC 함수
    [PunRPC]
    public void UseDoor(int index)
    {
        OpenableDoors[index].GetComponentInChildren<IItem>().Use(this.gameObject);
    }
    //문에 데미지를 입힐 때 RPC 함수
    [PunRPC]
    public void TakeDmgDoor(int index, bool isInside)
    {
        OpenableDoors[index].GetComponentInChildren<Door>().TakeDamage(isInside);
    }

    //사운드 오브젝트 생성 RPC 함수
    [PunRPC]
    public void SoundPlay(Vector3 pos , string name)
    {
        soundManager.PlayEffect(pos, name);
    }


    // 맵에 아이템 저장소 생성 함수
    void CreateItemStorage()
    {
        //자연스럽게 컨테이너를 넣기 위해서 컨테이너 오브젝트와 같이 생성. 프리펍으로 넣은 맵 안의 컨테이너는 제거.
        for(int i = 0; i< rootContainer.Length; i++)
        {
            GameObject oneStorage = Instantiate(itemStorage, rootContainer[i].transform.position, rootContainer[i].transform.localRotation, itemSpawnPoints.transform);
            GameObject _fixPos = Instantiate(fixPos, rootContainer[i].transform.position + rootContainer[i].transform.forward, rootContainer[i].transform.localRotation, oneStorage.transform) as GameObject;
            oneStorage.GetComponent<ItemStorage>().SetFixPos(_fixPos);
            ItemType nextItem = ItemType.Vaccine;
            if ( itemQueue.Count != 0)
            {
                nextItem = itemQueue.Dequeue();
            }

            GameObject container = Instantiate(GetContainer(rootContainer[i].name), rootContainer[i].transform.position, rootContainer[i].transform.localRotation, oneStorage.transform);

            oneStorage.GetComponent<ItemStorage>().SetType(nextItem);
            oneStorage.GetComponent<ItemStorage>().SetContainer(container, i);
            itemStorages.Add(oneStorage);
            Destroy(rootContainer[i]);
        }


        //떨어진 아이템 저장소도 생성하여 트리거 비활성화함.
        for (int j = 0; j < 3; j++)
        {
            GameObject oneDropStorage = Instantiate(itemStorage, transform.position, Quaternion.identity, dropItemPoints.transform);

            Instantiate(shineParticle, transform.position, Quaternion.identity, oneDropStorage.transform);

            GameObject nullContainer = null;
            oneDropStorage.GetComponent<ItemStorage>().SetContainer(nullContainer, DropStorageNum--);
            oneDropStorage.GetComponent<ItemStorage>().SetType(ItemType.None);
            oneDropStorage.GetComponent<BoxCollider>().size = new Vector3(2, 1, 2);
            oneDropStorage.GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
            oneDropStorage.GetComponent<BoxCollider>().enabled = false;

        }
    }

    // 아이템 저장소의 아이템을 보여주는 함수
    [PunRPC]
    public void ShowItem(int index, bool isActive)
    {
        if(index < 900)
        {   
            itemStorages[index].GetComponent<ItemStorage>().SetItemActive(isActive, this.gameObject);
        }
        else
        {   // 아이템 저장소 종류가 떨어진 아이템 일 경우 
            for (int i = 0; i < dropItemPoints.transform.childCount; i++)
            {
                if (dropItemPoints.transform.GetChild(i).GetComponent<ItemStorage>().index == index)
                {
                    Transform dropItem = dropItemPoints.transform.GetChild(i);
                    dropItem.GetComponent<ItemStorage>().SetItemActive( false, this.gameObject);
                    break;
                }
            }
        }
    }

    // 아이템 저장소의 이름에 따라 알맞은 저장소 오브젝트 반환 함수
    GameObject GetContainer(string name)
    {
        GameObject container = null;
        for (int i = 0; i < containers.Length; i++)
        {
            if (containers[i].name == name)
            {
                container = containers[i].gameObject;
                break;
            }
        }
        return container;
    }

    //떨어진 아이템 오브젝트 생성 함수

    [PunRPC]
    public void DropItemStorage(Vector3 pos, int type, int durability)
    {
        Transform dropItem = null; 
        for (int i = 0; i < dropItemPoints.transform.childCount; i++)
        {
            if (dropItemPoints.transform.GetChild(i) != null)
            {
                dropItem = dropItemPoints.transform.GetChild(i);
                dropItem.position = pos;
                dropItem.GetComponent<ItemStorage>().SetType((ItemType)type, durability);
                dropItem.GetComponent<ItemStorage>().SetItemActive( true, this.gameObject);
                dropItem.GetComponent<ItemStorage>().shineParticle.Play();
                dropItem.GetComponent<ItemStorage>().isTaken = false;
                dropItem.GetComponent<BoxCollider>().enabled = true;
                break;
            }
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
