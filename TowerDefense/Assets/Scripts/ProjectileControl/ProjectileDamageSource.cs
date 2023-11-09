using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectileControl
{
    public class ProjectileDamageSource : MonoBehaviour
    {
        private Projectile _projectile;
        private DecalProjector _shadowDecal;

        private void Awake()
        {
            _projectile = GetComponentInParent<Projectile>();
            _shadowDecal = _projectile.transform.GetChild(3).GetComponent<DecalProjector>();
        }

        private void OnEnable()
        {
            _shadowDecal.enabled = true;
        }

        private void OnDisable()
        {
            _shadowDecal.enabled = false;
            if (!_projectile.target) return;
            _projectile.Hit();
        }
    }
}