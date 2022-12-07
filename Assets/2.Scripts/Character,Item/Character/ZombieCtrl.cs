using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Item;

/* 좀비케릭터 클래스 
   작성자 : 이상준 
   내용 : 이동, 아이템 상호작용, 데미지, 죽음 등 메서드 포함
   마지막 수정일 : 2022.11.23
*/
public class ZombieCtrl : MonoBehaviour, ICharacter, IPunObservable
{

    private Animator anim;
    private Rigidbody rigid;
    public float RunSpd { get; set; }        // 달리기 속도 변환용 float형 변수
    public bool IsInvulerable { get; set; }  // 무적시간 제어용 bool형 변수

    private float dashSpeed;                 // 대쉬공격 속도 설정용 float형 변수
    private bool canMove;                    // 움직일 수 있는지 확인하기 위한 bool형 변수
    private Vector3 moveDir;                 // Run,Walk 등에 쓰인 dir 변수
    public Transform cameraArm;              // CameraArm 오브젝트 Transform 변수
    public Transform characterBody;          // CharacterBody 오브젝트 Transform 변수
    public Transform minimap;                // Minimap 오브젝트 Transform 변수

    private Vector3 net_currPos;             // 현재 위치 저장용 변수
    private Quaternion net_currRot;          // 현재 Quaternion값 저장용 변수
    private int net_anim;                    // 애니메이션 동기화용 int형 변수

    public Collider[] attackCols;            // 손에 붙어 있는 콜라이더
    public PhotonView pv;                    // 포톤뷰 변수
    private GameObject interactObj;          // 상호작용 오브젝트 변수
    private IEnumerator interaction;         // 상호작용 코루틴 변수
    private IEnumerator dash;                // 대쉬 공격 코루틴 변수
    public LayerMask targetMask;             // 타겟(인간) 마스크 
    public ZombieSoundCtrl audio;            // 효과음 컨트롤용 StudentSoundCtrl 변수
    private UIManager uiManager;             // UIManager 변수
    public ParticleSystem footsteps;         // 발자국 파티클 시스템
    private int stepDir;                     // 발자국 회전 방향 설정용 int형 변수
    public Material activeMat;               // 발자국 파티클 메터리얼

    // 정유진 추가 변수 
    // 계단 체크용 변수
    private Ray ray;                         // Ray 변수 
    private RaycastHit hitInfo;              // 레이저에 맞은 물체의 정보 받아오기용 RaycastHit형 변수
    private float maxSlopeAngle = 45.0f;     // 캐릭터가 올라갈 수 있는 최대 경사각 float형 변수

 

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = pv.instantiationData;
    }

    //상호작용 오브젝트(문)의 트리거에 반응 메서드
    private void OnTriggerEnter(Collider other)
    {
        if (pv.isMine)
        {
            if (other.gameObject.tag == "ImdUseItem")
            {
                if(other.gameObject.GetComponent<Door>() != null && !other.gameObject.GetComponent<Door>().isOpened)
                {
                    interactObj = other.gameObject;
                    IsInteractable(true);
                }
                
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (pv.isMine)
        {
            if (other.gameObject.tag == "ImdUseItem" && interactObj != null)
            {
                IsInteractable(false);
                interactObj = null;
            }
        }
    }
    
    //포톤 네트워크용 메서드 : 위치값과 각도 공유용 메서드
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(characterBody.localRotation);
        }
        else
        {
            net_currPos = (Vector3)stream.ReceiveNext();
            net_currRot = (Quaternion)stream.ReceiveNext();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (pv.isMine)
        {   //할퀴기 공격 
            if (collision.gameObject.tag == "Student" && anim.GetCurrentAnimatorStateInfo(0).IsName("Scratch") && !collision.gameObject.GetComponent<ICharacter>().IsInvulerable)
            {// 할퀴기 성공 판정의 조건 1. 대상이 학생(Student)일 것 2. 할퀴기 상태(애니)일 것 3. 대상이 무적상태가 아닐 것.
                collision.gameObject.GetComponent<ICharacter>().TakeDamage(1 , Vector3.zero);
            }
            //물기 공격
            else if (collision.gameObject.tag == "Student" && anim.GetCurrentAnimatorStateInfo(0).IsName("DashAttack") && !collision.gameObject.GetComponent<ICharacter>().IsInvulerable) //물기 공격 조건 1. 대상이 학생(Student)일것 2. 대쉬 공격 상태일 것. 3.Bite 코루틴이 한번만 실행될것.
            {// 할퀴기 성공 판정의 조건 1. 대상이 학생(Student)일 것 2. 대쉬 상태(애니)일 것 3. 대상이 무적상태가 아닐 것.
                collision.gameObject.GetComponent<ICharacter>().TakeDamage(pv.viewID ,collision.contacts[0].point);
                StopCoroutine(dash);
                rigid.velocity = Vector3.zero;
                StartCoroutine(this.Bite(collision.gameObject.transform.Find("StudentBody").Find("BitePos").gameObject));
            }
            //피격 판정(학생의 무기에 맞음)
            else if (collision.gameObject.tag == "Weapon" && !IsInvulerable)
            { //스턴(Hit) 조건 : 1.플레이어 무기(Weapon)과 충돌할 것. 2.내가 무적상태가 아닐 것
                TakeDamage(1, Vector3.zero);
            }
        }
    }

    //2022-11-15 정유진 추가
    //현재 캐릭터가 경사면에 있는지를 판별하는 bool형 함수 추가
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

    //2022-11-15 정유진 추가(캐릭터의 방향 설정)
    public Vector3 SetDirection(Vector3 direction)
    {
        //현재 캐릭터가 서 있는 경사 지형 평면 벡터로 이동 방향 벡터 투영
        return Vector3.ProjectOnPlane(direction, hitInfo.normal).normalized;
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        audio = GetComponent<ZombieSoundCtrl>();
        attackCols = transform.GetComponentsInChildren<Collider>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;
        net_anim = 0;
        gameObject.name = pv.owner.NickName;
        if (!pv.isMine)
        {
            cameraArm.gameObject.SetActive(false);
            minimap.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RunSpd = 5.0f;
        dashSpeed = 3f;
        IsInvulerable = false;
        canMove = true;
        attackCols[1].enabled = false;
        attackCols[2].enabled = false;
        stepDir = 1;
        if (pv.isMine)
        {
            this.gameObject.layer = 0;
            StartCoroutine(this.DetectStudent());
            uiManager.SetPlayer(this.gameObject);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (pv.isMine)
        {   // 마우스 왼쪽 버튼 : 할퀴기 공격
            if (Input.GetMouseButtonDown(0) && canMove)
            {
                StartCoroutine(this.Attack());
            }
            // 왼쪽 Shift 키 : 대쉬 공격(성공시 물기)
            if (Input.GetKeyDown(KeyCode.LeftShift) && canMove)
            {
                net_anim = 4;
                dash = DashAttack();
                StartCoroutine(dash);
            }
        }
    }

    private void FixedUpdate()
    {
        Run();
    }

    //이동함수. 카메라 앞 방향과 케릭터 몸 앞 방향을 일치시킴
    public void Run()
    {
        if (pv.isMine)
        {
            if (canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("DashAttack"))
            {
                Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                anim.SetFloat("RunSpeed", dir != Vector3.zero ? RunSpd : 0);
                if (anim.GetFloat("RunSpeed") != 0)
                {
                    Vector3 camForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
                    Vector3 camRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
                    moveDir = camForward * dir.z + camRight * dir.x;

                    //2022-11-15 유진 추가
                    //LookRotation 안에 넣어줄 변수...
                    //if문 안에서 moveDir 값을 바꿔줄 것이기 때문에 따로 추가했습니다
                    //선언 안해도 상관은 없지만 가독성을 위해 추가합니다!
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

                    characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 5.0f);
                    minimap.localRotation = Quaternion.Slerp(minimap.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 2.5f);
                }
                else
                {
                    rigid.velocity = Vector3.zero;
                }
                net_anim = anim.GetFloat("RunSpeed") != 0 ? 1 : 0;
                audio.PlaySound("Idle");
                string name = net_anim != 0 ? "Walk" : "None";
                audio.PlayBodySound(name);

            }
            else
            {
                //대쉬공격 중 방향전환
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("DashAttack"))
                {
                    Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                    if (dir != Vector3.zero)
                    {
                        Vector3 camForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
                        Vector3 camRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
                        moveDir = camForward * dir.z + camRight * dir.x;

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

                        rigid.velocity = (moveDir * RunSpd * 1.5f) + gravity;

                        characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 5.0f);
                        minimap.localRotation = Quaternion.Slerp(minimap.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 2.5f);
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

    // 주위의 학생 플레어이어를 감지해서
    // 뛰는 상태일 경우 발자국 파티클 생성메서드
    IEnumerator DetectStudent()
    {
        yield return new WaitForSeconds(3.0f);
        while (true)
        {
            yield return new WaitForSeconds(0.3f);
            Collider[] cols = Physics.OverlapSphere(transform.position, 10.0f, targetMask);
            if (cols.Length != 0)
            {//Physics.OverlapSphere 메서드를 통해서 콜라이더를 가져옴.
                foreach (Collider col in cols)
                {
                    if (col.gameObject.tag == "Student" && col.gameObject.GetComponent<ICharacter>().RunSpd > 6)
                    {// 인터페이스에 상속된 RunSpd 값을 통해서 6보다 클 경우(뛰는 상태) 그 자리에 발자국 파티클 생성.
                        Debug.Log("Running Student Detected");
                        ParticleSystem.EmitParams ep1 = new ParticleSystem.EmitParams();
                        footsteps.GetComponent<Renderer>().material = activeMat;
                        Transform studentBody = col.transform.Find("StudentBody");
                        ep1.position = col.transform.position + (col.transform.right * 0.5f * stepDir) + new Vector3(0, +0.2f, 0);
                        stepDir *= -1;
                        ep1.rotation = studentBody.rotation.eulerAngles.y;
                        footsteps.Emit(ep1, 1);
                    }
                }
            }
        }
    }

    //일반 공격(할퀴기) 
    IEnumerator Attack()
    {
        canMove = false;
        net_anim = 3;
        attackCols[1].enabled = true;
        attackCols[2].enabled = true;
        anim.SetTrigger("Attack");
        rigid.AddForce(transform.forward * 80);
        yield return new WaitForSeconds(0.3f);
        audio.PlaySound("Attack");
        yield return new WaitForSeconds(2.7f);
        canMove = true;
        attackCols[1].enabled = false;
        attackCols[2].enabled = false;
    }

    //대쉬 공격
    IEnumerator DashAttack()
    {
        canMove = false;

        attackCols[1].enabled = true;
        attackCols[2].enabled = true;

        anim.SetTrigger("DashAttack");
        audio.PlaySound("DashAttack");
        audio.PlayBodySound("Dash");
        pv.RPC("NetAnimSet", PhotonTargets.Others, 4);
        float currentSpd = dashSpeed;
        float lastAttackTime = Time.time;
        while (Time.time < lastAttackTime + 3.0f)
        {//케릭터 몸 기준 앞 방향으로 돌진
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("DashAttack"))
            {
                anim.SetTrigger("DashAttack");
            }
            
            rigid.velocity = characterBody.forward * currentSpd;
            anim.SetFloat("RunVelocity", (currentSpd * 0.2f));

            if (Time.time > lastAttackTime + 1.9f)
            {//대쉬 후 1.9초 후 이동속도 *0.8
                currentSpd *= 0.8f;
            }
            else
            { //대쉬 후 1.0초 후 
                if (Time.time > lastAttackTime + 1.0f)
                {//이동속도 *1.2
                    currentSpd *= 1.2f;
                }
                else
                {//이동속도 *1.1
                    currentSpd *= 1.1f;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
        rigid.velocity = Vector3.zero;
        
        anim.SetTrigger("DashAttackFail"); //대쉬가 그냥 끝날 경우 다시 일반 상태로 돌아옴
        pv.RPC("NetAnimSet", PhotonTargets.Others, 5);
        yield return new WaitForSeconds(1f);

        attackCols[1].enabled = false;
        attackCols[2].enabled = false;
        canMove = true;
    }

    //학생을 물었을 때 위치 고정 코루틴 메서드
    IEnumerator fixPos(Vector3 targetPos, GameObject target = null)
    {
        yield return null;

        float currTime = Time.time;
        Vector3 vec = Vector3.zero;
        rigid.isKinematic = true;
        if (target != null)
        {
            vec = target.transform.parent.transform.position - transform.position;
            vec.Normalize();

        }
        while (true)
        {

            transform.position = targetPos;
            if (vec != Vector3.zero) characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(vec), Time.deltaTime * 5.0f);
            yield return null;
            if (Time.time > currTime + 2.5f)
            {
                rigid.isKinematic = false;
                break;
            }
        }
    }

    //물기 코루틴 메서드
    IEnumerator Bite(GameObject bitePos)
    {
        canMove = false;
        audio.PlayBodySound(name, true);
        anim.SetTrigger("DashAttackSuccess");
        audio.PlaySound("Bite");

        rigid.velocity = Vector3.zero;
        Vector3 targetPos = new Vector3(bitePos.transform.position.x, bitePos.transform.parent.transform.position.y, bitePos.transform.position.z);
        StartCoroutine(fixPos(transform.position, bitePos.transform.parent.gameObject)); // 위치 고정 코루틴 메서드 실행
        rigid.velocity = Vector3.zero;

        net_anim = 6;
        IsInvulerable = true;        //물기 공격시간 동안 무적 상태 
        attackCols[1].enabled = false;
        attackCols[2].enabled = false;

        yield return new WaitForSeconds(4.5f);
        IsInvulerable = false;
        canMove = true;
    }

    //피격 메서드(로컬 오브젝트)
    public void TakeDamage(int _atkType, Vector3 vec)
    {
        if (!IsInvulerable) //감염 상태도 아니고, 무적도 아닐 때
        {
            pv.RPC("TakeDamagePhoton", PhotonTargets.All);
        }
    }

    //피격 포톤 RPC 메서드
    [PunRPC]
    void TakeDamagePhoton()
    {
        StartCoroutine(this.Hit());
    }


    //피격 처리 코루틴 메서드
    IEnumerator Hit()
    {
        canMove = false;
        rigid.velocity = Vector3.zero;
        net_anim = 7;
        IsInvulerable = true;  //물기 공격시간 동안 무적 상태 
        attackCols[1].enabled = false;
        attackCols[2].enabled = false;
        if (dash != null)
        {
            StopCoroutine(dash);
        }
        anim.SetTrigger("Hit");
        audio.PlaySound("Hit");
        yield return new WaitForSeconds(3f);
        IsInvulerable = false;
        canMove = true;
    }

    //상호작용 코루틴 실행, 중지 함수
    public void IsInteractable(bool temp) 
    {
        if (temp)
        {
            uiManager.ShowIntrText(true, this.gameObject.tag);
            interaction = this.Interaction();
            StartCoroutine(interaction);
        }
        else
        {
            uiManager.ShowIntrText(false, "");
            StopCoroutine(interaction);
        }
    }
    //주위에 상호작용 가능한 오브젝트가 있을시 발동되는 코루틴 메서드.
    IEnumerator Interaction()
    {
        yield return null;

        while (true)
        {
            Debug.Log("interaction...");
            yield return new WaitForSeconds(0f);
            if (interactObj == null || !interactObj.GetComponent<Collider>().enabled)
            { // 상호작용 오브젝트가 null 이거나 콜라이더가 enabled = false라면 중지.
                IsInteractable(false);
                yield break;
            }
            else
            {
                float dis = Vector3.Distance(transform.position, interactObj.transform.position);
                if (Input.GetKeyUp(KeyCode.E) && dis <= 2.5f && canMove)
                {// 해당 ItemStorage와 2.5f 거리 이내이고 E키를 누를 시
                    if (interactObj.gameObject.name.Contains("Door") && interactObj.GetComponent<Door>().breakCo == null)
                    { //문일 경우 열린 상태가 아니라면 실행
                        Debug.Log("attck door");
                        StartCoroutine(this.Kick(interactObj));
                        if (interactObj.GetComponent<IItem>().Durability <= 0)
                        {
                            IsInteractable(false);
                            yield break;
                        }
                    }
                }
            }
        }
    }

    //문을 차는 처리 코루틴 메서드
    IEnumerator Kick(GameObject interactObj)
    {
        yield return null;
        canMove = false;
        IItem item = interactObj.GetComponent<IItem>();
        anim.SetTrigger("Kick");
        pv.RPC("NetAnimSet", PhotonTargets.Others, 8);

        Vector3 vec = interactObj.transform.parent.transform.position - transform.position;
        vec.Normalize();
        float leftTime = Time.time;
        while (true)
        {//방향을 상호작용 오브젝트를 바라봄.
            yield return null;
            if (vec != Vector3.zero) characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(vec), Time.deltaTime * 5.0f);
            if (Time.time > leftTime + 1.0f)
            { 
                break;
            }
        }
        item.Use(this.gameObject);
        yield return new WaitForSeconds(0.5f);
        IsInvulerable = false;
        canMove = true;
    }

    //AniState 애니메이션 네트워크 실행용 RPC 함수
    [PunRPC]
    void NetAnimSet(int net_anim)
    {
        switch (net_anim)
        {
            case 0:
                anim.SetFloat("RunSpeed", 0);
                break;
            case 1:
                anim.SetFloat("RunSpeed", 1);
                break;
            case 3:
                anim.SetTrigger("Attack");
                break;
            case 4:
                anim.SetTrigger("DashAttack");
                break;
            case 5:
                anim.SetTrigger("DashAttackFail");
                break;
            case 6:
                anim.SetTrigger("DashAttackSuccess");
                break;
            case 7:
                anim.SetTrigger("Hit");
                break;
            case 8:
                anim.SetTrigger("Kick");
                break;
        }
    }

}
