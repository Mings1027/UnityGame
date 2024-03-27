using BackendControl;
using CustomEnumControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl.TowerDataControl
{
    public abstract class AttackTowerData : TowerData
    {
        public ushort curDamage { get; private set; }

        [field: SerializeField] public MeshFilter[] towerMeshes { get; private set; }
        [field: SerializeField] public float attackCooldown { get; private set; }

        [SerializeField] private ushort initDamage;

        public override void InitState()
        {
            base.InitState();
            var towerLevel = BackendGameData.userData.towerLevelTable[towerType.ToString()];
            curDamage = (ushort)(towerLevel * 5 + initDamage);
        }

        public override void UpgradeData(int towerLv)
        {
            base.UpgradeData(towerLv);
            curDamage = (ushort)(towerLv * 5 + initDamage);
        }
    }
}