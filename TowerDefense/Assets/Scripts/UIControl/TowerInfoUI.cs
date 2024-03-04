using System;
using System.Globalization;
using CustomEnumControl;
using DataControl.TowerDataControl;
using ManagerControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoUI : MonoBehaviour
    {
        private Image[] _starImages;
        private TowerType _towerType;
        private sbyte _prevTowerLevel;
        private Vector3 _towerPos;
        private bool _isTargeting;
        private Camera _cam;
        private Transform _towerTransform;

        private TMP_Text _healthText;
        private TMP_Text _rangeText;
        private TMP_Text _cooldownText;
        private TMP_Text _respawnText;
        private TMP_Text _damageText;

        [SerializeField] private RectTransform towerInfoCardRect;
        [SerializeField] private Image damageImage;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject rangeObj;
        [SerializeField] private GameObject rpmObj;
        [SerializeField] private GameObject respawnObj;
        [SerializeField] private GameObject towerLevelStar;
        [SerializeField] private CanvasGroup towerStatusPanelObj;

        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI sellGoldText;

        private void LateUpdate()
        {
            if (!_isTargeting) return;

            var towerPos = _cam.WorldToScreenPoint(_towerTransform.position);
            towerInfoCardRect.anchoredPosition =
                towerPos.x > Screen.width * 0.5f ? new Vector2(-600, 0) : new Vector2(600, 0);
        }

        private void OnDisable()
        {
            _isTargeting = false;
        }

        public void Init()
        {
            _cam = Camera.main;
            _prevTowerLevel = -1;
            _towerType = TowerType.None;
            _starImages = new Image[towerLevelStar.transform.childCount];

            for (var i = 0; i < _starImages.Length; i++)
            {
                _starImages[i] = towerLevelStar.transform.GetChild(i).GetChild(2).GetComponent<Image>();
            }

            _healthText = healthObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _rangeText = rangeObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _cooldownText = rpmObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _respawnText = respawnObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _damageText = damageImage.transform.GetChild(0).GetComponent<TMP_Text>();
        }

        public void SetCardPos(Transform towerTransform)
        {
            _isTargeting = true;
            _towerTransform = towerTransform;
        }

        public void SetTowerInfo(AttackTower tower, TowerData towerData, sbyte level,
            ushort upgradeCost, ushort sellCost, string towerName)
        {
            towerStatusPanelObj.alpha = 1;
            towerLevelStar.SetActive(true);

            if (towerData is AttackTowerData battleTowerData)
            {
                if (towerData.isUnitTower)
                {
                    var unitTower = (SummonTower)tower;
                    _healthText.text = (unitTower.unitHealth * (level + 1)).ToString();
                    _respawnText.text = unitTower.unitReSpawnTime.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    _cooldownText.text = battleTowerData.attackCooldown.ToString(CultureInfo.InvariantCulture);
                }
            }

            var isUnitTower = towerData.isUnitTower;
            healthObj.SetActive(isUnitTower);
            rangeObj.SetActive(!isUnitTower);
            rpmObj.SetActive(!isUnitTower);
            respawnObj.SetActive(isUnitTower);
            var towerType = tower.towerType;

            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                towerNameText.text = towerName;
                damageImage.sprite = UIManager.GetTowerType(towerType);
            }

            DisplayStarsForTowerLevel(level);
            goldText.text = upgradeCost + "G";
            _damageText.text = tower.towerDamage.ToString();
            _rangeText.text = tower.towerRange.ToString();
            sellGoldText.text = sellCost + "G";
        }

        public void SetSupportTowerInfo(SupportTower tower, ushort sellCost, string towerName)
        {
            towerStatusPanelObj.alpha = 0;
            towerLevelStar.SetActive(false);

            var towerType = tower.towerType;

            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                towerNameText.text = towerName;
            }

            sellGoldText.text = sellCost + "G";
        }

        private void DisplayStarsForTowerLevel(sbyte level)
        {
            if (_prevTowerLevel == level) return;
            _prevTowerLevel = level;

            for (var i = 0; i < towerLevelStar.transform.childCount; i++)
            {
                _starImages[i].enabled = i <= level;
            }
        }

        public void LocaleTowerName()
        {
            LocaleManager.ChangeLocaleAsync(LocaleManager.TowerCardTable, _towerType.ToString(), towerNameText)
                .Forget();
        }
    }
}