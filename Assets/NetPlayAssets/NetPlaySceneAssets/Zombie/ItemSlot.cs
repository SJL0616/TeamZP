using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

//아이템 슬롯의 부모 오브젝트
public class ItemSlot : MonoBehaviour
{
    private CurrentItem currentItem;
    // Start is called before the first frame update
    private void Awake()
    {
        currentItem = transform.GetComponentInChildren<CurrentItem>();
    }
    void Start()
    {
        FreshSlot();
    }

    //게임 시작시 슬롯을 비우는 메서드
    void FreshSlot() {
        currentItem.currType = ItemType.None;
    }

    //슬롯에 아이템 추가 메서드
    public void AddItem(ItemType type) 
    {
        currentItem.currType = type;
    }


}
