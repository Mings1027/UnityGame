using System;
using UnityEngine;

namespace GameControl
{
    public class SpriteBillboard : MonoBehaviour
    {
        private Camera _cam;
        private SpriteRenderer sprite;
        // private Transform player;
        // [Range(0, 50)] [SerializeField] private float distance;

        [SerializeField] private bool freezeXZ;

        private void Awake()
        {
            _cam = Camera.main;
            sprite = GetComponent<SpriteRenderer>();
            // player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void LateUpdate()
        {
            // if (Vector3.Distance(transform.position, player.position) > distance) return;
            transform.rotation = freezeXZ
                ? Quaternion.Euler(0, _cam.transform.eulerAngles.y, 0)
                : _cam.transform.rotation;

            sprite.flipX = transform.eulerAngles.y is > 0 and < 180;
        }

        // private void OnDrawGizmos()
        // {
        //     Gizmos.DrawWireSphere(transform.position, distance);
        // }
    }
}