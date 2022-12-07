using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using Character;

//학생(인간)케릭터 클래스 
// 작성자 : 정유진(초안 작성), 이상준
// 내용 : 이동, 아이템 상호작용, 데미지, 죽음 등 메서드 포함
// 마지막 수정일 : 2022.11.23
public class StudentCtrl : MonoBehaviour, ICharacter, IPunObservable
{   //이상준이 추가한 변수  ~ 2022-11-23
    public int life { get; set; }            // 남은 치료 가능 횟수
    public float RunSpd { get; set; }        // 달리기 속도 변환용 float형 변수
    private Vector3 dir;                     // Run,Walk 등에 쓰인 dir 변수
    private bool isRunning = false;          // 걷기 / 뛰기 변환시 사용되는 bool형 변수

    private bool canMove = true;             // 움직일 수 있는지 확인하기 위한 bool형 변수 
    public Transform cameraArm;              // CameraArm 오브젝트 Transform 변수
    public Transform characterBody;          // CharacterBody 오브젝트 Transform 변수
    public Transform minimap;                // Minimap 오브젝트 Transform 변수

    public LayerMask obstacleMask;           // 장애물 오브젝트 필터용 레이어 변수
    public PhotonView pv;                    // 포톤뷰 변수
    private Vector3 net_currPos;             // 현재 위치 저장용 변수
    private Quaternion net_currRot;          // 현재 Quaternion값 저장용 변수
    private int net_anim;                    // 애니메이션 동기화용 int형 변수

    public Transform[] items;                // 케릭터 손에 있는 아이템 오브젝트 배열(기본 비활성화 상태)
    private GameObject interactObj;          // 상호작용 오브젝트 변수
    private ItemStorage itemStorage;         // 아이템 스토리지 클래스 변수
    private float interactDelay;             // 상호작용 딜레이용 float 변수
    private IItem currentItem;               // 현재 케릭터 손에 활성화된 오브젝트의 IItem 클래스 변수
    private IEnumerator interaction;         // 상호작용 코루틴 변수
    private IEnumerator transition;          // 감염상태 코루틴 변수
    private IEnumerator lookAround;          // 두리번 거리는 애니메이션 호출용 코루틴 변수
    public bool IsInvulerable { get; set; }  // 무적시간 제어용 bool형 변수

    public GameObject bitePos;               // 좀비에게 물렸을 때 좀비가 고정되는 위치
    public GameObject bloodProjecter;        // 이 오브젝트의 MeshRenderer에 피가 묻는 효과를 주는 프로젝터 오브젝트
    public ParticleSystem bloodParticle;     // 피가 튀기는 효과용 파티클 시스템
    public ParticleSystem healParticle;      // 백신을 사용시 쓰이는 파티클 시스템
    public GameObject plagueParticle;        // 감염상태에서 쓰이는 파티클 시스템

    private Animator anim;
    private Rigidbody rigid;
    private UIManager uiManager;             // UIManager 변수
    private ItemManager itemManager;         // ItemManager 변수
    public StudentSoundCtrl audio;           // 효과음 컨트롤용 StudentSoundCtrl 변수
    private string audioName;                // 효과음 컨트롤용 메서드 파라미터용 String 형 변수

    // 정유진 추가 변수
    private float infectTime = 10.0f;        // ~ 2022-10-28 감염시간 13초(+ 기본 10초 + 코루틴에서 추가 3초)
    public bool isInjured = false;           // 부상 상태인지 확인하기 위한 bool형 변수 
    public bool isHealing = false;           // 물약을 마시고 있는 상태인지 확인하기 위한 bool형 변수 

                                             // 계단 체크용 변수
    private Ray ray;                         // Ray 변수 
    private RaycastHit hitInfo;              // 레이저에 맞은 물체의 정보 받아오기용 RaycastHit형 변수
    private float maxSlopeAngle = 45.0f;     // 캐릭터가 올라갈 수 있는 최대 경사각 float형 변수


    //작성자 : 이상준
    //내용 : 상호작용 오브젝트의 트리거에 반응 메서드
    //작성일 : 2022.10.28
    private void OnTriggerEnter(Collider other)
    {
        if (pv.isMine)
        {
            if (other.gameObject.CompareTag("ItemStorage") || other.gameObject.CompareTag("ImdUseItem"))
            {   // 상호작용 가능한 오브젝트의 트리거에 들어왔을 경우 호출
                Vector3 dirToTarget = (other.gameObject.transform.position - characterBody.transform.position).normalized;
                float dstToTarget = Vector3.Distance(characterBody.transform.position, other.gameObject.transform.position);
                //상호작용 오브젝트와 해당 오브젝트 사이에 벽이 없는지 Raycast를 통해 확인
                if (!Physics.Raycast(characterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget, obstacleMask))
                {                                   //벽이 없을 경우 
                    interactObj = other.gameObject; //현재 상호작용 오브젝트를 해당 트리거를 가지고 있는 오브젝트로 설정.
                    IsInteractable(true);
                }
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (pv.isMine)
        {
            if (other.gameObject.CompareTag("ItemStorage") || other.gameObject.CompareTag("ImdUseItem"))
            {//현재 상호작용 오브젝트 null로 설정.
                IsInteractable(false);
                interactObj = null;
            }
        }
    }


    private void Awake()
    {
        bloodParticle.Stop();
        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;
        gameObject.name = pv.owner.NickName;
        bloodProjecter.SetActive(false);
        healParticle = transform.Find("HealParticle").GetComponent<ParticleSystem>();
        healParticle.Stop();
        plagueParticle = transform.Find("PlagueParticle").gameObject;
        plagueParticle.SetActive(false);

        isRunning = false;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        audio = GetComponent<StudentSoundCtrl>();
        audioName = "Walk";
        if (pv.isMine)
        {
            this.gameObject.layer = 0;
            uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
            uiManager.SetPlayer(this.gameObject);
            itemManager = GameObject.FindGameObjectWithTag("ItemManager").GetComponent<ItemManager>();

        }
        if (!pv.isMine)
        {
            cameraArm.gameObject.SetActive(false);
            minimap.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        interactObj = null;
        for (int i = 0; i < items.Length; i++)
        {                                         
            items[i].gameObject.SetActive(false); //손에 있는 아이템 오브젝트 비활성화.
        }

        life = 3;                    //남은 치료 가능 횟수 3설정 감염시 -1
        RunSpd = 3.0f;              //임시 달리기 속도
        interactDelay = 0f;

        currentItem = null;

        net_anim = 0;
        if (!pv.isMine)
        {
            StartCoroutine(this.NetAnimSet()); //클론 오브젝트는 애니 동기화용 코루틴 계속 실행.
        }
    }

    private void FixedUpdate()
    {    
        Run();
    }

    public void Update()
    {
        if (pv.isMine)
        {
            //Left Shift 버튼: Run / Walk 전환  
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isRunning = !isRunning;
                //걷기, 뛰기 애니메이션 전환
                if (isRunning)
                {
                    RunSpd = 7.0f;
                    anim.SetBool("Run", true);
                    audioName = "Run";
                }
                else
                {
                    RunSpd = 3.0f;
                    anim.SetBool("Run", false);
                    audioName = "Walk";
                }
            }

            //AnyState 애니메이션 실행 시 이동메서드를 타지 않게 처리.(bool 조건 변수 false로 함)
            if (anim.GetCurrentAnimatorStateInfo(1).IsName("Drinking") ||
                anim.GetCurrentAnimatorStateInfo(0).IsName("PickingUp") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("PickingUp") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Bit") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Scratch") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Die") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Bash") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Throw"))
            {
                rigid.velocity = (dir * 0);
                canMove = false;
            }
            else
            {
                canMove = true;
            }

            //1 버튼 : 아이템 사용
            if (Input.GetKeyDown(KeyCode.Alpha1) && currentItem != null && canMove)
            {
                canMove = false;
                currentItem.Use(this.gameObject); // 아이템 인터페이스 메서드 사용
            }
        }
    }

  

    //최조 작성자 : 이상준
    // 수정자 : 정유진 (추가 사항에 주석 추가)
    //내용 : 이동함수. 카메라 앞 방향과 케릭터 몸 앞 방향을 일치시킴.
    //작성일 : 2022.10.29
    public void Run()
    {
        if (pv.isMine)
        {
            if (canMove)
            {
                dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                anim.SetFloat("Speed", dir != Vector3.zero ? RunSpd : 0);
                if (anim.GetFloat("Speed") != 0)
                {
                    Vector3 camForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
                    Vector3 camRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
                    Vector3 moveDir = camForward * dir.z + camRight * dir.x;

                    //2022-11-15 유진 추가
                    //LookRotation 안에 넣어줄 Vector3 변수
                    Vector3 rotateDir = camForward * dir.z + camRight * dir.x;

                    //내리막길을 자연스럽게 걷도록 함
                    Vector3 gravity = Vector3.down * Mathf.Abs(rigid.velocity.y);

                    //경사면에 있다면
                    if (IsOnSlope())
                    {
                        //계단 위에 있으면 각도를 수정해주고, 아니면 원래 설정한 각도로 감
                        moveDir = SetDirection(moveDir);
                        gravity = Vector3.zero;
                    }

                    rigid.velocity = (moveDir * RunSpd) + gravity;

                    //카메라 앞 방향과 케릭터 몸 앞 방향을 일치시킴
                    characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 5.0f);
                    minimap.localRotation = Quaternion.Slerp(minimap.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 2.5f);
                }
                else
                {
                    rigid.velocity = Vector3.zero;
                }
                net_anim = anim.GetFloat("Speed") != 0 ? (RunSpd < 4.0f ? 1 : 2) : 0;
                string _audioName =  anim.GetFloat("Speed") == 0 ? "None" : audioName;
                audio.PlaySound(_audioName);

                if (net_anim == 0)
                { // Idle 애니메이션 상태일 시 애니메이션 변환 함수 호출
                    if (lookAround == null && !anim.GetCurrentAnimatorStateInfo(0).IsName("IdleLookAround"))
                    {
                        lookAround = this.LookAround();
                        StartCoroutine(lookAround);
                    }
                }
                else
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("IdleLookAround"))
                    {
                        lookAround = null;
                        string name = "IsNotStill";
                        pv.RPC("SetPhotonAnim", PhotonTargets.All, name);
                    }
                }
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, net_currPos, Time.deltaTime * 8.0f);
            characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, net_currRot, Time.deltaTime * 5.0f);

        }
    }


    //작성자: 정유진
    //내용: 현재 캐릭터가 경사면에 있는지를 판별하는 bool형 함수 추가
    //작성일: 2022.11.15
    public bool IsOnSlope()
    {
        ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hitInfo, 150.0f))
        {
            //ray를 지면에 쐈을 때, 부딪힌 평면의 법선 벡터(normal)와 Vector3.up 사이의 각도로
            //캐릭터가 경사에 있는지를 확인할 수 있음 (0이면 평지, 아니면 경사)
            var angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            //각도가 0이 아니면서 maxSlopeAngle보다 작거나 같을 때,
            //즉 경사면 위에 있을 때 true 반환
            return angle != 0f && angle <= maxSlopeAngle;
        }

        return false;
    }

    //작성자: 정유진
    //내용: 캐릭터의 방향 설정
    //작성일: 2022.11.15
    public Vector3 SetDirection(Vector3 direction)
    {
        //현재 캐릭터가 서 있는 경사 지형 평면 벡터로 이동 방향 벡터 투영
        return Vector3.ProjectOnPlane(direction, hitInfo.normal).normalized;
    }
    //최초 작성자 : 정유진
    // 수정자 : 이상준
    //내용 : 3초 이상 가만히 있으면 두리번거리는 애니메이션으로 바뀌게 조정하는 함수 
    //       이상준이  코루틴 형식 메서드로 수정. 
    //작성일 : 2022.10.25
    IEnumerator LookAround()
    {
        yield return null;
        float leftTime = Time.time + 3.0f;
        while (true)
        {
            yield return null;
            if (Time.time > leftTime)
            {
                string name = "IsStill";
                pv.RPC("SetPhotonAnim", PhotonTargets.All, name);
                break;
            }
        }
        yield return null;
    }

    //작성자 : 이상준
    //내용 : 좀비에게 물렸을 때 호출되는 메서드
    //작성일 : 2022.10.25
    //데미지 처리 메서드. 
    public void TakeDamage(int _viewID , Vector3 vec)
    {
        Vector3 targetPos = Vector3.zero;
        if(_viewID > 999)
        {//공격한 상대의  ViewID가 999이상(실제 네트워크 상에 있는 플레이어)일 때
            //해당 플레이어의 오브젝트를 viewID를 통해 가져옴.
            GameObject zombie = PhotonView.Find(_viewID).gameObject.transform.Find("ZombieBody").Find("BitePos").gameObject;
            //해당 오브젝트의 위치 계산
            targetPos = new Vector3(zombie.transform.position.x, zombie.transform.parent.transform.position.y, zombie.transform.position.z);
            
        }
        //데미지 입는 포톤 RPC메서드 실행
        pv.RPC("Injured", PhotonTargets.All, _viewID, targetPos);
    }


    //작성자 : 이상준
    //내용 : 좀비에게 물렸을 때 호출되는 RPC메서드
    //작성일 : 2022.10.26
    [PunRPC]
    void Injured(int _viewID, Vector3 targetPos)
    {
        rigid.velocity = Vector3.zero;
        if (IsInvulerable) return;
        if (isHealing) isHealing = false;
        if (_viewID > 999)
        {//위치를 고정하는 코루틴 실행.
            StartCoroutine(fixPos(targetPos));
            bloodParticle.Play();
        }
        
        audio.PlayBodySound("Hit");
        anim.SetTrigger("Scratch");
        anim.SetBool("IsInjured", true);

        StartCoroutine(this.IsInvulerableTime());// 일정시간 무적시간을 부여하는 코루틴 실행
        Invoke("OnProjecter", 1.5f);

        if (pv.isMine)
        { // PhotonView 가 로컬 오브젝트일 때.
            life--; // 라이트 포인트 -1
            if (life <= 0)
            { //라이프 포인트가 <= 0 일 시 좀비로 바뀌는 메서드 실행.
                Invoke("Death", 2.5f);
                return;
            }

            if (!isInjured) //감염 상태도 아니고, 무적도 아닐 때
            {
                /*StageManager에 다른 네트워크 플레이어의 상태창에
                자신의 상태가 감염상태가 되도록 하는 메서드 실행.*/
                StageManager stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
                stageManager.SetTransition(pv.viewID, true);

                //일정시간 감염상태가 되는 코루틴 실행.
                transition = this.GetInjured();
                StartCoroutine(transition);
            }
        }
    }

    //작성자 : 이상준
    //내용 : 좀비에게 물렸을 때 좀비와 밀착되게 위치 고정하는 코루틴 메서드
    //작성일 : 2022.10.26
    IEnumerator fixPos(Vector3 targetPos, GameObject target = null)
    {
        yield return null;
        float currTime = Time.time;
        Vector3 vec = Vector3.zero;
       
        if (target != null)
        {// 저장소 상호작용
            transform.position = targetPos;
            Vector3 targetBodyPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            vec = targetBodyPos - transform.position;
            vec.Normalize();
        }
        else
        {//좀비에게 물렸을 때 콜라이더의 isKinematic = true;
            rigid.isKinematic = true;
        }
        while (true)
        {
           if(target == null)
            {//좀비에게 물렸을 때 좀비에 bitePos에 위치에 일정시간동안 고정.
                transform.position =targetPos;
            }
            yield return null;
            if (vec != Vector3.zero) characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(vec), Time.deltaTime * 5.0f);
            yield return null;
            if (Time.time > currTime + 1.5f)
            {
                bloodParticle.Stop();
                rigid.isKinematic = false;
                break;
            }
        }
    }


    //작성자 : 이상준
    //내용 : MeshRenderer에 피가 묻은 효과를 주는 Projecter 오브젝트 SetActive(true)하는 메서드
    //작성일 : 2022.10.26
    void OnProjecter()
    {
        if(bloodProjecter != null)
        {
            bloodProjecter.SetActive(true);
            plagueParticle.SetActive(true);
        }
    }

    //작성자 : 이상준
    //내용 : 일정시간동안 무적시간을 부여하는 메서드
    //작성일 : 2022.10.26
    IEnumerator IsInvulerableTime()
    {
        IsInvulerable = true; //이 bool형 변수를 true로 하면 공격 판정이 무효.
        yield return null;
        
        float invulerableTime = Time.time + 3.0f;
        while (true)
        {
            yield return null;
            if (Time.time > invulerableTime)
            {
                IsInvulerable = false; //3초 후 해제됨.
                break;
            }
        }
    }

    //최초 작성자 : 정유진
    // 수정자 : 이상준
    //내용 : 공격 입을 시 총 13초 카운트 후 좀비로 변하는 메서드 호출.
    //      (pv.isMine이 true인 로컬 오브젝트만 실행되는 함수.)
    //       이상준이  코루틴 형식 메서드로 수정. 
    //작성일 : 2022.10.25
    IEnumerator GetInjured()
    {
        isInjured = true;

        yield return new WaitForSeconds(3.0f);
        float lastBittenTime = Time.time;
       
        while (true)
        {
            yield return null;
            audio.PlayBodySound("Scary");
            Debug.Log("left Time :" + (infectTime - (Time.time - lastBittenTime)));
            if (Time.time > lastBittenTime + infectTime)
            {
                if (isInjured)
                {
                    Debug.Log("Dead");
                    Death();
                    break;
                }
            }
        }
        yield return null;
    }

    //최초 작성자 : 정유진
    // 수정자 : 이상준
    //내용 : 케릭터가 죽어서 좀비로 변하는 메서드
    //       StageManager 클래스에서 상태창 처리와 이 오브젝트 삭제, 좀비 오브젝트 생성 하게함.
    //작성일 : 2022.10.25
    void Death()
    {
        canMove = false;
        StopAllCoroutines();
        pv.RPC("SetPhotonAnim", PhotonTargets.Others, "Death");
        audio.PlayBodySound("Death");
        anim.SetTrigger("Die");
        SetActiveItem(ItemType.None);
        pv.RPC("RPCSetActiveItem", PhotonTargets.Others, (int)ItemType.None);
        GameObject.Find("StageManager").GetComponent<StageManager>().Transition(characterBody, pv.viewID);
    }

    //최초 작성자 : 정유진
    // 수정자 : 이상준
    //내용 : 감염 상태에서 치료제를 먹고 회복했을 때 메서드
    //      네트워크 플레이어 상태창에 이 플레이어의 상태를 감염에서 보통으로 되돌림(StageManager 클래스에서 처리) 
    //작성일 : 2022.10.25
    public void Heal()
    {
        if (transition != null)
        {
            audio.PlayBodySound("Health");
            StopCoroutine(transition);
            plagueParticle.SetActive(false);
            bloodProjecter.SetActive(false);
            transition = null;
            GameObject.Find("StageManager").GetComponent<StageManager>().SetTransition(pv.viewID, false);
            healParticle.Play();

            isInjured = false;
            anim.SetBool("IsInjured", false);
            net_anim = 12;
        }

    }

    //작성자 : 이상준
    //내용 : 아이템에 따른 로컬 오브젝트의 애니메이션 컨트롤 코루틴 실행 메서드
    //작성일 : 2022.10.25
    public void StartAnim()
    {
        StartCoroutine(AnimCtrl());
    }

    //애니메이션 컨트롤 코루틴 메서드
    IEnumerator AnimCtrl()
    {
        yield return new WaitForSeconds(0.1f);
        canMove = false;
        if (currentItem.Type == ItemType.Bat || currentItem.Type == ItemType.Mop) //방망이,대걸레일 때 휘두르는 애니 실행
        {
            anim.SetTrigger("Bash");
            string name = "Bash";
            pv.RPC("SetPhotonAnim", PhotonTargets.Others, name);

            if (currentItem.Type == ItemType.Bat) { name = "BatSwing"; } else { name = "MopSwing"; }

            itemManager.pv.RPC("SoundPlay", PhotonTargets.All, transform.position, name );
        }
        else if (currentItem.Type == ItemType.Baseball) //야구공일 때 던지는 애니 실행
        {
            anim.SetTrigger("Throw");
            yield return new WaitForSeconds(0.5f);
            itemManager.pv.RPC("SoundPlay", PhotonTargets.All, transform.position, "BallThrow");
        }
        else if (currentItem.Type == ItemType.Vaccine) //백신일 때 마시는 애니 실행
        {
            anim.SetTrigger("Drink");
            string name = "Drink";
            pv.RPC("SetPhotonAnim", PhotonTargets.Others, name);
            audio.PlayBodySound("Drink");
        }
    }

    //작성자 : 이상준
    //내용 : 아이템 상호작용 코루틴 실행, 중지 함수
    //       아이템 오브젝트의 태그에 따라 UiManager 클래스에서 글자 보이게 함.
    //작성일 : 2022.10.23
    public void IsInteractable(bool temp)
    {
        if (temp)
        {
            Debug.Log(" uiManager call");
            uiManager.ShowIntrText(true, interactObj.gameObject.tag);
            interaction = this.Interaction();
            StartCoroutine(interaction);
        }
        else
        {
            uiManager.ShowIntrText(false, "");
            StopCoroutine(interaction);
        }
    }

    //내용 : 주위에 상호작용 가능한 오브젝트가 있을시 발동되는 코루틴 메서드.
    IEnumerator Interaction()
    {
        yield return null;
        while (true)
        {
            yield return new WaitForSeconds( interactDelay);
            interactDelay = interactDelay > 0 ? 0 : interactDelay;
            if (interactObj != null  && interactObj.GetComponent<Collider>().enabled == false)
            {  // 상호작용 오브젝트가 null 이거나 콜라이더가 enabled = false라면 중지.
                IsInteractable(false);
            }
            if(interactObj != null )
            {
                float dis = Vector3.Distance(transform.position, interactObj.transform.position);
                if (Input.GetKeyUp(KeyCode.E) && dis <= 2.5f && canMove)
                {// 해당 ItemStorage와 2.5f 거리 이내이고 E키를 누를 시

                    switch (interactObj.gameObject.tag)
                    {
                        case "ItemStorage": // 태그가 아이템 저장소 일 때
                            itemStorage = interactObj.GetComponent<ItemStorage>();
                            
                            if (currentItem == null || itemStorage.SetItemActive(true, this.gameObject))
                            {//현재 손에 활성화된 아이템이 없을 경우 바로 아이템 습득
                                anim.SetTrigger("PickUp");
                                string name = "PickUp";
                                pv.RPC("SetPhotonAnim", PhotonTargets.All, name);
                                
                                GameObject fixObj = null;
                                if ((fixObj = interactObj.GetComponent<ItemStorage>().fixPos )!= null)
                                {
                                    Vector3 fixPos = new Vector3(fixObj.transform.position.x, transform.position.y, fixObj.transform.position.z);
                                    StartCoroutine(this.fixPos(fixPos, interactObj.transform.GetChild(5).gameObject));
                                }
                                SetActiveItem(itemStorage.Type);
                                IsInteractable(false);
                                break;
                            }
                            else
                            { //현재 활성화된 아이템이 있을 경우 아이템을 보여줌.
                                interactDelay = 2.0f;
                                anim.SetTrigger("PickUp");
                                string name = "PickUp";
                                pv.RPC("SetPhotonAnim", PhotonTargets.All, name);

                                GameObject fixObj = null;
                                if ((fixObj = interactObj.GetComponent<ItemStorage>().fixPos) != null)
                                {
                                    Vector3 fixPos = new Vector3(fixObj.transform.position.x, transform.position.y, fixObj.transform.position.z);
                                    StartCoroutine(this.fixPos(fixPos, interactObj));
                                }
                            }
                            break;

                        case "ImdUseItem":// 태그가 즉시 사용 아이템 일 때 즉시 그 아이템 사용.
                            Debug.Log("ImdUseItem");
                            interactObj.GetComponent<IItem>().Use(this.gameObject);
                            IsInteractable(false);
                            yield break;
                    }
                }
            }
        }
    }


    //작성자 : 이상준
    //내용 : 상호작용 오브젝트의 아이템 타입과 인벤토리 아이템의 아이템 비교하여
    //       떨어진 아이템 생성 혹은 인벤토리에 아이템 추가 메서드   
    //작성일 : 2022.10.23
    public void SetActiveItem(ItemType type)
    {
        if (itemStorage != null) { itemStorage.isTaken = true; itemStorage = null; }
        if (uiManager.CheckHavingSame(type)) { //아이템 인벤토리에 같은 아이템이 있을경우
            Debug.Log("Have Same Item");
            itemManager.pv.RPC("DropItemStorage", PhotonTargets.All, transform.position, (int)type, GetDurability(type)); //소지 아이템 타입, 내구도로 드랍 아이템 생성.
            int durability = interactObj.GetComponent<ItemStorage>().durability; // 생성된 아이템 내구도값을 새로 설정.
            if (durability != 0) { SetDurability(type, durability); }
            return;
        }
        
        if (interactObj != null) interactObj = null;

        //먼저 Ui 매니저로 인벤토리에 아이템을 추가하고 1번 아이템이 바뀔 경우 현재 스크립트내의 소지 아이템 타입과 비교하여 다를 경우 SetActive(true)
        ItemType firstSlotType = uiManager.SetItemSlot(type);
        if(currentItem == null || currentItem.Type != firstSlotType)
        {
            pv.RPC("RPCSetActiveItem", PhotonTargets.All, (int)firstSlotType);
        }
    }

    //소지 아이템의 내구도를 변경하는 메서드
    void SetDurability (ItemType type, int _durability)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetComponent<IItem>() != null && items[i].GetComponent<IItem>().Type == type)
            {
                items[i].GetComponent<IItem>().Durability =_durability;
                Debug.Log("Durability :" + items[i].GetComponent<IItem>().Durability);
            }
        }
    }
    //소지 아이템의 내구도 반환 메서드
    int GetDurability (ItemType type)
    {
        int durability = 1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetComponent<IItem>() != null && items[i].GetComponent<IItem>().Type == type)
            {
                durability = items[i].GetComponent<IItem>().Durability;
            }
        }
        return durability;
    }

    //케릭터 손(ItemPos)의 하위 오브젝트들을 SetActive(true / false)시키는 메서드.

   [PunRPC]
    void RPCSetActiveItem(int type)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetComponent<IItem>() != null && items[i].GetComponent<IItem>().Type == (ItemType)type)
            {
                if (pv.isMine)
                {
                    currentItem = items[i].GetComponent<IItem>();
                }
                items[i].gameObject.SetActive(true);
            }
            else
            {
                items[i].gameObject.SetActive(false);
            }
        }

        if((ItemType)type == ItemType.None && pv.isMine)
        {
            currentItem = null;
        }
    }

    //작성자 : 이상준
    //내용 : 야구공 오브젝트 사용시 공을 생성해서 던지는 RPC 메서드.
    //작성일 : 2022.10.23
    [PunRPC] //야구공 던지는 RPC 메서드
    public void UseBall()
    {
        StartCoroutine(this.ThrowBall());
    }

    IEnumerator ThrowBall()
    {
        anim.SetTrigger("Throw");
        GameObject ballObj = items[3].gameObject;
        ballObj.GetComponent<MeshRenderer>().enabled = false;
        Vector3 spawnPos = ballObj.transform.position;
        GameObject ball = Instantiate(items[3].gameObject, spawnPos, Quaternion.identity);
        ball.transform.parent = this.gameObject.transform;

        yield return new WaitForSeconds(0.8f);
        ball.transform.parent = null;
        ball.transform.forward = characterBody.forward;  //공의 로컬 방향 케릭터의 로컬 방향으로 설정
        ball.GetComponent<SphereCollider>().isTrigger = true;
        ball.GetComponent<SphereCollider>().radius = 4;
        Vector3 currentPos = ball.transform.position;
        while (true)
        {
            yield return null;
            ball.transform.Translate(Vector3.forward * 2.0f);
            float dis = Vector3.Distance(currentPos, ball.transform.position);
            if (dis >= 10)
            { //거리가 10 이상이면 해당 오브젝트 삭제
                Destroy(ball.gameObject);
                break;
            }
        }
        RPCSetActiveItem((int)ItemType.None);
    }

    //작성자 : 이상준
    //내용 : 백신 하위 오브젝트 활성화 오브젝트
    //작성일 : 2022.10.23
    [PunRPC] 
    public void PillsActive()
    {
        items[0].gameObject.GetComponent<Vaccine>().SetPillsActive();
    }

    //작성자 : 이상준
    //내용 : 포톤 네트워크용 메서드
    //      int형 net_Anim변수를 클론과 공유하여 클론의 애니메이션 실행. 
    //작성일 : 2022.10.23
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = pv.instantiationData;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(characterBody.localRotation);
            stream.SendNext(net_anim);
        }
        else { 
            net_currPos = (Vector3)stream.ReceiveNext();
            net_currRot = (Quaternion)stream.ReceiveNext();
            net_anim = (int)stream.ReceiveNext();
        }
    }

    //작성자 : 이상준
    //내용 :AniState 애니메이션 네트워크 실행용 RPC 함수
    //작성일 : 2022.10.23
    [PunRPC]
    void SetPhotonAnim(string aniName)
    {
        switch (aniName)
        {
            case "Drink":
                anim.SetTrigger("Drink");
                break;
            case "IsInjured":
                anim.SetBool("IsInjured", false);
                break;
            case "Death":
                anim.SetTrigger("Die");
                break;
            case "IsStill":
                anim.SetBool("IsStill", true);
                break;
            case "IsNotStill":
                anim.SetBool("IsStill", false);
                break;
            case "Bash":
                anim.SetTrigger("Bash");
                break;
            case "PickUp":
                anim.SetTrigger("PickUp");
                break;

        }
    }

    //작성자 : 이상준
    //내용 : 포톤 네트워크 클론 오브젝트가 애니메이션 동기화용 메서드.
    //작성일 : 2022.10.25
    IEnumerator NetAnimSet()
    {
        yield return null;

        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            switch (net_anim)
            {
                case 0:
                    anim.SetFloat("Speed", 0);
                    break;
                case 1:
                    anim.SetFloat("Speed", 3);
                    anim.SetBool("Run", false);
                    RunSpd = 3;
                    break;
                case 2:
                    anim.SetFloat("Speed", 11);
                    anim.SetBool("Run", true);
                    RunSpd = 7;
                    break;
                case 3:
                    anim.SetBool("IsStill", true);
                    break;
                case 4:
                    anim.SetTrigger("Bash");
                    break;
                case 5:
                    anim.SetBool("IsStill", false);
                    break;
                case 6:
                    anim.SetTrigger("PickUp");
                    break;
                case 7:
                    anim.SetTrigger("Throw");
                    break;
                case 8:
                    anim.SetTrigger("Drink");
                    break;
                case 9:
                    anim.SetTrigger("Scratch");
                    anim.SetBool("IsInjured", true);
                    break;
                case 10:
                    anim.SetTrigger("Bit");
                    break;
                case 12:
                    anim.SetBool("IsInjured", false);
                    healParticle.Play();
                    bloodProjecter.SetActive(false);
                    plagueParticle.SetActive(false);
                    break;
                case 13:
                    anim.SetTrigger("Die");
                    break;
            }
        }
    }
}
