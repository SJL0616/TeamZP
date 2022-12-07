using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

// 문 클래스
// 작성자 : 이상준
// 사용 함수 : 열기/닫기 기능. 데미지 입는 로직
// 작성일 : 2022.11.05
public class Door : MonoBehaviour, IItem
{

    public ItemType Type { get; set; }  // enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public int Durability { get; set; } // 내구도
    public float Cooltime { get; set; } // 쿨타임

    private Animator anim;
    public bool isOpened;               // 문이 열린 상태인지 확인용 bool형 변수
    private ItemManager itemManager;    // 아이템 매니저 클래스 
    public int arrayIndex;              // 문 오브젝트 관리용 배열에서 이 오브젝트의 인덱스
    public bool isInside;               // 좀비가 문을 방 안에서 공격하는지 밖에서 공격하는지 판별용 bool 형 변수
    private IEnumerator openCo;         // 문 열기/닫기 코루틴 변수
    public IEnumerator breakCo;         // 문 부수기 코루틴 변수


    private void Awake()
    {
        openCo = null;
        Cooltime = 0;                   // 쿨타임 0
        anim = transform.parent.GetComponentInChildren<Animator>();
        Durability = 3;                 // 내구도 3(3번 공격 당하면 부서짐)
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
    }

    private void Start()
    {
        Type = ItemType.ImdUseItem;     // 아이템 타입은 즉시 사용 아이템
        isOpened = false;               // 열린 상태 = false로 초기화
    }

    //관리용 인덱스 부여 함수
    public void SetIndex(int num)
    {
        this.arrayIndex = num;
    }

    //사용 함수(문 열기/닫기 기능, 공격 받는 로직)
    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        switch (target.gameObject.tag)
        {
            case "Zombie": // 상호작용 대상이 좀비이면 
                Vector3 myPos = transform.TransformPoint(transform.position);
                Vector3 targetPos = transform.TransformPoint(target.transform.position);
                Vector3 dir = (targetPos - myPos).normalized;

                isInside = dir.z > 0 ? false : true;  // 대상이 문 오브젝트 기준 방 안에 있는지 밖에 있는지 판단
                break;
            default:
                break;
        }

        if (target.gameObject.name != "ItemManager" && target.gameObject.tag == "Student")
        {  // 상호작용 대상이 인간이면 아이템 매니저에게 문 사용 기능 호출
            if (openCo != null) return;
            if (openCo == null)
            {
                openCo = this.OpenCoroutine();
                StartCoroutine(openCo); //로컬 오브젝트는 코루틴을 통해서 한번에 여러번 사용을 하지 못하게 함.
                itemManager.pv.RPC("UseDoor", PhotonTargets.Others, this.arrayIndex);
            }
        }
        else if (target.gameObject.name != "ItemManager" && target.gameObject.tag == "Zombie")
        {  // 상호작용 대상이 좀비면 아이템 매니저에게 문 공격받는 로직 호출
            if (breakCo != null) return;
            if (breakCo == null)
            {
                breakCo = this.TakeDamageCo(isInside);
                StartCoroutine(breakCo); //로컬 오브젝트는 코루틴을 통해서 한번에 여러번 사용을 하지 못하게 함.
                itemManager.pv.RPC("TakeDmgDoor", PhotonTargets.Others, this.arrayIndex, isInside);
            }
        }
        else
        {
            OpenDoor();
        }
    }

    // 문 사용(열기/닫기) 함수
    void OpenDoor()
    {
        if (!isOpened)
        {
            itemManager.SoundPlay(transform.position, "DoorOpen");
            anim.SetTrigger("Open");
            isOpened = true;
        }
        else
        {
            itemManager.SoundPlay(transform.position, "DoorClose");
            anim.SetTrigger("Close");
            isOpened = false;
        }
    }

    //로컬 오브젝트가 호출하는 문 사용(열기/닫기) 코루틴 함수
    IEnumerator OpenCoroutine()
    {
        yield return null;
        GetComponent<Collider>().enabled = false;
        if (!isOpened)
        {
            itemManager.SoundPlay(transform.position, "DoorOpen");
            anim.SetTrigger("Open");
            isOpened = true;
        }
        else
        {
            itemManager.SoundPlay(transform.position, "DoorClose");
            anim.SetTrigger("Close");
            isOpened = false;
        }
        yield return new WaitForSeconds(2.0f);
        GetComponent<Collider>().enabled = true;
        openCo = null; // 모든 로직이 끝나면 코루틴 변수 null 처리(다시 사용할 수 있게 함)
    }

    //문이 데미지를 함수
    public void TakeDamage(bool isInside)
    {
        Durability -= 1;             // 내구도 하락
        if (Durability == 0)
        {                            // 내구도 0이되면 부서지는 애니메이션, 콜라이더 비활성화
            string name = isInside == false ? "Broken1" : "Broken2";
            anim.SetTrigger(name);   // 애니메이션 실행
            Cooltime = 0; GetComponent<Collider>().enabled = false; 
            itemManager.SoundPlay(transform.position, "DoorTakeDmg"); //효과음 플레이

            return;
        }
        anim.SetTrigger("TakeDmg");
        itemManager.SoundPlay(transform.position, "DoorTakeDmg");
    }

    //로컬 오브젝트가 호출하는 문이 데미지를 입는 코루틴 함수
    IEnumerator TakeDamageCo(bool isInside)
    {
        yield return null;
        Durability -= 1;
        if (Durability == 0)
        {
            string name = isInside == false ? "Broken1" : "Broken2";
            anim.SetTrigger(name);
            Cooltime = 0; GetComponent<Collider>().enabled = false;
            itemManager.SoundPlay(transform.position, "DoorTakeDmg");

            breakCo = null;
            yield break;
        }
        anim.SetTrigger("TakeDmg");
        itemManager.SoundPlay(transform.position, "DoorTakeDmg");
        yield return new WaitForSeconds(3.0f);
        breakCo = null; // 모든 로직이 끝나면 코루틴 변수 null 처리(다시 사용할 수 있게 함)
    }
}