using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//스테이지 매니저클래스 
public class GameManager : MonoBehaviour
{
    public GameObject student;
    public GameObject zombie;
    public Vector3[] spawnPos;


    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        //CreatePlayers();
    }

    //게임 시작시 플레이어 생성 메서드
    void CreatePlayers()
    {
        if(student != null && zombie != null)
        {
            Instantiate(student, spawnPos[0], Quaternion.identity);
            Instantiate(zombie, spawnPos[1], Quaternion.identity);
        }
    }

    //좀비로 변할때 호출되는 메서드
    public void ZombieTransition(Transform target)
    {
        //1. 좀비 오브젝트 생성 / 2.점수 처리 추가 
        Destroy(target.root.gameObject);
        Instantiate(zombie, target.position, target.localRotation);
        
    }
}
