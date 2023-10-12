using System;
using UnityEngine;

namespace GameControl
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        private static bool _applicationQuit = false;
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_applicationQuit) return null;
                if (_instance != null) return _instance;
                _instance = FindObjectOfType(typeof(T)) as T;

                if (_instance != null) return _instance;
                var singletonObj = new GameObject();
                _instance = singletonObj.AddComponent<T>();
                singletonObj.name = typeof(T).ToString();

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _applicationQuit = false;
        }

        private void OnApplicationQuit()
        {
            _applicationQuit = true;
        }

        private void OnDestroy()
        {
            _applicationQuit = true;
        }
    }
}