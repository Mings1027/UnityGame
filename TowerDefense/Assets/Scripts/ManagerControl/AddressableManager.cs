using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

namespace ManagerControl
{
    public class AddressableManager : MonoBehaviour
    {
        private List<GameObject> _gameObjects;

        [SerializeField] private AssetReferenceGameObject[] managerObjects;

        protected void Awake()
        {
            _gameObjects = new List<GameObject>();
            // Application.targetFrameRate = 60;
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
                managerObjects[i].InstantiateAsync().Completed += obj => { _gameObjects.Add(obj.Result); };
            }

            await UniTask.Delay(2000);
            transform.GetChild(0).GetComponent<NavMeshSurface>().RemoveData();
            Destroy(transform.GetChild(0).gameObject);
        }

        private void Release()
        {
            for (int i = _gameObjects.Count - 1; i >= 0; i--)
            {
                if (_gameObjects.Count <= 0) return;
                var index = _gameObjects.Count - 1;
                Addressables.ReleaseInstance(_gameObjects[index]);
                _gameObjects.RemoveAt(index);
            }
        }
    }
}