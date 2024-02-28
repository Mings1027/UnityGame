using System;
using BackEnd;
using Cysharp.Threading.Tasks;
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

        private void Update()
        {
            if (Backend.ErrorHandler.UseAsyncQueuePoll)
            {
                Backend.ErrorHandler.Poll();
            }
        }

        public static async UniTaskVoid BackendInit()
        {
            await UniTask.RunOnThreadPool(() =>
            {
                BackendGameData.instance.GameDataGet();
                if (BackendGameData.userData == null)
                {
                    BackendGameData.instance.GameDataInsert();
                    BackendLogin.instance.UpdateNickname(
                        Backend.BMember.GetUserInfo().GetReturnValuetoJSON()["row"]["gamerId"].ToString()[..7]);
                }

                Debug.Log("테스트를 종료합니다");
            });

            if (Backend.IsInitialized)
            {
                Debug.Log("여기여기여기여기여기여기여기");
                Backend.ErrorHandler.InitializePoll(true);
                Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = () => { Debug.Log("외부 로그인 감지!!!"); };
            }
        }
    }
}