using System;
using System.Collections.Generic;
using DG.Tweening;
using StatusControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    private void OnEnable()
    {
        InvokeRepeating(nameof(Attack), 0, 1);
    }

    private void Attack()
    {
        print("13123123");
    }
}