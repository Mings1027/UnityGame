using System;
using System.Collections.Generic;
using System.Linq;
using CustomEnumControl;
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
        private Tween _downloadPanelTween;
        private Dictionary<string, long> _patchMap;

        [SerializeField] private GameObject downLoadPanel;
        [SerializeField] private Image blockImage;
        [SerializeField] private TMP_Text sizeInfoText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button downLoadButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text downValueText;
        [SerializeField] private Slider downSlider;

        [Header("Label")] [SerializeField] private AssetLabelReference[] assetLabels;

        private void Awake()
        {
            _downloadPanelTween = downLoadPanel.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
            _patchMap = new Dictionary<string, long>();
            startButton.gameObject.SetActive(false);
            startButton.onClick.AddListener(() =>
            {
                _downloadPanelTween.Restart();
                blockImage.enabled = true;
                startButton.gameObject.SetActive(false);
                downLoadButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
            });
            downLoadButton.onClick.AddListener(DownLoadButton);
            cancelButton.onClick.AddListener(CancelButton);
        }

        private void Start()
        {
            Time.timeScale = 1;
            blockImage.enabled = false;
            downSlider.gameObject.SetActive(false);
            InitAddressable().Forget();
        }

        private void OnDestroy()
        {
            _downloadPanelTween?.Kill();
        }

        private async UniTaskVoid InitAddressable()
        {
            await Addressables.InitializeAsync();
        }

#region CheckDown

        public async UniTaskVoid CheckUpdateFiles()
        {
            var labels = new List<string>();
            for (var i = 0; i < assetLabels.Length; i++)
            {
                labels.Add(assetLabels[i].labelString);
            }

            _patchSize = default;

            foreach (var handle in labels.Select(Addressables.GetDownloadSizeAsync))
            {
                await handle;

                _patchSize += handle.Result;
            }

            if (_patchSize > decimal.Zero) // 다운로드 할 것이 있음
            {
                blockImage.enabled = true;
                _downloadPanelTween.Restart();
                sizeInfoText.text = GetFilSize(_patchSize);
            }
            else // 다운로드 할 것이 없음
            {
                // sizeInfoText.enabled = false;
                // downValueText.text = " 100 % ";
                // downSlider.value = 1;
                downSlider.gameObject.SetActive(true);
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
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _downloadPanelTween.PlayBackwards();
            blockImage.enabled = false;
            downLoadButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            downSlider.gameObject.SetActive(true);
            PatchFiles().Forget();
        }

        private void CancelButton()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = false;
            startButton.gameObject.SetActive(true);
            downLoadButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            _downloadPanelTween.PlayBackwards();
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