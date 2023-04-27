using DG.Tweening;
using UnityEngine;

namespace TurretControl
{
    public class GunTurret : Turret
    {
        private Sequence weaponRecoilSequence;

        [SerializeField] private Transform[] cannon;

        [SerializeField] private int smoothTurnSpeed;
        [SerializeField] private float duration;
        [SerializeField] private int vibrato;
        [SerializeField] [Range(0, 1)] private float elasticity;

        protected override void Awake()
        {
            base.Awake();

            weaponRecoilSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            for (int i = 0; i < cannon.Length; i++)
            {
                weaponRecoilSequence.Append(cannon[i].transform
                        .DOPunchPosition(cannon[i].transform.forward * 0.1f, duration, vibrato, elasticity))
                    .SetSpeedBased();
            }
        }

        protected override void Targeting()
        {
            var dir = target.position - weapon.transform.position;
            var rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            weapon.transform.rotation =
                Quaternion.Slerp(weapon.transform.rotation, rot, smoothTurnSpeed * Time.deltaTime);
            var fireRot = Quaternion.Euler(0, rot.eulerAngles.y, 0);
            if (Quaternion.Angle(weapon.transform.rotation, fireRot) < 5)
            {
                if (attackAble)
                {
                    weaponRecoilSequence.Restart();
                    Attack();
                    StartCoolDown();
                }
            }
        }
    }
}