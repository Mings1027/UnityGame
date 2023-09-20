using System;
using PoolObjectControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl
{
    [CreateAssetMenu]
    public class WaveData : ScriptableObject
    {
        [FormerlySerializedAs("enemyWaves")] public EnemyInfo[] enemyInfos;
        [Serializable]
        public struct EnemyInfo
        {
            public int startSpawnWave;
            public PoolObjectKey enemyName;
            // public string enemyName;
            public int enemyCoin;
            public float atkDelay;
            public int damage;
            public float health;
        }
    }
    
}
