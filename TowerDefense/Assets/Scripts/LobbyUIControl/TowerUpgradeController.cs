using System.Globalization;
using System.Linq;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
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
        private TowerUpgradeButton _curTowerButton;
        private AttackTowerData _attackTowerData;
        private TowerType _towerType;
        private const byte TowerMaxLevel = 20;
        private string _levelUpString;
        private string _maxLevelString;

        //================= Tower Info ================
        private TMP_Text _atkText;
        private TMP_Text _healthText;
        private TMP_Text _rangeText;
        private TMP_Text _cooldownText;
        private TMP_Text _respawnText;
        private TMP_Text _levelUpText;

        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button closeInfoGroupButton;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Button initTowerLevelButton;

        [SerializeField] private CanvasGroup upgradePanelGroup;
        [SerializeField] private Transform towerButtons;
        [SerializeField] private CanvasGroup towerInfoGroup;
        [SerializeField] private Image infoGroupBackgroundImage;
        [SerializeField] private Image towerImage;
        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;

        [SerializeField] private TMP_Text xpText;
        [SerializeField] private TMP_Text towerLevelText;
        [SerializeField] private TMP_Text upgradeCostText;

        [Header("================= Tower Info ================")]
        [SerializeField] private GameObject atkObj;

        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject rangeObj;
        [SerializeField] private GameObject coolTimeObj;
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
            TowerDataManager.RemoveLocaleMethod();
        }

        private void OnDestroy()
        {
            _upgradePanelGroupSequence?.Kill();
            _deletePanelTween?.Kill();
        }

        private void UpgradePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.OnBackgroundImage();
            _upgradePanelGroupSequence.Restart();
        }

        private void Init()
        {
            TowerDataManager.Init();
            _atkText = atkObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _healthText = healthObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _rangeText = rangeObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _cooldownText = coolTimeObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _respawnText = respawnObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _levelUpText = levelUpButton.GetComponentInChildren<TMP_Text>();

            _levelUpString = LocaleManager.GetLocalizedString(LocaleManager.LobbyUITable, "Level Up");
            _maxLevelString = LocaleManager.GetLocalizedString(LocaleManager.LobbyUITable, "Max Level");
        }

        private void ChangeLocaleTowerDic(Locale locale)
        {
            _levelUpString = LocaleManager.GetLocalizedString(LocaleManager.LobbyUITable, "Level Up");
            _maxLevelString = LocaleManager.GetLocalizedString(LocaleManager.LobbyUITable, "Max Level");
        }

        private void TweenInit()
        {
            var upgradePanel = GetComponent<RectTransform>();
            _upgradePanelGroupSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(upgradePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(upgradePanel.DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _upgradePanelGroupSequence.OnComplete(() => upgradePanelGroup.blocksRaycasts = true);
            _upgradePanelGroupSequence.OnRewind(() => { _lobbyUI.OffBlockImage(); });
            upgradePanelGroup.blocksRaycasts = false;

            var towerInfoRect = towerInfoGroup.GetComponent<RectTransform>();
            _towerInfoGroupSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(towerInfoGroup.DOFade(1, 0.25f).From(0))
                .Join(towerInfoRect.DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _towerInfoGroupSequence.OnComplete(() => towerInfoGroup.blocksRaycasts = true);
            
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
                _lobbyUI.OffBackgroundImage();
                _lobbyUI.SetActiveButtons(true, false);
                upgradePanelGroup.blocksRaycasts = false;
                _upgradePanelGroupSequence.PlayBackwards();
                UniTask.RunOnThreadPool(() => { BackendGameData.instance.GameDataUpdate(); });
            });
            initTowerLevelButton.onClick.AddListener(() =>
            {
                FullscreenAlert.CancelableAlert(FullscreenAlertEnum.TowerLevelInitAlert, InitTowerLevel);
            });

            closeInfoGroupButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                levelUpButton.interactable = true;
                infoGroupBackgroundImage.enabled = false;
                towerInfoGroup.blocksRaycasts = false;
                _towerInfoGroupSequence.PlayBackwards();
            });
            levelUpButton.onClick.AddListener(() =>
            {
                var userData = BackendGameData.userData;
                var prevTowerLv = userData.towerLevelTable[_towerType.ToString()];

                if (prevTowerLv >= TowerMaxLevel || userData.xp < (prevTowerLv + 1) * 25)
                {
                    FloatingNotification.FloatingNotify(FloatingNotifyEnum.NeedMoreTowerCoin);
                    return;
                }

                LevelUp(userData, prevTowerLv);
                SetTowerInfo();
            });
        }

        private void TowerButtonInit()
        {
            var towerLevelTable = BackendGameData.userData.towerLevelTable;
            for (int i = 0; i < towerButtons.childCount; i++)
            {
                var towerUpgradeButton = towerButtons.GetChild(i).GetComponent<TowerUpgradeButton>();
                towerUpgradeButton.attackTowerData.InitState();
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
                    towerNameText.text = TowerDataManager.TowerInfoTable[_towerType].towerName;
                    towerDescriptionText.text = TowerDataManager.TowerInfoTable[_towerType].towerDescription;
                    var prevTowerLv = towerLevelTable[_towerType.ToString()];
                    if (prevTowerLv >= TowerMaxLevel)
                    {
                        levelUpButton.interactable = false;
                        _levelUpText.text = _maxLevelString;
                    }
                    else
                    {
                        _levelUpText.text = _levelUpString;
                    }

                    towerLevelText.text = prevTowerLv.ToString();
                    towerUpgradeButton.towerLevelText.text = "Lv. " + prevTowerLv;
                    upgradeCostText.text = ((prevTowerLv + 1) * 25).ToString();
                    GetTowerInfo();
                    SetTowerInfo();
                });
            }
        }

        private void LevelUp(UserData userData, int prevTowerLv)
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            userData.xp -= (prevTowerLv + 1) * 25;
            xpText.text = userData.xp.ToString();
            var curTowerLv = userData.towerLevelTable[_towerType.ToString()] += 1;

            towerLevelText.text = curTowerLv.ToString();
            _curTowerButton.towerLevelText.text = "Lv. " + curTowerLv;

            upgradeCostText.text = ((curTowerLv + 1) * 25).ToString();
            _attackTowerData.UpgradeData(curTowerLv);

            if (curTowerLv >= TowerMaxLevel)
            {
                levelUpButton.interactable = false;
                _levelUpText.text = _maxLevelString;
            }
        }

        private void GetTowerInfo()
        {
            var isUnitTower = _attackTowerData.isUnitTower;

            healthObj.SetActive(isUnitTower);
            rangeObj.SetActive(!isUnitTower);
            coolTimeObj.SetActive(!isUnitTower);
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

            _atkText.text = _attackTowerData.curDamage.ToString();
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