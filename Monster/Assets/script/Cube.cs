﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public int co;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)

    {

     //   Debug.Log("충돌 시작!");

    }



    // Collider 컴포넌트의 is Trigger가 false인 상태로 충돌중일 때
    //바닥은 제외해야 한다.
    private void OnCollisionStay(Collision collision)

    {

        
       // Debug.Log("충돌 중!");

    }



    // Collider 컴포넌트의 is Trigger가 false인 상태로 충돌이 끝났을 때

    private void OnCollisionExit(Collision collision)

    {
       // Debug.Log("충돌 끝!");

    }



}
