using BackEnd;
using Cysharp.Threading.Tasks;
using ManagerControl;
using UnityEngine;
using Utilities;

namespace BackendControl
{
    public class BackendManager : MonoBehaviour
    {
        private void Awake()
        {
            var bro = Backend.Initialize(true);

            if (bro.IsSuccess())
            {
                CustomLog.Log("초기화 성공 : " + bro);
            }
            else
            {
                CustomLog.LogError("초기화 실패 : " + bro);
            }
        }

        public async UniTaskVoid BackendInit()
        {
            await UniTask.RunOnThreadPool(() =>
            {
                BackendGameData.instance.GameDataGet();
                if (BackendGameData.userData == null)
                {
                    BackendGameData.instance.GameDataInsert();
                }

                BackendGameData.instance.GameDataUpdate();

                CustomLog.Log("테스트를 종료합니다");
                FindAnyObjectByType<DownloadManager>().CheckUpdateFiles().Forget();
            });
        }
    }
}