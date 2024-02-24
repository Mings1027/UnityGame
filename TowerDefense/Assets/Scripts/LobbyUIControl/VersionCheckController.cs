using System;
using BackEnd;
using Cysharp.Threading.Tasks;
using UIControl;
using UnityEngine;
using UnityEngine.Networking;
using Utilities;

namespace LobbyUIControl
{
    public class VersionCheckController : MonoBehaviour
    {
        // private const string BundleID = "com.DefenseCompany.RogueDefense";

        [SerializeField] private string appstoreURL = "itms-apps://itunes.apple.com/app/id6472429843?uo=4";
        [SerializeField] private string playStoreURL;

        [SerializeField] private NoticePanel updateNoticePanel;

        private void Start()
        {
            updateNoticePanel.OnConfirmButtonEvent += OpenAppStore;
            updateNoticePanel.OnCancelButtonEvent += Application.Quit;

            UpdateCheck();
        }

        private void UpdateCheck()
        {
            var client = new Version(Application.version);
            CustomLog.Log($"clientVersion : {client}");

// #if UNITY_EDITOR
//             Debug.Log("에디터 모드에서는 버전정보 조회할 수 없습니다.");
//             return;
// #endif
            Backend.Utils.GetLatestVersion(callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    Debug.LogError($"버전 정보 조회 실패하였습니다. \n {callback}");
                    return;
                }

                var version = callback.GetReturnValuetoJSON()["version"].ToString();
                var server = new Version(version);

                var result = server.CompareTo(client);
                if (result == 0)
                {
                    // 0 이면 두 버전이 일치하는 것 입니다.
                    // 아무 작업 안하고 리턴
                    return;
                }

                if (result < 0)
                {
                    // 0 미만인 경우 server 버전이 client 보다 작은 경우 입니다.
                    // 애플/구글 스토어에 검수를 넣었을 경우 여기에 해당 할 수 있습니다.
                    // ex)
                    // 검수를 신청한 클라이언트 버전은 3.0.0, 
                    // 라이브에 운용중인 클라이언트 버전은 2.0.0,
                    // 뒤끝 콘솔에 등록한 버전은 2.0.0 

                    // 아무 작업을 안하고 리턴
                    return;
                }
                // 0보다 크면 server 버전이 클라이언트 이후 버전일 수 있습니다.

                if (client == null)
                {
                    // 단 클라이언트 버전 정보가 null인 경우에도 0보다 큰 값이 리턴될 수 있습니다.
                    // 이 때는 아무 작업을 안하고 리턴하도록 하겠습니다.
                    Debug.LogError("클라이언트 버전 정보가 null 입니다.");
                    return;
                }

                // 여기까지 리턴 없이 왔으면 server 버전(뒤끝 콘솔에 등록한 버전)이 
                // 클라이언트보다 높은 경우 입니다.

                // 유저가 스토어에서 업데이트를 하도록 업데이트 UI를 띄워줍니다.
                OpenUpdateUI();
            });
        }

        private void OpenUpdateUI()
        {
            updateNoticePanel.OpenPopUp();
        }

        private void OpenAppStore()
        {
#if UNITY_IPHONE
            Application.OpenURL(appstoreURL);
#elif UNITY_ANDROID
            Application.OpenURL(playStoreURL);
#else
            Debug.LogError("지원하지 않는 플랫폼 입니다.");
#endif
        }
    }
}