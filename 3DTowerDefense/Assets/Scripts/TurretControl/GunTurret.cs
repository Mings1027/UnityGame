using UnityEngine;

namespace TurretControl
{
    public class GunTurret : Turret
    {
        [SerializeField] [Range(0, 1)] private float offsetDistance;
        [SerializeField] private int smoothTurnSpeed;

        [Header("Weapon Recoil")] [Space(10)] [SerializeField] [Range(0, 2)]
        private float punchDuration;

        [SerializeField] [Range(0, 3)] private int vibrato;
        [SerializeField] [Range(0, 1)] private float elasticity;

        private void Start()
        {
            WeaponController.WeaponInit(punchDuration, vibrato, elasticity);
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
                Attack();
                StartCoolDown();
            }

            Debug.DrawRay(WeaponController.transform.position, ShootDir * 5, Color.green);
        }

        protected override void Attack()
        {
            WeaponController.Attack(Damage, ShootDir).Forget();
        }
    }
}