using System;
using System.Collections.Generic;
using BackendControl;
using Cysharp.Threading.Tasks;
using InterfaceControl;
using UIControl;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ManagerControl
{
    public class AddressableManager : MonoBehaviour
    {
        private List<GameObject> _gameObjects;

        [SerializeField] private AssetReferenceGameObject[] managerObjects;

        protected void Awake()
        {
            _gameObjects = new List<GameObject>();
        }

        protected void Start()
        {
            ObjectInit().Forget();
        }

        private void OnDestroy()
        {
            Release();
        }

        private async UniTask SpawnObject()
        {
            for (var i = 0; i < managerObjects.Length; i++)
            {
                var handle = managerObjects[i].InstantiateAsync();
                await handle;
                handle.Completed += obj => _gameObjects.Add(obj.Result);
            }
        }

        private async UniTaskVoid ObjectInit()
        {
            await SpawnObject();
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                _gameObjects[i].GetComponent<IAddressableObject>().Init();
            }

            if (BackendGameData.isRestart)
            {
                BackendGameData.isRestart = false;
                UIManager.MapSelectButton(BackendGameData.difficultyLevel);
                Destroy(FindAnyObjectByType<MapSelectPanel>().gameObject);
            }

            await UniTask.Delay(2000);
            Destroy(GameObject.Find("Plane"));
        }

        private void Release()
        {
            for (var i = _gameObjects.Count - 1; i >= 0; i--)
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