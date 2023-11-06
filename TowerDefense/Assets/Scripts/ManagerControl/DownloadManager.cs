using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public class DownloadManager : MonoBehaviour
    {
        private long patchSize;
        private Dictionary<string, long> patchMap;

        [Header("UI")] [SerializeField] private Button startGameButton;
        [SerializeField] private Button downLoadButton;
        [SerializeField] private GameObject downLoadPanel;
        [SerializeField] private Slider downSlider;
        [SerializeField] private TMP_Text sizeInfoText;

        [Header("Label")] [SerializeField] private AssetLabelReference managerPrefabLabel;
        [SerializeField] private AssetLabelReference materialLabel;

        private void Awake()
        {
            patchMap = new Dictionary<string, long>();
            startGameButton.onClick.AddListener(StartGameButton);
            downLoadButton.onClick.AddListener(DownLoadButton);
        }

        private void Start()
        {
            Time.timeScale = 1;
            downLoadPanel.SetActive(false);
            InitAddressable().Forget();
            LocaleManager.ChangeLocale(0);
        }

        private async UniTaskVoid InitAddressable()
        {
            await Addressables.InitializeAsync();
        }

        private void StartGameButton()
        {
            startGameButton.gameObject.SetActive(false);
            CheckUpdateFiles().Forget();
        }

        #region CheckDown

        private async UniTaskVoid CheckUpdateFiles()
        {
            var labels = new List<string> { managerPrefabLabel.labelString, materialLabel.labelString };
            patchSize = default;

            foreach (var handle in labels.Select(Addressables.GetDownloadSizeAsync))
            {
                await handle;

                patchSize += handle.Result;
            }

            downLoadPanel.SetActive(true);
            startGameButton.gameObject.SetActive(false);

            if (patchSize > decimal.Zero) // 다운로드 할 것이 있음
            {
                sizeInfoText.text = GetFilSize(patchSize);
            }
            else // 다운로드 할 것이 없음
            {
                downLoadButton.gameObject.SetActive(false);

                sizeInfoText.text = "Building Tower...";
                await downSlider.DOValue(1, 2).From(0);
                StartGame();
                // 게임 시작되는 부분
            }
        }

        private string GetFilSize(long byteCount)
        {
            var size = "0 Bytes";

            if (byteCount >= 1073741824.0)
            {
                size = $"{byteCount / 1073741824.0:##.##}" + " GB";
            }
            else if (byteCount >= 1048576.0)
            {
                size = $"{byteCount / 1048576.0:##.##}" + " MB";
            }
            else if (byteCount >= 1024.0)
            {
                size = $"{byteCount / 1024.0:##.##}" + " KB";
            }
            else if (byteCount > 0 && byteCount < 1024.0)
            {
                size = byteCount + " Bytes";
            }

            return size;
        }

        #endregion

        #region DownLoad

        private void DownLoadButton()
        {
            downLoadButton.gameObject.SetActive(false);

            PatchFiles().Forget();
        }

        private async UniTask PatchFiles()
        {
            var labels = new List<string> { managerPrefabLabel.labelString, materialLabel.labelString };

            foreach (var label in labels)
            {
                var handle = Addressables.GetDownloadSizeAsync(label);

                await handle;

                if (handle.Result != decimal.Zero)
                {
                    DownLoadLabel(label).Forget();
                }
            }

            await CheckDownLoad();
        }

        private async UniTaskVoid DownLoadLabel(string label)
        {
            patchMap.Add(label, 0);
            var handle = Addressables.DownloadDependenciesAsync(label);

            while (!handle.IsDone)
            {
                patchMap[label] = handle.GetDownloadStatus().DownloadedBytes;
                await UniTask.Yield();
            }

            patchMap[label] = handle.GetDownloadStatus().TotalBytes;
            Addressables.Release(handle);
        }

        private async UniTask CheckDownLoad()
        {
            var total = 0f;

            while (true)
            {
                total += patchMap.Sum(tmp => tmp.Value);
                downSlider.value = total / patchSize;

                if (total == patchSize)
                {
                    StartGame();
                    break;
                }

                total = 0f;
                await UniTask.Yield();
            }
        }

        #endregion

        private void StartGame()
        {
            SceneManager.LoadScene("MainGameScene");
        }
    }
}