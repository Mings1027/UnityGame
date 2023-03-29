using System;
using AttackControl;
using DG.Tweening;
using GameControl;
using UnityEngine;


public class TestTower : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            print("this is enemy");
        }
    }
}