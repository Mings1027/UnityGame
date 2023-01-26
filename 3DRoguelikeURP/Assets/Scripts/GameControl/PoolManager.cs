using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public void SetPool<T>(IObjectPool<T> pool) where T : class { }
}