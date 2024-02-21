using System.Globalization;
using CustomEnumControl;
using DataControl.TowerDataControl;
using ManagerControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoUI : MonoBehaviour
    {
        private Camera _cam;
        private Image[] _starImages;
        private Vector3 _followTowerPos;
        private TowerType _towerType;
        private sbyte _prevTowerLevel;
        private bool _isTargeting;
        private Transform _statusInfoPanel;

        private TMP_Text _healthText;
        private TMP_Text _rangeText;
        private TMP_Text _cooldownText;
        private TMP_Text _respawnText;
        private TMP_Text _damageText;

        [SerializeField] private Transform followTowerUI;

        [SerializeField] private Image damageImage;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject rangeObj;
        [SerializeField] private GameObject rpmObj;
        [SerializeField] private GameObject respawnObj;
        [SerializeField] private Transform stars;

        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI sellGoldText;

#region Unity Event

        private void LateUpdate()
        {
            if (!_isTargeting) return;
            followTowerUI.position = _cam.WorldToScreenPoint(_followTowerPos);
            _statusInfoPanel.position = followTowerUI.position.x > Screen.width * 0.5f
                ? followTowerUI.position - new Vector3(500, 0, 0)
                : followTowerUI.position + new Vector3(500, 0, 0);
        }

        private void OnDisable()
        {
            _isTargeting = false;
        }

#endregion

        public void Init()
        {
            _prevTowerLevel = -1;
            _towerType = TowerType.None;
            _cam = Camera.main;
            _starImages = new Image[stars.childCount];
            _statusInfoPanel = transform.GetChild(1);

            for (var i = 0; i < _starImages.Length; i++)
            {
                _starImages[i] = stars.GetChild(i).GetChild(2).GetComponent<Image>();
            }

            _healthText = healthObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _rangeText = rangeObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _cooldownText = rpmObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _respawnText = respawnObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _damageText = damageImage.transform.GetChild(0).GetComponent<TMP_Text>();
        }

        public void SetInfoUI(Vector3 towerPos)
        {
            _isTargeting = true;
            _followTowerPos = towerPos;
        }

        public void SetTowerInfo(AttackTower tower, TowerData towerData, sbyte level,
            ushort upgradeCost, ushort sellCost, string towerName)
        {
            if (!_statusInfoPanel.gameObject.activeSelf) _statusInfoPanel.gameObject.SetActive(true);

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
            _statusInfoPanel.gameObject.SetActive(false);

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

            for (var i = 0; i < stars.childCount; i++)
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