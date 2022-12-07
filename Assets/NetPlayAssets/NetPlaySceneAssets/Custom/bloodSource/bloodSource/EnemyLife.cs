//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemyLife : MonoBehaviour
//{
//    private int life = 100;
//    private Transform myTr;
//    public GameObject enemyBloodEffect;
//    public Transform enemyBloodDecal;

//    public EnemyCtrl enemy;
//    public MeshRenderer lifeBar;

//    //포톤추가
//    public PhotonView pv = null;

//    void Awake()
//    {
//        myTr = GetComponent<Transform>();

//        //포톤 추가 , 다른 방식으로도 연결 가능하다~ 
//        //pv = GetComponent<PhotonView>();

//        //return component.GetComponent<PhotonView>();
//        pv = PhotonView.Get(this);
//    }

//    void OnCollisionEnter(Collision col)
//    {
//        if(col.gameObject.tag=="Bullet")
//        {
//            ContactPoint contact = col.contacts[0];
            
//            CreateBlood(contact.point);

//            //포톤 추가
//            int pow = col.gameObject.GetComponent<BulletCtrl>().power;
//            int id = col.gameObject.GetComponent<BulletCtrl>().playerId;

//            pv.RPC("Damage", PhotonTargets.AllBuffered, pow, id);

//            //포톤 추가함에 따라 아래는 주석처리 하겠음
//            //life -= col.gameObject.GetComponent<BulletCtrl>().power;
//            //lifeBar.material.SetFloat("_Progress", life / 100.0f);
//            //if(life<=0)
//            //{
//            //    //자신을 파괴시킨 적 네트워크 베이스의 스코어 증가시킴
//            //    StartCoroutine(this.SaveKillCount(col.gameObject.GetComponent<BulletCtrl>().playerId));
//            //    enemy.EnemyDie();
//            //}
//            //여기까지 주석처리

//            enemy.HitEnemy();
//        }
//    }

//    //포톤 추가
//    [PunRPC]
//    void Damage(int dam, int id)
//    {
//        life -= dam;
//        lifeBar.material.SetFloat("_Progress", life / 100.0f);

//        if(life<=0)
//        {
//            StartCoroutine(this.SaveKillCount(id));
//            enemy.EnemyDie();
//        }
//    }


//    void OnCollision(object[] _params)
//    {
//        Debug.Log(string.Format("info {0}:{1}", _params[0], _params[1]));

//        CreateBlood((Vector3)_params[0]);

//        int pow = (int)_params[1];
//        int id = (int)_params[2];

//        pv.RPC("PDamage", PhotonTargets.AllBuffered, pow, id);

//        //life -= (int)_params[1];
//        //lifeBar.material.SetFloat("_Progess", life / 100.0f);

//        //if(life<=0)
//        //{
//        //    enemy.EnemyDie();
//        //}
//    }

//    [PunRPC]
//    void PDamage(int dam, int id)
//    {
//        life -= dam;
//        lifeBar.material.SetFloat("_Progress", life / 100.0f);

//        if (life <= 0)
//        {
//            StartCoroutine(this.PSaveKillCount(id));
//            enemy.EnemyDie();
//        }
//    }

//    public void OnCollisionBarrel(Vector3 firePos)
//    {
//        CreateBlood(firePos);

//        life = 0;
//        lifeBar.material.SetFloat("_Progress", life / 100.0f);

//        enemy.EnemyBarrelDie(firePos);
//    }

//    void CreateBlood(Vector3 pos)
//    {
//        StartCoroutine(this.CreateBloodEffects(pos));
//    }

//    IEnumerator CreateBloodEffects(Vector3 pos)
//    {
//        GameObject enemyblood1 = Instantiate(enemyBloodEffect, pos, Quaternion.identity) as GameObject;
//        //Destroy(enemyblood1, 1.5f);

//        //만약 혈흔 프리팹에 차일드 오브젝트를(혈흔) up 방향으로 미리 올려놨다면...
//        //혈흔데칼의 생성되는 위치는 바닥에서 조금 올린 위치로 만들어야 바닥에 묻히지 않는다 
//        //근데 나는 이미 포지션 값 줘서 조절해놓긴 했음!^^
//        //Vector3 decalPos = myTr.position + (Vector3.up * 0.1f);

//        Quaternion decalRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
//        float scale = Random.Range(1.0f, 2.5f);

//        //혈흔데칼 프리팹 생성
//        //Transform enemyblood2=Instantiate(enemyBloodDecal, decalPos, decalRot) as Transform;
//        //만약 혈흔 프리펩에 차일드 오브젝트를(혈흔) up 방향으로 미리 올려놨다면...
//        Transform enemyblood2 = Instantiate(enemyBloodDecal, myTr.position, decalRot) as Transform;

//        enemyblood2.localScale = Vector3.one * scale;

//        yield return null;
//    }

//    //포톤 추가 
//    IEnumerator SaveKillCount(int firePlayerId)
//    {
//        GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
//        ////플레이어 추가
//        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

//        foreach(GameObject _base in bases)
//        {
//            var baseCtrl = _base.GetComponent<BaseCtrl>();
            
//            //네트워크 베이스의 playerId가 총알의 playerId와 동일한지 판단
//            //firePlayerId는 불렛 컨트롤의 플레이어id
//            if(baseCtrl!=null&&baseCtrl.playerId==firePlayerId)
//            {
//                baseCtrl.PlusKillCount();
//                break;
//            }
//        }

//        //foreach(GameObject _player in players)
//        //{
//        //    var playerCtrl = _player.GetComponent<PlayerCtrl>();

//        //    if(playerCtrl!=null&&playerCtrl.playerId==firePlayerId)
//        //    {
//        //        playerCtrl.PlusKillCount();
//        //        break;
//        //    }    
//        //}

//        yield return null;
//    }

//    IEnumerator PSaveKillCount(int firePlayerId)
//    {
//        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

//        foreach(GameObject _player in players)
//        {
//            var playerCtrl = _player.GetComponent<PlayerCtrl>();

//            if (playerCtrl != null&&playerCtrl.playerId==firePlayerId)
//            {
//                playerCtrl.PlusKillCount();
//                break;
//            }
//        }

//        yield return null;
//    }
//}
