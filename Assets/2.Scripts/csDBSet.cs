using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// DB에 이름 저장 하는 스크립트(이름만 저장되면 사람과 좀비 점수를 php문으로 인해 자동으로 1000점으로 세팅됨
public class csDBSet : MonoBehaviour
{
    string SetURL = "http://teamzombie.dothome.co.kr/Create.php"; //DB 호스트 도메인
    public InputField setname;

    //버튼 클릭
    public void FirstPlayer()
    {
        //2022-11-22 원빈 추가 글자수 생성 제한 
        if (setname.text.Length < 7 && setname.text.Length > 1)
        {
            StartCoroutine(SetName(setname.text));
        }
        else
        {
            if (setname.text.Length > 6)
            {   
                Debug.LogWarning("최대 글자 수는 6글자 입니다.");
            }
            if (setname.text.Length < 2)
            {
                Debug.LogWarning("최소 글자 수는 2글자 입니다.");
            }

        }
    }

    //DB에서 불러오는 것은 코루틴으로'만' 불러와짐
    IEnumerator SetName(string _name)
    {

        WWWForm form = new WWWForm(); //DB에 값을 보내겠다는 변수
        form.AddField("getname", _name); // 값을 보냄

        WWW dataPost = new WWW(SetURL, form); // DB에서 쿼리문 처리

        yield return dataPost; // 쿼리문 처리까지 대기

        if (dataPost.error != null)
        {
            Debug.LogWarning("Error!!!!");
        }
        else
        {

            if (System.Text.Encoding.UTF8.GetString(dataPost.bytes) == "Success")
            {
                SaveData();
                SceneManager.LoadScene("scLobby");
            }
            else
            {
                Debug.Log(System.Text.Encoding.UTF8.GetString(dataPost.bytes));
            }
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetString("Player", setname.text);
    }

}
