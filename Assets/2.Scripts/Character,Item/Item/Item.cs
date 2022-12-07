using UnityEngine;
using UnityEngine.UI;

// 아이템 네임스페이스, 인터페이스
// 작성자 : 이상준
// 내용 : 게임의 모든 아이템, 아이템 저장소가 상속해서 재구현하는 인터페이스
// 마지막 수정일 : 2022.11.23
namespace Item
{
    public enum ItemType
    {
        None,Vaccine, Bat, Mop, Baseball, ImdUseItem// 물약(내구도 : 1), 야구 배트(2), 대걸레(1), 야구공(1), 즉시사용아이템(1)
    }
    
    // 아이템 인터페이스
    public interface IItem
    {// 모든 아이템이 상속해서 구현

        ItemType Type { get; set; }  // 아이템 타입 enum형

        int Durability { get; set; } // 내구도 변수 

        float Cooltime { get; set; } // 아이템 쿨타임

        void Use(GameObject target); // 사용 메서드
    }

    //아이템 저장소 인터페이스
    public interface IItemStorage
    {// 아이템 저장소가 구현
        ItemType Type { get; set; }

        void SetType(ItemType type, int  durability);
    }

}