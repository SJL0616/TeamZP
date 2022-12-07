using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RankData : MonoBehaviour
{
    public Text rankNum;
    public Text txtPlayerName;
    public Text txtRankScore;
    public Image myProfile;

    public void DisplayRankData(string _rankNum, Sprite _img, string _name, string _score)
    {
        rankNum.text = _rankNum;
        myProfile.sprite = _img;
        txtPlayerName.text = _name;
        txtRankScore.text = _score;
    }


}
