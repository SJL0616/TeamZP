using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructor : MonoBehaviour
{
    public float deadTime;

    void Start()
    {
        Destroy(gameObject, deadTime);    
    }
}
