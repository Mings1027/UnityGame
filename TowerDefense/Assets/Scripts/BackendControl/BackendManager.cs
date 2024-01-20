using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    private void Start()
    {
        var be = Backend.Initialize(true);

        if (be.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + be);
        }
        else
        {
            Debug.LogError("초기화 실패 : "+ be);
        }
    }
}
