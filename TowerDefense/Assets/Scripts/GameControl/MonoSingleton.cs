using UnityEngine;

namespace GameControl
{
    [DisallowMultipleComponent]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;
        [SerializeField] protected bool dontDestroyOnLoad;
        
        protected virtual void Awake()
        {
            if(dontDestroyOnLoad) DontDestroyOnLoad(this);
            instance = this as T;
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}