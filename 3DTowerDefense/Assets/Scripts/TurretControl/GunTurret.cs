using DG.Tweening;
using UnityEngine;

namespace TurretControl
{
    public class GunTurret : Turret
    {
        private Sequence _recoilSequence;
        private Sequence _recoilTween;
        private Transform[] _weapon;


        [SerializeField] [Range(0, 1)] private float offsetDistance;
        [SerializeField] private int smoothTurnSpeed;
        [SerializeField] private float minZ, maxZ;
        [SerializeField] private float duration;
        [SerializeField] private Ease firstEase;
        [SerializeField] private Ease secondEase;

        [Space(10)] [SerializeField] private float punchDuration;
        [SerializeField] private int vibrato;
        [SerializeField] private float elasticity;

        protected override void Awake()
        {
            base.Awake();

            _weapon = new Transform[WeaponController.transform.childCount];
            for (var i = 0; i < _weapon.Length; i++)
            {
                _weapon[i] = WeaponController.transform.GetChild(i);
            }

            //주석부분이 원래 쓰던 로직임
            //DoPunch왜 하나는 되는데 여러개는 안됨 

            _recoilSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            foreach (var c in _weapon)
            {
                _recoilSequence
                    .Append(c.transform.DOLocalMoveZ(-minZ, duration).SetEase(firstEase))
                    .SetSpeedBased()
                    .Append(c.transform.DOLocalMoveZ(maxZ, duration).SetEase(secondEase))
                    .SetSpeedBased();
            }

            // _recoilTween = DOTween.Sequence().SetAutoKill(false).Pause();
            // foreach (var w in _weapon)
            // {
            //     _recoilTween.Append(w.DOPunchPosition(new Vector3(0, 0, w.position.z), punchDuration, vibrato,
            //         elasticity));
            // }
        }

        protected override void Targeting()
        {
            ShootDir = (Target.position - WeaponController.transform.position).normalized +
                       Target.forward * offsetDistance;
            var rot = Quaternion.LookRotation(new Vector3(ShootDir.x, 0, ShootDir.z));
            WeaponController.transform.rotation =
                Quaternion.Slerp(WeaponController.transform.rotation, rot, smoothTurnSpeed * Time.deltaTime);
            var fireRot = Quaternion.Euler(0, rot.eulerAngles.y, 0);

            if (Quaternion.Angle(WeaponController.transform.rotation, fireRot) >= 5) return;
            if (AttackAble)
            {
                _recoilSequence.Restart();
                // _recoilTween.Restart();
                Attack();
                StartCoolDown();
            }

            Debug.DrawRay(WeaponController.transform.position, ShootDir * 5, Color.green);
        }
    }
}