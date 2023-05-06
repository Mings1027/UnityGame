using BulletControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class WeaponController : MonoBehaviour
    {
        //현재 클래스의 자식이 Weapon이다 이 자식들마다 attackPoint를 가지고 있기 때문에 그 포인트를 가져온다.
        private Transform[] _attackPoint;
        private Weapon[] _weapons;

        private void Awake()
        {
            _attackPoint = new Transform[transform.childCount];
            for (int i = 0; i < _attackPoint.Length; i++)
            {
                _attackPoint[i] = transform.GetChild(i).GetChild(0);
            }

            _weapons = new Weapon[transform.childCount];
            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i] = transform.GetChild(i).GetComponent<Weapon>();
            }
        }

        public async UniTaskVoid Attack(int damage, Transform targetPos)
        {
            for (var i = 0; i < _attackPoint.Length; i++)
            {
                _weapons[i].Attack(damage, targetPos);
                await UniTask.Delay(100);
            }
        }
    }
}