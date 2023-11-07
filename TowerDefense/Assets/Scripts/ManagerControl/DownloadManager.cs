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
        private long _patchSize;
        private Dictionary<string, long> _patchMap;

        [Header("UI")] [SerializeField] private Button startGameButton;
        [SerializeField] private Button downLoadButton;
        [SerializeField] private GameObject downLoadPanel;
        [SerializeField] private Slider downSlider;
        [SerializeField] private TMP_Text downLoadText;
        [SerializeField] private TMP_Text sizeInfoText;

        [Header("Label")] [SerializeField] private AssetLabelReference[] assetLabels;

        private void Awake()
        {
            _patchMap = new Dictionary<string, long>();
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
            var labels = new List<string>();
            for (int i = 0; i < assetLabels.Length; i++)
            {
                labels.Add(assetLabels[i].labelString);
            }

            _patchSize = default;

            foreach (var handle in labels.Select(Addressables.GetDownloadSizeAsync))
            {
                await handle;

                _patchSize += handle.Result;
            }

            downLoadPanel.SetActive(true);
            startGameButton.gameObject.SetActive(false);

            if (_patchSize > decimal.Zero) // 다운로드 할 것이 있음
            {
                sizeInfoText.text = GetFilSize(_patchSize);
            }
            else // 다운로드 할 것이 없음
            {
                downLoadButton.gameObject.SetActive(false);

                sizeInfoText.enabled = false;
                downLoadText.DOCounter(0, 100, 0.5f);
                await downSlider.DOValue(1, 0.5f).From(0);
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
            var labels = new List<string>();
            for (int i = 0; i < assetLabels.Length; i++)
            {
                labels.Add(assetLabels[i].labelString);
            }

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
            _patchMap.Add(label, 0);
            var handle = Addressables.DownloadDependenciesAsync(label);

            while (!handle.IsDone)
            {
                _patchMap[label] = handle.GetDownloadStatus().DownloadedBytes;
                await UniTask.Yield();
            }

            _patchMap[label] = handle.GetDownloadStatus().TotalBytes;
            Addressables.Release(handle);
        }

        private async UniTask CheckDownLoad()
        {
            var total = 0f;
            downLoadText.text = "0 %";

            while (true)
            {
                total += _patchMap.Sum(tmp => tmp.Value);
                downSlider.value = total / _patchSize;
                downLoadText.text = (int)(downSlider.value * 100) + " %";

                if (total == _patchSize)
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