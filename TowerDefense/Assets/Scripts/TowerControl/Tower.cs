using CustomEnumControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    [DisallowMultipleComponent]
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public TowerType TowerType => towerType;
        
        [SerializeField] private TowerType towerType;

        #region Unity Event

        protected virtual void OnEnable()
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
        }

        #endregion

        #region Public Function

        public virtual void DisableObject()
        {
            var g = gameObject;
            g.SetActive(false);
            Destroy(g);
        }

        #endregion
    }
}