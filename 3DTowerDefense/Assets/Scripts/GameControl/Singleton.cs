using UnityEngine;

namespace GameControl
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));
                    if (instance == null)
                    {
                        var obj = new GameObject();
                        instance = obj.AddComponent(typeof(T)) as T;
                        obj.name = typeof(T).ToString();

                        DontDestroyOnLoad(obj);

                        return instance;
                    }

                    return instance;
                }

                return instance;
            }
        }
    }
}