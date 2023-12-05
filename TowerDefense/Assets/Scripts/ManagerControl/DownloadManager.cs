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
        private Dictionary<string, long> _patchMap;

        [Header("UI")] [SerializeField] private Button startGameButton;
        [SerializeField] private GameObject downLoadPanel;
        [SerializeField] private TMP_Text sizeInfoText;
        [SerializeField] private Button downLoadButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text downValueText;
        [SerializeField] private Slider downSlider;

        [Header("Label")] [SerializeField] private AssetLabelReference[] assetLabels;

        private void Awake()
        {
            _patchMap = new Dictionary<string, long>();
            startGameButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                StartGameButton();
            });
            downLoadButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                DownLoadButton();
            });
            cancelButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                startGameButton.gameObject.SetActive(true);
                downLoadPanel.transform.DOScale(0, 0.25f).SetEase(Ease.InBack);
            });
        }

        private void Start()
        {
            Time.timeScale = 1;
            downLoadPanel.SetActive(false);
            downSlider.gameObject.SetActive(false);
            InitAddressable().Forget();
            SoundManager.Instance.PlayBGM(SoundEnum.GameStart);
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

            if (_patchSize > decimal.Zero) // 다운로드 할 것이 있음
            {
                downLoadPanel.SetActive(true);
                await downLoadPanel.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
                sizeInfoText.text = GetFilSize(_patchSize);
            }
            else // 다운로드 할 것이 없음
            {
                sizeInfoText.enabled = false;
                downValueText.text = " 100 % ";
                downSlider.value = 1;
                await UniTask.Delay(500);
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
            downLoadPanel.transform.DOScale(0, 0.25f).SetEase(Ease.InBack);
            downLoadButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            downSlider.gameObject.SetActive(true);
            PatchFiles().Forget();
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
            DataManager.Init();
        }
    }
}