using System;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoUI : MonoBehaviour
    {
        private Camera _cam;

        private Vector3 _followTowerPos;

        private string _levelTxt, _damageTxt, _rangeTxt, _fireRateTxt, _sellCoinTxt;

        [SerializeField] private Transform followTowerUI;

        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI upgradeGoldText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI attackRangeText;
        [SerializeField] private TextMeshProUGUI attackDelayText;
        [SerializeField] private TextMeshProUGUI sellGoldText;

        private void Awake()
        {
            _cam = Camera.main;

            _levelTxt = "Level : ";
            _damageTxt = "Damage : ";
            _rangeTxt = "Range : ";
            _fireRateTxt = "Fire Rate : ";
            _sellCoinTxt = "Sell";

            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            followTowerUI.position = _cam.WorldToScreenPoint(_followTowerPos);
        }

        public void SetFollowTarget(Vector3 towerPos)
        {
            _followTowerPos = towerPos;
        }

        public void SetTowerInfo(TowerInfo towerInfo)
        {
            towerNameText.text = towerInfo.towerName;
            levelText.text = _levelTxt + towerInfo.level;
            upgradeGoldText.text = towerInfo.upgradeGold + "g";
            damageText.text = _damageTxt + towerInfo.damage;
            attackRangeText.text = _rangeTxt + towerInfo.range;
            attackDelayText.text = _fireRateTxt + towerInfo.delay;
            sellGoldText.text = _sellCoinTxt + "(" + towerInfo.sellGold + "g)";
        }
    }
}