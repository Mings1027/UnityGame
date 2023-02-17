using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject aaa;

    private void FixedUpdate()
    {
        if (!aaa)
        {
            print(aaa);
        }
    }
}