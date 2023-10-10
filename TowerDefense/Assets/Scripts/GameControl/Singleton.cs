using System;
using UnityEngine;

namespace GameControl
{
    public abstract class Singleton<T> : MonoBehaviour where T : class
    {
        private static readonly Lazy<T> _instance = new(() =>
        {
            if (FindObjectOfType(typeof(T)) is T instance) return instance;
            var obj = new GameObject(typeof(T).ToString());
            instance = obj.AddComponent(typeof(T)) as T;

            return instance;
        });

        public static T Instance => _instance.Value;
    }
}