using System;
using System.Collections.Generic;
using System.Globalization;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoUI : MonoBehaviour
    {
        private Camera _cam;

        private TowerData _towerData;
        private Locale prevLanguage;

        private Vector3 _followTowerPos;
        private TowerType _towerType;

        private const string TowerTypeTable = "TowerType Table";
        private string panelKey;
        private bool isTargeting;

        [Header("-------------Panel-------------")] [SerializeField]
        private GameObject towerInfoPanel;

        [SerializeField] private TextMeshProUGUI panelTowerName;

        [SerializeField] private TextMeshProUGUI panelTowerInfo;
        [SerializeField] private GameObject panelHealthObj;
        [SerializeField] private Image panelDamageImage;
        [SerializeField] private TextMeshProUGUI panelHealthText;
        [SerializeField] private TextMeshProUGUI panelDamageText;
        [SerializeField] private TextMeshProUGUI panelAtkDelayText;

        [Header("-------------Tower Status--------------")] [SerializeField]
        private Transform followTowerUI;

        [SerializeField] private GameObject healthObj;
        [SerializeField] private Transform stars;
        [SerializeField] private Sprite physicalSprite;
        [SerializeField] private Sprite magicSprite;
        [SerializeField] private Image damageImage;
        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI attackRangeText;
        [SerializeField] private TextMeshProUGUI attackDelayText;
        [SerializeField] private TextMeshProUGUI sellGoldText;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Start()
        {
            LocalizationSettings.SelectedLocaleChanged += OnChangeLocale;
            towerInfoPanel.SetActive(false);
        }

        private void OnDisable()
        {
            isTargeting = false;
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;
            followTowerUI.position = _cam.WorldToScreenPoint(_followTowerPos);
        }

        public void SetFollowTarget(Vector3 towerPos)
        {
            isTargeting = true;
            _followTowerPos = towerPos;
        }

        public void SetTowerInfo(TowerData towerData, bool isUnitTower, sbyte level, int sellGold)
        {
            _towerType = towerData.TowerType;
            if (isUnitTower)
            {
                healthObj.SetActive(true);
                var unitTowerData = (UnitTowerData)towerData;
                healthText.text = (unitTowerData.UnitHealth * (level + 1)).ToString();
            }
            else
            {
                healthObj.SetActive(false);
            }

            damageImage.sprite = towerData.IsMagicTower ? magicSprite : physicalSprite;

            var towerLevelData = towerData.TowerLevels[level];

            var text = LocalizationSettings.StringDatabase.GetLocalizedString(TowerTypeTable,
                towerData.TowerType.ToString(),
                LocalizationSettings.SelectedLocale);
            towerNameText.text = text;

            DisplayStarsForTowerLevel(level);
            costText.text = towerData.TowerUpgradeCost * (level + 1) + "g";
            damageText.text = towerLevelData.damage.ToString();
            attackRangeText.text = towerLevelData.attackRange.ToString();
            attackDelayText.text = towerLevelData.attackDelay.ToString(CultureInfo.InvariantCulture);
            sellGoldText.text = sellGold + "g";
        }

        private void DisplayStarsForTowerLevel(sbyte level)
        {
            for (var i = 0; i < stars.childCount; i++)
            {
                stars.GetChild(i).GetChild(0).localScale = Vector3.zero;

                if (i <= level)
                {
                    stars.GetChild(i).GetChild(0).DOScale(1, 0.1f);
                }
            }
        }

        public void SetPanelInfo(TowerData towerData, bool isUnitTower)
        {
            towerInfoPanel.SetActive(true);
            if (towerData.Equals(_towerData) && prevLanguage.Equals(LocalizationSettings.SelectedLocale)) return;
            _towerData = towerData;
            prevLanguage = LocalizationSettings.SelectedLocale;
            panelKey = "Panel-" + towerData.TowerType;
            panelTowerName.text = LocalizationSettings.StringDatabase.GetLocalizedString(TowerTypeTable,
                towerData.TowerType.ToString(), LocalizationSettings.SelectedLocale);
            panelTowerInfo.text = LocalizationSettings.StringDatabase.GetLocalizedString(TowerTypeTable, panelKey,
                LocalizationSettings.SelectedLocale);
            if (isUnitTower)
            {
                panelHealthObj.SetActive(true);
                var unitTowerData = (UnitTowerData)towerData;
                panelHealthText.text = unitTowerData.UnitHealth.ToString();
            }
            else
            {
                panelHealthObj.SetActive(false);
            }

            panelDamageImage.sprite = towerData.IsMagicTower ? magicSprite : physicalSprite;

            var towerLevelData = towerData.TowerLevels[0];
            panelDamageText.text = towerLevelData.damage.ToString();
            panelAtkDelayText.text = towerLevelData.attackDelay.ToString(CultureInfo.InvariantCulture);
        }

        public void DisablePanel() => towerInfoPanel.SetActive(false);

        private void OnChangeLocale(Locale locale)
        {
            ChangeLocaleAsync().Forget();
        }

        private async UniTaskVoid ChangeLocaleAsync()
        {
            var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync(TowerTypeTable);
            await loadingOperation;
            if (loadingOperation.Status == AsyncOperationStatus.Succeeded)
            {
                var table = loadingOperation.Result;
                var towerName = table.GetEntry(_towerType.ToString())?.GetLocalizedString();

                towerNameText.text = towerName;
            }
        }
    }
}