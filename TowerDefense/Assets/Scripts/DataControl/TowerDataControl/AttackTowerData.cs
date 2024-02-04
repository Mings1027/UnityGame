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
            var towerLevel = BackendGameData.userData.towerLevelTable[towerType.ToString()];
            curDamage = (ushort)(towerLevel * 5 + initDamage);
            curRange = (byte)(towerLevel + initRange);
            curRpm = initRpm;
        }

        public virtual void UpgradeData(int towerLv)
        {
            curDamage = (ushort)(towerLv * 5 + initDamage);
            curRange = (byte)(towerLv + initRange);
        }
    }
}