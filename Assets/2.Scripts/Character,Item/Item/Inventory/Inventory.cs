using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

// 인벤토리 관리 클래스
// 작성자 : 이상준
// 내용 : 아이템 습득, 사용, 교체 등 ItemSlot에 관한 모든 관리를 하는 클래스
// 작성일 : 2022.11.05
public class Inventory : MonoBehaviour
{
    [SerializeField]
    public CurrentItem[] currentItems;   //하위 모든 아이템 슬롯.
    private ItemSlot itemSlot;           // 1번 슬롯

    private void Awake()
    {
        currentItems = transform.GetComponentsInChildren<CurrentItem>();
        itemSlot = transform.parent.GetComponentInChildren<ItemSlot>(); 
    }

    // Start is called before the first frame update
    void Start()
    {
        FreshSlot();
    }

    //인벤토리 초기화 메서드
    void FreshSlot()
    {
        for(int i = 0; i< currentItems.Length; i++)
        {
            currentItems[i].currType = ItemType.None;
        }
    }

    //인벤토리에 아이템을 추가하고 1번 슬롯 아이템 타입을 반환 메서드
    public ItemType AddItem(ItemType type)
    {
        Debug.Log("Add Inventroy");
        for(int i = currentItems.Length -1; i >-1; i--)
        {
            if(currentItems[i].currType == ItemType.None && type != ItemType.None)
            {
                currentItems[i].currType = type;
                break;
            }
        }
        return currentItems[3].currType;
    }

    //아이템 사용 메서드
    public ItemType UseItem()
    {
        currentItems[3].currType = ItemType.None;

        for(int i = currentItems.Length - 1; i > -1; i--)
        {
            if (i == 0) { currentItems[i].currType = ItemType.None;
                break;        // 아이템 갯수가 0일 경우 종료
            }
            ItemType temp = currentItems[i - 1].currType;
            currentItems[i].currType = temp;
            
        }
        return currentItems[3].currType;
    }

    //습득한 아이템과 같은 아이템 소지중인지 판별하여 bool형 변수 반환 메서드
    public bool isHavingSame(ItemType type)
    {
        bool isHaving = false;
        for (int i = currentItems.Length - 1; i >-1; i--)
        {
            Debug.Log(i);
            if (currentItems[i].currType != ItemType.None && currentItems[i].currType == type)
            {
                isHaving = true;
                break;
            }
        }
        return isHaving;
    }

}
