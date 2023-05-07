using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WeaponControl
{
    public class WeaponController : MonoBehaviour
    {
        //현재 클래스의 자식이 Weapon이다 이 자식들마다 attackPoint를 가지고 있기 때문에 그 포인트를 가져온다.
        private Weapon[] _weapons;
        private Transform[] _attackPoint;

        private void Awake()
        {
            _weapons = new Weapon[transform.childCount];
            for (var i = 0; i < _weapons.Length; i++)
            {
                _weapons[i] = transform.GetChild(i).GetComponent<Weapon>();
            }

            _attackPoint = new Transform[transform.childCount];
            for (var i = 0; i < _attackPoint.Length; i++)
            {
                _attackPoint[i] = transform.GetChild(i).GetChild(0);
            }
        }

        public async UniTaskVoid Attack(int damage, Vector3 dir)
        {
            for (var i = 0; i < _attackPoint.Length; i++)
            {
                _weapons[i].Attack(damage, dir);
                await UniTask.Delay(100);
            }
        }
    }
}