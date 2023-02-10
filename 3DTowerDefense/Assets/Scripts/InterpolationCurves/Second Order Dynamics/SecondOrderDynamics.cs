#region Information

/*****
* 
* 26.09.2022
* 
* SecondOrderDynamics.cs implementation based on the video source provided by the author - t3ssel8r
* Link to the original YouTube video: https://www.youtube.com/watch?v=KPoeNZZ6H4s&t=554s&ab_channel=t3ssel8r
* 
*****/

#endregion Information

using Unity.Mathematics;
using UnityEngine;

namespace InterpolationCurves.Second_Order_Dynamics
{
    public class SecondOrderDynamics
    {
        private Vector3? _xp;
        private Vector3? _y, _yd;
        private readonly float _w, _z, _d, _k1, _k2, _k3;

        public SecondOrderDynamics(float f, float z, float r, Vector3 x0)
        {
            _w = 2 * math.PI * f;
            _z = z;
            _d = _w * math.sqrt(math.abs(z * z - 1));
            _k1 = z / (math.PI * f);
            _k2 = 1 / (_w * _w);
            _k3 = r * z / _w;

            _xp = x0;
            _y = x0;
            _yd = Vector3.zero;
        }

        public Vector3? Update(float T, Vector3 x, Vector3? xd = null)
        {
            if (xd == null)
            {
                xd = (x - _xp) / T;
                _xp = x;
            }

            float k1Stable, k2Stable;
            if (_w * T < _z)
            {
                k1Stable = _k1;
                k2Stable = Mathf.Max(_k2, T * T / 2 + T * _k1 / 2, T * _k1);
            }
            else
            {
                var t1 = math.exp(-_z * _w * T);
                var alpha = 2 * t1 * (_z <= 1 ? math.cos(T * _d) : math.cosh(T * _d));
                var beta = t1 * t1;
                var t2 = T / (1 + beta - alpha);
                k1Stable = (1 - beta) * t2;
                k2Stable = T * t2;
            }

            _y += T * _yd;
            _yd += T * (x + _k3 * xd - _y - k1Stable * _yd) / k2Stable;
            return _y;
        }
    }
}