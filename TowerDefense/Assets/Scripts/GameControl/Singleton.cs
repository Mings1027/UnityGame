using UnityEngine;

namespace GameControl
{
    [DisallowMultipleComponent]
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool _applicationQuit;
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_applicationQuit) return null;
                if (!_instance)
                {
                    _instance = (T)FindAnyObjectByType(typeof(T));
                    if (!_instance)
                    {
                        var obj = new GameObject(typeof(T).ToString());
                        _instance = obj.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        [SerializeField] private bool dontDestroyOnLoad;

        protected virtual void Awake()
        {
            _applicationQuit = false;
            if (!dontDestroyOnLoad) return;
            var obj = FindObjectsOfType<T>();
            if (obj.Length == 1)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            _applicationQuit = true;
        }
    }
}