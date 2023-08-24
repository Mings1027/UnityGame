using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestController : MonoBehaviour
{
    private bool enable;

    [SerializeField] private Test test;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack().Forget();
        }
    }

    private async UniTaskVoid Attack()
    {
        if (enable) return;
        enable = true;
        test.enabled = true;
        await UniTask.Delay(1000);
        test.enabled = false;
        enable = false;
    }
}