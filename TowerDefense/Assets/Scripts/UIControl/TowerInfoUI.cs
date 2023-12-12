using CustomEnumControl;
using DataControl;
using GameControl;
using ManagerControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.Localization.Settings;
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

        [Header("-------------Tower Status--------------")] [SerializeField]
        private Transform followTowerUI;

        [SerializeField] private GameObject healthObj;
        [SerializeField] private Transform stars;
        [SerializeField] private Image damageImage;
        [SerializeField] private Image delayImage;
        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI attackRangeText;
        [SerializeField] private TextMeshProUGUI rpmText;
        [SerializeField] private TextMeshProUGUI sellCostText;
        [SerializeField] private GameObject statusInfoPanel;

        #region Unity Event

        private void Awake()
        {
            _prevTowerLevel = -1;
            _towerType = TowerType.None;
            _cam = Camera.main;
            _starImages = new Image[stars.childCount];
            _statusInfoPanel = transform.GetChild(1);
            for (int i = 0; i < _starImages.Length; i++)
            {
                _starImages[i] = stars.GetChild(i).GetChild(0).GetComponent<Image>();
            }
        }

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

        public void SetInfoUI(Vector3 towerPos)
        {
            _isTargeting = true;
            _followTowerPos = towerPos;
        }

        public void SetSupportInfoUI()
        {
            statusInfoPanel.SetActive(false);
        }

        public void SetTowerInfo(Tower tower, bool isUnitTower, sbyte level, ushort upgradeCost, ushort sellCost)
        {
            if (!statusInfoPanel.activeSelf) statusInfoPanel.SetActive(true);
            if (tower.TowerData is BattleTowerData battleTowerData)
            {
                if (isUnitTower)
                {
                    var unitTower = (UnitTower)tower;
                    healthText.text = CachedNumber.GetUIText(unitTower.UnitHealth * (level + 1));
                    rpmText.text = CachedNumber.GetUIText(unitTower.UnitReSpawnTime);
                }
                else
                {
                    rpmText.text = CachedNumber.GetUIText(battleTowerData.AttackRpm);
                }
            }

            healthObj.SetActive(isUnitTower);

            var towerType = tower.TowerData.TowerType;
            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                var uiManager = UIManager.Instance;
                towerNameText.text = uiManager.towerNameDic[towerType];
                damageImage.sprite = uiManager.GetTowerType(tower.TowerData);
                delayImage.sprite = uiManager.IsUnitTower(tower.TowerData);
            }

            DisplayStarsForTowerLevel(level);
            costText.text = upgradeCost + "g";
            damageText.text = CachedNumber.GetUIText(tower.Damage);
            attackRangeText.text = CachedNumber.GetUIText(tower.TowerRange);
            sellCostText.text = sellCost + "g";
        }

        public void SetSupportTowerInfo(SupportTower tower, ushort sellCost)
        {
            var towerType = tower.TowerType;
            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                var uiManager = UIManager.Instance;
                towerNameText.text = uiManager.towerNameDic[towerType];
            }

            sellCostText.text = sellCost + "g";
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