using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using UnityEngine.UI;

// 백신 아이템 클래스
// 작성자 : 이상준
// 사용 함수 : 치료 기능 (감염상태 회복)
// 작성일 : 2022.11.05
public class Vaccine : MonoBehaviour, IItem
{
    public ItemType Type { get; set; } // enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public int Durability { get; set; } // 내구도
    public float Cooltime { get; set; } // 쿨타임
    public GameObject pills;            // 약병 오브젝트
    public IEnumerator healing;         // 아이템 사용 코루틴 변수

    private void Awake()
    {
        Type = ItemType.Vaccine; // 게임 시작시 백신으로 타입 설정.
    }

    private void Start()
    {
        Durability = 1;          // 내구도 1
        Cooltime = 0;            // 쿨타임 없음.
    }

    //사용 함수(힐 : 감염 상태에서 회복)
    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        if (Durability <= 0) return;     // 내구도 0이면 중지

        StudentCtrl st = target.GetComponent<StudentCtrl>();
        st.isHealing = true;
        
        st.StartAnim();                 //애니메이션 실행
        SetPillsActive();               // 약병 오브젝트 활성화
        st.pv.RPC("PillsActive", PhotonTargets.Others);
   
        healing = this.GetHeal(st);
        StartCoroutine(healing);        // 치료 함수 시작
        
    }
    public void SetPillsActive()
    {
        pills.SetActive(true);
    }

    // 치료 코루틴
    IEnumerator GetHeal(StudentCtrl st)
    {
        yield return new WaitForSeconds(1.0f);
        Durability -= 1;
        float currentTime = Time.time;
        while (true)
        {  //3초 후에 치료 
            Debug.Log("Left Heal Time"+ (3.0f - (Time.time - currentTime)));
            yield return null;
            if (!st.isHealing)
            {
                break;
            }
            else if (Time.time > currentTime + 2.0f)
            {
                st.Heal();
                st.isHealing = false;
                break;
            }
        }
        st.pv.RPC("RPCSetActiveItem", PhotonTargets.Others, (int)ItemType.None);
        st.SetActiveItem(ItemType.None);

    }

    private void OnEnable()
    {
        pills.SetActive(false); 
        Durability = 1;
    }
     void OnDisable()
    {
        pills.SetActive(false);
    }

}
