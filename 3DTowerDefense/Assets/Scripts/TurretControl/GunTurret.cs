using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace TurretControl
{
    public class GunTurret : Turret
    {
        private Sequence _recoilSequence;

        [SerializeField] private Transform[] cannon;

        [SerializeField] private int smoothTurnSpeed;
        [SerializeField] private float minZ, maxZ;
        [SerializeField] private float duration;
        [SerializeField] private Ease firstEase;
        [SerializeField] private Ease secondEase;

        protected override void Awake()
        {
            base.Awake();

            _recoilSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            foreach (var c in cannon)
            {
                _recoilSequence
                    .Append(c.transform.DOLocalMoveZ(-minZ, duration).SetEase(firstEase))
                    .SetSpeedBased()
                    .Append(c.transform.DOLocalMoveZ(maxZ, duration).SetEase(secondEase))
                    .SetSpeedBased();
            }
        }

        protected override void Targeting()
        {
            var dir = Target.position - Weapon.transform.position;
            var rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            Weapon.transform.rotation =
                Quaternion.Slerp(Weapon.transform.rotation, rot, smoothTurnSpeed * Time.deltaTime);
            var fireRot = Quaternion.Euler(0, rot.eulerAngles.y, 0);
            if (Quaternion.Angle(Weapon.transform.rotation, fireRot) < 5)
            {
                if (AttackAble)
                {
                    _recoilSequence.Restart();
                    Attack();
                    StartCoolDown();
                }
            }
        }
    }
}