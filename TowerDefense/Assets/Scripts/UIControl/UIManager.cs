using System;
using System.Collections.Generic;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using PoolObjectControl;
using StatusControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : Singleton<UIManager>, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler
    {
        private CameraManager _cameraManager;
        private Camera _cam;
        private Dictionary<TowerType, TowerData> _towerDataDictionary;
        private Dictionary<TowerType, GameObject> _towerObjDictionary;
        private Dictionary<TowerType, int> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerCostTextDictionary;

        private Sequence _onOffTowerBtnSequence;
        private Tween _towerInfoPanelTween;
        private Sequence _pauseSequence;
        private Sequence _changeLanguageSequence;
        private Sequence _notEnoughCostSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;

        private Tower _curSelectedTower;
        private UnitTower _curUnitTower;

        private Image _toggleTowerBtnImage;

        private TMP_Text[] _damageTextList;
        private TMP_Text[] _towerCostTexts;

        private int _sellTowerCost;
        private int _towerCost;
        private byte _curTimeScale;
        private byte _prevSize;
        private bool _isShowTowerBtn;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;
        private bool _isOnTowerButton;
        private bool _isPanelOpen;

        private Vector3 _prevPos;

        public TowerCardUI towerCardUI { get; private set; }

        public int TowerCost
        {
            get => _towerCost;
            set
            {
                _towerCost = value;
                costText.text = _towerCost.ToString();
            }
        }

        public bool IsOnUI { get; private set; }
        public TextMeshProUGUI WaveText => waveText;
        public Health BaseTowerHealth { get; private set; }

        [SerializeField] private EventSystem eventSystem;
        [SerializeField, Range(0, 30)] private byte lifeCount;
        [SerializeField] private int startCost;

        [Header("----------Tween------------"), SerializeField]
        private Transform notEnoughCostPanel;

        [SerializeField] private Transform towerPanel;
        [SerializeField] private Image cantMoveImage;
        [SerializeField] private Sprite physicalSprite;
        [SerializeField] private Sprite magicSprite;

        [Header("----------Tower Panel----------"), SerializeField]
        private TowerData[] towerDataList;

        [SerializeField] private TowerInfoUI towerInfoUI;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellTowerButton;

        [Header("----------Tower Buttons----------")] [SerializeField]
        private Image toggleTowerButton;

        [Header("----------For Unit Tower----------"), SerializeField]
        private Image checkUnitMoveButton;

        [Header("----------Game Over-------------"), SerializeField]
        private GameObject gameOverPanel;

        [SerializeField] private Transform languagePanel;
        [SerializeField] private Transform languageButtons;
        [SerializeField] private Transform pausePanel;

        [SerializeField] private Button openLanguagePanelButton;
        [SerializeField] private Button closeLanguagePanelButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button speedUpButton;

        [SerializeField] private Toggle bgmToggle;
        [SerializeField] private Toggle sfxToggle;

        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI speedUpText;

        [SerializeField, Range(0, 1)] private float camZoomTime;

        #region Unity Event

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsOnUI = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsOnUI = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsOnUI = false;
        }

        protected override void Awake()
        {
            base.Awake();
            eventSystem.enabled = false;
            towerCardUI = GetComponentInChildren<TowerCardUI>();
            _cameraManager = FindObjectOfType<CameraManager>();
            BaseTowerHealth = GetComponent<Health>();
            _cam = Camera.main;
            TowerButtonInit();
            TowerInit();
            TweenInit();
            MenuButtonInit();
        }

        protected override void Start()
        {
            base.Start();
            gameOverPanel.SetActive(false);
            BaseTowerHealth.Init(lifeCount);
            BaseTowerHealth.OnDeadEvent += GameOver;
            GameStart().Forget();
        }

        private void OnDisable()
        {
            _towerInfoPanelTween?.Kill();
            _notEnoughCostSequence?.Kill();
            _pauseSequence?.Kill();
            _changeLanguageSequence?.Kill();
            _onOffTowerBtnSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
        }

        #endregion

        private void TowerInit()
        {
            _towerDataDictionary = new Dictionary<TowerType, TowerData>();
            _towerObjDictionary = new Dictionary<TowerType, GameObject>();
            _towerCountDictionary = new Dictionary<TowerType, int>();
            _towerCostTextDictionary = new Dictionary<TowerType, TMP_Text>();

            foreach (var t in towerDataList)
            {
                _towerDataDictionary.Add(t.TowerType, t);
                _towerObjDictionary.Add(t.TowerType, t.Tower);
                _towerCountDictionary.Add(t.TowerType, 0);
            }

            for (var i = 0; i < towerDataList.Length; i++)
            {
                var towerType = towerDataList[i].TowerType;
                _towerCostTextDictionary.Add(towerType, _towerCostTexts[i]);
                _towerCostTextDictionary[towerType].text = _towerDataDictionary[towerType].TowerBuildCost + "g";
            }
        }

        private void TweenInit()
        {
            _toggleTowerBtnImage = toggleTowerButton.transform.GetChild(0).GetComponent<Image>();
            _onOffTowerBtnSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(towerPanel.DOLocalMoveX(250, 0.5f).SetRelative().SetEase(Ease.InOutBack))
                .Join(_toggleTowerBtnImage.transform.DORotate(new Vector3(0, 180, 0), 0.5f, RotateMode.LocalAxisAdd));

            _towerInfoPanelTween =
                towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();

            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(pausePanel.DOScale(1, 0.1f).From(0))
                .Append(pausePanel.DOLocalMoveY(0, 0.5f).From(Screen.height).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOLocalMoveY(200, 0.25f).SetRelative().SetEase(Ease.InOutBack))
                .Join(mainMenuButton.transform.DOScale(1, 0.1f).From(0)
                    .OnComplete(() => Application.targetFrameRate = 30))
                .PrependCallback(() => _cameraManager.enabled = !_cameraManager.enabled);

            _changeLanguageSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(languagePanel.DOScale(1, 0.1f).From(0))
                .Append(languagePanel.DOLocalMoveY(0, 0.5f).From(Screen.height).SetEase(Ease.OutBack));

            _notEnoughCostSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notEnoughCostPanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Append(notEnoughCostPanel.DOScale(0, 0.5f).SetDelay(0.3f).SetEase(Ease.InBounce));

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));
        }

        private void TowerButtonInit()
        {
            var towerButtonController = towerPanel.GetComponentInChildren<TowerButtonController>();
            towerButtonController.Init();
            for (var i = 0; i < towerDataList.Length; i++)
            {
                towerButtonController.SetDictionary(i, towerDataList[i]);
            }

            var towerButtons = towerButtonController.GetComponentsInChildren<TowerButton>();
            _towerCostTexts = new TMP_Text[towerButtons.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                _towerCostTexts[i] = towerButtons[i].GetComponentInChildren<TMP_Text>();
            }

            toggleTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                ToggleTowerButtons().Forget();
            });

            upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!_isPanelOpen) return;
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                TowerUpgrade();
                UpdateTowerInfo();
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                FocusUnitTower();
                MoveUnitButton();
            });
            checkUnitMoveButton.GetComponent<Button>().onClick.AddListener(CheckCanMoveUnit);
            checkUnitMoveButton.enabled = false;
            sellTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!_isPanelOpen) return;
                SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? SoundEnum.SellTower1 :
                    _curSelectedTower.TowerLevel < 4 ? SoundEnum.SellTower2 : SoundEnum.SellTower3);

                SellTower();
            });
        }

        private void MenuButtonInit()
        {
            _towerCost = startCost;
            _curTimeScale = 1;
            TowerCost = _towerCost;

            speedUpText.text = "x1";

            pauseButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                Time.timeScale = 0;
                _pauseSequence.Restart();
            });
            resumeButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                Time.timeScale = _curTimeScale;
                Application.targetFrameRate = 60;
                _pauseSequence.PlayBackwards();
            });
            bgmToggle.isOn = false;
            bgmToggle.onValueChanged.AddListener(delegate
            {
                BGMToggleImage();
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            });
            sfxToggle.isOn = false;
            sfxToggle.onValueChanged.AddListener(delegate
            {
                SfxToggleImage();
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            });

            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("Lobby"));
            openLanguagePanelButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _changeLanguageSequence.Restart();
            });
            for (var i = 0; i < languageButtons.childCount; i++)
            {
                var index = i;
                languageButtons.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                    LocaleManager.ChangeLocale(index);
                });
            }

            closeLanguagePanelButton.onClick.AddListener(_changeLanguageSequence.PlayBackwards);

            speedUpButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                SpeedUp();
            });
        }

        private async UniTaskVoid ToggleTowerButtons()
        {
            if (!_isShowTowerBtn)
            {
                OnToggleButton();

                _isShowTowerBtn = true;
                _onOffTowerBtnSequence.Restart();
            }
            else
            {
                _isShowTowerBtn = false;
                _onOffTowerBtnSequence.PlayBackwards();

                await UniTask.Delay(2000);
                if (!_isShowTowerBtn)
                {
                    await _toggleTowerBtnImage.DOFade(0, 1);

                    _isOnTowerButton = false;
                    toggleTowerButton.enabled = false;
                }
            }
        }

        public Sprite WhatTypeOfThisTower(TowerData t) => t.IsMagicTower ? magicSprite : physicalSprite;

        public void InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward,
            bool isUnitTower)
        {
            var t = Instantiate(_towerObjDictionary[towerType], placePos, Quaternion.identity).GetComponent<Tower>();
            var towerTransform = t.transform;
            towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
            towerTransform.GetChild(0).forward = towerForward;
            if (isUnitTower)
            {
                SetUnitPosition(t);
            }

            var towerData = _towerDataDictionary[towerType];
            BuildTower(t, placePos, towerData).Forget();
        }

        private async UniTaskVoid BuildTower(Tower t, Vector3 placePos, TowerData towerData)
        {
            var towerType = t.TowerData.TowerType;
#if UNITY_EDITOR
            // t.transform.SetParent(_towerManager.transform);
#endif
            _towerCountDictionary[towerType]++;

            t.TowerLevelUp();
            t.TowerInvestment = towerData.TowerBuildCost * _towerCountDictionary[towerType];
            TowerCost -= t.TowerInvestment;
            _towerCostTextDictionary[towerType].text =
                towerData.TowerBuildCost * (_towerCountDictionary[towerType] + 1) + "g";
            await DOTween.Sequence().Append(costText.transform.DOShakeScale(0.2f, 0.5f))
                .Join(costText.transform.DOShakeRotation(0.2f, 50, 10, 90, true, ShakeRandomnessMode.Harmonic))
                .Join(t.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(t.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                        _cam.transform.DOShakePosition(0.05f);
                    }))
                .WithCancellation(this.GetCancellationTokenOnDestroy());

            TowerSetting(t, towerData);
            TowerManager.Instance.AddTower(t);
        }

        private void SetUnitPosition(Component t)
        {
            NavMesh.SamplePosition(t.transform.position, out var hit, 5, NavMesh.AllAreas);
            t.TryGetComponent(out UnitTower u);
            u.unitSpawnPosition = hit.position;
        }

        private void TowerSetting(Tower t, TowerData towerData)
        {
            var towerInfoData = towerData.TowerLevels[t.TowerLevel];
            t.TowerSetting(towerInfoData.towerMesh, towerInfoData.damage, towerInfoData.attackRange,
                towerInfoData.attackDelay);
            upgradeButton.SetActive(!t.TowerLevel.Equals(4));

            t.OnClickTower += ClickTower;
        }

        private void TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            var towerType = tempTower.TowerData.TowerType;
            var towerData = _towerDataDictionary[towerType];

            if (_towerCost < towerData.TowerUpgradeCost * (_curSelectedTower.TowerLevel + 1))
            {
                _notEnoughCostSequence.Restart();
                return;
            }

            tempTower.TowerLevelUp();
            var towerLevel = tempTower.TowerLevel;
            var tt = towerData.TowerLevels[towerLevel];
            TowerCost -= _towerDataDictionary[towerType].TowerUpgradeCost * towerLevel;

            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, tempTower.transform.position);

            tempTower.TowerSetting(tt.towerMesh, tt.damage, tt.attackRange, tt.attackDelay);
            upgradeButton.SetActive(!towerLevel.Equals(4));

            TowerManager.Instance.SetIndicator(_curSelectedTower);
        }

        private void ClickTower(Tower clickedTower)
        {
            if (Input.touchCount != 1) return;
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            _towerInfoPanelTween.Restart();
            _isPanelOpen = true;
            if (_curSelectedTower) _curSelectedTower.DisableOutline();
            if (clickedTower.Equals(_curSelectedTower)) return;

            if (_curSelectedTower && _curSelectedTower.TowerData.IsUnitTower)
            {
                _curUnitTower.OffUnitIndicator();
            }

            _curSelectedTower = clickedTower;

            upgradeButton.SetActive(!clickedTower.TowerLevel.Equals(4));

            IfUnitTower();
            OpenEditButtonPanel();
            UpdateTowerInfo();
            TowerManager.Instance.SetIndicator(clickedTower);
        }

        private void IfUnitTower()
        {
            var isUnitTower = _curSelectedTower.TowerData.IsUnitTower;
            if (isUnitTower)
            {
                _curUnitTower = _curSelectedTower.GetComponent<UnitTower>();
            }

            moveUnitButton.SetActive(isUnitTower);
        }

        private void OpenEditButtonPanel()
        {
            towerInfoUI.enabled = true;
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);
        }

        private void UpdateTowerInfo()
        {
            var towerType = _curSelectedTower.TowerData.TowerType;
            var towerLevel = _curSelectedTower.TowerLevel;
            var curTowerData = _towerDataDictionary[towerType];

            _sellTowerCost = GetSellTowerCost(towerType);
            towerInfoUI.SetTowerInfo(curTowerData, curTowerData.IsUnitTower, towerLevel, _sellTowerCost);
        }

        private void OnToggleButton()
        {
            if (_isOnTowerButton) return;
            _toggleTowerBtnImage.DOFade(1, 0.2f).OnComplete(() =>
            {
                _isOnTowerButton = true;
                toggleTowerButton.enabled = true;
            });
        }

        public void UIOff()
        {
            OnToggleButton();
            OffUI();
        }

        private int GetSellTowerCost(TowerType towerType)
        {
            return _curSelectedTower.TowerInvestment +
                   _towerDataDictionary[towerType].TowerUpgradeCost * _curSelectedTower.TowerLevel;
        }

        private void FocusUnitTower()
        {
            _prevSize = (byte)_cam.orthographicSize;
            _prevPos = _cameraManager.transform.position;
            DOTween.Sequence()
                .Append(_cameraManager.transform.DOMove(_curSelectedTower.transform.position, camZoomTime))
                .Join(_cam.DOOrthoSize(10, camZoomTime));
        }

        private void RewindCamState()
        {
            DOTween.Sequence()
                .Append(_cameraManager.transform.DOMove(_prevPos, camZoomTime))
                .Join(_cam.DOOrthoSize(_prevSize, camZoomTime));
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            checkUnitMoveButton.enabled = true;
            _towerInfoPanelTween.PlayBackwards();
        }

        private void CheckCanMoveUnit()
        {
            var touch = Input.GetTouch(0);
            if (!touch.deltaPosition.Equals(Vector2.zero)) return;
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            if (hit.collider && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(_curUnitTower.transform.position, hit.point) <= _curUnitTower.TowerRange)
            {
                checkUnitMoveButton.enabled = false;
                TowerManager.Instance.StartMoveUnit(_curUnitTower, new Vector3(hit.point.x, 0, hit.point.z))
                    .Forget();
                RewindCamState();
                OffUI();
            }
            else
            {
                cantMoveImage.transform.position = Input.mousePosition;
                _cantMoveImageSequence.Restart();
            }
        }

        private void SellTower()
        {
            var towerData = _curSelectedTower.TowerData;
            _towerCountDictionary[towerData.TowerType]--;
            _towerCostTextDictionary[towerData.TowerType].text =
                towerData.TowerBuildCost * (_towerCountDictionary[towerData.TowerType] + 1) + "g";
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, _curSelectedTower.transform.position);

            TowerCost += _sellTowerCost;
            TowerManager.Instance.RemoveTower(_curSelectedTower);
            Destroy(_curSelectedTower.gameObject);
            OffUI();
        }

        public void OffUI()
        {
            if (!_isPanelOpen) return;
            _towerInfoPanelTween.PlayBackwards();

            _isPanelOpen = false;
            if (_curSelectedTower) _curSelectedTower.DisableOutline();
            _curSelectedTower = null;
            towerInfoUI.enabled = false;
            TowerManager.Instance.OffIndicator();

            if (!_curUnitTower) return;
            _curUnitTower.OffUnitIndicator();
            _curUnitTower = null;
        }

        public bool IsEnoughCost(TowerType towerType)
        {
            if (_towerCost >= _towerDataDictionary[towerType].TowerBuildCost * (_towerCountDictionary[towerType] + 1))
            {
                return true;
            }

            _notEnoughCostSequence.Restart();
            return false;
        }

        private void BGMToggleImage()
        {
            SoundManager.Instance.ToggleBGM(!bgmToggle.isOn);
        }

        private void SfxToggleImage()
        {
            SoundManager.Instance.ToggleSfx(!sfxToggle.isOn);
        }

        private async UniTaskVoid GameStart()
        {
            _cam.cullingMask = int.MaxValue;
            waveText.text = "0";
            Time.timeScale = 1;
            _cameraManager.enabled = false;
            await DOTween.Sequence()
                .Append(_cam.transform.DOLocalMove(new Vector3(0, 40, -40), 1.5f).SetEase(Ease.OutQuad))
                .Join(_cam.DOOrthoSize(17, 1.5f).From(100).SetEase(Ease.OutQuad))
                .Join(_cam.transform.DOLocalMove(new Vector3(0, 20, -20), 1.5f));
            SoundManager.Instance.PlayBGM(SoundEnum.WaveEnd);
            eventSystem.enabled = true;
            _cameraManager.enabled = true;
        }

        private void GameOver()
        {
            resumeButton.interactable = false;
            gameOverPanel.SetActive(true);
            _pauseSequence.Restart();
            _cameraManager.enabled = false;
            Time.timeScale = 0;
            DataManager.SaveDamageData();
        }

        private void SpeedUp()
        {
            _curTimeScale = (byte)(_curTimeScale % 3 + 1);
            Time.timeScale = _curTimeScale;
            speedUpText.text = $"x{_curTimeScale}";
        }
    }
}