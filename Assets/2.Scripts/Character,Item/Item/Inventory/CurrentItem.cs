using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Item;
using UnityEngine.EventSystems;

// 아이템 이미지 관리 클래스
// 작성자 : 이상준
// 내용 : ItemSlot의 자식 오브젝트. 현재 ItemSlot의 아이템 타입에 따라서
//        타입에 맞는 이미지를 보여줌.
//        DrangHandler 상속, 구현하여 마우스로 클릭 - 드래그하여 스왑 가능하게 구현하였음.
// 작성일 : 2022.11.05
public class CurrentItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private Image[] icons;
    private ItemType _currType;

    private UIManager uIManager;
    public ItemType currType
    {
        get { return _currType; }
        set
        {
            _currType = value;
            foreach(Image _Icon in icons)
            {
                //Debug.Log(_Icon.name);
                if(_Icon.name == _currType.ToString())
                {   //부모 오브젝트가 1번슬롯(활성화되는 아이템 슬롯)일 때, 케릭터 손의 오브젝트 변환 메서드 호출
                    if(transform.parent.name == "ItemSlot") uIManager.localPlayer.GetComponent<StudentCtrl>().pv.RPC("RPCSetActiveItem", PhotonTargets.All, (int)_currType);
                    _Icon.color = new Color(1, 1, 1, 1);
                }
                else
                {
                    _Icon.color = new Color(1, 1, 1, 0);
                }
            }
        }
    }
    private void Awake()
    {
        icons = transform.GetComponentsInChildren<Image>();
        uIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }



    //마우스 클릭 될 때 발생하는 이벤트 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (_currType != ItemType.None)
            {
                Debug.Log("this item type :" + _currType);
            }
        }
    }

    //드래그가 시작될 때 발동 메서드
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currType != ItemType.None)
        {
            DragSlot.instance.dragSlot = this;   // 스태틱 드래그 슬롯 인스턴스의 이 객체 대입.
            DragSlot.instance.DragSetImage(_currType); 
            DragSlot.instance.transform.parent.position = eventData.position;
        }
    }
    //드래그가 중일 때 발동 메서드
    public void OnDrag(PointerEventData eventData)
    {
        if(_currType != ItemType.None)
        {
            DragSlot.instance.transform.parent.position = eventData.position; //현재 이벤트 포지션으로 계속 옮김.
        }
    }

    //드래그가 끝날 때 발동 메서드
    public void OnEndDrag(PointerEventData eventData)
    {
        DragSlot.instance.DragSetImage(ItemType.None); // 스태틱 드래그 인스턴스 초기화.
        DragSlot.instance.dragSlot = null;
    }
    //자신에게 스태틱 드래그가 드랍될 때 발동 메서드
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("DragEnd");
        if (DragSlot.instance.dragSlot != null && DragSlot.instance.dragSlot.currType != ItemType.None && this.currType != ItemType.None)
        { // 이미지, 아이템 타입 스왑.
            ItemType tempType = currType;
            currType = DragSlot.instance.dragSlot.currType;
            DragSlot.instance.dragSlot.currType = tempType;
        }
    }




}
