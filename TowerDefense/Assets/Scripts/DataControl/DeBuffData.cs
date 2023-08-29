using System;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class DeBuffData : ScriptableObject
    {
        [Serializable]
        public struct SpeedDeBuffData
        {
            public float deBuffTime;
            public float decreaseSpeed;
        }

        public SpeedDeBuffData[] speedDeBuffData;
    }
}