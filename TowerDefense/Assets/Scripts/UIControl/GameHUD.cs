using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using ManagerControl;
using StatusControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class GameHUD : MonoSingleton<GameHUD>
    {
        private Sequence _hudSlideTween;
        private Sequence _cantMoveImageSequence;
        private Sequence _towerGoldTween;
        private Sequence _centerButtonSequence;
        private Tween _towerHealTween;
        private CancellationTokenSource _cts;
        private Transform _camTransform;

        private bool _isHUDVisible;
        private bool _enableCenterBtn;
        private int _towerGold;

        [SerializeField] private CanvasGroup gameHudGroup;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private ManaBar manaBar;
        [SerializeField] private Image cantMoveImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text goldText;
        [SerializeField, Range(0, 30)] private int playerHealth;
        [SerializeField, Range(0, 1000)] private int playerMana;
        [SerializeField] private CanvasGroup centerButtonGroup;
        [SerializeField] private TMP_Text waveText;

        private int towerGold
        {
            get => _towerGold;
            set
            {
                _towerGold = value;
                goldText.text = _towerGold.ToString();
                _towerGoldTween.Restart();
            }
        }

        public static TowerHealth towerHealth { get; private set; }
        public static Mana towerMana { get; private set; }

#region Unity Event

        private void OnDisable()
        {
            _hudSlideTween?.Kill();
            _cts.Cancel();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _cts.Cancel();
            _cts.Dispose();
        }

#endregion

#region Public Method

        public void Init()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var camManager = FindAnyObjectByType<CameraManager>();
            camManager.GameStartCamZoom();
            _camTransform = camManager.transform;
            var health = healthBar.GetComponent<TowerHealth>();
            health.Init(playerHealth);
            healthBar.Init(health);
            towerHealth = health;

            var mana = manaBar.GetComponent<Mana>();
            mana.Init(playerMana);
            manaBar.Init(mana);
            towerMana = mana;

            ButtonInit();
            TweenInit();
            CheckCamPos().Forget();
        }

        public static int GetTowerGold()
        {
            return instance.towerGold;
        }

        public static void CannotMove()
        {
            instance.CannotMovePrivate();
        }

        public static void TowerHeal()
        {
            instance.TowerHealPrivate();
        }

        public static void DisplayHUD()
        {
            instance.DisplayHUDPrivate();
        }

        public static void SetTowerGold(byte difficultyLevel)
        {
            var towerGold = instance.towerGold;
            towerGold = difficultyLevel switch
            {
                0 => 2000,
                1 => 4000,
                2 => 5000,
                3 => 6000,
                _ => towerGold
            };

            instance.towerGold = towerGold;
        }

        public static void IncreaseTowerGold(int gold)
        {
            instance.towerGold += gold;
        }

        public static void DecreaseTowerGold(int gold)
        {
            instance.towerGold -= gold;
        }

        public static void SetWaveText(string wave)
        {
            instance.waveText.text = wave;
        }

#endregion

#region Private Method

        private void CannotMovePrivate()
        {
            cantMoveImage.transform.position = Input.mousePosition;
            cantMoveImage.enabled = true;
            _cantMoveImageSequence.OnComplete(() => cantMoveImage.enabled = false).Restart();
        }

        private void ButtonInit()
        {
            closeButton.onClick.AddListener(() =>
            {
                gameHudGroup.blocksRaycasts = false;
                _hudSlideTween.PlayBackwards();
            });
            centerButtonGroup.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                centerButtonGroup.blocksRaycasts = false;
                _centerButtonSequence.OnRewind(() => { _enableCenterBtn = false; }).PlayBackwards();
                _camTransform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);
            });
        }

        private void TweenInit()
        {
            var rect = gameHudGroup.GetComponent<RectTransform>();
            _hudSlideTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(gameHudGroup.DOFade(1, 0.25f).From(0))
                .Join(rect.DOAnchorPosY(0, 0.25f).From(new Vector2(0, 100)));
            _hudSlideTween.OnRewind(() => { _isHUDVisible = false; });
            _hudSlideTween.OnComplete(() =>
            {
                _isHUDVisible = true;
                gameHudGroup.blocksRaycasts = true;
            });

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));

            _towerGoldTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(goldText.DOScale(1.2f, 0.125f))
                .Append(goldText.DOScale(1, 0.125f));

            var centerBtnRect = centerButtonGroup.GetComponent<RectTransform>();
            _centerButtonSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(centerBtnRect.DOAnchorPosX(150, 0.25f).From())
                .Join(centerButtonGroup.DOFade(1, 0.25f).From(0));

            var towerHealthRect = towerHealth.GetComponent<RectTransform>();
            _towerHealTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(towerHealthRect.DOScale(1.2f, 0.125f))
                .Append(towerHealthRect.DOScale(1, 0.125f));
        }

        private async UniTaskVoid CheckCamPos()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(2000, cancellationToken: _cts.Token);

                if (Vector3.Distance(_camTransform.position, Vector3.zero) > 10)
                {
                    if (_enableCenterBtn) continue;
                    _centerButtonSequence.OnComplete(() =>
                    {
                        _enableCenterBtn = true;
                        centerButtonGroup.blocksRaycasts = true;
                    }).Restart();
                }
            }
        }

        private void TowerHealPrivate()
        {
            _towerHealTween.Restart();
            towerHealth.Heal(5);
        }

        private void DisplayHUDPrivate()
        {
            if (_isHUDVisible) return;
            _hudSlideTween.Restart();
        }

#endregion
    }
}