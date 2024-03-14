using System.Threading;
using AdsControl;
using BackEnd;
using BackendControl;
using CurrencyControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class LobbyUI : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        [SerializeField] private Button startGameButton;

        [SerializeField] private CanvasGroup buttonsGroup;
        [SerializeField] private CanvasGroup inGameMoneyGroup;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image backgroundBlockImage;

        [field: SerializeField] public DiamondCurrency diamondCurrency { get; private set; }
        [field: SerializeField] public EmeraldCurrency emeraldCurrency { get; private set; }

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Time.timeScale = 1;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void Start()
        {
            Init();
            CheckAccessToken().Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void Init()
        {
            FindAnyObjectByType<AdmobManager>().OnRewardedEvent += () => { RewardedAdRewardAsync().Forget(); };

            backgroundBlockImage.enabled = false;
            backgroundImage.enabled = false;
            inGameMoneyGroup.alpha = 0;
            startGameButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                FadeController.FadeOutAndLoadScene("MainGameScene");
            });

            OffBackgroundImage();
            OffBlockImage();
        }

        private async UniTaskVoid RewardedAdRewardAsync()
        {
            await UniTask.Delay(1000);
            BackendGameData.userData.emerald += 50;
            BackendGameData.instance.GameDataUpdate();
            emeraldCurrency.SetText();
        }

        private async UniTaskVoid CheckAccessToken()
        {
            var isAlive = true;
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(3000, cancellationToken: _cts.Token);
                await UniTask.RunOnThreadPool(async () =>
                {
                    var bro = Backend.BMember.IsAccessTokenAlive();
                    if (bro.IsSuccess()) return;
                    await UniTask.SwitchToMainThread();
                    isAlive = false;
                    FullscreenAlert.NonCancelableAlert(FullscreenAlertEnum.DuplicateAccessAlert, () =>
                    {
                        BackendLogin.instance.LogOut();
                        FadeController.FadeOutAndLoadScene("LoginScene");
                    });
                }, cancellationToken: _cts.Token);
                if (!isAlive) break;
            }
        }

        public void SetActiveButtons(bool active, bool inGameMoneyActive)
        {
            buttonsGroup.alpha = active ? 1 : 0;
            buttonsGroup.blocksRaycasts = active;
            inGameMoneyGroup.alpha = inGameMoneyActive ? 1 : 0;
        }

        public void On()
        {
            diamondCurrency.On();
            emeraldCurrency.On();
        }

        public void Off()
        {
            diamondCurrency.Off();
            emeraldCurrency.Off();
        }

        public void OnBackgroundImage()
        {
            backgroundImage.enabled = true;
            backgroundBlockImage.raycastTarget = true;
        }

        public void OffBackgroundImage() => backgroundImage.enabled = false;

        public void OffBlockImage() => backgroundBlockImage.raycastTarget = false;
    }
}