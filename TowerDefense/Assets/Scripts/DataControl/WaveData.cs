using System;
using PoolObjectControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class WaveData : ScriptableObject
    {
        public EnemyInfo[] enemyInfos;
        [Serializable]
        public struct EnemyInfo
        {
            public int startSpawnWave;
            public PoolObjectKey enemyName;
            public int enemyCoin;
            public float atkDelay;
            public int damage;
            public float health;
        }
    }
    
}
