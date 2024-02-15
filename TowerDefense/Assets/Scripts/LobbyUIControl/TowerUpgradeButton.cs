using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class TowerUpgradeButton : MonoBehaviour
    {
        private TMP_Text _xpCostText;
        private TMP_Text _upgradeCountText;

        [field: SerializeField] public AttackTowerData attackTowerData { get; private set; }
    }
}