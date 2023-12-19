using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Profiling;

namespace ManagerControl
{
    public class AddressableManager : MonoBehaviour
    {
        private List<GameObject> _gameObjects;

        [SerializeField] private AssetReferenceGameObject[] managerObjects;

        protected void Awake()
        {
            _gameObjects = new List<GameObject>();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Profiler.maxUsedMemory = 134217728;
        }

        protected void Start()
        {
            SpawnObject().Forget();
        }

        private void OnDisable()
        {
            Release();
        }

        private async UniTaskVoid SpawnObject()
        {
            for (var i = 0; i < managerObjects.Length; i++)
            {
                var handle = managerObjects[i].InstantiateAsync();
                await handle;
                handle.Completed += obj => _gameObjects.Add(obj.Result);
            }

            await UniTask.Delay(2000);
            Destroy(GameObject.Find("Plane"));
        }

        private void Release()
        {
            for (int i = _gameObjects.Count - 1; i >= 0; i--)
            {
                if (_gameObjects.Count <= 0) return;
                var index = _gameObjects.Count - 1;
                if (_gameObjects[index] == null) continue;
                Addressables.ReleaseInstance(_gameObjects[index]);
                _gameObjects.RemoveAt(index);
            }
        }
    }
}