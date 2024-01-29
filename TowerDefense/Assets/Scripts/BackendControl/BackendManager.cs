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
                    BackendLogin.instance.UpdateNickname(
                        Backend.BMember.GetUserInfo().GetReturnValuetoJSON()["row"]["gamerId"].ToString()[..7]);
                }

                // BackendLogin.instance.UpdateNickname("원하는 이름");
                // BackendRank.instance.RankInsert(100);
                // BackendRank.instance.RankGet();

                CustomLog.Log("테스트를 종료합니다");
                FindAnyObjectByType<DownloadManager>().CheckUpdateFiles().Forget();
            });
        }
    }
}