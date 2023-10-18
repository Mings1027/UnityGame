using System.Globalization;
using DataControl;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoUI : MonoBehaviour
    {
        private Camera _cam;

        private Vector3 _followTowerPos;

        [SerializeField] private Transform followTowerUI;

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

        private void LateUpdate()
        {
            followTowerUI.position = _cam.WorldToScreenPoint(_followTowerPos);
        }

        public void SetFollowTarget(Vector3 towerPos) => _followTowerPos = towerPos;

        public void SetTowerInfo(TowerData towerData, bool isUnitTower, sbyte level, int sellGold)
        {
            healthObj.SetActive(isUnitTower);

            damageImage.sprite = towerData.IsMagicTower ? magicSprite : physicalSprite;

            var towerLevelData = towerData.TowerLevels[level];

            towerNameText.text = towerData.TowerType.ToString();

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

        public void OnUnitTowerHealth(int health) => healthText.text = health.ToString();
    }
}