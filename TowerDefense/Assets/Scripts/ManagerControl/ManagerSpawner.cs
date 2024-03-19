using BackendControl;
using Cysharp.Threading.Tasks;
using InterfaceControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class ManagerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] managerObjs;

        private void Start()
        {
            SpawnObject().Forget();
        }

        private async UniTaskVoid SpawnObject()
        {
            var objs = new GameObject[managerObjs.Length];
            for (var i = 0; i < managerObjs.Length; i++)
            {
                objs[i] = Instantiate(managerObjs[i]);
            }

            for (var i = 0; i < objs.Length; i++)
            {
                if (objs[i].TryGetComponent(out IMainGameObject addressableObject))
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