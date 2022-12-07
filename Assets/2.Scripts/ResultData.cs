using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultData : MonoBehaviour
{
    // 결과창 플레이어
    public Text myName;
    public Text myScore;

    public void DisplayResultData(string _name, string _score)
    {
        myName.text = _name;
        myScore.text = _score;
    }
}
