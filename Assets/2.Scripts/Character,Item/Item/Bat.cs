using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using Character;
using UnityEngine.UI;

// 야구 배트 무기 클래스
// 작성자 : 이상준
// 사용 함수 : 공격 기능 (휘두르기)
// 작성일 : 2022.11.05
public class Bat : MonoBehaviour, IItem
{ //enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public ItemType Type { get; set; }  // enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public int Durability { get; set; } // 내구도
    public float Cooltime { get; set; } // 쿨타임

    private Collider col;               // 무기 콜라이더

    private IEnumerator coolingtime;    // 쿨타임 코루틴
    private UIManager uIManager;        // UI매니저 클래스

    private void Awake()
    {
        Type = ItemType.Bat;           // 게임 시작시 야구 배트로 타입 설정.
    }

    private void Start()
    {
        uIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        Durability = 2;                // 내구도 2
        Cooltime = 5.0f;               // 쿨타임 5초.
        coolingtime = null;
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Zombie" && !collision.gameObject.GetComponent<ICharacter>().IsInvulerable)
        {   // 좀비가 공격에 맞을 시 데미지 입는 함수 호출.
            collision.gameObject.GetComponent<ICharacter>().TakeDamage(1,Vector3.zero);
        }
    }

    //사용 함수(공격 : 휘두르기)
    public void Use(GameObject target)
    {
        if (coolingtime != null || Durability <= 0) return;
        col.enabled = true;
        StudentCtrl st = target.GetComponent<StudentCtrl>();
        coolingtime = this.Cooling(st,4.8f);
        StartCoroutine(coolingtime);        
        st.StartAnim();
        Durability -= 1;
    }

    IEnumerator Cooling(StudentCtrl st, float cool)
    {
        yield return new WaitForSeconds(0.2f);
        
        col.enabled = false;
        float leftTime = 0;
        while (cool > leftTime)
        {
            leftTime += Time.deltaTime;
            uIManager.ShowCoolTime(leftTime, cool); // UI매니저 클래스를 통해서 쿨타임 이미지 표시
            yield return new WaitForFixedUpdate();
        }
        
        coolingtime = null;

        if (Durability <= 0)
        { // 쿨타임 0 이면 아이템 비활성화
            Durability = 2;
            st.pv.RPC("RPCSetActiveItem", PhotonTargets.Others, (int)ItemType.None);
            st.SetActiveItem(ItemType.None);
            col.enabled = false;
        };
    }

    private void OnDisable()
    {
        coolingtime = null;
    }
}
