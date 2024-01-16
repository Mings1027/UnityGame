using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VersionCheckController : MonoBehaviour
{
    private const string BundleID = "com.DefenseCompany.RogueDefense";
    private float prevVersion, curVersion;

    [SerializeField] private GameObject updateVersionPanel;
    [SerializeField] private Button appStoreButton;
    [SerializeField] private GameObject buttons;

    private void Start()
    {
        curVersion = float.Parse(Application.version);
        appStoreButton.onClick.AddListener(OpenAppStore);
        CheckVersion().Forget();
    }

    private async UniTaskVoid CheckVersion()
    {
        await CheckAppStoreVersion();

        if (prevVersion < 1.0f)
        {
            updateVersionPanel.SetActive(true);
            buttons.SetActive(false);
        }
        else
        {
            updateVersionPanel.SetActive(false);
        }
    }

    private async UniTask CheckAppStoreVersion()
    {
        try
        {
            var defaultUri = new UriBuilder("https://itunes.apple.com/lookup?bundleId=");
            defaultUri.Query += BundleID;
            var jsonText = await GetJsonText(defaultUri.Uri.ToString());
            var versionIndex = jsonText.IndexOf("version", StringComparison.Ordinal);
            var quoteIndex = jsonText.IndexOf(",", versionIndex + "version".Length, StringComparison.Ordinal);
            var versionValue = jsonText.Substring(versionIndex + "version".Length + 3,
                quoteIndex - versionIndex - "version".Length - 4);
            prevVersion = float.Parse(versionValue);
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
            else
            {
                return www.downloadHandler.text;
            }
        }
    }

    private void OpenAppStore()
    {
        Application.OpenURL("itms-apps://itunes.apple.com/app/id6472429843?uo=4");
        Application.Quit();
    }
}