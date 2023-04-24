using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TurretData : ScriptableObject
{
    public TurretInfo[] turretInfos = new TurretInfo[2];
    [Serializable]
    public class TurretInfo
    {
        public string turretName;
        public string turretDesc;
        public int minDamage, maxDamage;
        public float attackRange;
        public float attackDelay;
    }
}
