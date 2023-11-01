using System.Globalization;
using CustomEnumControl;
using DataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoUI : MonoBehaviour
    {
        private Camera _cam;
        private Locale prevLanguage;

        private Vector3 _followTowerPos;
        private TowerType _towerType;

        private bool isTargeting;

        [Header("-------------Tower Status--------------")] [SerializeField]
        private Transform followTowerUI;

        [SerializeField] private GameObject healthObj;
        [SerializeField] private Transform stars;
        [SerializeField] private Image damageImage;
        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI attackRangeText;
        [SerializeField] private TextMeshProUGUI attackDelayText;
        [SerializeField] private TextMeshProUGUI sellGoldText;

        #region Unity Event

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Start()
        {
            LocalizationSettings.SelectedLocaleChanged += OnChangeLocale;
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

        #endregion

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

            towerNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString(UIManager.TowerTypeTable,
                towerData.TowerType.ToString(), LocalizationSettings.SelectedLocale);

            damageImage.sprite = UIManager.Instance.WhatTypeOfThisTower(towerData);

            var towerLevelData = towerData.TowerLevels[level];

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

        private void OnChangeLocale(Locale locale)
        {
            LocaleManager.ChangeLocaleAsync(UIManager.TowerTypeTable, _towerType.ToString(), towerNameText).Forget();
        }
    }
}