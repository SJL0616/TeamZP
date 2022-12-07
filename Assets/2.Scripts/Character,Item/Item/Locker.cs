using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

// 라커(컨테이너의 한 종류) 클래스
// 작성자 : 이상준
// 사용 함수 : 열기/닫기 기능.
// 작성일 : 2022.11.05
public class Locker : MonoBehaviour, IItem
{

    public ItemType Type { get; set; }  // enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public int Durability { get; set; } // 내구도
    public float Cooltime { get; set; } // 쿨타임

    private Animator anim;
    private bool isOpen;                // 문이 열린 상태인지 확인용 bool형 변수
    private ItemManager itemManager;    // 아이템 매니저 클래스 

    private void Awake()
    {
        anim = GetComponent<Animator>();
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
    }

    private void Start()
    {
        Type = ItemType.ImdUseItem;     // 아이템 타입은 즉시 사용 아이템
        isOpen = false;                 // 열린 상태 = false로 초기화
    }

    //사용 함수(문 열기/닫기 기능)
    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        if (!isOpen)
        {
            itemManager.SoundPlay(transform.position, "LockerOpen"); //효과음 플레이
            anim.SetTrigger("Open");    // 문열리는 애니메이션
            isOpen = true;              // 문 열린 상태라고 bool형 변수에 저장.
        }
    }
}
