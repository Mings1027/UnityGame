using UnityEngine;

namespace GameControl
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = (T)FindObjectOfType(typeof(T));
                if (_instance != null) return _instance;
                var obj = new GameObject();
                _instance = obj.AddComponent(typeof(T)) as T;
                obj.name = typeof(T).ToString();

                // DontDestroyOnLoad(obj);

                return _instance;
            }
        }
    }
}