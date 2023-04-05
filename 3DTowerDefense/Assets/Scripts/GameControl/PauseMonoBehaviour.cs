using ManagerControl;
using UnityEngine;

namespace GameControl
{
    public class PauseMonoBehaviour : MonoBehaviour
    {
        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GameManager.Instance;
        }

        private void Update()
        {
            if (_gameManager.IsPause) return;
            UnityUpdate();
        }

        private void FixedUpdate()
        {
            if (_gameManager.IsPause) return;
            UnityFixedUpdate();
        }

        private void LateUpdate()
        {
            if (_gameManager.IsPause) return;
            UnityLateUpdate();
        }

        protected virtual void UnityUpdate()
        {
        }

        protected virtual void UnityFixedUpdate()
        {
        }

        protected virtual void UnityLateUpdate()
        {
        }
    }
}