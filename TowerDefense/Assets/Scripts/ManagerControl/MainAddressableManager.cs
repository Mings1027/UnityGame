using System.Collections.Generic;
using BackendControl;
using Cysharp.Threading.Tasks;
using InterfaceControl;
using UIControl;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ManagerControl
{
    public class MainAddressableManager : MonoBehaviour
    {
        private List<GameObject> _managerObjs;

        [SerializeField] private AssetReferenceGameObject[] managerObjs;

        private void Start()
        {
            _managerObjs = new List<GameObject>();
            SpawnObject().Forget();
        }

        private void OnDestroy()
        {
            for (var i = 0; i < managerObjs.Length; i++)
            {
                if (_managerObjs.Count <= 0) return;
                var index = _managerObjs.Count - 1;
                if (_managerObjs[index] == null) continue;
                Addressables.ReleaseInstance(_managerObjs[index]);
                _managerObjs.RemoveAt(index);
            }
        }

        private async UniTask InstantiateAsync()
        {
            for (var i = 0; i < managerObjs.Length; i++)
            {
                var handle = managerObjs[i].InstantiateAsync();
                await handle;
                handle.Completed += obj => _managerObjs.Add(obj.Result);
            }
        }

        private async UniTaskVoid SpawnObject()
        {
            await InstantiateAsync();
            await UniTask.Yield();
            for (var i = 0; i < managerObjs.Length; i++)
            {
                if (_managerObjs[i].TryGetComponent(out IMainGameObject addressableObject))
                {
                    addressableObject.Init();
                }
            }

            var mapSelectPanel = FindAnyObjectByType<MapSelectPanel>();
            if (BackendGameData.isRestart)
            {
                BackendGameData.isRestart = false;
                UIManager.MapSelectButton(BackendGameData.difficultyLevel);
                Destroy(mapSelectPanel.gameObject);
            }
            else
            {
                mapSelectPanel.OpenMapSelector();
            }

            FadeController.FadeInScene();

            await UniTask.Delay(5000);
            Destroy(GameObject.Find("Plane"));
        }
    }
}