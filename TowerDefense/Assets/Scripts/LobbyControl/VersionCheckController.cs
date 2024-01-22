using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LobbyControl
{
    public class VersionCheckController : MonoBehaviour
    {
        // private const string BundleID = "com.DefenseCompany.RogueDefense";
        private int _appStoreVersion; //현재 스토어에 올라가있는 버전
        private int _curAppVersion;
        [SerializeField] private GameObject updateVersionPanel;
        [SerializeField] private Button appStoreButton;

        private void Start()
        {
            updateVersionPanel.SetActive(false);
            _curAppVersion = int.Parse(Application.version.Replace(".", ""));
            appStoreButton.onClick.AddListener(OpenAppStore);
            CheckVersion().Forget();
        }

        private async UniTaskVoid CheckVersion()
        {
            await CheckAppStoreVersion();
            updateVersionPanel.SetActive(_curAppVersion < _appStoreVersion);
        }

        private async UniTask CheckAppStoreVersion()
        {
            try
            {
                var defaultUri = new UriBuilder("https://mings1027.github.io/appVersion.html");
                var jsonText = await GetJsonText(defaultUri.Uri.ToString());
                _appStoreVersion = int.Parse(jsonText.Replace(".", ""));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async UniTask<string> GetJsonText(string url)
        {
            using var www = UnityWebRequest.Get(url);
            {
                await www.SendWebRequest();
                if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                {
                    throw new Exception(www.error);
                }

                return www.downloadHandler.text;
            }
        }

        private void OpenAppStore()
        {
            Application.OpenURL("itms-apps://itunes.apple.com/app/id6472429843?uo=4");
            Application.Quit();
        }
    }
}