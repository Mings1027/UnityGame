using System.Collections;
using System.Collections.Generic;
using GameControl;
using UnityEngine;

public class TurretWeapon : MonoBehaviour
{
    [SerializeField] private Transform[] attackPoint;
    [SerializeField] private string bulletName;

    public void Attack()
    {
        for (int i = 0; i < attackPoint.Length; i++)
        {
            StackObjectPool.Get(bulletName, attackPoint[i].position);
        }
    }
}