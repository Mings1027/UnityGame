using System;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class WaveData : ScriptableObject
    {
        public EnemyWave[] enemyWaves;
        [Serializable]
        public struct EnemyWave
        {
            public int startSpawnWave;
            public string enemyName;
            public int enemyCoin;
            public int atkRange;
            public float atkDelay;
            public int damage;
            public float health;
        }
    }
    
}
