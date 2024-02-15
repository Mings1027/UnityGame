using System;
using System.Collections.Generic;
using System.Linq;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UIControl;
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

        [SerializeField] private NoticePanel downLoadNoticePanel;
        [SerializeField] private TMP_Text sizeInfoText;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text downValueText;
        [SerializeField] private Slider downSlider;

        [Header("Label")] [SerializeField] private AssetLabelReference[] assetLabels;

        private void Awake()
        {
            _patchMap = new Dictionary<string, long>();
            startButton.onClick.AddListener(async () =>
            {
                await CheckUpdateFiles();
                UpdateFiles();
            });

            downLoadNoticePanel.OnConfirmButtonEvent += DownLoadButton;
            downLoadNoticePanel.OnCancelButtonEvent += CancelButton;
        }

        private void Start()
        {
            Time.timeScale = 1;
            downSlider.gameObject.SetActive(false);
            InitAddressable().Forget();
        }

        private async UniTaskVoid InitAddressable()
        {
            await Addressables.InitializeAsync();
        }

#region CheckDown

        private async UniTask CheckUpdateFiles()
        {
            var labels = new List<string>();
            for (var i = 0; i < assetLabels.Length; i++)
            {
                labels.Add(assetLabels[i].labelString);
            }

            _patchSize = default;

            foreach (var handle in labels.Select(Addressables.GetDownloadSizeAsync))
            {
                Debug.Log(handle);
                await handle;

                _patchSize += handle.Result;
            }
        }

        private void UpdateFiles()
        {
            if (_patchSize > decimal.Zero) // 다운로드 할 것이 있음
            {
                Debug.Log("다운로드다운로드다운로드다운로드다운로드");
                sizeInfoText.text = GetFilSize(_patchSize);
                startButton.gameObject.SetActive(false);
                downLoadNoticePanel.OpenPopUp();
            }
            else // 다운로드 할 것이 없음
            {
                Debug.Log("no download");
                downSlider.gameObject.SetActive(true);
                startButton.gameObject.SetActive(false);
                ProgressBarTo100();
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
            downSlider.gameObject.SetActive(true);
            PatchFiles().Forget();
        }

        private void CancelButton()
        {
            startButton.gameObject.SetActive(true);
        }

        private async UniTask PatchFiles()
        {
            var labels = new List<string>();
            for (var i = 0; i < assetLabels.Length; i++)
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
            downValueText.text = "0 %";

            while (true)
            {
                total += _patchMap.Sum(tmp => tmp.Value);
                downSlider.value = total / _patchSize;
                downValueText.text = (int)(downSlider.value * 100) + " %";

                if (Math.Abs(total - _patchSize) < 0.01f)
                {
                    SceneManager.LoadSceneAsync("Lobby");
                    break;
                }

                total = 0f;
                await UniTask.Yield();
            }
        }

#endregion

        private void ProgressBarTo100()
        {
            downValueText.text = "100 %";
            downSlider.value = 1;
            SceneManager.LoadSceneAsync("Lobby");
        }
    }
}