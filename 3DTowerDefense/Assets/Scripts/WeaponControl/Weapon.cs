using BulletControl;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Weapon : MonoBehaviour
    {
        private Sequence _recoilSequence;
        private Tween _recoilTween;
        private Transform atkPos;

        [SerializeField] protected string bulletType;

        protected virtual void Awake()
        {
            atkPos = transform.GetChild(0);
        }

        public void RecoilInit(float punchDuration, int vibrato, float elasticity)
        {
            _recoilSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            _recoilSequence
                .Append(transform.DOPunchPosition(new Vector3(0, 0, -transform.position.z * 0.1f),
                    punchDuration, vibrato, elasticity))
                .SetEase(Ease.OutExpo);

            _recoilTween = transform.DOPunchPosition(new Vector3(0, 0, -transform.position.z * 0.1f),
                punchDuration, vibrato, elasticity).SetEase(Ease.OutExpo);
        }

        public void Attack(int damage, Vector3 dir)
        {
            _recoilSequence.Restart();
            var pos = atkPos.position;
            var rot = atkPos.rotation;
            StackObjectPool.Get("BulletExplosionVFX", pos);
            StackObjectPool.Get("BulletShootVFX", pos, rot);
            StackObjectPool.Get<Bullet>(bulletType, pos, rot).Init(damage, dir);
        }
    }
}