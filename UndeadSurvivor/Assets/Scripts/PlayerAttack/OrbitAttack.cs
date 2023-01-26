using GameControl;
using UnityEngine;

namespace PlayerAttack
{
    public sealed class OrbitAttack : MonoBehaviour
    {
        [SerializeField] private float degree;
        [SerializeField] private int rotateSpeed;
        [SerializeField] private float radius;
        [SerializeField] private Transform[] meleeBullets;
        [SerializeField] private int count;
        private bool _startOrbit;
        private Vector3 _curPos;

        private void OnEnable()
        {
            count = 0;
            meleeBullets = new Transform[10];
        }

        public void AddBullet()
        {
            if (count >= meleeBullets.Length) return;
            meleeBullets[count] = StackObjectPool.Get("OrbitBullet", transform.position).transform;
            count++;
        }

        private void Update()
        {
            Orbit();
        }

        private void Orbit()
        {
            // if (!_startOrbit) return;
            degree += Time.deltaTime * rotateSpeed;
            if (degree < 360)
            {
                for (var i = 0; i < count; i++)
                {
                    var bulletDis = degree + i * (360 / count);
                    var rad = Mathf.Deg2Rad * bulletDis;
                    var x = radius * Mathf.Sin(rad);
                    var y = radius * Mathf.Cos(rad);
                    _curPos = new Vector3(x, y, 0);
                    meleeBullets[i].SetPositionAndRotation(transform.position + _curPos,
                        Quaternion.Euler(0, 0, bulletDis * -1));
                }
            }
            else
            {
                degree = 0;
            }
        }
    }
}