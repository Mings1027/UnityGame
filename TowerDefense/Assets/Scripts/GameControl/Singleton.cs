using UnityEngine;

namespace GameControl
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool _applicationQuit;
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

        // protected virtual void Start()
        // {
        //     _applicationQuit = false;
        // }

        private void OnApplicationQuit()
        {
            _applicationQuit = true;
        }

        // private void OnDestroy()
        // {
        //     _applicationQuit = true;
        // }
    }
}