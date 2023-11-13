using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

namespace ManagerControl
{
    public class AddressableManager : MonoBehaviour
    {
        private List<GameObject> _gameObjects;

        [SerializeField] private AssetReferenceGameObject[] managerObjects;
        // [SerializeField] private AssetLabelReference managerLabel;

        protected void Awake()
        {
            _gameObjects = new List<GameObject>();
            Application.targetFrameRate = 60;
        }

        protected void Start()
        {
            SpawnObject();
        }

        private void OnDisable()
        {
            Release();
        }

        private void SpawnObject()
        {
            for (var i = 0; i < managerObjects.Length; i++)
            {
                managerObjects[i].InstantiateAsync().Completed += obj => { _gameObjects.Add(obj.Result); };
            }

            for (int i = 0; i < _gameObjects.Count; i++)
            {
                
            }
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