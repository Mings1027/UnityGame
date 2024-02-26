using System;
using UnityEngine;

namespace GameControl
{
    [DisallowMultipleComponent]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;
        
        protected virtual void Awake()
        {
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}