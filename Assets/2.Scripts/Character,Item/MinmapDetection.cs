using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 미니맵 감지 클래스
// 작성자 : 이상준
// 내용 : 미니맵에 마커를 표시하는 스크립트
// 작성일 : 2022.11.10
public class MinmapDetection : MonoBehaviour
{
    public float viewRadius;            // 미니맵의 오브젝트 감지 범위
    [Range(0, 360)]
    public float viewAngle;             // 케릭터의 시야각
    private GameObject bigMap;          // 전체 미니맵
    public GameObject CharacterBody;    // 케릭터 몸 오브젝트
    private float currMapIndex;         // 케릭터의 층 이동을 확인하기 위한 높이값
    public Material[] mapMats;          // 전체 미니맵의 메터리얼
    private bool isZombie;              // 케릭터가 인간인지 좀비인지 판별용 bool형 변수

    private GameObject marker;          // 기본 마커. 플레이어 마커 표시
    private List<GameObject> markers;   // 자식오브젝트의 모든 마커가 담긴 배열
    private Color zombieColor;          // 좀비 오브젝트의 표시 색
    private Color studentColor;         // 인간 오브젝트의 표시 색

    private float bgmOnStack;           // 추격 bgm 재생 조건 stack 값
    private float bgmOffStack;          // 추격 bgm 재생 중지 조건 stack 값
    private float extraTime;            // 추격 bgm stack 계산에 들어가는 시간 계산

    private int targetStack;            // 적 오브젝트가 감지되었을 때 증가되는 int형 변수
    private bool isBgmPlaying;          // 추격 bgm 재생 중인지 판별용 bool형 변수

    public LayerMask targetMask, obstacleMask; // 타겟 레이어

    public List<Transform> visibleTargets = new List<Transform>(); // 탐지된 오브젝트를 담을 배열

    private void Awake()
    {
        bigMap = GameObject.Find("BigMapBoard");
        transform.position = bigMap.transform.position + new Vector3(0, -1, 0);
        currMapIndex = 0;
        bgmOnStack = 0;
        bgmOffStack = 0;
        targetStack = 0;
        isBgmPlaying = false;
        markers = new List<GameObject>();
        marker = transform.Find("Marker").gameObject;
        marker.transform.position = bigMap.transform.position + new Vector3(0, +1.5f, 0);

        //기본 플레이어 수 + 2로 마커 생성. 일단 6으로 상정
        for (int i = 0; i < 6; i++)
        {
            GameObject _marker = Instantiate(marker, bigMap.transform.position + new Vector3(0, +1, 0), Quaternion.identity) as GameObject;            
            _marker.transform.parent = this.gameObject.transform;
            _marker.transform.localScale = new Vector3(2, 0.1f, 2);
            markers.Add(_marker);
            markers[i].SetActive(false);
        }
        marker.SetActive(true); 
        marker.GetComponent<MeshRenderer>().material.color = Color.green;

        if(transform.root.tag == "Zombie")
        {  // 플레이어가 좀비일 경우 
            isZombie = true;
            zombieColor = Color.white; // 좀비는 흰색
            studentColor = Color.red;  // 인간은 빨간색
            extraTime = 0.4f;
        }
        else
        { // 플레이어가 인간인 경우
            isZombie = false;
            zombieColor = Color.red;    // 좀비는 빨간
            studentColor = Color.white; // 인간은 흰색
            extraTime = 0;
        }
    }
    // Start is called before the first frame update
    void Start()
    { // 미니맵 표시를 위해서 오브젝트 탐지 코루틴 실행.
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    //전방 시야각 내의 관측 가능 오브젝트의 콜라이더가 있을 시 Marker List에 추가하는 메서드
    void FindVisibleTargets()
    { // 타겟 콜라이더를 탐지하면 대상과 나 사이에 벽이 없을 경우 탐지 오브젝트 배열에 추가.
        visibleTargets.Clear();
        Collider[] tartsInViewRadius = Physics.OverlapSphere(CharacterBody.transform.position, viewRadius, targetMask);

        for (int i = 0; i < tartsInViewRadius.Length; i++)
        {
            Transform target = tartsInViewRadius[i].transform;

            Vector3 dirToTarget = (target.position - CharacterBody.transform.position).normalized;
            if (target.gameObject.tag == "ItemStorage" && isZombie)
            {
                continue;
            }
            if (!isZombie)
            { // 인간일 경우 
                if( Vector3.Angle(CharacterBody.transform.forward, dirToTarget) < 360)
                {
                    float dstToTarget = Vector3.Distance(CharacterBody.transform.position, target.transform.position);
                    if (!Physics.Raycast(CharacterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget, obstacleMask))
                    {
                        if(target.gameObject.tag == "Zombie")
                        {
                            targetStack++;
                        } 
                        if (Vector3.Angle(CharacterBody.transform.forward, dirToTarget) < viewAngle / 2)
                        {
                            visibleTargets.Add(target);
                        }
                    }
                }
            }
            else
            {// 좀비일 경우 
                if (Vector3.Angle(CharacterBody.transform.forward, dirToTarget) < viewAngle / 2)
                {
                    float dstToTarget = Vector3.Distance(CharacterBody.transform.position, target.transform.position);
                    Debug.DrawLine(CharacterBody.transform.position + new Vector3(0, 1.5f, 0), target.transform.position, Color.red, 5.0f);
                    if (!Physics.Raycast(CharacterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget, obstacleMask))
                    {
                        if (target.gameObject.tag == "Student")
                        {
                            targetStack++;
                        }
                        visibleTargets.Add(target);
                    }
                }
            }
        }
        SetMarkerActive(visibleTargets.Count);
        SetBgmStack();
    }

    //탐지된 오브젝트의 수에 따라 Marker 오브젝트를 활성화 시키는 메서드
    void SetMarkerActive(int targetNum)
    {
        for(int i =0; i< markers.Count; i++)
        {
            markers[i].SetActive( i < targetNum ? true :false);
            
        }
    }

    //범위내의 관측 가능 오브젝트의 tag에 따라 Marker의 색 변경, 오브젝트 위치 트래킹
    private void LateUpdate()
    {
        float height = CharacterBody.transform.position.y - transform.position.y;
        int index = height < 4 ? 0 : 1;
        if (index != currMapIndex) SetMapMr(index);

        transform.position = new Vector3(CharacterBody.transform.position.x, bigMap.transform.position.y + -1, CharacterBody.transform.position.z);
        if (visibleTargets.Count != 0)
        {
            for(int i = 0; i < visibleTargets.Count; i++)
            {
                if(visibleTargets[i].gameObject != null)
                {
                    switch (visibleTargets[i].gameObject.tag)
                    {
                        case "Zombie":
                            markers[i].GetComponent<MeshRenderer>().material.color = zombieColor;
                            break;
                        case "ItemStorage":
                            markers[i].GetComponent<MeshRenderer>().material.color = Color.blue;
                            break;

                        case "Student":
                            markers[i].GetComponent<MeshRenderer>().material.color = studentColor;
                            break;
                    }
                    Vector3 targetPos = new Vector3(visibleTargets[i].position.x, markers[i].transform.position.y, visibleTargets[i].position.z);
                    markers[i].transform.position = Vector3.Lerp(markers[i].transform.position, targetPos, 0.8f);
                }
            }  
        }
    }

    // 탐지된 오브젝트에 따라서 일정시간 후에 추격 BGM을 껐다가 키는 로직
    void SetBgmStack()
    {
        if (targetStack > 0 )
        { // 적이 탐지되었다면
            targetStack = 0;
            bgmOffStack = 0;
            if (isBgmPlaying) return;
            bgmOnStack+= Time.deltaTime;
            
            if (bgmOnStack >= 0.18f && !isBgmPlaying) // 일정 시간 후에 추격 BGM 재생
            { isBgmPlaying = true;
                bgmOffStack = 0;
                bgmOnStack = 0;
              GameObject.Find("StageManager").GetComponent<StageManager>().StartChasingBgm(true);
            }
        }
        else
        { // 적이 탐지되지 않았다면
            targetStack = 0;
            bgmOffStack += Time.deltaTime;
            
            if (bgmOffStack >= 0.2f + extraTime && isBgmPlaying) // 일정 시간 후에 추격 BGM 중지
            {isBgmPlaying = false;
                bgmOnStack = 0;
                bgmOffStack = 0;
                GameObject.Find("StageManager").GetComponent<StageManager>().StartChasingBgm(false);

            }
        }
        
    }

    //위치에 따른 맵 이미지 변경 메서드
    void SetMapMr(int index)
    {
        MeshRenderer bigMapMr = bigMap.GetComponent<MeshRenderer>();
        bigMapMr.material = mapMats[index];
        currMapIndex = index;
    }

}
