using AttackControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        protected TargetFinder targetFinder;

        protected abstract void CheckState();
        protected abstract void Attack();

        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
        }
        
        protected virtual void Update()
        {
            CheckState();
        }
    }
}