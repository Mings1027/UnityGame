using System;
using DataControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour
    {
        private Camera _cam;
        public Vector3 targetPos { get; set; }

        private void Awake()
        {
            _cam = Camera.main;
        }
        
        private void LateUpdate()
        {
            transform.position = _cam.WorldToScreenPoint(targetPos);
        }

        public void ExpandMap()
        {
            var position = transform.position;
            MapController.Instance.ExpandMap(targetPos);
            ObjectPoolManager.Get(PoolObjectName.ExpandMapSmoke, position);

            gameObject.SetActive(false);
        }
    }
}