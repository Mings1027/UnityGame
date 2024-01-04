using System;
using System.Collections.Generic;
using EPOOutline;
using UnityEngine;

namespace Plugins.Easy_performant_outline.Scripts
{
    [ExecuteAlways]
    public class TargetStateListener : MonoBehaviour
    {
        private struct Callback
        {
            public readonly Outlinable target;
            public readonly Action action;

            public Callback(Outlinable target, Action action)
            {
                this.target = target;
                this.action = action;
            }
        }

        private readonly List<Callback> _callbacks = new List<Callback>();

        public void AddCallback(Outlinable outlinable, Action action)
        {
            _callbacks.Add(new Callback(outlinable, action));
        }

        public void RemoveCallback(Outlinable outlinable, Action callback)
        {
            var found = _callbacks.FindIndex(x => x.target == outlinable && x.action == callback);
            if (found == -1)
                return;
            
            _callbacks.RemoveAt(found);
        }

        private void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
        }

        public void ForceUpdate()
        {
            _callbacks.RemoveAll(x => x.target == null);
            foreach (var callback in _callbacks)
                callback.action();
        }

        private void OnBecameVisible()
        {
            ForceUpdate();
        }

        private void OnBecameInvisible()
        {
            ForceUpdate();
        }
    }
}