using UnityEngine;

namespace PlayerControl
{
    public class PlayerOrbitAttack : MonoBehaviour
    {
        [SerializeField] private float degree;
        [SerializeField] private int rotateSpeed;
        [SerializeField] private float radius;
        [SerializeField] private Transform[] meleeBullets;

        private void FixedUpdate()
        {
            RotateObj();
        }

        private void RotateObj()
        {
            degree += Time.fixedDeltaTime * rotateSpeed;
            if (degree < 360)
            {
                for (var i = 0; i < meleeBullets.Length; i++)
                {
                    var bulletDis = degree + i * (360 / meleeBullets.Length);
                    var rad = Mathf.Deg2Rad * bulletDis;
                    var x = radius * Mathf.Sin(rad);
                    var y = radius * Mathf.Cos(rad);
                    meleeBullets[i].position = transform.position + new Vector3(x, y, 0);
                    meleeBullets[i].rotation = Quaternion.Euler(0, 0, bulletDis * -1);
                }
            }
            else
            {
                degree = 0;
            }
        }

        private void AddBullet()
        {
            
        }
        
    }
}