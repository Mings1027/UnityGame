using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using TMPro;
using TowerControl;
using UIControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public struct TowerInfo
    {
        public string towerName;
        public int level;
        public int damage;
        public int range;
        public float delay;
        public int sellGold;
    }

    public class TowerManager : Singleton<TowerManager>
    {
        private TowerInfo _towerInfo;
        private Camera _cam;

        private Dictionary<string, GameObject> _towerDictionary;

        private Sequence _onOffTowerBtnSequence;
        private Tween _towerInfoPanelTween;
        private Sequence _pauseSequence;
        private Tween _notEnoughGoldTween;

        //  Tower Buttons
        private Transform _toggleTowerBtnImage;
        private bool _isShowTowerBtn;

        //  Tower Panel
        private Tower _curSelectedTower;
        private UnitTower _curUnitTower;

        private bool _isThreeSecPassed;
        private bool _inputDetected;
        private bool _isPanelOpen;
        private bool _isTower;
        private bool _isUnitTower;

        private int _sellTowerGold;
        private float _prevSize;
        private Vector3 _prevPos;

        // Damage Data
        private TMP_Text[] _damageTexts;

        private float _curTowerLife;
        private int _towerGold;
        private int _curSpeed;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;

        private CameraManager _cameraManager;
        private string[] _towerName;

        // For Unit Move
        private Sequence _cantMoveImageSequence;

        [SerializeField] private GameObject[] towerArray;

        [Header("----------Tower Buttons----------")] [SerializeField]
        private Button toggleTowerButton;

        [SerializeField] private Transform towerButtons;
        [SerializeField] private PlaceTowerController placeTowerButtonController;

        [Header("----------Tower Panel----------"), SerializeField]
        private TowerData[] towerData;

        [SerializeField] private string[] towerTierName;

        [SerializeField] private TowerInfoUI towerInfoUI;

        [SerializeField] private Button checkTowerButton;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellTowerButton;

        [Header("------------Damage Data----------"), SerializeField]
        private GameObject damagePanel;

        [Header("------------HUD------------"), SerializeField]
        private GameObject hudPanel;

        [SerializeField] private int[] towerBuildGold;
        [SerializeField] private int startGold;

        [SerializeField] private Transform pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button bgmButton;

        [SerializeField] private Button gameEndButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Sprite musicOnImage;
        [SerializeField] private Sprite musicOffImage;

        [SerializeField] private GameObject notEnoughGoldPanel;
        [SerializeField, Range(0, 30)] private int lifeCount;
        [SerializeField] private RectTransform playerHealthBar;
        [SerializeField] private Image lifeFillImage;
        [SerializeField] private TextMeshProUGUI lifeCountText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI speedUpText;

        [Header("----------Game Over----------"), SerializeField, Space(10)]
        private GameObject gameOverPanel;

        [SerializeField] private Button reStartButton;

        [Header("----------Indicator----------"), SerializeField]
        private ParticleSystem towerRangeParticle;

        [SerializeField] private ParticleSystem selectedTowerParticle;
        [SerializeField] private ParticleSystem unitDestinationParticle;

        [Header("----------For Unit Tower----------"), SerializeField]
        private GameObject checkUnitMoveButton;

        [SerializeField] private Image cantMoveImage;

        public bool IsPause { get; private set; }
        public TextMeshProUGUI WaveText => waveText;
        public PlaceTowerController PlaceTowerController => placeTowerButtonController;

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/
        private void Awake()
        {
            _cam = Camera.main;
            _cameraManager = FindObjectOfType<CameraManager>();
            lifeFillImage.fillAmount = 1;
            _curTowerLife = lifeCount;
            lifeCountText.text = _curTowerLife + " / " + lifeCount;
            TowerInit();
            TweenInit();
            TowerButtonInit();
            TowerEditButtonInit();
            MenuButtonInit();
            DamageTextInit();
            GameOverPanelInit();
        }

        private void Start()
        {
            hudPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            checkTowerButton.gameObject.SetActive(true);
            IndicatorInit();
        }

        private void OnDestroy()
        {
            _towerInfoPanelTween.Kill();
            _notEnoughGoldTween.Kill();
            _pauseSequence.Kill();
            _onOffTowerBtnSequence.Kill();
            _cantMoveImageSequence.Kill();
        }

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/

        #region Init

        private void TowerInit()
        {
            _towerName = new string[towerArray.Length];
            for (int i = 0; i < _towerName.Length; i++)
            {
                _towerName[i] = towerArray[i].name;
            }

            _towerDictionary = new Dictionary<string, GameObject>();
            for (int i = 0; i < towerArray.Length; i++)
            {
                _towerDictionary.Add(_towerName[i], towerArray[i]);
            }
        }

        private void TweenInit()
        {
            _toggleTowerBtnImage = toggleTowerButton.transform.GetChild(0);
            _onOffTowerBtnSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(toggleTowerButton.transform.DOLocalMoveX(250, 0.5f).SetRelative().SetEase(Ease.InOutBack))
                .Join(_toggleTowerBtnImage.DORotate(new Vector3(0, 180, 0), 0.5f, RotateMode.LocalAxisAdd))
                .Join(towerButtons.DOLocalMoveX(0, 0.5f).From(-250).SetEase(Ease.InOutBack));

            _towerInfoPanelTween =
                towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetEase(Ease.OutBack).SetAutoKill(false);

            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(pausePanel.DOLocalMoveY(0, 0.5f).From(Screen.height).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOLocalMoveY(200, 0.5f).SetRelative().SetEase(Ease.InOutBack))
                .PrependCallback(() => _cameraManager.enabled = !_cameraManager.enabled);

            _notEnoughGoldTween = notEnoughGoldPanel.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce)
                .SetAutoKill(false).Pause();

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));
        }

        private void TowerButtonInit()
        {
            toggleTowerButton.onClick.AddListener(ToggleTowerButtons);
        }

        private void TowerEditButtonInit()
        {
            upgradeButton.TryGetComponent(out Button upgradeBtn);
            upgradeBtn.onClick.AddListener(() =>
            {
                TowerUpgrade();
                UpdateTowerInfo();
            });
            moveUnitButton.TryGetComponent(out Button moveUnitBtn);
            moveUnitBtn.onClick.AddListener(() =>
            {
                FocusUnitTower();
                MoveUnitButton();
            });
            checkUnitMoveButton.TryGetComponent(out Button checkUnitMoveBtn);
            checkUnitMoveBtn.onClick.AddListener(CheckCanMoveUnit);
            checkUnitMoveButton.SetActive(false);
            sellTowerButton.TryGetComponent(out Button sellTowerBtn);
            sellTowerBtn.onClick.AddListener(SellTower);
            checkTowerButton.onClick.AddListener(CheckTowerButton);
        }

        private void MenuButtonInit()
        {
            _towerGold = startGold;
            _curSpeed = 1;
            goldText.text = "Gold : " + _towerGold;

            speedUpText.text = "x1";

            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            bgmButton.onClick.AddListener(BGMButton);
            gameEndButton.onClick.AddListener(() =>
            {
                DataManager.SaveDamageData();
                damagePanel.SetActive(true);
                damagePanel.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            });
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));
            speedUpButton.onClick.AddListener(SpeedUp);
        }

        private void DamageTextInit()
        {
            damagePanel.SetActive(false);
            var damageTextPanel = damagePanel.transform.GetChild(0);
            _damageTexts = new TMP_Text[damageTextPanel.childCount];
            for (var i = 0; i < _damageTexts.Length; i++)
            {
                _damageTexts[i] = damageTextPanel.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
            }
        }

        private void GameOverPanelInit()
        {
            reStartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        private void IndicatorInit()
        {
            towerRangeParticle.Clear();
            towerRangeParticle.Stop();
            selectedTowerParticle.Clear();
            selectedTowerParticle.Stop();
        }

        #endregion

        public void SetDamageText(Dictionary<string, int> damageDic)
        {
            var damageArray = damageDic.Values.ToArray();
            for (var i = 0; i < damageArray.Length; i++)
            {
                _damageTexts[i].text = damageArray[i].ToString();
            }
        }

        public void PlaceUnitTower(string towerName, Vector3 snappedPos, RaycastHit hit)
        {
            var unitTower = Instantiate(_towerDictionary[towerName], snappedPos, Quaternion.identity)
                .GetComponent<UnitTower>();
            unitTower.unitSpawnPosition = hit.point;
            _curSelectedTower = unitTower;

            TowerUpgrade();
        }

        public void PlaceTower(string towerName, Vector3 snappedPos)
        {
            _curSelectedTower = Instantiate(_towerDictionary[towerName], snappedPos, Quaternion.identity)
                .GetComponent<Tower>();

            TowerUpgrade();
        }

        public void GameStart()
        {
            hudPanel.SetActive(true);
            Time.timeScale = 1;
            _cam.DOOrthoSize(17, 1).From(100).SetEase(Ease.OutQuad);
        }

        private void GameOver()
        {
            gameOverPanel.SetActive(true);
            _cameraManager.enabled = false;
            Time.timeScale = 0;
            DataManager.SaveDamageData();
        }

        private void ToggleTowerButtons()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            if (!_isShowTowerBtn)
            {
                _isShowTowerBtn = true;
                InputManager.Instance.enabled = true;
                _onOffTowerBtnSequence.Restart();
            }
            else
            {
                _isShowTowerBtn = false;
                InputManager.Instance.enabled = false;
                _onOffTowerBtnSequence.PlayBackwards();
            }
        }

        private void CheckTowerButton()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit, Mathf.Infinity);

            if (hit.collider != null && hit.collider.TryGetComponent(out IFingerUp fingerUp))
            {
                fingerUp.FingerUp();
            }
            else
            {
                OffUI();
            }
        }

        private void ClickTower(Tower clickedTower)
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            _towerInfoPanelTween.Restart();
            _isTower = true;
            _isPanelOpen = true;

            if ((!_isUnitTower && _curSelectedTower == clickedTower) ||
                (_isUnitTower && _curUnitTower == clickedTower)) return;

            _curSelectedTower = clickedTower;

            upgradeButton.SetActive(_curSelectedTower.TowerLevel != 4);

            OpenEditButtonPanel();
            UpdateTowerInfo();
            SetIndicator();
            IfUnitTower();
        }

        private void OpenEditButtonPanel()
        {
            towerInfoUI.enabled = true;
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);
            if (!towerInfoUI.gameObject.activeSelf) towerInfoUI.gameObject.SetActive(true);
        }

        private void UpdateTowerInfo()
        {
            var towerLevel = towerData[(int)_curSelectedTower.towerTypeEnum];
            var towerLevelData = towerLevel.towerLevels[_curSelectedTower.TowerLevel];

            _towerInfo.towerName = towerTierName[_curSelectedTower.TowerLevel] + towerLevel.towerName;
            _towerInfo.level = _curSelectedTower.TowerLevel + 1;
            _towerInfo.damage = towerLevelData.damage;
            _towerInfo.range = towerLevelData.attackRange;
            _towerInfo.delay = towerLevelData.attackDelay;
            _towerInfo.sellGold = _sellTowerGold = GetTowerGold(_curSelectedTower);

            towerInfoUI.SetTowerInfo(_towerInfo);
        }

        private void SetIndicator()
        {
            var curTowerPos = _curSelectedTower.transform.position;
            selectedTowerParticle.transform.position = curTowerPos;
            selectedTowerParticle.Clear();
            selectedTowerParticle.Play();

            towerRangeParticle.transform.position = curTowerPos + new Vector3(0, 0.1f, 0);
            var shapeModule = towerRangeParticle.shape;
            shapeModule.radius = _curSelectedTower.TowerRange;
            towerRangeParticle.Clear();
            towerRangeParticle.Play();
        }

        private void IfUnitTower()
        {
            _isUnitTower = _curSelectedTower.TryGetComponent(out UnitTower unitTower);
            moveUnitButton.SetActive(_isUnitTower);
            if (!_isUnitTower) return;
            _curUnitTower = unitTower;
        }

        private void TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
#if UNITY_EDITOR
            tempTower.transform.SetParent(transform);
#endif
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            
            if (!EnoughGold())
            {
                return;
            }

            var t = towerData[(int)tempTower.towerTypeEnum];
            tempTower.TowerLevelUp();
            var tt = t.towerLevels[tempTower.TowerLevel];
            DecreaseGold(tempTower.TowerLevel);

            ObjectPoolManager.Get(StringManager.BuildSmoke, tempTower.transform.position);

            tempTower.TowerSetting(tt.towerMesh, tt.damage, tt.attackRange, tt.attackDelay);
            upgradeButton.SetActive(tempTower.TowerLevel != 4);

            if (tempTower.isSpawn)
            {
                SetIndicator();
            }
            else
            {
                tempTower.isSpawn = true;
                _curSelectedTower.OnClickTower += ClickTower;
                _curSelectedTower = null;
            }
        }

        private void FocusUnitTower()
        {
            _prevSize = _cam.orthographicSize;
            _prevPos = _cameraManager.transform.position;
            _cameraManager.transform.DOMove(_curSelectedTower.transform.position, 0.5f).SetEase(Ease.InQuart);
            _cam.DOOrthoSize(10, 0.5f).SetEase(Ease.InQuart);
        }

        private void RewindCamState()
        {
            _cameraManager.transform.DOMove(_prevPos, 0.5f).SetEase(Ease.InQuart);
            _cam.DOOrthoSize(_prevSize, 0.5f).SetEase(Ease.InQuart);
        }

        private void MoveUnitButton()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            moveUnitButton.SetActive(false);
            towerInfoUI.gameObject.SetActive(false);
            _curUnitTower = _curSelectedTower.GetComponent<UnitTower>();
            checkUnitMoveButton.SetActive(true);
        }

        private void SellTower()
        {
            SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? "SellTower1" :
                _curSelectedTower.TowerLevel < 4 ? "SellTower2" : "SellTower3");

            ObjectPoolManager.Get(StringManager.BuildSmoke, _curSelectedTower.transform);
            Destroy(_curSelectedTower.gameObject);

            IncreaseGold(_sellTowerGold);
            OffUI();
        }

        public void OffUI()
        {
            if (!_isPanelOpen) return;
            _towerInfoPanelTween.PlayBackwards();

            _isTower = false;
            _isPanelOpen = false;
            _curSelectedTower = null;
            towerInfoUI.enabled = false;

            if (towerRangeParticle.isPlaying)
            {
                towerRangeParticle.Clear();
                towerRangeParticle.Stop();
            }

            if (selectedTowerParticle.isPlaying)
            {
                selectedTowerParticle.Clear();
                selectedTowerParticle.Stop();
            }

            if (_curUnitTower == null) return;
            _curUnitTower.OffUnitIndicator();
            _curUnitTower = null;
        }

        public bool EnoughGold()
        {
            var enoughGold = _towerGold > towerBuildGold[_isTower ? _curSelectedTower.TowerLevel + 1 : 0];
            if (enoughGold) return true;
            NotEnoughGoldPopUp().Forget();
            return false;
        }

        private async UniTaskVoid NotEnoughGoldPopUp()
        {
            if (_callNotEnoughTween) return;
            _callNotEnoughTween = true;
            _notEnoughGoldTween.Restart();
            await UniTask.Delay(1000);
            _notEnoughGoldTween.PlayBackwards();
            _callNotEnoughTween = false;
        }

        #region For UnitTower

        private void CheckCanMoveUnit()
        {
            var touch = Input.GetTouch(0);
            if (touch.deltaPosition != Vector2.zero) return;
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            if (hit.collider != null && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(_curUnitTower.transform.position, hit.point) <=
                _curUnitTower.TowerRange)
            {
                StartMoveUnit(new Vector3(hit.point.x, 0, hit.point.z)).Forget();
            }
            else
            {
                cantMoveImage.transform.position = Input.mousePosition;
                _cantMoveImageSequence.Restart();
            }
        }

        private async UniTaskVoid StartMoveUnit(Vector3 pos)
        {
            unitDestinationParticle.transform.position = pos;
            unitDestinationParticle.Play();
            checkUnitMoveButton.SetActive(false);
            RewindCamState();

            await _curUnitTower.StartUnitMove(pos);
            OffUI();

            unitDestinationParticle.Stop();
        }

        #endregion

        #region HUD

        private void Pause()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            IsPause = true;
            Time.timeScale = 0;
            _pauseSequence.Restart();
        }

        private void Resume()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            IsPause = false;
            Time.timeScale = _curSpeed;
            _pauseSequence.PlayBackwards();
        }

        private void BGMButton()
        {
            bgmButton.image.sprite = SoundManager.Instance.BGMToggle() ? musicOnImage : musicOffImage;
        }

        private void SpeedUp()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            _curSpeed = _curSpeed % 3 + 1;
            Time.timeScale = _curSpeed;
            speedUpText.text = $"x{_curSpeed}";
        }

        private void DecreaseGold(int index)
        {
            _towerGold -= towerBuildGold[index];
            goldText.text = "Gold : " + _towerGold;
        }

        private int GetTowerGold(Tower tower)
        {
            var towerLevel = tower.TowerLevel + 1;
            var sum = 0;
            for (var i = 0; i < towerLevel; i++)
            {
                sum += towerBuildGold[i];
            }

            return sum;
        }

        public void IncreaseGold(int amount)
        {
            _towerGold += amount;
            goldText.text = "Gold : " + _towerGold;
        }

        public void DecreaseLifeCountEvent()
        {
            if (_curTowerLife == 0) return;
            _curTowerLife -= 1;
            playerHealthBar.DOShakePosition(0.2f, 15f, 4);
            lifeFillImage.fillAmount = _curTowerLife / lifeCount;
            lifeCountText.text = _curTowerLife + " / " + lifeCount;
            if (_curTowerLife > 0) return;
            GameOver();
        }

        #endregion
    }
}