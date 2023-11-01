using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            GameInit().Forget();
        }

        private async UniTaskVoid GameInit()
        {
            var sources = Resources.LoadAll<GameObject>("Prefabs");
            for (var i = 0; i < sources.Length; i++)
            {
                Instantiate(sources[i]);
            }

            await UniTask.Yield();

            Instantiate(Resources.Load<GameObject>("PoolObjectManager"));
            
            LocaleManager.ChangeLocale(0);
        }
    }
}