using UnityEngine;

namespace WeaponControl
{
    public class PenetrationMageBullet : MageBullet
    {
        private int _pCount;
        private int _targetIndex;
        private int _targetingCount;

        public Transform[] targetTransform;

        protected override void Awake()
        {
            base.Awake();
            targetTransform = new Transform[3];
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            _pCount++;
            _targetIndex++;
            BulletHit(other);

            if (_targetingCount > _pCount) return;
            _pCount = 0;
            _targetIndex = 0;
            gameObject.SetActive(false);
        }

        public void UpdateTarget(int size, Collider[] targets)
        {
            _targetingCount = size;
            for (var i = 0; i < size; i++)
            {
                targetTransform[i] = targets[i].transform;
            }
        }

        protected override void AttackPath()
        {
            if (_targetingCount <= 0) return;
            var dir = (targetTransform[_targetIndex].position + targetTransform[_targetIndex].up - transform.position)
                .normalized;
            rigid.velocity = dir * (BulletSpeed * Time.fixedDeltaTime);
        }
    }
}