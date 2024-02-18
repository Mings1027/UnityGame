using DataControl.TowerDataControl;
using TMPro;
using UnityEngine;

namespace LobbyUIControl
{
    public class TowerUpgradeButton : MonoBehaviour
    {
        private TMP_Text _xpCostText;
        private TMP_Text _upgradeCountText;

        [field: SerializeField] public AttackTowerData attackTowerData { get; private set; }
    }
}