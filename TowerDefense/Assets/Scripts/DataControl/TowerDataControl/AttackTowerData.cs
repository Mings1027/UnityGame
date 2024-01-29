using BackendControl;
using CustomEnumControl;
using UnityEngine;

namespace DataControl.TowerDataControl
{
    public abstract class AttackTowerData : TowerData
    {
        public ushort curDamage { get; private set; }
        public byte curRange { get; private set; }
        public ushort curRpm { get; private set; }
        [field: SerializeField] public MeshFilter[] towerMeshes { get; private set; }
        [SerializeField] private ushort initDamage;
        [SerializeField] private byte initRange;
        [SerializeField] private ushort initRpm;

        public override void InitState()
        {
            var towerLevel = BackendGameData.userData.towerLevelTable[towerType.ToString()] + 1;
            curDamage = (ushort)(towerLevel * initDamage);
            curRange = (byte)(towerLevel * initRange);
            curRpm = (ushort)(towerLevel * initRpm);
        }

        public virtual void UpgradeData(TowerType type)
        {
            var towerLevel = BackendGameData.userData.towerLevelTable[type.ToString()] += 1;
            towerLevel += 1;
            curDamage = (ushort)(towerLevel * initDamage);
            curRange = (byte)(towerLevel * initRange);
            curRpm = (ushort)(towerLevel * initRpm);
        }
    }
}