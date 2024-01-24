using System;
using System.Threading.Tasks;
using BackEnd;
using Cysharp.Threading.Tasks;
using LobbyControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;
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
                Debug.LogError("초기화 실패 : " + bro);
            }
        }

        public async void BackendInit()
        {
            await Task.Run(() =>
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