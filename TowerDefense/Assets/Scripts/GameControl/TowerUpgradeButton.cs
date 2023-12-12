using System;
using CustomEnumControl;
using DataControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameControl
{
    public class TowerUpgradeButton : MonoBehaviour
    {
        private TMP_Text _xpCostText;
        private TMP_Text _upgradeCountText;
        private byte _upgradeCount;
        public event Action OnSetXpTextEvent;

        [SerializeField] private BattleTowerData towerData;

        private void Awake()
        {
            var thisButton = GetComponent<Button>();
            _upgradeCount = (byte)PlayerPrefs.GetInt(towerData.TowerType + StringManager.UpgradeCount);
            _xpCostText = transform.GetChild(1).GetComponent<TMP_Text>();
            _upgradeCountText = transform.GetChild(2).GetComponent<TMP_Text>();

            if (_upgradeCount < 5)
            {
                _xpCostText.text = "XP : " + (_upgradeCount + 1) * 25;
                _upgradeCountText.text = "Lv " + _upgradeCount;
            }
            else
            {
                _xpCostText.text = "";
                _upgradeCountText.text = "Lv MAX";
                thisButton.interactable = false;
            }

            thisButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                if (_upgradeCount >= 5 || DataManager.Xp < (_upgradeCount + 1) * 25) return;
                DataManager.Xp -= (_upgradeCount + 1) * 25;
                OnSetXpTextEvent?.Invoke();
                _upgradeCount++;
                _xpCostText.text = "XP : " + (_upgradeCount + 1) * 25;
                _upgradeCountText.text = "Lv " + _upgradeCount;
                PlayerPrefs.SetInt(StringManager.Xp, DataManager.Xp);
                PlayerPrefs.SetInt(towerData.TowerType + StringManager.UpgradeCount, _upgradeCount);
                towerData.UpgradeData(towerData.TowerType);

                if (_upgradeCount >= 5)
                {
                    _upgradeCountText.text = "Lv MAX";
                    _xpCostText.text = "";
                    thisButton.interactable = false;
                }
            });
        }
    }
}