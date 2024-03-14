using System.Threading;
using BackEnd;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class PauseController : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private WaveManager _waveManager;
        private ItemBagController _itemBagController;

        private CanvasGroup _pauseCanvasGroup;

        private Sequence _pauseSequence;
        private Sequence _gameEndSequence;
        private Sequence _gameOverSequence;

        private Image _normalSpeedImage;
        private Image _doubleSpeedImage;
        private Image _tripleSpeedImage;

        private int _curTimeScale;

        [SerializeField] private CanvasGroup gameOverGroup;
        [SerializeField] private CanvasGroup gameEndGroup;

        [SerializeField] private RectTransform pausePanel;

        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button exitBattleButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button gameOverButton;
        [SerializeField] private Button gameEndButton;

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void Start()
        {
            Init();
            TweenInit();
            ButtonInit();
            CheckAccessToken().Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Application.targetFrameRate = 60;
            }
            else
            {
                UpdateSurvivedWave();
                _itemBagController.UpdateInventory();
                BackendGameData.instance.GameDataUpdate();
                Pause();
            }
        }

        private void OnApplicationQuit()
        {
            BackendLogin.instance.LogOut();
        }

#region Init

        private void Init()
        {
            _curTimeScale = 1;
            _waveManager = FindAnyObjectByType<WaveManager>();
            _itemBagController = FindAnyObjectByType<ItemBagController>();

            _pauseCanvasGroup = GetComponent<CanvasGroup>();
            _pauseCanvasGroup.blocksRaycasts = false;

            gameEndGroup.blocksRaycasts = false;
            gameOverGroup.blocksRaycasts = false;

            _normalSpeedImage = speedButton.transform.GetChild(0).GetComponent<Image>();
            _doubleSpeedImage = speedButton.transform.GetChild(1).GetComponent<Image>();
            _tripleSpeedImage = speedButton.transform.GetChild(2).GetComponent<Image>();

            _doubleSpeedImage.enabled = false;
            _tripleSpeedImage.enabled = false;
        }

        private void TweenInit()
        {
            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(_pauseCanvasGroup.DOFade(1, 0.25f).From(0))
                .Join(pausePanel.DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _gameEndSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(gameEndGroup.DOFade(1, 0.25f).From(0))
                .Join(gameEndGroup.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(0, 0.25f)
                    .From(new Vector2(0, -100)));
            _gameOverSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(gameOverGroup.DOFade(1, 0.25f).From(0))
                .Join(gameOverGroup.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(0, 0.25f)
                    .From(new Vector2(0, -100)));
        }

        private void ButtonInit()
        {
            pauseButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                Pause();
            });
            speedButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                SpeedUp();
            });

            exitBattleButton.onClick.AddListener(() =>
            {
                FullscreenAlert.NonCancelableAlert(FullscreenAlertEnum.ExitBattleAlert, ExitBattle);
            });
            restartButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                FullscreenAlert.NonCancelableAlert(FullscreenAlertEnum.RestartBattleAlert, Restart);
            });
            continueButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                Continue();
            });

            gameOverButton.onClick.AddListener(() =>
            {
                UpdateSurvivedWave();
                _itemBagController.UpdateInventory();
                BackendGameData.instance.GameDataUpdate();
                FadeController.FadeOutAndLoadScene("Lobby");
            });
            gameEndButton.onClick.AddListener(() =>
            {
                UpdateSurvivedWave();
                _itemBagController.UpdateInventory();
                BackendGameData.instance.GameDataUpdate();
                FadeController.FadeOutAndLoadScene("Lobby");
            });
        }

#endregion

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

        private void Pause()
        {
            Input.multiTouchEnabled = false;
            Time.timeScale = 0;
            _pauseCanvasGroup.blocksRaycasts = true;
            _pauseSequence.Restart();
            SoundManager.FadeOutVolume(SoundManager.BGMKey).Forget();
        }

        private void ExitBattle()
        {
            UpdateSurvivedWave();
            _itemBagController.UpdateInventory();
            BackendGameData.instance.GameDataUpdate();
            FadeController.FadeOutAndLoadScene("Lobby");
        }

        private void Restart()
        {
            FadeController.FadeOutAndLoadScene("ReStartScene");
            BackendGameData.isRestart = true;
        }

        private void Continue()
        {
            Input.multiTouchEnabled = true;
            Time.timeScale = _curTimeScale;
            _pauseSequence.OnRewind(() => _pauseCanvasGroup.blocksRaycasts = false).PlayBackwards();
            SoundManager.FadeInVolume(SoundManager.BGMKey).Forget();
        }

        private void SpeedUp()
        {
            _curTimeScale = (byte)(_curTimeScale % 3 + 1);
            Time.timeScale = _curTimeScale;
            _normalSpeedImage.enabled = _curTimeScale == 1;
            _doubleSpeedImage.enabled = _curTimeScale == 2;
            _tripleSpeedImage.enabled = _curTimeScale == 3;
        }

        private void UpdateSurvivedWave()
        {
            var curWave = WaveManager.curWave;
            if (curWave >= 1)
            {
                BackendGameData.instance.UpdateSurvivedWave((byte)(_waveManager.isStartWave
                    ? curWave - 1
                    : curWave));
            }
        }

        public void GameOver()
        {
            _gameOverSequence.OnComplete(() => gameOverGroup.blocksRaycasts = true).Restart();
            Time.timeScale = 0;
        }

        public void GameEnd()
        {
            _gameEndSequence.OnComplete(() => gameEndGroup.blocksRaycasts = true).Restart();
            Time.timeScale = 0;
        }
    }
}