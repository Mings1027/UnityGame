using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerEditButtonController : MonoBehaviour
    {
        private string _lastSelectedName;
        private Dictionary<string, Button> _towerEditButtonsDic;

        [SerializeField] private Sprite okSprite;
        [SerializeField] private Sprite[] _defaultSprites;
        [SerializeField] private Button[] editButtons;

        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button aUpgradeButton;
        [SerializeField] private Button bUpgradeButton;
        [SerializeField] private Button moveUnitButton;
        [SerializeField] private Button sellButton;

        private void Awake()
        {
            _towerEditButtonsDic = new Dictionary<string, Button>
            {
                { "UpgradeButton", upgradeButton },
                { "AUpgradeButton", aUpgradeButton },
                { "BUpgradeButton", bUpgradeButton },
                { "MoveUnitButton", moveUnitButton },
                { "SellButton", sellButton }
            };

            for (var i = 0; i < editButtons.Length; i++)
            {
                editButtons[i] = transform.GetChild(i).GetComponent<Button>();
                var index = i;
                editButtons[i].onClick.AddListener(() => TowerEditButton(transform.GetChild(index).name));
            }

            for (var i = 0; i < _defaultSprites.Length; i++)
            {
                _defaultSprites[i] = editButtons[i].image.sprite;
            }
        }

        private void BtnInit()
        {
            
        }

        private void TowerEditButton(string editBtnName)
        {
            if (editBtnName == _lastSelectedName)
            {
                //okButton
                return;
            }

            _lastSelectedName = editBtnName;
            
        }
    }
}