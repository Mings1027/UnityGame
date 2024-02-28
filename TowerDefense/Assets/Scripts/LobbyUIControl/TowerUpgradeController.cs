using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class TowerUpgradeController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private Tween _upgradePanelGroupSequence;
        private Tween _towerInfoGroupSequence;
        private Tween _deletePanelTween;
        private CanvasGroup _upgradePanelGroup;
        private TowerUpgradeButton _curTowerButton;
        private AttackTowerData _attackTowerData;
        private TowerType _towerType;
        private Dictionary<TowerType, string> _towerInfoDic;
        private const byte TowerMaxLevel = 20;

        //================= Tower Info ================
        private TMP_Text _healthText;
        private TMP_Text _rangeText;
        private TMP_Text _cooldownText;
        private TMP_Text _respawnText;

        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button closeInfoGroupButton;
        [SerializeField] private Button levelUpButton;

        [SerializeField] private NoticePanel notifyInitLevelPanel;
        [SerializeField] private Transform towerButtons;
        [SerializeField] private CanvasGroup towerInfoGroup;
        [SerializeField] private Image infoGroupBackgroundImage;
        [SerializeField] private Image towerImage;
        [SerializeField] private TMP_Text towerDescriptionText;

        [SerializeField] private TMP_Text xpText;
        [SerializeField] private TMP_Text towerLevelText;
        [SerializeField] private TMP_Text upgradeCostText;

        [Header("================= Tower Info ================")]
        [SerializeField] private Image damageImage;

        [SerializeField] private TMP_Text damageText;

        [SerializeField] private Sprite physicalSprite, magicSprite;

        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject rangeObj;
        [SerializeField] private GameObject rpmObj;
        [SerializeField] private GameObject respawnObj;

        private void Awake()
        {
            Input.multiTouchEnabled = false;
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleTowerDic;
        }

        private void Start()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            Init();
            TweenInit();
            ButtonInit();
            TowerButtonInit();
            xpText.text = BackendGameData.userData.xp.ToString();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= ChangeLocaleTowerDic;
        }

        private void OnDestroy()
        {
            _upgradePanelGroupSequence?.Kill();
            _deletePanelTween?.Kill();
        }

        private void UpgradePanel()
        {
            upgradeButton.gameObject.SetActive(false);
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.OnBackgroundImage();
            _upgradePanelGroupSequence.Restart();
        }

        private void Init()
        {
            _healthText = healthObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _rangeText = rangeObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _cooldownText = rpmObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _respawnText = respawnObj.transform.GetChild(1).GetComponent<TMP_Text>();

            _towerInfoDic = new Dictionary<TowerType, string>();
            var towerTypes = Enum.GetValues(typeof(TowerType));
            foreach (TowerType towerType in towerTypes)
            {
                if (towerType == TowerType.None) continue;
                _towerInfoDic.Add(towerType,
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, LocaleManager.CardKey + towerType));
            }
        }

        private void ChangeLocaleTowerDic(Locale locale)
        {
            foreach (var towerType in _towerInfoDic.Keys.ToList())
            {
                _towerInfoDic[towerType] =
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, LocaleManager.CardKey + towerType);
            }
        }

        private void TweenInit()
        {
            _upgradePanelGroup = GetComponent<CanvasGroup>();
            var upgradePanel = GetComponent<RectTransform>();
            _upgradePanelGroupSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_upgradePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(upgradePanel.DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _upgradePanelGroupSequence.OnComplete(() => _upgradePanelGroup.blocksRaycasts = true);
            _upgradePanelGroupSequence.OnRewind(() => { _lobbyUI.OffBlockImage(); });
            _upgradePanelGroup.blocksRaycasts = false;

            var towerInfoRect = towerInfoGroup.GetComponent<RectTransform>();
            _towerInfoGroupSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(towerInfoGroup.DOFade(1, 0.25f).From(0))
                .Join(towerInfoRect.DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _towerInfoGroupSequence.OnComplete(() => towerInfoGroup.blocksRaycasts = true);
            _towerInfoGroupSequence.OnRewind(() => towerInfoGroup.blocksRaycasts = false);
            towerInfoGroup.blocksRaycasts = false;
            infoGroupBackgroundImage.enabled = false;
        }

        private void ButtonInit()
        {
            upgradeButton.onClick.AddListener(() =>
            {
                UpgradePanel();
                _lobbyUI.SetActiveButtons(false, false);
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                upgradeButton.gameObject.SetActive(true);
                _lobbyUI.OffBackgroundImage();
                _lobbyUI.SetActiveButtons(true, false);
                _upgradePanelGroup.blocksRaycasts = false;
                _upgradePanelGroupSequence.PlayBackwards();
                UniTask.RunOnThreadPool(() => { BackendGameData.instance.GameDataUpdate(); });
            });
            notifyInitLevelPanel.OnConfirmButtonEvent += InitTowerLevel;

            closeInfoGroupButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                levelUpButton.interactable = true;
                infoGroupBackgroundImage.enabled = false;
                _towerInfoGroupSequence.PlayBackwards();
            });
            levelUpButton.onClick.AddListener(() =>
            {
                LevelUp();
                SetTowerInfo();
            });
        }

        private void TowerButtonInit()
        {
            var towerLevelTable = BackendGameData.userData.towerLevelTable;
            for (int i = 0; i < towerButtons.childCount; i++)
            {
                var towerUpgradeButton = towerButtons.GetChild(i).GetComponent<TowerUpgradeButton>();
                var towerLevel = towerLevelTable[towerUpgradeButton.attackTowerData.towerType.ToString()];
                towerUpgradeButton.attackTowerData.InitState();
                if (towerLevel >= TowerMaxLevel)
                    towerUpgradeButton.upgradeButton.interactable = false;

                towerUpgradeButton.towerLevelText.text =
                    "Lv. " + towerLevelTable[towerUpgradeButton.attackTowerData.towerType.ToString()];

                towerUpgradeButton.upgradeButton.onClick.AddListener(() =>
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    infoGroupBackgroundImage.enabled = true;
                    _towerInfoGroupSequence.Restart();
                    _curTowerButton = towerUpgradeButton;
                    _attackTowerData = towerUpgradeButton.attackTowerData;
                    _towerType = _attackTowerData.towerType;
                    towerImage.sprite = towerUpgradeButton.towerImage.sprite;
                    towerDescriptionText.text = _towerInfoDic[_towerType];
                    var prevTowerLv = towerLevelTable[_towerType.ToString()];
                    if (prevTowerLv >= TowerMaxLevel) levelUpButton.interactable = false;

                    towerLevelText.text = prevTowerLv.ToString();
                    towerUpgradeButton.towerLevelText.text = "Lv. " + prevTowerLv;
                    upgradeCostText.text = ((prevTowerLv + 1) * 25).ToString();
                    GetTowerInfo();
                    SetTowerInfo();
                });
            }
        }

        private void LevelUp()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            var userData = BackendGameData.userData;
            var prevTowerLv = userData.towerLevelTable[_towerType.ToString()];

            if (prevTowerLv >= TowerMaxLevel || userData.xp < (prevTowerLv + 1) * 25) return;
            userData.xp -= (prevTowerLv + 1) * 25;
            xpText.text = userData.xp.ToString();
            var curTowerLv = userData.towerLevelTable[_towerType.ToString()] += 1;

            towerLevelText.text = curTowerLv.ToString();
            _curTowerButton.towerLevelText.text = "Lv. " + curTowerLv;

            upgradeCostText.text = ((curTowerLv + 1) * 25).ToString();
            _attackTowerData.UpgradeData(curTowerLv);

            if (curTowerLv >= TowerMaxLevel) levelUpButton.interactable = false;
        }

        private void GetTowerInfo()
        {
            var isUnitTower = _attackTowerData.isUnitTower;

            damageImage.sprite = _attackTowerData.isMagicTower ? magicSprite : physicalSprite;

            healthObj.SetActive(isUnitTower);
            rangeObj.SetActive(!isUnitTower);
            rpmObj.SetActive(!isUnitTower);
            respawnObj.SetActive(isUnitTower);
        }

        private void SetTowerInfo()
        {
            if (_attackTowerData.isUnitTower)
            {
                var unitTower = (SummoningTowerData)_attackTowerData;
                _healthText.text = unitTower.curUnitHealth.ToString();
                _respawnText.text = unitTower.initReSpawnTime.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                _cooldownText.text = _attackTowerData.attackCooldown.ToString(CultureInfo.InvariantCulture);
            }

            damageText.text = _attackTowerData.curDamage.ToString();
            _rangeText.text = _attackTowerData.curRange.ToString();
        }

        private int GetSpentXp()
        {
            var spentXp = BackendGameData.userData.xp;
            var towerLevelTable = BackendGameData.userData.towerLevelTable;
            foreach (var key in towerLevelTable.Keys)
            {
                var towerLv = towerLevelTable[key];
                if (towerLv <= 0) continue;
                for (var i = 0; i < towerLv; i++)
                {
                    spentXp += (i + 1) * 25;
                }
            }

            return spentXp;
        }

        private void InitTowerLevel()
        {
            if (BackendGameData.userData.xp <= 0) return;

            var xp = BackendGameData.userData.xp = GetSpentXp();
            xpText.text = xp.ToString();

            foreach (var key in BackendGameData.userData.towerLevelTable.Keys.ToList())
            {
                BackendGameData.userData.towerLevelTable[key] = 0;
            }

            for (int i = 0; i < towerButtons.childCount; i++)
            {
                var towerUpgradeBtn = towerButtons.GetChild(i).GetComponent<TowerUpgradeButton>();
                towerUpgradeBtn.attackTowerData.InitState();
                towerUpgradeBtn.towerLevelText.text = "Lv. 0";
            }

            UniTask.RunOnThreadPool(() => { BackendGameData.instance.GameDataUpdate(); });
        }
    }
}