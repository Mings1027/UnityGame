using System;
using System.Threading;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        protected CancellationTokenSource Cts;
        protected bool attackable;

        protected virtual void OnEnable()
        {
            attackable = true;
            Cts?.Dispose();
            Cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            Cts.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}