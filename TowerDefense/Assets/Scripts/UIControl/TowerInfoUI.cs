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
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI attackRangeText;
        [SerializeField] private TextMeshProUGUI attackDelayText;
        [SerializeField] private TextMeshProUGUI sellCoinText;

        private void Awake()
        {
            _cam = Camera.main;

            _levelTxt = "Level :";
            _damageTxt = "Damage : ";
            _rangeTxt = "Range : ";
            _fireRateTxt = "Fire Rate : ";
            _sellCoinTxt = "Sell : ";

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

        public void SetText(string level, string damage, string range, string delay, string sellCoin,
            string towerName = "")
        {
            if (string.IsNullOrEmpty(towerName))
            {
                towerNameText.gameObject.SetActive(false);
            }
            else
            {
                towerNameText.gameObject.SetActive(true);
                towerNameText.text = towerName;
            }

            levelText.text = _levelTxt + level;
            damageText.text = _damageTxt + damage;
            attackRangeText.text = _rangeTxt + range;
            attackDelayText.text = _fireRateTxt + delay;
            sellCoinText.text = _sellCoinTxt + sellCoin;
        }
    }
}