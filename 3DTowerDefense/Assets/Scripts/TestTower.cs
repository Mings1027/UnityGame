using System;
using UnityEngine;


public class TestTower : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    private void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Player");
    }
}