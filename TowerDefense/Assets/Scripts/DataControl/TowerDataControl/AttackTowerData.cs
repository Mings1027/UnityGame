using CustomEnumControl;
using UnityEngine;
using Utilities;

namespace DataControl.TowerDataControl
{
    public abstract class AttackTowerData : TowerData
    {
        public MeshFilter[] TowerMeshes => towerMeshes;
        public ushort BaseDamage { get; private set; }
        public byte AttackRange { get; protected set; }
        public ushort AttackRpm { get; private set; }

        protected byte InitRange => initRange;

        [SerializeField] private MeshFilter[] towerMeshes;
        [SerializeField] private ushort initDamage;
        [SerializeField] private byte initRange;
        [SerializeField] private ushort initRpm;

        public override void InitState()
        {
            var damage = PlayerPrefs.GetInt(StringManager.DamageDic[TowerType]);
            if (damage <= 0)
            {
                PlayerPrefs.SetInt(StringManager.DamageDic[TowerType], initDamage);
                BaseDamage = initDamage;
            }
            else
            {
                BaseDamage = (ushort)damage;
            }

            AttackRpm = initRpm;
        }

        public virtual void UpgradeData(TowerType type)
        {
            var baseDamage = PlayerPrefs.GetInt(StringManager.DamageDic[type]);
            BaseDamage = (ushort)(baseDamage + 5);
            PlayerPrefs.SetInt(StringManager.DamageDic[type], BaseDamage);

            var attackRange = PlayerPrefs.GetInt(StringManager.RangeDic[type]);
            AttackRange = (byte)(attackRange + 1);
            PlayerPrefs.SetInt(StringManager.RangeDic[type], AttackRange);
        }
    }
}