using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameControl
{
    public class TimedObjectDestroyer : MonoBehaviour
    {
        [SerializeField] private float lifeTime;
        
        private async UniTaskVoid OnEnable()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(lifeTime));
            gameObject.SetActive(false);
        }
    }
}
