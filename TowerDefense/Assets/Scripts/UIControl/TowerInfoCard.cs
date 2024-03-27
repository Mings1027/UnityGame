using System;
using System.Globalization;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using TowerControl;
using UnitControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoCard : MonoBehaviour
    {
        private Image[] _starImages;
        private TowerType _towerType;
        private Vector3 _towerPos;
        private Camera _cam;
        private Transform _towerTransform;
        private Tween _towerDisappearTween;
        private Tweener _towerCardMoveTween;
        private CanvasGroup _towerCardGroup;
        private MoveUnitController _moveUnitController;
        private SummonTower _summonTower;
        private TowerData _towerData;

        private TMP_Text _atkText;
        private TMP_Text _healthText;
        private TMP_Text _rangeText;
        private TMP_Text _coolTimeText;
        private TMP_Text _respawnText;

        private const byte TowerMaxLevel = 5;
        private byte _prevTowerLevel;
        private bool _isTargeting;
        private bool _isClickSellButton;

        public bool startMoveUnit { get; private set; }
        public event Action OnTowerUpgradeEvent;
        public event Action OnSellTowerEvent;

        [SerializeField] private RectTransform towerInfoCardRect;

        [SerializeField] private RectTransform starParent;
        [SerializeField] private GameObject atkObj;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject rangeObj;
        [SerializeField] private GameObject coolTimeObj;
        [SerializeField] private GameObject respawnObj;

        [SerializeField] private Button upgradeButton;
        [SerializeField] private Image maxLevelImage;
        [SerializeField] private Button sellTowerButton;
        [SerializeField] private Image sellButtonImage;
        [SerializeField] private Button moveUnitButton;

        [SerializeField] private Sprite sellSprite;
        [SerializeField] private Sprite checkSprite;

        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI sellGoldText;

        private void Start()
        {
            Init();
            ButtonInit();
        }

        private void LateUpdate()
        {
            if (!_isTargeting) return;

            var towerPos = _cam.WorldToScreenPoint(_towerTransform.position);
            if (towerPos.x > Screen.width * 0.5f)
            {
                _towerCardMoveTween.ChangeStartValue(towerInfoCardRect.anchoredPosition)
                    .ChangeEndValue(new Vector2(-400, 0)).Restart();
            }
            else
            {
                _towerCardMoveTween.ChangeStartValue(towerInfoCardRect.anchoredPosition)
                    .ChangeEndValue(new Vector2(400, 0)).Restart();
            }
        }

        private void Init()
        {
            _cam = Camera.main;
            _prevTowerLevel = 0;
            _starImages = new Image[starParent.childCount];
            _towerCardGroup = GetComponent<CanvasGroup>();
            _moveUnitController = FindAnyObjectByType<MoveUnitController>();

            for (var i = 0; i < _starImages.Length; i++)
            {
                _starImages[i] = starParent.GetChild(i).GetChild(2).GetComponent<Image>();
            }

            _atkText = atkObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _healthText = healthObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _rangeText = rangeObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _coolTimeText = coolTimeObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _respawnText = respawnObj.transform.GetChild(1).GetComponent<TMP_Text>();

            _towerDisappearTween = _towerCardGroup.DOFade(1, 0.2f).From(0).SetAutoKill(false).Pause().SetUpdate(true);
            _towerDisappearTween.OnComplete(() => _towerCardGroup.blocksRaycasts = true);
            _towerDisappearTween.OnRewind(() => towerInfoCardRect.anchoredPosition = Vector2.zero);
            _towerCardMoveTween = towerInfoCardRect.DOAnchorPosX(towerInfoCardRect.anchoredPosition.x, 0.05f)
                .SetAutoKill(false).Pause().SetUpdate(true);
            _towerCardGroup.blocksRaycasts = false;
        }

        private void ButtonInit()
        {
            upgradeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                if (_isClickSellButton) ResetSprite();
                OnTowerUpgradeEvent?.Invoke();
            });
            sellTowerButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                if (_isClickSellButton)
                {
                    _isClickSellButton = false;
                    sellButtonImage.sprite = sellSprite;
                    OnSellTowerEvent?.Invoke();
                    BuildTowerManager.DeSelectTower();
                }
                else
                {
                    _isClickSellButton = true;
                    sellButtonImage.sprite = checkSprite;
                }
            });
            _moveUnitController.OnStopMoveUnit += () => { startMoveUnit = false; };
            moveUnitButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _moveUnitController.FocusUnitTower(_summonTower, _towerData);
                SetCardPos(false, null);
                CloseCard();
                startMoveUnit = true;
            });
        }

        public void SetCardPos(bool value, Transform towerTransform)
        {
            _isTargeting = value;
            _towerTransform = towerTransform;
        }

        public void OpenCard()
        {
            _towerDisappearTween.Restart();
        }

        public void CloseCard()
        {
            _towerCardGroup.blocksRaycasts = false;
            _towerDisappearTween.PlayBackwards();
        }

        public void SetTowerInfo(Tower tower, TowerData towerData, byte level,
            ushort upgradeCost, ushort sellCost, string towerName)
        {
            if (tower is AttackTower attackTower)
            {
                if (towerData.isUnitTower)
                {
                    var summonTowerData = (SummoningTowerData)towerData;
                    _healthText.text = (summonTowerData.curUnitHealth * level).ToString();
                    _respawnText.text = summonTowerData.initReSpawnTime.ToString(CultureInfo.InvariantCulture);
                    _summonTower = (SummonTower)tower;
                }
                else
                {
                    var atkTowerData = (AttackTowerData)towerData;
                    _coolTimeText.text = atkTowerData.attackCooldown.ToString(CultureInfo.InvariantCulture);
                }

                _atkText.text = attackTower.towerDamage.ToString();
            }
            else if (tower is SupportTower supportTower)
            {
                _atkText.text = "-";
                var supportTowerData = (SupportTowerData)towerData;
                _coolTimeText.text = supportTowerData.towerUpdateCooldown.ToString(CultureInfo.InvariantCulture);
            }

            var isUnitTower = towerData.isUnitTower;
            healthObj.SetActive(isUnitTower);
            rangeObj.SetActive(!isUnitTower);
            coolTimeObj.SetActive(!isUnitTower);
            respawnObj.SetActive(isUnitTower);
            var towerType = towerData.towerType;

            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                towerNameText.text = towerName;
            }

            DisplayStarsForTowerLevel(level);
            goldText.text = upgradeCost + "G";
            _rangeText.text = towerData.curRange.ToString();
            sellGoldText.text = sellCost + "G";

            upgradeButton.gameObject.SetActive(level != TowerMaxLevel);
            maxLevelImage.gameObject.SetActive(level == TowerMaxLevel);
            moveUnitButton.gameObject.SetActive(isUnitTower);
            if (_isClickSellButton) ResetSprite();
        }

        private void DisplayStarsForTowerLevel(byte level)
        {
            if (_prevTowerLevel == level) return;
            _prevTowerLevel = level;

            for (var i = 0; i < starParent.childCount; i++)
            {
                _starImages[i].enabled = i < level;
            }
        }

        private void ResetSprite()
        {
            _isClickSellButton = false;
            sellButtonImage.sprite = sellSprite;
        }
    }
}