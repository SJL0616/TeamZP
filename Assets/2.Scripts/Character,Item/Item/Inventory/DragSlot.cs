using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Item;

// 드래그 아이템 이미지 관리 클래스
// 작성자 : 이상준
// 내용 : 스태틱 오브젝트. 현재  드래그중인 슬롯의 아이템 타입에 따라서
//        타입에 맞는 이미지를 보여준다. 또 드랍을 통해 스왑이 가능하게 함.
// 작성일 : 2022.11.05
public class DragSlot : MonoBehaviour,IPointerClickHandler
{
    static public DragSlot instance; 
    public CurrentItem dragSlot; //클릭- 드래그한 슬롯 변수

    private Image[] icons;
    private ItemType _currType;
    public ItemType currType
    {
        get { return _currType; }
        set
        {
            _currType = value;
            foreach (Image _Icon in icons)
            {
                //Debug.Log(_Icon.name);
                if (_Icon.name == _currType.ToString())
                {
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
    }

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        currType = ItemType.None;
    }

    public void DragSetImage(ItemType type)
    {
        currType = type; //드래그한 슬롯의 이미지를 받아서 표시.
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("is Cilcked");
    }

   
}
