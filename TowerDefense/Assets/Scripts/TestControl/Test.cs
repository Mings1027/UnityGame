using System;
using PoolObjectControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace TestControl
{
    public class Test : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PoolObjectManager.Get(PoolObjectKey.Goblin, new Vector3(10, 10, 10));
            }
        }
    }
}