using System;
using UnityEngine;

namespace GameControl
{
    public class SettingPoolObject : MonoBehaviour
    {
        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}
