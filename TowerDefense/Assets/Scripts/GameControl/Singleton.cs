using UnityEngine;

namespace GameControl
{
    [DisallowMultipleComponent]
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool _applicationQuit;
        private static T _instance;
        private static GameObject _containerObject;

        public static T Instance
        {
            get
            {
                if (_applicationQuit) return null;
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        _instance = ContainerObject.GetComponent<T>();
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

        public static GameObject ContainerObject
        {
            get
            {
                if (_containerObject == null)
                    CreateContainerObject();
                return _containerObject;
            }
        }

        private static void CreateContainerObject()
        {
            if (_containerObject != null) return;
            _containerObject = new GameObject($"[Singleton] {typeof(T)}");
            if (_instance == null)
                _instance = ContainerObject.AddComponent<T>();
        }
    }
}