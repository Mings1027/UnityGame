using DataControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour
    {
        private Camera _cam;
        private RectTransform _buttonRectTransform;

        private void Awake()
        {
            _cam = Camera.main;
            _buttonRectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            var viewPos = _cam.WorldToViewportPoint(_buttonRectTransform.position);
            var isVisible = viewPos.x is >= 0 and <= 1 && viewPos.y is >= 0 and <= 1;
            if (!isVisible) return;
            if (_cam.transform.rotation == transform.rotation) return;
            transform.rotation = _cam.transform.rotation;
        }

        public void ExpandMap()
        {
            var position = transform.position;
            MapController.Instance.ExpandMap(position);
            ObjectPoolManager.Get(PoolObjectName.ExpandMapSmoke, position);

            gameObject.SetActive(false);
        }
    }
}