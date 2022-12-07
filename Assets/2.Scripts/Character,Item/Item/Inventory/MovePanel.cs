using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 아이템 인벤토리 UI 판넬 관리 클래스
// 작성자 : 이상준
// 내용 : 인간 플레이어가 I키로 이 스크립트 함수 호출, 인벤토리 슬롯 컨트롤(활성, 비활성)한다.
// 작성일 : 2022.11.05
public class MovePanel : MonoBehaviour
{
    private RectTransform showPos;       // 인벤토리 활성화 위치
    private Vector3 startPos;            // 인벤토리 시작 위치
    private bool isShowed;               // 인벤토리가 활성화인지 true / false 반환
    private IEnumerator moveCoroutine;   // 인벤토리  활성/비활성 코루틴 변수

    private void Awake()
    {
        showPos = transform.parent.Find("ShowPos").gameObject.GetComponent<RectTransform>();
        startPos = GetComponent<RectTransform>().anchoredPosition;
        isShowed = false;
        moveCoroutine = null;
    }
    // 인벤토리 활성/비활성 코루틴 호출 함수
    public void MoveToPos()
    {
        if(moveCoroutine == null)
        {
            moveCoroutine = this.Move();
            StartCoroutine(moveCoroutine);
        }
    }
    
    // 인벤토리 활성/비활성 코루틴
    IEnumerator Move()
    {
        yield return null;
        Vector3 targetPos = Vector3.zero;
        if (!isShowed)
        {
            targetPos = showPos.anchoredPosition;
            isShowed = true;
        }
        else
        {
            targetPos = startPos;
            isShowed = false;
        }

        float dis = 0;
        while (true)
        {
            yield return null;
            dis = Vector3.Distance(GetComponent<RectTransform>().anchoredPosition, targetPos);
            if (dis < 0.5)
            {
                moveCoroutine = null;
                break;
            }
            GetComponent<RectTransform>().anchoredPosition = Vector3.MoveTowards(GetComponent<RectTransform>().anchoredPosition, targetPos, 15.0f);
        }
    }



}
