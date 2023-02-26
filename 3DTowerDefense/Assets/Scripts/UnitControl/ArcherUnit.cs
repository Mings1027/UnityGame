using UnityEngine;

namespace UnitControl
{
    public class ArcherUnit : Unit
    {
        private float _atkDelay;

        public void Setting(float atkDelay)
        {
            _atkDelay = atkDelay;
        }
    }
}
