using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class csPlayer : MonoBehaviour
{
    public PhotonView pv;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        gameObject.name = pv.owner.NickName;
    }

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = pv.instantiationData;
    }
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
    public class Player
    {
        public string myName;
        public string myScore;
        
        public Player(string _name, string _score)
        {
            myName = _name;
            myScore = _score;
        }
    }

