using System;
using UnityEngine;

namespace GameControl
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object Lock = new();
        private static bool _applicationQuit;

        public static T Instance
        {
            get
            {
                if (_applicationQuit) return null;
                lock (Lock)
                {
                    if (_instance != null) return _instance;
                    _instance = FindObjectOfType<T>();
                    if (_instance != null) return _instance;
                    var componentName = typeof(T).ToString();
                    var findObject = GameObject.Find(componentName);
                    if (findObject == null) findObject = new GameObject(componentName);

                    _instance = findObject.AddComponent<T>();
                    DontDestroyOnLoad(_instance);

                    return _instance;
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationQuit = true;
        }

        public virtual void OnDestroy()
        {
            _applicationQuit = true;
        }
    }
}