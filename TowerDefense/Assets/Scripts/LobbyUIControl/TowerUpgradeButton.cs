using System;
using DataControl.TowerDataControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class TowerUpgradeButton : MonoBehaviour
    {
        [field: SerializeField] public AttackTowerData attackTowerData { get; private set; }
        [field: SerializeField] public Button upgradeButton { get; private set; }
        [field: SerializeField] public Image towerImage { get; private set; }
        [field: SerializeField] public TMP_Text towerLevelText { get; private set; }
    }
}