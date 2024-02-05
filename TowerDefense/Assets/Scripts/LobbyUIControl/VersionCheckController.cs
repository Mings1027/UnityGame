using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class VersionCheckController : MonoBehaviour
    {
        // private const string BundleID = "com.DefenseCompany.RogueDefense";
        private int _storeVersion; //현재 스토어에 올라가있는 버전
        private int _curAppVersion;

        [SerializeField] private string appstoreURL = "itms-apps://itunes.apple.com/app/id6472429843?uo=4";
        [SerializeField] private string playStoreURL;

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
            updateVersionPanel.SetActive(_curAppVersion < _storeVersion);
        }

        private async UniTask CheckAppStoreVersion()
        {
            try
            {
                var defaultUri = new UriBuilder("https://mings1027.github.io/appVersion.html");
                var jsonText = await GetJsonText(defaultUri.Uri.ToString());
                _storeVersion = int.Parse(jsonText.Replace(".", ""));
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
#if UNITY_IPHONE
            Application.OpenURL(appstoreURL);
#elif UNITY_ANDROID
            Application.OpenURL(playStoreURL);
#endif
            Application.Quit();
        }
    }
}